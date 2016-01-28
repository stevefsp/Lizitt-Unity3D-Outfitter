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
    /// A body part.  (Usually part of an outfit.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Body parts offer a higher resolution collision structure than can be provied by a 
    /// single outfit collider.  The status of the body part collider is used to to 
    /// enable/disable collider behavior as needed.
    /// </para>
    /// </remarks>
    public class BodyPart
        : MonoBehaviour
    {
        /*
         * Design notes:
         * 
         * Collision behavior is an important feature of the body part, so it is considered required
         * and is as strictly enforced as possible.  Rigidbody's are enforced, but less strictly,
         * on the assumption that the they will exist before they are needed.
         * 
         * Internal access to the collider should always start with calling the property so it
         * can auto-initialize if needed.  Later calls can be to the field.
         * 
         * The design assumes one of two use cases:  There is one rigidbody per collider.  Or, if
         * a rigidbody is shared, all of the rigidbody's colliders will be maintained
         * with the same status.  I.e. All in raycast mode, all in collision mode, etc.
         */

        [SerializeField]
        [Tooltip("The type (location) of the body part.")]
        private BodyPartType m_Type;

        /// <summary>
        /// The type (location) of the body part.
        /// </summary>
        public BodyPartType Type
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
                        Debug.LogWarning(Type + ": Non-kinematic rigidbody detected on body part."
                            + " Set rigidbody to kinemetic.", this);
                    }
                }
                else
                {
                    Debug.LogWarning(
                        Type + ": Body part collider does not have a rigidbody.", this);
                }
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
        [Tooltip("The owner of the body part. (Such as an outfit.)  (Optional)")]
        private GameObject m_Owner = null;

        /// <summary>
        /// The owner of the body part. (E.g. An outfit.)  (Optional)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an informational field.  It can be set to provide information to users of
        /// the body part.
        /// </para>
        /// </remarks>
        public GameObject Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; }
        }

        /// <summary>
        /// The body part collider's layer.
        /// </summary>
        public int Layer
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
        public ColliderStatus Status
        {
            get
            {
                // Do real checks just in case something misbehaves and alters the components
                // manually.

                var rb = Rigidbody;
                if (rb && rb.detectCollisions)
                {
                    if (Collider.enabled)
                        return ColliderStatus.DetectCollision;
                }
                else if (Collider.enabled)
                    return ColliderStatus.RaycastOnly;

                return ColliderStatus.Disabled;
            }

            set
            {
                if (!Collider)
                {
                    Debug.LogError(Type + ": Can't set status while the collider is null.", this);
                    return;
                }

                var rb = Rigidbody;
                if (!rb && value == ColliderStatus.DetectCollision)
                {
                    Debug.LogError(Type + ": Can't set status while the rigidbody is null.", this);
                    return;
                }

                switch (value)
                {
                    case ColliderStatus.DetectCollision:

                        m_Collider.enabled = true;
                        rb.detectCollisions = true;

                        break;

                    case ColliderStatus.RaycastOnly:

                        if (rb)
                            rb.detectCollisions = false;
                        m_Collider.enabled = true;

                        break;

                    case ColliderStatus.Disabled:

                        if (rb)
                            rb.detectCollisions = false;
                        m_Collider.enabled = false;

                        break;
                }
            }
        }
    }
}
