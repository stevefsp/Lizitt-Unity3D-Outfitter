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
    /// Synchronizes the animator controller and, optionally, the animator state from the previous body outfit to
    /// the current outfit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The component only synchronzes two existing animators, so no change is made to the current outfit if there 
    /// is no previous outfit or the previous outfit does not have an animator.
    /// </para>
    /// <para>
    /// The state synchronization will fail if the animator is in transition at the time of the observer event. Only 
    /// supports one Animator component per outfit. 
    /// </para>
    /// <para>
    /// Active at design-time with the exception of state synchronization. Can be an observer of any number of 
    /// <see cref="Body"/> instances.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = OutfitterUtil.AssetMenu + "Sync Body Outfit Animators", order = OutfitterUtil.BodyMenuOrder)]
    public class SyncBodyAnimators
        : BodyObserverObject
    {
        #region Settings

        [SerializeField]
        [Tooltip("Remove the previous outfit's animator controller after syncronization is complete, unless the"
            + " outfit was force released.")]
        private bool m_IncludeRemoval = true;

        /// <summary>
        /// Remove the outgoing outfit's animator controller after syncronization is complete, unless the outfit 
        /// was force released.
        /// </summary>
        /// <seealso cref="SetOutfit(Outfit, bool)"/>
        public bool IncludeRemoval
        {
            get { return m_IncludeRemoval; }
            set { m_IncludeRemoval = value; }
        }

        [SerializeField]
        [Tooltip("Synchronize state from the previous outfit to the current outfit.")]
        private bool m_IncludeState = false;

        /// <summary>
        /// Synchronize controller state from the previous outfit to the current outfit.
        /// </summary>
        public bool IncludeState
        {
            get { return m_IncludeState; }
            set { m_IncludeState = value; }
        }

        #endregion

        #region Body Observer

        protected sealed override void OnOutfitChange(Body sender, Outfit previous, bool wasForced)
        {
            Synchronize(sender.Outfit, previous, wasForced);
        }

        #endregion

        #region Main Handler

        /// <summary>
        /// Perform a synchronization based on the observer's settings.
        /// </summary>
        /// <param name="to">The outfit being synchronized to.</param>
        /// <param name="from">The outfit whose state is being synchronized to <paramref name="to"/>.</param>
        /// <param name="blockRemoval">
        /// If true, ignore the <see cref="IncludeRemoval"/> setting and block removal of the controller from
        /// <paramref name="from"/>
        /// </param>
        public void Synchronize(Outfit to, Outfit from, bool blockRemoval = false)
        {
            var currAnim = to ? GetAnimator(to) : null;
            var prevAnim = from ? GetAnimator(from) : null;

            if (currAnim && prevAnim)
            {
                currAnim.runtimeAnimatorController = prevAnim.runtimeAnimatorController;

                if (Application.isPlaying && m_IncludeState && prevAnim.runtimeAnimatorController)
                    PerformAnimatorSync(currAnim, prevAnim);
            }

            if (m_IncludeRemoval && !blockRemoval && prevAnim)
                prevAnim.runtimeAnimatorController = null;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get the animator for the outfit, or null if none found.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default behavior is to use <see cref="Outfit.GetAnimator()"/>.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <returns>The animator, or null if none found.</returns>
        protected virtual Animator GetAnimator(Outfit outfit)
        {
            // TODO: EVAL: Make this public?
            return outfit.GetAnimator();
        }

        /// <summary>
        /// Synchronize the controller state from the previous to the current animator.
        /// </summary>
        /// <param name="current">The current animator.</param>
        /// <param name="previous">The previous animator.</param>
        protected virtual void PerformAnimatorSync(Animator current, Animator previous)
        {
            // TODO: EVAL: Make this public?
            current.SynchronizeFrom(previous);
        }

        #endregion
    }
}
