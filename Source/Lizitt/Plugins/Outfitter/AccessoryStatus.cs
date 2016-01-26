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
    /// An accessory status.
    /// </summary>
    public enum AccessoryStatus
    {
        /// <summary>
        /// The accessory is inactive and unmanaged.  (Not mounted, not managed.)
        /// </summary>
        NotMounted = 0,

        /// <summary>
        /// The accessory is attached to a mount point.
        /// </summary>
        Mounted,

        /// <summary>
        /// The accessory is transitioning to 'mounted'. (Mount in progress.)
        /// </summary>
        Mounting,

        /// <summary>
        /// The accessory is transitioning to 'not mounted'. (Unmount in progress.)
        /// </summary>
        Unmounting,

        /// <summary>
        /// The accessory is inactive and managed.  (Not mounted, owned/managed.)
        /// </summary>
        Stored,

        /// <summary>
        /// The accessory has become invalid.  (Usually due to baking.)
        /// </summary>
        Invalid,
    }
}
