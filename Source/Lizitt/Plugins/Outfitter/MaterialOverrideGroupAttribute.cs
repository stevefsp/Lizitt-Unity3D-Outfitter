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
using System.Collections.Generic;
using com.lizitt.u3d;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// [Internal use only - not properly generalized]
    /// Display the <see cref="MaterialOverrideGroup"/> in a user friendly way by scanning
    /// the provided prototype for potential renderers.
    /// </summary>
    public class MaterialOverrideGroupAttribute
        : PropertyAttribute
    {
        /// <summary>
        /// The name of the field in the property's serialized object that references a 
        /// GameObject containing the renderers that can be selected.
        /// </summary>
        public readonly string prototypeField;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prototypeField">
        /// The name of the field in the property's serialized object that references a 
        /// GameObject containing the renderers that can be selected.
        /// </param>
        public MaterialOverrideGroupAttribute(string prototypeField)
        {
            this.prototypeField = prototypeField;
        }
    }
}