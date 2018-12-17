using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Maskox.Tests.Editor
{
    public class StepShaderTest
    {
        [UnityTest]
        public IEnumerator Zero_Or_One([Range(0.0f, 360.0f, 15.0f)] float angle)
        {
            using (var texture = new MonochromeTexture("Maskox/Step", 64, 64))
            {
                texture.Material.SetFloat("_Maskox_Angle", angle);

                yield return texture.Initialize();
                yield return texture.Update();

                Assert.IsTrue(texture.All(it => it.Alpha == 0.0f || it.Alpha == 1.0f));
            }
        }
    }
}
