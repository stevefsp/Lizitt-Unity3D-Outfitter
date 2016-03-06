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
    /// Sets the animator controller of incoming outfits.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only supports one animator component per outfit.
    /// </para>
    /// <para>
    ///  Active at design-time. Can observe multiple <see cref="Body"/> instances.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = OutfitterUtil.AssetMenu + "Set Body Animator", order = OutfitterUtil.BodyMenuOrder)]
    public class SetBodyAnimator
        : BodyObserverObject
    {
        #region Settings

        [Space]
        [SerializeField]
        [Tooltip("The controller to apply to incoming outfits.")]
        [RequiredValue(typeof(RuntimeAnimatorController))]
        private RuntimeAnimatorController m_Controller = null;  // TODO: Add setter/getter.

        [Space]
        [SerializeField]
        [Tooltip("Always set the incoming outfit's animator controller.  Otherwise, only set the controller if it"
            + " doesn't already have one.")]
        private bool m_AlwaysOverride = false;

        /// <summary>
        /// Always set the incoming outfit's animator controller.  Otherwise, only set the controller if it
        /// doesn't already have one.
        /// </summary>
        public bool AlwaysOverride
        {
            get { return m_AlwaysOverride; }
            set { m_AlwaysOverride = value; }
        }

        [SerializeField]
        [Tooltip("Remove the outgoing outfit's animator controller, unless the outfit was force released.")]
        private bool m_IncludeRemoval = false;

        /// <summary>
        /// Remove the outgoing outfit's animator controller, unless the outfit was force released.
        /// </summary>
        /// <seealso cref="SetOutfit(Outfit, bool)"/>
        public bool IncludeRemoval
        {
            get { return m_IncludeRemoval; }
            set { m_IncludeRemoval = value; }
        }

        #endregion

        #region Body Observer

        protected sealed override void OnOutfitChange(Body sender, Outfit previous, bool wasForced)
        {
            Apply(sender.Outfit);

            if (m_IncludeRemoval && !wasForced)
            {
                var anim = GetAnimator(previous);
                if (anim)
                    anim.runtimeAnimatorController = null;
            }
        }

        #endregion

        #region Main Handler

        /// <summary>
        /// Apply the animator to the outfit based on the observer's settings.
        /// </summary>
        /// <param name="outfit"></param>
        public void Apply(Outfit outfit)
        {
            if (!outfit)
                return;

            var anim = GetAnimator(outfit);

            if (anim)
            {
                if (m_AlwaysOverride || !anim.runtimeAnimatorController)
                    anim.runtimeAnimatorController = m_Controller;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get the outfit's animator, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default behavior is to use <see cref="Outfit.GetAnimator()"/>.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit. (May be null.)</param>
        /// <returns>The outfit's animator, or null if none found.</returns>
        protected virtual Animator GetAnimator(Outfit outfit)
        {
            return outfit ? outfit.GetAnimator() : null;
        }

        #endregion
    }
}
