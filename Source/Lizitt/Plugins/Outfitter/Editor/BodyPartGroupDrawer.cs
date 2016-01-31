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
using UnityEngine;
using com.lizitt.editor;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// <see cref="BodyPartGroup"/> custom editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(BodyPartGroup))]
    public sealed class BodyPartGroupDrawer
        : PropertyDrawer
    {
        ReferenceListControl m_GuiElement;

        private SerializedProperty GetListProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("m_Items");
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var listProp = GetListProperty(property);

            CheckInitialized(listProp);

            return m_GuiElement.GetPropertyHeight(listProp);
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var listProp = GetListProperty(property);

            CheckInitialized(listProp);

            m_GuiElement.OnGUI(position, listProp, label);
        }

        private void CheckInitialized(SerializedProperty listProperty)
        {
            if (m_GuiElement == null)
                m_GuiElement = new ReferenceListControl(listProperty, "Body Parts", true);
        }
    }
}

