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
    /// Synchronize the state of the previous outfit's body parts to the the current outfit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be an observer of any number of concurrent <see cref="Body"/> instances.
    /// </para>
    /// <para>
    /// Limitation: This observer is designed for memory efficiency.  It only synchronizes between two known 
    /// outfits, so the last known state is lost if the body outfit is set to null.
    /// </para>
    [CreateAssetMenu(menuName = LizittUtil.LizittMenu + "Sync BodyPart State",
        order = OutfitterUtil.BodyObserverMenuOrder + 2)]
    public class SyncBodyPartState
        : ScriptableObject, IBodyObserver
    {
        [SerializeField]
        [Tooltip("Persist the body part collider status.")]
        private bool m_IncludeStatus = true;

        /// <summary>
        /// Persist the body part coller status.
        /// </summary>
        public bool IncludeStatus
        {
            get { return m_IncludeStatus; }
            set { m_IncludeStatus = value; }
        }

        [SerializeField]
        [Tooltip("Persist the body part collider layer.")]
        private bool m_IncludeLayer = true;

        /// <summary>
        /// Persist the body part collider layer.
        /// </summary>
        public bool IncludeLayer
        {
            get { return m_IncludeLayer; }
            set { m_IncludeLayer = value; }
        }

        [SerializeField]
        [Tooltip("Persist the body part context unless it is the previous outfit's GameObject.")]  // << This is the correct behavior!
        private bool m_IncludeContext = false;

        /// <summary>
        /// Persist the body part context unless it is the previous outfit's GameObject.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The 'previous' body part's context will not be synchronized if the 'current' body part's context 
        /// if it is the previous outfit's GameObject.
        /// </para>
        /// </remarks>
        public bool IncludeContext
        {
            get { return m_IncludeContext; }
            set { m_IncludeContext = value; }
        }

        void IBodyObserver.OnOutfitChange(Body sender, Outfit previous)
        {
            Synchronize(sender.Outfit, previous);
        }

        void IBodyObserver.OnSoftReset(Body sender)
        {
            // Do nothing.
        }

        /// <summary>
        /// Synchronize the body part state of all common body parts based on the obvserver's configutation.
        /// </summary>
        /// <param name="to">The outfit being synchonized to. (Required)</param>
        /// <param name="from">The outfit being syncronzied from. (Required)</param>
        public void Synchronize(Outfit to, Outfit from)
        {
            Outfit.SynchronizeBodyPartState(to, from, m_IncludeStatus, m_IncludeLayer, m_IncludeContext);
        }

        /// <summary>
        /// Synchronize the state of the specified body part.
        /// </summary>
        /// <remarks>
        /// <para>
        /// At most the status, layer, and context are synchronized, depending on the observer settings.  Other 
        /// properties such as body part type, transform values, etc., are not included.
        /// </para>
        /// </remarks>
        /// <param name="to">The body part to sync to. (Required)</param>
        /// <param name="from">The body part to sync from. (Required)</param>
        /// <param name="ignoreContext">
        /// The context that should never be synchronized. (Usually the <paramref name="to"/>'s GameObject. 
        /// (Required if observer is configured to include context.)
        /// </param>
        public virtual void Synchronize(BodyPart to, BodyPart from, GameObject ignoreContext)
        {
            BodyPart.Synchronize(to, from, m_IncludeStatus, m_IncludeLayer, m_IncludeContext, ignoreContext);
        }
    }
}
