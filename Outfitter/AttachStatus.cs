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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// Indicates the results of an attach operation.
    /// </summary>
    public enum AttachStatus
    {
        /// <summary>
        /// Attached successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Not immediately attached.  Stored for a later attempt.
        /// </summary>
        /// <remarks>
        /// <para>This is returned my components that perform item management.</para>
        /// </remarks>
        Pending,

        /// <summary>
        /// Failed to attach and cannot attach due to a procedural or other error.
        /// </summary>
        FailedOnError,

        /// <summary>
        /// Could not attach, no available mount point.
        /// </summary>
        NoMountPoint,

        /// <summary>
        /// Could not attach, blocked by existing item or general block.
        /// </summary>
        Blocked,

        /// <summary>
        /// Could not attach, target is flagged as limited.
        /// </summary>
        Limited,
    }
}
