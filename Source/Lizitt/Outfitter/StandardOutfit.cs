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
    /// An outfit with common settings and behavior.
    /// </summary>
    public abstract class StandardOutfit
        : BodyOutfit
    {
        #region Elements

        /// <summary>
        /// Outfit information required by the standard outfit.
        /// </summary>
        public struct CoreOutfitInfo
        {
            /// <summary>
            /// The outfit mount points.
            /// </summary>
            public MountPoint[] mountPoints;

            /// <summary>
            /// The default accessories.
            /// </summary>
            public IEnumerable<BodyAccessory> accessories;

            /// <summary>
            /// The body colliders.
            /// </summary>
            public BodyCollider[] colliders;

            /// <summary>
            /// The body material override.
            /// </summary>
            public RendererMaterialPtr bodyMaterial;

            /// <summary>
            /// The head material override.
            /// </summary>
            public RendererMaterialPtr headMaterial;

            /// <summary>
            /// The eye material override.
            /// </summary>
            public RendererMaterialPtr eyeMaterial;

            /// <summary>
            /// The renderer that contains the head blend shapes.
            /// </summary>
            public SkinnedMeshRenderer blendHead;
        }

        #endregion

        #region Static Members

        /// <summary>
        /// Applies the configuration to the outfit in an unsafe manner.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Meant for use by the editor.  Does not perform any validation.  Must not be used 
        /// at runtime.
        /// </para>
        /// <para>
        /// All references are used directly by the outfit.  (No cloning.)
        /// </para>
        /// </remarks>
        public static void UnsafeApplyConfiguration(StandardOutfit outfit, CoreOutfitInfo info)
        {
            // Needs to remain public in order to support the editor.

            outfit.m_BlendHead = info.blendHead;
            outfit.m_BodyMaterial = info.bodyMaterial;
            outfit.m_BodyColliders = info.colliders;
            outfit.m_EyeMaterial = info.eyeMaterial;
            outfit.m_HeadMaterial = info.headMaterial;
            outfit.m_MountPoints = info.mountPoints;
        }

        #endregion

        #region Animation

        private SkinnedMeshRenderer m_BlendHead = null;
        private Animator m_Animator = null;

        /// <summary>
        /// The renderer that contains the head blend shapes.
        /// </summary>
        public sealed override SkinnedMeshRenderer BlendHead
        {
            get { return m_BlendHead; }
        }

        /// <summary>
        /// The outfit's animtor, or null if there is none.
        /// </summary>
        public sealed override Animator Animator
        {
            get { return m_Animator; }
        }

        #endregion

        #region Colliders & Physics

        private Collider m_SurfaceCollider = null;
        private Rigidbody m_SurfaceRigidBody = null;
        private BodyCollider[] m_BodyColliders = new BodyCollider[0];

        /// <summary>
        /// Creates the surface collider.
        /// </summary>
        /// <param name="motionTransform">The outfit's motion transform.</param>
        /// <returns>The surface collider.</returns>
        protected abstract Collider CreateSurfaceCollider(Transform motionTransform);

        /// <summary>
        /// <see cref="BodyOutfit.SurfaceCollider"/>
        /// </summary>
        public sealed override Collider SurfaceCollider
        {
            get { return m_SurfaceCollider; }
        }

        /// <summary>
        /// <see cref="BodyOutfit.SurfaceRigidBody"/>
        /// </summary>
        public sealed override Rigidbody SurfaceRigidBody
        {
            get { return m_SurfaceRigidBody; }
        }

        /// <summary>
        /// <see cref="BodyOutfit.HasBodyColliders"/>
        /// </summary>
        public sealed override bool HasBodyColliders
        {
            get { return m_BodyColliders.Length != 0; }
        }

        /// <summary>
        /// <see cref="BodyOutfit.GetColliders"/>
        /// </summary>
        public sealed override BodyCollider[] GetColliders()
        {
            return m_BodyColliders.Clone() as BodyCollider[];
        }

        /// <summary>
        /// <see cref="BodyOutfit.GetCollider"/>
        /// </summary>
        public sealed override BodyCollider GetCollider(MountPointType mountPoint)
        {
            foreach (var item in m_BodyColliders)
            {
                if (item.MountPoint == mountPoint)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// <see cref="BodyOutfit.ColliderStatus"/>
        /// </summary>
        public sealed override BodyColliderStatus ColliderStatus
        {
            get
            {
                return m_BodyColliders.Length == 0
                    ? BodyColliderStatus.Disabled
                    : m_BodyColliders[0].Status;
            }
        }

        /// <summary>
        /// <see cref="BodyOutfit.SetColliderStatus"/>
        /// </summary>
        public sealed override void SetColliderStatus(BodyColliderStatus status)
        {
            foreach (var item in m_BodyColliders)
            {
                item.Status = status;
            }
        }

        /// <summary>
        /// <see cref="BodyOutfit.BodyColliderLayer"/>
        /// </summary>
        public sealed override int BodyColliderLayer
        {
            get
            {
                if (m_BodyColliders.Length == 0)
                    throw new System.InvalidOperationException("There are no colliders.");

                return m_BodyColliders[0].Layer;
            }
        }

        /// <summary>
        /// <see cref="BodyOutfit.SetBodyColliderLayer"/>
        /// </summary>
        public sealed override void SetBodyColliderLayer(int layer)
        {
            foreach (var item in m_BodyColliders)
            {
                item.Layer = layer;
            }
        }

        #endregion

        #region Mount Points & Accessories

        private MountPoint[] m_MountPoints = new MountPoint[0];

        /// <summary>
        /// <see cref="BodyOutfit.GetMountPoint"/>
        /// </summary>
        public sealed override MountPoint GetMountPoint(MountPointType mountPoint)
        {
            foreach (var item in m_MountPoints)
            {
                if (item.MountType == mountPoint)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// The outfit's mount points.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<MountPoint> MountPoints()
        {
            foreach (var item in m_MountPoints)
                yield return item;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Gets the outfit information.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Return null on failure.  Concrete class is expected to send error messages.
        /// </para>
        /// </remarks>
        /// <param name="info">The generated outfit information.</param>
        /// <returns>The outfit's root transform.</returns>
        protected abstract GameObject GetOutfitInfo(out CoreOutfitInfo info);

        /// <summary>
        /// <see cref="BodyOutfit.OnInitializePre"/>
        /// </summary>
        protected sealed override IEnumerable<BodyAccessory> OnInitializePre()
        {
            // Ignore local settings.
            CoreOutfitInfo info;
            var go = GetOutfitInfo(out info);

            if (!go)
                // Any error messages should have been sent by the concrete class.
                this.SafeDestroy();

            go.name = this.name + "_Root";

            if (Application.isEditor)
                go.AddComponent<MakeSelectionBase>();

            var gotran = go.transform;

            gotran.parent = transform;
            gotran.localPosition = Vector3.zero;
            gotran.rotation = Quaternion.identity;

            UnsafeApplyConfiguration(this, info);

            m_Animator = GetComponentInChildren<Animator>();

            // See motion root doc for description of this precidence.
            // TODO: EVAL:  Is this too much automation?  Should assignment of the root be forced.
            if (m_Animator)
                m_MotionRoot = m_Animator.transform;
            else
            {
                var rootMount = GetMountPoint(MountPointType.Root);
                m_MotionRoot = rootMount == null ? transform : rootMount.Transform;
            }

            m_SurfaceCollider = CreateSurfaceCollider(m_MotionRoot);

            if (m_SurfaceCollider)
                m_SurfaceRigidBody = m_SurfaceCollider.GetComponent<Rigidbody>();

            return info.accessories;
        }

        #endregion

        #region Miscellaneious Features

        private RendererMaterialPtr m_BodyMaterial = null;
        private RendererMaterialPtr m_HeadMaterial = null;
        private RendererMaterialPtr m_EyeMaterial = null;

        private Transform m_MotionRoot = null;

        /// <summary>
        /// The outfit's motion root.
        /// </summary>
        public sealed override Transform MotionRoot
        {
            get { return m_MotionRoot; }
        }

        /// <summary>
        /// <see cref="BodyOutfit.Apply"/>
        /// </summary>
        public sealed override void Apply(BodyMaterialOverrides overrides)
        {
            m_HeadMaterial.Apply(overrides.HeadMaterial);
            m_EyeMaterial.Apply(overrides.EyeMaterial);
            m_BodyMaterial.Apply(overrides.BodyMaterial);
        }

        /// <summary>
        /// <see cref="BodyOutfit.PurgeOutfitComponents"/>
        /// </summary>
        public override void PurgeOutfitComponents()
        {
            // Note: Colliders are not considered outfit components.  So they aren't removed as
            // part of the purge.

            foreach (var item in GetComponentsInChildren<BodyAccessory>())
            {
                var controller = item.TakeOwnership(this, null);

                if (controller == null)
                {
                    Debug.LogWarning("Can't take ownership of accessory.  Accessory can't be baked: "
                        + item.name, this);
                    continue;
                }

                controller.Purge();
                item.SafeDestroy();
            }

            m_MountPoints = null;
            m_SurfaceCollider = null;
            m_BodyColliders = null;

            m_BodyMaterial = null;
            m_HeadMaterial = null;
            m_EyeMaterial = null;

            m_BlendHead = null;

            m_Animator = null;
            m_MotionRoot = null;
            m_SurfaceRigidBody = null;
        }

        #endregion
    }
}
