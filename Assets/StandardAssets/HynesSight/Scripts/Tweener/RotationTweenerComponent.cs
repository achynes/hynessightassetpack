using UnityEngine;

namespace HynesSight.Tweening
{
	public sealed class RotationTweenerComponent : TweenerComponent_Base
	{
		[SerializeField]
		private bool _localRotation;

		[SerializeField]
		private Vector3 _startValue;
		[SerializeField]
		private Vector3 _endValue;

		[SerializeField]
		private bool _muteX = false,
					 _muteY = false,
					 _muteZ = false;

		protected override void PlayLerp_Internal()
		{
			Tweener.RotationLerp(transform, _startValue, _endValue, _localRotation, _pingDuration, _loopCount, _muteX, _muteY, _muteZ, _useUnscaledTime, _updateType, _pingCurve, _onTweenEndEvent, _onPingEndEvent);
		}

		protected override void PlayPingPong_Internal()
		{
			Tweener.RotationPingPong(transform, _startValue, _endValue, _localRotation, _pingDuration, _pongDuration, _loopCount, _muteX, _muteY, _muteZ, _useUnscaledTime, _updateType, _pingCurve, _pongCurve, _onTweenEndEvent, _onPingEndEvent, _onPongEndEvent);
		}

		protected override void Pause_Internal()
		{
			Tweener.PauseRotationTween(transform);
		}

		protected override void Resume_Internal()
		{
			Tweener.ResumeRotationTween(transform);
		}

		protected override void Stop_Internal()
		{
			Tweener.StopRotationTween(transform);
		}
	}
}