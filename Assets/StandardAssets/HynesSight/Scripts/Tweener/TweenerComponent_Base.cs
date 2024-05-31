using UnityEngine;
using UnityEngine.Events;

namespace HynesSight.Tweening
{
	public abstract class TweenerComponent_Base : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField]
		private bool _shouldPingPong;

		[SerializeField]
		private bool _shouldLoopIndefinitely;
#endif

		[SerializeField]
		private bool _playOnStart;

		[SerializeField]
		protected bool _useUnscaledTime;

		[SerializeField]
		protected TweenUpdateType _updateType;

		private bool _isPlaying,
					 _isPaused;

		[SerializeField]
		protected float _pingDuration = 1.0f,
						_pongDuration = 1.0f;

		[SerializeField]
		protected int _loopCount = 1;

		[SerializeField]
		protected AnimationCurve _pingCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f),
								 _pongCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		[SerializeField]
		protected UnityEvent _onTweenEndEvent,
							 _onPingEndEvent,
							 _onPongEndEvent;

		private void Start()
		{
			if (_playOnStart)
			{
				Play();
			}
		}

		public void Play()
		{
			_isPlaying = true;

			if (_isPaused)
			{
				Resume();
			}
			else
			{
				if (_pongDuration > 0.0f)
				{
					PlayPingPong_Internal();
				}
				else
				{
					PlayLerp_Internal();
				}
			}
		}

		public void Pause()
		{
			if (_isPlaying)
			{
				_isPaused = true;

				Pause_Internal();
			}
		}

		public void Resume()
		{
			if (_isPlaying && _isPaused)
			{
				_isPaused = false;

				Resume_Internal();
			}
		}

		public void Stop()
		{
			if (_isPlaying)
			{
				_isPlaying = false;
				_isPaused = false;

				Stop_Internal();
			}
		}

		protected abstract void PlayLerp_Internal();
		protected abstract void PlayPingPong_Internal();

		protected abstract void Pause_Internal();
		protected abstract void Resume_Internal();
		protected abstract void Stop_Internal();
	}
}