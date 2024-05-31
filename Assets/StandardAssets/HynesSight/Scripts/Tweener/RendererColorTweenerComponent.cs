using UnityEngine;

namespace HynesSight.Tweening
{
	public sealed class RendererColorTweenerComponent : TweenerComponent_Base
	{
		[SerializeField]
		private Renderer _renderer;
		[SerializeField]
		private int _materialIndex;

		[SerializeField]
		private Color32 _startValue = Color.white;
		[SerializeField]
		private Color32 _endValue = Color.white;

		protected override void PlayLerp_Internal()
		{
			Tweener.ColorLerp(_renderer, _startValue, _endValue, _pingDuration, _materialIndex, _loopCount, _useUnscaledTime, _updateType, _pingCurve, _onTweenEndEvent, _onPingEndEvent);
		}

		protected override void PlayPingPong_Internal()
		{
			Tweener.ColorPingPong(_renderer, _startValue, _endValue, _pingDuration, _pongDuration, _materialIndex, _loopCount, _useUnscaledTime, _updateType, _pingCurve, _pongCurve, _onTweenEndEvent, _onPingEndEvent, _onPongEndEvent);
		}

		protected override void Pause_Internal()
		{
			Tweener.PauseColorTween(_renderer);
		}

		protected override void Resume_Internal()
		{
			Tweener.ResumeColorTween(_renderer);
		}

		protected override void Stop_Internal()
		{
			Tweener.StopColorTween(_renderer);
		}
	}
}