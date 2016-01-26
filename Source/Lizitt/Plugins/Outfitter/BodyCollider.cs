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
    /// A collider attached to or associated with a mount point.
    /// </summary>
    /// <remarks>
    /// Body colliders offer the option of a higher resolution collision structure than a 
    /// single collider structure.  The status of the collider to enable/disable the
    /// higher resoltuion behavior as needed.
    /// </remarks>
    public class BodyCollider
    {
        private Rigidbody m_RigidBody;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collider">The collider. (Required)</param>
        /// <param name="rigidBody">The rigid body. (Required)</param>
        /// <param name="mountPoint">The collider's mount point.</param>
        public BodyCollider(
            Collider collider, Rigidbody rigidBody, MountPointType mountPoint)
        {
            if (collider == null)
                throw new System.ArgumentNullException("collider");

            if (rigidBody == null)
                throw new System.ArgumentNullException("rigidBody");

            m_RigidBody = rigidBody;
            Collider = collider;
            MountPoint = mountPoint;
        }

        private Collider Collider { get; set; }  // Make public only when necessary.

        /// <summary>
        /// The mount point associated with the collider.
        /// </summary>
        public MountPointType MountPoint { get; private set; }

        /// <summary>
        /// The collider's game object.
        /// </summary>
        public GameObject GameObject
        {
            get { return Collider.gameObject; }
        }

        /// <summary>
        /// The collider's layer.
        /// </summary>
        public int Layer
        {
            get { return m_RigidBody.gameObject.layer; }
            set { m_RigidBody.gameObject.layer = value; }
        }

        /// <summary>
        /// The collider status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The status is dependant on the collider and rigidbody.  Changes to the status
        /// result in changes to the components.
        /// </para>
        /// </remarks>
        public BodyColliderStatus Status
        {
            get
            {
                // Do real checks just in case someone misbehaves and alters things
                // manually.
                if (m_RigidBody.detectCollisions)
                {
                    if (Collider.enabled)
                        return BodyColliderStatus.DetectCollision;
                }
                else if (Collider.enabled)
                        return BodyColliderStatus.Disabled;

                return BodyColliderStatus.Disabled;
            }

            set
            {
                switch (value)
                {
                    case BodyColliderStatus.DetectCollision:

                        Collider.enabled = true;
                        m_RigidBody.detectCollisions = true;
                        break;

                    case BodyColliderStatus.RaycastOnly:

                        m_RigidBody.detectCollisions = false;
                        Collider.enabled = true;
                        break;

                    case BodyColliderStatus.Disabled:

                        m_RigidBody.detectCollisions = false;
                        Collider.enabled = false;
                        break;
                }
            }
        }
    }
}
