using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// object에서 파생된 클래스는 일반적으로 Editor에 표시하도록 지원되지 않는데,이를 각type별로 하나하나 그려주는 클래스
/// 참고링크 : https://hacchi-man.hatenablog.com/entry/2021/04/16/220000
/// </summary>
/// 

namespace ActionDesigner.Editor
{
    public class FieldDrawer
    {
        private Action _onChangeValue;

        public void Draw(object obj, List<FieldInfo> fieldInfos, Action onChangeValue)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            _onChangeValue = onChangeValue;
            foreach (var field in fieldInfos)
            {
                DrawField(field, obj);
            }
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void DrawField(FieldInfo field, object obj)
        {
            var value = field.GetValue(obj);
            
            // SerializeReference 속성 체크 (안전한 방법)
#if UNITY_2019_3_OR_NEWER
            bool hasSerializeReference = false;
            var attributes = field.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                if (attr.GetType().Name == "SerializeReferenceAttribute")
                {
                    hasSerializeReference = true;
                    break;
                }
            }
#else
            bool hasSerializeReference = false; // Unity 2019.3 이전에서는 지원하지 않음
#endif
            
            object returnValue;
            if (hasSerializeReference)
            {
                returnValue = DrawSerializeReference(field.Name, field.FieldType, value, field);
            }
            else
            {
                returnValue = DrawValue(field.Name, field.FieldType, value);
            }

            if (returnValue == null && value != null)
            {
                field.SetValue(obj, null);
                _onChangeValue?.Invoke();
            }
            else if (returnValue != null && !returnValue.Equals(value))
            {
                field.SetValue(obj, returnValue);
                _onChangeValue?.Invoke();
            }
            else if (returnValue != value) // null과 null이 아닌 경우 비교
            {
                field.SetValue(obj, returnValue);
                _onChangeValue?.Invoke();
            }
        }
        
        protected virtual object DrawSerializeReference(string fieldName, Type fieldType, object value, FieldInfo fieldInfo)
        {
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.BeginVertical("box");
            
            // 현재 타입 표시
            string currentTypeName = value?.GetType().Name ?? "None";
            EditorGUILayout.LabelField($"{fieldName} ({currentTypeName})", EditorStyles.boldLabel);
            
            // 타입 선택 드롭다운
            var availableTypes = GetSerializeReferenceTypes(fieldType);
            
            var typeNames = new string[availableTypes.Count + 1];
            typeNames[0] = "None";
            
            int currentIndex = 0;
            for (int i = 0; i < availableTypes.Count; i++)
            {
                typeNames[i + 1] = availableTypes[i].Name;
                if (value != null && availableTypes[i] == value.GetType())
                {
                    currentIndex = i + 1;
                }
            }
            
            int newIndex = EditorGUILayout.Popup("Type", currentIndex, typeNames);
            
            // 타입이 변경되었을 때
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    value = null;
                }
                else
                {
                    var newType = availableTypes[newIndex - 1];
                    try
                    {
                        value = Activator.CreateInstance(newType);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to create instance of {newType.Name}: {e.Message}");
                        value = null;
                    }
                }
            }
            
            // 현재 객체의 필드들 그리기
            if (value != null)
            {
                EditorGUI.indentLevel++;
                DrawObjectFields(value);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            return value;
#else
            // Unity 2019.3 이전 버전에서는 기본 방식으로 그리기
            return DrawValue(fieldName, fieldType, value);
#endif
        }
        
        private List<Type> GetSerializeReferenceTypes(Type baseType)
        {
            var types = new List<Type>();
            
            // 현재 도메인의 모든 어셈블리에서 해당 타입을 상속하는 클래스들 찾기
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        bool isValid = false;
                        
                        if (baseType.IsInterface)
                        {
                            // 인터페이스인 경우
                            isValid = type != baseType && 
                                     type.GetInterfaces().Contains(baseType) &&
                                     !type.IsAbstract && 
                                     !type.IsInterface &&
                                     type.IsSerializable;
                        }
                        else
                        {
                            // 클래스인 경우
                            isValid = type != baseType && 
                                     baseType.IsAssignableFrom(type) &&
                                     !type.IsAbstract && 
                                     type.IsSerializable;
                        }
                        
                        if (isValid)
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (System.Exception)
                {
                    // 일부 어셈블리에서 타입 로드 실패는 무시
                    continue;
                }
            }
            
            return types.OrderBy(t => t.Name).ToList();
        }
        
        private void DrawObjectFields(object obj)
        {
            if (obj == null) return;
            
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                bool isPublic = field.IsPublic;
                bool isSerializeField = Attribute.IsDefined(field, typeof(SerializeField));
                
                if (isPublic || isSerializeField)
                {
                    var value = field.GetValue(obj);
                    var newValue = DrawValue(field.Name, field.FieldType, value);
                    
                    if (newValue == null && value != null)
                    {
                        field.SetValue(obj, null);
                        _onChangeValue?.Invoke();
                    }
                    else if (newValue != null && !newValue.Equals(value))
                    {
                        field.SetValue(obj, newValue);
                        _onChangeValue?.Invoke();
                    }
                    else if (newValue != value)
                    {
                        field.SetValue(obj, newValue);
                        _onChangeValue?.Invoke();
                    }
                }
            }
        }

        private object DrawValue(string fieldName, Type type, object value)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                value = DrawNullable(fieldName, type, value);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                value = DrawList(fieldName, type.GetGenericArguments()[0], (IList)value);
            }
            else if (type.IsEnum)
            {
                value = DrawEnum(fieldName, type, (Enum)value);
            }
            else if (type == typeof(bool))
            {
                value = DrawBool(fieldName, (bool)value);
            }
            else if (type == typeof(int))
            {
                value = DrawInt(fieldName, (int)value);
            }
            else if (type == typeof(uint))
            {
                value = DrawUint(fieldName, (uint)value);
            }
            else if (type == typeof(float))
            {
                value = DrawFloat(fieldName, (float)value);
            }
            else if (type == typeof(string))
            {
                value = DrawString(fieldName, (string)value);
            }
            else if (type == typeof(Vector2))
            {
                value = DrawVector2(fieldName, (Vector2)value);
            }
            else if (type == typeof(Vector2Int))
            {
                value = DrawVector2Int(fieldName, (Vector2Int)value);
            }
            else if (type == typeof(Vector3))
            {
                value = DrawVector3(fieldName, (Vector3)value);
            }
            else if (type == typeof(Vector3Int))
            {
                value = DrawVector3Int(fieldName, (Vector3Int)value);
            }
            else if (type == typeof(Vector4))
            {
                value = DrawVector4(fieldName, (Vector4)value);
            }
            else if (type == typeof(Color))
            {
                value = DrawColor(fieldName, (Color)value);
            }
            else if (type == typeof(AnimationCurve))
            {
                value = DrawAnimationCurve(fieldName, (AnimationCurve)value);
            }
            else if (type == typeof(Gradient))
            {
                value = DrawGradient(fieldName, (Gradient)value);
            }
            else if (type.IsClass || type.IsInterface)
            {
                value = DrawObject(fieldName, value, type);
            }
            else
            {
                EditorGUILayout.LabelField(fieldName, "invalid Type: " + type.Name);
            }
            return value;
        }
        
        private object DrawSerializeReferenceDirectly(string fieldName, Type fieldType, object value)
        {
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.BeginVertical("box");
            
            // 현재 타입 표시
            string currentTypeName = value?.GetType().Name ?? "None";
            EditorGUILayout.LabelField($"{fieldName} ({currentTypeName})", EditorStyles.boldLabel);
            
            // 타입 선택 드롭다운
            var availableTypes = GetSerializeReferenceTypes(fieldType);
            
            var typeNames = new string[availableTypes.Count + 1];
            typeNames[0] = "None";
            
            int currentIndex = 0;
            for (int i = 0; i < availableTypes.Count; i++)
            {
                typeNames[i + 1] = availableTypes[i].Name;
                if (value != null && availableTypes[i] == value.GetType())
                {
                    currentIndex = i + 1;
                }
            }
            
            int newIndex = EditorGUILayout.Popup("Type", currentIndex, typeNames);
            
            // 타입이 변경되었을 때
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    value = null;
                }
                else
                {
                    var newType = availableTypes[newIndex - 1];
                    try
                    {
                        value = Activator.CreateInstance(newType);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to create instance of {newType.Name}: {e.Message}");
                        value = null;
                    }
                }
            }
            
            // 현재 객체의 필드들 그리기
            if (value != null)
            {
                EditorGUI.indentLevel++;
                DrawObjectFields(value);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            return value;
#else
            return DrawObject(fieldName, value, fieldType);
#endif
        }
        protected virtual object DrawNullable(string fieldName, Type type, object value)
        {
            var baseType = Nullable.GetUnderlyingType(type);
            var hasValue = value != null;
            if (hasValue)
            {
                return DrawValue(fieldName + "?", baseType, value);
            }
            EditorGUILayout.LabelField(fieldName, "null");
            return value;
        }

        protected virtual IList DrawList(string fieldName, Type type, IList value)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    for (var i = 0; i < value.Count; i++)
                    {
                        var v = value[i];
                        value[i] = DrawValue(i.ToString(), type, v);
                    }
                }
            }
            return value;
        }

        protected virtual Enum DrawEnum(string fieldName, Type type, Enum value)
        {
            return EditorGUILayout.EnumPopup(fieldName, value);
        }
        protected virtual bool DrawBool(string fieldName, bool value)
        {
            return EditorGUILayout.Toggle(fieldName, value);
        }
        protected virtual int DrawInt(string fieldName, int value)
        {
            return EditorGUILayout.IntField(fieldName, value);
        }
        protected virtual object DrawUint(string fieldName, uint value)
        {
            return EditorGUILayout.FloatField(fieldName, value);
        }
        protected virtual float DrawFloat(string fieldName, float value)
        {
            return EditorGUILayout.FloatField(fieldName, value);
        }
        protected virtual string DrawString(string fieldName, string value)
        {
            return EditorGUILayout.TextField(fieldName, value);
        }
        protected virtual Vector2 DrawVector2(string fieldName, Vector2 value)
        {
            return EditorGUILayout.Vector2Field(fieldName, value);
        }
        protected virtual Vector2Int DrawVector2Int(string fieldName, Vector2Int value)
        {
            return EditorGUILayout.Vector2IntField(fieldName, value);
        }
        protected virtual Vector3 DrawVector3(string fieldName, Vector3 value)
        {
            return EditorGUILayout.Vector3Field(fieldName, value);
        }
        protected virtual Vector3Int DrawVector3Int(string fieldName, Vector3Int value)
        {
            return EditorGUILayout.Vector3IntField(fieldName, value);
        }
        protected virtual Vector3 DrawVector4(string fieldName, Vector4 value)
        {
            return EditorGUILayout.Vector4Field(fieldName, value);
        }
        protected virtual Color DrawColor(string fieldName, Color value)
        {
            return EditorGUILayout.ColorField(fieldName, value);
        }
        protected virtual AnimationCurve DrawAnimationCurve(string fieldName, AnimationCurve value)
        {
            return EditorGUILayout.CurveField(fieldName, value);
        }
        protected virtual Gradient DrawGradient(string fieldName, Gradient value)
        {
            return EditorGUILayout.GradientField(fieldName, value);
        }
        protected virtual object DrawObject(string fieldName, object value, Type type)
        {
            // UnityEngine.Object를 상속하는 경우에만 ObjectField 사용
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(fieldName, (UnityEngine.Object)value, type, true);
            }
            // 인터페이스이거나 추상 클래스인 경우 SerializeReference 스타일로 처리
            else if (type.IsInterface || type.IsAbstract)
            {
                return DrawSerializeReferenceStyle(fieldName, type, value);
            }
            else
            {
                // 일반 클래스인 경우 필드들을 재귀적으로 그리기
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(fieldName, EditorStyles.boldLabel);
                
                if (value != null)
                {
                    EditorGUI.indentLevel++;
                    DrawObjectFields(value);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField("Null");
                }
                
                EditorGUILayout.EndVertical();
                return value;
            }
        }
        
        private object DrawSerializeReferenceStyle(string fieldName, Type fieldType, object value)
        {
            EditorGUILayout.BeginVertical("box");
            
            // 현재 타입 표시
            string currentTypeName = value?.GetType().Name ?? "None";
            EditorGUILayout.LabelField($"{fieldName} ({currentTypeName})", EditorStyles.boldLabel);
            
            // 타입 선택 드롭다운
            var availableTypes = GetSerializeReferenceTypes(fieldType);
            
            var typeNames = new string[availableTypes.Count + 1];
            typeNames[0] = "None";
            
            int currentIndex = 0;
            for (int i = 0; i < availableTypes.Count; i++)
            {
                typeNames[i + 1] = availableTypes[i].Name;
                if (value != null && availableTypes[i] == value.GetType())
                {
                    currentIndex = i + 1;
                }
            }
            
            int newIndex = EditorGUILayout.Popup("Type", currentIndex, typeNames);
            
            // 타입이 변경되었을 때
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    value = null;
                }
                else
                {
                    var newType = availableTypes[newIndex - 1];
                    try
                    {
                        value = Activator.CreateInstance(newType);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to create instance of {newType.Name}: {e.Message}");
                        value = null;
                    }
                }
            }
            
            // 현재 객체의 필드들 그리기
            if (value != null)
            {
                EditorGUI.indentLevel++;
                DrawObjectFields(value);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            return value;
        }
    }
}