using UnityEngine;
using UnityEditor;

namespace Maskox
{
    public sealed class MaskTextureDrawer : MaterialPropertyDrawer
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            => GetTextureFieldHeight();

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type == MaterialProperty.PropType.Texture &&
                prop.textureDimension == UnityEngine.Rendering.TextureDimension.Tex2D)
            {
                var texture = prop.textureValue;
                if (texture != null)
                {
                    var customArea = editor.GetTexturePropertyCustomArea(position);
                    customArea.xMax -= EditorStyles.objectField.margin.left;

                    var spacing = EditorGUIUtility.standardVerticalSpacing;
                    var offsetArea = Rect.MinMaxRect(
                        customArea.xMin,
                        customArea.yMax - EditorGUIUtility.singleLineHeight,
                        customArea.xMax,
                        customArea.yMax);
                    var sizeArea = Rect.MinMaxRect(
                        customArea.xMin,
                        offsetArea.yMin - spacing - EditorGUIUtility.singleLineHeight,
                        customArea.xMax,
                        offsetArea.yMin - spacing);

                    var vector = prop.textureScaleAndOffset;

                    EditorGUI.BeginChangeCheck();
                    var originalLabelWidth = EditorGUIUtility.labelWidth;
                    try
                    {
                        EditorGUIUtility.labelWidth = customArea.width * 0.4f;
                        vector.x = Mathf.Clamp(EditorGUI.FloatField(sizeArea, "Length", vector.x), 0.001f, 1.0f);
                        vector.z = EditorGUI.Slider(offsetArea, "Position", vector.z, 0.0f, 1.0f);
                    }
                    finally
                    {
                        if (EditorGUI.EndChangeCheck())
                        {
                            prop.textureScaleAndOffset = vector;
                        }

                        EditorGUIUtility.labelWidth = originalLabelWidth;
                    }
                }
            }

            editor.TextureProperty(position, prop, label.text, label.tooltip, false);
        }

        internal static float GetTextureFieldHeight() => 64.0f;
    }
}
