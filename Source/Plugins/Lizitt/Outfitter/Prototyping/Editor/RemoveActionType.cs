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
namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// The type of remove operation to perform.
    /// </summary>
    public enum RemoveActionType
    {
        // Values match the EditorUtility.DisplayDialogComplex() return values, except for 'undefined'.

        /// <summary>
        /// A dialog should be used to make the choice.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Remove the item and preserve it.  (E.g. Leave it in the scene.)
        /// </summary>
        RemoveOnly = 0,

        /// <summary>
        /// Remove the item and destroy it.
        /// </summary>
        RemoveAndDestroy = 1,

        /// <summary>
        /// Cancel the removal operation.
        /// </summary>
        Cancel = 2,
    }
}
