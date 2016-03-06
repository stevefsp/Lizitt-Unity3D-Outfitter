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
    /// Provides various outfitter utilities.
    /// </summary>
    public static class OutfitterUtil
    {
        #region Unity Editor

        /*
         * Utility members for use with the Unity editor.  E.g. With CreateAssetMenu, AddComponentMenu, and MenuItem.
         */

        /// <summary>
        /// The outfitter menu path for the GameObject and Component menus.
        /// </summary>
        public const string Menu = LizittUtil.LizittMenu + "Outfitter/";

        #region Asset Menu

        /// <summary>
        /// The base outfitter menu for the asset creation menu.
        /// </summary>
        public const string AssetMenu = "Lizitt Outfitter/";

        /// <summary>
        /// The base menu order for outfit related assets.
        /// </summary>
        public const int OutfitAssetMenuOrder = LizittUtil.AssetMenuOrderStart + 1;

        /// <summary>
        /// The base menu order for accessory related assets. (Except mounters.)
        /// </summary>
        public const int AccessoryAssetMenuOrder = OutfitAssetMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for mounter related assets.
        /// </summary>
        public const int MounterAssetMenuOrder = AccessoryAssetMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for body related assets.
        /// </summary>
        public const int BodyAssetMenuOrder = MounterAssetMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        #endregion

        #region Component Menu

        /// <summary>
        /// The base menu order for outfit related components.
        /// </summary>
        public const int OutfitComponentMenuOrder = LizittUtil.ComponentMenuOrderStart;

        /// <summary>
        /// The base menu order for accessory related components. (Except mounters.)
        /// </summary>
        public const int AccessoryComponentMenuOrder = OutfitComponentMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for mounter related components.
        /// </summary>
        public const int MounterComponentMenuOrder = AccessoryComponentMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for body related components.
        /// </summary>
        public const int BodyComponentMenuOrder = MounterComponentMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for editor-only related components.
        /// </summary>
        public const int EditorComponentMenuOrder = BodyComponentMenuOrder + LizittUtil.MenuGroupAllocation + 10;

        /// <summary>
        /// The base menu order for outfit related components.
        /// </summary>

        #endregion

        #endregion

        #region Body Coverage

        /// <summary>
        /// All body coverage flags.
        /// </summary>
        public const BodyCoverage BodyCoverageAll = (BodyCoverage)(~0);

        /// <summary>
        /// The default choice for sorting body coverage flag values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Because this value is constant it can be used with attributes.
        /// </para>
        /// </remarks>
        public const bool SortBodyCoverage = true;

        #endregion

        #region Mount Result

        /// <summary>
        /// The status is not 'success' and not 'stored'.
        /// </summary>
        /// <param name="status">The status to evaluate.</param>
        /// <returns>The status is not 'success' and not 'stored'.</returns>
        public static bool IsFailed(this MountResult status)
        {
            return !(status == MountResult.Success || status == MountResult.Stored);
        }

        #endregion

        #region Accessory Status

        /// <summary>
        /// True if the status is 'mounted' or 'mounting'.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>True if the status is 'mounted' or 'mounting'.</returns>
        public static bool IsMounted(this AccessoryStatus status)
        {
            return status == AccessoryStatus.Mounted || status == AccessoryStatus.Mounting;
        }

        #endregion

        #region Outfit Type

        // Keep for post-v0.2 use.

        ///// <summary>
        ///// The standard default outfit.
        ///// </summary>
        //public const OutfitType DefaultOutfit = (OutfitType)0;

        ///// <summary>
        ///// True if the outfit type is classified as a 'standard' outfit. 
        ///// (A long term, general use outfit.)
        ///// </summary>
        ///// <param name="typ">The outfity type to check.</param>
        ///// <returns>
        ///// True if the outfit type is classified as a 'standard' outfit. 
        ///// </returns>
        //public static bool IsStandard(this OutfitType typ)
        //{
        //    return (int)typ < (int)OutfitType.Custom;
        //}

        ///// <summary>
        ///// True if the outfit type is classified as a 'custom' outfit. 
        ///// (A short term, special use outfit.)
        ///// </summary>
        ///// <param name="typ">The outfity type to check.</param>
        ///// <returns>
        ///// True if the outfit type is classified as a 'custom' outfit. 
        ///// </returns>
        //public static bool IsCustom(this OutfitType typ)
        //{
        //    return typ != OutfitType.None && !IsStandard(typ);
        //}

        #endregion
    }
}