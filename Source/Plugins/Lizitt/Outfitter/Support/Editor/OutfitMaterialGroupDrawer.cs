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
    [CustomPropertyDrawer(typeof(OutfitMaterialGroupAttribute))]
    public sealed class OutfitMaterialGroupDrawer
        : PropertyDrawer
    {
        private const string ItemFieldName = "m_Items";
        private const float ElementLabelWidth = 75;

        OutfitMaterialListControl m_GuiControl;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (m_GuiControl == null)
                Initialize(label);

            return m_GuiControl.GetPropertyHeight(property.FindPropertyRelative(ItemFieldName));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_GuiControl == null)
                Initialize(label);

            m_GuiControl.OnGUI(position, property.FindPropertyRelative(ItemFieldName), label);
        }

        private void Initialize(GUIContent label)
        {
            var settings = new OutfitMaterialListInfo();

            settings.ItemDataPropName = "m_Material";
            settings.ItemTypePropName = "m_Type";

            settings.ListHeaderLabel = label.text;

            settings.ElementHeight = ReorderableListControl.SingleElementHeight;

            settings.ValidateElement = Validate;
            settings.DrawElement = DrawElement;

            m_GuiControl = new OutfitMaterialListControl(settings);
        }

        private static PropertyValidationResult Validate(SerializedProperty property, out string message)
        {
            if (!property.objectReferenceValue)
            {
                message = "Material is required.";
                return PropertyValidationResult.Error;
            }

            message = null;
            return PropertyValidationResult.Success;
        }

        private static void DrawElement(
            Rect position, SerializedProperty property, GUIContent label, GUIStyle labelStyle)
        {
            var rect = new Rect(position.x, position.y, ElementLabelWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(rect, label, labelStyle);

            rect = new Rect(rect.xMax + 5, rect.y, position.width - rect.width - 5, rect.height);

            EditorGUI.PropertyField(rect, property, GUIContent.none);
        }
    }
}

