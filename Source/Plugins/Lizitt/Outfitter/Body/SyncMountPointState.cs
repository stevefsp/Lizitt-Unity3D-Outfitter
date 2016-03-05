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
    /// Synchronize the state of the outgoing outfit's mount points to the current outfit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Active at design-time. Can observe multiple <see cref="Body"/> instances.
    /// </para>
    /// <para>
    /// This observer is designed for memory efficiency.  It only synchronizes between two known outfits, so the last 
    /// known state is lost if the body outfit is set to null.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = OutfitterUtil.AssetMenu + "Sync MountPoint State", order = OutfitterUtil.BodyMenuOrder)]
    public class SyncMountPointState
        : BodyObserverObject
    {
        [SerializeField]
        [Tooltip("Synchronize the mount point 'is blocked' state.")]
        private bool m_IncludeBlocked = true;

        /// <summary>
        /// Synchronize the mount point 'is blocked' state.
        /// </summary>
        public bool IncludeBlockedState
        {
            get { return m_IncludeBlocked; }
            set { m_IncludeBlocked = value; }
        }

        [SerializeField]
        [Tooltip("Persist the context unless it is the outgoing outfit's GameObject.")]  // This is the correct behavior.
        private bool m_IncludeContext = false;

        /// <summary>
        /// Persist the context unless it is the outgoing outfit's GameObject.
        /// </summary>
        /// </remarks>
        public bool IncludeContext
        {
            get { return m_IncludeContext; }
            set { m_IncludeContext = value; }
        }

        protected sealed override void OnOutfitChange(Body sender, Outfit previous, bool wasForced)
        {
            Synchronize(sender.Outfit, previous);
        }

        /// <summary>
        /// Synchronize the mount point state of all common mount points using the observer's settings.
        /// </summary>
        /// <param name="to">The outfit being synchonized to. (Required)</param>
        /// <param name="from">The outfit being syncronzied from. (Required)</param>
        public virtual void Synchronize(Outfit to, Outfit from)
        {
            Outfit.SynchronizeMountPointState(to, from, m_IncludeBlocked, m_IncludeContext);
        }

        /// <summary>
        /// Synchronize the state to the specified mount point using the observer's settings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The block state and context are synchronized, depending on the observer settings.  Other 
        /// properties such as location type, transform values, etc., are ignored.
        /// </para>
        /// </remarks>
        /// <param name="to">The mount point to sync to. (Required)</param>
        /// <param name="from">The mount point to sync from. (Required)</param>
        /// The context that should never be synchronized. (Usually the <paramref name="from"/>'s GameObject. 
        /// (Required if observer is configured to include context.)
        /// </param>
        public virtual void Synchronize(MountPoint to, MountPoint from, GameObject ignoreContext)
        {
            MountPoint.Synchronize(to, from, m_IncludeBlocked, m_IncludeContext, ignoreContext);
        }
    }
}
