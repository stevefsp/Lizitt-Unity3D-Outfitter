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
    /// Accessories are usually components meant to be attached to outfit mount points.  But
    /// an accessory can be attached to any mount point, no matter the owner.  Accessories can
    /// be visible, such as a hat, sun glasses, a baseball bat, etc.  Or they can be
    /// purely functional such as a perception system.
    /// </para>
    /// <para>
    /// The functionality of an Accessory component can extended in one of two ways:  
    /// Class extension and through the use of <see cref="IAccessoryObserver"/>s.  
    /// </para>
    /// <para>
    /// Warning: Do not make Accessory classes a required component of other components. 
    /// I.e. Don't do this:  [RequireComponent(typeof(Accessory))]. Doing so can prevent proper 
    /// baking.
    /// </para>
    /// </remarks>
    public abstract class Accessory
        : MonoBehaviour
    {
        /// <summary>
        /// The current accessory status.
        /// </summary>
        public abstract AccessoryStatus Status { get; }

        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All standard implementations require the observer to be a Unity Object, so it is
        /// acceptable for an accessory to reject an observer.  The accessory will post
        /// an error to the console if it rejects an accessory.
        /// </para>
        /// <para>
        /// An observer will only be added once.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required)</param>
        /// <returns>
        /// True if the observer was accepted or already added.  False if the observer was rejected.
        /// </returns>
        public abstract bool AddObserver(IAccessoryObserver observer);

        /// <summary>
        /// Remove the specified event listener.
        /// </summary>
        /// <param name="observer">The observer to remove.</param>
        public abstract void RemoveObserver(IAccessoryObserver observer);
         
        /// <summary>
        /// The location the accessory is mounted to, or null if not mounted.  (Mount operation
        /// may still be in progress.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property will be non-null if <see cref="Status"/> is 'mounted' or 'mounting'.
        /// Otherwise it will be null.
        /// </para>
        /// </remarks>
        public abstract MountPoint CurrentLocation { get; }

        /// <summary>
        /// True if the accessory is currently attached to the specified mount location.
        /// (Mount operation may still be in progress.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a convenience method that can be used rather than null checking then getting
        /// the type of <see cref="CurrentLocation"/>.  It will always return true if the
        /// location type of <see cref="CurrentLocation"/> is <paramref name="locationType"/>.
        /// Otherwise it will be false.
        /// </para>
        /// </remarks>
        /// <param name="locationType">The mount location.</param>
        /// <returns>True if the accessory is currently attached to the specified mount location.</returns>
        public abstract bool IsMountedTo(MountPointType locationType);

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
        /// True if the accessory can be mounted to the specified location without violating the
        /// specified coverage restrictions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Many accessories have different coverage for different mount points.  In such
        /// cases, the <paramref name="restrictions"/> allows the accessory to evaluate
        /// whether it should be mounted to the specified location.
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
        /// True if the accessory can be mounted to the specified location, ignoring any coverage
        /// restrictions.
        /// </summary>
        /// <param name="locationType">The desired location.</param>
        /// <param name="restrictions">Disallowed body coverage.</param>
        /// <returns>
        /// True if the accessory can mount to the specified location, ignoring coverage
        /// restrictions.
        /// </returns>
        public bool CanMount(MountPointType locationType)
        {
            return CanMount(locationType, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The body coverage of the accessory when attached to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return zero if either the accessory has no coverage for the location, or it
        /// can't mount to the specified location at all.
        /// </para>
        /// </remarks>
        /// <param name="locationType">The mount location.</param>
        /// <returns>
        /// The body coverage of the accessory when attached to the specified mount point.
        /// </returns>
        public abstract BodyCoverage GetCoverageFor(MountPointType locationType);

        /// <summary>
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true and
        /// no <paramref name="priorityMounter"/> is used. But it is valid to use a call to this
        /// method without pre-checking mountability. E.g. As an optimitation, it is valid to
        /// simply call this method on a list of all available accessories to let the accessory 
        /// decide whether or not it can attach.
        /// </para>
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in situations where an accessory 
        /// uses a generic mounter that doesn't provide coverage information.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location.</param>
        /// <param name="priorityMounter">
        /// The mounter to attempt before any others are tried.  (Essentially, a custom mounter.)
        /// </param>
        /// <param name="additionalCoverage">
        /// Additional coverage to apply on a successful mount, above and beyond the coverage
        /// supplied by the mounter or built into the accessory.
        /// </param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public abstract bool Mount(
            MountPoint location, AccessoryMounter priorityMounter, BodyCoverage additionalCoverage);

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
        /// decide whether or not it can attach.
        /// </para>
        /// <param name="location">The mount location.</param>
        /// <param name="priorityMounter">
        /// The mounter to attempt before any others are tried.  (Essentially, a custom mounter.)
        /// </param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>

        public bool Mount(MountPoint location, AccessoryMounter priorityMounter)
        {
            return Mount(location, priorityMounter, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true.
        /// But it is valid to use a call to this method without pre-checking mountability. 
        /// E.g. As an optimitation, it is valid to simply call this method on a list of all
        /// available accessories to let the accessory decide whether or not it can attach.
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in situations where an accessory 
        /// uses a generic mounter that doesn't provide coverage information.
        /// </para>
        /// <param name="location">The mount location.</param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>

        public bool Mount(MountPoint location)
        {
            return Mount(location, null, 0);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Unmount the accessory from its current location.
        /// </summary>
        /// <remarks>
        /// An unmount operation will always succeed.  A false return value indicates that an
        /// unmount was not necessary.
        /// </remarks>
        /// <param name="priorityMounter">
        /// The mounter to attempt before any others are tried.  (Essentially, a custom unmounter.)
        /// </param>
        /// <returns>True if the mount status of the accessory has changed.</returns>
        public abstract bool Unmount(AccessoryMounter priorityMounter);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Optional parameter key duplication bug workaround.

        /// <summary>
        /// Unmount the accessory from its current location.
        /// </summary>
        /// <remarks>
        /// An unmount operation will always succeed.  A false return value indicates that an
        /// unmount was not necessary.
        /// </remarks>
        /// <returns>True if the mount status of the accessory has changed.</returns>
        public bool Unmount()
        {
            return Unmount(null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

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
        /// A transition to storage can only occur form the 'not mounted' state. Calling this
        /// method from any other state except 'stored' will result in a failure.
        /// </para>
        /// </remarks>
        /// <returns>
        /// True if already in the stored state, or if the transition to the stored
        /// state was successful.
        /// </returns>
        public abstract bool Store();

        /// <summary>
        /// Transition out of the 'stored' state. (If needed.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Lazy calling is permitted. Will only take action if the accessory is currently in
        /// the 'stored' state.
        /// </para>
        /// </remarks>
        /// <returns>
        /// True if a transition out of the 'stored' state occurred.  Otherwise false.
        /// </returns>
        public abstract bool Unstore();

        /// <summary>
        /// Request the accessory bake itself for use as a static, non-accessory object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// What happens to an accessory when it is baked is implementation specific.
        /// The accessory may bake its skinned meshes, remove colliders, etc.  Baking most often
        /// happens when baking the outfit the accessory is attached to.
        /// </para>
        /// <para>
        /// It is a valid behavior for an accessory to simply auto-unmount as a result of a bake
        /// request.  This supports situations where an accessory is semi-independant.
        /// E.g. A fairy that is fluttering around an outfit's head.  A bake (a.k.a death)
        /// of the outfit results in the fairy being freed.
        /// </para>
        /// <para>
        /// The accessory component is responsible for destroying itself as appropriate.  So
        /// the client only needs to call this method then dispose of its references to the 
        /// component.
        /// </para>
        /// </remarks>
        public abstract void Bake();
    }
}

