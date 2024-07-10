using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(RouteSetting))]
    public class RouteSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            SerializedProperty enableProp = property.FindPropertyRelative("enable");
            SerializedProperty constraintTypeProp = property.FindPropertyRelative("constraintType");
            SerializedProperty lockOppositeProp = property.FindPropertyRelative("lockOpposite");
            SerializedProperty invFacingProp = property.FindPropertyRelative("invFacing");
            SerializedProperty facingRotationProp = property.FindPropertyRelative("facingRotation");
            SerializedProperty extendingOffsetProp = property.FindPropertyRelative("extendingOffset");
            SerializedProperty moveCurveProp = property.FindPropertyRelative("moveCurve");
            SerializedProperty lookAtCurveProp = property.FindPropertyRelative("lookAtCurve");
            SerializedProperty lookAtWeightCurveProp = property.FindPropertyRelative("lookAtWeightCurve");
            Rect rect = position;
            rect.height = height;
            if (GUI.Button(rect, "Enable Route", enableProp.boolValue ? "WarningOverlay" : "ProjectBrowserHeaderBgTop"))
                enableProp.boolValue = !enableProp.boolValue;
            if (!enableProp.boolValue)
                return;

            rect = position;
            rect.y += height + 5;
            rect.height -= height;
            GUI.BeginGroup(rect, GUI.skin.GetStyle("CurveEditorBackground"));
            rect = new Rect(5, 2, position.width - 10, height);
            EditorGUI.PropertyField(rect, constraintTypeProp);

            float width = position.width - 40;
            EditorGUI.BeginDisabledGroup(constraintTypeProp.enumValueIndex == 0);
            rect = new Rect(20, height + 2, width / 2, height);
            EditorGUI.PropertyField(rect, lockOppositeProp);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(constraintTypeProp.enumValueIndex == 0 || constraintTypeProp.enumValueIndex == 2);
            rect = new Rect(20 + width / 2, height + 2, width / 2, height);
            EditorGUI.PropertyField(rect, invFacingProp);

            rect = new Rect(20, height * 2 + 2, position.width - 25, height);
            EditorGUI.PropertyField(rect, facingRotationProp);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(constraintTypeProp.enumValueIndex == 0 || constraintTypeProp.enumValueIndex == 1);
            rect = new Rect(20, height * 3 + 2, position.width - 25, height);
            EditorGUI.PropertyField(rect, extendingOffsetProp);
            EditorGUI.EndDisabledGroup();

            rect = new Rect(5, height * 4 + 2, position.width - 10, height);
            EditorGUI.PropertyField(rect, moveCurveProp);

            rect = new Rect(5, height * 5 + 2, position.width - 10, height);
            EditorGUI.PropertyField(rect, lookAtCurveProp);

            rect = new Rect(5, height * 6 + 2, position.width - 10, height);
            EditorGUI.PropertyField(rect, lookAtWeightCurveProp);
            GUI.EndGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty enableProp = property.FindPropertyRelative("enable");
            float height = enableProp.boolValue ? EditorGUIUtility.singleLineHeight * 7 + 5 : 0;
            return EditorGUIUtility.singleLineHeight + height;
        }

        public static void DrawRouteSetting(SerializedProperty property, SplineEditorTool splineEditor)
        {
            SerializedProperty enableProp = property.FindPropertyRelative("enable");
            if (enableProp.boolValue)
            {
                EditorGUILayout.BeginVertical("FrameBox");
                EditorGUILayout.BeginVertical("dockarea");
                splineEditor.DrawEditorPreviewSetting(true, "Editor");
                EditorGUILayout.EndVertical();
                splineEditor.OnInspectorGUI(false);
                EditorGUILayout.EndVertical();
            }
        }
    }
}