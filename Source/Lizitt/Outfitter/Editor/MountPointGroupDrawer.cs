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
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using com.lizitt.u3d.editor;

namespace com.lizitt.outfitter.editor
{
    [CustomPropertyDrawer(typeof(MountPointGroup))]
    public class MountPointGroupDrawer
        : PropertyDrawer
    {
        private const string ItemPropName = "m_Items";
        private const string ItemTransformPropName = "m_Transform";
        private const string ItemTypePropName = "m_Type";

        private static readonly float HeaderHeight =
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private static readonly float FooterHeight =
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private static readonly float ElementHeight =
            EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 2)
            + 3;  // Improves the layout, especially for the final element, above the footer.

        private ReorderableList m_List;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float items = property.FindPropertyRelative(ItemPropName).arraySize;

            return HeaderHeight + FooterHeight + EditorGUIUtility.singleLineHeight
                + (ElementHeight * Mathf.Max(1, items));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_List == null)
                CreatePrototypeList(property);

            label = EditorGUI.BeginProperty(position, label, property);

            //m_List.DoLayoutList();  // Causes layout exceptions.
            m_List.DoList(position);

            EditorGUI.EndProperty();
        }

        private void CreatePrototypeList(SerializedProperty property)
        {
            var list = new ReorderableList(property.serializedObject
                , property.FindPropertyRelative(ItemPropName)
                , true, true, true, true);

            list.headerHeight = HeaderHeight;
            list.footerHeight = FooterHeight;

            list.drawHeaderCallback = delegate(Rect rect)
            {
                EditorGUI.LabelField(rect, "Mount Points");
            };

            list.elementHeight = ElementHeight;

            list.drawElementCallback =
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    var typProp = element.FindPropertyRelative(ItemTypePropName);
                    var transProp = element.FindPropertyRelative(ItemTransformPropName);

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

                    EditorGUI.PropertyField(rect, transProp, GUIContent.none);
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
            element.FindPropertyRelative(ItemTypePropName).intValue = typValue;

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
                if (element.FindPropertyRelative(ItemTypePropName).intValue == typValue)
                    return true;
            }

            return false;
        }
    }
}

