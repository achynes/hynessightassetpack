using System.Collections.Generic;
using UnityEngine;

namespace HynesSight
{
	public sealed class Echoer : SingletonBehaviour<Echoer>
	{
		private class EchoHandler
		{
			private DynamicParamsDelegate _delegate;
			private object[] _params;

			private float _delay;
			private int _repeatCount;

			private float _startTime;
			private bool _useUnscaledTime;

			private bool _paused;

			public EchoHandler(DynamicParamsDelegate inDelegate, float delay, float currentTime, int repeatCount, bool useUnscaledTime, params object[] inParams)
			{
				_delegate = inDelegate;
				_delay = delay;
				_startTime = currentTime;
				_repeatCount = repeatCount;
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

				if (currentTime - _startTime > _delay)
				{
					_delegate.Invoke(_params);

					if (_repeatCount > 0)
					{
						_repeatCount--;

						if (_repeatCount == 0)
						{
							return true;
						}

						_startTime = currentTime;
					}
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

		private static Dictionary<int, EchoHandler> _echoHandlers = new Dictionary<int, EchoHandler>();

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
			if (_echoHandlers.Count > 0)
			{
				float time = Time.time;

				foreach (KeyValuePair<int, EchoHandler> echoHandler in _echoHandlers)
				{
					if (echoHandler.Value.CheckTime())
					{
						_indexesToRemove.Add(echoHandler.Key);
					}
				}

				foreach (int n in _indexesToRemove)
				{
					StopEcho(n);
				}

				_indexesToRemove.Clear();
			}
		}

		/// <summary>
		/// Called before any Echo functions to instantiate the Echoer singleton instance.
		/// This instance will be DontDestroyOnLoad.
		/// The Echoer is lazily instantiated if not called manually, but it's better to call this at the start.
		/// </summary>
		public static void InitTweener()
		{
			if (null != Instance)
			{
				Debug.LogWarning("Echoer is already initialised.");
				return;
			}

			GameObject echoerGO = new GameObject("Echoer");
			Echoer echoer = echoerGO.AddComponent<Echoer>();
			echoer._dontDestroyOnLoad = true;
			_isInitialised = true;
		}

		private static void PreEchoChecks()
		{
			CheckInitialised();

			while (_echoHandlers.ContainsKey(_currentIndex))
			{
				_currentIndex++;
			}
		}

		private static void CheckInitialised()
		{
			if (!_isInitialised)
			{
				Debug.Log("Echoer lazily initialised");
				InitTweener();
			}
		}

		private static int Echo(DynamicParamsDelegate inDelegate, float delay, float currentTime, int repeatCount, bool useUnscaledTime = false, params object[] inParams)
		{
			PreEchoChecks();

			_echoHandlers.Add(_currentIndex, new EchoHandler(inDelegate, delay, currentTime, repeatCount, useUnscaledTime, inParams));

			return _currentIndex;
		}

		public static void ResumeEcho(int index)
		{
			CheckInitialised();

			if (_echoHandlers.ContainsKey(index))
			{
				_echoHandlers[index].TogglePaused(false);
			}
		}

		public static void PauseEcho(int index)
		{
			CheckInitialised();

			if (_echoHandlers.ContainsKey(index))
			{
				_echoHandlers[index].TogglePaused(true);
			}
		}

		public static void ResetEchoTimer(int index)
		{
			CheckInitialised();

			if (_echoHandlers.ContainsKey(index))
			{
				_echoHandlers[index].ResetTimer(Time.time);
			}
		}

		public static void StopEcho(int index)
		{
			CheckInitialised();

			if (_echoHandlers.ContainsKey(index))
			{
				_echoHandlers.Remove(index);
			}
		}

		public static void StopAllEcho()
		{
			CheckInitialised();

			if (_echoHandlers.Count > 0)
			{
				foreach (int index in _echoHandlers.Keys)
				{
					StopEcho(index);
				}

				_echoHandlers = new Dictionary<int, EchoHandler>();
			}
		}

		public static bool IsEchoRunningAtIndex(int index)
		{
			return _echoHandlers.ContainsKey(index);
		}
	}
}