using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using RangeAttribute = NUnit.Framework.RangeAttribute;

namespace Maskox.Tests.Editor
{
    public class RadialShaderTest
    {
        [UnityTest]
        public IEnumerator Incremental(
            [Range(-1.0f, +1.5f, 4)] float dx,
            [Range(-2.0f, +1.2f, 3)] float dy)
        {
            using (var map = new MonochromeTexture("Maskox/Radial"))
            {
                var center = new Vector2(0.5f, 0.5f);

                map.Material.SetVector("_Maskox_Center", center);

                yield return map.Initialize();
                yield return map.Update();

                var direction = dx != 0.0f || dy != 0.0f ? new Vector2(dx, dy).normalized : Vector2.right;
                map.AssertIncremental(center, center + (direction * 0.5f));
            }
        }
    }
}
