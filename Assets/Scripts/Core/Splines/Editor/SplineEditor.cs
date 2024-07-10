using UnityEditor;
using UnityEngine;

namespace Splines
{
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        Spline self;
        Transform trans;
        SerializedProperty offsetProp;

        [SerializeField] SplineEditorTool tool;
        [SerializeField] SplineAsset copyAsset;

        private void OnEnable()
        {
            self = (Spline)target;
            trans = self.transform;
            offsetProp = serializedObject.FindProperty("offset");
            SerializedProperty dataProp = serializedObject.FindProperty("data");
            tool = new SplineEditorTool(this, serializedObject, dataProp, self.data);
            tool.func_GetScale += GetScale;
            tool.func_InverseTransformPoint += InverseTransformPoint;
            tool.func_TransformDirection += TransformDirection;
            tool.func_CalcOffset += CalcOffset;
            tool.func_CalcInverseOffset += CalcInverseOffset;
            tool.func_CalcPositionWS += CalcPositionWS;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical("dockarea");
            if (self.data.points.Count == 0)
            {
                tool.DrawEditorPreviewSetting(false, "Editor");
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                SplineAsset _copyAsset = EditorGUILayout.ObjectField(copyAsset, typeof(SplineAsset), false) as SplineAsset;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "SplineEditor: CopyAsset changed");
                    copyAsset = _copyAsset;
                }
                EditorGUI.BeginDisabledGroup(copyAsset == null);
                if (GUILayout.Button("Copy from SplineAsset"))
                    CopyFromSplineAsset(copyAsset);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
            else
            {
                tool.DrawEditorPreviewSetting(true, "Editor");
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create SplineAsset", GUILayout.Width(EditorGUIUtility.currentViewWidth * .6f)))
                    CreateSplineAsset();
                GUILayout.Space(EditorGUIUtility.currentViewWidth * .015f);
                if (GUILayout.Button("Clear", GUILayout.Width(EditorGUIUtility.currentViewWidth * .385f - 45)))
                    tool.ClearData();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(offsetProp);
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(5);
            tool.OnInspectorGUI(false);
        }

        private void OnSceneGUI()
        {
            tool.OnSceneGUI(Color.white);
        }

        void CopyFromSplineAsset(SplineAsset asset)
        { 
            if(asset == null)
                return;
            Undo.RecordObject(self, "SplineEditor: CopyAsset");
            SerializedObject assetSerializedObject = new SerializedObject(asset);
            SerializedProperty _dataProp = assetSerializedObject.FindProperty("data");
            SerializedProperty _pointsProp = _dataProp.FindPropertyRelative("points");
            SerializedProperty _pointsExtraProp = _dataProp.FindPropertyRelative("editorPointsExtra");
            assetSerializedObject.Update();
            tool.typeProp.enumValueIndex = _dataProp.FindPropertyRelative("type").enumValueIndex;
            tool.EuclideanLengthProp.floatValue = _dataProp.FindPropertyRelative("euclideanLength").floatValue;
            tool.pointsProp.ClearArray();
            tool.pointsExtraProp.ClearArray();
            for (int i = 0; i < _pointsProp.arraySize; i++)
            {
                SerializedProperty _itemPoint = _pointsProp.GetArrayElementAtIndex(i);
                SerializedProperty _itemPointExtra = _pointsExtraProp.GetArrayElementAtIndex(i);

                tool.pointsProp.InsertArrayElementAtIndex(tool.pointsProp.arraySize);
                SerializedProperty itemPointProp = tool.pointsProp.GetArrayElementAtIndex(tool.pointsProp.arraySize - 1);
                tool.pointsExtraProp.InsertArrayElementAtIndex(tool.pointsExtraProp.arraySize);
                SerializedProperty itemPointExtraProp = tool.pointsExtraProp.GetArrayElementAtIndex(tool.pointsExtraProp.arraySize - 1);
                itemPointProp.FindPropertyRelative("point").vector3Value = _itemPoint.FindPropertyRelative("point").vector3Value;
                itemPointProp.FindPropertyRelative("forwardTangentOffset").vector3Value = _itemPoint.FindPropertyRelative("forwardTangentOffset").vector3Value;
                itemPointProp.FindPropertyRelative("backTangentOffset").vector3Value = _itemPoint.FindPropertyRelative("backTangentOffset").vector3Value;
                itemPointExtraProp.FindPropertyRelative("tangentHandleType").enumValueIndex = _itemPointExtra.FindPropertyRelative("tangentHandleType").enumValueIndex;
                itemPointExtraProp.FindPropertyRelative("rotation").quaternionValue = _itemPointExtra.FindPropertyRelative("rotation").quaternionValue;
                itemPointExtraProp.FindPropertyRelative("forwardTangentRotation").quaternionValue = _itemPointExtra.FindPropertyRelative("forwardTangentRotation").quaternionValue;
                itemPointExtraProp.FindPropertyRelative("backTangentRotation").quaternionValue = _itemPointExtra.FindPropertyRelative("backTangentRotation").quaternionValue;
            }
        }

        void CreateSplineAsset()
        {
            SplineAsset asset = CreateInstance<SplineAsset>();
            for (int i = 0; i < self.data.points.Count; i++)
                asset.data.points.Add(self.data.points[i]);
            SerializedObject assetSerializedObject = new SerializedObject(asset);
            SerializedProperty dataProp = assetSerializedObject.FindProperty("data");
            assetSerializedObject.Update();
            dataProp.FindPropertyRelative("type").enumValueIndex = tool.typeProp.enumValueIndex;
            dataProp.FindPropertyRelative("euclideanLength").floatValue = tool.EuclideanLengthProp.floatValue;
            SerializedProperty extraProp = dataProp.FindPropertyRelative("editorPointsExtra");
            for (int i = 0; i < tool.pointsExtraProp.arraySize; i++)
            {
                SerializedProperty item = tool.pointsExtraProp.GetArrayElementAtIndex(i);
                extraProp.InsertArrayElementAtIndex(extraProp.arraySize);
                SerializedProperty itemProp = extraProp.GetArrayElementAtIndex(extraProp.arraySize - 1);
                itemProp.FindPropertyRelative("tangentHandleType").enumValueIndex = item.FindPropertyRelative("tangentHandleType").enumValueIndex;
                itemProp.FindPropertyRelative("rotation").quaternionValue = item.FindPropertyRelative("rotation").quaternionValue;
                itemProp.FindPropertyRelative("forwardTangentRotation").quaternionValue = item.FindPropertyRelative("forwardTangentRotation").quaternionValue;
                itemProp.FindPropertyRelative("backTangentRotation").quaternionValue = item.FindPropertyRelative("backTangentRotation").quaternionValue;
            }
            assetSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            
            string folder = trans.gameObject.scene.path;
            int lastStrIndex = folder.LastIndexOf('/');
            folder = folder.Substring(0, lastStrIndex);
            string path = $"{folder}/{CheckName<SplineAsset>(folder, trans.name + "_Spline", "asset")}.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
        }

        Vector3 GetScale()
        {
            return trans.lossyScale;
        }

        Vector3 InverseTransformPoint(Vector3 position)
        {
            return trans.InverseTransformPoint(position) - offsetProp.vector3Value;
        }

        Vector3 TransformDirection(Vector3 dir)
        {
            Vector3 scale = trans.lossyScale;
            dir.x *= scale.x;
            dir.y *= scale.y;
            dir.z *= scale.z;
            return trans.TransformDirection(dir.normalized);
        }

        Vector3 CalcOffset(Vector3 offset)
        {
            return trans.TransformVector(offset);
        }

        Vector3 CalcInverseOffset(Vector3 offset)
        {
            return trans.InverseTransformVector(offset);
        }

        Vector3 CalcPositionWS(Vector3 point)
        {
            return trans.TransformPoint(point + offsetProp.vector3Value);
        }

        static string CheckName<T>(string folderPath, string name, string suf) where T : Object
        {
            string newName = name;
            int tryIndex = 1;
            while (true)
            {
                string path = $"{folderPath}/{newName}.{suf}";
                if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
                    break;
                else
                {
                    string end = tryIndex.ToString("D2");
                    newName = $"{name} {end}";
                    tryIndex++;
                }
            }
            return newName;
        }
    }
}