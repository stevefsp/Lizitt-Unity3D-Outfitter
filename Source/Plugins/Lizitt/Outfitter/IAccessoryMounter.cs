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
using UnityEngine;

namespace com.lizitt.outfitter

{    /// <summary>
    /// An object responsible for mounting an accessory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All accessories are required to support custom mounters via their <see cref="Accessory.Mount"/> method,  
    /// but some accessories have built-in mount capabilities and don't use mounters for normal operation.
    /// </para>
    /// <para>
    /// A specific mounter may only be able to mount an accessory to a single location, or multiple locations.  
    /// It may require the accessory to be in a specifc state before performing a mount.  (E.g. A mounter that can
    /// only transfer an accessory from the 'right hand' to the 'left hand'.)  A mounter may perform a simple 
    /// mount that completes immediately, or a complex mount involving animations.  There are very few limitations
    /// on what a mounter can do.
    /// </para>
    /// </remarks>
    public interface IAccessoryMounter
    {
        /// <summary>
        /// The coverage of the accessory when it is attached to the specified location using the mounter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return zero if either the accessory will have no coverage for the location, or the mounter can 
        /// never mount to the specified location. (See: <see cref="CanMount"/>)
        /// </para>
        /// </remarks>
        /// <param name="locationType">The mount location</param>
        /// <returns>
        /// The coverage of the accessory when it is attached to the specified mount point.
        /// </returns>
        /// <seealso cref="CanMount"/>
        BodyCoverage GetCoverageFor(MountPointType locationType);

        /// <summary>
        /// The default location type the of the mounter.
        /// <remarks>
        /// <para>
        /// All mounters must be able to mount to at least one type of mount point.  This property defines that type.
        /// But there is no quarentee that the mounter will successfully mount all accessories and to all all 
        /// locations of this type since a mounter is allowed to have complex prerequisites.  E.g. Only mount to 
        /// locations that are part of an outfit and the outfit is in a particular configuration.  The only way to
        /// guarentee a successful mount is to first check <see cref="CanMount"/>.
        /// </para>
        /// </remarks>
        MountPointType DefaultLocationType { get; }

        /// <summary>
        /// Determines if the mounter can mount the accessory to the specified location based on the accessory's 
        /// current state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are several common reasons the mounter will return false.  The mount location is not supported.
        /// The accessory is not is a supported state.  The type of  the accessory is not supported. Etc. 
        /// For example: A mounter may be designed to only mount a two handed weapon from the right hand to 
        /// the upper back, so it will return false if the accessory is not a two handed weapon currently mounted 
        /// to the right hand, or the mount location is not the upper back.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to check. (Required)</param>
        /// <param name="locationType">The mount location.</param>
        /// <returns>True if the mounter can be used to mount the accessory to the specified location.</returns>
        bool CanMount(Accessory accessory, MountPointType locationType);

        /// <summary>
        /// Initializes the mount operation for the specified accessory and location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the initialization is successful, then either <see cref="UpdateMount"/> must be called through to
        /// completion, or <see cref="CancelMount"/> used to cancel the operation.
        /// </para>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true.  But it is valid to use 
        /// a call to this method without pre-checking mountability.  E.g. As an optimitation, it is valid to simply
        /// call this method on a list of all available mounters until one succeeds or all fail.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="location">The mount location (Required)</param>
        /// <returns>True if initialization was successful.  False if the mount operation is not supported.</returns>
        bool InitializeMount(Accessory accessory, MountPoint location);

        /// <summary>
        /// Processes the mount operation until it completes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="InitializeMount"/> must be called and return true before calling this method, then either
        /// this method must be called through to completion or <see cref="CancelMount"/> used to cancel the operation.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to update. (Required)</param>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="immediateComplete">
        /// If true then the mounter should complete immeidately, even though it may normally take time to complete.
        /// False to complete its update normally.
        /// </param>
        /// <returns>True while the mount operation is in-progress.  False when the operation is complete.</returns>
        bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete = false);

        /// <summary>
        /// Immediately cancel an in-progress mount operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to cancel a mount operation started by <see cref="InitializeMount"/> but not 
        /// completed by <see cref="UpdateMount"/>.  The state of the accessory following cancellation is 
        /// implementation dependant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory. (Required)</param>
        /// <param name="location">The mount location. (Required).</param>
        void CancelMount(Accessory accessory, MountPoint location);

        /// <summary>
        /// Informs the mounter that an accessory that uses the mounter is about to be destroyed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// What a mounter does is implementation specific.  If a mounter is associated with only a single accessory
        /// it may choose to destroy itself.  If it serves mulitple accessories then is may choose to do nothing.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory about to be destroyed. (Required)</param>
        /// <param name="type">The type of destroy being performed.</param>
        void OnAccessoryDestroy(Accessory accessory, DestroyType type);
    }
}
