using System;
using System.Collections;
using UnityEngine;

namespace Maskox.Tests.Editor
{
    public sealed class MonochromeTexture : IDisposable
    {
        private Material material;
        private CustomRenderTexture writableTexture;
        private Texture2D readableTexture;

        public Material Material => material;
        public float this[Vector2 uv] => this[uv.x, uv.y];
        public float this[float u, float v]
        {
            get
            {
                var texture = readableTexture;
                if (texture != null)
                    return texture.GetPixelBilinear(u, v).r;
                else
                    return 0.0f;
            }
        }

        public MonochromeTexture(string shaderName) : this(Shader.Find(shaderName)) { }
        public MonochromeTexture(Shader shader) : this(shader, 256, 256, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { }
        public MonochromeTexture(Shader shader, int width, int height, RenderTextureFormat format, RenderTextureReadWrite readWrite)
        {
            this.material = new Material(shader);
            this.writableTexture = new CustomRenderTexture(width, height, format, readWrite)
            {
                initializationMode = CustomRenderTextureUpdateMode.OnDemand,
                updateMode = CustomRenderTextureUpdateMode.OnDemand,
                material = this.material,
            };
            this.readableTexture = new Texture2D(writableTexture.width, writableTexture.height, TextureFormat.ARGB32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        public void Dispose()
        {
            if (writableTexture != null)
            {
                writableTexture.DiscardContents();
                writableTexture.Release();
            }

            Destroy(ref material);
            Destroy(ref writableTexture);
            Destroy(ref readableTexture);
        }

        private void Destroy<T>(ref T objectToDestroy) where T : UnityEngine.Object
        {
            var obj = objectToDestroy;
            objectToDestroy = null;

            if (obj != null)
                UnityEngine.Object.DestroyImmediate(obj);
        }

        public IEnumerator Initialize()
        {
            if (writableTexture.IsCreated() == false)
                writableTexture.Create();

            writableTexture.Initialize();

            yield return null;
        }

        public IEnumerator Update()
        {
            var previousCount = writableTexture.updateCount;
            writableTexture.Update();
            while (previousCount == writableTexture.updateCount)
                yield return null;

            UpdateReadableTexture();
        }

        public void AssertIncremental(Vector2 start, Vector2 end, float step = 0.01f)
        {
            for (var t = step; t < 1.0f; t += step)
                NUnit.Framework.Assert.LessOrEqual(this[Vector2.Lerp(start, end, t - step)], this[Vector2.Lerp(start, end, t)]);
        }

        private void UpdateReadableTexture()
        {
            var originalActiveRenderTexture = RenderTexture.active;
            try
            {
                RenderTexture.active = writableTexture;
                Graphics.SetRenderTarget(writableTexture);
                readableTexture.ReadPixels(new Rect(0, 0, readableTexture.width, readableTexture.height), 0, 0);
                readableTexture.Apply(false, false);
            }
            finally
            {
                RenderTexture.active = originalActiveRenderTexture;
            }
        }
    }
}
