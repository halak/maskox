using UnityEngine;
using UnityEditor;

namespace Maskox
{
    public sealed class ContourTextureDrawer : MaterialPropertyDrawer
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            => MaskTextureDrawer.GetTextureFieldHeight();

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type == MaterialProperty.PropType.Texture &&
                prop.textureDimension == UnityEngine.Rendering.TextureDimension.Tex2D)
            {
                var texture = prop.textureValue;
                if (texture != null)
                {
                    var curve = texture.ToCurve();
                    if (curve != null)
                    {
                        var curveArea = editor.GetTexturePropertyCustomArea(position);
                        curveArea.xMax -= EditorStyles.objectField.margin.left;
                        EditorGUI.CurveField(curveArea, curve);
                    }
                }
            }

            editor.TextureProperty(position, prop, label.text, label.tooltip, false);
        }
    }
}
