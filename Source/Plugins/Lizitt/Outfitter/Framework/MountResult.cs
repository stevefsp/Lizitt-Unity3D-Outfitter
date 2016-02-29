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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// The result of an accessory add or mount operation.
    /// </summary>
    public enum MountResult
    {
        /// <summary>
        /// Successfully mounted and, if applicable added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The exact meaning of this result is context senstive.  If it is returned by an outfit mount operation
        /// it means the mount was successful.  If returned by an accessory manager, such as a Body component, it 
        /// means that both the mount was successful and the manager accepted management of the accessory.
        /// </para>
        /// </remarks>
        Success = 1,

        /// <summary>
        /// Not immediately mounted.  Stored for a later attempt.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This status will only be returned by accessory managers that support storage, such as a Body component.
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
        /// Failed: Accessory rejected mount.  
        /// (E.g. No mounter available and/or invalid state.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mounters and accessories are designed to be used lazily.  It is acceptable to try to mount an accessory
        /// to a mount point, or try to use a mounter with an accessory, without first checking to see if the operation
        /// will succeed.  This status indicates a failure to mount due to the accessory deciding it can't mount
        /// when asked to mount.
        /// </para>
        /// <para>
        /// There are three main reasons an accessory will reject a mount request:  It doesn't know how to mount to the
        /// requested location.  For complex accessories, the state of the accessory doesn't allow the mount.  Or 
        /// there was an error in the requrest, such as a null mount location.
        /// </para>
        /// </remarks>
        RejectedByAccessory,

        /// <summary>
        /// Failed due to an error.  (E.g. Invalid argument value).
        /// </summary>
        FailedOnError,
    }
}
