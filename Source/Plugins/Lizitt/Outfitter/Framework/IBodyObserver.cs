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
    /// A component that responds to <see cref="Body"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Body"/> requires that this interface to be implemented by a UnityEngine.Object for serialization 
    /// purposes.
    /// </para>
    /// </remarks>
    public interface IBodyObserver
    {
        /// <summary>
        /// Sent at the end of an outfit change operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="previous"/> is the outfit that was replaced. The current outfit can be obtained 
        /// from <paramref name="sender"/>.  Both the previous and current outfits may be null.
        /// </para>
        /// <para>
        /// This event is useful for clients that need to synchronize or persist state between outfits. E.g. Persist
        /// material state, synchronise animators, etc.
        /// </para>
        /// <para>
        /// If <paramref name="wasForced"/> is true then the observer should minimize changes to the previous outfit.
        /// Only changes required to keep the outfit valid should be made.  For example, an observer that normally
        /// removes or resets the outfit's animator controller will usually leave the animator alone.  While an 
        /// observer that sets body part and mount point contexts will usually still clear those values since
        /// they are rarely appropriate for any outfit no longer associated with the observer.
        /// </para>
        /// <para>
        /// A completely uncontrolled loss of the previous outfiit occured if <paramref name="wasForced"/> is true 
        /// but <paramref name="previous"/> is null.  E.g. The outfit was improperly destroyed using 
        /// Object.Destory().
        /// </para>
        /// </remarks>
        /// <param name="sender">The soruce of the event. (Required)</param>
        /// <param name="previous">
        /// The original outfit that was replaced and has been released by the body, or null if there is none.
        /// </param>
        /// <param name="wasForced">
        /// If true the outfit was force released and should be generally excempt from release activities that 
        /// alter its state.
        /// </param>
        void OnOutfitChange(Body sender, Outfit previous, bool wasForced);
    }
}

