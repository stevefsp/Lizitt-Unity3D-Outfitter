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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// Well known outfit material types.
    /// </summary>
    public enum OutfitMaterialType
    {
        /* 
         * Design notes:
         * 
         * This class can be customized to meet the needs of the target project.  Just make sure you understand 
         * all notes before making changes. 
         * 
         * New name-value pairs can be added at any time. Unused pairs can be removed with no impact. 
         * 
         * The meaning of names can be repurposed and refactored as desired.
         * 
         * Be careful when removing pairs that have been used, or changing values that have been assigned to an 
         * asset or scene object.
         * 
         */

        /// <summary>
        /// The body material.
        /// </summary>
        Body = 0,

        /// <summary>
        /// The eye material.
        /// </summary>
        Eye = 1,

        /// <summary>
        /// The head material.
        /// </summary>
        Head = 2,

        /// <summary>
        /// The team material.
        /// </summary>
        Team = 3,

        /// <summary>
        /// Custom material A.
        /// </summary>
        CustomA = 1001,

        /// <summary>
        /// Custom material B.
        /// </summary>
        CustomB = 1002,
    }
}
