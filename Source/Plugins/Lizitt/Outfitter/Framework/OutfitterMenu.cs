/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
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
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Provides various constants for use with Unity Editor menus attributes.
    /// </summary>
    public static class OutfitterMenu
    {
        /*
         * Utility members for use with the Unity editor.  E.g. With CreateAssetMenu, AddComponentMenu, and MenuItem.
         */

        /// <summary>
        /// The outfitter menu path for the GameObject and Component menus.
        /// </summary>
        public const string Menu = LizMenu.Menu + "Outfitter/";

        #region Asset Menu

        /// <summary>
        /// The base outfitter menu for the asset creation menu.
        /// </summary>
        public const string AssetMenu = "Lizitt Outfitter/";

        /// <summary>
        /// The base menu order for outfit related assets.
        /// </summary>
        public const int OutfitAssetMenuOrder = LizMenu.AssetMenuOrderStart + 1;

        /// <summary>
        /// The base menu order for accessory related assets. (Except mounters.)
        /// </summary>
        public const int AccessoryAssetMenuOrder = OutfitAssetMenuOrder + LizMenu.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for mounter related assets.
        /// </summary>
        public const int MounterAssetMenuOrder = AccessoryAssetMenuOrder + LizMenu.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for body related assets.
        /// </summary>
        public const int BodyAssetMenuOrder = MounterAssetMenuOrder + LizMenu.MenuGroupAllocation + 10;

        #endregion

        #region Component Menu

        /// <summary>
        /// The base menu order for outfit related components.
        /// </summary>
        public const int OutfitComponentMenuOrder = LizMenu.ComponentMenuOrderStart;

        /// <summary>
        /// The base menu order for accessory related components. (Except mounters.)
        /// </summary>
        public const int AccessoryComponentMenuOrder = OutfitComponentMenuOrder + LizMenu.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for mounter related components.
        /// </summary>
        public const int MounterComponentMenuOrder = AccessoryComponentMenuOrder + LizMenu.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for body related components.
        /// </summary>
        public const int BodyComponentMenuOrder = MounterComponentMenuOrder + LizMenu.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for editor-only related components.
        /// </summary>
        public const int EditorComponentMenuOrder = BodyComponentMenuOrder + LizMenu.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for outfit related components.
        /// </summary>

        #endregion
    }
}