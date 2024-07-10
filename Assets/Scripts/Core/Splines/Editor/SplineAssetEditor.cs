using UnityEditor;
using UnityEngine;

namespace Splines
{
    [CustomEditor(typeof(SplineAsset))]
    public class SplineAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
}