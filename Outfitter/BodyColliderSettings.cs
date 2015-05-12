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
using com.lizitt.u3d;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Body collider settings for an outfit. (Both the main (surface) collider 
/// and body part colliders.)
/// </summary>
public class BodyColliderSettings
    : ScriptableObject
{
    #region Unity Editor Fields

    [Header("Surface Collider")]

    [SerializeField]
    [Tooltip("The layer for the surface collider.")]
    [UnityLayer]
    private int m_SurfaceLayer = UnityLayer.Default;

    [SerializeField]
    [Tooltip("The settings for the surface collider.")]
    private CapsuleColliderParams m_SurfaceCollider = new CapsuleColliderParams();

    [Header("Mount Point Colliders")]

    [SerializeField]
    [Tooltip("Post a warning message if the outfit doesn't support a defined collider.")]
    private bool m_WarnOnNoMount = true;

    [SerializeField]
    [Tooltip("The layer for the body colliders.")]
    [UnityLayer]
    private int m_PartsLayer = UnityLayer.Default;

    [Space(5)]

    [SerializeField]
    private CapsuleBodyColliderParams[] m_Capsules = new CapsuleBodyColliderParams[0];

    [SerializeField]
    private BoxBodyColliderParams[] m_Boxes = new BoxBodyColliderParams[0];

    [SerializeField]
    private SphereBodyColliderParams[] m_Spheres = new SphereBodyColliderParams[0];

    #endregion

    #region Surface Collider Features

    /// <summary>
    /// Creates the collider based on the settings.
    /// </summary>
    /// <param name="motionTransform">The body's motion root.</param>
    /// <returns>The collider that was created.</returns>
    public Collider CreateSurfaceCollider(Transform motionTransform)
    {
        var col = m_SurfaceCollider.Create(motionTransform);

        var rb = col.gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;

        col.gameObject.layer = m_SurfaceLayer;

        return col;
    }

    #endregion

    #region Body Collider Features

    /// <summary>
    /// Applies the body colliders to the mount points.
    /// </summary>
    /// <param name="mountPoints">The available mounts points.</param>
    /// <returns>The colliders that were created.</returns>
    public BodyCollider[] ApplyTo(MountPoint[] mountPoints)
    {
        var result = new List<BodyCollider>();

        foreach (var item in m_Capsules)
        {
            var bcol = CreateCollider(mountPoints, item.MountPoint, item);

            if (bcol != null)
                result.Add(bcol);
        }

        foreach (var item in m_Boxes)
        {
            var bcol = CreateCollider(mountPoints, item.MountPoint, item);

            if (bcol != null)
                result.Add(bcol);
        } 
        
        foreach (var item in m_Spheres)
        {
            var bcol = CreateCollider(mountPoints, item.MountPoint, item);

            if (bcol != null)
                result.Add(bcol);
        }

        return result.ToArray();
    }

    private BodyCollider CreateCollider(
        MountPoint[] mountPoints, MountPointType mountPoint, ColliderParams config)
    {
        Transform trans = null;

        foreach (var item in mountPoints)
        {
            if (item.MountType == mountPoint)
            {
                trans = item.Transform;
                break;
            }
        }

        if (!trans)
        {
            if (m_WarnOnNoMount)
            {
                Debug.LogWarning(
                    "Outfit does not have the required mountpoint for a collider: " + mountPoint);
            }
            return null;
        }

        var col = config.Create(trans);

        col.name = mountPoint + "_Collider";
        col.gameObject.layer = m_PartsLayer;

        var rb = col.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        return new BodyCollider(col, rb, mountPoint);
    }

    #endregion
}
