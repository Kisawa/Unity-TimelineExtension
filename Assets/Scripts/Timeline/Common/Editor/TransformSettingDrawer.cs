using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(TransformSetting))]
    public class TransformSettingDrawer : PropertyDrawer
    {
        static GUIStyle Style_HeaderText => GUI.skin.GetStyle("HeaderLabel");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            SerializedProperty positionProp = property.FindPropertyRelative("position");
            SerializedProperty eulerAngleProp = property.FindPropertyRelative("eulerAngle");
            SerializedProperty scaleProp = property.FindPropertyRelative("scale");
            Rect rect = position;
            rect.x -= 10;
            rect.width += 10;
            EditorGUI.LabelField(rect, "Transform Setting", Style_HeaderText);

            rect = position;
            rect.y += height;
            rect.x += 15;
            rect.width = 85;
            EditorGUI.LabelField(rect, "Position");
            rect.x += 85;
            rect.width = position.width - 100;
            Vector3 pos = EditorGUI.Vector3Field(rect, "", positionProp.vector3Value);
            positionProp.vector3Value = pos;

            rect = position;
            rect.y += height * 2;
            rect.x += 15;
            rect.width = 85;
            EditorGUI.LabelField(rect, "Rotation");
            rect.x += 85;
            rect.width = position.width - 100;
            Vector3 eulerAngle = EditorGUI.Vector3Field(rect, "", eulerAngleProp.vector3Value);
            eulerAngleProp.vector3Value = eulerAngle;

            rect = position;
            rect.y += height * 3;
            rect.x += 15;
            rect.width = 85;
            EditorGUI.LabelField(rect, "Scale");
            rect.x += 85;
            rect.width = position.width - 100;
            Vector3 scale = EditorGUI.Vector3Field(rect, "", scaleProp.vector3Value);
            scaleProp.vector3Value = scale;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4;
        }
    }
}