using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(ProxyBase<Color>))]
    public class ProxyColorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            Rect rect = position;
            rect.height = height;
            SerializedProperty originProp = property.FindPropertyRelative("origin");
            SerializedProperty endProp = property.FindPropertyRelative("end");
            SerializedProperty hdrProp = property.FindPropertyRelative("HDR");
            EditorGUI.PropertyField(rect, hdrProp);
            rect.y += height;
            originProp.colorValue = EditorGUI.ColorField(rect, new GUIContent(originProp.displayName), originProp.colorValue, true, true, hdrProp.boolValue);
            rect.y += height;
            endProp.colorValue = EditorGUI.ColorField(rect, new GUIContent(endProp.displayName), endProp.colorValue, true, true, hdrProp.boolValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }
}