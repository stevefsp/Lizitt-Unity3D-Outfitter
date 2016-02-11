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
using com.lizitt.editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// Editor for <see cref="SimpleOutfitFactory"/>.
    /// </summary>
    [CustomEditor(typeof(SimpleOutfitFactory))]
    public class SimpleOutfitFactoryEditor
        : Editor
    {
        private const string ListFieldName = "m_Variants";
        private const string ItemNameField = "name";
        private const string ItemPrototypeField = "prototype";

        private ReorderableList m_List;

        /// <summary>
        /// The Editor inspector GUI method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Simple Outfit Factory", EditorGUIUtil.BoldLabel);

            EditorGUILayout.Space();

            if (m_List == null)
                CreateReorderableList();

            m_List.DoLayoutList();

            EditorGUILayout.HelpBox(typeof(SimpleOutfitFactory).Name 
                + "\n\nInstantiates the requested prototype"
                + " without modification and passes it to the client.", 
                MessageType.Info, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateReorderableList()
        {
            var list = new ReorderableList(serializedObject
                , serializedObject.FindProperty(ListFieldName)
                , true, true, true, true);

            list.elementHeight = (EditorGUIUtility.singleLineHeight 
                + EditorGUIUtility.standardVerticalSpacing * 2) * 2.2f;

            list.drawHeaderCallback = delegate(Rect rect)
            {
                EditorGUI.LabelField(rect, "Variants", EditorGUIUtil.BoldLabel);
            };
              
            list.drawElementCallback =
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    // Name field //////////////////////////////////////////////////////////////////

                    var prop = element.FindPropertyRelative(ItemNameField);

                    GUIStyle style;

                    if (prop.stringValue == null || string.IsNullOrEmpty(prop.stringValue.Trim()))
                        style = EditorGUIUtil.RedLabel;
                    else
                        style = EditorStyles.label;

                    var rect = new Rect(position.x, 
                        position.y + EditorGUIUtility.standardVerticalSpacing, 
                        70, EditorGUIUtility.singleLineHeight);

                    // Note: Labels don't display the tooltip.  So GUIContent is not useful.
                    EditorGUI.LabelField(rect, prop.displayName, style);

                    rect = new Rect(
                        rect.xMax + 5, rect.y, position.width - rect.width - 5, rect.height);

                    EditorGUI.PropertyField(rect, prop, GUIContent.none);

                    // Prototpye field /////////////////////////////////////////////////////////////

                    prop = element.FindPropertyRelative(ItemPrototypeField);

                    if (!prop.objectReferenceValue)
                        style = EditorGUIUtil.RedLabel;
                    // TODO: Can't do this check yet.  Validation doesn't work well on prefabs.  Supposed to be fixed in 5.3.
                    // else if (!((Outfit)prop.objectReferenceValue).IsOutfitValid())
                    //    // Not required to be valid until after instantiate.  So only a warning.
                    //    style = EditorGUIUtil.YellowLabel;
                    else
                        style = EditorStyles.label;

                    rect = new Rect(position.x,
                        rect.y + EditorGUIUtility.singleLineHeight 
                        + EditorGUIUtility.standardVerticalSpacing, 
                        70, EditorGUIUtility.singleLineHeight);

                    EditorGUI.LabelField(rect, prop.displayName, style);

                    rect = new Rect(
                        rect.xMax + 5, rect.y, position.width - rect.width - 5, rect.height);

                    EditorGUI.PropertyField(rect, prop, GUIContent.none);
                };

            list.onAddCallback = 
                delegate(ReorderableList lst)
                {
                    int nidx = lst.serializedProperty.arraySize;

                    lst.serializedProperty.arraySize++;

                    var element = lst.serializedProperty.GetArrayElementAtIndex(nidx);
                    element.FindPropertyRelative(ItemNameField).stringValue = "Outfit-" + nidx;
                    element.FindPropertyRelative(ItemPrototypeField).objectReferenceValue = null;

                    lst.serializedProperty.serializedObject.ApplyModifiedProperties();

                    list.index = nidx;
                };

            m_List = list;
        }
    }
}
