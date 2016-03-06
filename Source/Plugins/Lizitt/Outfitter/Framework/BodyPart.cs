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
    /// A body part.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Body parts provide higher resolution collision data than can be provied by a single outfit collider.  
    /// <see cref="RigidbodyBehavior"/> is used to modify body part behavior as needed.
    /// </para>
    /// <para>
    /// While body parts are normally associated with an <see cref="Outfit"/>, the owner can be any GameObject.
    /// Standard components don't care.
    /// </para>
    /// </remarks>
    /// <seealso cref="Outfit"/>
    [AddComponentMenu(OutfitterUtil.Menu + "Body Part", OutfitterUtil.OutfitComponentMenuOrder + 1)]
    public class BodyPart
        : MonoBehaviour
    {
        /*
         * Design notes:
         * 
         * Collision behavior is an important feature of the body part, so the collider is treated as required
         * and is as strictly enforced as possible.  Rigidbody's are enforced, but less strictly, on the assumption
         * that the they will exist before they are needed.
         * 
         * Internal access to the collider should always start with calling the property so it can auto-initialize
         * if needed.  Later calls can be to the field.
         * 
         * The design assumes one of two use cases:  There is one rigidbody per collider.  Or, if a rigidbody is
         * shared, all of the rigidbody's colliders will be maintained with the same status.  I.e. All in raycast
         * mode, all in collision mode, etc.
         */

        [SerializeField]
        [Tooltip("The location of the body part.")]
        [SortedEnumPopup(typeof(BodyPartType))]
        private BodyPartType m_Type;

        /// <summary>
        /// The type of the body part.
        /// </summary>
        public BodyPartType PartType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        [SerializeField]
        [Tooltip(
            "The body part's collider. (If unassigned, defaults to the first collider found during a child search.)")]
        [RequiredValue(typeof(Collider), true)]
        private Collider m_Collider = null;

        /// <summary>
        /// The body part's collider. (Required)  
        /// </summary>
        /// <remarks>
        /// <para>
        /// If unassigned, defaults to the first collider found during a child search.
        /// </para>
        /// </remarks>
        public Collider Collider
        {
            get 
            {
                if (!m_Collider)
                {
                    var col = GetComponentInChildren<Collider>();

                    if (col)
                    {
                        if (Application.isPlaying)
                        {
                            m_Collider = col;
                            Debug.LogWarningFormat(this, "Body part did not have a collider: {0}. Auto-assigned: {1}",
                                PartType, col.name, this);
                        }
                    }
                }

                return m_Collider; 
            }
            set 
            { 
                m_Collider = value;

                // Complain lots.
                if (m_Collider)
                {
                    if (!m_Collider.GetAssociatedRigidBody())
                        Debug.LogWarningFormat("Body part collider does not have a rigidbody: {0}, Collider: {1}",
                            PartType, m_Collider.name, this);
                }
                else
                    Debug.LogWarning("Body part assigned a null collider: " + PartType, this);
            }
        }

        /// <summary>
        /// The body part's Rigidbody component.
        /// </summary>
        public Rigidbody Rigidbody
        {
            // Property first, then field.
            get { return Collider ? Collider.GetAssociatedRigidBody() : null; }
        }

        [SerializeField]
        [Tooltip("The data context of the body part. (Optional)")]
        private GameObject m_Context = null;

        /// <summary>
        /// The data context of the body part. (Optional)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an informational field used to provide context to users of the body part.  For example, it can
        /// be set to the agent that owns the outfit that owns the body part.
        /// </para>
        /// </remarks>
        public GameObject Context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }

        /// <summary>
        /// The body part collider's layer.
        /// </summary>
        public int ColliderLayer
        {
            get { return Collider ? Collider.gameObject.layer : 0; }
            set
            {
                if (Collider)
                    Collider.gameObject.layer = value;
            }
        }

        /// <summary>
        /// The body part collider's collision status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Changes to the status can result in changes to both the body part's collider and rigidbody.
        /// </para>
        /// </remarks>
        public ColliderBehavior ColliderBehavior
        {
            // Property first, then field.
            get { return Collider ? Collider.GetBehavior() : ColliderBehavior.Disabled; }
        }

        /// <summary>
        /// Synchronize the state of the specified body parts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The main purpose of this method is to synchronized the state of body parts on two different outfits.
        /// </para>
        /// <para>
        /// The status, layer, and context are synchronized, depending on the parameter values.  Other 
        /// properties such as body part type, transform values, etc., are not included.
        /// </para>
        /// </remarks>
        /// <param name="to">The body part to sync to. (Required)</param>
        /// <param name="from">The body part to sync from. (Required)</param>
        /// <param name="includeRigidbodyBehavior">Synchronize the collider status.</param>
        /// <param name="includeLayer">Synchronize the collider layer.</param>
        /// <param name="includeContext">Synchronize the context unless it is <paramref name="ignoreContext"/>.</param>
        /// <param name="ignoreContext">
        /// The context that should never be synchronized. (Usually the object <paramref name="from"/> is a member of,
        /// such at its outfit. (Required if <paramref name="includeContext"/> is true.)
        /// </param>
        public static void Synchronize(BodyPart to, BodyPart from,
            bool includeRigidbodyBehavior, bool includeLayer, bool includeContext, GameObject ignoreContext)
        {
            if (includeRigidbodyBehavior)
            {
                if (to.Collider && from.Collider)
                {
                    var fromRb = from.Collider.GetAssociatedRigidBody();
                    var toRb = to.Collider.GetAssociatedRigidBody();
                    if (fromRb & toRb)
                    {
                        toRb.SetBehavior(fromRb.GetBehavior());
                        toRb.useGravity = fromRb.useGravity;
                        if (Application.isPlaying)
                            toRb.detectCollisions = toRb.detectCollisions;
                    }
                }
            }

            if (includeLayer)
                to.ColliderLayer = from.ColliderLayer;

            if (includeContext && from.Context != ignoreContext.gameObject)
                to.Context = from.Context;
        }
    }
}
