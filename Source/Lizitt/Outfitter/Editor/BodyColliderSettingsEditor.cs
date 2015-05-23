﻿/*
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
    public static class BodyColliderSettingsEditor
    {
        private const string MenuItemName = "Body Collider Settings";
        private const int MenuPriority = EditorUtil.AssetGroup + OutfitterEditor.MenuPriority + 1;

        [MenuItem(EditorUtil.AssetCreateMenu + MenuItemName, false, MenuPriority)]
        [MenuItem(EditorUtil.UnityAssetCreateMenu + MenuItemName, false, MenuPriority)]
        static void CreateAsset()
        {
            var item = EditorUtil.CreateAsset<BodyColliderSettings>("", OutfitterEditor.AssetLabel);
            Selection.activeObject = item;
        }

        /*
         * Design notes:
         * 
         * Prototyped custom editor options to try to improve the body collider array layouts.
         * The default array handling is ugly and difficult to browse.
         * 
         * Can't use ReorderableList because the elements require too much vertical space 
         * and it requires a custom editor for each collider type since EditorGUI.PropertyField 
         * creates a foldout that is closed by default. Foldouts in a ReorderableList list are 
         * very ugly.  
         * 
         * Other options aren't any better since they would require custom array handling code,
         * just for this one-off. Too much work for too little benefit.
         */
    }
}
