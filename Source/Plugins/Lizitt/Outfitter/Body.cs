﻿/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
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
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// A body used to persist settings as outfits are changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The body is useful when an agent's outfits are transient in nature.  It allows important settings and
    /// components to be persisted as outfits are swapped in and out. For example, adding an accessory, such as
    /// a perception system, to the body will result in the accessory automatically transfering between outfits
    /// as they change.  Observers can be used to apply, override, and persist settings not automatically handled
    /// by the body. E.g. Apply material overrides, persist collider status, etc. 
    /// </para>
    /// </remarks>
    public abstract class Body
        : MonoBehaviour
    {
        #region Core

        /// <summary>
        /// The transform that currently controls body motion and represents its location in the world.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The body is guarenteed to have a motion root even when it doesn't have an outfit. If there is an outfit, 
        /// this will be the outfit's motion root.  Otherwise it will be the body's default motion root.
        /// </para>
        /// <para>
        /// Important: This is a dynamic value.  It changes whenever the outfit is changed.
        /// </para>
        /// </remarks>
        public abstract Transform MotionRoot { get; }

        /// <summary>
        /// The current outfit's primary collider, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: This is a dynamic value.  It changes whenever the outfit is changed.
        /// </para>
        /// </remarks>
        public abstract Collider PrimaryCollider { get; }

        /// <summary>
        /// The current outfit's primary rigidbody, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: This is a dynamic value.  It changes whenever the outfit is changed.
        /// </para>
        /// </remarks>
        public abstract Rigidbody PrimaryRigidBody { get; }

        #endregion

        #region Outfit

        /// <summary>
        /// The current outfit, or null if there is none.
        /// </summary>
        public abstract Outfit Outfit { get; }

        /// <summary>
        /// Sets the current outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is an error that prevents <paramref name="outfit"/> from being applied then its reference 
        /// is returned, so <c>SetOutfit(outfit) == outfit</c> indicates a failure.
        /// </para>
        /// </remarks>
        /// <param name="outfit">
        /// The outfit to apply to the body, or null to remove the current outfit.
        /// </param>
        /// <returns>
        /// The previous outfit, or a refererence to <paramref name="outfit"/> if there was an error.
        /// </returns>
        public abstract Outfit SetOutfit(Outfit outfit);

        #endregion

        #region Body Accessories  (Local, not outfit.)

        public abstract IBodyAccessoryManager Accessories { get; }

        #endregion

        #region Observers

        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All standard implementations require the observer to be a Unity Object for serialization, so it is 
        /// acceptable for the body to reject an observer.  An error message will be logged if an observer is rejected.
        /// </para>
        /// <para>
        /// An observer can only be added once.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required)</param>
        /// <returns>
        /// True if the observer was accepted or already added.  False if the observer was rejected.
        /// </returns>
        public abstract bool AddObserver(IBodyObserver observer);

        /// <summary>
        /// Remove the specified event observer.
        /// </summary>
        /// <param name="observer">The observer to remove.</param>
        public abstract void RemoveObserver(IBodyObserver observer);

        #endregion

#if UNITY_EDITOR

        #region Editor Only

        /// <summary>
        /// Add all Unity Objects that may change while performing body operations to the provided list. 
        /// (Including the body itself.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo can result in changes
        /// being lost.  When updating the body in the editor, this method will be used by the base class to 
        /// obtain a list of all known Unity Objects that may be impacted by changes to the body.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale in the editor.  Concrete implementaitons must place it
        /// inside a UNITY_EDITOR conditional compile section.
        /// </para>
        /// </remarks>
        /// <param name="list">The list to add objects to.  (Required)</param>
        protected abstract void GetUndoObjects(List<Object> list);

        /// <summary>
        /// Add all Unity Objects that may change while performing body operations to the provided list. 
        /// (Including the body itself.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo can result in changes
        /// being lost.  When updating the body in the editor, this method can be used to obtain a list of all 
        /// known Unity Objects that may be impacted by changes to the body.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale for use in the editor.
        /// </para>
        /// </remarks>
        /// <param name="body">The body. (Required.)</param>
        /// <param name="list">The list to add objects to.</param>
        /// <returns>
        /// The reference to <paramref name="list"/> if it is provided.  Otherwise a reference to a newly created list.
        /// </returns>
        public static List<Object> UnsafeGetUndoObjects(Body body, List<Object> list = null)
        {
            if (list == null)
                list = new List<Object>();

            body.GetUndoObjects(list);

            return list;
        }

        #endregion

#endif
    }
}