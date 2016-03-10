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
using UnityEditor;
using UnityEngine;
using com.lizitt.editor;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// <see cref="Outfit"/> editor helper methods.
    /// </summary>
    public partial class OutfitEditor
        : Editor
    {
        #region Accessory Members

        public static MountResult MountAccessory(
            Outfit outfit, Accessory accessory, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(outfit))
            {
                Debug.LogError("Can't modify an outfit asset.  Outfit must be in the scene.", outfit);
                return MountResult.FailedOnError;
            }

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Add Accessory" : undoLabel;

            Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

            bool isNew = AssetDatabase.Contains(accessory);
            if (isNew)
            {
                var name = accessory.name;
                accessory = accessory.Instantiate();

                accessory.name = name;

                // Record undo later.
            }
            else
                Undo.RecordObjects(Accessory.UnsafeGetUndoObjects(accessory).ToArray(), undoLabel);

            var origParent = accessory.transform.parent;

            var result = outfit.Mount(accessory);

            if (result.IsFailed())
            {
                Debug.LogWarningFormat(outfit, "Accessory mount failed: {0}: {1}", accessory.name, result);

                if (isNew)
                    accessory.gameObject.SafeDestroy();

                accessory = null;
            }
            else
            {
                if (isNew)
                    Undo.RegisterCreatedObjectUndo(accessory.gameObject, undoLabel);
                var parent = accessory.transform.parent;
                accessory.transform.parent = origParent;
                Undo.SetTransformParent(accessory.transform, parent, undoLabel);
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return result;
        }

        public static bool ReleaseAccessory(Outfit outfit, Accessory accessory, float offsetDistance = 0,
            RemoveActionType removeType = RemoveActionType.Undefined, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(outfit))
            {
                Debug.LogError("Can't modify an outfit asset.  Outfit must be in the scene.", outfit);
                return false;
            }

            if (removeType == RemoveActionType.Undefined)
            {
                removeType = (RemoveActionType)EditorUtility.DisplayDialogComplex("Remove accessory from body?",
                    string.Format("Remove '{0}' from '{1}'?", accessory.name, outfit.name),
                    "Remove only", "Remove and destroy", "Do not remove");
            }

            if (removeType == RemoveActionType.Cancel)
                return false;

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Remove Accessory" : undoLabel;
            if (singleUndo)
                Undo.IncrementCurrentGroup();

            Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

            var parent = accessory.transform.parent;

            outfit.Release(accessory);

            accessory.transform.parent = parent;
            Undo.SetTransformParent(accessory.transform, null, undoLabel);

            if (removeType == RemoveActionType.RemoveAndDestroy)
            {
                accessory.Destroy(DestroyType.GameObject, true);
                Undo.DestroyObjectImmediate(accessory.gameObject);
            }
            else if (offsetDistance > 0)
            {
                // This extra record call is needed for some reason.  Otherwise only **part** of the translation 
                // is recorded.  Maybe the problem has to do with the unpartenting?  Anyway, more fun with undo...
                Undo.RecordObject(accessory.transform, undoLabel);
                accessory.transform.position += outfit.transform.right * -offsetDistance;
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return true;
        }

        #endregion

        #region Body Part Members

        public static void ApplyBodyPartColliderStatus(Outfit outfit, RigidbodyBehavior status, bool singleUndo = true, 
            string undoLabel = "Set BP Collider Status")
        {
            if (AssetDatabase.Contains(outfit))
            {
                Debug.LogError("Can't modify an outfit asset.  Outfit must be in the scene.", outfit);
                return;
            }

            if (outfit.BodyPartCount == 0)
                return;

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            for (int i = 0; i < outfit.BodyPartCount; i++)
            {
                var bp = outfit.GetBodyPart(i);
                if (bp)
                {
                    var rb = bp.Rigidbody;
                    if (rb)
                        LizittEditorGUIUtil.SetRigidbodyBehavior(rb, status, false, undoLabel);
                }
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        public static bool ApplyBodyPartLayer(
            Outfit outfit, int layer, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(outfit))
            {
                Debug.LogError("Can't modify an outfit asset.  Outfit must be in the scene.", outfit);
                return false;
            }

            if (outfit.BodyPartCount == 0)
                return false;

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Set Body Part Layer" : undoLabel;

            Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

            outfit.ApplyBodyPartLayer(layer);

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return true;
        }

        #endregion

        #region Primary Collider Members

        public static bool SetPrimaryColliderLayer(
            Outfit outfit, int layer, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(outfit))
            {
                Debug.LogError("Can't modify an outfit asset.  Outfit must be in the scene.", outfit);
                return false;
            }

            if (!outfit.PrimaryCollider)
                return false;

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Set Primary Collider Layer" : undoLabel;

            Undo.RecordObject(outfit.PrimaryCollider.gameObject, undoLabel);

            outfit.PrimaryCollider.gameObject.layer = layer;

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return true;
        }

        #endregion

        #region Outfit Material Members

        public static int ApplyOutfitMaterial(Outfit outfit, OutfitMaterialType matType, Material material, 
            bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(outfit))
            {
                Debug.LogError("Can't modify an outfit asset.  Outfit must be in the scene.", outfit);
                return 0;
            }

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Set Outfit Material" : undoLabel;
            Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

            var count = outfit.ApplySharedMaterial(matType, material);

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return count;
        }

        #endregion
    }
}
