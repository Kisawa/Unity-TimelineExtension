using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(EffectSetting))]
    public class EffectSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            SerializedProperty actOnTypeProp = property.FindPropertyRelative("actOnType");
            SerializedProperty constraintTypeProp = property.FindPropertyRelative("constraintType");
            SerializedProperty targetReferCoordinateProp = property.FindPropertyRelative("targetReferCoordinate");

            Rect rect = position;
            EditorGUI.PropertyField(rect, actOnTypeProp);

            rect.y += height;
            EditorGUI.PropertyField(rect, constraintTypeProp);

            if (constraintTypeProp.enumValueIndex == 2)
            {
                SerializedProperty fixPositionProp = property.FindPropertyRelative("fixPosition");
                SerializedProperty fixRotationProp = property.FindPropertyRelative("fixRotation");
                SerializedProperty unitScaleProp = property.FindPropertyRelative("unitScale");
                Rect _rect = rect;
                _rect.x += 20;
                _rect.width -= 20;
                _rect.y += height + 2;
                _rect.height = height * 3;
                GUI.BeginGroup(_rect, GUI.skin.GetStyle("HelpBox"));
                _rect = new Rect(0, 0, rect.width, height);
                EditorGUI.PropertyField(_rect, fixPositionProp);
                _rect = new Rect(0, height, rect.width, height);
                EditorGUI.PropertyField(_rect, fixRotationProp);
                _rect = new Rect(0, height * 2, rect.width, height);
                EditorGUI.PropertyField(_rect, unitScaleProp);
                GUI.EndGroup();
                rect.y += height * 3 + 7;
            }

            EditorGUI.BeginDisabledGroup(actOnTypeProp.enumValueIndex == 0);
            rect.y += height;
            EditorGUI.PropertyField(rect, targetReferCoordinateProp);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty constraintTypeProp = property.FindPropertyRelative("constraintType");
            float extraHeight = constraintTypeProp.enumValueIndex == 2 ? EditorGUIUtility.singleLineHeight * 3 + 7 : 0;
            return EditorGUIUtility.singleLineHeight * 3 + extraHeight;
        }
    }
}