using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Maskox
{
    public static class UnityEditorExtensions
    {
        private static Texture2D temporaryTexture = null;
        private static readonly Dictionary<ulong, AnimationCurve> curveCache = new Dictionary<ulong, AnimationCurve>();

        public static AnimationCurve ToCurve(this Texture texture)
        {
            if (texture == null)
                return null;

            AnimationCurve curve = null;
            var cacheKey = ((ulong)texture.updateCount << 32) | (uint)texture.GetInstanceID();
            if (texture == null || curveCache.TryGetValue(cacheKey, out curve))
                return curve;

            var texture2D = texture as Texture2D;
            if (texture2D != null)
                curve = texture2D.ToCurve();

            var customRenderTexture = texture as CustomRenderTexture;
            if (customRenderTexture != null)
                curve = customRenderTexture.ToCurve();

            if (curve != null)
            {
                if (curveCache.Count > 16)
                    curveCache.Clear();

                curveCache[cacheKey] = curve;
            }

            return curve;
        }

        private static AnimationCurve ToCurve(this Texture2D texture)
        {
            if (texture.isReadable)
            {
                var curve = new AnimationCurve();

                const int Samples = 256;
                const float dx = 1.0f / Samples;

                var y0 = GetY(texture, 0.0f);
                curve.AddKey(new Keyframe(0.0f, y0, 1.0f, 1.0f, 0.0f, 0.0f));

                var y1 = GetY(texture, dx);
                curve.AddKey(new Keyframe(dx, y1, 1.0f, 1.0f, 0.0f, 0.0f));

                for (var i = 2; i <= Samples; i++)
                {
                    var x = (float)i / Samples;
                    var y = GetY(texture, x);
                    var keyframe = new Keyframe(x, y, 1.0f, 1.0f, 0.0f, 0.0f);
                    if (Mathf.Approximately((y - y1) / dx, (y1 - y0) / dx))
                        curve.MoveKey(curve.length - 1, keyframe);
                    else
                        curve.AddKey(keyframe);

                    y0 = y1;
                    y1 = y;
                }

                return curve;
            }
            else
            {
                var texturePath = AssetDatabase.GetAssetPath(texture);

                var textureData = File.ReadAllBytes(texturePath);
                var temporaryTexture = GetTemporaryTexture(texture.width, texture.height);
                if (temporaryTexture.LoadImage(textureData, false) && temporaryTexture.isReadable)
                    return temporaryTexture.ToCurve();
                else
                    return null;
            }
        }

        private static AnimationCurve ToCurve(this CustomRenderTexture texture)
        {
            var temporaryTexture = GetTemporaryTexture(texture.width, texture.height);

            var originalActiveRenderTexture = RenderTexture.active;
            try
            {
                RenderTexture.active = texture;
                Graphics.SetRenderTarget(texture);
                temporaryTexture.ReadPixels(new Rect(0, 0, temporaryTexture.width, temporaryTexture.height), 0, 0);
                temporaryTexture.Apply(false, false);
            }
            finally
            {
                Graphics.SetRenderTarget(null);
                RenderTexture.active = originalActiveRenderTexture;
            }

            return temporaryTexture.ToCurve();
        }

        private static float GetY(Texture2D texture, float x)
            => texture.GetPixelBilinear(x, 0.5f).grayscale;

        private static Texture2D GetTemporaryTexture(int width, int height)
        {
            if (temporaryTexture == null)
            {
                temporaryTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
                temporaryTexture.hideFlags = HideFlags.DontSave;
                temporaryTexture.wrapMode = TextureWrapMode.Clamp;
            }

            if (temporaryTexture.width != width || temporaryTexture.height != height)
                temporaryTexture.Resize(width, height);

            return temporaryTexture;
        }
    }
}
