using System;
using UnityEngine;
using UnityEngine.UI;

namespace Maskox
{
    public sealed class SlideShow : MonoBehaviour
    {
        [SerializeField]
        private Shader shader = null;
        [SerializeField]
        private Graphic[] graphics = Array.Empty<Graphic>();
        [SerializeField]
        private Texture[] maskTextures = Array.Empty<Texture>();
        [SerializeField]
        private Texture contourTexture = null;
        [SerializeField, Range(0.01f, 1.0f)]
        private float contourLength = 0.4f;
        [SerializeField, Range(0.01f, 3.0f)]
        private float transitionTime = 1.0f;
        [SerializeField, Range(0.0f, 5.0f)]
        private float delayTime = 1.0f;

        private int maskTextureId = 0;
        private int contourTextureId = 0;
        private Graphic currentGraphic = null;
        private float currentTime = 0.0f;

        private void Start()
        {
            maskTextureId = Shader.PropertyToID("_Maskox_MaskTex");
            contourTextureId = Shader.PropertyToID("_Maskox_ContourTex");
            graphics = graphics ?? Array.Empty<Graphic>();

            for (var i = 0; i < graphics.Length; i++)
            {
                var material = new Material(shader);
                material.hideFlags = HideFlags.DontSave;
                graphics[i].material = material;
            }
        }

        private void Update()
        {
            currentTime += Time.deltaTime;

            while (currentTime >= transitionTime + delayTime)
            {
                currentTime -= transitionTime + delayTime;

                if (currentGraphic != null)
                    currentGraphic.material.SetTextureOffset(maskTextureId, new Vector2(1.0f, 0.0f));

                var index = UnityEngine.Random.Range(0, graphics.Length - 1);
                var selectedGraphic = graphics[index];
                if (selectedGraphic == currentGraphic)
                    selectedGraphic = graphics[++index];

                selectedGraphic.transform.SetAsLastSibling();
                selectedGraphic.material.SetTexture(maskTextureId, maskTextures[UnityEngine.Random.Range(0, maskTextures.Length)]);
                currentGraphic = selectedGraphic;
            }

            if (currentGraphic != null)
            {
                var t = Mathf.Clamp01((currentTime - delayTime) / transitionTime);
                var material = currentGraphic.material;
                material.SetTextureOffset(maskTextureId, new Vector2(t, 0.0f));
                material.SetTextureScale(maskTextureId, new Vector2(contourLength, 1.0f));
                material.SetTexture(contourTextureId, contourTexture);
            }
        }
    }
}
