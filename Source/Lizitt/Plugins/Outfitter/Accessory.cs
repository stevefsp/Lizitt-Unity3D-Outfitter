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
    /// An accessory that can be attached to a mount point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Accessories are any game object that needs to be attached to a mount point.  They can
    /// be visible, such as a hat, sun glasses, a baseball bat, etc.  Or they can be
    /// purely functional such as a perception system. Accessories are usually components meant 
    /// to be attached to outfits. But they can be attached to any mount point, no matter 
    /// the owner.  
    /// </para>
    /// <para>
    /// The functionality of an Accessory can extended in one of two ways:  
    /// Class extension and through the use of <see cref="IAccessoryObserver"/>s.  
    /// </para>
    /// <para>
    /// Warning: Do not make the Accessory component a required component.  I.e. Don't do this:  
    /// [RequireComponent(typeof(Accessory))]. Doing so can prevent proper baking.
    /// </para>
    /// </remarks>
    public abstract class Accessory
        : MonoBehaviour
    {
        #region Core State

        /// <summary>
        /// The owner of the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ownership is managed by the accessory, and required in order to mount or store
        /// the accessory.  Otherwise it is null.  It is an informational/data field and has
        /// no control functionality.
        /// </para>
        /// </remarks>
        public abstract GameObject Owner { get; }

        /// <summary>
        /// The current accessory status.
        /// </summary>
        public abstract AccessoryStatus Status { get; }
         
        /// <summary>
        /// The location the accessory is mounted to, or null if not mounted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property will be non-null if <see cref="Status"/> is 'mounted' or 'mounting'.
        /// Otherwise it will be null.
        /// </para>
        /// </remarks>
        public abstract MountPoint CurrentLocation { get; }

        #endregion

        #region Coverage and Limits

        /// <summary>
        /// If true, the accessory can be attached to a mount point no matter the value of
        /// the mount point owner's 'accessories limited' flag.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag is a hint.  While a concreate accessory can perform any sort of validation it
        /// choses before mounting, an accessory is not repsonbile for knowing anything about
        /// the owner of the mount point it is given.  It is generally the responsibility of
        /// the component that is telling the accessory to mount, to first check this flag
        /// and respond accordingly.
        /// </para>
        /// <para>
        /// Limited outfits are outfits that generally should not have visible accessories 
        /// attached. E.g. You generally don't want a hat or glasses to be attached to a full
        /// body outfit such as a space suit.
        /// </para>
        /// </remarks>
        public abstract bool IgnoreLimited { get; set; }

        /// <summary>
        /// The coverage for the current mount point, or zero if not mounted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An accessory does not have to have coverage.  So this may have a value of zero even
        /// when mounted.  It will only have a non-zero value if <see cref="Status"/> is
        /// 'mounted' or 'mounting'.
        /// </para>
        /// </remarks>
        public abstract BodyCoverage CurrentCoverage { get; }

        /// <summary>
        /// The body coverage of the accessory when attached to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return zero if either the accessory has no coverage when mounted to the location, 
        /// or it can't mount to the specified location at all.  (See: <see cref="CanMount"/>)
        /// </para>
        /// </remarks>
        /// <param name="locationType">The mount location.</param>
        /// <returns>
        /// The body coverage of the accessory when attached to the specified mount point.
        /// </returns>
        /// <seealso cref="CanMount"/>
        public abstract BodyCoverage GetCoverageFor(MountPointType locationType);

        #endregion

        #region Mounting

        /// <summary>
        /// The location type the accessory can mount to as long as there are no state restrictions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All acceessories must be able to mount to at least one type of mount point.  This property defines 
        /// that type. But there is no quarentee that the accessory will successfully mount to all mount 
        /// points of this type since an accessory is allowed to have complex prerequisites.  E.g. Only mount to
        /// locations that are part of an outfit and the outfit is in a particular configuration.
        /// </para>
        /// </remarks>
        public abstract MountPointType DefaultLocationType { get; }

        /// <summary>
        /// True if the accessory can be mounted to the location without violating the
        /// specified coverage restrictions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an accessory can mount to multiple locations it will often have different 
        /// coverage for each location.  In such cases, <paramref name="restrictions"/> 
        /// allows the accessory to evaluate whether it should be mounted to the specified location.
        /// This method will return false if the accessory's coverage for the location overlaps 
        /// with <paramref name="restrictions"/>. 
        /// </para>
        /// </remarks>
        /// <param name="locationType">The desired location.</param>
        /// <param name="restrictions">Disallowed body coverage.</param>
        /// <returns>
        /// True if the accessory can mount to the specified location without violating the
        /// specified coverage restrictions.
        /// </returns>
        public abstract bool CanMount(MountPointType locationType, BodyCoverage restrictions);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Optional parameter key duplication bug workaround.

        /// <summary>
        /// True if the accessory can be mounted to the specified location, without regard for
        /// coverage restrictions.
        /// </summary>
        /// <param name="locationType">The desired location.</param>
        /// <returns>
        /// True if the accessory can mount to the specified location,  without regard for
        /// coverage restrictions.
        /// </returns>
        public bool CanMount(MountPointType locationType)
        {
            return CanMount(locationType, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to succeed if <see cref="CanMount"/> returns true and
        /// no <paramref name="priorityMounter"/> is used. But it is valid to use a call to this
        /// method without pre-checking mountability. E.g. As an optimitation, it is valid to
        /// simply call this method on a list of all available accessories to let the accessory 
        /// decide whether or not it can mount.
        /// </para>
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in situations where an accessory 
        /// uses a generic mounter that doesn't provide coverage information.  On a successful
        /// mount the additional coverage will be added to the coverage
        /// supplied by the mounter or built into the accessory.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount point. (Required)</param>
        /// <param name="owner">
        /// The object that will own the accessory after a successful mount. (Required)
        /// </param>
        /// <param name="priorityMounter">
        /// The mounter to attempt to use before any others are tried.  (I.e. A custom mounter.)
        /// (Optional)
        /// </param>
        /// <param name="additionalCoverage">
        /// Additional coverage to apply on a successful mount, above and beyond the coverage
        /// supplied by the mounter or built into the accessory.
        /// </param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public abstract bool Mount(MountPoint location, GameObject owner, 
            IAccessoryMounter priorityMounter, BodyCoverage additionalCoverage);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Optional parameter key duplication bug workaround.

        /// <summary>
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true and
        /// no <paramref name="priorityMounter"/> is used. But it is valid to use a call to this
        /// method without pre-checking mountability. E.g. As an optimitation, it is valid to
        /// simply call this method on a list of all available accessories to let the accessory 
        /// decide whether or not it can mount.
        /// </para>
        /// <param name="location">The mount point. (Required.)</param>
        /// <param name="priorityMounter">
        /// The mounter to attempt to use before any others are tried.  (I.e. A custom mounter.)
        /// (Optional)
        /// </param>
        /// </remarks>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public bool Mount(MountPoint location, GameObject owner, IAccessoryMounter priorityMounter)
        {
            return Mount(location, owner, priorityMounter, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true.
        /// But it is valid to use a call to this method without pre-checking mountability. 
        /// E.g. As an optimitation, it is valid to simply call this method on a list of all
        /// available accessories to let the accessory decide whether or not it can mount.
        /// <para>
        /// <param name="location">The mount point. (Required)</param>
        /// <param name="owner">
        /// The object that will own the accessory after a successful mount. (Required)
        /// </param>
        /// </para>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public bool Mount(MountPoint location, GameObject owner)
        {
            return Mount(location, owner, null, 0);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Release & Storage

        /// <summary>
        /// Release the accessory, unmounting or unstoring it as needed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The release operation will always succeed and always complete immediately.
        /// </para>
        /// </remarks>
        public abstract void Release();

        /// <summary>
        /// Transition to the stored state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A stored accessory will have no visible or functional presence in the scene. 
        /// Storage is used by accessory managers to deactivate accessories when they are not
        /// in use. E.g. A body will store a persisent accessory when it can't be attached to 
        /// the current outfit.
        /// </para>
        /// <para>
        /// The storage opeteration will fail only when <paramref name="owner"/> is null.
        /// </para>
        /// </remarks>
        /// <param name="owner">
        /// The object that will own the the stored accessory. (Required)
        /// </param>
        /// <returns>
        /// True if the store was successful, otherwise false.
        /// </returns>
        public abstract bool Store(GameObject owner);

        #endregion

        #region Destroy

        ///// <summary>
        ///// Bake the accessory for use as a static, non-accessory object.
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// What happens to an accessory when it is baked is implementation specific.
        ///// The accessory may bake its skinned meshes, remove colliders, etc.  Baking most often
        ///// happens when baking the outfit the accessory is attached to.
        ///// </para>
        ///// <para>
        ///// It is a valid behavior for an accessory to reject the bake and simply auto-unmount 
        ///// as a result of a bake request.  This supports situations where an accessory is 
        ///// semi-independant. E.g. A fairy that is fluttering around an outfit's head.  
        ///// A bake (a.k.a death) of the agent results in the fairy being freed.
        ///// </para>
        ///// <para>
        ///// The accessory component is responsible for destroying itself as appropriate.  So
        ///// the client only needs to call this method then dispose of its references to the 
        ///// component.
        ///// </para>
        ///// </remarks>
        //public abstract void Bake();

        /// <summary>
        /// Destroy the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the best way of destroying an accessory since the accessory will send
        /// out events to its observers and other associated components so they can properly
        /// respond.
        /// </para>
        /// <para>
        /// The component is responsible for destroying itself as appropriate.  So
        /// the client only needs to call this method then dispose of its references to the 
        /// component.
        /// </para>
        /// <para><strong>Baking</strong></para>
        /// <para>
        /// What happens to an accessory when it is baked is implementation specific.
        /// The accessory may bake its skinned meshes, remove colliders, etc.  Baking most often
        /// happens when baking the outfit the accessory is attached to.
        /// </para>
        /// <para>
        /// It is valid behavior for an accessory to reject the bake and simply auto-release 
        /// as a result of the request.  This supports situations where an accessory is 
        /// semi-independant. E.g. A fairy that is fluttering around an outfit's head.  
        /// A bake (a.k.a death) of the agent results in the fairy being freed.
        /// </para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        public abstract void Destroy(DestroyType typ);

        #endregion

        #region Observers

        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All standard implementations require the observer to be a Unity Object for
        /// serialization, so it is acceptable for an accessory to reject an observer.  An
        /// error message will be logged if an observer is rejected.
        /// </para>
        /// <para>
        /// An observer can only be added once.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required)</param>
        /// <returns>
        /// True if the observer was accepted or already added.  False if the observer was rejected.
        /// </returns>
        public abstract bool AddObserver(IAccessoryObserver observer);

        /// <summary>
        /// Remove the specified event observer.
        /// </summary>
        /// <param name="observer">The observer to remove.</param>
        public abstract void RemoveObserver(IAccessoryObserver observer);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Determines if the mounter can mount the accessory to the specified location based on 
        /// the accessory's current state and without violating the coverage restrictions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method implements the standard method for this check, including all appropriate null checks.
        /// (E.g. If there is no accessory, it will return false.)
        /// </para>
        /// <para>
        /// The coverage restrictions are violated if a successful mount will result in a coverage that overlaps
        /// <paramref name="restrictions"/>.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory. (Optional)</param>
        /// <param name="mounter">The mounter. (Optional)</param>
        /// <param name="locationType">The mount location type.</param>
        /// <param name="restrictions">The body coverage restrictions.</param>
        /// <returns>
        /// True if accessory and mounter are non-null and the mounter can mount the accessory to the specified 
        /// location based on the accessory's current state and coverage restrictions.</returns>
        public static bool CanMount(
            Accessory accessory, IAccessoryMounter mounter, MountPointType locationType, BodyCoverage restrictions)
        {
            if (accessory && !LizittUtil.IsUnityDestroyed(mounter)
                && (mounter.GetCoverageFor(locationType) & restrictions) == 0)
            {
                return mounter.CanMount(accessory, locationType);
            }

            return false;
        }

        #endregion
    }
}

