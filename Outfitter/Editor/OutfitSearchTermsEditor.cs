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
using com.lizitt.u3d.editor;
using UnityEditor;

namespace com.lizitt.outfitter.editor
{
    [CustomEditor(typeof(OutfitSearchTerms))]
    public class OutfitSearchTermsEditor
        : Editor
    {
        private const string MenuItemName = "Outfit Search Terms";
        private const int MenuPriority = EditorUtil.AssetGroup + OutfitterEditor.MenuPriority + 2;

        [MenuItem(EditorUtil.AssetCreateMenu + MenuItemName, false, MenuPriority)]
        [MenuItem(EditorUtil.UnityAssetCreateMenu + MenuItemName, false, MenuPriority)]
        static void CreateAsset()
        {
            var item = EditorUtil.CreateAsset<OutfitSearchTerms>("", OutfitterEditor.AssetLabel);
            Selection.activeObject = item;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox("Terms are used to perform partial case-insenstive matches of"
                + " object names in an outfit prototype.  The type of object is dependant on the term.",
                    MessageType.Info, true);
        }
    }
}
