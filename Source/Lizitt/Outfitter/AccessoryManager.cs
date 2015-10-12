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
using System.Collections.Generic;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An accessory manager.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This utility class is useful for implementing standard accessory attach and storage
    /// behavior.  Error handling, callbacks, auto-attach behavior, etc. can result in a 
    /// lot of support code.  This class handles most of that.
    /// </para>
    /// </remarks>
    public class AccessoryManager
        : IEnumerable<BodyAccessory>
    {
        #region Delegates

        /// <summary>
        /// Accessory attach delegate.
        /// </summary>
        /// <param name="accessory"></param>
        /// <param name="attach"></param>
        /// <returns></returns>
        public delegate AttachStatus Attach(BodyAccessory accessory, AttachMethod attach);

        #endregion

        #region Fields

        private Transform m_Storage;

        private readonly List<BodyAccessory.Controller> m_Controllers;
        private readonly Queue<BodyAccessory.Controller> m_ProcessQueue 
            = new Queue<BodyAccessory.Controller>(1);

        private readonly Attach m_Attach;
        private readonly System.Func<bool> m_CanAutoAttach;
        private bool m_IsRechecking;

        private readonly BodyAccessory.Controller.AutoRelease m_AutoRelease;

        #endregion

        #region Construction and Enumeration

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accessories">The initial accessories to add to the manager.</param>
        /// <param name="storage">
        /// The transform to use for storage of accessories that can't be attached.
        /// </param>
        /// <param name="attach">The method to run to attach the accessory.</param>
        /// <param name="canAutoAttach">
        /// The method that indicates whether or not accessories can be auto-attached when other
        /// accessories are removed/detached.
        /// </param>
        public AccessoryManager(BodyAccessory[] accessories, Transform storage,
            Attach attach, System.Func<bool> canAutoAttach)
        {
            if (!storage || attach == null || canAutoAttach == null)
            {
                throw new System.ArgumentNullException();
            }

            m_Attach = attach;
            m_CanAutoAttach = canAutoAttach;
            m_Storage = storage;

            // TODO: EVAL: Remove error handling?
            // There is a lot of complexity for internal error handling.  After project
            // stabilizes, consider removing some of this complexity.

            m_AutoRelease = new BodyAccessory.Controller.AutoRelease(HandleAutoRelease);

            // IMPORTANT: Only accessory handling after this point.

            if (accessories == null)
            {
                m_Controllers = new List<BodyAccessory.Controller>(0);
                return;
            }

            m_Controllers = new List<BodyAccessory.Controller>(accessories.Length);

            foreach (var item in accessories)
            {
                if (!item)
                    continue;

                var controller = item.TakeOwnership(m_Storage, m_AutoRelease);

                if (controller == null)
                {
                    Debug.LogError("Could not take control of accessory: " + item.name,
                        m_Storage);
                    continue;
                }

                if (controller.Store(m_Storage))
                    m_Controllers.Add(controller);
                else
                {
                    // An internal error because concrete accessories are not allowed to 
                    // refuse storage.
                    Debug.LogError("Internal error: Accessory refused storage: " + item.name,
                        m_Storage);

                    item.ReleaseOwnership(m_Storage);
                }
            }
        }

        /// <summary>
        /// Managed accessory indexer.
        /// </summary>
        /// <param name="index">The index of the accessory. 
        /// [Limits: 0 &lt;= value &lt; <see cref="Count"/>]</param>
        /// <returns></returns>
        public BodyAccessory this[int index]
        {
            get { return m_Controllers[index].Accessory; }
        }

        /// <summary>
        /// The number of accessories being managed
        /// </summary>
        public int Count
        {
            get { return m_Controllers.Count; }
        }

        /// <summary>
        /// Enumerator of accesories being managed
        /// </summary>
        /// <returns></returns>
        public IEnumerator<BodyAccessory> GetEnumerator()
        {
            foreach (var item in m_Controllers)
                yield return item.Accessory;
        }

        /// <summary>
        /// Enumerator of accesories being managed.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Management

        /// <summary>
        /// The transform used to store managed, but unattached, accessories.
        /// </summary>
        public Transform Storage
        {
            get { return m_Storage; }
        }

        /// <summary>
        /// Add the accesory to the manager.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default behavior is that an accessory that can't be attached is stored, unless the
        /// attach status is <see cref="AttachStatus.FailedOnError"/>.  This behavior can 
        /// be overriden using <paramref name="mustSucceed"/>.  If <paramref name="mustSucceed"/> 
        /// is true, then any failure to attach will be converted to a failure on error.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to add.</param>
        /// <param name="mustSucceed">
        /// If true the accessory must attach successfully and will not be stored if the
        /// attached fails.
        /// </param>
        /// <returns>The status of the attach.</returns>
        public AttachStatus Add(BodyAccessory accessory, bool mustSucceed = false)
        {
            if (!accessory)
            {
                Debug.LogError("Attempted to add a null accessory.");
                return AttachStatus.FailedOnError;
            }

            if (accessory.IsOwnedBy(m_Storage))
            {
                Debug.LogWarning( "Accessory already added.  Returned current status: " 
                    + accessory.name, m_Storage);

                return accessory.Status == AccessoryStatus.Attached
                    ? AttachStatus.Success 
                    : AttachStatus.Pending;
            }

            if (!Application.isPlaying)
            {
                Debug.LogError("Can only add accessories while in play mode.");
                return AttachStatus.FailedOnError;
            }

            if (accessory.Status != AccessoryStatus.Limbo)
            {
                Debug.LogError("Invalid accessory state.  Accessory is in use or purged: "
                    + accessory.Status);
                return AttachStatus.FailedOnError;
            }

            var controller = accessory.TakeOwnership(m_Storage, HandleAutoRelease);

            if (controller == null)
            {
                Debug.LogError("Invalid accessory state. Could not take ownership: "
                    + accessory.name, m_Storage);

                return AttachStatus.FailedOnError;
            }

            var result = m_Attach(controller.Accessory, controller.Attach);

            if (result == AttachStatus.FailedOnError)
                return result;

            if (result != AttachStatus.Success)
            {
                if (mustSucceed)
                    return result;

                if (!controller.Store(m_Storage))
                {
                    Debug.LogError(
                        "Internal error.  Accessory refused storage: " + accessory, m_Storage);

                    accessory.ReleaseOwnership(m_Storage);

                    return AttachStatus.FailedOnError;
                }

                result = AttachStatus.Pending;
            }

            m_Controllers.Add(controller);

            return result;
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
        public bool Remove(BodyAccessory accessory)
        {
            if (!accessory)
            {
                Debug.LogError("Accessory is null.", m_Storage);
                return false;
            }

            if (!Application.isPlaying)
            {
                Debug.LogError("Can only detach accessories while in play mode.");
                return false;
            }

            if (!accessory.IsOwnedBy(m_Storage))
            {
                Debug.LogError("Accessory is not owned by this manager: " + accessory.name, 
                    m_Storage);
                return false;
            }

            var controller = FindController(accessory, true);

            controller.Release(null);
            controller.Accessory.ReleaseOwnership(m_Storage);

            RecheckAccessories(m_CanAutoAttach());
                
            return true;
        }

        /// <summary>
        /// Detach all managed accessories.
        /// </summary>
        public void DetachAll()
        {
            // Designed so that a release error will not result in an iteration error.

            m_ProcessQueue.Clear();
            foreach (var item in m_Controllers)
                m_ProcessQueue.Enqueue(item);

            while (m_ProcessQueue.Count > 0)
            {
                var item = m_ProcessQueue.Dequeue();

                if (item.Accessory.Status == AccessoryStatus.Attached)
                    // Do not release ownership.
                    item.Release(m_Storage);
            }
        }

        /// <summary>
        /// Try to attach all accessories currently in a storage state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the the auto-attach delegate used during construction is returning true, this will 
        /// happen autoamtically as accessories are removed.  This method can be used if
        /// accessories need to be checked at other times.  For example: After running 
        /// <see cref="DetachAll"/> and need to re-attach all.
        /// </para>
        /// </remarks>
        public void TryAttachAll()
        {
            RecheckAccessories(true);
        }

        #endregion

        #region Utility Methods

        private void RecheckAccessories(bool canCheck)
        {
            if (!canCheck || m_IsRechecking)
                return;

            m_IsRechecking = true;  //  Prevents recursion due to unexpected events.

            // Designed to prevent iteration errors due to unexpected events.

            m_ProcessQueue.Clear();
            foreach (var item in m_Controllers)
                m_ProcessQueue.Enqueue(item);

            while (m_ProcessQueue.Count > 0)
            {
                var item = m_ProcessQueue.Dequeue();

                var accessory = item.Accessory;

                if (accessory.Status == AccessoryStatus.Stored)
                {
                    if (m_Attach(item.Accessory, item.Attach) == AttachStatus.FailedOnError)
                    {
                        Debug.LogError("Accessory under management failed to attach on error."
                            + " Removed from management: "
                            + item.Accessory.name, m_Storage);

                        item.Release(null);
                        item.Accessory.ReleaseOwnership(m_Storage);
                        m_Controllers.Remove(item);
                    }
                }
            }

            m_IsRechecking = false;
        }

        private void HandleAutoRelease(BodyAccessory.Controller controller, bool Destroyed)
        {
            m_Controllers.Remove(controller);
        }

        private BodyAccessory.Controller FindController(BodyAccessory accessory, bool remove = false)
        {
            for (int i = 0; i < m_Controllers.Count; i++)
            {
                var item = m_Controllers[i];

                if (item.Accessory == accessory)
                {
                    if (remove)
                        m_Controllers.RemoveAt(i);

                    return item;
                }
            }

            return null;
        }

        #endregion
    }
}

