using System.Collections.Generic;
using UnityEngine;

namespace HynesSight
{
	public sealed class DelegateRepeater : SingletonBehaviour<DelegateRepeater>
	{
		private class DelegateRepeatHandler
		{
			private DynamicParamsDelegate _delegate;
			private object[] _params;

			private float _delay;
			private int _callCount;

			private float _startTime;
			private bool _useUnscaledTime;

			private bool _paused;

			public DelegateRepeatHandler(DynamicParamsDelegate inDelegate, float delay, int callCount, bool useUnscaledTime, params object[] inParams)
			{
				_delegate = inDelegate;
				_delay = delay;
				_startTime = useUnscaledTime ? Time.unscaledTime : Time.time;
				_callCount = callCount;
				_useUnscaledTime = useUnscaledTime;
				_params = inParams;
			}

			public bool CheckTime()
			{
				if (_paused)
				{
					return false;
				}

				float currentTime = _useUnscaledTime ? Time.unscaledTime : Time.time;

				if (currentTime - _startTime >= _delay)
				{
					_delegate.Invoke(_params);
					
					_callCount--;
					
					if (_callCount <= 0)
						return true;
					else
						_startTime = currentTime;
				}

				return false;
			}

			public void TogglePaused(bool paused)
			{
				_paused = paused;
			}

			public void ResetTimer(float currentTime)
			{
				_startTime = currentTime;
			}
		}
		
		private static bool _isInitialised;

		private static Dictionary<int, DelegateRepeatHandler> _repeatHandlers = new Dictionary<int, DelegateRepeatHandler>();

		private static int _currentIndex;

		private static List<int> _indexesToRemove;

		protected override void OnDestroy()
		{
			if (Instance == this)
			{
				_isInitialised = false;
			}

			base.OnDestroy();
		}

		private void Update()
		{
			if (_repeatHandlers.Count > 0)
			{
				float time = Time.time;

				foreach (KeyValuePair<int, DelegateRepeatHandler> repeatHandler in _repeatHandlers)
				{
					if (repeatHandler.Value.CheckTime())
					{
						_indexesToRemove.Add(repeatHandler.Key);
					}
				}

				foreach (int n in _indexesToRemove)
				{
					StopRepeating(n);
				}

				_indexesToRemove.Clear();
			}
		}

		/// <summary>
		/// Called before any Repeater functions to instantiate the Repeater singleton instance.
		/// This instance will be DontDestroyOnLoad.
		/// The Repeater is lazily instantiated if not called manually, but it's better to call this at the start.
		/// </summary>
		public static void InitDelegateRepeater()
		{
			if (null != Instance)
			{
				Debug.LogWarning("Delegate Repeater is already initialised.");
				return;
			}

			GameObject repeaterGo = new GameObject("Delegate Repeater");
			DelegateRepeater repeater = repeaterGo.AddComponent<DelegateRepeater>();
			repeater._dontDestroyOnLoad = true;
			_isInitialised = true;
		}

		private static void NewRepeatDelegateChecks()
		{
			CheckInitialised();

			while (_repeatHandlers.ContainsKey(_currentIndex))
			{
				_currentIndex++;
			}
		}

		private static void CheckInitialised()
		{
			if (!_isInitialised)
			{
				Debug.Log("Delegate Repeater lazily initialised");
				InitDelegateRepeater();
			}
		}

		private static int StartRepeating(DynamicParamsDelegate inDelegate, float delay, int callCount, bool useUnscaledTime = false, params object[] inParams)
		{
			if (inDelegate == null)
			{
				Debug.LogError("Asked to call a null delegate. Discarded the request.");
				return -1;
			}

			if (inDelegate.Target == null || inDelegate.Method == null)
			{
				Debug.LogErrorFormat("Asked to call a delegate with an invalid Target or Method. Registered method was %s.%s.", inDelegate.Target != null ? inDelegate.Target : "NULL", inDelegate.Method != null ? inDelegate.Method : "NULL");
				return -1;
			}

			if (callCount <= 0)
			{
				Debug.LogErrorFormat("Asked to call a delegate %i times. Registered method was %s.%s. Discarded the request.", callCount, inDelegate.Target, inDelegate.Method.Name);
				return -1;
			}

			NewRepeatDelegateChecks();

			_repeatHandlers.Add(_currentIndex, new DelegateRepeatHandler(inDelegate, delay, callCount, useUnscaledTime, inParams));

			return _currentIndex;
		}

		public static void ResumeRepeating(int index)
		{
			CheckInitialised();

			if (_repeatHandlers.ContainsKey(index))
			{
				_repeatHandlers[index].TogglePaused(false);
			}
		}

		public static void PauseRepeating(int index)
		{
			CheckInitialised();

			if (_repeatHandlers.ContainsKey(index))
			{
				_repeatHandlers[index].TogglePaused(true);
			}
		}

		public static void ResetRepeaterTimer(int index)
		{
			CheckInitialised();

			if (_repeatHandlers.ContainsKey(index))
			{
				_repeatHandlers[index].ResetTimer(Time.time);
			}
		}

		public static void StopRepeating(int index)
		{
			CheckInitialised();

			if (_repeatHandlers.ContainsKey(index))
			{
				_repeatHandlers.Remove(index);
			}
		}

		public static void StopAllRepeating()
		{
			CheckInitialised();

			if (_repeatHandlers.Count > 0)
			{
				foreach (int index in _repeatHandlers.Keys)
				{
					StopRepeating(index);
				}

				_repeatHandlers = new Dictionary<int, DelegateRepeatHandler>();
			}
		}

		public static bool IsDelegateRepeatingAtIndex(int index)
		{
			return _repeatHandlers.ContainsKey(index);
		}
	}
}