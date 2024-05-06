using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace HynesSight.Tweening
{
	public enum TweenUpdateType
	{
		Update = 0,
		LateUpdate = 1,
		FixedUpdate = 2
	}

	/// <summary>
	/// Singleton manager for tweening values.
	/// </summary>
	public sealed class Tweener : SingletonBehaviour<Tweener>
	{
		private abstract class TweenHelper_Base
		{
			private float _timer,
						  _pingDuration,
						  _pongDuration;

			private bool _useUnscaledTime,
						 _useFixedDeltaTime;

			private int _loopCounter;

			private bool _isPinging,
						 _isPaused,
						 _isFinished;

			private AnimationCurve _pingCurve,
								   _pongCurve;

			private UnityEvent _onTweenEndEvent,
							   _onPingEndEvent,
							   _onPongEndEvent;

			protected abstract void ModifyTarget(float lerpValue);
			protected abstract bool CanTween();

			protected TweenHelper_Base(float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
			{
				_timer = 0.0f;
				_isPinging = true;
				_pingDuration = pingDuration;
				_pongDuration = pongDuration;
				_loopCounter = loopCount;
				_useUnscaledTime = useUnscaledTime;
				_useFixedDeltaTime = useFixedDeltaTime;
				_onTweenEndEvent = onTweenEndEvent;
				_onPingEndEvent = onPingEndEvent;
				_onPongEndEvent = onPongEndEvent;

				_pingCurve = (null == pingCurve) ? _defaultCurve : pingCurve;
				_pongCurve = (null == pongCurve) ? _defaultCurve : pongCurve;
			}

			/// <summary>
			/// After updating the lerp, returns:
			/// true if the lerp has finished (including all extra loops),
			/// true if the renderer has become null,
			/// false if the lerp is meant to loop indefinitely,
			/// false if the lerp simply hasn't reached the target yet.
			/// </summary>
			public bool UpdateLerp()
			{
				if (!CanTween())
				{
					return true;
				}

				if (_isPaused)
				{
					return false;
				}

				if(_useFixedDeltaTime)
				{
					_timer += _useUnscaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;
				}
				else
				{
					_timer += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
				}

				float duration = _isPinging ? _pingDuration : _pongDuration;
				float lerpValue = _isPinging ? (_timer / duration) : (1.0f - (_timer / duration));

				ModifyTarget(_isPinging ? _pingCurve.Evaluate(lerpValue) : _pongCurve.Evaluate(lerpValue));

				bool isFinished = false;

				if (_timer >= duration)
				{
					_timer = 0.0f;

					if (_pongDuration > 0.0f)
					{
						if (_isPinging)
						{
							if (null != _onPingEndEvent)
							{
								_onPingEndEvent.Invoke();
							}
						}
						else
						{
							if (null != _onPongEndEvent)
							{
								_onPongEndEvent.Invoke();
							}

							if (_loopCounter > 0)
							{
								_loopCounter--;

								if (_loopCounter == 0)
								{
									isFinished = true;
								}
							}
						}

						_isPinging = !_isPinging;
					}
					else if (_loopCounter > 0)
					{
						if (null != _onPingEndEvent)
						{
							_onPingEndEvent.Invoke();
						}

						_loopCounter--;

						if (_loopCounter == 0)
						{
							isFinished = true;
						}
					}
				}

				return isFinished;
			}

			public void TogglePaused(bool paused)
			{
				_isPaused = paused;
			}

			public void OnFinished()
			{
				if (_isFinished)
				{
					return;
				}

				_isFinished = true;
				_needsCleanUp = true;

				if (null != _onTweenEndEvent)
				{
					_onTweenEndEvent.Invoke();
				}
			}
		}

		private sealed class ColorTweenHelper_Renderer : TweenHelper_Base
		{
			private Renderer _renderer;

			private int _materialIndex;

			private Color32 _startColor,
							_endColor;

			public ColorTweenHelper_Renderer(Renderer renderer, Color32 startColor, Color32 endColor, float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, int materialIndex, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
				: base(pingDuration, pongDuration, loopCount, useUnscaledTime, useFixedDeltaTime, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent)
			{
				_renderer = renderer;
				_startColor = startColor;
				_endColor = endColor;
				_materialIndex = materialIndex;
			}

			protected override bool CanTween()
			{
				return null != _renderer;
			}

			protected override void ModifyTarget(float lerpValue)
			{
				_renderer.materials[_materialIndex].color = Color32.Lerp(_startColor, _endColor, lerpValue);
			}
		}

		private sealed class ColorTweenHelper_Graphic : TweenHelper_Base
		{
			private Graphic _graphic;

			private Color32 _startColor,
							_endColor;

			public ColorTweenHelper_Graphic(Graphic graphic, Color32 startColor, Color32 endColor, float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
				: base(pingDuration, pongDuration, loopCount, useUnscaledTime, useFixedDeltaTime, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent)
			{
				_graphic = graphic;
				_startColor = startColor;
				_endColor = endColor;
			}

			protected override bool CanTween()
			{
				return null != _graphic;
			}

			protected override void ModifyTarget(float lerpValue)
			{
				_graphic.color = Color32.Lerp(_startColor, _endColor, lerpValue);
			}
		}

		private abstract class TransformTweenHelper_Base : TweenHelper_Base
		{
			protected Transform _transform;

			protected bool _muteX,
						   _muteY,
						   _muteZ;

			protected sealed override bool CanTween()
			{
				return null != _transform;
			}

			public TransformTweenHelper_Base(Transform transform, bool muteX, bool muteY, bool muteZ, float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
				: base(pingDuration, pongDuration, loopCount, useUnscaledTime, useFixedDeltaTime, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent)
			{
				_transform = transform;

				_muteX = muteX;
				_muteY = muteY;
				_muteZ = muteZ;
			}
		}

		private sealed class PositionTweenHelper : TransformTweenHelper_Base
		{
			private Vector3 _startPosition,
							_endPosition;

			bool _isLocalPosition;

			public PositionTweenHelper(Transform transform, Vector3 startPosition, Vector3 endPosition, bool isLocalPosition, bool muteX, bool muteY, bool muteZ, float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
				: base(transform, muteX, muteY, muteZ, pingDuration, pongDuration, loopCount, useUnscaledTime, useFixedDeltaTime, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent)
			{
				_startPosition = startPosition;
				_endPosition = endPosition;
				_isLocalPosition = isLocalPosition;
			}

			protected override void ModifyTarget(float lerpValue)
			{
				Vector3 newPosition = Vector3.Lerp(_startPosition, _endPosition, lerpValue);

				if (_muteX)
				{
					newPosition.x = _transform.position.x;
				}
				if (_muteY)
				{
					newPosition.y = _transform.position.y;
				}
				if (_muteZ)
				{
					newPosition.z = _transform.position.z;
				}

				if (_isLocalPosition)
				{
					_transform.localPosition = newPosition;
				}
				else
				{
					_transform.position = newPosition;
				}
			}
		}

		private sealed class RotationTweenHelper : TransformTweenHelper_Base
		{
			private float _startRotationX,
						  _startRotationY,
						  _startRotationZ,
						  _endRotationX,
						  _endRotationY,
						  _endRotationZ;

			bool _isLocalRotation;

			public RotationTweenHelper(Transform transform, Vector3 startRotation, Vector3 endRotation, bool isLocalRotation, bool muteX, bool muteY, bool muteZ, float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
				: base(transform, muteX, muteY, muteZ, pingDuration, pongDuration, loopCount, useUnscaledTime, useFixedDeltaTime, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent)
			{
				_startRotationX = startRotation.x;
				_startRotationY = startRotation.y;
				_startRotationZ = startRotation.z;
				_endRotationX = endRotation.x;
				_endRotationY = endRotation.y;
				_endRotationZ = endRotation.z;
				_isLocalRotation = isLocalRotation;
			}
			
			protected override void ModifyTarget(float lerpValue)
			{
				float xRotation = _muteX ? _transform.rotation.eulerAngles.x : Mathf.Lerp(_startRotationX, _endRotationX, lerpValue),
					  yRotation = _muteY ? _transform.rotation.eulerAngles.y : Mathf.Lerp(_startRotationY, _endRotationY, lerpValue),
					  zRotation = _muteZ ? _transform.rotation.eulerAngles.z : Mathf.Lerp(_startRotationZ, _endRotationZ, lerpValue);

				Quaternion _newRotation = Quaternion.Euler(xRotation, yRotation, zRotation);

				if (_isLocalRotation)
				{
					_transform.localRotation = _newRotation;
				}
				else
				{
					_transform.rotation = _newRotation;
				}
			}
		}

		private sealed class ScaleTweenHelper : TransformTweenHelper_Base
		{
			private Vector3 _startScale,
							_endScale;

			public ScaleTweenHelper(Transform transform, Vector3 startScale, Vector3 endScale, bool muteX, bool muteY, bool muteZ, float pingDuration, float pongDuration, int loopCount, bool useUnscaledTime, bool useFixedDeltaTime, AnimationCurve pingCurve, AnimationCurve pongCurve, UnityEvent onTweenEndEvent, UnityEvent onPingEndEvent, UnityEvent onPongEndEvent)
				: base(transform, muteX, muteY, muteZ, pingDuration, pongDuration, loopCount, useUnscaledTime, useFixedDeltaTime, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent)
			{
				_startScale = startScale;
				_endScale = endScale;
			}
			
			protected override void ModifyTarget(float lerpValue)
			{
				Vector3 newScale = Vector3.Lerp(_startScale, _endScale, lerpValue);

				if (_muteX)
				{
					newScale.x = _transform.localScale.x;
				}
				if (_muteY)
				{
					newScale.y = _transform.localScale.y;
				}
				if (_muteZ)
				{
					newScale.z = _transform.localScale.z;
				}

				_transform.localScale = newScale;
			}
		}
		
		// When null is passed to a Tween function for an AnimationCurve value, we default to this value.
		private static readonly AnimationCurve _defaultCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
		
		// Optimisation flags. These prevent the Tweener from performing loops over the Dictionaries and Lists when there are no Tweens active.
		private static bool _needsUpdate,
							_needsLateUpdate,
							_needsFixedUpdate,
							_needsCleanUp;
		
		private static Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> _colorTweenHelpersRenderer_Update = new Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base>();
		private static Dictionary<Graphic, TweenHelper_Base> _colorTweenHelpersGraphic_Update = new Dictionary<Graphic, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _positionTweenHelpers_Update = new Dictionary<Transform, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _rotationTweenHelpers_Update = new Dictionary<Transform, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _scaleTweenHelpers_Update = new Dictionary<Transform, TweenHelper_Base>();

		private static List<KeyValuePair<Renderer, int>> _colorTweensToRemoveRenderer_Update = new List<KeyValuePair<Renderer, int>>();
		private static List<Graphic> _colorTweensToRemoveGraphic_Update = new List<Graphic>();
		private static List<Transform> _positionTweensToRemove_Update = new List<Transform>();
		private static List<Transform> _rotationTweensToRemove_Update = new List<Transform>();
		private static List<Transform> _scaleTweensToRemove_Update = new List<Transform>();

		private static Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> _colorTweenHelpersRenderer_LateUpdate = new Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base>();
		private static Dictionary<Graphic, TweenHelper_Base> _colorTweenHelpersGraphic_LateUpdate = new Dictionary<Graphic, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _positionTweenHelpers_LateUpdate = new Dictionary<Transform, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _rotationTweenHelpers_LateUpdate = new Dictionary<Transform, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _scaleTweenHelpers_LateUpdate = new Dictionary<Transform, TweenHelper_Base>();

		private static List<KeyValuePair<Renderer, int>> _colorTweensToRemoveRenderer_LateUpdate = new List<KeyValuePair<Renderer, int>>();
		private static List<Graphic> _colorTweensToRemoveGraphic_LateUpdate = new List<Graphic>();
		private static List<Transform> _positionTweensToRemove_LateUpdate = new List<Transform>();
		private static List<Transform> _rotationTweensToRemove_LateUpdate = new List<Transform>();
		private static List<Transform> _scaleTweensToRemove_LateUpdate = new List<Transform>();

		private static Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> _colorTweenHelpersRenderer_FixedUpdate = new Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base>();
		private static Dictionary<Graphic, TweenHelper_Base> _colorTweenHelpersGraphic_FixedUpdate = new Dictionary<Graphic, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _positionTweenHelpers_FixedUpdate = new Dictionary<Transform, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _rotationTweenHelpers_FixedUpdate = new Dictionary<Transform, TweenHelper_Base>();
		private static Dictionary<Transform, TweenHelper_Base> _scaleTweenHelpers_FixedUpdate = new Dictionary<Transform, TweenHelper_Base>();

		private static List<KeyValuePair<Renderer, int>> _colorTweensToRemoveRenderer_FixedUpdate = new List<KeyValuePair<Renderer, int>>();
		private static List<Graphic> _colorTweensToRemoveGraphic_FixedUpdate = new List<Graphic>();
		private static List<Transform> _positionTweensToRemove_FixedUpdate = new List<Transform>();
		private static List<Transform> _rotationTweensToRemove_FixedUpdate = new List<Transform>();
		private static List<Transform> _scaleTweensToRemove_FixedUpdate = new List<Transform>();
		
		private void Update()
		{
			if (_needsUpdate)
			{
				UpdateTweenHelpers(_colorTweenHelpersRenderer_Update, _colorTweensToRemoveRenderer_Update);
				UpdateTweenHelpers(_colorTweenHelpersGraphic_Update, _colorTweensToRemoveGraphic_Update);
				UpdateTweenHelpers(_positionTweenHelpers_Update, _positionTweensToRemove_Update);
				UpdateTweenHelpers(_rotationTweenHelpers_Update, _rotationTweensToRemove_Update);
				UpdateTweenHelpers(_scaleTweenHelpers_Update, _scaleTweensToRemove_Update);
			}
		}

		private void LateUpdate()
		{
			if (_needsLateUpdate)
			{
				UpdateTweenHelpers(_colorTweenHelpersRenderer_LateUpdate, _colorTweensToRemoveRenderer_LateUpdate);
				UpdateTweenHelpers(_colorTweenHelpersGraphic_LateUpdate, _colorTweensToRemoveGraphic_LateUpdate);
				UpdateTweenHelpers(_positionTweenHelpers_LateUpdate, _positionTweensToRemove_LateUpdate);
				UpdateTweenHelpers(_rotationTweenHelpers_LateUpdate, _rotationTweensToRemove_LateUpdate);
				UpdateTweenHelpers(_scaleTweenHelpers_LateUpdate, _scaleTweensToRemove_LateUpdate);
			}

			CleanUpFinishedTweens();
		}

		private void FixedUpdate()
		{
			if (_needsFixedUpdate)
			{
				UpdateTweenHelpers(_colorTweenHelpersRenderer_FixedUpdate, _colorTweensToRemoveRenderer_FixedUpdate);
				UpdateTweenHelpers(_colorTweenHelpersGraphic_FixedUpdate, _colorTweensToRemoveGraphic_FixedUpdate);
				UpdateTweenHelpers(_positionTweenHelpers_FixedUpdate, _positionTweensToRemove_FixedUpdate);
				UpdateTweenHelpers(_rotationTweenHelpers_FixedUpdate, _rotationTweensToRemove_FixedUpdate);
				UpdateTweenHelpers(_scaleTweenHelpers_FixedUpdate, _scaleTweensToRemove_FixedUpdate);
			}

			CleanUpFinishedTweens();
		}

		/// <summary>
		/// Update the Tweeners in a given Dictionary.
		/// </summary>
		/// <typeparam name="T">The type of the Dictionary Keys.</typeparam>
		/// <param name="tweenHelpers_">The Dictionary of Tweeners to update.</param>
		/// <param name="tweensToRemove_">List to use in cleaning up finished Tweens.</param>
		private static void UpdateTweenHelpers<T>(Dictionary<T, TweenHelper_Base> tweenHelpers, List<T> tweensToRemove)
		{
			if (tweenHelpers.Count > 0)
			{
				float deltaTime = Time.deltaTime;

				foreach (KeyValuePair<T, TweenHelper_Base> tweenHelper in tweenHelpers)
				{
					if (!tweensToRemove.Contains(tweenHelper.Key) && tweenHelper.Value.UpdateLerp())
					{
						tweenHelper.Value.OnFinished();
						tweensToRemove.Add(tweenHelper.Key);
					}
				}
			}
		}
		
		/// <summary>
		/// Must be called at the end of the frame, to make sure finished Tweens are cleaned up.
		/// </summary>
		private static void CleanUpFinishedTweens()
		{
			if (!_needsCleanUp)
			{
				return;
			}

			if (_needsUpdate)
			{
				foreach (KeyValuePair<Renderer, int> rendererMaterial in _colorTweensToRemoveRenderer_Update)
				{
					_colorTweenHelpersRenderer_Update.Remove(rendererMaterial);
				}
				_colorTweensToRemoveRenderer_Update.Clear();

				foreach (Graphic graphic in _colorTweensToRemoveGraphic_Update)
				{
					_colorTweenHelpersGraphic_Update.Remove(graphic);
				}
				_colorTweensToRemoveGraphic_Update.Clear();

				foreach (Transform transform in _positionTweensToRemove_Update)
				{
					_positionTweenHelpers_Update.Remove(transform);
				}
				_positionTweensToRemove_Update.Clear();
				
				foreach (Transform transform in _rotationTweensToRemove_Update)
				{
					_rotationTweenHelpers_Update.Remove(transform);
				}
				_rotationTweensToRemove_Update.Clear();

				foreach (Transform transform in _scaleTweensToRemove_Update)
				{
					_scaleTweenHelpers_Update.Remove(transform);
				}
				_scaleTweensToRemove_Update.Clear();
			}

			if (_needsLateUpdate)
			{
				foreach (KeyValuePair<Renderer, int> rendererMaterial in _colorTweensToRemoveRenderer_LateUpdate)
				{
					_colorTweenHelpersRenderer_LateUpdate.Remove(rendererMaterial);
				}
				_colorTweensToRemoveRenderer_LateUpdate.Clear();

				foreach (Graphic graphic in _colorTweensToRemoveGraphic_LateUpdate)
				{
					_colorTweenHelpersGraphic_LateUpdate.Remove(graphic);
				}
				_colorTweensToRemoveGraphic_LateUpdate.Clear();

				foreach (Transform transform in _positionTweensToRemove_LateUpdate)
				{
					_positionTweenHelpers_LateUpdate.Remove(transform);
				}
				_positionTweensToRemove_LateUpdate.Clear();

				foreach (Transform transform in _rotationTweensToRemove_LateUpdate)
				{
					_rotationTweenHelpers_LateUpdate.Remove(transform);
				}
				_rotationTweensToRemove_LateUpdate.Clear();

				foreach (Transform transform in _scaleTweensToRemove_LateUpdate)
				{
					_scaleTweenHelpers_LateUpdate.Remove(transform);
				}
				_scaleTweensToRemove_LateUpdate.Clear();
			}

			if (_needsFixedUpdate)
			{
				foreach (KeyValuePair<Renderer, int> rendererMaterial in _colorTweensToRemoveRenderer_FixedUpdate)
				{
					_colorTweenHelpersRenderer_FixedUpdate.Remove(rendererMaterial);
				}
				_colorTweensToRemoveRenderer_FixedUpdate.Clear();

				foreach (Graphic graphic in _colorTweensToRemoveGraphic_FixedUpdate)
				{
					_colorTweenHelpersGraphic_FixedUpdate.Remove(graphic);
				}
				_colorTweensToRemoveGraphic_FixedUpdate.Clear();

				foreach (Transform transform in _positionTweensToRemove_FixedUpdate)
				{
					_positionTweenHelpers_FixedUpdate.Remove(transform);
				}
				_positionTweensToRemove_FixedUpdate.Clear();

				foreach (Transform transform in _rotationTweensToRemove_FixedUpdate)
				{
					_rotationTweenHelpers_FixedUpdate.Remove(transform);
				}
				_rotationTweensToRemove_FixedUpdate.Clear();

				foreach (Transform transform in _scaleTweensToRemove_FixedUpdate)
				{
					_scaleTweenHelpers_FixedUpdate.Remove(transform);
				}
				_scaleTweensToRemove_FixedUpdate.Clear();
			}

			_needsUpdate = _colorTweenHelpersRenderer_Update.Count > 0 || _colorTweenHelpersGraphic_Update.Count > 0 || _positionTweenHelpers_Update.Count > 0 || _rotationTweenHelpers_Update.Count > 0 || _scaleTweenHelpers_Update.Count > 0;
			_needsLateUpdate = _colorTweenHelpersRenderer_LateUpdate.Count > 0 || _colorTweenHelpersGraphic_LateUpdate.Count > 0 || _positionTweenHelpers_LateUpdate.Count > 0 || _rotationTweenHelpers_LateUpdate.Count > 0 || _scaleTweenHelpers_LateUpdate.Count > 0;
			_needsFixedUpdate = _colorTweenHelpersRenderer_FixedUpdate.Count > 0 || _colorTweenHelpersGraphic_FixedUpdate.Count > 0 || _positionTweenHelpers_FixedUpdate.Count > 0 || _rotationTweenHelpers_FixedUpdate.Count > 0 || _scaleTweenHelpers_FixedUpdate.Count > 0;
			_needsCleanUp = false;
		}
		
		/// <summary>
		/// Lerps a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ColorLerp(SpriteRenderer renderer, Color32 startColor, Color32 endColor, float duration, int loopCount = 1, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			ColorLerp(renderer, startColor, endColor, duration, 0, loopCount, useUnscaledTime, updateType, lerpCurve, onTweenEndEvent, onLerpEndEvent);
		}

		/// <summary>
		/// Lerps a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorLerp(SpriteRenderer renderer, Color32 startColor, Color32 endColor, LerpData lerpData)
		{
			ColorLerp(renderer, startColor, endColor, lerpData._lerpDuration, 0, lerpData._loopCount, lerpData._useUnscaledTime, lerpData._updateType, lerpData._lerpCurve, lerpData._onTweenEndEvent, lerpData._onLerpEndEvent);
		}

		/// <summary>
		/// PingPong a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times the PingPong will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ColorPingPong(SpriteRenderer renderer, Color32 startColor, Color32 endColor, float pingDuration, float pongDuration, int loopCount = 1, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			ColorPingPong(renderer, startColor, endColor, pingDuration, pongDuration, 0, loopCount, useUnscaledTime, updateType, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent);
		}

		/// <summary>
		/// PingPong a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorPingPong(SpriteRenderer renderer, Color32 startColor, Color32 endColor, PingPongData pingPongData)
		{
			ColorPingPong(renderer, startColor, endColor, pingPongData._pingDuration, pingPongData._pongDuration, 0, pingPongData._loopCount, pingPongData._useUnscaledTime, pingPongData._updateType, pingPongData._pingCurve, pingPongData._pongCurve, pingPongData._onTweenEndEvent, pingPongData._onPingEndEvent, pingPongData._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="materialIndex_">Index of the material to set the Color32.</param>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ColorLerp(Renderer renderer, Color32 startColor, Color32 endColor, float duration, int materialIndex = 0, int loopCount = 1, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			ColorPingPong(renderer, startColor, endColor, duration, 0.0f, materialIndex, loopCount, useUnscaledTime, updateType, lerpCurve, null, onTweenEndEvent, onLerpEndEvent, null);
		}

		/// <summary>
		/// Lerps a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorLerp(Renderer renderer, Color32 startColor, Color32 endColor, LerpData lerpData, int materialIndex = 0)
		{
			ColorLerp(renderer, startColor, endColor, lerpData._lerpDuration, materialIndex, lerpData._loopCount, lerpData._useUnscaledTime, lerpData._updateType, lerpData._lerpCurve, lerpData._onTweenEndEvent, lerpData._onLerpEndEvent);
		}

		/// <summary>
		/// PingPong a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="materialIndex_">Index of the material to set the Color32.</param>
		/// <param name="loopCount_">Number of times the PingPong will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ColorPingPong(Renderer renderer, Color32 firstColor, Color32 secondColor, float pingDuration, float pongDuration, int materialIndex = 0, int loopCount = 1, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;
			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersRenderer_Update;
					_needsUpdate = true;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_LateUpdate;
					_needsLateUpdate = true;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_FixedUpdate;
					_needsFixedUpdate = true;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}
			
			StopColorTween(renderer, materialIndex, updateType, true);
			
			tweenHelpers.Add(new KeyValuePair<Renderer, int>(renderer, materialIndex), new ColorTweenHelper_Renderer(renderer, firstColor, secondColor, pingDuration, pongDuration, loopCount, useUnscaledTime, updateType == TweenUpdateType.FixedUpdate, pingCurve, pongCurve, materialIndex, onTweenEndEvent, onPingEndEvent, onPongEndEvent));
		}

		/// <summary>
		/// PingPong a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorPingPong(Renderer renderer, Color32 startColor, Color32 endColor, PingPongData pingPongData, int materialIndex = 0)
		{
			ColorPingPong(renderer, startColor, endColor, pingPongData._pingDuration, pingPongData._pongDuration, materialIndex, pingPongData._loopCount, pingPongData._useUnscaledTime, pingPongData._updateType, pingPongData._pingCurve, pingPongData._pongCurve, pingPongData._onTweenEndEvent, pingPongData._onPingEndEvent, pingPongData._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a UI Graphic's Color32 value, with optional looping, callbacks and AnimationCurves. UI Graphics include many elements, such as Image and Text Components.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ColorLerp(Graphic graphic, Color32 startColor, Color32 endColor, float duration, int loopCount = 1, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			ColorPingPong(graphic, startColor, endColor, duration, 0.0f, loopCount, useUnscaledTime, updateType, lerpCurve, null, onTweenEndEvent, onLerpEndEvent, null);
		}

		/// <summary>
		/// Lerps a Graphic's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorLerp(Graphic graphic, Color32 startColor, Color32 endColor, LerpData lerpData)
		{
			ColorLerp(graphic, startColor, endColor, lerpData._lerpDuration, lerpData._loopCount, lerpData._useUnscaledTime, lerpData._updateType, lerpData._lerpCurve, lerpData._onTweenEndEvent, lerpData._onLerpEndEvent);
		}

		/// <summary>
		/// PingPong a UI Graphic's Color32 value, with optional looping, callbacks and AnimationCurves. UI Graphics include many elements, such as Image and Text Components.
		/// </summary>
		/// <param name="loopCount_">Number of times the PingPong will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ColorPingPong(Graphic graphic, Color32 startColor, Color32 endColor, float pingDuration, float pongDuration, int loopCount = 1, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;
			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersGraphic_Update;
					_needsUpdate = true;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_LateUpdate;
					_needsLateUpdate = true;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_FixedUpdate;
					_needsFixedUpdate = true;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}
			
			StopColorTween(graphic, updateType, true);

			tweenHelpers.Add(graphic, new ColorTweenHelper_Graphic(graphic, startColor, endColor, pingDuration, pongDuration, loopCount, useUnscaledTime, updateType == TweenUpdateType.FixedUpdate, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent));
		}

		/// <summary>
		/// PingPong a Graphic's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorPingPong(Graphic graphic, Color32 startColor, Color32 endColor, PingPongData pingPongData)
		{
			ColorPingPong(graphic, startColor, endColor, pingPongData._pingDuration, pingPongData._pongDuration, pingPongData._loopCount, pingPongData._useUnscaledTime, pingPongData._updateType, pingPongData._pingCurve, pingPongData._pongCurve, pingPongData._onTweenEndEvent, pingPongData._onPingEndEvent, pingPongData._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void PositionLerp(Transform transform, Vector3 startPosition, Vector3 endPosition, bool isLocalPosition, float duration, int loopCount = 1, bool muteX = false, bool muteY = false, bool muteZ = false, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			PositionPingPong(transform, startPosition, endPosition, isLocalPosition, duration, 0.0f, loopCount, muteX, muteY, muteZ, useUnscaledTime, updateType, lerpCurve, null, onTweenEndEvent, onLerpEndEvent, null);
		}

		/// <summary>
		/// Lerps a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void PositionLerp(Transform transform, Vector3 startPosition, Vector3 endPosition, bool isLocalPosition, LerpData lerpData, bool muteX = false, bool muteY = false, bool muteZ = false)
		{
			PositionLerp(transform, startPosition, endPosition, isLocalPosition, lerpData._lerpDuration, lerpData._loopCount, muteX, muteY, muteZ, lerpData._useUnscaledTime, lerpData._updateType, lerpData._lerpCurve, lerpData._onTweenEndEvent, lerpData._onLerpEndEvent);
		}

		/// <summary>
		/// PingPongs a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void PositionPingPong(Transform transform, Vector3 startPosition, Vector3 endPosition, bool isLocalPosition, float pingDuration, float pongDuration, int loopCount = 1, bool muteX = false, bool muteY = false, bool muteZ = false, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _positionTweenHelpers_Update;
					_needsUpdate = true;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _positionTweenHelpers_LateUpdate;
					_needsLateUpdate = true;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _positionTweenHelpers_FixedUpdate;
					_needsFixedUpdate = true;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}
			
			StopPositionTween(transform, updateType, true);

			tweenHelpers.Add(transform, new PositionTweenHelper(transform, startPosition, endPosition, isLocalPosition, muteX, muteY, muteZ, pingDuration, pongDuration, loopCount, useUnscaledTime, updateType == TweenUpdateType.FixedUpdate, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent));
		}

		/// <summary>
		/// PingPongs a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void PositionPingPong(Transform transform, Vector3 startPosition, Vector3 endPosition, bool isLocalPosition, PingPongData pingPongData, bool muteX = false, bool muteY = false, bool muteZ = false)
		{
			PositionPingPong(transform, startPosition, endPosition, isLocalPosition, pingPongData._pingDuration, pingPongData._pongDuration, pingPongData._loopCount, muteX, muteY, muteZ, pingPongData._useUnscaledTime, pingPongData._updateType, pingPongData._pingCurve, pingPongData._pongCurve, pingPongData._onTweenEndEvent, pingPongData._onPingEndEvent, pingPongData._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void RotationLerp(Transform transform, Vector3 startRotation, Vector3 endRotation, bool isLocalRotation, float duration, int loopCount = 1, bool muteX = false, bool muteY = false, bool muteZ = false, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			RotationPingPong(transform, startRotation, endRotation, isLocalRotation, duration, 0.0f, loopCount, muteX, muteY, muteZ, useUnscaledTime, updateType, lerpCurve, null, onTweenEndEvent, onLerpEndEvent, null);
		}

		/// <summary>
		/// Lerps a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void RotationLerp(Transform transform, Vector3 startRotation, Vector3 endRotation, bool isLocalRotation, LerpData lerpData, bool muteX = false, bool muteY = false, bool muteZ = false)
		{
			RotationLerp(transform, startRotation, endRotation, isLocalRotation, lerpData._lerpDuration, lerpData._loopCount, muteX, muteY, muteZ, lerpData._useUnscaledTime, lerpData._updateType, lerpData._lerpCurve, lerpData._onTweenEndEvent, lerpData._onLerpEndEvent);
		}

		/// <summary>
		/// PingPongs a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void RotationPingPong(Transform transform, Vector3 startRotation, Vector3 endRotation, bool isLocalRotation, float pingDuration, float pongDuration, int loopCount, bool muteX = false, bool muteY = false, bool muteZ = false, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _rotationTweenHelpers_Update;
					_needsUpdate = true;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _rotationTweenHelpers_LateUpdate;
					_needsLateUpdate = true;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _rotationTweenHelpers_FixedUpdate;
					_needsFixedUpdate = true;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			StopRotationTween(transform, updateType, true);

			tweenHelpers.Add(transform, new RotationTweenHelper(transform, startRotation, endRotation, isLocalRotation, muteX, muteY, muteZ, pingDuration, pongDuration, loopCount, useUnscaledTime, updateType == TweenUpdateType.FixedUpdate, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent));
		}

		/// <summary>
		/// PingPongs a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void RotationPingPong(Transform transform, Vector3 startRotation, Vector3 endRotation, bool isLocalRotation, PingPongData pingPongData, bool muteX = false, bool muteY = false, bool muteZ = false)
		{
			PositionPingPong(transform, startRotation, endRotation, isLocalRotation, pingPongData._pingDuration, pingPongData._pongDuration, pingPongData._loopCount, muteX, muteY, muteZ, pingPongData._useUnscaledTime, pingPongData._updateType, pingPongData._pingCurve, pingPongData._pongCurve, pingPongData._onTweenEndEvent, pingPongData._onPingEndEvent, pingPongData._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ScaleLerp(Transform transform, Vector3 startScale, Vector3 endScale, float duration, int loopCount = 1, bool muteX = false, bool muteY = false, bool muteZ = false, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			ScalePingPong(transform, startScale, endScale, duration, 0.0f, loopCount, muteX, muteY, muteZ, useUnscaledTime, updateType, lerpCurve, null, onTweenEndEvent, onLerpEndEvent, null);
		}

		/// <summary>
		/// Lerps a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ScaleLerp(Transform transform, Vector3 startScale, Vector3 endScale, LerpData lerpData, bool muteX = false, bool muteY = false, bool muteZ = false)
		{
			ScaleLerp(transform, startScale, endScale, lerpData._lerpDuration, lerpData._loopCount, muteX, muteY, muteZ, lerpData._useUnscaledTime, lerpData._updateType, lerpData._lerpCurve, lerpData._onTweenEndEvent, lerpData._onLerpEndEvent);
		}

		/// <summary>
		/// PingPongs a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ScalePingPong(Transform transform, Vector3 startScale, Vector3 endScale, float pingDuration, float pongDuration, int loopCount = 1, bool muteX = false, bool muteY = false, bool muteZ = false, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _scaleTweenHelpers_Update;
					_needsUpdate = true;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _scaleTweenHelpers_LateUpdate;
					_needsLateUpdate = true;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _scaleTweenHelpers_FixedUpdate;
					_needsFixedUpdate = true;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}
			
			StopScaleTween(transform, updateType, true);

			tweenHelpers.Add(transform, new ScaleTweenHelper(transform, startScale, endScale, muteX, muteY, muteZ, pingDuration, pongDuration, loopCount, useUnscaledTime, updateType == TweenUpdateType.FixedUpdate, pingCurve, pongCurve, onTweenEndEvent, onPingEndEvent, onPongEndEvent));
		}

		/// <summary>
		/// PingPongs a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ScalePingPong(Transform transform, Vector3 startScale, Vector3 endScale, PingPongData pingPongData, bool muteX = false, bool muteY = false, bool muteZ = false)
		{
			ScalePingPong(transform, startScale, endScale, pingPongData._pingDuration, pingPongData._pongDuration, pingPongData._loopCount, muteX, muteY, muteZ, pingPongData._useUnscaledTime, pingPongData._updateType, pingPongData._pingCurve, pingPongData._pongCurve, pingPongData._onTweenEndEvent, pingPongData._onPingEndEvent, pingPongData._onPongEndEvent);
		}

		public static void ResumeColorTween(SpriteRenderer spriteRenderer, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			ResumeColorTween(spriteRenderer, updateType);
		}

		public static void ResumeColorTween(Renderer renderer, int materialIndex = 0, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersRenderer_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer, materialIndex);
			if (tweenHelpers.ContainsKey(rendererMaterial))
			{
				tweenHelpers[rendererMaterial].TogglePaused(false);
			}
		}

		public static void ResumeColorTween(Graphic graphic, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersGraphic_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(graphic))
			{
				tweenHelpers[graphic].TogglePaused(false);
			}
		}

		public static void ResumePositionTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _positionTweenHelpers_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _positionTweenHelpers_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _positionTweenHelpers_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].TogglePaused(false);
			}
		}

		public static void ResumeRotationTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _rotationTweenHelpers_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _rotationTweenHelpers_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _rotationTweenHelpers_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].TogglePaused(false);
			}
		}

		public static void ResumeScaleTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _scaleTweenHelpers_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _scaleTweenHelpers_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _scaleTweenHelpers_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].TogglePaused(false);
			}
		}

		public static void PauseColorTween(SpriteRenderer spriteRenderer, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			PauseColorTween(spriteRenderer, updateType);
		}

		public static void PauseColorTween(Renderer renderer, int materialIndex = 0, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersRenderer_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer, materialIndex);
			if (tweenHelpers.ContainsKey(rendererMaterial))
			{
				tweenHelpers[rendererMaterial].TogglePaused(true);
			}
		}

		public static void PauseColorTween(Graphic graphic, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersGraphic_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(graphic))
			{
				tweenHelpers[graphic].TogglePaused(true);
			}
		}

		public static void PausePositionTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _positionTweenHelpers_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _positionTweenHelpers_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _positionTweenHelpers_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].TogglePaused(true);
			}
		}

		public static void PauseRotationTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _rotationTweenHelpers_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _rotationTweenHelpers_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _rotationTweenHelpers_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].TogglePaused(true);
			}
		}

		public static void PauseScaleTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _scaleTweenHelpers_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _scaleTweenHelpers_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _scaleTweenHelpers_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].TogglePaused(true);
			}
		}

		public static void StopColorTween(SpriteRenderer spriteRenderer, TweenUpdateType updateType = TweenUpdateType.Update, bool cleanUpImmediately = false)
		{
			StopColorTween(spriteRenderer, 0, updateType, cleanUpImmediately);
		}

		public static void StopColorTween(Renderer renderer, int materialIndex = 0, TweenUpdateType updateType = TweenUpdateType.Update, bool cleanUpImmediately = false)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;
			List<KeyValuePair<Renderer, int>> tweensToRemove = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersRenderer_Update;
					tweensToRemove = _colorTweensToRemoveRenderer_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_LateUpdate;
					tweensToRemove = _colorTweensToRemoveRenderer_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersRenderer_FixedUpdate;
					tweensToRemove = _colorTweensToRemoveRenderer_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer, materialIndex);
			if (tweenHelpers.ContainsKey(rendererMaterial))
			{
				tweenHelpers[rendererMaterial].OnFinished();
				tweensToRemove.Add(rendererMaterial);

				if (cleanUpImmediately)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopColorTween(Graphic graphic, TweenUpdateType updateType = TweenUpdateType.Update, bool cleanUpImmediately = false)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;
			List<Graphic> tweensToRemove = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _colorTweenHelpersGraphic_Update;
					tweensToRemove = _colorTweensToRemoveGraphic_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_LateUpdate;
					tweensToRemove = _colorTweensToRemoveGraphic_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _colorTweenHelpersGraphic_FixedUpdate;
					tweensToRemove = _colorTweensToRemoveGraphic_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(graphic))
			{
				tweenHelpers[graphic].OnFinished();
				tweensToRemove.Add(graphic);

				if (cleanUpImmediately)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopPositionTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update, bool cleanUpImmediately = false)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			List<Transform> tweensToRemove = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _positionTweenHelpers_Update;
					tweensToRemove = _positionTweensToRemove_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _positionTweenHelpers_LateUpdate;
					tweensToRemove = _positionTweensToRemove_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _positionTweenHelpers_FixedUpdate;
					tweensToRemove = _positionTweensToRemove_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].OnFinished();
				tweensToRemove.Add(transform);

				if (cleanUpImmediately)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopRotationTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update, bool cleanUpImmediately = false)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			List<Transform> tweensToRemove = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _rotationTweenHelpers_Update;
					tweensToRemove = _rotationTweensToRemove_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _rotationTweenHelpers_LateUpdate;
					tweensToRemove = _rotationTweensToRemove_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _rotationTweenHelpers_FixedUpdate;
					tweensToRemove = _rotationTweensToRemove_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].OnFinished();
				tweensToRemove.Add(transform);

				if (cleanUpImmediately)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopScaleTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update, bool cleanUpImmediately = false)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			List<Transform> tweensToRemove = null;

			switch (updateType)
			{
				case TweenUpdateType.Update:
					tweenHelpers = _scaleTweenHelpers_Update;
					tweensToRemove = _scaleTweensToRemove_Update;
					break;
				case TweenUpdateType.LateUpdate:
					tweenHelpers = _scaleTweenHelpers_LateUpdate;
					tweensToRemove = _scaleTweensToRemove_LateUpdate;
					break;
				case TweenUpdateType.FixedUpdate:
					tweenHelpers = _scaleTweenHelpers_FixedUpdate;
					tweensToRemove = _scaleTweensToRemove_FixedUpdate;
					break;
				default:
					DebugHelpers.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform))
			{
				tweenHelpers[transform].OnFinished();
				tweensToRemove.Add(transform);

				if (cleanUpImmediately)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopAllTweens(bool cleanUpImmediately = false)
		{
			if (_needsUpdate)
			{
				foreach (KeyValuePair<Renderer, int> rendererMaterial in _colorTweenHelpersRenderer_Update.Keys)
				{
					StopColorTween(rendererMaterial.Key, rendererMaterial.Value);
				}

				foreach (Graphic graphic in _colorTweenHelpersGraphic_Update.Keys)
				{
					StopColorTween(graphic);
				}

				foreach (Transform transform in _positionTweenHelpers_Update.Keys)
				{
					StopPositionTween(transform);
				}

				foreach (Transform transform in _rotationTweenHelpers_Update.Keys)
				{
					StopRotationTween(transform);
				}

				foreach (Transform transform in _scaleTweenHelpers_Update.Keys)
				{
					StopScaleTween(transform);
				}
			}

			if (_needsLateUpdate)
			{
				foreach (KeyValuePair<Renderer, int> rendererMaterial in _colorTweenHelpersRenderer_LateUpdate.Keys)
				{
					StopColorTween(rendererMaterial.Key, rendererMaterial.Value);
				}

				foreach (Graphic graphic in _colorTweenHelpersGraphic_LateUpdate.Keys)
				{
					StopColorTween(graphic);
				}

				foreach (Transform transform in _positionTweenHelpers_LateUpdate.Keys)
				{
					StopPositionTween(transform);
				}

				foreach (Transform transform in _rotationTweenHelpers_LateUpdate.Keys)
				{
					StopRotationTween(transform);
				}

				foreach (Transform transform in _scaleTweenHelpers_LateUpdate.Keys)
				{
					StopScaleTween(transform);
				}
			}

			if (_needsFixedUpdate)
			{
				foreach (KeyValuePair<Renderer, int> rendererMaterial in _colorTweenHelpersRenderer_FixedUpdate.Keys)
				{
					StopColorTween(rendererMaterial.Key, rendererMaterial.Value);
				}

				foreach (Graphic graphic in _colorTweenHelpersGraphic_FixedUpdate.Keys)
				{
					StopColorTween(graphic);
				}

				foreach (Transform transform in _positionTweenHelpers_FixedUpdate.Keys)
				{
					StopPositionTween(transform);
				}

				foreach (Transform transform in _rotationTweenHelpers_FixedUpdate.Keys)
				{
					StopRotationTween(transform);
				}

				foreach (Transform transform in _scaleTweenHelpers_FixedUpdate.Keys)
				{
					StopScaleTween(transform);
				}
			}

			if (cleanUpImmediately)
			{
				CleanUpFinishedTweens();
			}
		}

		public static bool IsPlayingColorTween(SpriteRenderer spriteRenderer, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			return IsPlayingColorTween(spriteRenderer, 0, updateType);
		}

		public static bool IsPlayingColorTween(Renderer renderer, int materialIndex = 0, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer, materialIndex);

			switch (updateType)
			{
				case TweenUpdateType.Update:
					return _colorTweenHelpersRenderer_Update.ContainsKey(rendererMaterial);
				case TweenUpdateType.LateUpdate:
					return _colorTweenHelpersRenderer_LateUpdate.ContainsKey(rendererMaterial);
				case TweenUpdateType.FixedUpdate:
					return _colorTweenHelpersRenderer_FixedUpdate.ContainsKey(rendererMaterial);
				default:
					DebugHelpers.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingColorTween(Graphic graphic, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			switch (updateType)
			{
				case TweenUpdateType.Update:
					return _colorTweenHelpersGraphic_Update.ContainsKey(graphic);
				case TweenUpdateType.LateUpdate:
					return _colorTweenHelpersGraphic_LateUpdate.ContainsKey(graphic);
				case TweenUpdateType.FixedUpdate:
					return _colorTweenHelpersGraphic_FixedUpdate.ContainsKey(graphic);
				default:
					DebugHelpers.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingPositionTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			switch (updateType)
			{
				case TweenUpdateType.Update:
					return _positionTweenHelpers_Update.ContainsKey(transform);
				case TweenUpdateType.LateUpdate:
					return _positionTweenHelpers_LateUpdate.ContainsKey(transform);
				case TweenUpdateType.FixedUpdate:
					return _positionTweenHelpers_FixedUpdate.ContainsKey(transform);
				default:
					DebugHelpers.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingRotationTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			switch (updateType)
			{
				case TweenUpdateType.Update:
					return _rotationTweenHelpers_Update.ContainsKey(transform);
				case TweenUpdateType.LateUpdate:
					return _rotationTweenHelpers_LateUpdate.ContainsKey(transform);
				case TweenUpdateType.FixedUpdate:
					return _rotationTweenHelpers_FixedUpdate.ContainsKey(transform);
				default:
					DebugHelpers.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingScaleTween(Transform transform, TweenUpdateType updateType = TweenUpdateType.Update)
		{
			switch (updateType)
			{
				case TweenUpdateType.Update:
					return _scaleTweenHelpers_Update.ContainsKey(transform);
				case TweenUpdateType.LateUpdate:
					return _scaleTweenHelpers_LateUpdate.ContainsKey(transform);
				case TweenUpdateType.FixedUpdate:
					return _scaleTweenHelpers_FixedUpdate.ContainsKey(transform);
				default:
					DebugHelpers.PrintSwitchDefaultError();
					return false;
			}
		}
	}

	/// <summary>
	/// Convenience struct for serializing Lerp data on the inspector with a single field.
	/// </summary>
	[System.Serializable]
	public struct LerpData
	{
		[SerializeField]
		public float _lerpDuration;

		[SerializeField]
		public bool _useUnscaledTime;

		[SerializeField]
		public TweenUpdateType _updateType;

		[SerializeField]
		public int _loopCount;

		[SerializeField]
		public AnimationCurve _lerpCurve;

		[SerializeField]
		public UnityEvent _onTweenEndEvent,
						  _onLerpEndEvent;

		public LerpData(float lerpDuration = 0.0f, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, int loopCount = 1,
						 AnimationCurve lerpCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onLerpEndEvent = null)
		{
			_lerpDuration = lerpDuration;
			_useUnscaledTime = useUnscaledTime;
			_updateType = updateType;
			_loopCount = loopCount;
			_lerpCurve = lerpCurve == null ? AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f) : lerpCurve;
			_onTweenEndEvent = onTweenEndEvent;
			_onLerpEndEvent = onLerpEndEvent;
		}
	}

	/// <summary>
	/// Convenience struct for serializing PingPong data on the inspector with a single field.
	/// </summary>
	[System.Serializable]
	public struct PingPongData
	{
		[SerializeField]
		public float _pingDuration,
					 _pongDuration;

		[SerializeField]
		public bool _useUnscaledTime;

		[SerializeField]
		public TweenUpdateType _updateType;

		[SerializeField]
		public int _loopCount;

		[SerializeField]
		public AnimationCurve _pingCurve,
							   _pongCurve;

		[SerializeField]
		public UnityEvent _onTweenEndEvent,
						  _onPingEndEvent,
						  _onPongEndEvent;

		public PingPongData(float pingDuration = 0.0f, float pongDuration = 0.0f, bool useUnscaledTime = false, TweenUpdateType updateType = TweenUpdateType.Update, int loopCount = 1,
							 AnimationCurve pingCurve = null, AnimationCurve pongCurve = null, UnityEvent onTweenEndEvent = null, UnityEvent onPingEndEvent = null, UnityEvent onPongEndEvent = null)
		{
			_pingDuration = pingDuration;
			_pongDuration = pongDuration;
			_useUnscaledTime = useUnscaledTime;
			_updateType = updateType;
			_loopCount = loopCount;
			_pingCurve = pingCurve == null ? AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f) : pingCurve;
			_pongCurve = pongCurve == null ? AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f) : pongCurve;
			_onTweenEndEvent = onTweenEndEvent;
			_onPingEndEvent = onPingEndEvent;
			_onPongEndEvent = onPongEndEvent;
		}
	}
}