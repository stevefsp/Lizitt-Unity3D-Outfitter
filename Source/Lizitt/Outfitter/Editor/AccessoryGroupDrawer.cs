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

namespace com.lizitt.outfitter.editor
{
    [CustomPropertyDrawer(typeof(AccessoryGroup))]
    public class AccessoryGroupDrawer
        : PropertyDrawer
    {
        private const string ItemPropName = "m_Items";

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
                    EditorGUI.LabelField(rect, "Accessories");
                };

            list.elementHeight = ElementHeight;

            list.drawElementCallback =
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    position = new Rect(position.x,
                        position.y + EditorGUIUtility.standardVerticalSpacing,
                        position.width, EditorGUIUtility.singleLineHeight);

                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(position, element, GUIContent.none);
                };

            list.onAddCallback = delegate(ReorderableList roList)
            {
                roList.index = AddPrototype(roList.serializedProperty);
            };

            m_List = list;
        }

        private int AddPrototype(SerializedProperty listProp)
        {
            int nidx = listProp.arraySize;

            listProp.arraySize++;

            var element = listProp.GetArrayElementAtIndex(nidx);
            // Override default behavior.  Rarely want to duplicate accessories.
            element.objectReferenceValue = null;

            listProp.serializedObject.ApplyModifiedProperties();

            return nidx;
        }
    }
}
