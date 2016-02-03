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
    /// A body outfit representing the physical presence of an agent.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The body outfit generally consists of the models, renderers, animators, colliders, 
    /// and other components the represent an agents physical presence in the scene.
    /// </para>
    /// <para>
    /// The functionality of an outfit can extended in one of two ways:  
    /// Class extension and through the use of <see cref="IOutfitObservers"/>s.  
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
        /// The primary collider for the outfit. (Optional)
        /// </summary>
        public abstract Collider PrimaryCollider { get; set; }

        /// <summary>
        /// The primary rigidbody for the outfit, or null if there is none.
        /// </summary>
        public abstract Rigidbody PrimaryRigidbody { get; }

        /// <summary>
        /// The owner of the outfit. (Optional.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an informational field.  It can be set to provide information about the
        /// the outfit owner.  E.g. The Body, Outfitter, agent, or object pool that currently
        /// controls the the outfit.
        /// </para>
        /// </remarks>
        public abstract GameObject Owner { get; set; }

        #endregion

        #region Body Parts

        /// <summary>
        /// The maximum number of body parts the outfit can have. [Limit: >= 0]
        /// </summary>
        /// <para>
        /// How body parts are managed and stored is implementation specific.  This value
        /// may be fixed or dynamic. It may represent a true buffer size, or simply the number
        /// of currently avaiable body parts.
        /// </para>
        public abstract int BodyPartBufferSize { get; }

        /// <summary>
        /// True if there is at least one body part.
        /// </summary>
        public abstract bool HasBodyParts { get; }

        /// <summary>
        /// The body part at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index">
        /// The index. [0 &lt;= value &lt <see cref="BodyPartBufferSize"/>]
        /// </param>
        /// <returns>
        /// The body part at the specified index, or null if there is none.
        /// </returns>
        public abstract BodyPart GetBodyPart(int index);

        /// <summary>
        /// Gets the body part associated with the specified type, or null if there is none.
        /// </summary>
        /// <param name="typ">The type.</param>
        /// <returns>
        /// The body part associated with the specified type, or null if there is none.
        /// </returns>
        public abstract BodyPart GetBodyPart(BodyPartType typ);

        /// <summary>
        /// Applies the colider status to all body parts.
        /// </summary>
        /// <param name="status">The desired status.</param>
        public abstract void ApplyBodyPartStatus(ColliderStatus status);

        /// <summary>
        /// Applies the layer all body parts.
        /// </summary>
        /// <param name="layer">The layer id.</param>
        public abstract void ApplyBodyPartLayer(int layer);

        #endregion

        #region Mount Points & Accessories

        /// <summary>
        /// If true, only accessories marked to ignore this flag can be successfully attached.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The purpose of this flag is to allow an outfit to be marked as not appropriate for
        /// most accessories.  Consider a full body space suit.  It is normally not appropriate
        /// for hats, glasses, etc. to be mounted to such an outfit.  So it is marked as limited.
        /// </para>
        /// </remarks>
        public abstract bool AccessoriesLimited { get; set; }

        /// <summary>
        /// The built-in coverage blocks for the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Built-in coverage blocks represent blocks owned by the outfit rather than accessories.
        /// Accessories that have coverage that overlaps the blocks will normally no be allowed
        /// to mount.
        /// </para>
        /// <para>
        /// Example:  An outfit with a built-in hat will have coverage blocks for the top of the 
        /// head.
        /// </para>
        /// </remarks>
        public abstract BodyCoverage CoverageBlocks { get; set; }

        /// <summary>
        /// The combined coverage from both <see cref="CoverageBlocks"/> and accessory coverage.
        /// </summary>
        public abstract BodyCoverage CurrentCoverage { get; }

        /// <summary>
        /// True if the there is a least one mount point.
        /// </summary>
        public abstract bool HasMountPoints { get; }

        /// <summary>
        /// The maximum number of mount points the outfit can have.
        /// </summary>
        /// <remarks>
        /// <para>
        /// How mount points are managed and stored is implementation specific.  This value
        /// may be fixed or dynamic. It may represent a true buffer size, or simply the number
        /// of currently avaiable mount points.
        /// </para>
        /// </remarks>
        public abstract int MountPointBufferSize { get; }

        /// <summary>
        /// Get the mount point at the specified index, or null if there is none.
        /// </summary>
        /// <para>
        /// Depending on the implementation, an index location may contain a null value.
        /// </para>
        /// <param name="index">
        /// The index. [0 &lt;= value &lt; <see cref="MountPointBufferSize"/>
        /// </param>
        /// <returns>
        /// The mount point at the specified index, or null if there is none.
        /// </returns>
        public abstract MountPoint GetMountPoint(int index);

        /// <summary>
        /// Gets the specified mount point, or null if there is none.
        /// </summary>
        /// <param name="locationType">The mount location.</param>
        /// <returns>The specified mount point, or null if there is none.</returns>
        public abstract MountPoint GetMountPoint(MountPointType locationType);

        /// <summary>
        /// The number of accessories mounted to the outfit.
        /// </summary>
        public abstract int AccessoryCount { get; }

        /// <summary>
        /// Get the accessory at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The return value will always be non-null unless the accessory is improperly destroyed
        /// while mounted.
        /// </para>
        /// </remarks>
        /// <param name="index">The index. [0 &lt;= value &lt; <see cref="AccessoryCount"/>]</param>
        /// <returns>The accessory at the specified index.</returns>
        public abstract Accessory GetAccessory(int index);

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore coverage restrictions and the value of <see cref="AccessoriesLimited"/>.
        /// </param>
        /// <param name="priorityMounter">The mounter that should be tried before any other
        /// mounters.  (A custom mounter.)  (Optional)</param>
        /// <param name="additionalCoverage">
        /// Coverage to add to the accessory if it is successfully mounted, above and beyond any 
        /// coverage inherent in the accessory.
        /// </param>
        /// <returns>The result of the mount attempt.</returns>
        public abstract MountStatus Mount(Accessory accessory, MountPointType locationType,
            bool ignoreRestrictions, AccessoryMounter priorityMounter, 
            BodyCoverage additionalCoverage);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Optional parameter key duplication bug workaround.

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore coverage restrictions and the value of <see cref="AccessoriesLimited"/>.
        /// </param>
        /// <param name="priorityMounter">The mounter that should be tried before any other
        /// mounters.  (A custom mounter.)  (Optional)</param>
        /// <returns>The result of the mount attempt.</returns>
        public MountStatus Mount(Accessory accessory, MountPointType locationType,
            bool ignoreRestrictions, AccessoryMounter priorityMounter)
        {
            return Mount(accessory, locationType, ignoreRestrictions, priorityMounter, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore coverage restrictions and the value of <see cref="AccessoriesLimited"/>.
        /// </param>
        /// <returns>The result of the mount attempt.</returns>
        public MountStatus Mount(Accessory accessory, MountPointType locationType,
            bool ignoreRestrictions)
        {
            return Mount(accessory, locationType, ignoreRestrictions, null, 0);
        }

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <param name="accessory">The accessory to mount. (Required)</param>
        /// <param name="locationType">The location to mount the accessory to.</param>
        /// <returns>The result of the mount attempt.</returns>
        public MountStatus Mount(Accessory accessory, MountPointType locationType)
        {
            return Mount(accessory, locationType, false, null, 0);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Unmount and release the the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The release operation will always succeed if the accessory is is known to the outfit.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory. (Required.)</param>
        /// <returns>
        /// True if an release occurred, false if the accessory is not known to the outfit.
        /// </returns>
        public abstract bool Release(Accessory accessory);

        #endregion

        #region Observers

        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All standard implementations require the observer to be a Unity Object for
        /// serialization purposes, so it is acceptable for an outfit to reject an observer.  An
        /// error message will be logged if an observer is rejected.
        /// </para>
        /// </para>
        /// <para>
        /// An observer can only be added once.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required)</param>
        /// <returns>
        /// True if the observer was accepted or already added.  False if the observer was rejected.
        /// </returns>
        public abstract bool AddObserver(IOutfitObserver observer);

        /// <summary>
        /// Remove the specified event listener.
        /// </summary>
        /// <param name="observer">The observer to remove. (Required)</param>
        public abstract void RemoveObserver(IOutfitObserver observer);

        #endregion

        #region Renderers

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
        /// Gets the specified shared material, or null if it is not defined.
        /// </summary>
        /// <param name="typ">The material type.</param>
        /// <returns>
        /// The specified shared material, or null if it is not defined.
        /// </returns>
        public abstract Material GetSharedMaterial(OutfitMaterialType typ);

        /// <summary>
        /// Sets the specified shared material.
        /// </summary>
        /// <param name="typ">The material type.</param>
        /// <param name="material">The material to apply. (Required.)</param>
        /// <returns>True if the material was successfully applied.</returns>
        public abstract int ApplySharedMaterial(OutfitMaterialType typ, Material material);

        #endregion

        #region Pooling
        //TODO: POOLING


        //public abstract PoolingType PoolingType { get; set; }
        //public abstract void SetPoolingType(PoolingType value, bool safe = true);

        //public abstract int PoolingId { get; set; }  // Don't use 'group' in the name.  That is a pooling type.

        #endregion

        #region Miscellaneous

        //////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Destroy the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: Only the owner of the oufit should call this method.
        /// </para>
        /// <para>
        /// This is the best way of destroying an outfit since the outfit will send
        /// events to its observers and other associated components so they can properly
        /// respond.
        /// </para>
        /// <para>
        /// The component is responsible for destroying itself as appropriate.  So
        /// the client only needs to call this method then dispose of its references to the 
        /// component.
        /// </para>
        /// <para><strong>Baking</strong>
        /// </para>
        /// Baking is the process of converting the outfit into a non-outfit state.  What
        /// exactly happens during the bake is implemenation specific.  It may result in
        /// the baking of skinned meshes into static meshes.  It may result in conversion to
        /// a ragdoll configuration. Etc.
        /// <para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        /// <param name="referenceOutfit">
        /// The outfit that the the current outfit is derived from.  (E.g. Was instanced from.)
        /// Or null if the outfit has no known source. (Only applies to the 'bake' type.)
        /// </param>
        public abstract void Destroy(DestroyType typ, Outfit referenceOutfit);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Optional parameter key duplication bug workaround.

        /// <summary>
        /// Destroy the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: Only the owner of the oufit should call this method.
        /// </para>
        /// <para>
        /// This is the best way of destroying an outfit since the outfit will send
        /// events to its observers and other associated components so they can properly
        /// respond.
        /// </para>
        /// <para>
        /// The component is responsible for destroying itself as appropriate.  So
        /// the client only needs to call this method then dispose of its references to the 
        /// component.
        /// </para>
        /// <para><strong>Baking</strong>
        /// </para>
        /// Baking is the process of converting the outfit into a non-outfit state.  What
        /// exactly happens during the bake is implemenation specific.  It may result in
        /// the baking of skinned meshes into static meshes.  It may result in conversion to
        /// a ragdoll configuration. Etc.
        /// <para>
        /// </remarks>
        /// <param name="typ">The type of destruction.</param>
        public void Destory(DestroyType typ)
        {
            Destroy(typ, null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Utilities

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

        /// <summary>
        /// Implements a safe, standard way of setting a the outfit's body part layers, 
        /// if any body parts exist.
        /// </summary>
        /// <param name="outfit">The outit. (Required)</param>
        /// <param name="layer">The desired layer.</param>
        /// <returns>The clamped layer.</returns>
        public static int SetBodyPartLayer(Outfit outfit, int layer)
        {
            layer = Mathf.Max(0, layer);

            if (outfit)
                outfit.ApplyBodyPartLayer(layer);

            return layer;
        }

        #endregion
    }
}
