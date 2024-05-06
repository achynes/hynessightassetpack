using UnityEngine;

namespace HynesSight
{
    /// <summary>
    /// Singleton base class; provides a static accessor to Components that are only ever in one instance.
    /// If marked as a permanent singleton, the root gameObject is not destroyed when loading new scenes.
    /// </summary>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
	    /// <summary>
	    /// Static singleton instance of the component.
	    /// </summary>
	    public static T Instance { get; private set; }

	    /// <summary>
	    /// If true, the component's root Transform is marked as DontDestroyOnLoad().
	    /// </summary>
	    public bool _dontDestroyOnLoad = true;

	    /// <summary>
	    /// Singleton Awake callback sets up the static Instance of the component. Make sure to call base.Awake() when overriding to keep this behaviour.
	    /// </summary>
	    protected virtual void Awake()
	    {
		    // If an instance of this singleton already exists, destroy this one before it causes ambiguity.
		    if (Instance != null)
            {
                Debug.LogWarning("Tried to instantiate two singletons of type: " + GetType());
			    Destroy(this);
                return;
            }

            Instance = this as T;
		
		    if (_dontDestroyOnLoad)
            {
			    DontDestroyOnLoad(transform.root);
            }
	    }

	    protected virtual void OnDestroy()
	    {
		    // If the object is destroyed, detach it from the static instance.
		    if (Instance == this)
            {
			    Instance = null;
            }
	    }

		private static T CreateInstance(string gameObjectName, bool dontDestroyOnLoad)
		{
			if (null != Instance)
			{
				Debug.LogWarning("Tried to instantiate two singletons of type: " + Instance.GetType());
				return Instance;
			}

			GameObject holderObject = new GameObject(gameObjectName);

			T singletonComponent = holderObject.AddComponent<T>();
			singletonComponent._dontDestroyOnLoad = dontDestroyOnLoad;

			return singletonComponent;
		}
    }
}
