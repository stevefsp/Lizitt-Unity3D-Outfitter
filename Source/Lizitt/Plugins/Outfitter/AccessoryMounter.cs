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
{
    /// <summary>
    /// A component responsible for mounting and/or unmounting an accessory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All accessories are required to support custom mounters.  But not all accessories use
    /// mounters.  Some have built-in mount capatilities.
    /// </para>
    /// <para>
    /// A specific mounter may only be able to mount an accessory to a single location, or 
    /// multiple locations.  It may require the accessory to be in a specifc state before
    /// performing a mount.  (E.g. A mounter that can only transfer an accessory from the
    /// 'right hand' to the 'left hand'.)  A mounter may perform a simple mount that completes 
    /// immediately, or a complex mount involving animations.  There are very few limitations
    /// on what a mounter can do.
    /// </para>
    /// </remarks>
    public abstract class AccessoryMounter
        : MonoBehaviour
    {
        /// <summary>
        /// The coverage of the accessory when it is attached to the specified location using the
        /// mounter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return zero if either the accessory will have no coverage for the location, or the
        /// mounter can never mount to the specified location. (See: <see cref="CanMount"/>)
        /// </para>
        /// </remarks>
        /// <param name="locationType">The mount location</param>
        /// <returns>
        /// The coverage of the accessory when it is attached to the specified mount point.
        /// </returns>
        /// <seealso cref="CanMount"/>
        public abstract BodyCoverage GetCoverageFor(MountPointType locationType);

        /// <summary>
        /// Determines if the mounter can mount the accessory to the specified location based on 
        /// the accessory's current state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are several common reasons the mounter will return false.  The mount location
        /// is not supported. The accessory is not is a supported state.  The type of 
        /// the accessory is not supported. Etc. For example: A mounter may be designed to only
        /// mount a two handed weapon from the right hand to the upper back.  So it will return
        /// false if the accessory is not a two handed weapon currently mounted 
        /// to the right hand, or the mount location is not the upper back.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to check. (Required)</param>
        /// <param name="locationType">The mount location.</param>
        /// <returns>
        /// True if the mounter can be used to mount the accessory to the specified location.
        /// </returns>
        public abstract bool CanMount(Accessory accessory, MountPointType locationType);

        /// <summary>
        /// Determines if the mounter can unmount the accessory based on the accessory's current 
        /// state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Accessories are always required to provide built-in unmount capabilities.  (An unmount
        /// can never fail.)  But using a mounter to perform an unmount allows implementation of
        /// custom behavior.
        /// </para>
        /// <para>
        /// The default behavior is to always return false.  Override this method to change this
        /// behavior.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to check. (Required)</param>
        /// <returns>
        /// True if the mounter can be used to unmount the accessory.
        /// </returns>
        public virtual bool CanUnmount(Accessory accessory)
        {
            return false;
        }

        /// <summary>
        /// Initializes the mount operation for the specified accessory and location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the initialization is successful, then either <see cref="UpdateMount"/>
        /// must be called through to completion, or <see cref="CancelMount"/> used to cancel the
        /// operation.
        /// </para>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> or 
        /// <see cref="CanUnmount"/> (as appropriate) return true.  But it is
        /// valid to use a call to this method without pre-checking mountability.  E.g. As an
        /// optimitation, it is valid to simply call this method on a list of all available 
        /// mounters until one succeeds or all fail.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount/unmount. (Required)</param>
        /// <param name="location">The mount location, or null to unmount.</param>
        /// <returns>
        /// True if initialization was successful.  False if the mount operation is not supported.
        /// </returns>
        public abstract bool InitializeMount(Accessory accessory, MountPoint location);

        /// <summary>
        /// Processes the mount/unmount operation until it completes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="InitializeMount"/> must be called and return true before calling this
        /// method, then either this method must be called through to completion or 
        /// <see cref="CancelMount"/> used to cancel the operation.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory mount operation to update. (Required)</param>
        /// <param name="location">The mount location, or null to unmount.</param>
        /// <returns>
        /// True while the mount operation is in-progress.  False when the operation is
        /// complete.
        /// </returns>
        public abstract bool UpdateMount(Accessory accessory, MountPoint location);

        /// <summary>
        /// Immediately cancels an in-progress mount/unmount operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to cancel a mount operation started by <see cref="InitializeMount"/>
        /// but not completed by <see cref="UpdateMount"/>.  The state of the accessory following
        /// cancellation is implementation dependant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to update. (Required)</param>
        /// <param name="location">The mount location, or null for unmount operations.</param>
        public abstract void CancelMount(Accessory accessory, MountPoint location);

        /// <summary>
        /// Instructs the mounter that its owner has been baked and is no longer avaiable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// What a mounter does when this method is called is implementation specific.
        /// The default behavior is for the mounter to self-destory if <paramref name="gameObject"/>
        /// is equal to the mounter's GameObject.  But some mounters are designed to be 
        /// shared between between multiple owners, in which case it may ignore the bake call 
        /// completely.
        /// </para>
        /// <para>
        /// Override this method in order to replace or extend the default behavior.
        /// </para>
        /// </remarks>
        /// <param name="gameObject">The mounter owner's GameObject. (Required)</param>
        public virtual void BakePost(GameObject gameObject)
        {
            if (gameObject == this.gameObject)
                this.SafeDestroy();
        }
    }
}
