﻿/*
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
    /// A component that implements persistant accessory management for a <see cref="Body"/> or body-like object 
    /// where the target is an <see cref="Outfit"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component is usually a required component of a <see cref="Body"/>, but it can act as a persistant
    /// accessory manager for any component that manages an outfit.  Just keep the outfit up-to-date then 
    /// add/modify/remove accessories.
    /// </para>
    /// <para>
    /// This component keeps track of its accessories.  If ownership is taken over by an external component it will
    /// release automatically remove the accessory.
    /// </para>
    /// </remarks>
    [SerializeField]
    public class BodyAccessoryManager
        : MonoBehaviour, IAccessoryObserver, IBodyAccessoryManager
    {
        #region Outfit

        /// <summary>
        /// The outfit that accessories are to be applied to, or null if there is no current
        /// outfit.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private Outfit m_Outfit = null;
        public Outfit Outfit
        {
            get { return m_Outfit; }
        }

        /// <summary>
        /// Set the current outfit and update accessory mounting as appropriate.
        /// </summary>
        /// <remarks>
        /// The behavior of each accessory during a change in the outfit is determined by
        /// the method used to add it to the outfit manager.
        /// </remarks>
        /// <param name="outfit">The outfit, or null if there is no outfit.</param>
        public void SetOutfit(Outfit outfit)
        {
            if (m_Outfit == outfit)
                return;

            m_Outfit = outfit;

            // The loop has to be forward.  A user may expect that behavior.  E.g. Accessories
            // earlier in the list have priority.

            bool purgeNeeded = false;
            for (int i = 0; i < m_Items.Count; i++)
            {
                var mountInfo = m_Items[i];

                if (mountInfo.Accessory)
                {
                    if (!m_Outfit || MountToOutfit(ref mountInfo) != MountResult.Success)
                        StoreAccessory(ref mountInfo);
                }
                else
                {
                    Debug.LogError("Improperly destroyed accessory detected. Will purge.", this);
                    purgeNeeded = true;
                    continue;
                }

                m_Items[i] = mountInfo;
            }

            if (purgeNeeded)
                PurgeNullAcessories();
        }

        #endregion

        #region Accessories General

        [SerializeField]
        [HideInInspector]
        private List<AccessoryMountInfo> m_Items = new List<AccessoryMountInfo>(4);

        /// <summary>
        /// The number of accessories under management.
        /// </summary>
        public int Count
        {
            get { return m_Items.Count; }
        }

        public AccessoryMountInfo this[int index]
        {
            get { return m_Items[index]; }
        }

        public AccessoryMountInfo this[Accessory accessory]
        {
            get
            {
                if (accessory)
                {
                    for (int i = 0; i < m_Items.Count; i++)
                    {
                        if (m_Items[i].Accessory == accessory)
                            return m_Items[i];
                    }
                }

                return new AccessoryMountInfo();
            }
        }

        public bool Contains(Accessory accessory)
        {
            if (!accessory)
                return false;

            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Accessory == accessory)
                    return true;
            }

            return false;
        }

        #endregion

        #region Add Accessory

        /// <summary>
        /// Mount the accessory to all outfits, or store it when the it can't be mounted.
        /// </summary>
        /// <param name="accessory">The accessory to add.</param>
        /// <param name="addSettings">The accessory mount settings.</param>
        /// <param name="mustMount">
        /// If true a failure to immediately mount will result in a failure to add.  Otherwise
        /// a failure to immeidately mount will result in the accessory being stored.
        /// </param>
        /// <returns>
        /// The result of the add operation.  (Will only ever be 'success' or 'failure'.)
        /// </returns>
        public MountResult Add(
            Accessory accessory, AccessoryAddSettings addSettings, bool mustMount = false)
        {
            if (!accessory)
            {
                Debug.LogError("Accessory is null.");
                return MountResult.FailedOnError;
            }

            // Remember: Don't need to check mounter validity.  Settings setter does that.

            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Accessory && m_Items[i].Accessory == accessory)
                {
                    // This is an error.  Must use modify method to change configuration.
                    Debug.LogError("Accessory is already added: " + accessory.name, this);
                    return MountResult.FailedOnError;
                }
            }

            MountResult status = MountResult.FailedOnError;

            var mountInfo = new AccessoryMountInfo();
            mountInfo.Accessory = accessory;
            mountInfo.Apply(addSettings);

            if (m_Outfit)
                status = MountToOutfit(ref mountInfo);
            else if (mustMount)
            {
                Debug.LogError("Must succeed failure.  No outfit: " + accessory.name, this);
                return MountResult.FailedOnError;
            }

            bool isMounted = (status == MountResult.Success);
            if (!isMounted)
            {
                if (mustMount)
                {
                    Debug.LogErrorFormat(this,
                        "Must succeed failure.  Failed to mount to outfit: Accessory: {0}, Status: {1}",
                        accessory.name, status);

                    return MountResult.FailedOnError;
                }

                StoreAccessory(ref mountInfo);
            }

            LinkAccessory(mountInfo);

            return isMounted ? MountResult.Success : MountResult.Stored;
        }


        public MountResult Add(
            Accessory accessory, MountPointType locationType, bool ignoreRestrictions = false, bool mustMount = false)
        {
            var settings = new AccessoryAddSettings();
            settings.IgnoreRestrictions = ignoreRestrictions;
            settings.LocationType = locationType;
            settings.Mounter = null;
            settings.AdditionalCoverage = 0;

            return Add(accessory, settings, mustMount);
        }

        public MountResult Add(Accessory accessory, bool ignoreRestrictions = false, bool mustMount = false)
        {
            var settings = new AccessoryAddSettings();
            settings.IgnoreRestrictions = ignoreRestrictions;
            settings.LocationType = accessory.DefaultLocationType;
            settings.Mounter = null;
            settings.AdditionalCoverage = 0;

            return Add(accessory, settings, mustMount);
        }

        #endregion

        #region Modify Accessory

        /// <summary>
        /// Modify the settings for an existing accessory.  (Performs remounting as needed.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will always attempt a mount/remount if <see cref="Outfit"/> is non-null.
        /// </para>
        /// <para>
        /// A failure to mount when needed will result in a transition to storage.  All other errors will
        /// result in no change to the accessory's state, so an accessory will never be discarded by a failed
        /// modify.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to change.</param>
        /// <param name="settings">The new settings for the accessory.</param>
        /// <returns>The result of the modification.</returns>
        public MountResult Modify(Accessory accessory, AccessoryAddSettings settings)
        {
            if (!accessory)
            {
                Debug.LogError("Can't modify a null accessory.", this);
                return MountResult.FailedOnError;
            }

            // Remember: Don't need to check mounter validity.  Settings setter does that.

            for (var i = 0; i < m_Items.Count; i++)
            {
                var mountInfo = m_Items[i];


                if (mountInfo.Accessory && mountInfo.Accessory == accessory)
                {
                    /*
                     * Design note:
                     * 
                     * There can be lots of reasons for modifying an accessory, some of which don't need/want a
                     * re-mount.  But it is better to keep it simple.  All modify calls for a mounted accessory
                     * result in a mount.)
                     */
                    mountInfo.Apply(settings);

                    if (m_Outfit)
                    {
                        if (MountToOutfit(ref mountInfo) != MountResult.Success)
                            StoreAccessory(ref mountInfo);
                    }
                    // else it is already in storage.

                    m_Items[i] = mountInfo;
                    
                    return mountInfo.Accessory.Status.IsMounted() ? MountResult.Success : MountResult.Stored;
                }
            }

            Debug.LogError("Attempt made to modify an unknown accessory: " + accessory.name, this);
            return MountResult.FailedOnError;
        }

        public MountResult Modify(Accessory accessory, MountPointType locationType, bool ignoreRestrictions = false)
        {
            var settings = new AccessoryAddSettings();
            settings.IgnoreRestrictions = ignoreRestrictions;
            settings.LocationType = locationType;

            return Modify(accessory, settings);
        }

        #endregion

        #region Remove Accessory

        /// <summary>
        /// Removes the accessory from management, unmounting as needed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are vaious reasons why an accessory that was added isn't recognized by the
        /// remove operation.  Some examples:  The accessory may have unmounted itself or been
        /// unmounted directly through the outfit. Outfit specific accessories are auto-removed 
        /// when the outfit changes.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to remove.</param>
        /// <param name="priorityMounter">
        /// The priority mounter to use if unmounting is required.  If non-null, this will
        /// override any value provided during the add operation.
        /// </param>
        /// <returns>
        /// True if accessory was removed, false if the accessory is unrecognized.
        /// </returns>
        public bool Remove(Accessory accessory)
        {
            if (!accessory)
                return false;

            for (int i = 0; i < m_Items.Count; i++)
            {
                var mountInfo = m_Items[i];

                if (mountInfo.Accessory == accessory)
                {
                    UnlinkAccessory(i);
                    accessory.Release();  // Keep it simple.

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Reset

        public void Reset()
        {
            m_Outfit = null;

            for (int i = m_Items.Count - 1; i >= 0; i--)
            {
                if (m_Items[i].Accessory)
                {
                    if (m_Items[i].Accessory.Owner == this)
                    {
                        UnlinkAccessory(i);
                        m_Items[i].Accessory.Release();
                    }
                }
            }

            m_Items.Clear();
        }

        #endregion

        #region Utility Members

        private Accessory m_IgnoreAccessoryStateEvent;

        /// <summary>
        /// Attempts to mount the accessory to the current outfit and updates its information.
        /// (The outfit must be non-null!)
        /// </summary>
        private MountResult MountToOutfit(ref AccessoryMountInfo mountInfo)
        {
            // If this is a remount and it fails the accessory will be released.  So...
            m_IgnoreAccessoryStateEvent = mountInfo.Accessory;

            var status = m_Outfit.Mount(mountInfo.Accessory, mountInfo.LocationType,
                mountInfo.IgnoreRestrictions, mountInfo.Mounter, mountInfo.AdditionalCoverage);

            m_IgnoreAccessoryStateEvent = null;

            return status;
        }

        private bool StoreAccessory(ref AccessoryMountInfo mountInfo)
        {
            var accessory = mountInfo.Accessory;
            
            accessory.Store(gameObject);

            accessory.transform.parent = transform;
            accessory.transform.localPosition = Vector3.zero;
            accessory.transform.localRotation = Quaternion.identity;

            return true;
        }

        private void PurgeNullAcessories()
        {
            for (int i = m_Items.Count - 1; i >= 0; i--)
            {
                if (!m_Items[i].Accessory)
                    m_Items.RemoveAt(i);
            }
        }

        private void LinkAccessory(AccessoryMountInfo info)
        {
            m_Items.Add(info);
            info.Accessory.AddObserver(this);
        }

        private void UnlinkAccessory(Accessory accessory)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Accessory == accessory)
                {
                    UnlinkAccessory(i);
                    return;
                }
            }
        }

        private void UnlinkAccessory(int index)
        {
            var accessory = m_Items[index].Accessory;

            if (accessory)
                accessory.RemoveObserver(this);

            m_Items.RemoveAt(index);
        }

        #endregion

        #region Accessory Observer

        void IAccessoryObserver.OnStateChange(Accessory sender)
        {
            if (m_IgnoreAccessoryStateEvent == sender)
                return;

            switch (sender.Status)
            {
                case AccessoryStatus.Unmanaged:

                    UnlinkAccessory(sender);
                    break;

                case AccessoryStatus.Stored:

                    if (sender.Owner != gameObject)
                        UnlinkAccessory(sender);

                    break;

                default:

                    if (!m_Outfit || sender.Owner != m_Outfit.gameObject)
                        UnlinkAccessory(sender);

                    break;
            }
        }

        void IAccessoryObserver.OnDestroy(Accessory sender, DestroyType typ)
        {
            UnlinkAccessory(sender);
        }

        #endregion
    }
}