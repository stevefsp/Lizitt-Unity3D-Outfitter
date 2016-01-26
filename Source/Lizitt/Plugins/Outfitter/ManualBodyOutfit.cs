using com.lizitt.u3d;
using UnityEngine;
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An outfit with all components manually assigned.
    /// </summary>
    public class ManualBodyOutfit
        : BodyOutfit
    {
        #region Core

        [Header("Core References")]

        [SerializeField]
        [Tooltip("The transform that is used to move the outfit. (Defaults to the"
            + " Animator's transform, if present.  Otherwise defaults to component's"
            + " transform.)")]
        private Transform m_MotionRoot = null;
        public override Transform MotionRoot
        {
            get { return m_MotionRoot; }
        }

        [SerializeField]
        [Tooltip("The outfit's animator.  (If not assigned, defaults to the first Animator"
            + " found via a child serach.)")]
        private Animator m_Animator = null;
        public override Animator Animator
        {
            get { return m_Animator; }
        }

        private void InitializeCore()
        {
            if (!m_Animator)
                m_Animator = GetComponentInChildren<Animator>();

            if (!m_MotionRoot)
                m_MotionRoot = m_Animator ? m_Animator.transform : transform;
        }

        #endregion

        #region Renderers

        [Header("Renderers")]

        [SerializeField]
        [Tooltip("The material the will be replaced when a body material override is"
            + " applied to the outfit.")]
        [RendererMaterialPtr]
        private RendererMaterialPtr m_BodyMaterial = null;

        [SerializeField]
        [Tooltip("The material the will be replaced when a head material override is"
            + " applied to the outfit.")]
        [RendererMaterialPtr]
        private RendererMaterialPtr m_HeadMaterial = null;

        [SerializeField]
        [Tooltip("The material the will be replaced when a eye material override is"
            + " applied to the outfit.")]
        [RendererMaterialPtr]
        private RendererMaterialPtr m_EyeMaterial = null;

        [SerializeField]
        [Tooltip("The renderer that contains the head's blendshaps.  (If applicable.)")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_HeadBlendShapes")]
        private SkinnedMeshRenderer m_BlendShapeRenderer = null;
        public override SkinnedMeshRenderer BlendHead
        {
            get { return m_BlendShapeRenderer; }
        }

        public override void Apply(BodyMaterialOverrides overrides)
        {
            m_HeadMaterial.Apply(overrides.HeadMaterial);
            m_EyeMaterial.Apply(overrides.EyeMaterial);
            m_BodyMaterial.Apply(overrides.BodyMaterial);
        }

        private void InitializeRenderers()
        {
            if (m_BlendShapeRenderer && 
                (!m_BlendShapeRenderer.sharedMesh || m_BlendShapeRenderer.sharedMesh.blendShapeCount == 0))
            {
                m_BlendShapeRenderer = null;
                Debug.LogError(
                    "Mesh on renderer does not have any blend shapes: " + m_BlendShapeRenderer, this);
            }
        }

        #endregion

        #region Colliders (Surface & Body)

        [Header("Surface & Body Part Colliders")]

        [SerializeField]
        [Tooltip("The main collider for the outfit.")]
        private Collider m_SurfaceCollider = null;
        public override Collider SurfaceCollider
        {
            get { return m_SurfaceCollider; }
        }

        private Rigidbody m_RigidBody = null;
        public override Rigidbody SurfaceRigidBody
        {
            get { return m_RigidBody; }
        }

        [Space(5)]

        [SerializeField]
        [Tooltip("The layer of the body part colliders. (Will be applied at initialization.)")]
        [UnityLayer]
        private int m_PartColliderLayer = 0;
        public override int BodyColliderLayer
        {
            get { return m_PartColliderLayer; }
        }

        public override void SetBodyColliderLayer(int layer)
        {
            if (layer < 1 || layer > 32)
            {
                Debug.LogError("Body collider layer is invalid (Out of range): " + layer, this);
                return;
            }

            m_PartColliderLayer = layer;

            foreach (var item in m_BodyPartColliders)
                item.Layer = m_PartColliderLayer;
        }

        [SerializeField]
        [Tooltip("The status of the body part colliders. (Will be applied at initialization.)")]
        private BodyColliderStatus m_PartColliderStatus = BodyColliderStatus.Disabled;
        public override BodyColliderStatus ColliderStatus
        {
            get { return m_PartColliderStatus; }
        }

        public override void SetColliderStatus(BodyColliderStatus status)
        {
            m_PartColliderStatus = status;

            foreach (var item in m_BodyPartColliders)
                item.Status = m_PartColliderStatus;
        }

        [Space(5)]

        [SerializeField]
        [Tooltip("The body part colliders")]
        private BodyColliderInfoGroup m_PartColliderInfo = new BodyColliderInfoGroup();

        private List<BodyCollider> m_BodyPartColliders = null;
        public override BodyCollider GetCollider(MountPointType mountPoint)
        {
            foreach (var item in m_BodyPartColliders)
            {
                if (item.MountPoint == mountPoint)
                    return item;
            }
            
            return null;
        }

        public override BodyCollider[] GetColliders()
        {
            return (BodyCollider[])m_BodyPartColliders.ToArray();
        }

        public override bool HasBodyColliders
        {
            get { return m_BodyPartColliders.Count > 0; }
        }

        private void IntitializeColliders()
        {
            if (!m_SurfaceCollider)
                m_SurfaceCollider = m_MotionRoot.GetComponent<Collider>();

            if (m_SurfaceCollider)
                m_RigidBody = m_SurfaceCollider.GetComponent<Rigidbody>();

            m_BodyPartColliders = new List<BodyCollider>(m_PartColliderInfo.Count);

            foreach (var item in m_PartColliderInfo)
            {
                if (!item.Collider)
                    continue;

                var rb = item.Collider.GetComponent<Rigidbody>();

                if (!rb)
                {
                    Debug.LogError(
                        "Body part collider without a rigidbody: " + item.Collider, item.Collider);
                    continue;
                }

                // Don't bother checking for duplicate mount points.  The editor is expected
                // to handle that.
                m_BodyPartColliders.Add(new BodyCollider(item.Collider, rb, item.Type));
            }

            SetBodyColliderLayer(m_PartColliderLayer);
            SetColliderStatus(m_PartColliderStatus);

            m_PartColliderInfo = new BodyColliderInfoGroup();
        }

        #endregion

        #region Mount Points

        [Header("Mount Points")]

        [SerializeField]
        [Tooltip("The available mount points.")]
        private MountPointGroup m_MountPoints = new MountPointGroup();
        public override MountPoint GetMountPoint(MountPointType mountPoint)
        {
            foreach (var item in m_MountPoints)
            {
                if (item != null && item.MountType == mountPoint)
                    return item;
            }

            return null;
        }

        #endregion

        #region Accessories

        [Header("Accessories")]

        [SerializeField]
        [Tooltip("If true, only accessories marked to ignore this flag can be successfully"
            + " attached. (Will not block to the accessories listed below.)")]
        private bool m_AccessoriesLimited = false;
        public override bool AccessoriesLimited
        {
            get { return m_AccessoriesLimited; }
        }

        [SerializeField]
        [Tooltip("The built-in coverage blocks for the outfit. (Will not block the"
            + " accessories listed below.)")]
        [EnumFlags(typeof(BodyCoverage))]
        private BodyCoverage m_CoverageBlocks = 0;
        public override BodyCoverage CoverageBlocks
        {
            get { return m_CoverageBlocks; }
        }

        [Space(5)]

        [SerializeField]
        [Tooltip("If true, the accessories listed below represent prototypes that are to be"
            + " instantiated then attached.  Otherwise they represent instantiated objects that"
            + "  are ready to be attached.")]
        private bool m_InstantiateAccessory = false;

        [SerializeField]
        [Tooltip("Accessories that will be attached to the outfit on initialization."
            + " (Limit flag and blocks are ignored.)")]
        private AccessoryGroup m_Accessories = new AccessoryGroup();

        private IEnumerable<BodyAccessory> InitializeAccessories()
        {
            if (m_Accessories.Count == 0)
                return new BodyAccessory[0];

            var accessories = new List<BodyAccessory>();

            foreach (var accessory in m_Accessories)
            {
                if (!accessory)
                    // Empty slot.
                    continue;

                bool mounted = false;
                foreach (var mountPoint in m_MountPoints)
                {
                    if (mountPoint.MountType == accessory.MountPoint)
                    {
                        BodyAccessory accInstance = accessory;
                        if (m_InstantiateAccessory)
                        {
                            accInstance = accInstance.Instantiate<BodyAccessory>();
                            accInstance.StripCloneName();
                        }

                        var controller = accInstance.TakeOwnership(this, null);

                        if (controller == null || !controller.Attach(mountPoint.Transform))
                        {
                            // Will happen on technical errors only.  So accessory should have
                            // sent a debug message.

                            accInstance.SafeDestroy();
                            accInstance = null;

                            continue;
                        }

                        // Even though the accessory will only ever self release, can't lock
                        // ownership.  Doing so would prevent proper baking of the outfit.  
                        // (I.e. Purging.)
                        accInstance.ReleaseOwnership(this, true);

                        accessories.Add(accInstance);
                        mounted = true;

                        break;
                    }
                }

                if (!mounted)
                {
                    // This is an error because this is outfit construction.
                    Debug.LogError(
                        "No mount point found for accessory: " + accessory.gameObject.name, this);

                    if (!m_InstantiateAccessory)
                        // Accessory is an instance, not a prototype.  So destory it.
                        accessory.SafeDestroy();
                }

            }

            m_Accessories = new AccessoryGroup();

            return accessories;
        }

        #endregion

        #region Initialization & Purge

        protected override IEnumerable<BodyAccessory> OnInitializePre()
        { 
            InitializeCore();  // Keep first.
            IntitializeColliders();
            InitializeRenderers();

            return InitializeAccessories();
        }

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

            m_MountPoints = new MountPointGroup();
            m_SurfaceCollider = null;
            m_BodyPartColliders = null;

            m_BodyMaterial = null;
            m_HeadMaterial = null;
            m_EyeMaterial = null;

            m_BlendShapeRenderer = null;

            m_Animator = null;
            m_MotionRoot = null;
            m_RigidBody = null;
        }

        #endregion
    }
}
