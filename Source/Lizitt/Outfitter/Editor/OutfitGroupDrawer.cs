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
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using com.lizitt.u3d.editor;

namespace com.lizitt.outfitter.editor
{
    [CustomPropertyDrawer(typeof(OutfitGroup))]
    public class OutfitGroupDrawer
        : PropertyDrawer
    {
        private const string ProtoPropName = "m_Prototypes";
        private const string DefaultPropName = "m_DefaultOutfit";
        private const string StartPropName = "m_StartOutfit";
        private const string ItemTypName = "typ";
        private const string ItemProtoName = "prototype";

        private readonly static GUIContent StartLabel = new GUIContent("Start Outfit",
            "The outfit type that should be applied on component start.");

        private readonly static GUIContent DefaultLabel = new GUIContent("Default Outfit",
            "The outfit type to use when the requested outfit type is not defined.");

        private ReorderableList m_PrototypeList;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Non-list fields.  List handles its own height.
            return 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
                + EditorGUIUtility.standardVerticalSpacing * 2;  // For extra space at bottom.
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var space = EditorGUIUtility.standardVerticalSpacing;

            // Start Outfit

            var rec = new Rect(position.x, position.y, position.width, lineHeight);
            var prop = property.FindPropertyRelative(StartPropName);
            var plabel = StartLabel;
            
            var choice = OutfitterEditorUtil.OutfitTypePopup(
                rec, plabel, prop.intValue, OutfitFilterType.ExcludeCustom);

            if (choice != prop.intValue)
                prop.intValue = choice;

            // Default Outfit

            rec = new Rect(rec.xMin, rec.yMax + space, rec.width, lineHeight);
            prop = property.FindPropertyRelative(DefaultPropName);
            plabel = DefaultLabel;

            choice = OutfitterEditorUtil.OutfitTypePopup(
                rec, plabel,prop.intValue, OutfitFilterType.StandardOnly);

            if (choice != prop.intValue)
            {
                prop.intValue = choice;

                var listProp = property.FindPropertyRelative(ProtoPropName);
                if (!IsOutfitInList(listProp, choice))
                    AddPrototype(listProp, choice);    
            }

            if (m_PrototypeList == null)
                CreatePrototypeList(property);

            m_PrototypeList.DoLayoutList();

            EditorGUI.EndProperty();
        }

        private void CreatePrototypeList(SerializedProperty property)
        {
            var list = new ReorderableList(property.serializedObject
                , property.FindPropertyRelative(ProtoPropName)
                , true, true, true, true);

            list.drawHeaderCallback = delegate(Rect rect)
                {
                    var label = new GUIContent("Predefined Prototypes");
                    var style = GUI.skin.label;

                    if (list.serializedProperty.arraySize == 0)
                    {
                        style = EditorGUIUtil.RedLabel;
                        label.tooltip = "Default outfit is not defined.";
                    }

                    EditorGUI.LabelField(rect, label, style);
                };

            list.drawElementCallback = 
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    var typProp = element.FindPropertyRelative(ItemTypName);
                    var protoProp = element.FindPropertyRelative(ItemProtoName);

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
                    else if (!protoProp.objectReferenceValue)
                    {
                        var dval = property.FindPropertyRelative(DefaultPropName).intValue;

                        if (typProp.intValue == dval)
                        {
                            style = EditorGUIUtil.RedLabel;
                            label.tooltip = "Default outfit needs assignment.";
                        }
                        else
                        {
                            style = EditorGUIUtil.YellowLabel;
                            label.tooltip = "Will use default prototype: " + (OutfitType)dval;
                        }
                    }

                    EditorGUI.LabelField(rect, label, style);

                    rect = new Rect(
                        rect.xMax + 5, rect.y, position.width - rect.width - 5, rect.height);

                    EditorGUI.PropertyField(rect, protoProp, GUIContent.none);
                };

            GenericMenu.MenuFunction2 addItem = delegate(object typValue)
            {
                list.index = AddPrototype(list.serializedProperty, (int)typValue);
            };

            GenericMenu.MenuFunction addAllItems = delegate()
            {
                var stdNames = OutfitterEditorUtil.StandardOutfitNames;
                var stdValues = OutfitterEditorUtil.StandardOutfitValues;

                var idx = list.serializedProperty.arraySize;

                for (int i = 0; i < stdNames.Length; i++)
                {
                    if (!IsOutfitInList(list.serializedProperty, stdValues[i]))
                        AddPrototype(list.serializedProperty, stdValues[i]);
                }

                list.index = idx;  // Assumes never called unless if list is full.
            };

            list.onAddDropdownCallback = delegate(Rect rect, ReorderableList roList)
            {
                var menu = new GenericMenu();

                var stdNames = OutfitterEditorUtil.StandardOutfitNames;
                var stdValues = OutfitterEditorUtil.StandardOutfitValues;

                for (int i = 0; i < stdNames.Length; i++)
                {
                    if (!IsOutfitInList(list.serializedProperty, stdValues[i]))
                        menu.AddItem(new GUIContent(stdNames[i]), false, addItem, stdValues[i]);
                }

                if (menu.GetItemCount() == 0)
                    menu.AddDisabledItem(new GUIContent("[None Available]"));
                else
                    menu.AddItem(new GUIContent("All"), false, addAllItems);

                menu.ShowAsContext();
            };

            list.onRemoveCallback = delegate(ReorderableList roList)
            {
                var element = roList.serializedProperty.GetArrayElementAtIndex(roList.index);
                var defaultValue = property.FindPropertyRelative(DefaultPropName).intValue;

                if (element.FindPropertyRelative(ItemTypName).intValue == defaultValue)
                {
                    Debug.LogWarning(
                        "Can't remove the default outfit: " + (OutfitType)defaultValue);
                    return;
                }

                ReorderableList.defaultBehaviours.DoRemoveButton(roList);
            };

            m_PrototypeList = list;
        }

        private int AddPrototype(SerializedProperty listProp, int typValue)
        {
            int nidx = listProp.arraySize;

            listProp.arraySize++;

            var element = listProp.GetArrayElementAtIndex(nidx);
            element.FindPropertyRelative(ItemTypName).intValue = typValue;
            element.FindPropertyRelative(ItemProtoName).objectReferenceValue = null;

            listProp.serializedObject.ApplyModifiedProperties();

            return nidx;
        }

        private bool IsOutfitInList(SerializedProperty listProp, int typValue)
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative(ItemTypName).intValue == typValue)
                    return true;
            }

            return false;
        }
    }
}
