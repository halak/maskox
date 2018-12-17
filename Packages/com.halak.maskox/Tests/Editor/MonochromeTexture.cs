using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maskox.Tests.Editor
{
    public struct MonochromeTexel
    {
        public readonly int X;
        public readonly int Y;
        public readonly float Alpha;

        public MonochromeTexel(int x, int y, float alpha)
        {
            this.X = x;
            this.Y = y;
            this.Alpha = alpha;
        }

        public override string ToString() => FormattableString.Invariant($"{Alpha:0.###} ({X}, {Y})");
    }

    public sealed class MonochromeTexture : IDisposable, IEnumerable<MonochromeTexel>
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
        public MonochromeTexture(string shaderName, int width, int height) : this(Shader.Find(shaderName), width, height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { }
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

        public void Export(string path)
        {
            var bytes = readableTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
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

        public IEnumerator<MonochromeTexel> GetEnumerator()
        {
            var pixels = readableTexture.GetPixels(0);
            var width = readableTexture.width;
            var height = readableTexture.height;
            var index = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                    yield return new MonochromeTexel(x, y, pixels[index++].r);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
