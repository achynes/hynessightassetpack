using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
	// Depth of 1 means we only search the first layer of children under the Transform, and so on. If depth is less than 1, we do a full depth search.
	public static List<T> GetComponentsInChildren<T>(this Transform transform, int searchDepth = 0)
	{
		List<T> resultList = new List<T>();

		if (searchDepth <= 0)
		{
			// Easy case, native call does a full-depth search already.
			transform.GetComponentsInChildren(resultList);
			return resultList;
		}

		// Get from self first.
		transform.GetComponents(resultList);

		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform child = transform.GetChild(i);

			// Use recursion to search deeper until you hit the last layer.
			if (searchDepth <= 1)
				resultList.AddRange(child.GetComponents<T>());
			else
				resultList.AddRange(child.GetComponentsInChildren<T>(searchDepth - 1));
		}

		return resultList;
	}

	public static void SortBySiblingIndex(this Transform[] transforms)
	{
		if (transforms == null || transforms.Length == 0)
		{
			Debug.LogWarning("No Transforms specified; null returned.");
			return;
		}
		else if (transforms[0].parent == null)
		{
			Debug.LogWarning("Specified Transforms must share a parent Transform; null returned.");
			return;
		}

		Transform parentTransform = transforms[0].parent;

		foreach (Transform transformToSort in transforms)
		{
			if (transformToSort.parent != parentTransform || transformToSort.parent == null)
			{
				Debug.LogWarning("Specified Transforms must share a parent Transform; null returned.");
				return;
			}
		}

		System.Array.Sort(transforms, new TransformSiblingComparer());
	}

	public static void SortBySiblingIndex(this List<Transform> transforms)
	{
		if (transforms == null || transforms.Count == 0)
		{
			Debug.LogWarning("No Transforms specified; null returned.");
			return;
		}
		else if (transforms[0].parent == null)
		{
			Debug.LogWarning("Specified Transforms must share a parent Transform; null returned.");
			return;
		}

		Transform parentTransform = transforms[0].parent;

		foreach (Transform transformToSort in transforms)
		{
			if (transformToSort.parent != parentTransform || transformToSort.parent == null)
			{
				Debug.LogWarning("Specified Transforms must share a parent Transform; null returned.");
				return;
			}
		}

		transforms.Sort(new TransformSiblingComparer());
	}

	// IComparer for sorting Transform arrays based on their sibling indexes; use with System.Array.Sort().
	class TransformSiblingComparer : IComparer<Transform>
	{
		public int Compare(Transform firstTransform, Transform secondTransform)
		{
			if (firstTransform.GetSiblingIndex() == secondTransform.GetSiblingIndex())
				return 0;

			if (firstTransform.GetSiblingIndex() < secondTransform.GetSiblingIndex())
				return -1;

			return 1;
		}
	}
}
