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
    /// An accessory attached to a body.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Warning: Do not make concrete classes a required component.
    /// I.e. Don't do this: [RequireComponent(typeof(BodyAccessory))]
    /// Doing so can prevent proper conversion to a static non-accessory object which is 
    /// required for baking an outfit.
    /// </para>
    /// <para>
    /// This class enforces all required accessory behavior.
    /// </para>
    /// </remarks>
    public abstract partial class BodyAccessory
    {
        /// <summary>
        /// Implements the control interface for the accessory.
        /// </summary>
        public class Controller
        {
            #region Delegates

            /// <summary>
            /// A method that is called when a controller is released automatically for some reason.
            /// </summary>
            /// <param name="controller">The controller being released.</param>
            /// <param name="destroyed">
            /// True if the release is due to the accessory being destoryed.
            /// </param>
            public delegate void AutoRelease(Controller controller, bool destroyed);

            #endregion


            private readonly AutoRelease m_OnAutoRelease;
            private readonly AttachMethod m_Attach;

            /// <summary>
            /// The accessory associated with the controller.
            /// </summary>
            public BodyAccessory Accessory { get; private set; }

            /// <summary>
            /// Undocumented. Internal use only.
            /// </summary>
            internal Object Owner { get; private set; }

            /// <summary>
            /// Undocumented. Internal use only.
            /// </summary>
            internal Controller(BodyAccessory accessory, Object owner, AutoRelease onAutoRelease)
            {
                Accessory = accessory;
                Owner = owner;
                m_OnAutoRelease = onAutoRelease;

                m_Attach = new AttachMethod(this.LocalAttach);
            }

            /// <summary>
            /// True if the controller is currently active.  (Has not been released.)
            /// </summary>
            public bool IsValid
            {
                get { return Accessory && Owner; }
            }

            /// <summary>
            /// Attach the accessory.
            /// </summary>
            public AttachMethod Attach
            {
                get { return m_Attach; }
            }

            /// <summary>
            /// Store the accessory on the target.
            /// </summary>
            /// <para>
            /// It is only valid to call this method while the accessory is in the limbo state.
            /// </para>
            /// <param name="target">
            /// The transform to parent the accessory to for storage.
            /// </param>
            /// <param name="allowRootStorage">
            /// If true a null <paramref name="target"/> is allowed.
            /// </param>
            /// <returns>True if the storage was successful.</returns>
            public bool Store(Transform target, bool allowRootStorage = false)
            {
                if (!CheckValid())
                    return false;

                return Accessory.Store(target, allowRootStorage);
            }

            /// <summary>
            /// Release the accessory.
            /// </summary>
            /// <param name="toStorage">
            /// The parent of the object after the release. (Can be null.)
            /// </param>
            /// <returns>True if the release was successful.</returns>
            public bool Release(Transform toStorage)
            {
                if (!CheckValid())
                    return false;

                return Accessory.Release(toStorage);
            }

            /// <summary>
            /// Purges the accessory. (Destorys the accessory state.  It becomes a
            /// static non-accessory object.)
            /// </summary>
            /// <returns>True if the accessory was successfully purged.</returns>
            public bool Purge()
            {
                if (!CheckValid())
                    return false;

                return Accessory.Purge();
            }

            private bool CheckValid()
            {
                if (!IsValid)
                {
                    Debug.LogError("Controller is not valid. Accessory and/or owner has been cleared.");
                    return false;
                }

                return true;
            }

            private bool LocalAttach(Transform target)
            {
                if (!CheckValid())
                    return false;

                return Accessory.Attach(target);
            }

            /// <summary>
            /// Undocumented. Internal use only.
            /// </summary>
            internal void SendAutoRelease(bool destroyed)
            {
                Owner = null;

                if (m_OnAutoRelease != null)
                    m_OnAutoRelease(this, destroyed);
            }
        }
    }
}

