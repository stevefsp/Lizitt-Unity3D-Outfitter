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
    /// Body parts provide higher resolution collision data than can be provied by a  single outfit collider.  
    /// <see cref="ColliderStatus"/> is used to  modify collider behavior as needed.
    /// </para>
    /// <para>
    /// While body parts are normally associated with an <see cref="Outfit"/>, the owner may any GameObject.
    /// Standard components don't care.
    /// </para>
    /// </remarks>
    /// <seealso cref="Outfit"/>
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
        [Tooltip("The type (location) of the body part.")]
        private BodyPartType m_Type;

        /// <summary>
        /// The type (location) of the body part.
        /// </summary>
        public BodyPartType PartType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        [SerializeField]
        [Tooltip("The body part's collider.")]
        [RequiredValue(typeof(Collider))]
        private Collider m_Collider = null;

        /// <summary>
        /// The body part's collider.
        /// </summary>
        public Collider Collider
        {
            get 
            { 
                if (!m_Collider)
                    m_Collider = GetComponentInChildren<Collider>();

                return m_Collider; 
            }
            set 
            { 
                if (!value)
                    Debug.LogError("Body part collider can't be null.", this);

                m_Collider = value;

                // Don't expect this to happen often, so benefit of these checks is worth it.
                var rb = Rigidbody;
                if (rb)
                {
                    if (!rb.isKinematic)
                    {
                        rb.isKinematic = true;
                        Debug.LogWarning(
                            PartType + ": Non-kinematic rigidbody detected on body part. Set rigidbody to kinemetic.", 
                            this);
                    }
                }
                else
                    Debug.LogWarning(PartType + ": Body part collider does not have a rigidbody.", this);
            }
        }

        /// <summary>
        /// The body part's Rigidbody component.
        /// </summary>
        public Rigidbody Rigidbody
        {
            // Property first, then field.
            get { return Collider ? m_Collider.GetAssociatedRigidBody() : null; }
        }

        [SerializeField]
        [Tooltip("The data context of the body part.  (Optional)")]
        private GameObject m_Context = null;

        /// <summary>
        /// The data context of the body part.  (Optional)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an informational field.  It can be used to provide information to users of the body part.  For 
        /// example, it can be set to the agent that owns the outfit that owns the body part.
        /// </para>
        /// <para>
        /// Standard components automatically initialize this value to their own GameObject if the value is not
        /// aleady assigned.  This is meant as a helpful automation.  The value can be reassigned as desired
        /// without impacting standard component behavior.
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
            // Property then field.
            get { return Collider ? m_Collider.gameObject.layer : 0; }
            set
            {
                if (Collider)
                    m_Collider.gameObject.layer = value;
            }
        }

        /// <summary>
        /// The body part collider's collision status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The status is dependant on a combination of the collider and rigidbody.  Changes to 
        /// the status result in changes to these components.
        /// </para>
        /// </remarks>
        public ColliderStatus ColliderStatus
        {
            get { return Collider ? m_Collider.GetStatus() : ColliderStatus.Disabled; }
            set
            {
                if (!Collider)
                {
                    Debug.LogError(PartType + ": Can't set status while the collider is null.", this);
                    return;
                }

                m_Collider.SetStatus(value);
            }
        }

        /// <summary>
        /// Synchronize the state of the specified body parts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The status, layer, and context are synchronized, depending on the parameter values.  Other 
        /// properties such as body part type, transform values, etc., are not included.
        /// </para>
        /// </remarks>
        /// <param name="to">The body part to sync to. (Required)</param>
        /// <param name="from">The body part to sync from. (Required)</param>
        /// <param name="includeStatus">Synchronize the collider status.</param>
        /// <param name="includeLayer">Synchronize the collider layer.</param>
        /// <param name="includeContext">Synchronize the context unless it is <paramref name="ignoreContext"/>.</param>
        /// <param name="ignoreContext">
        /// The context that should never be synchronized. (Usually the object <paramref name="to"/> is a member of,
        /// such at its Outfit. (Required if <paramref name="includeContext"/> is true.)
        /// </param>
        public static void Synchronize(BodyPart to, BodyPart from,
            bool includeStatus, bool includeLayer, bool includeContext, GameObject ignoreContext)
        {
            if (includeStatus)
                to.ColliderStatus = from.ColliderStatus;

            if (includeLayer)
                to.ColliderLayer = from.ColliderLayer;

            if (includeContext && from.Context != ignoreContext.gameObject)
                to.Context = from.Context;
        }
    }
}
