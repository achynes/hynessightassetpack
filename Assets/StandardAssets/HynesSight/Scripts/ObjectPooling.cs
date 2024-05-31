using System.Collections.Generic;
using UnityEngine;

namespace HynesSight.ObjectPooling
{
	/// <summary>
	/// Class for pooling GameObjects. Cycles through IPoolableComponents on the GameObject and notifies them when it is pooled.
	/// </summary>
	public class GameObjectPool
	{
		// The template for adding more objects to the pool; this should typically be a prefab.
		GameObject _originalObject;

		Dictionary<GameObject, List<IPoolableComponent>> _pooledGameObjectsWithComponents;
		List<GameObject> _currentlyUnpooledGameObjects;

		public GameObjectPool(GameObject originalObject_, int initialPoolSize_)
		{
			_originalObject = originalObject_;
			_pooledGameObjectsWithComponents = new Dictionary<GameObject, List<IPoolableComponent>>(initialPoolSize_);
			_currentlyUnpooledGameObjects = new List<GameObject>(initialPoolSize_);
        
			for (int n = 0; n < initialPoolSize_; n++)
			{
				AddNewPoolMember();
			}
		}

		public GameObject GetGameObjectFromPool()
		{
			foreach (GameObject gameObject in _pooledGameObjectsWithComponents.Keys)
			{
				if (!_currentlyUnpooledGameObjects.Contains(gameObject))
				{
					gameObject.SetActive(true);

					_currentlyUnpooledGameObjects.Add(gameObject);

					List<IPoolableComponent> poolableComponents = _pooledGameObjectsWithComponents[gameObject];
					for (int n = poolableComponents.Count - 1; n > -1; n--)
					{
						poolableComponents[n].OnUnpooled();
					}

					return gameObject;
				}
			}

			GameObject newPoolMember = AddNewPoolMember();
			newPoolMember.SetActive(true);
			return newPoolMember;
		}

		public void ReturnGameObjectToPool(GameObject gameObjectToReturn_)
		{
			gameObjectToReturn_.SetActive(false);
			_currentlyUnpooledGameObjects.Remove(gameObjectToReturn_);
		}

		private GameObject AddNewPoolMember()
		{
			GameObject newObject = Object.Instantiate(_originalObject);
			newObject.SetActive(false);

			MonoBehaviour[] allMonoBehaviors = newObject.GetComponentsInChildren<MonoBehaviour>(true);
			List<IPoolableComponent> newPoolableComponents = new List<IPoolableComponent>(allMonoBehaviors.Length);

			for (int n = allMonoBehaviors.Length - 1; n > -1; n--)
			{
				IPoolableComponent poolableComponent = allMonoBehaviors[n] as IPoolableComponent;
				if (null != poolableComponent)
				{
					newPoolableComponents.Add(poolableComponent);
				}
			}

			newPoolableComponents.TrimExcess();
			_pooledGameObjectsWithComponents.Add(newObject, newPoolableComponents);

			return newObject;
		}
	}

	/// <summary>
	/// Interface required by poolable objects and GameObjects. Use the OnPooled function to reset values and state of the object.
	/// </summary>
	public interface IPoolableComponent
	{
		void OnUnpooled();
	}
}