using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Maskox
{
    public sealed class ContourDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            base.OnGUI(position, prop, label, editor);
        }
    }
}
