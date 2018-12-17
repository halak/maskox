using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityAssert = UnityEngine.Assertions.Assert;
using RangeAttribute = NUnit.Framework.RangeAttribute;

namespace Maskox.Tests.Editor
{
    public class LinearShaderTest
    {
        [UnityTest]
        public IEnumerator Incremental([Range(0.0f, 360.0f, 15.0f)] float angle)
        {
            using (var texture = new MonochromeTexture("Maskox/Linear"))
            {
                texture.Material.SetFloat("_Maskox_Angle", angle);

                yield return texture.Initialize();
                yield return texture.Update();

                var radian = angle * Mathf.Deg2Rad;

                var direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                UnityAssert.AreEqual(0.0f, direction.z);
                UnityAssert.AreApproximatelyEqual(1.0f, direction.magnitude);

                var center = new Vector2(0.5f, 0.5f);
                var extents = new Vector2(direction.x * 0.5f, direction.y * 0.5f);
                var start = center - extents;
                var end = center + extents;

                UnityAssert.AreApproximatelyEqual(0.5f, texture[0.5f, 0.5f], 1.0f / 256.0f);

                texture.AssertIncremental(start, end);
            }
        }
    }
}
