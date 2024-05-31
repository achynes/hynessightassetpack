using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace HynesSightEditor
{
	/// <summary>
	/// Class containing methods for sorting Objects in Unity.
	/// </summary>
	class SortUtility
	{
		/// <summary>
		/// Sorts the selected GameObjects by sibling index into a new array.
		/// </summary>
		/// <returns>A sorted array of the selected GameObjects.</returns>
		static public GameObject[] SortGameObjectsSelectionBySiblingIndex(GameObject[] gameObjectsToSort_)
		{
            if (gameObjectsToSort_.Length == 0)
            {
                Debug.LogWarning("No GameObjects selected; null returned.");
                return null;
            }

			Transform[] sortedTransforms = new Transform[gameObjectsToSort_.Length];
			for (int counter = 0; counter < sortedTransforms.Length; counter++)
            {
                sortedTransforms[counter] = gameObjectsToSort_[counter].transform;
            }
			sortedTransforms = SortTransformsSelectionBySiblingIndex(sortedTransforms);

			GameObject[] sortedGameObjects = new GameObject[sortedTransforms.Length];
			for (int counter = 0; counter < sortedGameObjects.Length; counter++)
            {
				sortedGameObjects[counter] = sortedTransforms[counter].gameObject;
            }

			return sortedGameObjects;
		}

		/// <summary>
		/// Sorts the selected Transforms by sibling index into a new array.
		/// </summary>
		/// <returns>A sorted array of the selected Transforms.</returns>
		static public Transform[] SortTransformsSelectionBySiblingIndex(Transform[] transformsToSort_)
		{
			if (transformsToSort_ == null || transformsToSort_.Length == 0)
            {
                Debug.LogWarning("No Transforms selected; null returned.");
                return null;
            }
            else if (transformsToSort_[0].parent == null)
            {
				Debug.LogWarning("Selected Transforms must share a parent Transform; null returned.");
				return null;
            }

			Transform parentTransform = Selection.transforms[0].parent;

			for (int counter = 1; counter < Selection.gameObjects.Length; counter++)
			{
				if (Selection.transforms[counter].parent != parentTransform || Selection.transforms[counter].parent == null)
				{
					Debug.LogWarning("Selected Transforms must share a parent Transform; null returned.");
					return null;
				}
			}

			Transform[] sortedTransforms = transformsToSort_;

			Array.Sort(sortedTransforms, new TransformSiblingIComparer());

			return sortedTransforms;
		}

		/// <summary>
		/// IComparer for sorting Transform arrays based on their sibling indexes; use with System.Array.Sort().
		/// </summary>
		public class TransformSiblingIComparer : IComparer<Transform>
		{
			public int Compare(Transform firstTransform_, Transform secondTransform_)
			{
				if (firstTransform_.GetSiblingIndex() == secondTransform_.GetSiblingIndex())
                {
					return 0;
                }
				else if (firstTransform_.GetSiblingIndex() < secondTransform_.GetSiblingIndex())
                {
					return -1;
                }
				else
                {
					return 1;
                }
			}
		}
	}
}