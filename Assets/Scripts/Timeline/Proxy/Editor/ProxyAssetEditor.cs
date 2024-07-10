using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CustomTimeline
{
    [CustomEditor(typeof(ProxyAsset))]
    public class ProxyAssetEditor : Editor
    {
        static ProxyData cache;

        ProxyAsset self;
        SerializedProperty proxiesProp;
        ReorderableList dataList;

        private void OnEnable()
        {
            self = serializedObject.targetObject as ProxyAsset;
            proxiesProp = serializedObject.FindProperty("proxies");
            dataList = new ReorderableList(serializedObject, proxiesProp, true, true, true, true);
            dataList.onAddDropdownCallback = AddDropdown;
            dataList.drawElementCallback = DrawElementCallback;
            dataList.elementHeightCallback = ElementHeightCallback;
            dataList.drawHeaderCallback = DrawHeaderCallback;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            dataList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawHeaderCallback(Rect position)
        {
            Rect rect = position;
            rect.x -= 5;
            rect.width = 100;
            rect.y += 1;
            EditorGUI.BeginDisabledGroup(cache == null);
            if (GUI.Button(rect, "Paste", "ToolbarSearchTextFieldJumpButton"))
            {
                Undo.RecordObject(self, "Proxy Asset: Copy Proxy");
                ProxyData data = self.proxies.FirstOrDefault(x => x.name == cache.name);
                if (data == null)
                {
                    data = new ProxyData();
                    data.name = cache.name;
                    self.proxies.Add(data);
                }
                data.proxy = cache.proxy.Copy();
                data.curve = new AnimationCurve(cache.curve.keys);
                EditorUtility.SetDirty(self);
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawElementCallback(Rect position, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prop = proxiesProp.GetArrayElementAtIndex(index);
            Rect rect = position;
            rect.y += 2;
            rect.x += 5;
            rect.width -= 5;
            EditorGUI.BeginChangeCheck();
            ProxyDataDrawer.Draw(rect, prop, 100, -10);
            if (EditorGUI.EndChangeCheck())
            {
                SerializedProperty nameProp = prop.FindPropertyRelative("name");
                string proxyName = nameProp.stringValue;
                if (!string.IsNullOrWhiteSpace(proxyName))
                {
                    for (int i = 0; i < self.proxies.Count; i++)
                    {
                        if (i == index)
                            continue;
                        string _proxyName = self.proxies[i].name;
                        if (proxyName == _proxyName)
                        {
                            nameProp.stringValue = "";
                            break;
                        }
                    }
                }
            }
            rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x = rect.width - 60;
            rect.width = 99;
            rect.y += 2.5f;
            if (GUI.Button(rect, "Copy", "RL FooterButton"))
                cache = self.proxies[index];
        }

        float ElementHeightCallback(int index)
        {
            SerializedProperty prop = proxiesProp.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(prop);
        }

        void AddDropdown(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Float"), false, () =>
            {
                Undo.RecordObject(self, "Proxy Asset: Add Proxy");
                ProxyData data = new ProxyData();
                data.proxy = new ProxyFloat();
                self.proxies.Add(data);
                EditorUtility.SetDirty(self);
            });
            menu.AddItem(new GUIContent("Vector"), false, () =>
            {
                Undo.RecordObject(self, "Proxy Asset: Add Proxy");
                ProxyData data = new ProxyData();
                data.proxy = new ProxyVector();
                self.proxies.Add(data);
                EditorUtility.SetDirty(self);
            });
            menu.AddItem(new GUIContent("Color"), false, () =>
            {
                Undo.RecordObject(self, "Proxy Asset: Add Proxy");
                ProxyData data = new ProxyData();
                data.proxy = new ProxyColor();
                self.proxies.Add(data);
                EditorUtility.SetDirty(self);
            });
            menu.AddItem(new GUIContent("Int"), false, () =>
            {
                Undo.RecordObject(self, "Proxy Asset: Add Proxy");
                ProxyData data = new ProxyData();
                data.proxy = new ProxyInt();
                self.proxies.Add(data);
                EditorUtility.SetDirty(self);
            });
            menu.DropDown(buttonRect);
        }
    }
}