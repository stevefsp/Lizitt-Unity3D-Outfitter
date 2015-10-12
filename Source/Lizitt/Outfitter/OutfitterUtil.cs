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
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Implements various outfitter utility features.
    /// </summary>
    public static class OutfitterUtil
    {
        /// <summary>
        /// The default choice for sorting body coverage flag values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is constant so it can be used with attributes.
        /// </para>
        /// </remarks>
        public const bool SortBodyCoverage = true;

        /// <summary>
        /// The standard default outfit.
        /// </summary>
        public const OutfitType DefaultOutfit = OutfitType.Casual;

        #region Extensions

        /// <summary>
        /// True if the outfit type is classified as a 'standard' outfit. 
        /// (A long term, general use outfit.)
        /// </summary>
        /// <param name="typ">The outfity type to check.</param>
        /// <returns>
        /// True if the outfit type is classified as a 'standard' outfit. 
        /// </returns>
        public static bool IsStandard(this OutfitType typ)
        {
            return (int)typ < (int)OutfitType.Custom;
        }

        /// <summary>
        /// True if the outfit type is classified as a 'custom' outfit. 
        /// (A short term, special use outfit.)
        /// </summary>
        /// <param name="typ">The outfity type to check.</param>
        /// <returns>
        /// True if the outfit type is classified as a 'custom' outfit. 
        /// </returns>
        public static bool IsCustom(this OutfitType typ)
        {
            return typ != OutfitType.None && !IsStandard(typ);
        }

        /// <summary>
        /// The status is not success and not pending.
        /// </summary>
        /// <param name="status">The status to evaluate.</param>
        /// <returns>The status is not success and not pending.</returns>
        public static bool IsFailed(this AttachStatus status)
        {
            return !(status == AttachStatus.Success || status == AttachStatus.Pending);
        }

        #endregion
    }
}