using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(RhythmSetting))]
    public class RhythmSettingDrawer : PropertyDrawer
    {
        public static float minLimit;
        public static float maxLimit = 5;
        static GUIStyle Style_HeaderText => GUI.skin.GetStyle("HeaderLabel");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            SerializedProperty enableProp = property.FindPropertyRelative("Enable");
            SerializedProperty startProp = property.FindPropertyRelative("RhythmStart");
            SerializedProperty endProp = property.FindPropertyRelative("RhythmEnd");
            SerializedProperty curveProp = property.FindPropertyRelative("RhythmCurve");
            Rect rect = position;
            rect.x -= 13;
            rect.width += 13;
            rect.y += 1;
            enableProp.boolValue = EditorGUI.Toggle(rect, enableProp.boolValue);

            rect = position;
            rect.x += 25;
            rect.width -= 25;
            GUI.Label(rect, "Rhythm", Style_HeaderText);

            EditorGUI.BeginDisabledGroup(!enableProp.boolValue);

            EditorGUI.BeginDisabledGroup(minLimit >= maxLimit || minLimit == float.NegativeInfinity || maxLimit == float.PositiveInfinity);
            rect = position;
            rect.y += height;
            rect.x += 15;
            rect.width = 85;
            EditorGUI.LabelField(rect, "Time Range");
            float start = startProp.floatValue, end = endProp.floatValue;
            Rect rect0 = rect;
            rect0.x += 85;
            rect0.width = 50;
            start = EditorGUI.FloatField(rect0, "", start);
            start = Mathf.Clamp(start, minLimit, end);
            Rect rect1 = rect0;
            rect1.x += 50;
            rect1.width = position.width - 200;
            EditorGUI.MinMaxSlider(rect1, ref start, ref end, minLimit, maxLimit);
            Rect rect2 = rect1;
            rect2.x = rect1.x + rect1.width;
            rect2.width = 50;
            end = EditorGUI.FloatField(rect2, "", end);
            end = Mathf.Clamp(end, start, maxLimit);
            startProp.floatValue = start;
            endProp.floatValue = end;
            EditorGUI.EndDisabledGroup();

            rect = position;
            rect.y += height * 2;
            rect.x += 15;
            rect.width = 85;
            EditorGUI.LabelField(rect, "Curve");
            Rect rect3 = rect;
            rect3.x += 85;
            rect3.width = position.width - 100;
            EditorGUI.PropertyField(rect3, curveProp, new GUIContent());
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }
}