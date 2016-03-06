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
    /// <see cref="OutfitMaterialTargetGroupAttribute"/> custom editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(OutfitMaterialTargetGroupAttribute))]
    public sealed class OutfitMaterialTargetGroupDrawer
        : PropertyDrawer
    {
        private const string ItemFieldName = "m_Items";
        private const float ElementLabelWidth = 75;

        OutfitMaterialListControl m_GuiControl;
        RendererMaterialPtrControl m_MaterialControl;

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        /// <param name="property">See Unity documentation.</param>
        /// <param name="label">See Unity documentation.</param>
        /// <returns>See Unity documentation.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CheckInitialize(property, label);

            return m_GuiControl.GetPropertyHeight(property.FindPropertyRelative(ItemFieldName));
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        /// <param name="position">See Unity documentation.</param>
        /// <param name="property">See Unity documentation.</param>
        /// <param name="label">See Unity documentation.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CheckInitialize(property, label);

            m_GuiControl.OnGUI(position, property.FindPropertyRelative(ItemFieldName), label);
        }

        private void CheckInitialize(SerializedProperty property, GUIContent label)
        {
            if (m_GuiControl != null)
                return;

            var attr = attribute as OutfitMaterialTargetGroupAttribute;
            m_MaterialControl = new RendererMaterialPtrControl(attr.SearchPropertyPath);

            var settings = new OutfitMaterialListInfo();

            settings.ItemDataPropName = "m_Target";
            settings.ItemTypePropName = "m_Type";

            settings.ListHeaderLabel = label.text;

            settings.ElementHeight = EditorGUIUtility.singleLineHeight  // For label.
                + EditorGUIUtility.standardVerticalSpacing * 4
                + m_MaterialControl.GetPropertyHeight(property, GUIContent.none);

            settings.InitializeElement = InititializeElement;
            settings.DrawElement = DrawElement;

            settings.AllowMultipleTargets = attr.AllowMultipleTargets;

            m_GuiControl = new OutfitMaterialListControl(settings);
        }

        private void InititializeElement(SerializedProperty property)
        {
            RendererMaterialPtrControl.FindRendererProperty(property).objectReferenceValue = null;
            RendererMaterialPtrControl.FindIndexProperty(property).intValue = -1;
        }

        private void DrawElement(
            Rect position, SerializedProperty property, GUIContent label, GUIStyle labelStyle)
        {
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(rect, label, labelStyle);

            rect = EditorGUIUtil.NextGuiElementPosition(rect);
            rect.height = m_MaterialControl.GetPropertyHeight(property, GUIContent.none);

            m_MaterialControl.OnGUI(rect, property, GUIContent.none);
        }
    }
}

