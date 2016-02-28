/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
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
    /// A component that implements persistant accessory management for a <see cref="Body"/> or body-like object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A body accessory manager has a 'target' that it mounts its accessories to.  While the target is usually an
    /// <see cref="Outfit"/>, it can be anything that accessories can be mounted to.  How/when a manager mounts, unmounts, 
    /// and stores accessories is implementation specific.
    /// </para>
    /// </remarks>
    public interface IBodyAccessoryManager
    {
        /// <summary>
        /// Add the accessory, mounting to the manager's target or storing it as appropriate.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all targets that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>
        /// This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <param name="settings">The mount settings.</param>
        /// <param name="mustMount">
        /// If true a failure to immediately mount will result in a failure to add.  Otherwise a failure to 
        /// immeidately mount will result in the accessory being stored.
        /// </param>
        /// <returns> The result of the add operation.</returns>
        MountResult Add(Accessory accessory, AccessoryAddSettings settings, bool mustMount = false);

        /// <summary>
        /// Add the accessory, mounting to the manager's target or storing it as appropriate.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all targets that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>
        /// This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <param name="locationType">The mount location.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </param>
        /// <param name="mustMount">
        /// If true a failure to immediately mount will result in a failure to add.  Otherwise a failure to 
        /// immeidately mount will result in the accessory being stored.
        /// </param>
        /// <returns>The result of the add operation.</returns>
        MountResult Add(
            Accessory accessory, MountPointType locationType, bool ignoreRestrictions = false, bool mustMount = false);

        /// <summary>
        /// Add the accessory using its default mount location, mounting to the manager's target or storing it as 
        /// appropriate.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all targets that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>
        /// This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </param>
        /// <param name="mustMount">
        /// If true a failure to immediately mount will result in a failure to add.  Otherwise a failure to 
        /// immeidately mount will result in the accessory being stored.
        /// </param>
        /// <returns>The result of the add operation.</returns>
        MountResult Add(Accessory accessory, bool ignoreRestrictions = false, bool mustMount = false);

        /// <summary>
        /// Modify and remount the accessory already being managed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The most common reason for modifying an accessory is to mount it to a new location on the current target.  
        /// Modifying any setting will result in a remount attempt.  <strong>Any</strong> failure to mount will result
        /// in the accessory being stored.
        /// </para>
        /// <para>
        /// This method will return only 'success', 'stored', or 'failed on error'.  If an error is returned then
        /// the settings were rejected and the accessory remains unchanged.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <param name="settings">The new settings.</param>
        /// <param name="retryStored">
        /// If true and the modified accessory transtions to stored, then retry mounts for any stored accessories.
        /// </param>
        /// <returns>The result of the modification.</returns>
        MountResult Modify(Accessory accessory, AccessoryAddSettings settings, bool retryStored = false);

        /// <summary>
        /// Modify and remount the accessory already being managed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The most common reason for modifying an accessory is to mount it to a new location on the current target.  
        /// Modifying any setting will result in a remount attempt.  <strong>Any</strong> failure to mount will result
        /// in the accessory being stored.
        /// </para>
        /// <para>
        /// This method will return only 'success', 'stored', or 'failed on error'.  If an error is returned then
        /// the settings were rejected and the accessory remains unchanged.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <param name="locationType">The mount location.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </param>
        /// <param name="retryStored">
        /// If true and the modified accessory transtions to stored, then retry mounts for any stored accessories.
        /// </param>
        /// <returns>The result of the modification.</returns>
        MountResult Modify(
            Accessory accessory, MountPointType locationType, bool ignoreRestrictions = false, bool retryStored = false);

        /// <summary>
        /// Remove the accessory.
        /// </summary>
        /// <param name="accessory">The accessory.</param>
        /// <param name="retryStored">
        /// If true and the removed accesosry was mounted, then retry mounts for any stored accessories.
        /// </param>
        /// <returns>
        /// True if the accessory was known to the body and removed.  Otherwise the accessory was not recognized.
        /// </returns>
        bool Remove(Accessory accessory, bool retryStored = false);

        /// <summary>
        /// The number of accessories that have been added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The manager will properly respond to external actors taking control of its accessories using
        /// <see cref="Accessory.Mount"/>, <see cref="Accessory.Release"/>, <see cref="Accessory.Destroy"/>, and
        /// other standard methods.  But if an accessory is improperly destroyed using Object.Destroy(), its
        /// entry will continue to be included in his count until cleanup is performed. How the cleanup occurs
        /// is implementation specific.
        /// </para>
        /// </remarks>
        int Count { get; }

        /// <summary>
        /// The mount information for the specified accessory or a default object if the accessory was improperly 
        /// destroyed or is not known to the body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the accessory is mounted, its mount information will have an outfit assigned.  Otherwise the accessory
        /// is in a stored state.
        /// </para>
        /// <para>
        /// If an accessory has been improperly destroyed the object returned will have no accessory assigned.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <returns>
        /// The mount information for the specified accessory or a default object if the accessory was improperly 
        /// destroyed or is not known to the body.
        /// </returns>
        AccessoryMountInfo this[Accessory accessory] { get; }

        /// <summary>
        /// The mount information at the specified index or a default object if the accessory was improperly destroyed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the accessory is mounted, its mount information will have an owner assigned.  Otherwise the accessory
        /// is in a stored state.
        /// </para>
        /// <para>
        /// If an accessory has been improperly destroyed the object returned will have no accessory assigned.
        /// </para>
        /// </remarks>
        /// <param name="index">The index. [0 &lt;= value &lt;<see cref="Count"/>]</param>
        /// <returns>
        /// The mount information at the specified index or a default object if the accessory was improperly destroyed.
        /// </returns>
        AccessoryMountInfo this[int index] { get; }

        /// <summary>
        /// True if the accessory is known to the manager, otherwise false.
        /// </summary>
        /// <param name="accessory">The accessory.</param>
        /// <returns>
        /// True if the accessory is known to the manager, otherwise false.
        /// </returns>
        bool Contains(Accessory accessory);
    }
}
