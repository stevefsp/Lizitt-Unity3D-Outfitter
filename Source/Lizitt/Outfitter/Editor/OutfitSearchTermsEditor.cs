/*
 * Copyright (c) 2015 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using com.lizitt.u3d.editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.lizitt.outfitter.editor
{
    [CustomEditor(typeof(OutfitSearchTerms))]
    [CanEditMultipleObjects]
    public class OutfitSearchTermsEditor
        : Editor
    {
        #region Menu Items

        private const string MenuItemName = "Outfit Search Terms";
        private const int MenuPriority = EditorUtil.AssetGroup + OutfitterEditor.MenuPriority + 2;

        [MenuItem(EditorUtil.AssetCreateMenu + MenuItemName, false, MenuPriority)]
        [MenuItem(EditorUtil.UnityAssetCreateMenu + MenuItemName, false, MenuPriority)]
        private static void CreateAsset()
        {
            var item = EditorUtil.CreateAsset<OutfitSearchTerms>("", OutfitterEditor.AssetLabel);
            Selection.activeObject = item;
        }

        #endregion

        private const string MountListName = "m_MountTerms";
        private const string ItemTypeName = "m_MountType";
        private const string ItemTermName = "m_Term";

        private struct Props
        {
            public SerializedProperty headTerm;
            public SerializedProperty eyeTerm;
            public SerializedProperty bodyTerm;
            public SerializedProperty blendTerm;

            public Props(SerializedObject sobj)
            {
                headTerm = sobj.FindProperty("m_HeadTerm");
                eyeTerm = sobj.FindProperty("m_EyeTerm");
                bodyTerm = sobj.FindProperty("m_BodyTerm");
                blendTerm = sobj.FindProperty("m_BlendHeadTerm");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var props = new Props(serializedObject);

            EditorGUILayout.LabelField("Material Terms", EditorGUIUtil.BoldLabel);

            EditorGUILayout.PropertyField(props.headTerm);
            EditorGUILayout.PropertyField(props.eyeTerm);
            EditorGUILayout.PropertyField(props.bodyTerm);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Renderer Terms", EditorGUIUtil.BoldLabel);

            EditorGUILayout.PropertyField(props.blendTerm);

            DrawMountPointTerms();

            if (UnityEngine.GUI.changed)
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox("Terms are used to perform partial, case-insenstive matches of"
                + " object names in an outfit prototype.  The type of object is dependant on the"
                + " term.", 
                MessageType.Info, true);
        }

        private ReorderableList m_List;

        private void DrawMountPointTerms()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Mount Points", EditorGUIUtil.BoldLabel);

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Can't edit mount point terms while in multi-edit mode.",
                    MessageType.Warning, true);
                return;
            }

            if (m_List == null)
                CreateReorderableList();

            m_List.DoLayoutList();
        }

        private void CreateReorderableList()
        {
            var list = new ReorderableList(serializedObject
                , serializedObject.FindProperty(MountListName)
                , true, true, true, true);

            list.drawHeaderCallback = delegate(Rect rect)
            {
                EditorGUI.LabelField(rect, "Mount Point Terms");
            };

            list.drawElementCallback =
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    var typProp = element.FindPropertyRelative(ItemTypeName);
                    var termProp = element.FindPropertyRelative(ItemTermName);

                    GUIContent label;

                    bool hasError = false;
                    if (typProp.enumValueIndex == -1)
                    {
                        label = new GUIContent("Invalid Type", "Enum type value changed or removed?");
                        hasError = true;
                    }
                    else
                        label = new GUIContent(typProp.enumDisplayNames[typProp.enumValueIndex]);

                    var rect = new Rect(position.x, 
                        position.y + EditorGUIUtility.standardVerticalSpacing,
                        90, EditorGUIUtility.singleLineHeight);

                    GUIStyle style = GUI.skin.label;

                    if (hasError)
                        style = EditorGUIUtil.RedLabel;

                    EditorGUI.LabelField(rect, label, style);

                    rect = new Rect(
                        rect.xMax + 5, rect.y, position.width - rect.width - 5, rect.height);

                    EditorGUI.PropertyField(rect, termProp, GUIContent.none);
                };

            GenericMenu.MenuFunction2 addItem = delegate(object typValue)
            {
                list.index = AddItem(list.serializedProperty, (int)typValue);
            };

            GenericMenu.MenuFunction addAllItems = delegate()
            {
                var stdNames = OutfitterEditorUtil.MountPointTypeNames;
                var stdValues = OutfitterEditorUtil.MountPointTypeValues;

                var idx = list.serializedProperty.arraySize;

                for (int i = 0; i < stdNames.Length; i++)
                {
                    if (!IsInList(list.serializedProperty, stdValues[i]))
                        AddItem(list.serializedProperty, stdValues[i]);
                }

                list.index = idx;  // Assumes never called if list is full.
            };

            list.onAddDropdownCallback = delegate(Rect rect, ReorderableList roList)
            {
                var menu = new GenericMenu();

                var stdNames = OutfitterEditorUtil.MountPointTypeNames;
                var stdValues = OutfitterEditorUtil.MountPointTypeValues;

                for (int i = 0; i < stdNames.Length; i++)
                {
                    if (!IsInList(list.serializedProperty, stdValues[i]))
                        menu.AddItem(new GUIContent(stdNames[i]), false, addItem, stdValues[i]);
                }

                if (menu.GetItemCount() == 0)
                    menu.AddDisabledItem(new GUIContent("[None Available]"));
                else
                    menu.AddItem(new GUIContent("All"), false, addAllItems);

                menu.ShowAsContext();
            };

            m_List = list;
        }

        private int AddItem(SerializedProperty listProp, int typValue)
        {
            int nidx = listProp.arraySize;

            listProp.arraySize++;

            var element = listProp.GetArrayElementAtIndex(nidx);
            element.FindPropertyRelative(ItemTypeName).intValue = typValue;
            element.FindPropertyRelative(ItemTermName).stringValue = ((MountPointType)typValue).ToString();

            listProp.serializedObject.ApplyModifiedProperties();

            return nidx;
        }

        private bool IsInList(SerializedProperty listProp, int typValue)
        {
            // Hack.
            if (typValue == (int)MountPointType.Root)
                return true;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative(ItemTypeName).intValue == typValue)
                    return true;
            }

            return false;
        }
    }
}
