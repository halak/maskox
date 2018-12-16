using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Maskox
{
    public static class TextureGenerator
    {
        private static readonly string AutoLabel = "MonochromePainter.Auto";

        public static async Task ExportAsync(CustomRenderTexture customRenderTexture, float progress)
        {
            EditorUtility.DisplayProgressBar("Monochrome Painter - Export", customRenderTexture.name, progress);

            customRenderTexture.Initialize();

            await Task.Delay(100);

            var revision = customRenderTexture.updateCount;

            customRenderTexture.Update();

            while (customRenderTexture.updateCount <= revision)
                await Task.Delay(16);

            var texture = new Texture2D(
                customRenderTexture.width,
                customRenderTexture.height,
                TextureFormat.ARGB32,
                false,
                customRenderTexture.descriptor.sRGB == false);

            Graphics.SetRenderTarget(customRenderTexture);
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply(false, false);

            var bytes = texture.EncodeToPNG();

            UnityEngine.Object.DestroyImmediate(texture);

            var assetPath = AssetDatabase.GetAssetPath(customRenderTexture);
            File.WriteAllBytes(Path.ChangeExtension(assetPath, "png"), bytes);
        }

        [MenuItem("Assets/Monochrome Painter/Export")]
        private static void ExportAll()
        {
            EditorUtility.DisplayProgressBar("Monochrome Painter - Export", "Starting...", 0.0f);

            var context = TaskScheduler.FromCurrentSynchronizationContext();
            var assetGUIDs = AssetDatabase.FindAssets($"l:{AutoLabel} t:{nameof(CustomRenderTexture)}");

            Task chainedTask = null;
            for (var i = 0; i < assetGUIDs.Length; i++)
            {
                var progress = (float)i / assetGUIDs.Length;
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                var asset = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(assetPath);
                if (asset != null)
                {
                    if (chainedTask == null)
                    {
                        chainedTask = Task.Factory.StartNew(
                            (state) => ExportAsync((CustomRenderTexture)state, progress),
                            asset,
                            CancellationToken.None,
                            TaskCreationOptions.LongRunning,
                            context).Unwrap();
                    }
                    else
                    {

                        chainedTask = chainedTask.ContinueWith(
                            (task, state) => ExportAsync((CustomRenderTexture)state, progress),
                            asset,
                            CancellationToken.None,
                            TaskContinuationOptions.LongRunning,
                            context).Unwrap();
                    }
                }
            }

            if (chainedTask != null)
            {
                chainedTask = chainedTask
                    .ContinueWith((task) =>
                    {
                        EditorUtility.ClearProgressBar();
                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    context);
            }
        }
    }
}
