using UnityEngine;

namespace HynesSight.Tweening
{
	public enum TransformTweenStartType
	{
		Current = 0,
		Transform,
		Vector
	}

	public enum TransformTweenEndType
	{
		Transform = 0,
		Vector
	}

	public sealed class PositionTweenerComponent : TweenerComponent_Base
	{
		[SerializeField]
		private bool _localPosition;

		[SerializeField]
		private TransformTweenStartType _startType = TransformTweenStartType.Current;

		[SerializeField]
		private TransformTweenEndType _endType = TransformTweenEndType.Transform;

		[SerializeField]
		private Vector3 _startVector;

		[SerializeField]
		private Transform _startTransform;

		[SerializeField]
		private Vector3 _endVector;

		[SerializeField]
		private Transform _endTransform;

		[SerializeField]
		private bool _muteX = false,
					 _muteY = false,
					 _muteZ = false;

		private Vector3 StartValue
		{
			get
			{
				Vector3 startValue = Vector3.zero;

				switch (_startType)
				{
					case TransformTweenStartType.Current:
						startValue = _localPosition ? transform.InverseTransformPoint(transform.position) : transform.position;
						break;
					case TransformTweenStartType.Transform:
						startValue = _localPosition ? _startTransform.InverseTransformPoint(_startTransform.position) : _startTransform.position;
						break;
					case TransformTweenStartType.Vector:
						startValue = _startVector;
						break;
					default:
						DebugHelpers.PrintSwitchDefaultError();
						break;
				}

				return startValue;
			}
		}

		private Vector3 EndValue
		{
			get
			{
				Vector3 endValue = Vector3.zero;

				switch (_endType)
				{
					case TransformTweenEndType.Transform:
						endValue = _localPosition ? _endTransform.InverseTransformPoint(_endTransform.position) : _endTransform.position;
						break;
					case TransformTweenEndType.Vector:
						endValue = _endVector;
						break;
					default:
						DebugHelpers.PrintSwitchDefaultError();
						break;
				}

				return endValue;
			}
		}

		protected override void PlayLerp_Internal()
		{
			Tweener.PositionLerp(transform, StartValue, EndValue, _localPosition, _pingDuration, _loopCount, _muteX, _muteY, _muteZ, _useUnscaledTime, _updateType, _pingCurve, _onTweenEndEvent, _onPingEndEvent);
		}

		protected override void PlayPingPong_Internal()
		{
			Tweener.PositionPingPong(transform, StartValue, EndValue, _localPosition, _pingDuration, _pongDuration, _loopCount, _muteX, _muteY, _muteZ, _useUnscaledTime, _updateType, _pingCurve, _pongCurve, _onTweenEndEvent, _onPingEndEvent, _onPongEndEvent);
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