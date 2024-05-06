using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
	public static string GetFullScenePath(this GameObject _gameObject)
	{
		Transform _nextTransform = _gameObject.transform;

		string _path = "";
		while (_nextTransform != null)
		{
			_path = "/" + _nextTransform.gameObject.name + _path;
			_nextTransform = _nextTransform.parent;
		}

		return _path;
	}

	public static List<GameObject> GetChildGameObjects(this GameObject gameObject, int searchDepth = 0)
	{
		return gameObject.transform.GetComponentsInChildren<Transform>(searchDepth).ConvertAll(transform => transform.gameObject);
	}

	public static void SortBySiblingIndex(this GameObject[] gameObjects)
	{
		if (gameObjects.Length == 0)
		{
			Debug.LogWarning("No GameObjects specified; null returned.");
			return;
		}

		Transform[] sortedTransforms = System.Array.ConvertAll(gameObjects, gameObject => gameObject.transform);
		sortedTransforms.SortBySiblingIndex();

		gameObjects = System.Array.ConvertAll(sortedTransforms, transform => transform.gameObject);
	}

	public static void SortBySiblingIndex(this List<GameObject> gameObjects)
	{
		if (gameObjects.Count == 0)
		{
			Debug.LogWarning("No GameObjects specified; null returned.");
			return;
		}

		List<Transform> sortedTransforms = gameObjects.ConvertAll(gameObject => gameObject.transform);
		sortedTransforms.SortBySiblingIndex();
		
		gameObjects = sortedTransforms.ConvertAll(transform => transform.gameObject);
	}
}
