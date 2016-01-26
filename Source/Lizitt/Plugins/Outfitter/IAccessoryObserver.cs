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
    /// Most accessories require this interface to be implemented by a Component so its association
    /// to the accessory can be properly serialized.
    /// </para>
    /// </remarks>
    public interface IAccessoryObserver
    {
        /// <summary>
        /// Process a change in accessory status.
        /// </summary>
        /// <param name="sender">The source of the accessory event.</param>
        /// <param name="status">The accessory status.</param>
        void OnStatusChange(Accessory sender, AccessoryStatus status);

        /// <summary>
        /// Process an accessory bake event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called before the accessory it becomes invalid.
        /// </para>
        /// </remarks>
        /// <param name="sender">The accessory that is being baked.</param>
        void OnBake(Accessory sender);

        /// <summary>
        /// Process an accessory post-bake event.  (The accessory is no longer avaiable.)
        /// </summary>
        /// <param name="sender">The original accessory's GameObject.</param>
        void OnBakePost(GameObject sender);
    }
}

