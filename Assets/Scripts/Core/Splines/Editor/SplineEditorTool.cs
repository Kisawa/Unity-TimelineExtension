using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Splines
{
    [Serializable]
    public class SplineEditorTool
    {
        static GUIStyle Style_EditorHeaderText => GUI.skin.GetStyle("HeaderLabel");
        static SplineDataCache cache;

        const float handleSize = 0.04f;
        const float pickSize = 0.06f;
        const float tangentLinkWidth = 3;
        const float lineWidth = 10;
        const float directionWidth = 3;

        Editor core;
        SerializedObject serializedObject;
        SplineData data;

        public SerializedProperty pointsProp { get; private set; }
        public SerializedProperty pointsExtraProp { get; private set; }
        public SerializedProperty typeProp { get; private set; }
        public SerializedProperty EuclideanLengthProp { get; private set; }

        ReorderableList pointsList;

        [SerializeField] int selectedPointIndex = -1;
        [SerializeField] int selectedForwardTangentPointIndex = -1;
        [SerializeField] int selectedBackTangentPointIndex = -1;

        [SerializeField] int previewPoint = 0;
        [SerializeField] float previewLength = .5f;

        public event Func<Vector3> func_GetScale;
        public event Func<Vector3, Vector3> func_InverseTransformPoint;
        public event Func<Vector3, Vector3> func_TransformDirection;
        public event Func<Vector3, Vector3> func_CalcOffset;
        public event Func<Vector3, Vector3> func_CalcInverseOffset;
        public event Func<Vector3, Vector3> func_CalcPositionWS;

        public SplineEditorTool(Editor core, SerializedObject serializedObject, SerializedProperty dataProp, SplineData data)
        {
            this.core = core;
            this.serializedObject = serializedObject;
            this.data = data;
            pointsProp = dataProp.FindPropertyRelative("points");
            pointsExtraProp = dataProp.FindPropertyRelative("editorPointsExtra");

            typeProp = dataProp.FindPropertyRelative("type");
            EuclideanLengthProp = dataProp.FindPropertyRelative("euclideanLength");

            pointsList = new ReorderableList(serializedObject, pointsProp, false, true, true, true);
            pointsList.drawHeaderCallback = drawHeaderCallback;
            pointsList.drawElementCallback = drawElementCallback;
            pointsList.onAddCallback = onAddCallback;
            pointsList.onRemoveCallback = onRemoveCallback;
            pointsList.elementHeightCallback = elementHeightCallback;
            pointsList.onSelectCallback = onSelectCallback;
        }

        public void OnInspectorGUI(bool refresh)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProp);
            if (EditorGUI.EndChangeCheck())
                refresh = true;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(EuclideanLengthProp);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            pointsList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
                refresh = true;
            serializedObject.ApplyModifiedProperties();

            if (refresh)
            {
                serializedObject.Update();
                EuclideanLengthProp.floatValue = SplineUtil.CalcEuclideanLength(data);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void DrawEditorPreviewSetting(bool preview = true, string label = "")
        {
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            if(!string.IsNullOrEmpty(label))
                EditorGUILayout.LabelField(label, Style_EditorHeaderText);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(pointsProp.arraySize == 0);
            if (GUILayout.Button("Copy"))
                CopyBuffer();
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(cache == null);
            if (GUILayout.Button("Paste"))
                PasteBuffer();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            if (preview)
            {
                if (!string.IsNullOrEmpty(label))
                    EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                int _previewPoint = EditorGUILayout.IntField("Preview Point", previewPoint);
                float _previewLength = EditorGUILayout.Slider(previewLength, .1f, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(core, "SplinesEditor: Preview point changed");
                    previewPoint = Mathf.Clamp(_previewPoint, 0, 200);
                    previewLength = _previewLength;
                    SceneView.RepaintAll();
                }
                if (!string.IsNullOrEmpty(label))
                    EditorGUI.indentLevel--;
            }
        }

        public void OnSceneGUI(Color colorTint, bool pointHandle = true, bool tangentPointHandle = true)
        {
            serializedObject.Update();
            bool refresh = false;
            Vector3 last = Vector3.zero;
            Vector3 lastForwardTangent = Vector3.zero;
            for (int i = 0; i < data.points.Count; i++)
            {
                SerializedProperty prop = pointsProp.GetArrayElementAtIndex(i);
                SerializedProperty extraProp = pointsExtraProp.GetArrayElementAtIndex(i);
                Handles.color = selectedPointIndex == i || selectedForwardTangentPointIndex == i || selectedBackTangentPointIndex == i ? Color.red * colorTint : Color.white * colorTint;
                EditorGUI.BeginDisabledGroup(!pointHandle);
                Vector3 positionWS = DrawPoint(prop, extraProp, i, ref refresh);
                EditorGUI.EndDisabledGroup();
                Handles.color = selectedPointIndex == i || selectedForwardTangentPointIndex == i || selectedBackTangentPointIndex == i ? new Color(1, 0, 0, .5f) * colorTint : new Color(1, 1, 1, .25f) * colorTint;
                EditorGUI.BeginDisabledGroup(!tangentPointHandle);
                Vector3 forwardTangentPositionWS = DrawForwardTangentPoint(prop, extraProp, i, positionWS, ref refresh);
                Vector3 backTangentPositionWS = DrawBackTangentPoint(prop, extraProp, i, positionWS, ref refresh);
                EditorGUI.EndDisabledGroup();
                if (i > 0)
                {
                    float size = Mathf.Clamp(HandleUtility.GetHandleSize(last + (positionWS - last) * .5f), .3f, .5f);
                    Handles.DrawBezier(last, positionWS, lastForwardTangent, backTangentPositionWS, Color.white * colorTint, null, lineWidth * size);
                }
                last = positionWS;
                lastForwardTangent = forwardTangentPositionWS;
            }
            if (pointsProp.arraySize > 1 && typeProp.enumValueIndex == 1)
            {
                SerializedProperty prop = pointsProp.GetArrayElementAtIndex(0);
                SerializedProperty pointProp = prop.FindPropertyRelative("point");
                SerializedProperty backTangentProp = prop.FindPropertyRelative("backTangentOffset");
                Vector3 positionWS = CalcPositionWS(pointProp.vector3Value);
                float size = Mathf.Clamp(HandleUtility.GetHandleSize(last + (positionWS - last) * .5f), .3f, .5f);
                Handles.DrawBezier(last, positionWS, lastForwardTangent, positionWS + CalcOffset(backTangentProp.vector3Value), Color.white * colorTint, null, lineWidth * size);
            }
            for (int i = 0; i < previewPoint; i++)
            {
                float rate = 1f / previewPoint * i;
                Vector3 point = CalcPositionWS(SplineUtil.GetPoint(data, rate));
                Vector3 dir = TransformDirection(SplineUtil.GetDirection(data, rate));
                Handles.color = new Color(0, 1, 0, .5f) * colorTint;
                float size = Mathf.Clamp(HandleUtility.GetHandleSize(point), .3f, .5f);
                Handles.DrawLine(point, point + dir * previewLength, directionWidth * size);
                Handles.color = Color.blue * colorTint;
                Handles.DrawWireCube(point, GetScale() * .025f * size);
            }
            serializedObject.ApplyModifiedProperties();

            if (refresh)
            {
                serializedObject.Update();
                EuclideanLengthProp.floatValue = SplineUtil.CalcEuclideanLength(data);
                serializedObject.ApplyModifiedProperties();
                core.Repaint();
            }
        }

        Vector3 DrawPoint(SerializedProperty prop, SerializedProperty extraProp, int index, ref bool refresh)
        {
            if (index < 0 || index >= data.points.Count)
                return CalcPositionWS(Vector3.zero);
            SerializedProperty pointProp = prop.FindPropertyRelative("point");
            SerializedProperty rotationProp = extraProp.FindPropertyRelative("rotation");
            Vector3 positionWS = CalcPositionWS(pointProp.vector3Value);
            Quaternion rotation = rotationProp.quaternionValue;

            float size = HandleUtility.GetHandleSize(positionWS);
            if (Handles.Button(positionWS, rotation, handleSize * size, pickSize * size, Handles.DotHandleCap))
            {
                Undo.RecordObject(core, "SplinesEditor: Select changed");
                selectedPointIndex = index;
                selectedForwardTangentPointIndex = -1;
                selectedBackTangentPointIndex = -1;
                pointsList.index = index;
                core.Repaint();
            }

            if (selectedPointIndex == index)
            {
                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    positionWS = Handles.DoPositionHandle(positionWS, Tools.pivotRotation == PivotRotation.Local ? rotation : Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        pointProp.vector3Value = InverseTransformPoint(positionWS);
                        refresh = true;
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    EditorGUI.BeginChangeCheck();
                    rotation = Handles.DoRotationHandle(rotation, positionWS);
                    if (EditorGUI.EndChangeCheck())
                        rotationProp.quaternionValue = rotation;
                }
            }
            return positionWS;
        }

        Vector3 DrawForwardTangentPoint(SerializedProperty prop, SerializedProperty extraProp, int index, Vector3 origin, ref bool refresh)
        {
            if (index < 0 || index >= data.points.Count)
                return CalcPositionWS(Vector3.zero);
            SerializedProperty forwardTangentPointProp = prop.FindPropertyRelative("forwardTangentOffset");
            SerializedProperty backTangentPointProp = prop.FindPropertyRelative("backTangentOffset");
            SerializedProperty tangentRotationProp = extraProp.FindPropertyRelative("forwardTangentRotation");
            SerializedProperty handleTypeProp = extraProp.FindPropertyRelative("tangentHandleType");
            Vector3 positionWS = origin + CalcOffset(forwardTangentPointProp.vector3Value);
            Quaternion rotation = tangentRotationProp.quaternionValue;

            float size = Mathf.Clamp(HandleUtility.GetHandleSize(positionWS), .3f, .5f);
            if (Handles.Button(positionWS, rotation, handleSize * size, pickSize * size, Handles.DotHandleCap))
            {
                Undo.RecordObject(core, "SplinesEditor: Select changed");
                selectedPointIndex = -1;
                selectedForwardTangentPointIndex = index;
                selectedBackTangentPointIndex = -1;
                pointsList.index = index;
                core.Repaint();
            }
            Handles.DrawLine(origin, positionWS, size * tangentLinkWidth);

            if (selectedForwardTangentPointIndex == index)
            {
                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    positionWS = Handles.DoPositionHandle(positionWS, Tools.pivotRotation == PivotRotation.Local ? rotation : Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 offset = CalcInverseOffset(positionWS - origin);
                        forwardTangentPointProp.vector3Value = offset;
                        if (handleTypeProp.enumValueIndex == 1)
                            backTangentPointProp.vector3Value = -offset.normalized * backTangentPointProp.vector3Value.magnitude;
                        else if (handleTypeProp.enumValueIndex == 2)
                            backTangentPointProp.vector3Value = -offset;
                        refresh = true;
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    EditorGUI.BeginChangeCheck();
                    rotation = Handles.DoRotationHandle(rotation, positionWS);
                    if (EditorGUI.EndChangeCheck())
                        tangentRotationProp.quaternionValue = rotation;
                }
            }
            return positionWS;
        }

        Vector3 DrawBackTangentPoint(SerializedProperty prop, SerializedProperty extraProp, int index, Vector3 origin, ref bool refresh)
        {
            if (index < 0 || index >= data.points.Count)
                return CalcPositionWS(Vector3.zero);
            SerializedProperty forwardTangentPointProp = prop.FindPropertyRelative("forwardTangentOffset");
            SerializedProperty backTangentPointProp = prop.FindPropertyRelative("backTangentOffset");
            SerializedProperty tangentRotationProp = extraProp.FindPropertyRelative("backTangentRotation");
            SerializedProperty handleTypeProp = extraProp.FindPropertyRelative("tangentHandleType");
            Vector3 positionWS = origin + CalcOffset(backTangentPointProp.vector3Value);
            Quaternion rotation = tangentRotationProp.quaternionValue;

            float size = Mathf.Clamp(HandleUtility.GetHandleSize(positionWS), .3f, .5f);
            if (Handles.Button(positionWS, rotation, handleSize * size, pickSize * size, Handles.DotHandleCap))
            {
                Undo.RecordObject(core, "SplinesEditor: Select changed");
                selectedPointIndex = -1;
                selectedForwardTangentPointIndex = -1;
                selectedBackTangentPointIndex = index;
                pointsList.index = index;
                core.Repaint();
            }
            Handles.DrawLine(origin, positionWS, size * tangentLinkWidth);

            if (selectedBackTangentPointIndex == index)
            {
                if (Tools.current == Tool.Move)
                {
                    EditorGUI.BeginChangeCheck();
                    positionWS = Handles.DoPositionHandle(positionWS, Tools.pivotRotation == PivotRotation.Local ? rotation : Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 offset = CalcInverseOffset(positionWS - origin);
                        backTangentPointProp.vector3Value = offset;
                        if (handleTypeProp.enumValueIndex == 1)
                            forwardTangentPointProp.vector3Value = -offset.normalized * forwardTangentPointProp.vector3Value.magnitude;
                        else if (handleTypeProp.enumValueIndex == 2)
                            forwardTangentPointProp.vector3Value = -offset;
                        refresh = true;
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    EditorGUI.BeginChangeCheck();
                    rotation = Handles.DoRotationHandle(rotation, positionWS);
                    if (EditorGUI.EndChangeCheck())
                        tangentRotationProp.quaternionValue = rotation;
                }
            }
            return positionWS;
        }

        float elementHeightCallback(int index)
        {
            return pointsList.elementHeight * 2;
        }

        void drawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Point List");
        }

        void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = pointsProp.GetArrayElementAtIndex(index);
            SerializedProperty extraProp = pointsExtraProp.GetArrayElementAtIndex(index);

            SerializedProperty pointProp = prop.FindPropertyRelative("point");
            rect.y += 2;
            Rect rect0 = rect;
            rect0.height *= .5f;
            pointProp.vector3Value = EditorGUI.Vector3Field(rect0, "", pointProp.vector3Value);

            Rect rect1 = rect0;
            rect1.x += 12;
            rect1.y += rect0.height - 2;
            rect1.height *= .9f;
            rect1.width *= .15f;
            if (GUI.Button(rect1, "Lookat"))
                SceneView.lastActiveSceneView.LookAt(CalcPositionWS(pointProp.vector3Value));

            Rect rect2 = rect1;
            rect2.x += rect1.width * 1.1f;
            rect2.width = rect1.width * 2f;
            if (GUI.Button(rect2, "Reset Rotation"))
            {
                SerializedProperty rotationProp = extraProp.FindPropertyRelative("rotation");
                rotationProp.quaternionValue = Quaternion.identity;
                SerializedProperty forwardTangentRotationProp = extraProp.FindPropertyRelative("forwardTangentRotation");
                forwardTangentRotationProp.quaternionValue = Quaternion.identity;
                SerializedProperty backTangentRotationProp = extraProp.FindPropertyRelative("backTangentRotation");
                backTangentRotationProp.quaternionValue = Quaternion.identity;
            }

            Rect rect3 = rect2;
            rect3.x += rect2.width + rect1.width * .1f;
            rect3.y += 1;
            rect3.width = rect1.width * 1.1f;
            SerializedProperty tangentHandleTypeProp = extraProp.FindPropertyRelative("tangentHandleType");
            EditorGUI.BeginChangeCheck();
            int tangentHandleTypeIndex = EditorGUI.Popup(rect3, tangentHandleTypeProp.enumValueIndex, new string[] { "Free", "Aligned", "Mirror" });
            if (EditorGUI.EndChangeCheck())
            {
                tangentHandleTypeProp.enumValueIndex = tangentHandleTypeIndex;
                SerializedProperty forwardTangentOffsetProp = prop.FindPropertyRelative("forwardTangentOffset");
                SerializedProperty backTangentOffsetProp = prop.FindPropertyRelative("backTangentOffset");
                if (tangentHandleTypeIndex == 1)
                    backTangentOffsetProp.vector3Value = -forwardTangentOffsetProp.vector3Value.normalized * backTangentOffsetProp.vector3Value.magnitude;
                else if (tangentHandleTypeIndex == 2)
                    backTangentOffsetProp.vector3Value = -forwardTangentOffsetProp.vector3Value;
            }

            Rect rect4 = rect3;
            rect4.y -= 1;
            rect4.x += rect3.width + rect1.width * .1f;
            rect4.width = rect1.width * 2f;
            if (GUI.Button(rect4, "Linear Tangent"))
            {
                SerializedProperty forwardTangentOffsetProp = prop.FindPropertyRelative("forwardTangentOffset");
                SerializedProperty backTangentOffsetProp = prop.FindPropertyRelative("backTangentOffset");
                SerializedProperty forwardTangentRotationProp = extraProp.FindPropertyRelative("forwardTangentRotation");
                SerializedProperty backTangentRotationProp = extraProp.FindPropertyRelative("backTangentRotation");
                if (pointsProp.arraySize == 1)
                {
                    SerializedProperty rotationProp = extraProp.FindPropertyRelative("rotation");
                    forwardTangentOffsetProp.vector3Value = (Tools.pivotRotation == PivotRotation.Local ? rotationProp.quaternionValue * Vector3.right : Vector3.right) * .25f;
                    backTangentOffsetProp.vector3Value = -(Tools.pivotRotation == PivotRotation.Local ? rotationProp.quaternionValue * Vector3.right : Vector3.right) * .25f;
                }
                else
                {
                    SerializedProperty forwardPointProp = null;
                    if (index + 1 < pointsProp.arraySize)
                        forwardPointProp = pointsProp.GetArrayElementAtIndex(index + 1);
                    else if (index + 1 == pointsProp.arraySize && typeProp.enumValueIndex == 1)
                        forwardPointProp = pointsProp.GetArrayElementAtIndex(0);
                    if (forwardPointProp != null)
                    {
                        SerializedProperty forwardPoint = forwardPointProp.FindPropertyRelative("point");
                        Vector3 dir = forwardPoint.vector3Value - pointProp.vector3Value;
                        forwardTangentOffsetProp.vector3Value = dir * .25f;
                        forwardTangentRotationProp.quaternionValue = Quaternion.FromToRotation(Vector3.right, dir.normalized);
                    }

                    SerializedProperty backPointProp = null;
                    if (index - 1 >= 0)
                        backPointProp = pointsProp.GetArrayElementAtIndex(index - 1);
                    else if (index == 0 && typeProp.enumValueIndex == 1)
                        backPointProp = pointsProp.GetArrayElementAtIndex(pointsProp.arraySize - 1);
                    if (backPointProp != null)
                    {
                        SerializedProperty backPoint = backPointProp.FindPropertyRelative("point");
                        Vector3 dir = backPoint.vector3Value - pointProp.vector3Value;
                        backTangentOffsetProp.vector3Value = dir * .25f;
                        backTangentRotationProp.quaternionValue = Quaternion.FromToRotation(Vector3.right, dir.normalized);
                    }
                }
            }
        }

        void onAddCallback(ReorderableList list)
        {
            pointsProp.InsertArrayElementAtIndex(pointsProp.arraySize);
            pointsExtraProp.InsertArrayElementAtIndex(pointsExtraProp.arraySize);
            SerializedProperty prop = pointsProp.GetArrayElementAtIndex(pointsProp.arraySize - 1);
            SerializedProperty extraProp = pointsExtraProp.GetArrayElementAtIndex(pointsExtraProp.arraySize - 1);

            SerializedProperty pointProp = prop.FindPropertyRelative("point");
            SerializedProperty rotationProp = extraProp.FindPropertyRelative("rotation");
            if (pointsProp.arraySize == 1)
                rotationProp.quaternionValue = Quaternion.identity;
            else
            {
                SerializedProperty lastPointExtraProp = pointsExtraProp.GetArrayElementAtIndex(pointsExtraProp.arraySize - 2);
                SerializedProperty lastRotationProp = lastPointExtraProp.FindPropertyRelative("rotation");
                rotationProp.quaternionValue = lastRotationProp.quaternionValue;
            }
            pointProp.vector3Value += Tools.pivotRotation == PivotRotation.Local ? rotationProp.quaternionValue * Vector3.right : Vector3.right;

            SerializedProperty forwardTangentProp = prop.FindPropertyRelative("forwardTangentOffset");
            forwardTangentProp.vector3Value = (Tools.pivotRotation == PivotRotation.Local ? rotationProp.quaternionValue * Vector3.right : Vector3.right) * .25f;
            SerializedProperty backTangentProp = prop.FindPropertyRelative("backTangentOffset");
            backTangentProp.vector3Value = -(Tools.pivotRotation == PivotRotation.Local ? rotationProp.quaternionValue * Vector3.right : Vector3.right) * .25f;

            SerializedProperty forwardTangentRotationProp = extraProp.FindPropertyRelative("forwardTangentRotation");
            forwardTangentRotationProp.quaternionValue = rotationProp.quaternionValue;
            SerializedProperty backTangentRotationProp = extraProp.FindPropertyRelative("backTangentRotation");
            backTangentRotationProp.quaternionValue = rotationProp.quaternionValue * Quaternion.Euler(0, 180, 0);
        }

        void onRemoveCallback(ReorderableList list)
        {
            int index = list.index;
            pointsProp.DeleteArrayElementAtIndex(index);
            pointsExtraProp.DeleteArrayElementAtIndex(index);
        }

        void onSelectCallback(ReorderableList list)
        {
            selectedPointIndex = list.index;
            selectedForwardTangentPointIndex = list.index;
            selectedBackTangentPointIndex = list.index;
        }

        public void ClearData()
        {
            EuclideanLengthProp.floatValue = 0;
            pointsProp.ClearArray();
            pointsExtraProp.ClearArray();
        }

        Vector3 GetScale()
        {
            Vector3 scale = Vector3.one;
            if (func_GetScale != null)
                scale = func_GetScale.Invoke();
            return scale;
        }

        Vector3 InverseTransformPoint(Vector3 position)
        {
            if (func_InverseTransformPoint != null)
                position = func_InverseTransformPoint.Invoke(position);
            return position;
        }

        Vector3 TransformDirection(Vector3 dir)
        {
            if (func_TransformDirection != null)
                dir = func_TransformDirection.Invoke(dir);
            return dir;
        }

        Vector3 CalcOffset(Vector3 offset)
        {
            if (func_CalcOffset != null)
                offset = func_CalcOffset.Invoke(offset);
            return offset;
        }

        Vector3 CalcInverseOffset(Vector3 offset)
        {
            if (func_CalcInverseOffset != null)
                offset = func_CalcInverseOffset.Invoke(offset);
            return offset;
        }

        Vector3 CalcPositionWS(Vector3 point)
        {
            if (func_CalcPositionWS != null)
                point = func_CalcPositionWS.Invoke(point);
            return point;
        }

        void CopyBuffer()
        {
            cache = new SplineDataCache();
            cache.typeEnumValueIndex = typeProp.enumValueIndex;
            cache.euclideanLength = EuclideanLengthProp.floatValue;
            for (int i = 0; i < pointsProp.arraySize; i++)
            {
                Point point = new Point();
                EditorPointExtra extraPoint = new EditorPointExtra();
                SerializedProperty prop = pointsProp.GetArrayElementAtIndex(i);
                SerializedProperty pointProp = prop.FindPropertyRelative("point");
                SerializedProperty forwardTangentOffsetProp = prop.FindPropertyRelative("forwardTangentOffset");
                SerializedProperty backTangentOffsetProp = prop.FindPropertyRelative("backTangentOffset");
                point.point = pointProp.vector3Value;
                point.forwardTangentOffset = forwardTangentOffsetProp.vector3Value;
                point.backTangentOffset = backTangentOffsetProp.vector3Value;
                SerializedProperty extraProp = pointsExtraProp.GetArrayElementAtIndex(i);
                SerializedProperty extraTangentHandleTypeProp = extraProp.FindPropertyRelative("tangentHandleType");
                SerializedProperty extraRotationProp = extraProp.FindPropertyRelative("rotation");
                SerializedProperty extraForwardTangentRotationProp = extraProp.FindPropertyRelative("forwardTangentRotation");
                SerializedProperty extraBackTangentRotationProp = extraProp.FindPropertyRelative("backTangentRotation");
                extraPoint.tangentHandleType = Enum.Parse<TangentHandleType>(extraTangentHandleTypeProp.enumNames[extraTangentHandleTypeProp.enumValueIndex]);
                extraPoint.rotation = extraRotationProp.quaternionValue;
                extraPoint.forwardTangentRotation = extraForwardTangentRotationProp.quaternionValue;
                extraPoint.backTangentRotation = extraBackTangentRotationProp.quaternionValue;
                cache.points.Add(point);
                cache.editorPointsExtra.Add(extraPoint);
            }
        }

        void PasteBuffer()
        {
            if (cache == null)
                return;
            typeProp.enumValueIndex = cache.typeEnumValueIndex;
            EuclideanLengthProp.floatValue = cache.euclideanLength;
            pointsProp.ClearArray();
            pointsExtraProp.ClearArray();
            for (int i = 0; i < cache.points.Count; i++)
            {
                Point point = cache.points[i];
                EditorPointExtra extraPoint = cache.editorPointsExtra[i];
                pointsProp.InsertArrayElementAtIndex(pointsProp.arraySize);
                SerializedProperty prop = pointsProp.GetArrayElementAtIndex(pointsProp.arraySize - 1);
                prop.FindPropertyRelative("point").vector3Value = point.point;
                prop.FindPropertyRelative("forwardTangentOffset").vector3Value = point.forwardTangentOffset;
                prop.FindPropertyRelative("backTangentOffset").vector3Value = point.backTangentOffset;
                pointsExtraProp.InsertArrayElementAtIndex(pointsExtraProp.arraySize);
                SerializedProperty extraProp = pointsExtraProp.GetArrayElementAtIndex(pointsExtraProp.arraySize - 1);
                extraProp.FindPropertyRelative("tangentHandleType").enumValueIndex = Array.IndexOf(Enum.GetNames(typeof(TangentHandleType)), extraPoint.tangentHandleType.ToString());
                extraProp.FindPropertyRelative("rotation").quaternionValue = extraPoint.rotation;
                extraProp.FindPropertyRelative("forwardTangentRotation").quaternionValue = extraPoint.forwardTangentRotation;
                extraProp.FindPropertyRelative("backTangentRotation").quaternionValue = extraPoint.backTangentRotation;
            }
        }

        class SplineDataCache
        {
            public int typeEnumValueIndex;
            public float euclideanLength;
            public List<Point> points = new List<Point>();
            public List<EditorPointExtra> editorPointsExtra = new List<EditorPointExtra>();
        }
    }
}