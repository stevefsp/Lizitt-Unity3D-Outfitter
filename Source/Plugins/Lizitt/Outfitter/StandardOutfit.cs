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
    [AddComponentMenu(LizittUtil.LizittMenu + "Standard Outfit", OutfitterUtil.BaseMenuOrder + 0)]
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

        // TODO: Update editor so it includes mount point type labels for each entry.

        [SerializeField]
        [ObjectList("Body Parts")]
        private BodyPartGroup m_Parts = new BodyPartGroup(0);         // Refactor note: Field name used in the editor.

        public sealed override int BodyPartCount
        {
            get { return m_Parts.BufferSize; }
        }

        public sealed override BodyPart GetBodyPart(int index)
        {
            return m_Parts[index];
        }

        /// <summary>
        /// Replaces the current body parts with the provided body parts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization.
        /// </para>
        /// <para>
        /// If <paramref name="asReference"/> is true, then all external references to the body part array must be
        /// discared or behavior will be undefined.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="asReference">
        /// If true the outfit will use the reference to the body part rray.  Otherwise the array will be copied.
        /// </param>
        /// <param name="mountPoints">The body parts, or null to clear all body parts.</param>
        public static void UnsafeSet(StandardOutfit outfit, bool asReference, params BodyPart[] bodyParts)
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
        /// If <paramref name="replace"/> is false, the order of currently defined body parts will be preserved.  New
        /// body parts will be added to the end of the body part list.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="replace">
        /// If true, all current body parts will be cleared and replaced with the result of the refresh.  Otherwise
        /// only newly detected body parts will be added.
        /// </param>
        /// <returns>The number of body parts added, or all  if <paramref name="replace"/> is true.</returns>
        public static int UnsafeRefreshBodyParts(StandardOutfit outfit, bool replace = false)
        {
            var items = outfit.GetComponentsInChildren<BodyPart>();

            if (replace)
            {
                UnsafeSet(outfit, true, items);
                return items.Length;
            }

            var before = outfit.m_Parts.Count;

            outfit.m_Parts.CompressAndAdd(items);

            return outfit.m_Parts.Count - before;
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
                    Debug.LogError("Outfit: Blend shape renderer is not a child of this outfit: " + value.name, this);
                    return;
                }

                m_BlendRenderer = value;
            }
        }

        [Space(5)]

        [SerializeField]
        [OutfitMaterialTargetGroup(true)]
        [UnityEngine.Serialization.FormerlySerializedAs("m_OutfitMaterials")]
        private OutfitMaterialTargetGroup m_OutfitMaterialTargets = new OutfitMaterialTargetGroup();  // Refactor note: Field name used in the editor.

        public sealed override Material GetSharedMaterial(OutfitMaterialType typ)
        {
            return m_OutfitMaterialTargets.GetSharedMaterial(typ);
        }

        public override int OutfitMaterialCount
        {
            get { return m_OutfitMaterialTargets.Count; }
        }

        public override OutfitMaterial GetSharedMaterial(int index)
        {
            return m_OutfitMaterialTargets[index].SharedOutfitMaterial;
        }

        public sealed override int ApplySharedMaterial(OutfitMaterialType typ, Material material)
        {
            return m_OutfitMaterialTargets.ApplySharedMaterial(typ, material);
        }

        public sealed override bool IsMaterialDefined(OutfitMaterialType typ)
        {
            return m_OutfitMaterialTargets.IsDefined(typ);
        }

        public override OutfitMaterialType[] GetOutfitMaterialTypes()
        {
            return m_OutfitMaterialTargets.GetMaterialTypes();
        }

        /// <summary>
        /// Sets the specified outfit material target, or adds a new material target if the
        /// type does not exist.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is meant for use during configuration of the outfit.  It is not normal to change material
        /// targets while the outfit is in use.
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

            outfit.m_OutfitMaterialTargets.AddTarget(typ, target);
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
            get { return m_Accessories.Count; }
        }

        public sealed override Accessory GetAccessory(int index)
        {
            return m_Accessories[index];
        }

        public sealed override MountResult Mount(Accessory accessory, MountPointType locationType, 
            bool ignoreRestrictions, IAccessoryMounter priorityMounter, BodyCoverage additionalCoverage)
        {
            // Error checks are optimized with the assumption that the mount will succeed.
            // Remounts to the same location are allowed.  (Mounter may have changed or may have special 
            // remount behavior.)

            if (!ignoreRestrictions)
            {
                if (AccessoriesLimited && !accessory.IgnoreLimited)
                {
                    Release(accessory);
                    return MountResult.OutfitIsLimited;
                }


                var currentCoverage = CurrentCoverage;
                if (m_Accessories.Contains(accessory))
                    currentCoverage &= ~accessory.CurrentCoverage;

                if (((accessory.GetCoverageFor(locationType) | additionalCoverage) & currentCoverage) != 0)
                {
                    Release(accessory);
                    return MountResult.CoverageBlocked;
                }
            }

            var location = GetMountPoint(locationType);
            if (!location)
            {
                Release(accessory);
                return MountResult.NoMountPoint;
            }
            else if (location.IsBlocked)
            {
                Release(accessory);
                return MountResult.LocationBlocked;
            }

            if (priorityMounter != null && LizittUtil.IsUnityDestroyed(priorityMounter))
            {
                Debug.LogError("The priority mounter is a reference to a destroyed object.", this);
                Release(accessory);
                return MountResult.FailedOnError;
            }

            if (!accessory.Mount(location, gameObject, priorityMounter, additionalCoverage))
            {
                Release(accessory);
                return MountResult.RejectedByAccessory;
            }

            LinkAccessory(accessory);

            Observers.SendMount(this, accessory);

            return MountResult.Success;
        }

        public sealed override bool Release(Accessory accessory)
        {
            if (UnlinkAccessory(accessory))
            {
                accessory.Release();
                SendUnmount(accessory);

                return true;
            }

            return false;
        }

        private void SendUnmount(Accessory accessory)
        {
            Observers.SendRelease(this, accessory);
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

        void IAccessoryObserver.OnStateChange(Accessory sender)
        {
            bool release = false;

            switch (sender.Status)
            {
                case AccessoryStatus.Mounted:
                case AccessoryStatus.Mounting:

                    release = !(sender.Owner == gameObject && Contains(sender.CurrentLocation));

                    break;

                default:

                    release = true;
                    break;
            }

            if (release)
            {
                UnlinkAccessory(sender);
                SendUnmount(sender);
            }
        }

        void IAccessoryObserver.OnDestroy(Accessory sender, DestroyType typ)
        {
            if (typ == DestroyType.Bake)
            {
                Debug.LogError("Released accessory: Accessory baked, but not by this outfit: "
                    + sender.name, this);
            }

            UnlinkAccessory(sender);
            SendUnmount(sender);
        }

        #endregion

        #region Utility Features

        // Note: Static utility members specific to a feature colocated with the feature.

        public override void Destroy(DestroyType typ, bool prepareOnly, Outfit referenceOutfit)
        {
            Observers.SendDestroy(this, typ, referenceOutfit);

            // Don't worry about unknown assessories/components added improperly to the outfit. If that is 
            // important to the user, the user can create a bake extension to handle it.

            for (int i = 0; i < m_Accessories.Count; i++)
            {
                if (m_Accessories[i])
                {
                    m_Accessories[i].RemoveObserver(this);
                    m_Accessories[i].Destroy(typ);
                }
            }

            Reset();

            if (!prepareOnly)
            {
                if (typ == DestroyType.GameObject)
                    gameObject.SafeDestroy();
                else
                    this.SafeDestroy();
            }
        }

        protected override void Reset()
        {
            m_BlendRenderer = null;
            m_OutfitMaterialTargets = new OutfitMaterialTargetGroup();  // Materials Reset() is not as safe.
            m_Parts.Clear(0);
            m_Accessories.Clear();
            Observers.Clear();
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
        /// Will attempt to locate and assign the MotionRoot, primary collider, body parts, and mount points, 
        /// and observers.  Lists will be updated such that the order of existing items will not change.
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
            RefreshObservers(outfit);
        }

        #endregion

#if UNITY_EDITOR

        #region Editor Only

        protected override void GetUndoObjects(System.Collections.Generic.List<Object> list)
        {
            base.GetUndoObjects(list);

            for (int i = 0; i < m_Parts.Count; i++)
            {
                var item = m_Parts[i];
                if (item)
                {
                    list.Add(item);
                    if (item.Collider)
                    {
                        list.Add(item.Collider);
                        list.Add(item.Collider.gameObject);  // For layer changes.
                    }
                    if (item.Rigidbody)
                        list.Add(item.Rigidbody);
                }
            }

            foreach (var item in m_Accessories)
            {
                if (item)
                    Accessory.UnsafeGetUndoObjects(item, list);
            }

            if (m_BlendRenderer)
                list.Add(m_BlendRenderer);

            for (int i = 0; i < m_OutfitMaterialTargets.Count; i++)
            {
                var item = m_OutfitMaterialTargets[i];

                if (item.Target != null && item.Target.Renderer)
                    list.Add(item.Target.Renderer);
            }
        }

        #endregion

#endif
    }
}
