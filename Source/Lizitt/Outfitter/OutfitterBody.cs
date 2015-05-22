/*
 * Copyright (c) 2015 Stephen A. Pratt
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
using com.lizitt.u3d;
using System.Collections.Generic;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// A persistent agent body used to manage outfits and accessories.
    /// </summary>
    public abstract class OutfitterBody
        : MonoBehaviour
    {
        /*
         * TODO: EVAL: Potential body features.
         * 
         * Ability to set a custom accessory iterator for use when attaching stored accessories.  
         * Let's the user add more complex decision making on when, and which order, to add 
         * accessories.
         * 
         * Ability to detach an accessory based on body coverage.  This would allow user to
         * remove lower priority attachments before adding a high priority attachment.
         * The difficulty will be communicating when the block is permanent, and to allow the
         * currently attached accessory to refuse.  (Or implement attachment priority?)
         */

        #region Elements

        /// <summary>
        /// Body related information.
        /// </summary>
        protected struct Info
        {
            /// <summary>
            /// The motion root when there is no outfit.
            /// </summary>
            public Transform defaultMotionRoot;

            /// <summary>
            /// The surface collider layer.
            /// </summary>
            public int surfaceColliderLayer;

            /// <summary>
            /// The layer of the body colliders.
            /// </summary>
            public int bodyColliderLayer;

            /// <summary>
            /// The status of the body colliders.
            /// </summary>
            public BodyColliderStatus bodyColliderStatus;

            /// <summary>
            /// The body material overrides for all outfits.
            /// </summary>
            public BodyMaterialOverrides materialOverrides;

            /// <summary>
            /// The accessories meant to be applied to all outfits.
            /// </summary>
            public BodyAccessory[] accessories;
        }

        #endregion

        #region Delegates

        /// <summary>
        /// An action related to a body.
        /// </summary>
        /// <param name="body">The body the action relates to.</param>
        public delegate void BodyAction(OutfitterBody body);

        #endregion

        #region Fields and Events

        // Note: Using the prefix "Base" on some of the fields so they won't clash with serialized 
        // versions in concrete classes.

        private int m_BaseSurfaceColliderLayer = 0;
        private int m_BaseBodyColliderLayer;
        private BodyColliderStatus m_BaseBodyColliderStatus;

        private BodyMaterialOverrides m_BaselMaterialOverrides;

        private BodyOutfit m_Outfit;

        /// <summary>
        /// Doubles as check for initialization.
        /// </summary>
        private AccessoryManager m_Manager = null;

        private Transform m_MotionRoot = null;

        private Transform m_BaseDefaultMotionRoot = null;

        /// <summary>
        /// Sent before an outfit change operation is started.
        /// </summary>
        public event BodyAction OnOutfitChangePre;

        /// <summary>
        /// Sent after an outfit change operation is complete.
        /// </summary>
        public event BodyAction OnOutfitChangePost;

        /// <summary>
        /// Sent on any animator change.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is sent during <see cref="SetAnimatorController"/> and
        /// any time an outfit is changed.
        /// </para>
        /// </remarks>
        public event BodyAction OnAnimatorChange;

        #endregion

        #region Protected API

        /// <summary>
        /// Called at the end <see cref="Initialize"/>
        /// </summary>
        protected virtual void LocalInitializePost()
        {
        }

        /// <summary>
        /// Called before an outfit change is applied.
        /// </summary>
        protected virtual void LocalOutfitChangePre()
        {
        }

        /// <summary>
        /// Called after an outfit change has occurred.
        /// </summary>
        protected virtual void LocalOutfitChangePost()
        {
        }

        #endregion

        #region General Properties & Methods

        /// <summary>
        /// True if the body has an outfit.
        /// </summary>
        public bool HasOutfit
        {
            get { return m_Outfit; }
        }

        /// <summary>
        /// The current outfit. (May be null.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Warning:  Mutating the body directly can result in undefined behavior.
        /// </para>
        /// </remarks>
        public BodyOutfit Outfit
        {
            get { return m_Outfit; }
        }

        /// <summary>
        /// The current outfit's body accessory coverage, or zero if there is no outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return 0 if there is no outfit.
        /// </para>
        /// </remarks>
        public BodyCoverage Coverage
        {
            get { return m_Outfit ? m_Outfit.Coverage : 0; }
        }

        /// <summary>
        /// If true, only accessories marked to ignore this flag can be successfully attached.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return true if there is no outfit.
        /// </para>
        /// </remarks>
        public bool AccessoriesLimited
        {
            get
            {
                // If there is no body, the the behavior is 'limited accessories'.  And no
                // mount points.
                return m_Outfit ? m_Outfit.AccessoriesLimited : true;
            }
        }

        /// <summary>
        /// The accessories added to the body. (Default and added.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the count of the body accessories, both attached and stored.  Does not 
        /// include permanent outfit accessories or accessories attached directly via
        /// <see cref="Outfit"/>.
        /// </para>
        /// </remarks>
        public int AccessoryCount
        {
            get { return m_Manager.Count; }
        }

        /// <summary>
        /// An enumerator of all transient body accessories.  (Attached and stored.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Does not include permanent outfit accessories or accessories attached directly
        /// via <see cref="Outfit"/>.
        /// </para>
        /// </remarks>
        public IEnumerable<BodyAccessory> Accessories
        {
            get { return m_Manager; }
        }

        /// <summary>
        /// The transform that controls body motion and represents its location in the world.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is an outfit, this is the same as the outfit's <see cref="Outfit.MotionRoot"/>.
        /// If there is no outfit then this is the default motion root for the body.
        /// </para>
        /// <para>
        /// The body is guarenteed to have a motion transform after initialization, even when it 
        /// doesn't have an outfit.  Only an improperlay managed body will ever have a null
        /// value after awake.
        /// </para>
        /// <para>
        /// Important: This is a dynamic value.  The value changes whenever the outfit is 
        /// changed.
        /// </para>
        /// </remarks>
        public Transform MotionRoot
        {
            get { return m_MotionRoot; }
        }

        /// <summary>
        /// The current outfit's animator, or null if it doesn't exist.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: This is a dynamic value.  The value changes whenever the outfit is changed.
        /// </para>
        /// <para>
        /// Important: Do not set an outfit's animator controller directly via <see cref="Outfit"/>.
        /// Use <see cref="SetAnimatorController"/>.
        /// </para>
        /// </remarks>
        public Animator Animator
        {
            get { return m_Outfit ? m_Outfit.Animator : null; }
        }

        /// <summary>
        /// Sets the animator on the current outfit.
        /// </summary>
        /// <para>
        /// Will only apply the controller if it is different from the current controller.
        /// </para>
        /// <param name="controller">The controller to apply.</param>
        public void SetAnimatorController(RuntimeAnimatorController controller)
        {
            if (!Animator)
            {
                Debug.LogError("Can't set animator controller.  No animator available.", this);
                return;
            }

            if (Animator.runtimeAnimatorController == controller)
                return;

            Animator.runtimeAnimatorController = controller;

            if (OnAnimatorChange != null)
                OnAnimatorChange(this);
        }

        /// <summary>
        /// Gets the specified mount point, or null if the mount point does not exist.
        /// </summary>
        /// <param name="mountPoint">The mount point to retrieve.</param>
        /// <returns>The specified mount point, or null if the mount point does not exist.</returns>
        public MountPoint GetMountPoint(MountPointType type)
        {
            return m_Outfit ? m_Outfit.GetMountPoint(type) : null;
        }

        #endregion

        #region Initialization

        // Protected modifier required to support Unity introspection behavior.
        protected void Awake()
        {
            CheckInitialized();
        }

        private bool IsInitialized
        {
            get { return m_Manager != null; }
        }

        /// <summary>
        /// Gets body information for the current body.
        /// </summary>
        /// <param name="instantiated">
        /// If true, all components should be instantiated.  (Rather than prefabs or prototypes.)
        /// </param>
        /// <returns>Body information for the current body.</returns>
        protected abstract OutfitterBody.Info GetInfo(bool instantiated);

        /// <summary>
        /// Initializes the body if it needs to be initialized.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initialization normally happens automatically during awake.  But in some cases, such
        /// as an outfit baking operation, the body may need to be initialized manually.
        /// </para>
        /// </remarks>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("Attempted to re-initialize body.", this);
                return;
            }

            var info = GetInfo(true);

            m_BaseDefaultMotionRoot = 
                info.defaultMotionRoot ? info.defaultMotionRoot : this.transform;
            m_MotionRoot = m_BaseDefaultMotionRoot;

            m_BaseSurfaceColliderLayer = info.surfaceColliderLayer;
            m_BaseBodyColliderLayer = info.bodyColliderLayer;
            m_BaseBodyColliderStatus = info.bodyColliderStatus;

            m_BaselMaterialOverrides = info.materialOverrides == null
                ? new BodyMaterialOverrides()
                : info.materialOverrides;

            m_Manager = new AccessoryManager(info.accessories, transform, Attach, CanAutoAttach);

            LocalInitializePost();
        }

        #endregion

        #region Head Blending

        /// <summary>
        /// The blend shape renderer, or null if there is none.
        /// </summary>
        public SkinnedMeshRenderer BlendHead
        {
            get { return m_Outfit ? m_Outfit.BlendHead : null; }
        }

        /// <summary>
        /// The number of blend shapes in <see cref="BlendHead"/>, or 0 if <see cref="BlendHead"/>
        /// is null.
        /// </summary>
        public int HeadBlendShapeCount
        {
            get
            {
                return (m_Outfit && m_Outfit.BlendHead)
                    ? m_Outfit.BlendHead.sharedMesh.blendShapeCount
                    : 0;
            }
        }

        /// <summary>
        /// Gets the weight of the specified head blend shape, or 0 if <see cref="BlendHead"/> is
        /// null.
        /// </summary>
        /// <param name="index">The index of the blend shape.</param>
        /// <returns>
        /// The weight of the specified head blend shape, or null if there is no blend shape
        /// at the index.
        /// </returns>
        public float GetHeadBlendWeight(int index)
        {
            return (m_Outfit && m_Outfit.BlendHead)
                ? m_Outfit.BlendHead.GetBlendShapeWeight(index)
                : 0;
        }

        /// <summary>
        /// Sets the weight of the specified blend shape if it exists.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will silently fail if <see cref="BlendHead"/> is null.
        /// </para>
        /// </remarks>
        /// <param name="index">The index of the blend shape.</param>
        /// <param name="value">The blend shape weight.</param>
        public void SetHeadBlendWeight(int index, float value)
        {
            if (m_Outfit && m_Outfit.BlendHead)
                m_Outfit.BlendHead.SetBlendShapeWeight(index, value);
        }

        #endregion

        #region Accessories

        /// <summary>
        /// Add the accessory to the body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The accessory is added to the current outfit and switched to new outfits as 
        /// appropriate. If <paramref name="mustSucceed"/> is true, then any falure to 
        /// immediately attach will result in the accessory not getting added.  If 
        /// <paramref name="mustSucceed"/> is false and the attachement does not succeed, then it 
        /// will be stored for latter attachment.  Re-attachment is retried whenever the 
        /// outfit changes or an accessosry is detached.
        /// </para>
        /// <para>
        /// The priority of an accessory is intrinsic in the order it is added.  Default accessories
        /// defined by the body are always added first.  When a new outfit is set or an accessory
        /// is removed, then pending accessories are given a chance to try to attach again in the
        /// order they have been added.
        /// </para>
        /// <para>
        /// To attach a temporary destructable accessory, get the current outfit's mount
        /// point and attach the accessory directly.  The accessory will then be
        /// destroyed when the outfit is destroyed.
        /// </para>
        /// <para>
        /// The accessory must be an instantiated runtime object, not an asset prototype.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The instantiated accessory to add.</param>
        /// <param name="mustSucceed">
        /// If true the accessory will only be added if it immediately succeeds in attaching.
        /// </param>
        /// <returns>
        /// The result of the add operation.
        /// </returns>
        public AttachStatus AddAccessory(BodyAccessory accessory, bool mustSucceed = false)
        {
            return m_Manager.Add(accessory, mustSucceed);
        }

        /// <summary>
        /// Remove the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Detaches the accessory or removes it from storage as approriate, then releases it 
        /// from management.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to remove.</param>
        /// <returns>True if the accessory was removed.  (No errors.)</returns>
        public bool RemoveAccessory(BodyAccessory accessory)
        {
            return m_Manager.Remove(accessory);
        }

        private AttachStatus Attach(BodyAccessory accessory, AttachMethod attach)
        {
            CheckInitialized();

            return m_Outfit
                ? m_Outfit.AttachAccessory(accessory, attach)
                : AttachStatus.Pending;
        }

        private bool CanAutoAttach()
        {
            return m_Outfit;
        }

        #endregion

        #region Colliders

        /// <summary>
        /// The collider resonsible for environment collision dectection and response.
        /// </summary>
        public Collider SurfaceCollider
        {
            get { return m_Outfit ? m_Outfit.SurfaceCollider : null; }
        }

        /// <summary>
        /// The current outfit's surface collider rigidbody.
        /// </summary>
        public Rigidbody SurfaceRigidBody
        {
            get { return m_Outfit ? m_Outfit.SurfaceRigidBody : null; }
        }

        /// <summary>
        /// An array of the current outfit's body colliders.
        /// </summary>
        /// <returns></returns>
        public BodyCollider[] GetBodyColliders()
        {
            return m_Outfit ? m_Outfit.GetColliders() : new BodyCollider[0];
        }

        /// <summary>
        /// Gets the body collider for the specified mount point. (For the current outfit.)
        /// </summary>
        /// <param name="mountPoint">The mount point.</param>
        /// <returns>The body collider for the specified mount point.</returns>
        public BodyCollider GetBodyCollider(MountPointType mountPoint)
        {
            return m_Outfit ? m_Outfit.GetCollider(mountPoint) : null;
        }

        /// <summary>
        /// The body collider status that is being maintained by the body.
        /// (When there is an outfit.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// When there is an outfit, this value will equal <see cref="CurrentBodyColliderStatus"/>.
        /// Otherwise this value will be the status that will be applied to the next outfit.
        /// </para>
        /// </remarks>
        public BodyColliderStatus DesiredBodyColliderStatus
        {
            get { return m_BaseBodyColliderStatus; }
        }

        /// <summary>
        /// The body collider status for the current outfit, or 
        /// <see cref="BodyColliderStatus.Disabled"/> if there is no outfit.
        /// </summary>
        public BodyColliderStatus CurrentBodyColliderStatus
        {
            get
            {
                return m_Outfit ? m_Outfit.ColliderStatus : BodyColliderStatus.Disabled;
            }
        }

        /// <summary>
        /// Sets the body collider status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is no outfit, the value will be applied when the next outfit is set.
        /// </para>
        /// </remarks>
        /// <param name="status">The status to apply.</param>
        public void SetBodyColliderStatus(BodyColliderStatus status)
        {
            CheckInitialized();

            if (m_Outfit)
                m_Outfit.SetColliderStatus(status);

            m_BaseBodyColliderStatus = status;
        }

        /// <summary>
        /// True if there are body colliders.  (Has an outfit and the outfit has colliders.)
        /// </summary>
        public bool HasBodyColliders
        {
            get { return m_Outfit ? m_Outfit.HasBodyColliders : false; }
        }

        /// <summary>
        /// The layer of the body colliders.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value persists when there is no outfit.
        /// </para>
        /// </remarks>
        public int BodyColliderLayer
        {
            get { return m_Outfit ? m_Outfit.BodyColliderLayer : m_BaseBodyColliderLayer; }
        }

        /// <summary>
        /// Sets the layer of the body colliders.
        /// </summary>
        /// <param name="layer">The layer to apply.</param>
        public void SetBodyColliderLayer(int layer)
        {
            CheckInitialized();

            layer = Mathf.Max(0, layer);

            if (m_Outfit)
                m_Outfit.SetBodyColliderLayer(layer);

            m_BaseBodyColliderLayer = layer;
        }

        /// <summary>
        /// The layer of the surface collider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value persists when there is no outfit.
        /// </para>
        /// </remarks>
        public int SurfaceColliderLayer
        {
            get
            {
                return m_Outfit
                    ? m_Outfit.SurfaceCollider.gameObject.layer
                    : m_BaseBodyColliderLayer;
            }
        }

        /// <summary>
        /// Sets the layer of the surface collider.
        /// </summary>
        /// <param name="layer">The layer to apply.</param>
        public void SetSurfaceColliderLayer(int layer)
        {
            CheckInitialized();

            layer = Mathf.Max(0, layer);

            if (m_Outfit)
                m_Outfit.SurfaceCollider.gameObject.layer = layer;

            m_BaseBodyColliderLayer = layer;
        }

        #endregion

        #region Outfit Handling

        /// <summary>
        /// Sets the current outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can't be used to clear the outfit.  Use <see cref="ClearOutfit"/>.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit to apply to the body. (Can't be null.)</param>
        /// <param name="changeCallback">The </param>
        /// <returns>The old outfit or null on an error.</returns>
        public BodyOutfit SetOutfit(BodyOutfit outfit)
        {
            if (!outfit)
                return ClearOutfit();

            if (outfit == m_Outfit)
            {
                Debug.LogWarning("Outfit is already set. Ignored set request.");
                return null;
            }

            CheckInitialized();

            outfit.MotionRoot.position = m_MotionRoot.position;
            outfit.MotionRoot.rotation = m_MotionRoot.rotation;

            SendOutfitChangedPre();

            if (m_Outfit)
                UnloadOutfit();

            var oldOutfit = m_Outfit;
            m_Outfit = outfit;
            m_MotionRoot = m_Outfit.MotionRoot;

            LoadOutfit(m_Outfit, false);

            SendOutfitChangedPost();

            return oldOutfit;
        }

        /// <summary>
        /// Clears the current outfit.  (Sets it to null.)
        /// </summary>
        /// <returns>The old outfit, or null if there was none.</returns>
        public BodyOutfit ClearOutfit()
        {
            if (!m_Outfit)
                return null;

            CheckInitialized();

            m_BaseDefaultMotionRoot.position = m_Outfit.MotionRoot.position;
            m_BaseDefaultMotionRoot.rotation = m_Outfit.MotionRoot.rotation;

            SendOutfitChangedPre();
            UnloadOutfit();

            var oldOutfit = m_Outfit;
            m_Outfit = null;
            m_MotionRoot = m_BaseDefaultMotionRoot;

            SendOutfitChangedPost();

            return oldOutfit;
        }

        /// <summary>
        /// Applies the body settings to an external outfit.
        /// </summary>
        /// <param name="externalOutfit">The outfit to apply the body settings to.</param>
        /// <param name="snapTransform">
        /// True if the external outfit should be snapped to body's <see cref="MotionRoot"/>.
        /// </param>
        public void ApplyTo(BodyOutfit externalOutfit, bool snapTransform = false)
        {
            if (externalOutfit == m_Outfit)
            {
                Debug.LogWarning(
                    "Invalid operation: External outfit same as this outfit.", this);
                return;
            }

            LoadOutfit(externalOutfit, true, snapTransform);
        }

        #endregion

        #region Local Utility Methods

        private void CheckInitialized()
        {
            if (!IsInitialized)
                Initialize();
        }

        private void LoadOutfit(BodyOutfit outfit, bool isExternalOutfit, bool snapTransform = false)
        {
            // Note:  Internal/external have evolved into very different loads. But keeping them 
            // in one place for ease of refactoring, with all loading operations in one place.

            // Note: snapTransform is only applicable to external outfit.

            if (isExternalOutfit)
            {
                var info = GetInfo(true);

                if (info.accessories != null)
                {
                    foreach (var item in info.accessories)
                    {
                        if (!item)
                            continue;

                        var controller = item.TakeOwnership(this, null);

                        if (outfit.AttachAccessory(item, controller.Attach) == AttachStatus.Success)
                            controller.Accessory.ReleaseOwnership(this, true);
                        else
                            item.gameObject.SafeDestroy();
                    }
                }

                if (info.materialOverrides != null)
                    outfit.Apply(info.materialOverrides);

                outfit.SetColliderStatus(info.bodyColliderStatus);
                outfit.SetBodyColliderLayer(info.bodyColliderLayer);
                outfit.SurfaceCollider.gameObject.layer = info.surfaceColliderLayer;

                var snapTrans = info.defaultMotionRoot ? info.defaultMotionRoot : transform;

                if (snapTransform)
                {
                    // Some outfit assets have a build in local offset.  Don't want than.
                    // The outfit and the motion should should start off colocated.
                    outfit.transform.position = snapTrans.position;
                    outfit.transform.rotation = snapTrans.rotation;
                    outfit.MotionRoot.position = snapTrans.position;
                    outfit.MotionRoot.rotation = snapTrans.rotation;
                }
            }
            else
            {
                m_Manager.TryAttachAll();

                outfit.Apply(m_BaselMaterialOverrides);
                outfit.SetColliderStatus(m_BaseBodyColliderStatus);
                outfit.SetBodyColliderLayer(m_BaseBodyColliderLayer);
                outfit.SurfaceCollider.gameObject.layer = m_BaseSurfaceColliderLayer;
            }
        }

        /// <summary>
        /// Unloads outfit components owned by the body.
        /// </summary>
        private void UnloadOutfit()
        {
            m_Manager.DetachAll();
        }

        private void SendOutfitChangedPre()
        {
            LocalOutfitChangePre();

            if (OnOutfitChangePre != null)
                OnOutfitChangePre(this);
        }

        private void SendOutfitChangedPost()
        {
            LocalOutfitChangePost();

            if (OnOutfitChangePost != null)
                OnOutfitChangePost(this);

            if (OnAnimatorChange != null)
                OnAnimatorChange(this);
        }

        #endregion
    }
}
