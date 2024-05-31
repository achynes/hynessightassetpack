using UnityEngine;
using UnityEngine.UI;

namespace HynesSight.Tweening
{
	public sealed class UIGraphicColorTweenerComponent : TweenerComponent_Base
	{
		[SerializeField]
		private Graphic _graphic;

		[SerializeField]
		private Color32 _startValue = Color.white;
		[SerializeField]
		private Color32 _endValue = Color.white;
		
		protected override void PlayLerp_Internal()
		{
			Tweener.ColorLerp(_graphic, _startValue, _endValue, _pingDuration, _loopCount, _useUnscaledTime, _updateType, _pingCurve, _onTweenEndEvent, _onPingEndEvent);
		}

		protected override void PlayPingPong_Internal()
		{
			Tweener.ColorPingPong(_graphic, _startValue, _endValue, _pingDuration, _pongDuration, _loopCount, _useUnscaledTime, _updateType, _pingCurve, _pongCurve, _onTweenEndEvent, _onPingEndEvent, _onPongEndEvent);
		}

		protected override void Pause_Internal()
		{
			Tweener.PauseColorTween(_graphic);
		}

		protected override void Resume_Internal()
		{
			Tweener.ResumeColorTween(_graphic);
		}

		protected override void Stop_Internal()
		{
			Tweener.StopColorTween(_graphic);
		}
	}
}