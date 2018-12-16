using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityAssert = UnityEngine.Assertions.Assert;

namespace Maskox.Tests.Editor
{
    public class LinearWipeShaderTest
    {
        [UnityTest]
        public IEnumerator Incremental(
            [Values(0.0f, 90.0f, 180.0f, 270.0f)]
            [Random(-720.0f, 720.0f, 10)]
            float angle)
        {
            using (var map = new MonochromeTexture("Maskox/Linear"))
            {
                map.Material.SetFloat("_Maskox_Angle", angle);

                yield return map.Initialize();
                yield return map.Update();

                var radian = angle * Mathf.Deg2Rad;

                var direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                UnityAssert.AreEqual(0.0f, direction.z);
                UnityAssert.AreApproximatelyEqual(1.0f, direction.magnitude);

                // https://drafts.csswg.org/css-images-3/#funcdef-linear-gradient
                var halfLength = (Mathf.Abs(1.0f * Mathf.Cos(radian)) + Mathf.Abs(1.0f * Mathf.Sin(radian))) / 2.0f;
                var center = new Vector2(0.5f, 0.5f);
                var extents = new Vector2(direction.x * halfLength, direction.y * halfLength);
                var start = center - extents;
                var end = center + extents;

                UnityAssert.AreApproximatelyEqual(0.0f, map[start], 1.0f / 256.0f);
                UnityAssert.AreApproximatelyEqual(0.5f, map[0.5f, 0.5f], 1.0f / 256.0f);
                UnityAssert.AreApproximatelyEqual(1.0f, map[end], 1.0f / 256.0f);

                map.AssertIncremental(start, end);
            }
        }
    }
}
