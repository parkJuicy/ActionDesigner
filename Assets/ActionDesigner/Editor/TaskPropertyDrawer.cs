using ActionDesigner.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    /// <summary>
    /// Task 타입을 위한 PropertyDrawer - SerializeReferenceExtensions 기반
    /// </summary>
    [CustomPropertyDrawer(typeof(Task), true)]
    public class TaskPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // PropertyField를 사용하되, Task 타입에는 SubclassSelectorAttribute가 적용되어
            // SerializeReferenceExtensions의 예쁜 UI가 자동으로 사용됨
            var propertyField = new PropertyField(property);
            
            // 라벨 개선
            if (!string.IsNullOrEmpty(property.displayName))
            {
                propertyField.label = property.displayName;
            }
            
            // 스타일 적용
            propertyField.style.marginBottom = 2;
            
            return propertyField;
        }

        // IMGUI 폴백 - SerializeReferenceExtensions가 자동으로 처리
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
