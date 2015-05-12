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
using com.lizitt.outfitter;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The body outfit representing the physical presence of an agent.
/// </summary>
/// <remarks>
/// <para>
/// The body outfit generally consists of the models, renderers, animators, colliders, 
/// and other components the represents an agents physical presence in the scene.
/// </para>
/// <para>
/// Mount points and accessories:
/// </para>
/// <para>
/// Mount points need to be blocked at times, but it is not appropraite to allow permanent b
/// locking since sometimes it may be appropriate to override a block.  E.g. A magic fairy 
/// should be able to fly around the character's head, even if the head has a full coverage
/// helmet that blocks other head mounted accessories.
/// </para>
/// <para>
/// Also can't just say, if you don't want it mounted, don't define the mount.  Mount points 
/// are often used for collideres and other non-accessory related purposes.  E.g. To mount 
/// perception components to the head.  So here are the rules:
/// </para>
/// <para>
/// If it is normally inappropriate to attach any accessories, configure
/// <see cref="AccessoriesLimited"/> as true to indicate that only 'special' accessories
/// should be attached.  Only accessories with <see cref="BodyAccessory.IgnoreLimited"/> can
/// be attached.  All others will be blocked.
/// </para>
/// <para>
/// The concept of body coverage allows selective blocking of new attachments.  This can be
/// at the outfit level.  E.g. Don't ever attach accesories that have head coverage.
/// Or it can be dynamic based on the currently attached accessories.  E.g. If one accessory has
/// <see cref="BodyCoverage.HeadTop"/> coverage, no other accessories with the same coverage
/// can be attached.  Coverage blocking can be overridden by defining an accessory with no 
/// coverage.  Consider a full head helmet.  It should block all other other hair, eye, and face 
/// accessories.  Except, once again, the flying fairy.
/// </para>
/// <para>
/// Of course, if a mount point is not defined, then it will be permanently blocked.  The only
/// guarenteed mount point is <see cref="MountPointType.Root"/>.
/// </para>
/// <para>
/// Examples:
/// </para>
/// <para>
/// Nude body    : Limited
/// Hat          : Limited. Covers headtop.
/// Glasses      : Limited. Covers eyes.
/// Space Helmet : Limited, Covers headtop, eyes, and face.  (Blocks hat and glasses.)
/// Fairy        : Ignore Limited. No coverage. Never blocked as long as its mount point exists.
/// </para>
/// </remarks>
public abstract class BodyOutfit
    : MonoBehaviour
{
    /*
     * Design notes:
     * 
     * A lot of the abstract properties could be fully implemented here.  But most concrete
     * classes will want to serialize all or most of the property backing fields.
     * Don't want to interfere with that type of design.
     */

    #region Animation

    /// <summary>
    /// The renderer thta contains the face blend shapes.  (May be null.)
    /// </summary>
    public abstract SkinnedMeshRenderer BlendHead { get; }

    /// <summary>
    /// The outfit animator.  (May be null.)
    /// </summary>
    public abstract Animator Animator { get; }

    #endregion

    #region Colliders & Physics

    /// <summary>
    /// The main collider for the outfit.
    /// </summary>
    public abstract Collider SurfaceCollider { get; }

    /// <summary>
    /// The main rigid body for the outfit.
    /// </summary>
    public abstract Rigidbody SurfaceRigidBody { get; }

    /// <summary>
    /// An array of all body colliders.
    /// </summary>
    /// <returns>An array of all body colliders.</returns>
    public abstract BodyCollider[] GetColliders();

    /// <summary>
    /// Gets the body collider for the specified mount point.
    /// </summary>
    /// <param name="mountPoint">The mount point.</param>
    /// <returns>The body collider for the specified mount point.</returns>
    public abstract BodyCollider GetCollider(MountPointType mountPoint);

    /// <summary>
    /// Sets body collider status.
    /// </summary>
    /// <param name="status">The desired status.</param>
    public abstract void SetColliderStatus(BodyColliderStatus status);

    /// <summary>
    /// The status of the body colliders.
    /// </summary>
    public abstract BodyColliderStatus ColliderStatus { get; }

    /// <summary>
    /// True if the outfit has body colliders.
    /// </summary>
    public abstract bool HasColliders { get; }

    /// <summary>
    /// The layer of the body colliders.
    /// </summary>
    public abstract int ColliderLayer { get; }

    /// <summary>
    /// Sets the layer of the body colliders.
    /// </summary>
    /// <param name="layer"></param>
    public abstract void SetColliderLayer(int layer);

    #endregion

    #region Initialization

    private bool m_IsInitialized = false;

    /// <summary>
    /// True if the outfit has been intialized.
    /// </summary>
    protected bool IsInitialized
    {
        get { return m_IsInitialized; }
    }

    // Unity reflection behavior requires that this be protected.
    protected void Start()
    {
        if (!m_IsInitialized)
            Initialize();
    }

    /// <summary>
    /// Initializes the outfit if it hasn't already been initialized.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The outfit is automatically initialized during the start method.  This method can be used
    /// to force the initialization early.
    /// </para>
    /// </remarks>
    public void Initialize()
    {
        if (m_IsInitialized)
        {
            Debug.LogWarning("Outfit is already initialized.", this);
            return;
        }

        m_IsInitialized = true;  // Keep first.  Never want a re-try.

        var iter = OnInitializePre();

        if (iter == null)
            // Local is saying: I don't have any, or ignore my accessories.
            return;

        foreach (var item in iter)
        {
            if (item)
            {
                if (m_AccessoryGroup == null)
                    m_AccessoryGroup = new AccessoryCoverageGroup();

                m_AccessoryGroup.Add(item);
            }
        }

        OnInitializePost();
    }

    /// <summary>
    /// Called early in the <see cref="Initialize"/> operation.
    /// </summary>
    /// <returns>
    /// A list of all accessories to attach to outfit an initialization. (The default accessories.)
    /// </returns>
    protected abstract IEnumerable<BodyAccessory> OnInitializePre();

    /// <summary>
    /// Called during <see cref="Initialize"/> after all default accessories have been attached.
    /// </summary>
    protected virtual void OnInitializePost()
    {
    }

    #endregion

    #region Mount Points & Accessories

    private AccessoryCoverageGroup m_AccessoryGroup = null;

    /// <summary>
    /// If true, only accessories marked to ignore this flag can be successfully attached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The purpose of this flag is to allow an outfit to be marked as not appropriate for
    /// most accessories.  Consider a nude or semi-nude outfit.  It is normally not appropriate
    /// for a character that has glasses to wear the glasses with those types of outfits.  So
    /// they are marked as limited.
    /// </para>
    /// </remarks>
    public abstract bool AccessoriesLimited { get; }

    /// <summary>
    /// The built-in coverage blocks for the outfit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Built-in coverage blocks represent blocks owned by the outfit rather than 
    /// accessories.
    /// </para>
    /// <para>
    /// Built-in coverage blocks are usually long term or permanent.  But some may the result of
    /// built-in accessories that detach themselves after a period of time.  So coverage blocks 
    /// may change.
    /// </para>
    /// </remarks>
    public abstract BodyCoverage CoverageBlocks { get; }

    /// <summary>
    /// The current total coverage from both blocks and transient accessories.
    /// </summary>
    public BodyCoverage Coverage
    {
        get
        {
            if (m_AccessoryGroup == null)
                return CoverageBlocks;

            return m_AccessoryGroup.Coverage | CoverageBlocks;
        }
    }

    /// <summary>
    /// The current coverage from accessories added using <see cref="AttachAccessory"/>.
    /// </summary>
    public BodyCoverage AccessoryCoverage
    {
        get { return m_AccessoryGroup == null ? 0 : m_AccessoryGroup.Coverage; }
    }

    /// <summary>
    /// Gets the specified mount point, or null if the mount point does not exist.
    /// </summary>
    /// <param name="mountPoint">The mount point to retrieve.</param>
    /// <returns>The specified mount point, or null if the mount point does not exist.</returns>
    public abstract MountPoint GetMountPoint(MountPointType mountPoint);

    /// <summary>
    /// Attach the accessory to the outfit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Success will be returned if the accessory is already attached.  But 
    /// <paramref name="attach"/> will not be called.
    /// </para>
    /// </remarks>
    /// <param name="accessory">The accessory to attach.</param>
    /// <param name="attach">The method to call once the attack is 'approved' by the outfit.</param>
    /// <returns>The status of the attach.</returns>
    public virtual AttachStatus AttachAccessory(BodyAccessory accessory, AttachMethod attach)
    {
        if (m_AccessoryGroup != null && m_AccessoryGroup.Contains(accessory))
        {
            Debug.LogWarning("Attempted to attach same accessory more than once: " + accessory.name);
            // Technically is was a success since it is attached.
            return AttachStatus.Success;
        }

        if (AccessoriesLimited && !accessory.IgnoreLimited)
            return AttachStatus.Limited;

        if ((accessory.Coverage & Coverage) != 0)
            return AttachStatus.Blocked;

        var mountPoint = GetMountPoint(accessory.MountPoint);

        if (mountPoint == null)
            return AttachStatus.NoMountPoint;

        if (!attach(mountPoint.Transform))
            return AttachStatus.FailedOnError;

        if (m_AccessoryGroup == null)
            m_AccessoryGroup = new AccessoryCoverageGroup();

        m_AccessoryGroup.Add(accessory);

        return AttachStatus.Success;
    }

    #endregion

    #region Miscellaneous Features

    /// <summary>
    /// Applies the material overrides to the outfit.
    /// </summary>
    /// <param name="overrides"></param>
    public abstract void Apply(BodyMaterialOverrides overrides);

    /// <summary>
    /// The transform that is used to move the outfit. (The motion root.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The motion root is auto-detected at initialization with the following precidence:
    /// </para>
    /// <para>
    /// The animator transform.<br/>
    /// The predefined root mount point.<br/>
    /// The instantiated outfit transform.<br/>
    /// The outfit component's transform.
    /// </para>
    /// <para>
    /// The motion root will never be null.  If an outfit exists, it must have a motion root.
    /// </para>
    /// </remarks>
    public abstract Transform MotionRoot { get; }

    /// <summary>
    /// Deletes all outfit related sub-components.  (Essentially bakes the outfit.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// After calling this method, the outfit will be essentially useless.  Attempting to use the
    /// outfit after the purge will result in undefined behavior.  The caller must delete the 
    /// root outfit component to complete the process.
    /// </para>
    /// </remarks>
    public abstract void PurgeOutfitComponents();

    #endregion
}

