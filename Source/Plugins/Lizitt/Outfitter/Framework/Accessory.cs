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
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An accessory that can be attached to a mount point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An accesory is any game object that needs to be attached to a mount point.  It can be visible, such
    /// as a hat, sun glasses, a baseball bat, etc.  Or it can be purely functional such as a perception 
    /// system. An accessory is usually a component meant to be attached to outfits. But it can be attached to 
    /// any mount point, no matter the owner.  
    /// </para>
    /// <para>
    /// The meaning of 'attached' and 'mounted' is implementation specific.  An accessory may be parented to its 
    /// mount point, or it may simply use the mount point as a reference location.  (E.g. A fairy that flutters
    /// arount in the vicinity of the mount point.)
    /// </para>
    /// <para>
    /// The functionality of an Accessory can be extended in one of two ways:  Class extension and through the use 
    /// of <see cref="IAccessoryObserver"/>s.  
    /// </para>
    /// <para>
    /// Warning: Do not make the Accessory component a required component.  I.e. Don't do this:  
    /// [RequireComponent(typeof(Accessory))]. Doing so can prevent proper baking.
    /// </para>
    /// </remarks>
    /// <seealso cref="MountPoint"/>
    /// <seealso cref="IAccessoryMounter"/>
    /// <seealso cref="IAccessoryObserver"/>
    public abstract class Accessory
        : MonoBehaviour
    {
        #region Core State

        /// <summary>
        /// The owner of the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Ownership is managed by the accessory and required in order to mount or store the accessory.  Otherwise
        /// it is null.  It is an informational/data field and has no control functionality.
        /// </para>
        /// </remarks>
        public abstract GameObject Owner { get; }

        /// <summary>
        /// The current accessory status.
        /// </summary>
        public abstract AccessoryStatus Status { get; }

        /// <summary>
        /// True if the accessory has an owner and a status other than 'unmanaged'.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Because UnityEngine.Object's can be unceremoniously destroyed it is generally best to check both status and
        /// ownership to determine if an accessory is truly managed.  This property performs the appropriate checks.
        /// </para>
        /// </remarks>
        public bool IsManaged
        {
            get { return Owner && Status != AccessoryStatus.Unmanaged; }
        }

        /// <summary>
        /// The location the accessory is mounted to, or null if not mounted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property will be non-null if <see cref="Status"/> is 'mounted' or 'mounting'. Otherwise it 
        /// will be null.
        /// </para>
        /// <para>
        /// The meaning of and 'mounted' is implementation specific.  The accessory may be parented to its mount 
        /// point, or it may simply use the mount point as a reference location.  (E.g. A fairy that flutters
        /// arount in the vicinity of the mount point.)
        /// </para>
        /// </remarks>
        public abstract MountPoint CurrentLocation { get; }

        #endregion

        #region Coverage and Limits

        /// <summary>
        /// If true the accessory can be attached to a mount point no matter the value of the target's 
        /// 'accessories limited' flag.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag is a hint.  While a concreate accessory can perform any sort of validation it choses before
        /// mounting, an accessory is not repsonbile for knowing anything about the mount point it is given.  It is
        /// generally the responsibility of the component that is telling the accessory to mount to first check 
        /// this flag and respond accordingly.
        /// </para>
        /// <para>
        /// Limited outfits are outfits that generally should not have visible accessories attached. E.g. You 
        /// generally don't want a hat or glasses to be attached to a full coverage outfit such as a space suit.
        /// </para>
        /// </remarks>
        public abstract bool IgnoreLimited { get; set; }

        /// <summary>
        /// The coverage for the current mount point, or zero if not mounted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An accessory does not have to have coverage, so this may have a value of zero even when mounted.  It 
        /// will only have a non-zero value if <see cref="Status"/> is 'mounted' or 'mounting'.
        /// </para>
        /// </remarks>
        public abstract BodyCoverage CurrentCoverage { get; }

        /// <summary>
        /// The body coverage of the accessory when mounted to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return zero if either the accessory has no coverage when mounted to the location, or it can't mount
        /// to the specified location at all.  (See: <see cref="CanMount"/>)
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location.</param>
        /// <returns>The body coverage of the accessory when mounted to the specified mount point.</returns>
        /// <seealso cref="CanMount"/>
        public abstract BodyCoverage GetCoverageFor(MountPoint location);

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
        /// locations that are part of an outfit, and the outfit is in a particular configuration.
        /// </para>
        /// </remarks>
        public abstract MountPointType DefaultLocationType { get; }

        /// <summary>
        /// True if the accessory can be mounted to the location without violating the specified coverage restrictions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When an accessory can mount to multiple locations it will often have different coverage for each 
        /// location.  In such cases, <paramref name="restrictions"/> allows the accessory to evaluate whether it
        /// should be mounted to a specific location. This method will return false if the accessory's coverage
        /// for the location overlaps with <paramref name="restrictions"/>. 
        /// </para>
        /// </remarks>
        /// <param name="location">The desired location.</param>
        /// <param name="restrictions">Disallowed body coverage.</param>
        /// <returns>
        /// True if the accessory can mount to the specified location without violating the specified coverage
        /// restrictions.
        /// </returns>
        public abstract bool CanMount(MountPoint location, BodyCoverage restrictions);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        /// <summary>
        /// True if the accessory can be mounted to the specified location without regard for coverage restrictions.
        /// </summary>
        /// <param name="location">The desired location.</param>
        /// <returns>
        /// True if the accessory can mount to the specified location without regard for coverage restrictions.
        /// </returns>
        public bool CanMount(MountPoint location)
        {
            return CanMount(location, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will always succeed if <see cref="CanMount(MountPoint, BodyCoverage)"/> returns true.
        /// </para>
        /// <para>
        /// Supports lazy calling.  E.g. As an optimitation it is valid to simply call this method on a list of 
        /// available accessories and let each accessory decide whether or not it can mount.
        /// </para>
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in situations where an accessory uses a generic mounter
        /// that doesn't provide coverage information.  On a successful mount the additional coverage will be
        /// added to the coverage supplied by the mounter and/or built into the accessory.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The object that will own the accessory after a successful mount. (Required)</param>
        /// <param name="priorityMounter">The mounter to attempt before any others are tried. (Optional) </param>
        /// <param name="additionalCoverage">
        /// Additional coverage to apply on a successful mount, above and beyond the coverage supplied by the 
        /// mounter and/or built into the accessory. (Optional)
        /// </param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public abstract bool Mount(MountPoint location, GameObject owner, IAccessoryMounter priorityMounter, 
            BodyCoverage additionalCoverage);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will always succeed if <see cref="CanMount(MountPoint, BodyCoverage)"/> returns true.
        /// </para>
        /// <para>
        /// Supports lazy calling.  E.g. As an optimitation it is valid to simply call this method on a list of 
        /// available accessories and let each accessory decide whether or not it can mount.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The object that will own the accessory after a successful mount. (Required)</param>
        /// <param name="priorityMounter">The mounter to attempt before any others are tried. (Optional)</param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public bool Mount(MountPoint location, GameObject owner, IAccessoryMounter priorityMounter)
        {
            return Mount(location, owner, priorityMounter, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will always succeed if <see cref="CanMount(MountPoint, BodyCoverage)"/> returns true.
        /// </para>
        /// <para>
        /// Supports lazy calling.  E.g. As an optimitation it is valid to simply call this method on a list of 
        /// available accessories and let each accessory decide whether or not it can mount.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The object that will own the accessory after a successful mount. (Required)</param>
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
        /// A stored accessory will have no visible or functional presence in the scene. Storage is used by 
        /// accessory managers to deactivate accessories when they are not in use. E.g. A body will store a 
        /// persisent accessory when it can't be attached to the current outfit.
        /// </para>
        /// <para>
        /// The storage operation will fail only if <paramref name="owner"/> is null.
        /// </para>
        /// </remarks>
        /// <param name="owner">The object that will own the the stored accessory. (Required)</param>
        /// <returns>True if the store was successful or false on an error.</returns>
        public abstract bool Store(GameObject owner);

        #endregion

        #region Destroy

        /// <summary>
        /// Destroy the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the best way of destroying an accessory since it allows the accessory to notify its observers
        /// and any other associated components so they can properly respond.
        /// </para>
        /// <para>
        /// If <paramref name="prepareOnly"/> is false the component will destroy itself, so the client only needs
        /// to call this method then dispose of its references.  If <paramref name="prepareOnly"/> is true, it is
        /// the responsiblity of the caller to properly destroy the component or its GameObject.  The primary use 
        /// case for <paramref name="prepareOnly"/> is when performing operations in the editor that require
        /// Undo behavior.
        /// </para>
        /// <para><strong>Baking</strong></para>
        /// <para>
        /// What happens to an accessory when it is baked is implementation specific. The accessory may bake its 
        /// skinned meshes, remove colliders, etc.  Baking most often happens when the outfit the accessory is
        /// attached to is baked.
        /// </para>
        /// <para>
        /// It is a valid behavior for an accessory to reject a bake and simply auto-release as a result of the 
        /// request.  This supports situations where an accessory is semi-independant. E.g. A fairy that is 
        /// fluttering around the outfit's head. A bake (a.k.a death) of the outfit results in the fairy being freed.
        /// </para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        /// <param name="prepareOnly">
        /// If true, the component will only prepare for destruction, but won't actually destroy itself.
        /// </param>
        public abstract void Destroy(DestroyType typ, bool prepareOnly);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        /// <summary>
        /// Destroy the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the best way of destroying an accessory since it allows the accessory to notify its observers
        /// and any other associated components so they can properly respond.
        /// </para>
        /// <para><strong>Baking</strong></para>
        /// <para>
        /// What happens to an accessory when it is baked is implementation specific. The accessory may bake its 
        /// skinned meshes, remove colliders, etc.  Baking most often happens when the outfit the accessory is
        /// attached to is baked.
        /// </para>
        /// <para>
        /// It is a valid behavior for an accessory to reject a bake and simply auto-release as a result of the 
        /// request.  This supports situations where an accessory is semi-independant. E.g. A fairy that is 
        /// fluttering around the outfit's head. A bake (a.k.a death) of the outfit results in the fairy being freed.
        /// </para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        /// <param name="prepareOnly">
        /// If true, the component will only prepare for destruction, but won't actually destroy itself.
        /// </param>
        public void Destroy(DestroyType typ)
        {
            Destroy(typ, false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Observers

        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An observer can only be added once and it must be implmented by a UnityEngine.Object for serialization 
        /// puposes.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required)</param>
        /// <returns>
        /// True if the observer was accepted or already added, or false if the observer is not implemented by
        /// a UnityEngine.Object.
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
        /// Determines if the mounter can mount the accessory to the specified location based on the accessory's 
        /// current state and without violating the coverage restrictions.
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
        /// <param name="location">The mount location. (Optional)</param>
        /// <param name="restrictions">The body coverage restrictions.</param>
        /// <returns>
        /// True if accessory, mounter, and location are all non-null and the mounter can mount the accessory to the 
        /// specified location with the coverage restrictions.
        /// </returns>
        public static bool CanMount(
            Accessory accessory, IAccessoryMounter mounter, MountPoint location, BodyCoverage restrictions)
        {
            if (accessory && location && !LizUtil.IsUnityDestroyed(mounter)
                && (mounter.GetCoverageFor(location) & restrictions) == 0)
            {
                return mounter.CanMount(accessory, location);
            }

            return false;
        }

        #endregion

#if UNITY_EDITOR

        #region Editor Only

        /// <summary>
        /// Add all UnityEngine.Object's that may change while performing accessory operations to the provided list. 
        /// (Including the accessory itself.) (Will not clear the list prior to use.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo method can result in changes
        /// being lost.  When updating the accessory in the editor this method will be used by the base class to 
        /// obtain a list of all known UnityEngine.Object's that may be impacted by changes to the accessory.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale in the editor.  Concrete implementations must place it
        /// inside a UNITY_EDITOR conditional compile section.
        /// </para>
        /// </remarks>
        /// <param name="list">The list to add objects to.  (Required)</param>
        protected abstract void GetUndoObjects(List<Object> list);

        /// <summary>
        /// Add all UnityEngine.Object's that may change while performing accessory operations to the provided list. 
        /// (Including the accessory itself.) (Will not clear the list prior to use.)
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo method can result in changes
        /// being lost.  When updating the accessory in the editor this method will be used by the base class to 
        /// obtain a list of all known UnityEngine.Object's that may be impacted by changes to the accessory.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale in the editor.  Concrete implementations must place it
        /// inside a UNITY_EDITOR conditional compile section.
        /// </para>
        /// <param name="accessory">The accessory. (Required.)</param>
        /// <param name="list">The list to add objects to.</param>
        /// <returns>
        /// The reference to <paramref name="list"/> if it is provided.  Otherwise a reference to a newly created list.
        /// </returns>
        public static List<Object> UnsafeGetUndoObjects(Accessory accessory, List<Object> list = null)
        {
            if (list == null)
                list = new List<Object>();

            accessory.GetUndoObjects(list);

            return list;
        }

        #endregion

#endif
    }
}

