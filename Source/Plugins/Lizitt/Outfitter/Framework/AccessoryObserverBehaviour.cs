﻿/*
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
    /// A base object for all acessory observer behaviours.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is no requirement that all Monobehaviour based accessory observers inherit from this class.  It is 
    /// simply a useful base from which to provide common utility and editor search features.
    /// </para>
    /// </remarks>
    public abstract class AccessoryObserverBehaviour
        : MonoBehaviour, IAccessoryObserver
    {
        void IAccessoryObserver.OnStateChange(Accessory sender)
        {
            OnStateChange(sender);
        }

        protected abstract void OnStateChange(Accessory sender);

        void IAccessoryObserver.OnDestroy(Accessory sender, DestroyType typ)
        {
            OnDestroy(sender, typ);
        }

        protected abstract void OnDestroy(Accessory sender, DestroyType typ);
    }
}
