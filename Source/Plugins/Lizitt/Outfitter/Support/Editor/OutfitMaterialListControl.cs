/*
 * Copyright (c) 2016 Stephen A. Pratt
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
using com.lizitt.editor;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// Displays a user friendly <see cref="OutfitMaterialTargetGroup"/> list GUI control.
    /// </summary>
    public sealed class OutfitMaterialListControl
    {
        private static readonly float HeaderHeight =
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private static readonly float FooterHeight =
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private static readonly float EmptyElementHeight = EditorGUIUtility.singleLineHeight * 1.5f;

        private OutfitMaterialListInfo m_Settings;
        private ReorderableList m_List;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">The list settings.</param>
        public OutfitMaterialListControl(OutfitMaterialListInfo settings)
        {
            m_Settings = settings;

            if (string.IsNullOrEmpty(m_Settings.ListHeaderLabel))
                m_Settings.ListHeaderLabel = "Elements";
        }

        /// <summary>
        /// Gets the draw height of the list.
        /// </summary>
        /// <param name="listProperty">The list property. (Array of outfit materials.)</param>
        /// <param name="label">The </param>
        /// <returns></returns>
        public float GetPropertyHeight(SerializedProperty listProperty)
        {
            // As of Unity 5.2.3, the height of a re-orderable list must be no less than
            // (Header + Footer + OneElement).  Doing so will cause draw problems for
            // empty lists.

            float elementCount = listProperty.arraySize;

            var result = HeaderHeight + FooterHeight + EditorGUIUtility.singleLineHeight;

            return result + (elementCount == 0 
                ? EmptyElementHeight 
                : m_Settings.ElementHeight * elementCount);
        }

        /// <summary>
        /// Draw the GUI control.
        /// </summary>
        /// <param name="position">The draw position.</param>
        /// <param name="listProperty">The list property. (Array of outfit materials.)</param>
        /// <param name="label">The root property's label.</param>
        public void OnGUI(Rect position, SerializedProperty listProperty, GUIContent label)
        {
            if (m_List == null)
                CreateList(listProperty);

            label = EditorGUI.BeginProperty(position, label, listProperty);

            // See comments in GetPropertyHeight() for why this is necessary.
            m_List.elementHeight = listProperty.arraySize == 0
                ? EmptyElementHeight
                : m_Settings.ElementHeight;

            m_List.DoList(position);

            EditorGUI.EndProperty();
        }

        private void CreateList(SerializedProperty listProperty)
        {
            var list = new ReorderableList(listProperty.serializedObject, listProperty
                , true, true, true, true);

            list.headerHeight = HeaderHeight;
            list.footerHeight = FooterHeight;

            list.drawHeaderCallback = delegate(Rect rect)
            {
                EditorGUI.LabelField(rect, m_Settings.ListHeaderLabel);
            };

            list.elementHeight = m_Settings.ElementHeight;

            list.drawElementCallback =
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    var typProp = element.FindPropertyRelative(m_Settings.ItemTypePropName);
                    var dataProp = element.FindPropertyRelative(m_Settings.ItemDataPropName);

                    GUIContent label;
                    GUIStyle style = EditorStyles.label;

                    if (typProp.enumValueIndex == -1)
                    {
                        label = new GUIContent("Invalid Type", "Enum type value changed or removed?");
                        style = LizittEditorGUIUtil.RedLabel;
                    }
                    else
                    {
                        string vmessage = null;
                        var vcheck = m_Settings.ValidateElement == null 
                            ? PropertyValidationResult.Success 
                            : m_Settings.ValidateElement(dataProp, out vmessage);

                        label = new GUIContent(typProp.enumDisplayNames[typProp.enumValueIndex]);
                        if (!string.IsNullOrEmpty(vmessage))
                            label.tooltip = vmessage;

                        switch (vcheck)
                        {
                            case PropertyValidationResult.Warning:

                                style = LizittEditorGUIUtil.YellowLabel;
                                break;

                            case PropertyValidationResult.Error:

                                style = LizittEditorGUIUtil.RedLabel;
                                break;
                        }
                    }

                    var rect = new Rect(
                        position.x, position.y + EditorGUIUtility.standardVerticalSpacing,
                        position.width, position.height);

                    m_Settings.DrawElement(rect, dataProp, label, style);
                };

            GenericMenu.MenuFunction2 addItem = delegate(object typValue)
            {
                list.index = AddItem(list.serializedProperty, (int)typValue);
            };

            GenericMenu.MenuFunction addAllItems = delegate()
            {
                var stdNames = OutfitterEditorUtil.OutfitMaterialTypeNames;
                var stdValues = OutfitterEditorUtil.OutfitMaterialTypeTypeValues;

                var idx = list.serializedProperty.arraySize;

                for (int i = 0; i < stdNames.Length; i++)
                {
                    if (!IsInList(list.serializedProperty, stdValues[i]))
                        AddItem(list.serializedProperty, stdValues[i]);
                }

                list.index = idx;  // Assumes function is never called if list is full.
            };

            list.onAddDropdownCallback = delegate(Rect rect, ReorderableList roList)
            {
                var menu = new GenericMenu();

                var stdNames = OutfitterEditorUtil.OutfitMaterialTypeNames;
                var stdValues = OutfitterEditorUtil.OutfitMaterialTypeTypeValues;

                for (int i = 0; i < stdNames.Length; i++)
                {
                    if (m_Settings.AllowMultipleTargets || !IsInList(list.serializedProperty, stdValues[i]))
                        menu.AddItem(new GUIContent(stdNames[i]), false, addItem, stdValues[i]);
                }

                if (menu.GetItemCount() == 0)
                    menu.AddDisabledItem(new GUIContent("<None Available>"));
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
            element.FindPropertyRelative(m_Settings.ItemTypePropName).intValue = typValue;

            if (m_Settings.InitializeElement != null)
                m_Settings.InitializeElement(element.FindPropertyRelative(m_Settings.ItemDataPropName));

            listProp.serializedObject.ApplyModifiedProperties();

            return nidx;
        }

        private bool IsInList(SerializedProperty listProp, int typValue)
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative(m_Settings.ItemTypePropName).intValue == typValue)
                    return true;
            }

            return false;
        }
    }
}

