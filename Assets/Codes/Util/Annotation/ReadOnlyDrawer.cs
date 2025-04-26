using UnityEditor;
using UnityEngine;

namespace Codes.Util.Annotation
{
    public class ReadOnlyAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;  // 비활성화 (읽기 전용)
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;   // 다시 활성화
        }
    }
}