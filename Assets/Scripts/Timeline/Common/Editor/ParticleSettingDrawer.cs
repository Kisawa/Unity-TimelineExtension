using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomTimeline
{
    [CustomPropertyDrawer(typeof(ParticleSetting))]
    public class ParticleSettingDrawer : PropertyDrawer
    {
        static GUIStyle Style_HeaderText => GUI.skin.GetStyle("HeaderLabel");
        const int k_MaxRandInt = 10000;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            SerializedProperty autoRandomSeedProp = property.FindPropertyRelative("autoRandomSeed");
            SerializedProperty randomSeedProp = property.FindPropertyRelative("randomSeed");
            Rect rect = position;
            rect.x -= 10;
            rect.width += 10;
            EditorGUI.LabelField(rect, "Particle Setting", Style_HeaderText);

            rect = position;
            rect.y += height;
            rect.x += 15;
            rect.width -= 115;
            bool autoRandomSeed = EditorGUI.Toggle(rect, "Auto Random Seed", autoRandomSeedProp.boolValue);
            autoRandomSeedProp.boolValue = autoRandomSeed;
            rect.x += rect.width;
            rect.width = 100;
            rect.height -= 1;
            if (!autoRandomSeed && GUI.Button(rect, "Random"))
                randomSeedProp.intValue = Random.Range(1, k_MaxRandInt);

            EditorGUI.BeginDisabledGroup(autoRandomSeed);
            rect = position;
            rect.y += height * 2;
            rect.x += 15;
            rect.width -= 15;
            int randomSeed = EditorGUI.IntField(rect, "Random Seed", randomSeedProp.intValue);
            randomSeed = Mathf.Max(0, randomSeed);
            randomSeedProp.intValue = randomSeed;
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }
}