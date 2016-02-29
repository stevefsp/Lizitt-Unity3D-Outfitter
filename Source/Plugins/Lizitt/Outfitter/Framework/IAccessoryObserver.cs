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
    /// A component that responds to <see cref="Accessory"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most accessories require this interface to be implemented by a UnityEngine.Object for
    /// serialization purposes, so accessories are allowed to reject observers that are not 
    /// UnityEngine.Objects.
    /// </para>
    /// </remarks>
    public interface IAccessoryObserver
    {
        /// <summary>
        /// Process a change in accessory status.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is sent if any of the following accessory properties changes:
        /// <see cref="Accessory.Status"/>, <see cref="Accessory.Owner"/>, 
        /// <see cref="Accessory.CurrentLocation"/>.
        /// </para>
        /// </remarks>
        /// <param name="sender">The source of the accessory event.</param>
        /// <param name="status">The accessory status.</param>
        void OnStateChange(Accessory sender);

        /// <summary>
        /// Prepare for the destruction of the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is sent before the accessory performs any internal operations, so the
        /// accessory starts out as valid.  Whether it stays they way depends on the actions
        /// of the observers.
        /// </para>
        /// <para>
        /// Destruction of the accessory is inevitable.  There is nothing an observer can do to
        /// stop it.
        /// </para>
        /// </remarks>
        /// <param name="sender">The accessory that is to be destroyed.</param>
        /// <param name="typ">The destruction type.</param>
        void OnDestroy(Accessory sender, DestroyType typ);
    }
}

