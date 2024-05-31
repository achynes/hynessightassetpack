using UnityEngine;

namespace HynesSight.Tweening
{
	public sealed class SpriteColorTweenerComponent : TweenerComponent_Base
	{
		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Color32 _startValue = Color.white;
		[SerializeField]
		private Color32 _endValue = Color.white;

		protected override void PlayLerp_Internal()
		{
			Tweener.ColorLerp(_spriteRenderer, _startValue, _endValue, _pingDuration, _loopCount, _useUnscaledTime, _updateType, _pingCurve, _onTweenEndEvent, _onPingEndEvent);
		}

		protected override void PlayPingPong_Internal()
		{
			Tweener.ColorPingPong(_spriteRenderer, _startValue, _endValue, _pingDuration, _pongDuration, _loopCount, _useUnscaledTime, _updateType, _pingCurve, _pongCurve, _onTweenEndEvent, _onPingEndEvent, _onPongEndEvent);
		}

		protected override void Pause_Internal()
		{
			Tweener.PauseColorTween(_spriteRenderer);
		}

		protected override void Resume_Internal()
		{
			Tweener.ResumeColorTween(_spriteRenderer);
		}

		protected override void Stop_Internal()
		{
			Tweener.StopColorTween(_spriteRenderer);
		}
	}
}