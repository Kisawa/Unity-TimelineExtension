using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(ProxyData))]
    public class ProxyDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(position, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty proxyProp = property.FindPropertyRelative("proxy");
            float proxyHeight = EditorGUI.GetPropertyHeight(proxyProp) + 5;
            return EditorGUIUtility.singleLineHeight * 2 + proxyHeight;
        }

        public static void Draw(Rect position, SerializedProperty property, float inputSpace = 0, float proxySpace = 5)
        {
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty curveProp = property.FindPropertyRelative("curve");
            Rect rect = position;
            rect.width = 60;
            EditorGUI.LabelField(rect, $"{nameProp.displayName}:", GUI.skin.GetStyle("BoldLabel"));
            rect.x += rect.width;
            rect.width = position.width - rect.width - inputSpace;
            nameProp.stringValue = EditorGUI.TextField(rect, nameProp.stringValue, GUI.skin.GetStyle("BoldTextField"));
            rect = position;
            rect.y += height;
            rect.width = 60;
            EditorGUI.LabelField(rect, $"{curveProp.displayName}:", GUI.skin.GetStyle("BoldLabel"));
            rect.x += rect.width;
            rect.width = position.width - rect.width - 75;
            EditorGUI.PropertyField(rect, curveProp, new GUIContent());
            rect.x += rect.width;
            rect.width = 75;
            rect.y += 1;
            rect.height -= 1;
            if (GUI.Button(rect, "Inv"))
                curveProp.animationCurveValue = PlayableUtils.InvAnimationCurve(curveProp.animationCurveValue);

            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(nameProp.stringValue));
            SerializedProperty proxyProp = property.FindPropertyRelative("proxy");
            rect = position;
            rect.x += proxySpace;
            rect.width -= proxySpace;
            rect.y += height * 2 + 1;
            rect.height = EditorGUI.GetPropertyHeight(proxyProp) + 4;
            GUI.BeginGroup(rect, GUI.skin.GetStyle("ProfilerDetailViewBackground"));
            rect = new Rect(-proxySpace, 2, rect.width + proxySpace, rect.height);
            EditorGUI.PropertyField(rect, proxyProp);
            GUI.EndGroup();
            EditorGUI.EndDisabledGroup();
        }
    }
}