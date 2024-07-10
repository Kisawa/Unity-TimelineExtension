using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameObjectNode
{
    [Serializable]
    public class GameObjectNodeBase
    {
        public Object core { get; set; }
        public Transform Self { get; protected set; }
        public GameObjectNodeBase parent { get; protected set; }
        public List<GameObjectNodeBase> Children { get; protected set; }
        public string path { get; protected set; }
        public bool unfold { get; protected set; }

        public GameObjectNodeBase(Transform trans, string path)
        {
            if (trans == null)
                return;
            Self = trans;
            Children = new List<GameObjectNodeBase>();
            this.path = path;
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform _trans = trans.GetChild(i);
                if (!CheckGameObjectAvailable(_trans))
                    continue;
                GameObjectNodeBase node = new GameObjectNodeBase(_trans, $"{path}/{_trans.name}");
                node.parent = this;
                Children.Add(node);
            }
        }

        public void BeginFoldout()
        {
            GUIContent foldout_off = EditorGUIUtility.IconContent("d_IN_foldout_act");
            GUIContent foldout_on = EditorGUIUtility.IconContent("d_IN_foldout_act_on");
            EditorGUILayout.BeginHorizontal();
            if (Children != null && Children.Count > 0)
            {
                if (GUILayout.Button(unfold ? foldout_on : foldout_off, "ObjectPickerTab", GUILayout.Width(20)))
                    unfold = !unfold;
            }
            else
                GUILayout.Space(23);
        }

        public void EndFoldout()
        {
            EditorGUILayout.EndHorizontal();
        }

        public virtual void Draw(float space, int jump = 0)
        {
            BeginFoldout();
            GUILayout.Space(space * jump);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(Self, typeof(Transform), true);
            EditorGUI.EndDisabledGroup();
            EndFoldout();
        }

        public virtual void DrawChildren(float space, int jump = 0)
        {
            if (Self == null)
                return;
            for (int i = 0; i < Children.Count; i++)
            {
                GameObjectNodeBase node = Children[i];
                node.Draw(space, jump);
                if (node.unfold)
                    node.DrawChildren(space, jump + 1);
            }
        }

        public virtual void Draw(float space)
        {
            if (Self == null)
                return;
            Draw(space, 0);
            if (unfold)
                DrawChildren(space, 1);
        }

        protected void UndoRecordObject()
        {
            if (core == null)
                return;
            Undo.RecordObject(core, "GameObjectNode changed");
        }

        protected static bool CheckGameObjectAvailable(Transform trans)
        {
            return trans != null &&
                !trans.gameObject.hideFlags.HasFlag(HideFlags.DontSave) &&
                !trans.gameObject.hideFlags.HasFlag(HideFlags.DontSaveInBuild) &&
                !trans.gameObject.hideFlags.HasFlag(HideFlags.DontSaveInEditor) &&
                !trans.gameObject.hideFlags.HasFlag(HideFlags.HideAndDontSave) &&
                !trans.gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy);
        }
    }
}