using UnityEngine;

namespace HynesSight.Tweening
{
	public sealed class PositionTweenerComponent : TweenerComponent_Base
	{
		[SerializeField]
		private bool _localPosition;

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
			Tweener.PositionLerp(transform, _startValue, _endValue, _localPosition, _pingDuration, _loopCount, _muteX, _muteY, _muteZ, _useUnscaledTime, _updateType, _pingCurve, _onTweenEndEvent, _onPingEndEvent);
		}

		protected override void PlayPingPong_Internal()
		{
			Tweener.PositionPingPong(transform, _startValue, _endValue, _localPosition, _pingDuration, _pongDuration, _loopCount, _muteX, _muteY, _muteZ, _useUnscaledTime, _updateType, _pingCurve, _pongCurve, _onTweenEndEvent, _onPingEndEvent, _onPongEndEvent);
		}

		protected override void Pause_Internal()
		{
			Tweener.PausePositionTween(transform);
		}

		protected override void Resume_Internal()
		{
			Tweener.ResumePositionTween(transform);
		}

		protected override void Stop_Internal()
		{
			Tweener.StopPositionTween(transform);
		}
	}
}