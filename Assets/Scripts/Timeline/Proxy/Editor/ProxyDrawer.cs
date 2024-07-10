using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(Proxy))]
    public class ProxyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = position;
            if (property.managedReferenceValue == null)
            {
                rect.y -= 3;
                EditorGUI.LabelField(rect, "Null Proxy", GUI.skin.GetStyle("PreMiniLabel"));
            }
            else
            {
                string path = property.propertyPath;
                if (property.NextVisible(true))
                {
                    do
                    {
                        rect.height = EditorGUI.GetPropertyHeight(property, true);
                        EditorGUI.PropertyField(rect, property, new GUIContent(property.displayName), true);
                        rect.y += rect.height;
                    }
                    while (property.NextVisible(false) && property.propertyPath.StartsWith(path));
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            if (property.managedReferenceValue == null)
                height = EditorGUIUtility.singleLineHeight;
            else
            {
                string path = property.propertyPath;
                if (property.NextVisible(true))
                {
                    do
                        height += EditorGUI.GetPropertyHeight(property, true);
                    while (property.NextVisible(false) && property.propertyPath.StartsWith(path));
                }
            }
            return height;
        }
    }
}