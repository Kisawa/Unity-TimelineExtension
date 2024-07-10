using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameObjectNode
{
    [Serializable]
    public class GameObjectNode_Select : GameObjectNodeBase
    {
        Func<GameObjectNode_Select, bool> condition;
        Action<GameObjectNode_Select, bool> onSelectChanged;

        public GameObjectNode_Select(Transform trans, string path, Func<GameObjectNode_Select, bool> condition, Action<GameObjectNode_Select, bool> onSelectChanged) : base(null, "")
        {
            if (trans == null)
                return;
            Self = trans;
            Children = new List<GameObjectNodeBase>();
            this.path = path;
            this.condition = condition;
            this.onSelectChanged = onSelectChanged;
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform _trans = trans.GetChild(i);
                if (!CheckGameObjectAvailable(_trans))
                    continue;
                string _path = string.IsNullOrEmpty(path) ? _trans.name : $"{path}/{_trans.name}";
                GameObjectNode_Select node = new GameObjectNode_Select(_trans, _path, condition, onSelectChanged);
                node.parent = this;
                Children.Add(node);
            }
        }

        public override void Draw(float space, int jump = 0)
        {
            BeginFoldout();
            GUILayout.Space(space * jump);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(Self, typeof(Transform), true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginChangeCheck();
            bool _toggle = EditorGUILayout.Toggle(CheckSelected(), GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
                onSelectChanged?.Invoke(this, _toggle);
            EndFoldout();
        }

        public override void DrawChildren(float space, int jump = 0)
        {
            if (Self == null)
                return;
            for (int i = 0; i < Children.Count; i++)
            {
                GameObjectNode_Select node = Children[i] as GameObjectNode_Select;
                if (node.CheckSelected() || node.CheckUpwardUnfold())
                    node.Draw(space, jump);
                node.DrawChildren(space, jump + 1);
            }
        }

        public override void Draw(float space)
        {
            if (Self == null)
                return;
            Draw(space, 0);
            DrawChildren(space, 1);
        }

        bool CheckUpwardUnfold()
        {
            if (parent == null)
                return true;
            GameObjectNode_Select toggleParent = parent as GameObjectNode_Select;
            if (toggleParent.unfold)
                return (toggleParent.CheckSelected() && toggleParent.unfold) || toggleParent.CheckUpwardUnfold();
            else
                return false;
        }

        bool CheckSelected()
        {
            return condition == null ? false : condition.Invoke(this);
        }
    }
}