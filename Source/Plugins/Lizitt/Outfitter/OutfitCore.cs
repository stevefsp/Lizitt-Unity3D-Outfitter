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
    /// Implements the most common outfit features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements the outfit features that are least likely to change from
    /// implementation to implementation.  It is a good extension point for implementations
    /// that need to have their own accessory handling, body part handling, and
    /// material handling.
    /// </para>
    /// </remarks>
    public abstract class OutfitCore
        : Outfit
    {
        /*
         * Design notes:
         * 
         * Custom editors exist for this class.  In order to reduce hard coded field names,
         * the fields are organized into sections. Take care when refactoring field names that
         * are marked as being used in the editor, and make sure you understand the custom
         * editor design before rearranging, adding, or deleting fields.
         */

        #region Core Settings (Editor Section)

        [Space]  // Space after script field.

        [SerializeField]
        [Tooltip("The transform that is used to move the outfit. (Required)")]
        [LocalComponentPopupAttribute(typeof(Transform), true)]
        private Transform m_MotionRoot;         // Refactor note: Field name used in the editor.

        public sealed override Transform MotionRoot
        {
            get 
            {
                if (!m_MotionRoot)
                    m_MotionRoot = transform;

                return m_MotionRoot; 
            }
        }

        /// <summary>
        /// Sets the outfit's motion root.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method is used after outfit initialization. 
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required.)</param>
        /// <param name="motionRoot">The motion root. (Required.)</param>
        public static void UnsafeSetMotionRoot(OutfitCore outfit, Transform motionRoot)
        {
            // Hack: Naming convention: An overload failure requires this to be unique method name.
            // This method used to be UnsafeSet(Outfit, Transform).  But for some reason 
            // calls meant for this method were redirecting to 
            // UnsafeSet(StandardOutfit, bool, BodyPart[]) in StandardBody.  So had to 
            // abandon the overload naming convention for this method.
            // Note:  This problem only started to happen when the body part features were
            // moved from OutfitCore to StandardOutfit.

            if (motionRoot)
            {
                var check = motionRoot.GetComponentInParent<OutfitCore>();
                if (check != outfit)
                {
                    // This is not a completely accurate test.  But it is ok since having
                    // multiple outfits in the parent hierachy is not appropriate.
                    Debug.LogError("The motion root is not a child of the outfit.", outfit);
                    return;
                }

                outfit.m_MotionRoot = motionRoot;
            }
            else
                Debug.LogError("Invalid motion root.  Motion root can't be null.", outfit);
        }

        [SerializeField]
        [Tooltip("The main collider for the outfit. (Optional)")]
        [LocalComponentPopupAttribute(typeof(Collider))]
        private Collider m_PrimaryCollider;         // Refactor note: Field name used in the editor.

        public sealed override Collider PrimaryCollider
        {
            get { return m_PrimaryCollider; }
            set
            {
                if (!value)
                {
                    m_PrimaryCollider = null;
                    return;
                }

                var check = value.GetComponentInParent<OutfitCore>();
                if (check != this)
                {
                    Debug.LogError(
                        "Outfit: Primary collider must be a child its outfit: " + value.name, this);

                    return;
                }

                m_PrimaryCollider = value;

                if (!PrimaryRigidbody)
                    Debug.LogWarning("Outfit: Primary collider does not have a rigidbody.", this);
            }

        }

        public sealed override Rigidbody PrimaryRigidbody
        {
            get { return m_PrimaryCollider ? m_PrimaryCollider.GetAssociatedRigidBody() : null; }
        }

        [SerializeField]
        [Tooltip("The owner of the outfit. (Various standard implemetations will automatically set"
            + " this value as control of the outfit is handed off between them.)"
            + " (Informational, Optional)")]
        private GameObject m_Owner = null;

        public sealed override GameObject Owner
        {
            get { return m_Owner; }
        }

        [SerializeField]
        [Tooltip("Use the default storage for this outfit.  Activate/deactivate the"
            + " component's GameObject as appropriate.")]
        private bool m_UseDefaultStorage = true;

        /// <summary>
        /// Use the default storage for this outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If true, deactivates the component's GameObject when it transitions into 'stored', and
        /// activates it when it transitions out of 'stored'.
        /// </para>
        /// </remarks>
        public bool UseDefaultStorage
        {
            get { return m_UseDefaultStorage; }
        }

        /// <summary>
        /// Set the value of <see cref="UseDefaultStorage"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if the value of <see cref="UseDefaultStorage"/> is changed 
        /// after outfit initialization.
        /// </para>
        /// </remarks>
        public static void UnsafeSetUseDefaultStorage(OutfitCore outfit, bool useDefaultStorage)
        {
            /*
             * Design note: The reason this is unsafe is because of unpredicable beahvior if
             * the value is changed during use of the outfit.  E.g. What should the outfit do
             * if default storage is turned off when it is already in storage?  What will the
             * user expect?
             */

            outfit.m_UseDefaultStorage = useDefaultStorage;
        }

        [SerializeField]
        [HideInInspector]
        private OutfitStatus m_Status;

        public sealed override  OutfitStatus Status
        {
            get { return m_Status; }
        }

        /// <summary>
        /// Set the outfit status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="owner"/> must be non-null for all states except 'unmanaged'.
        /// </para>
        /// <para>
        /// Activates and deactivates the component's GameObject as appropriate based on the value 
        /// of <see cref="UseDefaultStorage"/>.
        /// </para>
        /// <para>
        /// If using default storage: The 'stored' event will be sent before GameObject
        /// deactivation.  The other events will be sent after activation.  (I.e. Events will 
        /// always be sent while the GameObject is active.)
        /// </para>
        /// </remarks>
        /// <param name="status">The status.</param>
        /// <param name="owner">The owner.  (Required for all status except 'unmanaged'.)</param>
        /// <returns>
        /// True if the operation was successful.  False on error.
        /// </returns>
        public override bool SetState(OutfitStatus status, GameObject owner)
        {
            if (!(status == OutfitStatus.Unmanaged || owner))
            {
                Debug.LogError("Can't set status with a null owner: " + status, this);
                return false;
            }

            if (m_Status == status && m_Owner == owner)
                return true;

            m_Owner = owner;
            m_Status = status;

            if (m_Status == OutfitStatus.Stored)
            {
                // Event before deactivation.
                m_Observers.SendStateChange(this);

                if (m_UseDefaultStorage)
                    gameObject.SetActive(false);
            }
            else
            {
                // Event after activation.
                if (m_UseDefaultStorage)
                    gameObject.SetActive(true);

                m_Observers.SendStateChange(this);
            }

            return true;
        }

        private void ResetCoreSettings()
        {
            m_MotionRoot = null;
            m_PrimaryCollider = null;
            m_Owner = null;
            m_Status = OutfitStatus.Unmanaged;
            m_UseDefaultStorage = true;
        }

        #endregion

        #region General Accessory Settings & Mount Points (Editor Section)

        #region Accessory Settings

        [SerializeField]
        [Tooltip("Only accessories marked to ignore this flag should be attached to the outfit.")]
        private bool m_Limited;         // Refactor note: Field name used in the editor.

        public override bool AccessoriesLimited
        {
            // Leave unsealed so the setter can be overriden.
            get { return m_Limited; }
            set { m_Limited = value; }
        }

        [SerializeField]
        [Tooltip("The built-in coverage blocks for the outfit.  Accessories that have any of these"
            + " coverages will not be able to attach to the outfit.")]
        [EnumFlags(typeof(BodyCoverage), OutfitterUtil.SortBodyCoverage)]
        private BodyCoverage m_Blocks = 0;

        public sealed override BodyCoverage CoverageBlocks
        {
            get { return m_Blocks; }
            set { m_Blocks = value; }
        }

        public override BodyCoverage CurrentCoverage
        {
            // Expect extension.
            get { return CoverageBlocks; }
        }

        private void ResetAccessorySettings()
        {
            m_Blocks = 0;
        }

        #endregion

        #region Mount Points

        [SerializeField]
        [Tooltip("Perform a refresh of the mount points during outfit initialization."
            + " (Flexible, but less efficient than assigning mount points at design-time.)")]
        private bool m_AutoLoadMounts = false;

        /// <summary>
        /// If true, a refresh of the mount points will ocucr during outfit initialization.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Refreshing at run-time is flexible, but less efficient than assigning mount points at 
        /// design-time.
        /// </para>
        /// </remarks>
        public bool AutoLoadMountPoints
        {
            get { return m_AutoLoadMounts; }
            set { m_AutoLoadMounts = value; }
        }

        [Space(10)]

        [SerializeField]
        [ObjectList("Mount Points")]
        private MountPointGroup m_MountPoints = new MountPointGroup(0);  // Refactor note: Field name used in the editor.

        public sealed override bool HasMountPoints 
        {
            get
            {
                CheckInitializeMountPoints();
                return m_MountPoints.HasItem;
            }
        }

        public sealed override int MountPointBufferSize
        {
            get 
            {
                CheckInitializeMountPoints();
                return m_MountPoints.BufferSize; 
            }
        }

        public sealed override MountPoint GetMountPoint(int index)
        {
            CheckInitializeMountPoints();
            return m_MountPoints[index];
        }

        public sealed override MountPoint GetMountPoint(MountPointType locationType)
        {
            CheckInitializeMountPoints();
            return m_MountPoints[locationType];
        }

        /// <summary>
        /// True if the specified mount point is part of the outfit.
        /// </summary>
        /// <param name="location">The mount point.</param>
        /// <returns>True if the specified mount point is part of the outfit.</returns>
        public bool IsLocalMountPoint(MountPoint location)
        {
            return location && m_MountPoints.Contains(location);
        }

        /// <summary>
        /// Replaces the current mount points with the provided mounts points.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization or after
        /// accessories are attached.
        /// </para>
        /// <para>
        /// If <paramref name="asReference"/> is true, then all external references to the
        /// mount point array must be discared or behavior will be undefined.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="asReference">If true, the <see cref="mountPoints"/> refrence will be used
        /// internally, otherwise the array will be copied.</param>
        /// <param name="mountPoints">The mount points, or null to clear all mount points.</param>
        public static void UnsafeSet(
            OutfitCore outfit, bool asReference, params MountPoint[] mountPoints)
        {
            // Design note: While odd, it is not required that a mount point be
            // a child of the outfit.  So don't put any restrictions in place for that.

            if (mountPoints == null)
                outfit.m_MountPoints.Clear();
            else
                MountPointGroup.UnsafeReplaceItems(outfit.m_MountPoints, asReference, mountPoints);
        }

        /// <summary>
        /// Clear all mount points.
        /// </summary>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization or after
        /// accessories are attached.
        /// </para>
        /// <param name="outfit">The outfit. (Required.)</param>
        public static void UnsafeClearMountPoints(OutfitCore outfit)
        {
            outfit.m_MountPoints.Clear();
        }

        /// <summary>
        /// Refreshes the mount points using a child search.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method if used after outfit initialization or after
        /// accessories are attached.
        /// </para>
        /// <para>
        /// If <paramref name="replace"/> is false, the order of currently defined mount points
        /// will be preserved and new mount points will be added to the end of the list.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="replace">
        /// If true, all current mount points will be cleared and replaced with the result of
        /// the refresh.  Otherwise only newly detected mount points will be added.
        /// </param>
        /// <returns>
        /// The number of mount points added, or all if <paramref name="replace"/> is true.
        /// </returns>
        public static int UnsafeRefreshMountPoints(OutfitCore outfit, bool replace)
        {
            var fitems = outfit.GetComponentsInChildren<MountPoint>();

            if (replace)
            {
                UnsafeSet(outfit, true, fitems);
                return fitems.Length;
            }

            var before = outfit.m_MountPoints.ItemCount;

            outfit.m_MountPoints.CompressAndAdd(fitems);

            return outfit.m_MountPoints.ItemCount - before;
        }

        // Not destructive.  No need to serialize.
        private bool m_IsMountsInitialized = false;

        private void CheckInitializeMountPoints()
        {
            if (m_IsMountsInitialized)
                return;

            m_IsMountsInitialized = true;

            if (m_AutoLoadMounts)
                UnsafeRefreshMountPoints(this, false);

            m_MountPoints.SetOwnership(gameObject, true);
        }

        private void ResetMountPointSettings()
        {
            m_AutoLoadMounts = false;
            m_MountPoints.Clear();
        }

        #endregion

        #endregion

        #region Outfit Observers (Editor Section)

        [SerializeField]
        [ObjectList("IOutfitObserver Objects", typeof(IOutfitObserver))]
        private OutfitObserverGroup m_Observers = new OutfitObserverGroup(0);   // Refactor note: Field name used in the editor.

        protected OutfitObserverGroup Observers
        {
            get { return m_Observers; }
        }

        public sealed override bool AddObserver(IOutfitObserver observer)
        {
            return m_Observers.Add(observer, this) != -1;
        }

        public sealed override void RemoveObserver(IOutfitObserver observer)
        {
            m_Observers.Remove(observer);
        }

        #endregion

        #region Initialization

        protected virtual void Awake()
        {
            CheckInitializeMountPoints();
        }

        #endregion

        #region Utility Features

        // Note: Static untility members specific to a feature are colocated with their feature.

        /// <summary>
        /// Resets all field values except the observers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The observers are not cleared by this method since there are use cases that require
        /// observer communication after the reset.  (I.e. BakePost.)  Whatever calls the
        /// reset must decide if/when to clear the observers.
        /// </para>
        /// </remarks>
        protected virtual void Reset()
        {
            ResetCoreSettings();
            ResetAccessorySettings();
            ResetMountPointSettings();
        }

        #endregion
    }
}
