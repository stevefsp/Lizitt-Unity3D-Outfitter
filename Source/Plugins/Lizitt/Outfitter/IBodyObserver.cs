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
    /// Most bodies require this interface to be implemented by a Unity Object for serialization purposes, so outfits
    /// are allowed to reject observers that are not  Unity Objects.
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
        /// </remarks>
        /// <param name="sender">The body sending the event. (Required)</param>
        /// <param name="previous">
        /// The outfit that was replaced and is being released by the body, or null if there is none.
        /// </param>
        void OnOutfitChange(Body sender, Outfit previous);

        /// <summary>
        /// Sent when the body has lost control of an outfit through unexpected means.  (E.g. If the outfit was
        /// destoryed what under the control of the body.)
        /// </summary>
        /// <param name="sender">The body. (Required.)</param>
        void OnSoftReset(Body sender);
    }
}

