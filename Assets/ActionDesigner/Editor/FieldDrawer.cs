using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
            var returnValue = DrawValue(field.Name, field.FieldType, value);

            if (returnValue == null)
            {
                field.SetValue(obj, null);
                _onChangeValue?.Invoke();
            }
            else if (!returnValue.Equals(value))
            {
                field.SetValue(obj, returnValue);
                _onChangeValue?.Invoke();
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
            else if (type.IsClass)
            {
                value = DrawObject(fieldName, value, type);
            }
            else
            {
                EditorGUILayout.LabelField(fieldName, "invalid Type: " + type.Name);
            }
            return value;
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
        protected virtual UnityEngine.Object DrawObject(string fieldName, object value, Type type)
        {
            return EditorGUILayout.ObjectField(fieldName, (UnityEngine.Object)value, type, true);
        }
    }
}