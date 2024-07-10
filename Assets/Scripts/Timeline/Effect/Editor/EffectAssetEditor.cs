using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GameObjectNode;
using Splines;
using UnityEditor.Timeline;

namespace CustomTimeline
{
    [CustomEditor(typeof(EffectAsset))]
    public class EffectAssetEditor : Editor
    {
        EffectAsset self;
        GameObjectNode_Select _nodeTree;
        GameObjectNode_Select nodeTree
        { 
            get
            {
                if (_nodeTree == null && self.owner != null)
                    _nodeTree = new GameObjectNode_Select(self.owner.transform, "", gameObjectNodeCondition, gameObjectNodeOnSelectChanged);
                return _nodeTree;
            }
        }

        [SerializeField] SplineEditorTool routeSplineEditor;
        EffectInstance splineDrawInstance;

        SerializedProperty prefabProp;
        SerializedProperty instantiateTransPathProp;
        SerializedProperty effectSettingProp;
        SerializedProperty transformSettingProp;
        SerializedProperty particleSettingProp;
        SerializedProperty rhythmSettingProp;
        SerializedProperty routeSettingProp;

        private void OnEnable()
        {
            self = serializedObject.targetObject as EffectAsset;
            prefabProp = serializedObject.FindProperty("prefab");
            instantiateTransPathProp = serializedObject.FindProperty("instantiateTransPath");
            effectSettingProp = serializedObject.FindProperty("effectSetting");
            transformSettingProp = serializedObject.FindProperty("transformSetting");
            particleSettingProp = serializedObject.FindProperty("particleSetting");
            rhythmSettingProp = serializedObject.FindProperty("rhythmSetting");
            routeSettingProp = serializedObject.FindProperty("routeSetting");
            SerializedProperty routeProp = routeSettingProp.FindPropertyRelative("route");
            routeSplineEditor = new SplineEditorTool(this, serializedObject, routeProp, self.routeSetting.route);
            routeSplineEditor.func_CalcPositionWS += SplineTool_func_CalcPositionWS;
            routeSplineEditor.func_InverseTransformPoint += SplineTool_func_InverseTransformPoint;
            routeSplineEditor.func_TransformDirection += SplineTool_func_TransformDirection;
            routeSplineEditor.func_CalcOffset += SplineTool_func_CalcOffset;
            routeSplineEditor.func_CalcInverseOffset += SplineTool_func_CalcInverseOffset;
            SceneView.duringSceneGui += SceneView_duringSceneGui;
            Undo.undoRedoPerformed += undoRedoPerformed;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            Undo.undoRedoPerformed -= undoRedoPerformed;
        }

        private void SceneView_duringSceneGui(SceneView obj)
        {
            splineDrawInstance = null;
            if (self.instances == null)
                return;
            for (int i = 0; i < self.instances.Count; i++)
            {
                splineDrawInstance = self.instances[i];
                if (splineDrawInstance.gameObject == null)
                    continue;
                if (Tools.current == Tool.Rect && splineDrawInstance.gameObject.activeSelf)
                {
                    Quaternion rot = Tools.pivotRotation == PivotRotation.Local ? splineDrawInstance.transform.rotation : Quaternion.identity;
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos = Handles.DoPositionHandle(splineDrawInstance.transform.position, rot);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 offset = splineDrawInstance.TransformWorldToObjectVector(pos - splineDrawInstance.transform.position);
                        serializedObject.Update();
                        SerializedProperty positionProp = transformSettingProp.FindPropertyRelative("position");
                        positionProp.vector3Value += offset;
                        serializedObject.ApplyModifiedProperties();
                        Repaint();
                        TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
                    }
                    EditorGUI.BeginChangeCheck();
                    rot = Handles.DoRotationHandle(splineDrawInstance.transform.rotation, pos);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Quaternion rotate = splineDrawInstance.CorrectQuaternion(Quaternion.Inverse(splineDrawInstance.editor_PlayableRouteRotation * splineDrawInstance.TransformObjectToWorldRotation(Quaternion.identity)) * rot);
                        serializedObject.Update();
                        SerializedProperty eulerAngleProp = transformSettingProp.FindPropertyRelative("eulerAngle");
                        eulerAngleProp.vector3Value = rotate.eulerAngles;
                        serializedObject.ApplyModifiedProperties();
                        Repaint();
                        TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
                    }
                }
                if (self.routeSetting.enable)
                {
                    EditorGUI.BeginChangeCheck();
                    bool deformed = splineDrawInstance.routeRotation != Quaternion.identity || splineDrawInstance.routeScale != Vector3.one;
                    routeSplineEditor.OnSceneGUI(deformed ? new Color(1, 1, 1, .5f) : Color.white, !deformed);
                    if (EditorGUI.EndChangeCheck())
                        TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
                }
            }
        }

        private void undoRedoPerformed()
        {
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(prefabProp);
            EditorGUILayout.BeginVertical("Badge");
            EditorGUILayout.PropertyField(effectSettingProp);
            if (nodeTree != null)
                nodeTree.Draw(15);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(transformSettingProp);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(particleSettingProp);
            GUILayout.Space(10);
            RhythmSettingDrawer.maxLimit = (float)self._duration;
            EditorGUILayout.PropertyField(rhythmSettingProp);
            GUILayout.Space(20);
            EditorGUILayout.PropertyField(routeSettingProp);

            serializedObject.ApplyModifiedProperties();

            RouteSettingDrawer.DrawRouteSetting(routeSettingProp, routeSplineEditor);
        }

        private bool gameObjectNodeCondition(GameObjectNode_Select gameObjectNodeToggle)
        {
            return gameObjectNodeToggle.path == instantiateTransPathProp.stringValue;
        }

        private void gameObjectNodeOnSelectChanged(GameObjectNode_Select gameObjectNodeToggle, bool res)
        {
            if (!res)
                return;
            instantiateTransPathProp.stringValue = gameObjectNodeToggle.path;
        }

        private Vector3 SplineTool_func_CalcInverseOffset(Vector3 offset)
        {
            if (splineDrawInstance == null)
                return offset;
            offset = Quaternion.Inverse(splineDrawInstance.routeRotation) * PlayableUtils.DivisionVector(offset, splineDrawInstance.routeScale);
            return splineDrawInstance.TransformWorldToObjectVector(offset);
        }

        private Vector3 SplineTool_func_CalcOffset(Vector3 offset)
        {
            if (splineDrawInstance == null)
                return offset;
            offset = PlayableUtils.MultiplyVector(offset, splineDrawInstance.routeScale);
            return splineDrawInstance.routeRotation * splineDrawInstance.TransformObjectToWorldVector(offset);
        }

        private Vector3 SplineTool_func_TransformDirection(Vector3 dir)
        {
            if (splineDrawInstance == null)
                return dir;
            dir = PlayableUtils.MultiplyVector(dir, splineDrawInstance.routeScale);
            return splineDrawInstance.routeRotation * splineDrawInstance.TransformObjectToWorldDirection(dir.normalized);
        }

        private Vector3 SplineTool_func_InverseTransformPoint(Vector3 pos)
        {
            if (splineDrawInstance == null)
                return pos;
            Vector3 positionOS = splineDrawInstance.TransformWorldToObject(pos);
            Vector3 offset = splineDrawInstance.TransformObjectToWorldVector(positionOS);
            offset = Quaternion.Inverse(splineDrawInstance.routeRotation) * offset;
            offset = PlayableUtils.DivisionVector(offset, splineDrawInstance.routeScale);
            return splineDrawInstance.TransformWorldToObjectVector(offset) - self.transformSetting.position;
        }

        private Vector3 SplineTool_func_CalcPositionWS(Vector3 pos)
        {
            if (splineDrawInstance == null)
                return pos;
            pos = PlayableUtils.MultiplyVector(pos, splineDrawInstance.routeScale);
            pos = splineDrawInstance.routeRotation * splineDrawInstance.TransformObjectToWorldVector(pos);
            return pos + splineDrawInstance.TransformObjectToWorld(self.transformSetting.position);
        }
    }
}