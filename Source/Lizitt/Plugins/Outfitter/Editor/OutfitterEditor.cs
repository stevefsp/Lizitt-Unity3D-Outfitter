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

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// The <see cref="Outfitter"/> editor.
    /// </summary>
    [CustomEditor(typeof(Outfitter))]
    public class OutfitterEditor
        : Editor
    {
        /// <summary>
        /// The standard menu priority for Outfitter related components.
        /// </summary>
        public const int MenuPriority = 200;

        /// <summary>
        /// The standard asset label for for outfitter assets.
        /// </summary>
        public const string AssetLabel = "Outfitter";

        /// <summary>
        /// The Editor inspector gui method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var targ = target as Outfitter;

            base.OnInspectorGUI();

            if (AssetDatabase.IsMainAsset(targ.gameObject)
                || AssetDatabase.IsSubAsset(targ.gameObject))
            {
                return;
            }

            EditorGUILayout.Separator();

            if (!Application.isPlaying)
            {
                if (targ.DefaultOutfit != OutfitType.None
                    && GUILayout.Button("Bake Placeholder"))
                {
                    targ.DeletePlaceholder();
                    targ.BakePlaceholder(true);
                }

                if (GUILayout.Button("Delete Placeholders"))
                    targ.DeletePlaceholder();
            }
        }
    }
}
