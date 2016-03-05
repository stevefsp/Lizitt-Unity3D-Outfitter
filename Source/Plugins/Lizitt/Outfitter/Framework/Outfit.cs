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
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// The high level status of an outfit.
    /// </summary>
    public enum OutfitStatus
    {
        /// <summary>
        /// The outfit is not managed and has no formal owner.
        /// </summary>
        Unmanaged = 0,

        /// <summary>
        /// The outfit is in active use.  (Ownership required.)
        /// </summary>
        InUse,

        /// <summary>
        /// The outfit is in a stored state.  (Usually non-visible.)  (Ownership required.)
        /// </summary>
        Stored,
    }

    /// <summary>
    /// A body outfit representing the physical presence of an agent.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The body outfit generally consists of the models, renderers, animators, colliders, and other components that
    /// represent an agents physical presence in the scene.
    /// </para>
    /// <para>
    /// The functionality of an outfit can extended in one of two ways:  Class extension and through the use 
    /// of <see cref="IOutfitObservers"/>s.  
    /// </para>
    /// <para>
    /// Warning: Do not make the Outfit component a required component.  I.e. Don't do this:  
    /// [RequireComponent(typeof(Outfit))]. Doing so can prevent proper baking.
    /// </para>
    /// </remarks>
    public abstract class Outfit
        : MonoBehaviour
    {
        #region Core Settings

        /// <summary>
        /// The transform that is used to move the outfit. (Required, always exists.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The motion root will never be null for a valid outfit.
        /// </para>
        /// </remarks>
        public abstract Transform MotionRoot { get; }

        /// <summary>
        /// The primary collider for the outfit, or null if there is none.
        /// </summary>
        public abstract Collider PrimaryCollider { get; set; }

        /// <summary>
        /// The primary rigidbody for the outfit, or null if there is none.
        /// </summary>
        public abstract Rigidbody PrimaryRigidbody { get; }

        /// <summary>
        /// The owner of the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is changed using <see cref="SetState"/>.  A value is required for all states except 'unmanaged'.
        /// </para>
        /// </remarks>
        public abstract GameObject Owner { get; }

        /// <summary>
        /// The status of the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is changed using <see cref="SetState"/>.
        /// </para>
        /// </remarks>
        public abstract OutfitStatus Status { get; }

        /// <summary>
        /// True if the outfit has an owner and is 'in-use' or 'stored'.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Because UnityEngine.Objects can be unceremoniously destroyed it is generally best to check both status and
        /// ownership to determine if an outfit is truly managed.  This property performs all appropriate checks.
        /// </para>
        /// </remarks>
        public bool IsManaged
        {
            get { return Owner && Status != OutfitStatus.Unmanaged; }
        }

        /// <summary>
        /// Set the outfit status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="owner"/> must be non-null for all states except 'unmanaged'.
        /// </para>
        /// </remarks>
        /// <param name="status">The status.</param>
        /// <param name="owner">The owner.  (Required for all status except 'unmanaged'.)</param>
        /// <returns>True if the operation was successful.  False on error.</returns>
        public abstract bool SetState(OutfitStatus status, GameObject owner);

        #endregion

        #region Body Parts

        /// <summary>
        /// The maximum number of body parts the outfit can have. (Some entries may be null.)
        /// </summary>
        /// <para>
        /// How body parts are managed and stored is implementation specific.  This value may be fixed or dynamic.  
        /// It may represent a true buffer size, or simply the number of currently avaiable body parts.
        /// </para>
        public abstract int BodyPartCount { get; }

        /// <summary>
        /// The body part at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index">The index. [0 &lt;= value &lt; <see cref="BodyPartCount"/>]</param>
        /// <returns>The body part at the specified index, or null if there is none.</returns>
        public abstract BodyPart GetBodyPart(int index);

        /// <summary>
        /// Gets the body part associated with the specified type, or null if there is none.
        /// </summary>
        /// <param name="typ">The type.</param>
        /// <returns>The body part associated with the specified type, or null if there is none.</returns>
        public BodyPart GetBodyPart(BodyPartType typ)
        {
            for (int i = 0; i < BodyPartCount; i++)
            {
                var item = GetBodyPart(i);
                if (item && item.PartType == typ)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// True if there is at least one body part.
        /// </summary>
        public bool HasBodyParts 
        { 
            get
            {
                for (int i = 0; i < BodyPartCount; i++)
                {
                    if (GetBodyPart(i))
                        return true;
                }

                return false;
            }
        }
        /// <summary>
        /// True if the body part is part of the outfit.
        /// </summary>
        /// <param name="bodyPart">The body part. (Required)</param>
        /// <returns>True if the body part is a part of the outfit.</returns>
        public bool Contains(BodyPart bodyPart)
        {
            if (bodyPart)
            {
                for (int i = 0; i < BodyPartCount; i++)
                {
                    var item = GetBodyPart(i);
                    if (item && item == bodyPart)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies the rigidbody behavior to all body parts owned by the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Warning: If non-body part colliders are associated with the same rigidbody as a body part collider, then 
        /// the non-body part colliders will be caught up in the behavior change.
        /// </para>
        /// </remarks>
        /// <param name="status">The desired behavior.</param>
        public void ApplyBodyPartRehavior(RigidbodyBehavior behavior)
        {
            // Optimized for one rigidbody per collider.  Other configurations will result in duplicated effort.
            for (int i = 0; i < BodyPartCount; i++)
            {
                var item = GetBodyPart(i);
                if (item && item.Collider)
                {
                    var rb = item.Collider.GetAssociatedRigidBody();
                    if (rb)
                        rb.SetBehavior(behavior, true);
                }
            }
        }

        /// <summary>
        /// Applies the layer all body part colliders owned by the outfit.
        /// </summary>
        /// <param name="layer">The layer. [0 &lt;= value &lt; 32</param>
        public void ApplyBodyPartLayer(int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogError("Body part collider layer is out of range: " + layer);
                return;
            }

            for (int i = 0; i < BodyPartCount; i++)
            {
                var item = GetBodyPart(i);
                if (item)
                    item.ColliderLayer = layer;
            }
        }

        /// <summary>
        /// Applies the context to all body parts owned by the outfit.
        /// </summary>
        /// <param name="context">The context to apply.</param>
        public virtual void ApplyBodyPartContext(GameObject context)
        {
            for (int i = 0; i < BodyPartCount; i++)
            {
                var item = GetBodyPart(i);
                if (item)
                    item.Context = context;
            }
        }

        /// <summary>
        /// Synchronize the body part state of all common body parts.
        /// </summary>
        /// <param name="to">The outfit being synchonized to. (Required)</param>
        /// <param name="from">The outfit state is syncronzied from. (Required)</param>
        /// <param name="includeStatus">Synchronize the collider status.</param>
        /// <param name="includeLayer">Synchronize the collider layer.</param>
        /// <param name="includeContext">
        /// Synchronize the context unless it is the <paramref name="from"/> object's GameObject.
        /// </param>
        public static void SynchronizeBodyPartState( 
            Outfit to, Outfit from, bool includeStatus, bool includeLayer, bool includeContext)
        {
            if (!(from && to))
                return;

            for (int i = 0; i < from.BodyPartCount; i++)
            {
                var prevPart = from.GetBodyPart(i);
                if (prevPart)
                {
                    var part = to.GetBodyPart(prevPart.PartType);
                    if (part)
                    {
                        BodyPart.Synchronize(
                            part, prevPart, includeStatus, includeLayer, includeContext, from.gameObject);
                    }
                }
            }
        }

        #endregion

        #region Mount Points & Accessories

        /// <summary>
        /// If true, only accessories marked to ignore this flag can be successfully attached.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The purpose of this flag is to allow an outfit to be marked as not appropriate for most accessories.  
        /// Consider a full body space suit.  It is normally not appropriate for hats, glasses, etc. to be mounted
        /// to such an outfit, so it is marked as limited.
        /// </para>
        /// </remarks>
        public abstract bool AccessoriesLimited { get; set; }

        /// <summary>
        /// The built-in coverage blocks for the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Built-in coverage blocks represent blocks owned by the outfit rather than accessories. Accessories that
        /// have coverage that overlaps the blocks will normally no be allowed to mount. Example:  An outfit with a
        /// built-in hat will have coverage blocks for the top of the head.
        /// </para>
        /// </remarks>
        public abstract BodyCoverage CoverageBlocks { get; set; }

        /// <summary>
        /// The combined coverage from both <see cref="CoverageBlocks"/> and accessory coverage.
        /// </summary>
        public abstract BodyCoverage CurrentCoverage { get; }

        /// <summary>
        /// The maximum number of mount points the outfit can have. (Some entries may be null.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// How mount points are managed and stored is implementation specific.  This value may be fixed or
        /// dynamic. It may represent a true buffer size, or simply the number of currently avaiable mount points.
        /// </para>
        /// </remarks>
        public abstract int MountPointCount { get; }

        /// <summary>
        /// Get the mount point at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index">The index. [0 &lt;= value &lt; <see cref="MountPointCount"/></param>
        /// <returns>The mount point at the specified index, or null if there is none.</returns>
        public abstract MountPoint GetMountPoint(int index);

        /// <summary>
        /// Gets the specified mount point, or null if there is none.
        /// </summary>
        /// <param name="locationType">The location type.</param>
        /// <returns>The specified mount point, or null if there is none.</returns>
        public virtual MountPoint GetMountPoint(MountPointType locationType)
        {
            for (int i = 0; i < MountPointCount; i++)
            {
                var item = GetMountPoint(i);
                if (item && item.LocationType == locationType)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// True if the there is a least one mount point.
        /// </summary>
        public bool HasMountPoints 
        { 
            get
            {
                for (int i = 0; i < MountPointCount; i++)
                {
                    if (GetMountPoint(i))
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// True if the mount point is part of the outfit.
        /// </summary>
        /// <param name="location">The mount point. (Required)</param>
        /// <returns>True if the mount point is a part of the outfit.</returns>
        public bool Contains(MountPoint location)
        {
            if (location)
            {
                for (int i = 0; i < MountPointCount; i++)
                {
                    var item = GetMountPoint(i);
                    if (item && item == location)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies the context to all mount points owned by the outfit.
        /// </summary>
        /// <param name="context">The context.</param>
        public void ApplyMountPointContext(GameObject context)
        {
            for (int i = 0; i < MountPointCount; i++)
            {
                var item = GetMountPoint(i);
                if (item)
                    item.Context = context;
            }
        }

        /// <summary>
        /// Synchronize the mount point state of all common mount points.
        /// </summary>
        /// <param name="to">The outfit being synchonized to. (Required)</param>
        /// <param name="from">The outfit being syncronzied from. (Required)</param>
        /// <param name="includeBlocked">Synchronize the mount point 'is blocked' state.</param>
        /// <param name="includeContext">
        /// Synchronize the context unless it is the <paramref name="from"/> object's GameObject.
        /// </param>
        public static void SynchronizeMountPointState(Outfit to, Outfit from, bool includeBlocked, bool includeContext)
        {
            if (!(from && to))
                return;

            for (int i = 0; i < from.MountPointCount; i++)
            {
                var prevPart = from.GetMountPoint(i);
                if (prevPart)
                {
                    var part = to.GetMountPoint(prevPart.LocationType);
                    if (part)
                        MountPoint.Synchronize(part, prevPart, includeBlocked, includeContext, from.gameObject);
                }
            }
        }

        /// <summary>
        /// The number of accessories mounted to the outfit.
        /// </summary>
        public abstract int AccessoryCount { get; }

        /// <summary>
        /// Get the accessory at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The return value will always be non-null unless the accessory is improperly destroyed while mounted.
        /// </para>
        /// </remarks>
        /// <param name="index">The index. [0 &lt;= value &lt; <see cref="AccessoryCount"/>]</param>
        /// <returns>The accessory at the specified index.</returns>
        public abstract Accessory GetAccessory(int index);

        /// <summary>
        /// True if the accessory is mounted to the outfit.
        /// </summary>
        /// <param name="accessory">The accessory. (Required)</param>
        /// <returns>True if the accessory is mounted to the outfit.</returns>
        public bool IsMounted(Accessory accessory)
        {
            if (accessory)
            {
                for (int i = 0; i < AccessoryCount; i++)
                {
                    var knownAcc = GetAccessory(i);
                    if (knownAcc && knownAcc == accessory)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remounting to a different or same location is allowed.  If a remount fails the accessory will be
        /// released.  This behavior is due to the multiple actors involved.  E.g. The outfit, accessory, and 
        /// possibly a mounter. The outfit can't guarentee behavior of all actors, so it implements a behavior that
        /// is consistant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore coverage restrictions and the value of <see cref="AccessoriesLimited"/>.
        /// </param>
        /// <param name="priorityMounter">
        /// The mounter that should be tried before any other mounters.  (Optional)
        /// </param>
        /// <param name="additionalCoverage">
        /// Coverage to add to the accessory if it is successfully mounted, above and beyond any coverage inherent
        /// iin the accessory and/or mounter.
        /// </param>
        /// <returns>The result of the mount attempt.</returns>
        public abstract MountResult Mount(Accessory accessory, MountPointType locationType,
            bool ignoreRestrictions, IAccessoryMounter priorityMounter, BodyCoverage additionalCoverage);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remounting to a different or same location is allowed.  If a remount fails the accessory will be
        /// released.  This behavior is due to the multiple actors involved.  E.g. The outfit, accessory, and 
        /// possibly a mounter. The outfit can't guarentee behavior of all actors, so it implements a behavior that
        /// is consistant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore coverage restrictions and the value of <see cref="AccessoriesLimited"/>.
        /// </param>
        /// <param name="priorityMounter">
        /// The mounter that should be tried before any other mounters.  (Optional)
        /// </param>
        /// <returns>The result of the mount attempt.</returns>
        public MountResult Mount(Accessory accessory, MountPointType locationType,
            bool ignoreRestrictions, IAccessoryMounter priorityMounter)
        {
            return Mount(accessory, locationType, ignoreRestrictions, priorityMounter, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remounting to a different or same location is allowed.  If a remount fails the accessory will be
        /// released.  This behavior is due to the multiple actors involved.  E.g. The outfit, accessory, and 
        /// possibly a mounter. The outfit can't guarentee behavior of all actors, so it implements a behavior that
        /// is consistant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore coverage restrictions and the value of <see cref="AccessoriesLimited"/>.
        /// </param>
        /// <returns>The result of the mount attempt.</returns>
        public MountResult Mount(Accessory accessory, MountPointType locationType, bool ignoreRestrictions)
        {
            return Mount(accessory, locationType, ignoreRestrictions, null, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remounting to a different or same location is allowed.  If a remount fails the accessory will be
        /// released.  This behavior is due to the multiple actors involved.  E.g. The outfit, accessory, and 
        /// possibly a mounter. The outfit can't guarentee behavior of all actors, so it implements a behavior that
        /// is consistant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <returns>The result of the mount attempt.</returns>
        public MountResult Mount(Accessory accessory, MountPointType locationType)
        {
            return Mount(accessory, locationType, false, null, 0);
        }

        /// <summary>
        /// Mount the accessory to its default location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remounting to a different or same location is allowed.  If a remount fails the accessory will be
        /// released.  This behavior is due to the multiple actors involved.  E.g. The outfit, accessory, and 
        /// possibly a mounter. The outfit can't guarentee behavior of all actors, so it implements a behavior that
        /// is consistant.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <returns>The result of the mount attempt.</returns>
        public MountResult Mount(Accessory accessory)
        {
            return Mount(accessory, accessory.DefaultLocationType, false, null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Unmount and release the the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The release operation will always succeed if the accessory is is known to the outfit.
        /// </para>
        /// <para>
        /// This method can be called lazily.  It will only take action if the accessory is known to the outfit.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory. (Required.)</param>
        /// <returns>True if an release occurred, false if the accessory is not known to the outfit.</returns>
        public abstract bool Release(Accessory accessory);

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
        /// <param name="observer">The observer to add. (Required. Must be a UnityEngine.Object.)</param>
        /// <returns>
        /// True if the observer was accepted or already added, or false if the observer is not implemented by
        /// a UnityEngine.Object.
        /// </returns>
        public abstract bool AddObserver(IOutfitObserver observer);

        /// <summary>
        /// Remove the specified event listener.
        /// </summary>
        /// <param name="observer">The observer to remove. (Required)</param>
        public abstract void RemoveObserver(IOutfitObserver observer);

        #endregion

        #region Renderers / Materials

        /// <summary>
        /// The outfit's blend shape renderer. (Optional)
        /// </summary>
        /// <remarks>
        /// <para>
        /// An outfit can only support a single blend shape renderer.
        /// </para>
        /// </remarks>
        public abstract SkinnedMeshRenderer BlendShapeRenderer { get; set; }

        /// <summary>
        /// True if the material is defined and can be set.
        /// </summary>
        /// <param name="typ">The material type.</param>
        /// <returns>True if the material is defined and can be set.</returns>
        public abstract bool IsMaterialDefined(OutfitMaterialType typ);

        /// <summary>
        /// Get the material types that are defined and can be set.
        /// </summary>
        /// <returns>The material types that are defined and can be set.</returns>
        public abstract OutfitMaterialType[] GetOutfitMaterialTypes();

        // TODO: Add accessors for non-shared materials.

        /// <summary>
        /// The number of outfit materials. (Some entries may be undefined.)
        /// </summary>
        public abstract int OutfitMaterialCount { get; }

        /// <summary>
        /// Gets the specified outfit material.  (Material may be null.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The meaning of a null material is implemetnation specific.  It can indicate that the target's material
        /// is null, that there are no targets for the type, that there is a configuration issue, etc.  
        /// <see cref="IsMaterialDefined"/> is helpful in determining the cause of a null.
        /// </para>
        /// </remarks>
        /// <param name="typ">The outfit material index. [0 &lt;= value &lt; <see cref="OutfitMaterialCount"/>]</param>
        /// <returns>
        /// The specified shared outfit material. (Material may be null.)
        /// </returns>
        public abstract OutfitMaterial GetSharedMaterial(int index);

        /// <summary>
        /// Gets the specified shared material, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A null value can mean there are no valid targets of the specified type, or that the target's material 
        /// is null.  <see cref="IsMaterialDefined"/> is helpful in determining the cause of an unexpected null.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>
        /// The specified shared material, or null if it is not defined.
        /// </returns>
        public abstract Material GetSharedMaterial(OutfitMaterialType typ);

        /// <summary>
        /// Sets the material of all material targets of the specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be called lazily.  If the material target is not defined the method will simply return 
        /// zero.
        /// </para>
        /// <para>
        /// This method can't be used to apply a null material.  Null materials will be ignored.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <param name="material">The material to apply. (Required.)</param>
        /// <returns>The number of targets the material was successfully applied to.</returns>
        public abstract int ApplySharedMaterial(OutfitMaterialType typ, Material material);

        #endregion

        #region Destroy Members

        /// <summary>
        /// Destroy the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: Only the owner of the oufit should call this method.
        /// </para>
        /// <para>
        /// This is the best way of destroying an outfit since it allows outfit to alert its observers and 
        /// other any other associated components so they can properly respond.
        /// </para>
        /// <para>
        /// If <paramref name="prepareOnly"/> is false the component will destroy itself as appropriate, so the 
        /// client only needs to call this method then dispose of its references to the component. 
        /// If <paramref name="prepareOnly"/> is true, it is the responsiblity of the caller to properly destroy
        /// the component or its GameObject.  The primary use case for <paramref name="prepareOnly"/> is when
        /// performing operations in the editor that required Undo behavior.
        /// </para>
        /// <para>
        /// Baking is the process of converting the outfit into a non-outfit state.  What exactly happens during
        /// the bake is implemenation specific.  It may result in the baking of skinned meshes into static meshes,  
        /// conversion to a ragdoll, etc.
        /// <para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        /// <param name="prepareOnly">
        /// If true, the outfit will only prepare for destruction, but won't actually destroy itself.
        /// </param>
        /// <param name="referenceOutfit">
        /// The outfit that the the current outfit is derived from.  (E.g. Was instanced from.) Or null if the outfit
        /// has no known source. (Only applies to the 'bake' type.)
        /// </param>
        public abstract void Destroy(DestroyType typ, bool prepareOnly, Outfit referenceOutfit);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.


        /// <summary>
        /// Destroy the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: Only the owner of the oufit should call this method.
        /// </para>
        /// <para>
        /// This is the best way of destroying an outfit since it allows outfit to alert its observers and 
        /// other any other associated components so they can properly respond.
        /// </para>
        /// <para>
        /// If <paramref name="prepareOnly"/> is false the component will destroy itself as appropriate, so the 
        /// client only needs to call this method then dispose of its references to the component. 
        /// If <paramref name="prepareOnly"/> is true, it is the responsiblity of the caller to properly destroy
        /// the component or its GameObject.  The primary use case for <paramref name="prepareOnly"/> is when
        /// performing operations in the editor that required Undo behavior.
        /// </para>
        /// <para>
        /// Baking is the process of converting the outfit into a non-outfit state.  What exactly happens during
        /// the bake is implemenation specific.  It may result in the baking of skinned meshes into static meshes,  
        /// conversion to a ragdoll, etc.
        /// <para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        /// <param name="prepareOnly">
        /// If true, the outfit will only prepare for destruction, but won't actually destroy itself.
        /// </param>
        public void Destroy(DestroyType typ, bool prepareOnly)
        {
            Destroy(typ, prepareOnly, null);
        }

        /// <summary>
        /// Destroy the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: Only the owner of the oufit should call this method.
        /// </para>
        /// <para>
        /// This is the best way of destroying an outfit since it allows outfit to alert its observers and 
        /// other any other associated components so they can properly respond.
        /// </para>
        /// <para>
        /// If <paramref name="prepareOnly"/> is false the component will destroy itself as appropriate, so the 
        /// client only needs to call this method then dispose of its references to the component. 
        /// If <paramref name="prepareOnly"/> is true, it is the responsiblity of the caller to properly destroy
        /// the component or its GameObject.  The primary use case for <paramref name="prepareOnly"/> is when
        /// performing operations in the editor that required Undo behavior.
        /// </para>
        /// <para>
        /// Baking is the process of converting the outfit into a non-outfit state.  What exactly happens during
        /// the bake is implemenation specific.  It may result in the baking of skinned meshes into static meshes,  
        /// conversion to a ragdoll, etc.
        /// <para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        public void Destroy(DestroyType typ)
        {
            Destroy(typ, false, null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Utility Members

        /// <summary>
        /// Performs a standard search of the outfit for an Animator.
        /// </summary>
        /// <remarks>
        /// <para>An animator attached to the outfit's motion root takes precidence.</para>
        /// </remarks>
        /// <returns>The outfit's animator, or null if none was found.</returns>
        public Animator GetAnimator()
        {
            Animator result = null;

            if (MotionRoot)
                result = MotionRoot.GetComponent<Animator>();

            return result ? result : GetComponentInChildren<Animator>();
        }

        #endregion

#if UNITY_EDITOR

        #region Editor-only Members

        /// <summary>
        /// Add all UnityEngine.Objects that may change while performing outfit operations to the provided list. 
        /// (Including the outfit itself.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo can result in changes
        /// being lost.  When updating the outfit in the editor, this method will be used by the base class to 
        /// obtain a list of all known UnityEngine.Objects that may be impacted by changes to the outfit.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale in the editor.  Concrete implementaitons must place it
        /// inside a UNITY_EDITOR conditional compile section.
        /// </para>
        /// </remarks>
        /// <param name="list">The list to add objects to.  (Required)</param>
        protected abstract void GetUndoObjects(List<Object> list);

        /// <summary>
        /// Add all UnityEngine.Objects that may change while performing outfit operations to the provided list. 
        /// (Including the oufit itself.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo can result in changes
        /// being lost.  When updating the outfit in the editor, this method can be used to obtain a list of all 
        /// known UnityEngine.Objects that may be impacted by changes to the outfit.
        /// </para>
        /// <para>
        /// While not technically a 'part' of the outfit, the object list includes any detected Animator components.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale for use in the editor.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required.)</param>
        /// <param name="list">The list to add objects to.</param>
        /// <returns>
        /// The reference to <paramref name="list"/> if it is provided.  Otherwise a reference to a newly created list.
        /// </returns>
        public static List<Object> UnsafeGetUndoObjects(Outfit outfit, List<Object> list = null)
        {
            if (list == null)
                list = new List<Object>();

            outfit.GetUndoObjects(list);

            var anims = outfit.GetComponentsInChildren<Animator>();
            if (anims.Length > 0)
                list.AddRange(anims);

            return list;
        }

        #endregion

#endif
    }
}
