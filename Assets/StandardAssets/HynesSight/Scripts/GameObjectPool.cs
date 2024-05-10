using System.Collections.Generic;
using UnityEngine;

namespace HynesSight
{
	// Interface required by poolable objects and GameObjects. Use the OnPooled function to reset values and state of the object.
	public interface IPoolableComponent
	{
		void OnPooled();
		void OnUnpooled();
	}

	// Class for pooling GameObjects. Cycles through IPoolableComponents on the GameObject and notifies them when it is pooled.
	public class GameObjectPool
	{
		// The template for adding more objects to the pool; this should typically be a prefab.
		GameObject _originalObject;

		Dictionary<GameObject, List<IPoolableComponent>> _pooledGameObjectsWithComponents;
		List<GameObject> _currentlyUnpooledGameObjects;

		public GameObjectPool(GameObject originalObject, int initialPoolSize)
		{
			_originalObject = originalObject;
			_pooledGameObjectsWithComponents = new Dictionary<GameObject, List<IPoolableComponent>>(initialPoolSize);
			_currentlyUnpooledGameObjects = new List<GameObject>(initialPoolSize);
        
			for (int n = 0; n < initialPoolSize; n++)
				AddNewPoolMember(startActive: false);
		}

		public GameObject GetGameObjectFromPool()
		{
			GameObject selectedGameObject = null;

			foreach (GameObject gameObject in _pooledGameObjectsWithComponents.Keys)
			{
				if (!_currentlyUnpooledGameObjects.Contains(gameObject))
				{
					selectedGameObject = gameObject;
					break;
				}
			}

			if (selectedGameObject == null)
			 selectedGameObject = AddNewPoolMember(true);

			_currentlyUnpooledGameObjects.Add(selectedGameObject);

			selectedGameObject.SetActive(true);

			List<IPoolableComponent> poolableComponents = _pooledGameObjectsWithComponents[selectedGameObject];
			for (int n = poolableComponents.Count - 1; n > -1; n--)
				poolableComponents[n].OnUnpooled();

			return selectedGameObject;
		}

		public void ReturnGameObjectToPool(GameObject gameObjectToReturn)
		{
			gameObjectToReturn.SetActive(false);

			List<IPoolableComponent> poolableComponents = _pooledGameObjectsWithComponents[gameObjectToReturn];
			for (int n = poolableComponents.Count - 1; n > -1; n--)
				poolableComponents[n].OnPooled();

			_currentlyUnpooledGameObjects.Remove(gameObjectToReturn);
		}

		private GameObject AddNewPoolMember(bool startActive)
		{
			GameObject newObject = Object.Instantiate(_originalObject);
			newObject.SetActive(startActive);

			MonoBehaviour[] allMonoBehaviors = newObject.GetComponentsInChildren<MonoBehaviour>(true);
			List<IPoolableComponent> newPoolableComponents = new List<IPoolableComponent>(allMonoBehaviors.Length);

			for (int n = allMonoBehaviors.Length - 1; n > -1; n--)
			{
				IPoolableComponent poolableComponent = allMonoBehaviors[n] as IPoolableComponent;
				if (null != poolableComponent)
					newPoolableComponents.Add(poolableComponent);
			}

			newPoolableComponents.TrimExcess();
			_pooledGameObjectsWithComponents.Add(newObject, newPoolableComponents);

			return newObject;
		}
	}
}