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
    /// Most accessories require this interface to be implemented by a Unity Object for
    /// serialization purposes.  So accessories are allowed to  reject observers that are not 
    /// Unity Objects.
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
        /// This method is called before the accessory performs any of its internal bake 
        /// operations, but whether or not an accessory feature is still useable depends on
        /// the call order of the observers.
        /// </para>
        /// <param name="sender">The accessory that is being baked.</param>
        void OnBake(Accessory sender);

        /// <summary>
        /// Perform post bake operations.  (The accessory component is no longer available.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called after the acessory is no longer valid.  Its purpose is to 
        /// allow components that depend on the accessory to clean themselves up.  
        /// (E.g. Self destroy.)
        /// </para>
        /// </remarks>
        /// <param name="outfit">The baked outfit's GameObject.</param>
        void OnBakePost(GameObject sender);
    }
}

