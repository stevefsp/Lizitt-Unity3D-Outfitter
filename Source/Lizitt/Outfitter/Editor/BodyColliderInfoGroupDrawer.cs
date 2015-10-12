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
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using com.lizitt.u3d.editor;

namespace com.lizitt.outfitter.editor
{
    [CustomPropertyDrawer(typeof(BodyColliderInfoGroup))]
    public class BodyColliderInfoGroupDrawer
        : BaseMountPointDataListDrawer
    {
        protected override string ItemPropName 
        {
            get { return "m_Items"; }
        }

        protected override string ItemDataPropName
        {
            get { return "m_Collider"; }
        }

        protected override string ItemTypePropName
        {
            get { return "m_Type"; }
        }

        protected override string ListHeaderLabel
        {
            get { return "Body Part Colliders"; }
        }
    }
}

