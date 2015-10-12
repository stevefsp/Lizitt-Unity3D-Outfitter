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
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An accessory that can be added to a body or attached to an outfit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Warning: Do not make concrete classes a required component.
    /// I.e. Don't do this: [RequireComponent(typeof(BodyAccessory))]
    /// Doing so can prevent proper conversion to a static non-accessory object which is 
    /// required for baking an outfit.
    /// </para>
    /// <para>
    /// This class enforces all required accessory behavior.
    /// </para>
    /// </remarks>
    public abstract partial class BodyAccessory
        : MonoBehaviour
    {
        /*
         * TODO: EVAL:  Consider preserving active accessory state during instantiation.
         * 
         * Accessories are meant to range from very simple to very complex.  E.g. A static hat 
         * that enables and disables its renderers depending on attach status, or a fairy flying 
         * around the agent's head with sparkle emitters.
         * 
         * Accessories are also meant to be bakable in order to support outfit baking.
         * 
         * The problem is that runtime accessory instantiation only preserves certain state.  E.g.
         * animators reset to default with skinned mesh renderers in bind pose.  Stanard accessories
         * will re-initialize to inactive.  Etc.
         * 
         * Need to figure out a good standard method of properly baking.  Introduce a 
         * clone/copy state method?
         * 
         */

        #region Delegates

        /// <summary>
        /// A delegate used to communicate a status change.
        /// </summary>
        /// <param name="accessory">The accessory whose status has changed.</param>
        /// <param name="status">The current accessory status.</param>
        public delegate void StatusChange(BodyAccessory accessory, AccessoryStatus status);

        #endregion

        #region Initialization

        private bool m_IsInitialized;

        // Protected modifier required to support Unity introspection behavior.
        /// <summary>
        /// The standard Monobehaviour start method.
        /// </summary>
        protected void Start()
        {
            CheckInitialize();
        }

        private void CheckInitialize()
        {
            if (m_IsInitialized)
                return;

            m_IsInitialized = true;
            OnInitialize();
        }

        /// <summary>
        /// Called during <see cref="Start"/> or on the first main operation performed, whichever is first.
        /// (E.g. Attach, Store, Purge, etc.)
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        #endregion

        #region Status

        /// <summary>
        /// IMPORTANT: Use property to mutate.
        /// </summary>
        private AccessoryStatus m_Status;

        /// <summary>
        /// Called whenever the accessory status changes.
        /// </summary>
        public event StatusChange OnStatusChange;

        /// <summary>
        /// The current accessory status.
        /// </summary>
        public AccessoryStatus Status
        {
            get { return m_Status; }
        }

        private void SetStatus(AccessoryStatus status, bool sendEvent)
        {
            if (m_Status == status)  // << Important check.
                return;

            m_Status = status;

            if (sendEvent)
                SendStatusChange();
        }

        private void SendStatusChange()
        {
            if (OnStatusChange != null)
                OnStatusChange(this, m_Status);
        }

        #endregion

        #region Mount & Coverage

        /// <summary>
        /// The mount point the accessory should be attached to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this property only applies during the attach operation.  The value is
        /// ignored while attached.
        /// </para>
        /// </remarks>
        public abstract MountPointType MountPoint { get; set; }

        /// <summary>
        /// If true, will attach to outfits flagged as limited
        /// </summary>
        /// <remarks>
        /// <para>
        /// Limited outfits include outfits such as nudes and semi-nudes that generally should
        /// not have accessories attached.  E.g. You generally don't want a hat or a 
        /// pair of glasses to be included on a nude.  If this value is true, the accessory will
        /// attach to the outfit irrespective of the state of the outfit's limited flag.
        /// </para>
        /// <para>
        /// The value of this property only applies during the attach operation.  The value is
        /// ignored while attached.
        /// </para>
        /// </remarks>
        public abstract bool IgnoreLimited { get; set; }

        /// <summary>
        /// If true the coverage can be changed.  Otherwise an attempt to change the overage
        /// will result in an error.
        /// </summary>
        public abstract bool IsCoverageDynamic { get; }

        /// <summary>
        /// The current body coverage of the accessory. (Value may be dynamic.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The accessory decides whether or not the coverage is dynamic.   Calling the setter
        /// on a non-dynamic accessory will result in an error.
        /// </para>
        /// <para>
        /// Dynamic coverage is supported in order to allow accessories to purposefully be attached
        /// to unexpected mount points.  Changing coverage should be used with care.
        /// </para>
        /// <para>
        /// There is no requirement for attached accessories to respond to changes in coverage.
        /// </para>
        /// </remarks>
        public abstract BodyCoverage Coverage { get; set; }

        #endregion

        #region Ownership

        private Controller m_Controller;

        /// <summary>
        /// True if there is an active controller. 
        /// </summary>
        public bool IsOwned
        {
            get { return m_Controller != null && m_Controller.IsValid; }
        }

        /// <summary>
        /// True if the accessory is owned by the specified object.
        /// </summary>
        /// <param name="owner">The object to check.</param>
        /// <returns>True if the accessory is owned by the object.</returns>
        public bool IsOwnedBy(Object owner)
        {
            return (IsOwned && m_Controller.Owner == owner);
        }

        /// <summary>
        /// Sets the object as the owner of the accessory and provides a controller.
        /// </summary>
        /// <para>
        /// An accessory can only have one owner.  This method will return null if 
        /// <see cref="IsOwned"/> is true.  (The same owner can't take ownership twice.)
        /// </para>
        /// <param name="owner">The owner of the accessory.</param>
        /// <param name="onAutoRelease">
        /// The method to call if the accessory must be auto-released.
        /// </param>
        /// <returns>An accessory controller if successfull.</returns>
        public Controller TakeOwnership(Object owner, Controller.AutoRelease onAutoRelease)
        {
            CheckInitialize();

            if (!owner)
            {
                Debug.LogError("Owner is null.", this);
                return null;
            }

            if (IsOwned)
            {
                Debug.LogError(this.name + ": This accessory is already owned by "
                    + m_Controller.Owner.name, this);
                return null;
            }

            m_Controller = new Controller(this, owner, onAutoRelease);

            return m_Controller;
        }

        /// <summary>
        /// Release ownership of the accessory, allowing other objects to take control.
        /// </summary>
        /// <param name="owner">The current owner of the accessory.</param>
        /// <param name="keepStatus">
        /// If true, then the accessories current status will not be altered.
        /// Otherwise, the accessory will be released to Limbo.
        /// </param>
        public void ReleaseOwnership(Object owner, bool keepStatus = false)
        {
            if (!IsOwned)
                return;

            if (owner != m_Controller.Owner)
            {
                Debug.LogError(
                    "Only the owner can release ownership: " + m_Controller.Owner.name, this);
                return;
            }

            m_Controller = null;

            if (keepStatus)
                return;

            switch (m_Status)
            {
                case AccessoryStatus.Attached:
                case AccessoryStatus.Stored:

                    Release(null);
                    break;
            }
        }

        #endregion

        #region Control

        private bool m_BlockAutoRelease;

        private bool Attach(Transform target)
        {
            CheckInitialize();

            if (!(m_Status == AccessoryStatus.Limbo || m_Status == AccessoryStatus.Stored))
            {
                // It is an error to try to call attach on an already attached accessory.
                Debug.LogError(name + ": Can't attach accessory.  Invalid starting state: " + m_Status);

                return false;
            }

            m_BlockAutoRelease = true;

            if (!OnAttachPre(target))
            {
                m_BlockAutoRelease = false;
                return false;
            }

            FinalizeAttach(target);

            SetStatus(AccessoryStatus.Attached, true);

            OnAttachPost();
            m_BlockAutoRelease = false;

            return true;
        }

        private bool Store(Transform target, bool allowRootStorage = false)
        {
            CheckInitialize();

            if (m_Status != AccessoryStatus.Limbo)
            {
                // It is an error to try to call store on an already stored accessory.
                // Should release, then store.  This helps minimize the chance of bugs and
                // poor design going unnoticed.
                Debug.LogError("Can't store accessory.  Invalid starting state: " + m_Status);
                return false;
            }

            if (!allowRootStorage && !target)
            {
                Debug.LogError("Can't store accessory at scene root.", this);
                return false;
            }

            FinalizeAttach(target);
            SetStatus(AccessoryStatus.Stored, true);

            m_BlockAutoRelease = true;
            OnStored();
            m_BlockAutoRelease = false;

            return true;
        }

        /// <summary>
        /// Detaches the accessory from its mount point and parents it to a holding object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The purpose of the holding object is to keep unmounted accessories from cluttering
        /// the root of the scene.
        /// </para>
        /// </remarks>
        /// <param name="toStorage">
        /// The object that will hold the accessory until it is ready to attach to a 
        /// new mount point. (Can be null, though not recommended.)
        /// </param>
        private bool Release(Transform toStorage)
        {
            CheckInitialize();

            switch (m_Status)
            {
                case AccessoryStatus.Stored:

                    if (toStorage && toStorage == transform.parent)
                        // No need to transfer storage.
                        return true;

                    m_BlockAutoRelease = true;
                    OnReleaseFromStoragePre();
                    m_BlockAutoRelease = false;

                    transform.parent = null;
                    SetStatus(AccessoryStatus.Limbo, false);

                    if (toStorage)
                        Store(toStorage);

                    return true;

                case AccessoryStatus.Attached:

                    m_BlockAutoRelease = true;
                    OnReleaseFromAttachedPre();
                    m_BlockAutoRelease = false;

                    transform.parent = null;
                    SetStatus(AccessoryStatus.Limbo, false);  // May be temp.  No event.

                    if (toStorage)
                        Store(toStorage);  // Handle event.
                    else
                        SendStatusChange();  // Manual event.

                    return true;

                case AccessoryStatus.Limbo:

                    // Harmless.  Don't even bother warning about the repeat call.
                    return true;
            }

            // May seem harmless to call release on a purged accessory.  But it is unexpected and
            // it indicates the the caller may be unaware of the current purged state, which is
            // bad.
            Debug.LogError("Can't release accessory.  Invalid starting state: " + m_Status);
            return false;
        }

        /// <summary>
        /// Destroys all accessory related components and behaviors in prepration for the deletion
        /// of the accessory component and conversion to a static object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are use cases where the owner of an outfit needs to bake the outfit into a static
        /// representation.  E.g. For preview purposes in the editor, or when an agent dies and
        /// animation is complete.  This method prepares the accessory for conversion to a static 
        /// non-accessory object.
        /// </para>
        /// <para>
        /// If is expected that the caller of this method will immediately delete the BodyAccesory
        /// component following the call.  Behavior is undefined if there is further interaction
        /// with the accessory.
        /// </para>
        /// </remarks>
        private bool Purge()
        {
            CheckInitialize();

            if (m_Status == AccessoryStatus.Stored)
            {
                // Can't see a use case for this.  So being strict so callers no they are doing
                // something inappropriate.
                Debug.LogError("Can't purge accessory.  Invalid starting state: " + m_Status);
                return false;
            }

            if (m_Status == AccessoryStatus.Purged)
                return true;

            SetStatus(AccessoryStatus.Purged, true);

            m_BlockAutoRelease = true;
            OnPurge();
            m_BlockAutoRelease = false;

            return true;
        }

        /// <summary>
        /// Parent's and positions the accessory.
        /// </summary>
        private void FinalizeAttach(Transform parent)
        {
            transform.parent = parent;

            if (!PreserveWorldTransform)
            {
                transform.localPosition = AttachPosition;
                transform.localEulerAngles = AttachRotation;
            }
        }

        /// <summary>
        /// Called after standard error checks succeed and before the attach operation is performed.
        /// </summary>
        /// <param name="target">The transform the accessory will be attached to.</param>
        /// <returns>True if the attach should succeed.  Otherwise false.</returns>
        protected virtual bool OnAttachPre(Transform target)
        {
            return true;
        }

        /// <summary>
        /// Called after the attach has been successfully completed.
        /// </summary>
        /// <returns></returns>
        protected virtual void OnAttachPost()
        {
        }

        /// <summary>
        /// Called after the accessory has transitioned to the stored state. 
        /// </summary>
        protected virtual void OnStored()
        {
        }

        /// <summary>
        /// Called just before the transition to limbo from the the storage state.
        /// </summary>
        protected virtual void OnReleaseFromStoragePre()
        {
        }

        /// <summary>
        /// Called just before the transition to limbo from the attached state.
        /// </summary>
        protected virtual void OnReleaseFromAttachedPre()
        {

        }

        /// <summary>
        /// Purge the accessory.  (See <see cref="Purge"/>.)
        /// </summary>
        protected abstract void OnPurge();  // Dont' make this virtual.  Concrete's need a reminder.

        /// <summary>
        /// True if the auto-release was accepted, false if it was blocked.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implements standard auto-release behavior.  An accessory can be released by its owner
        /// and by the accessory itself.  By using this method for the release, the accessory
        /// properly communicates the release so the owner is notified.
        /// </para>
        /// </remarks>
        /// <param name="toBeDestroyed">
        /// True if the accessory is preparing to destroy itself.
        /// </param>
        /// <returns>True if the auto-release was accepted, false if it was blocked.</returns>
        protected bool AutoRelease(bool toBeDestroyed)
        {
            if (m_BlockAutoRelease)
            {
                Debug.LogError("Accessory attempted to auto-release during a status change.", this);
                return false;
            }

            if (m_Status != AccessoryStatus.Purged)
                Release(null);

            if (IsOwned)
            {
                m_Controller.SendAutoRelease(toBeDestroyed);
                m_Controller = null;
            }

            return true;
        }

        #endregion

        #region Transform Related

        /// <summary>
        /// The position offset to use when attaching the accessory. (Offset from the mount point.) 
        /// (Ignored if <see cref="PreserveWorldTransform"/> is true.)
        /// </summary>
        public virtual Vector3 AttachPosition
        {
            get { return Vector3.zero; }
        }

        /// <summary>
        /// The rotation offset to use when attaching the accessory. (Offset from the mount point.) 
        /// (Ignored if <see cref="PreserveWorldTransform"/> is true.)
        /// </summary>
        public virtual Vector3 AttachRotation
        {
            get { return Vector3.zero; }
        }

        /// <summary>
        /// If true, <see cref="AttachPosition"/> and <see cref="AttachRotation"/> are ignored
        /// and no mount point snapping will occur.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Useful if the accessory will be manaully interpolated to the mount point.
        /// </para>
        /// </remarks>
        public virtual bool PreserveWorldTransform
        {
            get { return false; }
        }

        #endregion
    }
}

