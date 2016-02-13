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
    /// A body used to persist settings as outfits are changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The body is useful when an agent's outfits are transient in nature.  It allows important settings and
    /// components to be persisted as outfit out swapped in and out. For example, adding an accessory, such as
    /// a perception system, to the body will result in the accessory automatically transfering between outfits
    /// as they change.  Observers can be used to persist further settings not automatically persisted by the body.
    /// E.g. Persist material changes, collider status, etc. 
    /// </para>
    /// </remarks>
    public abstract class Body
        : MonoBehaviour
    {
        #region Core

        /// <summary>
        /// The transform that currently controls body motion and represents its location in the 
        /// world.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The body is guarenteed to have a motion root even when it doesn't have an outfit.
        /// If there is an outfit, this will be the outfit's motion root.  Otherwise it will
        /// be the body's default motion root.
        /// </para>
        /// <para>
        /// Important: This is a dynamic value.  It changes whenever the outfit is changed.
        /// </para>
        /// </remarks>
        public abstract Transform MotionRoot { get; }

        /// <summary>
        /// The current outfit's primary collider, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: This is a dynamic value.  It changes whenever the outfit is changed.
        /// </para>
        /// </remarks>
        public abstract Collider PrimaryCollider { get; }

        /// <summary>
        /// The current outfit's primary rigidbody, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Important: This is a dynamic value.  It changes whenever the outfit is changed.
        /// </para>
        /// </remarks>
        public abstract Rigidbody PrimaryRigidBody { get; }

        #endregion

        #region Outfit

        /// <summary>
        /// The current outfit, or null if there is none.
        /// </summary>
        public abstract Outfit Outfit { get; }

        /// <summary>
        /// Sets the current outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If there is an error that prevents <paramref name="outfit"/> from being applied then its reference 
        /// is returned.  So <c>SetOutfit(outfit) == outfit</c> indicates a failure.
        /// </para>
        /// </remarks>
        /// <param name="outfit">
        /// The outfit to apply to the body, or null to remove the current outfit.
        /// </param>
        /// <returns>
        /// The previous outfit, or a refererence to <paramref name="outfit"/> if there was an error.
        /// </returns>
        public abstract Outfit SetOutfit(Outfit outfit);

        #endregion

        #region Body Accessories  (Local, not outfit.)

        /// <summary>
        /// The number of accessories that have been added to the body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The body tracks when accessories are remove from its control by external actors using accessory methods
        /// such as <see cref="Accessory.Mount"/>, <see cref="Accessory.Release"/>, <see cref="Accessory.Destroy"/>, 
        /// etc.  But if an accessory is improperly destroyed using Object.Destroy() it will still show up in this 
        /// value until the body recognizes the problem and performs cleanup. 
        /// </para>
        /// </remarks>
        public abstract int AccessoryCount { get; }

        /// <summary>
        /// Get the mount information at the specified index or a default object if the accessory was 
        /// improperly destroyed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the accessory is mounted, its mount information will have an owner assigned.  Otherwise the accessory
        /// is in a stored state.
        /// </para>
        /// <para>
        /// If an accessory has been improperly destoryed the object returned will have no accessory assigned.
        /// </para>
        /// </remarks>
        /// <param name="index">The index. [0 &lt;= value &lt <see cref="AccessoryCount"/>]</param>
        /// <returns>
        /// The mount information at the specified index or a default object if the accessory was 
        /// improperly destroyed.
        /// </returns>
        public abstract AccessoryMountInfo GetAccessoryInfo(int index);

        /// <summary>
        /// Get the mount information for the specified accessory or a default object if the accessory was improperly 
        /// destroyed or is not known to the body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the accessory is mounted, its mount information will have an owner assigned.  Otherwise the accessory
        /// is in a stored state.
        /// </para>
        /// <para>
        /// If an accessory has been improperly destoryed the object returned will have no accessory assigned.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory.</param>
        /// <returns>
        /// The mount information for the specified accessory or a default object if the accessory was improperly 
        /// destroyed or is not known to the body.
        /// </returns>
        public abstract AccessoryMountInfo GetAccessoryInfo(Accessory accessory);

        /// <summary>
        /// True if the body is persisting the accessory between its outfits, false if the accessory is not known
        /// to the body.
        /// </summary>
        /// <param name="accessory">The accessory to check.</param>
        /// <returns>
        /// True if the body is persisting the accessory between its outfits, false if the accessory is not known
        /// to the body.
        /// </returns>
        public abstract bool ContainsAccessory(Accessory accessory);

        /// <summary>
        /// Add an accessory that will persist between outfits or be stored if there is no outfit the accessory
        /// can mount to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all outfits that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory to add.</param>
        /// <param name="settings">The accessory settings.</param>
        /// <param name="mustMount">
        /// True if a failure to immediately mount to an outfit is considered a failure.  Otherwise a non-error
        /// failure to mount will result in the accessory being stored for a later attempt.
        /// </param>
        /// <returns>The result of the add operation.</returns>
        public abstract MountResult AddAccessory(Accessory accessory, AccessoryAddSettings settings, bool mustMount);

        ////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        /// <summary>
        /// Add an accessory that will persist between outfits or be stored if there is no outfit the accessory
        /// can mount to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all outfits that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory to add.</param>
        /// <param name="settings">The accessory settings.</param>
        /// <returns>The result of the add operation.</returns>
        public MountResult AddAccessory(Accessory accessory, AccessoryAddSettings settings)
        {
            return AddAccessory(accessory, settings, false);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an accessory that will persist between outfits or be stored if there is no outfit the accessory
        /// can mount to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all outfits that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory to add.</param>
        /// <param name="locationType">The mount target of the accessory.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore outfit 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </param>
        /// <param name="mustMount">
        /// True if a failure to immediately mount to an outfit is considered a failure.  Otherwise a non-error
        /// failure to mount will result in the accessory being stored for a later attempt.
        /// </param>
        /// <returns>The result of the add operation.</returns>
        public MountResult AddAccessory(Accessory accessory, MountPointType locationType, 
            bool ignoreRestrictions = false, bool mustMount = false)
        {
            var settings = new AccessoryAddSettings();
            settings.IgnoreRestrictions = ignoreRestrictions;
            settings.LocationType = locationType;
            settings.Mounter = null;
            settings.AdditionalCoverage = 0;

            return AddAccessory(accessory, settings, mustMount);
        }

        /// <summary>
        /// Add an accessory that will persist between outfits or be stored if there is no outfit the accessory
        /// can mount to.  (Target the accessory's default mount location.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// A persistant accessory will be mounted to all outfits that accept the accessory.  Otherwise it will
        /// be stored for later mounting.
        /// </para>
        /// <para>This method will return only three results: 'success', 'stored', and 'failed on error'.  This is
        /// because a non-error failure to mount results in storage.</para>
        /// </remarks>
        /// <param name="accessory">The accessory to add.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore outfit 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </param>
        /// <param name="mustMount">
        /// True if a failure to immediately mount to an outfit is considered a failure.  Otherwise a non-error
        /// failure to mount will result in the accessory being stored for a later attempt.
        /// </param>
        /// <returns>The result of the add operation.</returns>
        public MountResult AddAccessory(Accessory accessory, bool ignoreRestrictions = false, bool mustMount = false)
        {
            return AddAccessory(accessory, accessory.DefaultLocationType, ignoreRestrictions, mustMount);
        }

        /// <summary>
        /// Modify and remount the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The most common reason for modifying an accessory is to mount it to a new location.  Modifying any
        /// setting will result in a remount attempt.  <strong>Any</strong> failure to mount will result in the 
        /// accessory being stored.
        /// </para>
        /// <para>
        /// This method will return only 'success', 'stored', or 'failed on error'.  If an error is returned then
        /// the settings were rejected and the accessory remains unchanged.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to modify.</param>
        /// <param name="settings">The new settings.</param>
        /// <returns>The result of the modification.</returns>
        public abstract MountResult ModifyAccessory(Accessory accessory, AccessoryAddSettings settings);

        /// <summary>
        /// Modify and remount the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The most common reason for modifying an accessory is to mount it to a new location.  Modifying any
        /// setting will result in a remount attempt.  <strong>Any</strong> failure to mount will result in the 
        /// accessory being stored.
        /// </para>
        /// <para>
        /// This method will return only 'success', 'stored', or 'failed on error'.  If an error is returned then
        /// the settings were rejected and the accessory remains unchanged.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to modify.</param>
        /// <param name="locationType">The mount target of the accessory.</param>
        /// <param name="ignoreRestrictions">
        /// If true, ignore outfit 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </param>
        /// <returns>The result of the modification.</returns>
        public MountResult ModifyAccessory(
            Accessory accessory, MountPointType locationType, bool ignoreRestrictions = false)
        {
            var settings = new AccessoryAddSettings();
            settings.IgnoreRestrictions = ignoreRestrictions;
            settings.LocationType = locationType;

            return ModifyAccessory(accessory, settings);
        }

        // Reminder:  Don't implement this.  It can result in the following: ModifyAccessory(accessory)
        // That is way too confusing for users.
        //public MountResult ModifyAccessory(Accessory accessory, bool ignoreRestrictions = false)
        //{
        //}

        /// <summary>
        /// Remove the accessory.
        /// </summary>
        /// <param name="accessory">The accessory to remove.</param>
        /// <returns>
        /// True if the accessory was known to the body and removed.  Otherwise the accessory was not recognized.
        /// </returns>
        public abstract bool RemoveAccessory(Accessory accessory);

        #endregion

        #region Observers


        /// <summary>
        /// Add the specified event observer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All standard implementations require the observer to be a Unity Object for serialization, so it is 
        /// acceptable for the body to reject an observer.  An error message will be logged if an observer is rejected.
        /// </para>
        /// <para>
        /// An observer can only be added once.
        /// </para>
        /// </remarks>
        /// <param name="observer">The observer to add. (Required)</param>
        /// <returns>
        /// True if the observer was accepted or already added.  False if the observer was rejected.
        /// </returns>
        public abstract bool AddObserver(IBodyObserver observer);

        /// <summary>
        /// Remove the specified event observer.
        /// </summary>
        /// <param name="observer">The observer to remove.</param>
        public abstract void RemoveObserver(IBodyObserver observer);

        #endregion
    }
}
