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

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// <see cref="StandardBody"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(StandardBody))]
    public partial class StandardBodyEditor
        : BodyEditor
    {
        #region Main Editor

        private StandardBody Target
        {
            get { return target as StandardBody; }
        }

        public override bool RequiresConstantRepaint()
        {
            return OutfitterEditorUtil.ShowInspectorActions;
        }

        private bool m_IsAsset;

        void OnEnable()
        {
            m_IsAsset = AssetDatabase.Contains(target);
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_IsAsset)
                return;

            OutfitterEditorUtil.ShowInspectorActions = 
                EditorGUILayout.Foldout(OutfitterEditorUtil.ShowInspectorActions, "Actions");

            if (OutfitterEditorUtil.ShowInspectorActions)
                DrawActions();
            
            EditorGUILayout.Space();
        }

        #endregion
    }
}
