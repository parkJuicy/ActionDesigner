using ActionDesigner.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ActionDesigner.Editor
{
    /// <summary>
    /// Behavior 타입을 위한 PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(IBehavior), true)]
    public class BehaviorPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propertyField = new PropertyField(property);
            
            if (!string.IsNullOrEmpty(property.displayName))
            {
                propertyField.label = property.displayName;
            }
            
            propertyField.style.marginBottom = 2;
            return propertyField;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
    
    /// <summary>
    /// Condition 타입을 위한 PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(ICondition), true)]
    public class ConditionPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propertyField = new PropertyField(property);
            
            if (!string.IsNullOrEmpty(property.displayName))
            {
                propertyField.label = property.displayName;
            }
            
            propertyField.style.marginBottom = 2;
            return propertyField;
        }

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
