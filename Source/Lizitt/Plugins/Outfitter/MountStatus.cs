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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// Represents the possible results of a body accessory add operation.
    /// </summary>
    public enum MountStatus
    {
        /// <summary>
        /// Added successfully. (Transfer of ownership succeeded.)
        /// </summary>
        Success = 1,

        /// <summary>
        /// Not immediately added.  Stored for a later attempt.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This status may be returned my components that support accessory management.
        /// </para>
        /// </remarks>
        Stored,

        /// <summary>
        /// Failed: Blocked by coverage restrictions.
        /// </summary>
        CoverageBlocked,

        /// <summary>
        /// Failed: Blocked by 'accessories limited' flag.
        /// </summary>
        OutfitIsLimited,

        /// <summary>
        /// Failed: Required mount point not found.
        /// </summary>
        NoMountPoint,

        /// <summary>
        /// Failed: Required mount point is blocked.
        /// </summary>
        LocationBlocked,

        /// <summary>
        /// Failed: Accessory rejected mount.  (E.g. No mounter available for location or state.)
        /// </summary>
        RejectedByAccessory,

        /// <summary>
        /// Failed due to an error.  (Must succeed but no outfit, invalid arguments, etc.)
        /// </summary>
        FailedOnError,


    }
}
