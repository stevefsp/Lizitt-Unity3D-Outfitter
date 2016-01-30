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
using System.Collections.Generic;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An generalized outfit that implements all of the standard outfit behavior.
    /// </summary>
    public class StandardOutfit
        : OutfitCore, IAccessoryObserver
    {
        /*
         * Design notes:
         * 
         * Custom editors exist for this class.  In order to reduce hard coded field names,
         * the fields are organized into sections. Take care when refactoring field names that
         * are marked as being used in the editor, and make sure you understand the custom
         * editor design before rearranging, adding, or deleting fields.
         */

        #region Body Parts (Editor Section)

        public sealed override void ApplyBodyPartStatus(ColliderStatus status)
        {
            CheckInitializeBodyParts();
            m_Parts.ApplyStatusToAll(status);
        }

        public sealed override void ApplyBodyPartLayer(int layer)
        {
            CheckInitializeBodyParts();
            m_Parts.ApplyLayerToAll(layer);
        }

        [SerializeField]
        [Tooltip("Perform a refresh of the body parts during outfit initialization."
            + " (Flexible, but less efficient than assigning body parts at design time.)")]
        private bool m_AutoLoadParts = false;    // Refactor note: Field name used in the editor.

        /// <summary>
        /// If true, a refresh of the body parts will ocucr during outfit initialization.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Refreshing at run-time is flexible, but less efficient than assigning mount points at 
        /// design time.
        /// </para>
        /// </remarks>
        public bool AutoLoadBodyParts
        {
            get { return m_AutoLoadParts; }
            set { m_AutoLoadParts = value; }
        }

        [Space(10)]

        [SerializeField]
        [Tooltip("Outfit body parts.")]
        private BodyPartGroup m_Parts = new BodyPartGroup(0);         // Refactor note: Field name used in the editor.

        public sealed override bool HasBodyParts
        {
            get 
            {
                CheckInitializeBodyParts();
                return m_Parts.HasItem; 
            }
        }

        public sealed override int BodyPartBufferSize
        {
            get
            {
                CheckInitializeBodyParts(); 
                return m_Parts.BufferSize;
            }
        }

        public sealed override BodyPart GetBodyPart(int index)
        {
            CheckInitializeBodyParts();
            return m_Parts[index];
        }

        public sealed override BodyPart GetBodyPart(BodyPartType typ)
        {
            CheckInitializeBodyParts();
            return m_Parts[typ];
        }

        // Safe to re-run.  So don't need to serialize.
        private bool m_IsPartsInitialized = false;

        private void CheckInitializeBodyParts()
        {
            if (m_IsPartsInitialized)
                return;

            m_IsPartsInitialized = true;

            if (m_AutoLoadParts)
                UnsafeRefreshBodyParts(this, false);

            m_Parts.SetOwnership(gameObject, true);
        }

        /// <summary>
        /// Replaces the current body parts with the provided body parts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization.
        /// </para>
        /// <para>
        /// If <paramref name="asReference"/> is true, then all external references to the
        /// body part array must be discared or behavior will be undefined.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="asReference">If true the outfit will use the reference to the body part
        /// array.  Otherwise the array will be copied.</param>
        /// <param name="mountPoints">The body parts, or null to clear all body parts.</param>
        public static void UnsafeSet(
            StandardOutfit outfit, bool asReference, params BodyPart[] bodyParts)
        {
            BodyPartGroup.UnsafeReplaceItems(outfit.m_Parts, asReference, bodyParts);
        }

        /// <summary>
        /// Clear all body parts.
        /// </summary>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization.
        /// </para>
        /// <param name="outfit">The outfit. (Required.)</param>
        public static void UnsafeClearBodyParts(StandardOutfit outfit)
        {
            outfit.m_Parts.Clear(0);
        }

        /// <summary>
        /// Refreshes the body parts by performing a child search.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization.
        /// </para>
        /// <para>
        /// If <paramref name="replace"/> is false, the order of currently defined body parts
        /// will be preserved.  New body parts will be added to the end of the body part list.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="replace">
        /// If true, all current body parts will be cleared and replaced with the result of
        /// the refresh.  Otherwise only newly detected body parts will be added.
        /// </param>
        /// <returns>
        /// The number of body parts added, or all  if <paramref name="replace"/> is true.
        /// </returns>
        public static int UnsafeRefreshBodyParts(StandardOutfit outfit, bool replace = false)
        {
            var items = outfit.GetComponentsInChildren<BodyPart>();

            if (replace)
            {
                UnsafeSet(outfit, true, items);
                return items.Length;
            }

            var before = outfit.m_Parts.ItemCount;

            outfit.m_Parts.CompressAndAdd(items);

            return outfit.m_Parts.ItemCount - before;
        }

        #endregion

        #region Renderers & Materials (Editor Section)

        [SerializeField]
        [Tooltip("The renderer that contains the outfit's blend shapes.  (Optional)")]
        [LocalComponentPopupAttribute(typeof(Renderer))]
        private SkinnedMeshRenderer m_BlendRenderer;         // Refactor note: Field name used in the editor.

        public sealed override SkinnedMeshRenderer BlendShapeRenderer
        {
            get { return m_BlendRenderer; }
            set
            {
                if (!value)
                {
                    m_BlendRenderer = null;
                    return;
                }

                // The safest fallback for a failure is to set the current renderer to null.

                if (!(value.sharedMesh && value.sharedMesh.blendShapeCount > 0))
                {
                    // Allowing a non-shape renderer is too much of a risk, even if the shared
                    // mesh is going to be build out later.

                    m_BlendRenderer = null;

                    Debug.LogError("Outfit: Blend shape renderer does not contain any blend shapes: "
                        + value.name, this);

                    return;
                }

                var check = value.GetComponentInParent<OutfitCore>();
                if (check != this)
                {
                    m_BlendRenderer = null;

                    Debug.LogError("Outfit: Blend shape renderer is not a child of this outfit: "
                        + value.name, this);

                    return;
                }

                m_BlendRenderer = value;
            }
        }

        [Space(5)]

        [SerializeField]
        [OutfitMaterials(true)]
        private OutfitMaterialGroup m_OutfitMaterials = new OutfitMaterialGroup();         // Refactor note: Field name used in the editor.

        public sealed override Material GetSharedMaterial(OutfitMaterialType typ)
        {
            return m_OutfitMaterials.GetSharedMaterial(typ);
        }

        public sealed override int ApplySharedMaterial(OutfitMaterialType typ, Material material)
        {
            return m_OutfitMaterials.ApplySharedMaterial(typ, material);
        }

        public sealed override bool IsMaterialDefined(OutfitMaterialType typ)
        {
            return m_OutfitMaterials.IsDefined(typ);
        }

        /// <summary>
        /// Sets the specified outfit material target, or adds a new material target if the
        /// type does not exist.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is meant for use during configuration of the outfit.  It is not
        /// normal to change material targets while the outfit is in use.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="typ">The material type.</param>
        /// <param name="target">The new target for the material type.</param>
        public static void AddMaterialTarget(
            StandardOutfit outfit, OutfitMaterialType typ, RendererMaterialPtr target)
        {
            // Design note: This is a class member because its use is expected to be limited
            // to outfit initialization.  Don't want to unnecessarily clutter the instance API.

            outfit.m_OutfitMaterials.AddTarget(typ, target);
        }

        #endregion

        #region Accessory Features

        public override BodyCoverage CurrentCoverage
        {
            // Allow extension.
            get
            {
                var result = base.CurrentCoverage;

                for (int i = 0; i < m_Accessories.Count; i++)
                {
                    if (m_Accessories[i])
                        result |= m_Accessories[i].CurrentCoverage;
                }

                return result;
            }
        }

        [SerializeField]
        [HideInInspector]
        private List<Accessory> m_Accessories = new List<Accessory>(4);

        public sealed override int AccessoryCount
        {
            get 
            {
                CheckInitializedAccessories();
                return m_Accessories.Count; 
            }
        }

        public sealed override Accessory GetAccessory(int index)
        {
            CheckInitializedAccessories();
            return m_Accessories[index];
        }

        // Ok to re-run.  No need to serialize.
        private bool m_AccessoriesInitialized = false;
        private void CheckInitializedAccessories()
        {
            if (!m_AccessoriesInitialized)
            {
                m_AccessoriesInitialized = true;
                m_Accessories.PurgeNulls();
            }
        }

        public sealed override MountStatus Mount(Accessory accessory, MountPointType locationType,
            bool ignoreRestrictions, AccessoryMounter priorityMounter, 
            BodyCoverage additionalCoverage)
        {
            // Error checks are optimized with the assumption that the mount will succeed.

            if (m_Accessories.Contains(accessory))
            {
                Debug.LogWarning(
                    "Attempted to attach same accessory more than once.  Attempt ignored: "
                    + accessory.name);

                // It is a success since the accessory is mounted. But no event.
                return MountStatus.Success;
            }

            if (!ignoreRestrictions)
            {
                if (AccessoriesLimited && !accessory.IgnoreLimited)
                    return MountStatus.OutfitIsLimited;

                if (((accessory.GetCoverageFor(locationType) | additionalCoverage) 
                    & CurrentCoverage) != 0)
                {
                    return MountStatus.CoverageBlocked;
                }
            }

            var mountPoint = GetMountPoint(locationType);
            if (!mountPoint)
                return MountStatus.NoMountPoint;
            else if (mountPoint.IsBlocked)
                return MountStatus.LocationBlocked;

            //Debug.Log("ACCMR: " + !accessory.Mount(mountPoint, priorityMounter, additionalCoverage));
            if (!accessory.Mount(mountPoint, priorityMounter, additionalCoverage))
                return MountStatus.RejectedByAccessory;

            LinkAccessory(accessory);

            Observers.SendMount(this, accessory);

            return MountStatus.Success;
        }

        public sealed override bool Unmount(Accessory accessory, AccessoryMounter priorityMounter)
        {
            if (UnlinkAccessory(accessory))
            {
                accessory.Unmount(priorityMounter);
                SendUnmount(accessory);

                return true;
            }

            return false;
        }

        private void SendUnmount(Accessory accessory)
        {
            Observers.SendUnmount(this, accessory);
        }

        private void LinkAccessory(Accessory accessory)
        {
            accessory.AddObserver(this);
            m_Accessories.Add(accessory);
        }

        private bool UnlinkAccessory(Accessory accessory)
        {
            if (m_Accessories.Remove(accessory))
            {
                accessory.RemoveObserver(this);
                return true;
            }

            return false;
        }

        #endregion

        #region Accessory Observer

        void IAccessoryObserver.OnStatusChange(Accessory sender, AccessoryStatus status)
        {
            switch (status)
            {
                case AccessoryStatus.NotMounted:
                case AccessoryStatus.Unmounting:

                    // An auto-unmount occurred.
                    UnlinkAccessory(sender);
                    SendUnmount(sender);

                    break;

                case AccessoryStatus.Invalid:
                case AccessoryStatus.Stored:

                    Debug.LogErrorFormat(this, 
                        "Released accessory: Invalid status change, not by this outfit: {0}"
                        + " (Status: {1})", sender.name, status);

                    UnlinkAccessory(sender);
                    SendUnmount(sender);

                    break;
            }
        }

        void IAccessoryObserver.OnBake(Accessory sender)
        {
            Debug.LogError("Released accessory: Accessory baked, but not by this outfit: " 
                + sender.name, this);

            UnlinkAccessory(sender);
            SendUnmount(sender);
        }

        void IAccessoryObserver.OnBakePost(GameObject gameObject)
        {
        }

        #endregion

        #region Utility Features

        // Note: Static utility members specific to a feature colocated with the feature.

        /// <summary>
        /// Deletes all outfit related sub-components.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bake process runs as follows:
        /// </para>
        /// <para>
        /// <ol>
        /// <li>
        /// Notify all observers that a bake is being started to allow them to perform
        /// custom bake operations.
        /// </li>
        /// <li>
        /// Bake all accessories.
        /// </li>
        /// <li>
        /// Invalidate the outfit.  (Outfit is no longer useable.)
        /// </li>
        /// <li>
        /// Notify all observers that the bake operation is complete to allow them to do any
        /// required cleanup.
        /// </li>
        /// <li>
        /// Destroy self.
        /// </li>
        /// </ol>
        /// </para>
        /// </remarks>
        public override void Bake(Outfit source)
        {
            Observers.SendBake(this, source);

            // Don't worry about unknown assessories/components added improperly to the outfit.
            // If that is important to the user, the user can create a bake extension to handle it.

            for (int i = 0; i < m_Accessories.Count; i++)
            {
                if (m_Accessories[i])
                {
                    m_Accessories[i].RemoveObserver(this);
                    m_Accessories[i].Bake();
                }
            }

            m_Accessories.Clear();

            Reset();  // Invalidates the outfit, except for the observers.

            Observers.SendBakePost(gameObject);
            Observers.Clear();

            this.SafeDestroy();
        }

        protected override void Reset()
        {
            m_BlendRenderer = null;
            m_OutfitMaterials = new OutfitMaterialGroup();  // Materials Reset() is not as safe.
            m_Parts.Clear(0);
            base.Reset();
        }

        /// <summary>
        /// Attempts to auto-detect settings using standard component searches.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Using this method after outfit initialization can result in undefined behavior.
        /// </para>
        /// <para>
        /// The MotionRoot is guarenteed to be assigned after this process completes.
        /// </para>
        /// <para>
        /// Will attempt to locate and assign the MotionRoot, primary collider, body parts, and
        /// mount points, and observers.  Lists will be updated such that the order of existing
        /// items will not change.
        /// </para>
        /// <para>
        /// Warning: Will not attempt to check the validity in existing settings. 
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required.)</param>
        public static void UnsafeRefreshAllSettings(StandardOutfit outfit)
        {
            var outfitAnimator = outfit.GetAnimator();

            if (!outfit.MotionRoot)
            {
                var motionRoot = outfitAnimator
                    ? outfitAnimator.transform
                    : outfit.transform;

                UnsafeSetMotionRoot(outfit, motionRoot);
            }

            if (!outfit.PrimaryCollider)
            {
                var collider = outfit.MotionRoot.GetComponent<Collider>();  // Not a child search.

                if (collider && collider.GetAssociatedRigidBody())
                    outfit.PrimaryCollider = collider;
            }

            if (!outfit.BlendShapeRenderer)
            {
                foreach (var item in outfit.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (item.sharedMesh && item.sharedMesh.blendShapeCount > 0)
                    {
                        outfit.BlendShapeRenderer = item;
                        break;
                    }
                }
            }

            UnsafeRefreshBodyParts(outfit, false);
            UnsafeRefreshMountPoints(outfit, false);
            outfit.RefreshObservers();
        }

        #endregion

        #region Context Menus

        [ContextMenu("Refresh All Settings")]
        private void AutoDetectSettings()
        {
            UnsafeRefreshAllSettings(this);
        }

        [ContextMenu("Refresh Mount Points")]
        private void RefreshMountPoints_Menu()   // Suffix prevents name clash in editor.
        {
            if (UnsafeRefreshMountPoints(this, false) == 0)
                Debug.Log("Refresh mount points: No new mount points found.", this);
        }

        [ContextMenu("Refresh Body Parts")]
        private void RefreshBodyParts_Menu()   // Suffix prevents name clash in editor.
        {
            if (UnsafeRefreshBodyParts(this, false) == 0)
                Debug.Log("Refresh body parts: No new body parts found.", this);
        }

        [ContextMenu("Refresh Observers")]
        private void RefreshObservers_Menu()
        {
            // Design note: This process is designed to support observers that have been linked
            // from other game objects.

            if (RefreshObservers() == 0)
                Debug.Log("Refresh outfit observers: No new obserers found.", this);
        }

        private int RefreshObservers()
        {
            Observers.PurgeNulls();

            var refreshItems = this.GetComponents<IOutfitObserver>();

            var count = 0;
            if (refreshItems.Length > 0)
            {
                // Add new items to end.
                foreach (var refreshItem in refreshItems)
                {
                    if (!Observers.Contains(refreshItem))
                    {
                        Observers.Add(refreshItem);
                        count++;
                    }
                }
            }

            return count;
        }

        [ContextMenu("Reset Mount Points")]
        private void ResetMountPoints()
        {
            UnsafeClearMountPoints(this);
        }

        [ContextMenu("Reset Body Parts")]
        private void ResetBodyParts()
        {
            UnsafeClearBodyParts(this);
        }

        [ContextMenu("Reset Observers")]
        private void ResetObservers()
        {
            Observers.Clear();
        }

        #endregion

        #region Pooling

        // TODO: POOLING
        // TOOD: EVAL: Should pooling be moved to the OutfitCore.

        //[Header("Pooling")]

        //[SerializeField]
        //private int m_PoolingId;
        //public override int PoolingId
        //{
        //    get { return m_PoolingId; }
        //    set { m_PoolingId = value; }
        //}

        //[SerializeField]
        //private PoolingType m_PoolingMode;
        //public override PoolingType PoolingType
        //{
        //    get { return m_PoolingMode; }
        //    set { m_PoolingMode = value; }
        //}

        //public override void SetPoolingType(PoolingType value, bool safe = true)
        //{
        //    if (safe && value.IsLessRestrictiveThan(PoolingType))
        //        return;

        //    PoolingType = value;
        //}

        #endregion
    }
}
