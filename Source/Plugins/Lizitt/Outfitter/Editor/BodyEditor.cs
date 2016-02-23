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

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// <see cref="Body"/> helper methods.
    /// </summary>
    public partial class BodyEditor
        : Editor
    {
        #region Outfit Members

        /// <summary>
        /// Properly removes the body's outfit while in editor mode.  (Includes undo.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="singleUndo"/> will create a single undo action.  If calling this method is part of
        /// a larger operation that needs a single undo, then set <paramref name="singleUndo"/> to false. Behavior 
        /// is undefined if <paramref name="singleUndo"/> is false and the caller does not properly group this method's
        /// actions.  
        /// </para>
        /// </remarks>
        /// <param name="body">The body. (Required. Must be a scene object.)</param>
        /// <param name="removeType">The type of removal action, or 'undefined' to display a dialog box.</param>
        /// <param name="repoOffset">
        /// The amount to offset the position of the removed outfit so it doesn't obstruct the body. [Limit: >= 0]
        /// </param>
        /// <param name="singleUndo">
        /// If true, will group all actions into a single undo.  Otherwise the caller must properly group the undos.
        /// </param>
        /// <param name="undoLabel">The label to use for the undo, or null to use the default label.</param>
        /// <returns>True if the outfit was successfully set.</returns>
        public static bool RemoveOutfit(Body body, float repoOffset = 0, 
            RemoveActionType removeType = RemoveActionType.Undefined, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(body))
            {
                Debug.LogError("Can't modify a body asset.  Body must be in the scene.", body);
                return false;
            }

            if (!body.Outfit)
            {
                Debug.LogWarning("Body has no outfit to remove.", body);
                return true;
            }

            if (removeType == RemoveActionType.Undefined)
            {
                removeType = (RemoveActionType)EditorUtility.DisplayDialogComplex("Remove the outfit from its body?",
                    string.Format("Remove '{0}' from '{1}'?", body.Outfit.name, body.name),
                    "Remove only", "Remove and destroy", "Do not remove");
            }
                    
            if (removeType == RemoveActionType.Cancel)
                return false;

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Remove Outfit" : undoLabel;

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            Undo.RecordObjects(Body.UnsafeGetUndoObjects(body).ToArray(), undoLabel);  // Includes outfit.

            var origParent = body.Outfit.transform.parent;

            var outfit = body.SetOutfit(null);

            outfit.transform.parent = origParent;
            Undo.SetTransformParent(outfit.transform, null, undoLabel);

            if (removeType == RemoveActionType.RemoveAndDestroy)
            {
                outfit.Destroy(DestroyType.GameObject, true);
                Undo.DestroyObjectImmediate(outfit.gameObject);
            }
            else if (repoOffset > 0)
            {
                // This extra record call is needed for some reason, otherwise only **part** of the translation 
                // is recorded.  Maybe the problem has to do with the change in the parent?  Anyway, more fun 
                // with undo...
                Undo.RecordObject(outfit.transform, undoLabel);
                outfit.transform.position += body.transform.right * repoOffset;
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return true;
        }

        #endregion

        #region Accessory Members

        /// <summary>
        /// Properly adds the accessory to the body's while in editor mode.  (Includes undo.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="singleUndo"/> will create a single undo action.  If calling this method is part of
        /// a larger operation that needs a single undo, then set <paramref name="singleUndo"/> to false. Behavior 
        /// is undefined if <paramref name="singleUndo"/> is false and the caller does not properly group this method's
        /// actions.
        /// </para>
        /// </remarks>
        /// <param name="body">The body. (Required. Must be a scene object.)</param>
        /// <param name="outfit">The accessory. (Required)</param>
        /// <param name="singleUndo">
        /// If true, will group all actions into a single undo.  Otherwise the caller must properly group the undos.
        /// </param>
        /// <param name="undoLabel">The label to use for the undo, or null to use the default label.</param>
        /// <returns>True if the outfit was successfully set.</returns>
        public static bool AddAccessory(
            Body body, Accessory accessory, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(body))
            {
                Debug.LogError("Can't modify a body asset.  Body must be in the scene.", body);
                return false;
            }

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Add Accessory" : undoLabel;

            Undo.RecordObjects(Body.UnsafeGetUndoObjects(body).ToArray(), undoLabel);

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

            bool isDirty = false;

            if (body.Accessories.Add(accessory).IsFailed())
            {
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
                isDirty = true;
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return isDirty;
        }

        public static bool RemoveAccessory(Body body, Accessory accessory, float offsetDistance = 0, 
            RemoveActionType removeType = RemoveActionType.Undefined, bool singleUndo = true, string undoLabel = null)
        {
            if (AssetDatabase.Contains(body))
            {
                Debug.LogError("Can't modify a body asset.  Body must be in the scene.", body);
                return false;
            }

            if (removeType == RemoveActionType.Undefined)
            {
                removeType = (RemoveActionType)EditorUtility.DisplayDialogComplex("Remove accessory from body?",
                    string.Format("Remove '{0}' from '{1}'?", accessory.name, body.name),
                    "Remove only", "Remove and destroy", "Do not remove");
            }

            if (removeType == RemoveActionType.Cancel)
                return false;

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Remove Accessory" : undoLabel;
            if (singleUndo)
                Undo.IncrementCurrentGroup();

            Undo.RecordObjects(Body.UnsafeGetUndoObjects(body).ToArray(), undoLabel);

            var parent = accessory.transform.parent;

            body.Accessories.Remove(accessory);

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
                accessory.transform.position += body.transform.right * -offsetDistance;
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return true;
        }

        #endregion
    }
}
