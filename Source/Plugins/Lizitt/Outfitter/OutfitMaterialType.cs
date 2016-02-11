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
    /// Common (will known) outfit material types.
    /// </summary>
    public enum OutfitMaterialType
    {
        /* 
         * Design notes:
         * 
         * Make sure you understand all notes before refactoring this element. 
         * 
         * The meaning of any value can be repurposed.  Names can be refactored.
         * 
         * The values do not need to be sequential.
         * 
         * It is ok to add new types at any time.  
         * 
         * Unused types can be removed with no impact.
         * Be careful when removing or changing the values of a type that has been assigned 
         * to an asset or scene object.  The assignment may become unusable at run-time or it may
         * be forced to a default value the next time it is accessed in the editor.
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
