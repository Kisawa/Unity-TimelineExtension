using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(ProxyBase<Vector4>))]
    public class ProxyVectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            Rect rect = position;
            rect.height = height;
            SerializedProperty channelCountProp = property.FindPropertyRelative("channelCount");
            SerializedProperty originProp = property.FindPropertyRelative("origin");
            SerializedProperty endProp = property.FindPropertyRelative("end");
            channelCountProp.intValue = EditorGUI.Popup(rect, channelCountProp.displayName, channelCountProp.intValue - 2, new string[] { "Vector2", "Vector3", "Vector4" }) + 2;
            rect.y += 1;
            switch (channelCountProp.intValue)
            {
                case 2:
                    rect.y += height;
                    originProp.vector4Value = EditorGUI.Vector2Field(rect, originProp.displayName, originProp.vector4Value);
                    rect.y += height;
                    endProp.vector4Value = EditorGUI.Vector2Field(rect, endProp.displayName, endProp.vector4Value);
                    break;
                case 3:
                    rect.y += height;
                    originProp.vector4Value = EditorGUI.Vector3Field(rect, originProp.displayName, originProp.vector4Value);
                    rect.y += height;
                    endProp.vector4Value = EditorGUI.Vector3Field(rect, endProp.displayName, endProp.vector4Value);
                    break;
                default:
                    rect.y += height;
                    originProp.vector4Value = EditorGUI.Vector4Field(rect, originProp.displayName, originProp.vector4Value);
                    rect.y += height;
                    endProp.vector4Value = EditorGUI.Vector4Field(rect, endProp.displayName, endProp.vector4Value);
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 1;
        }
    }
}