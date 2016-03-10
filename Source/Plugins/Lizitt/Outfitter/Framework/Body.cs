/*
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
    /// There are two primary use cases for a body:
    /// </para>
    /// <para>
    /// An agent's outfits are transient in nature - The body allows important settings and
    /// components to be persisted as outfits are swapped in and out. For example, adding an accessory such as
    /// a perception system to the body will result in the accessory automatically transfering between outfits
    /// as they change. 
    /// </para>
    /// <para>
    /// The outfits supplied to an agent are generic in nature and per agent adjustements need to be made - The body
    /// can be preloaded with accessories specific to the agent.  Observers can be added to apply overrides and other
    /// agent specific settings.
    /// </para>
    /// <para>
    /// The only built in persitance is for accessories.  But observers can be used to apply, override, and 
    /// persist other settings. E.g. Apply material overrides, persist collider status, etc. 
    /// </para>
    /// </remarks>
    public abstract class Body
        : MonoBehaviour
    {
        // TODO: v0.3: Add IsAccessoryMounting() method.

        #region Core

        /// <summary>
        /// The transform that represents body's motion, location, and orientation in the world.
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
        /// <para>
        /// <paramref name="forceRelease"/> is used for operations such as baking, where the outfit being released
        /// needs to maintain its current state rather than undergo reversion operations.  Any accessories managed 
        /// by the body will be left in place rather than removed and stored in the body.  Observers decide for
        /// themselves how to respond to a forced release.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit to apply to the body, or null to remove the current outfit.</param>
        /// <param name="forceRelease">Release the current outfit with minimal state changes.</param>
        /// <returns>The previous outfit, or a refererence to <paramref name="outfit"/> if there was an error.</returns>
        public abstract Outfit SetOutfit(Outfit outfit, bool forceRelease);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        /// <summary>
        /// Sets the current outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is an error that prevents <paramref name="outfit"/> from being applied then its reference 
        /// is returned, so <c>SetOutfit(outfit) == outfit</c> indicates a failure.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit to apply to the body, or null to remove the current outfit.</param>
        /// <param name="forceRelease">Release the current outfit with minimal state changes.</param>
        /// <returns>The previous outfit, or a refererence to <paramref name="outfit"/> if there was an error.</returns>
        public Outfit SetOutfit(Outfit outfit)
        {
            return SetOutfit(outfit, false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Body Accessories  (Local, not outfit.)

        /// <summary>
        /// The body's accessory manager.
        /// </summary>
        public abstract IBodyAccessoryManager Accessories { get; }

        #endregion

        #region Observers

        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An observer can only be added once and it must be implmented by a UnityEngine.Object for serialization 
        /// puposes.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required. Must be a UnityEngine.Object.)</param>
        /// <returns>
        /// True if the observer was accepted or already added, or false if the observer is not implemented by
        /// a UnityEngine.Object.
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
        /// Add all UnityEngine.Objects that may change while performing body operations to the provided list. 
        /// (Including the body itself.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo method can result in changes
        /// being lost.  When updating the body in the editor, this method will be used by the base class to 
        /// obtain a list of all known UnityEngine.Object's that may be impacted by changes to the body.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale in the editor.  Concrete implementaitons must place it
        /// inside a UNITY_EDITOR conditional compile section.
        /// </para>
        /// </remarks>
        /// <param name="list">The list to add objects to.  (Required)  (Will not be cleared prior to use.)</param>
        protected abstract void GetUndoObjects(List<Object> list);

        /// <summary>
        /// Add all UnityEngine.Objects that may change while performing body operations to the provided list. 
        /// (Including the body itself.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity is very finicky about changes to scene and project assets through code while outside of play mode.
        /// Failure to properly register changes through either a SerializedObject or Undo method can result in changes
        /// being lost.  When updating the body in the editor, this method will be used by the base class to 
        /// obtain a list of all known UnityEngine.Object's that may be impacted by changes to the body.
        /// </para>
        /// <para>
        /// Warning: This method is only avaibale in the editor.  Concrete implementaitons must place it
        /// inside a UNITY_EDITOR conditional compile section.
        /// </para>
        /// </remarks>
        /// <param name="body">The body. (Required.)</param>
        /// <param name="list">The list to add objects to. (Will not be cleared prior to use.)</param>
        /// <returns>
        /// The reference to <paramref name="list"/> if it was provided.  Otherwise a reference to a newly created list.
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
