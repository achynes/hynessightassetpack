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

			protected abstract void ModifyTarget(float lerpValue_);
			protected abstract bool CanTween();

			protected TweenHelper_Base(float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
			{
				_timer = 0.0f;
				_isPinging = true;
				_pingDuration = pingDuration_;
				_pongDuration = pongDuration_;
				_loopCounter = loopCount_;
				_useUnscaledTime = useUnscaledTime_;
				_useFixedDeltaTime = useFixedDeltaTime_;
				_onTweenEndEvent = onTweenEndEvent_;
				_onPingEndEvent = onPingEndEvent_;
				_onPongEndEvent = onPongEndEvent_;

				_pingCurve = (null == pingCurve_) ? _defaultCurve : pingCurve_;
				_pongCurve = (null == pongCurve_) ? _defaultCurve : pongCurve_;
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

			public void TogglePaused(bool paused_)
			{
				_isPaused = paused_;
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

			public ColorTweenHelper_Renderer(Renderer renderer_, Color32 startColor_, Color32 endColor_, float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, int materialIndex_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
				: base(pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, useFixedDeltaTime_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_)
			{
				_renderer = renderer_;
				_startColor = startColor_;
				_endColor = endColor_;
				_materialIndex = materialIndex_;
			}

			protected override bool CanTween()
			{
				return null != _renderer;
			}

			protected override void ModifyTarget(float lerpValue_)
			{
				_renderer.materials[_materialIndex].color = Color32.Lerp(_startColor, _endColor, lerpValue_);
			}
		}

		private sealed class ColorTweenHelper_Graphic : TweenHelper_Base
		{
			private Graphic _graphic;

			private Color32 _startColor,
							_endColor;

			public ColorTweenHelper_Graphic(Graphic graphic_, Color32 startColor_, Color32 endColor_, float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
				: base(pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, useFixedDeltaTime_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_)
			{
				_graphic = graphic_;
				_startColor = startColor_;
				_endColor = endColor_;
			}

			protected override bool CanTween()
			{
				return null != _graphic;
			}

			protected override void ModifyTarget(float lerpValue_)
			{
				_graphic.color = Color32.Lerp(_startColor, _endColor, lerpValue_);
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

			public TransformTweenHelper_Base(Transform transform_, bool muteX_, bool muteY_, bool muteZ_, float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
				: base(pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, useFixedDeltaTime_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_)
			{
				_transform = transform_;

				_muteX = muteX_;
				_muteY = muteY_;
				_muteZ = muteZ_;
			}
		}

		private sealed class PositionTweenHelper : TransformTweenHelper_Base
		{
			private Vector3 _startPosition,
							_endPosition;

			bool _isLocalPosition;

			public PositionTweenHelper(Transform transform_, Vector3 startPosition_, Vector3 endPosition_, bool isLocalPosition_, bool muteX_, bool muteY_, bool muteZ_, float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
				: base(transform_, muteX_, muteY_, muteZ_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, useFixedDeltaTime_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_)
			{
				_startPosition = startPosition_;
				_endPosition = endPosition_;
				_isLocalPosition = isLocalPosition_;
			}

			protected override void ModifyTarget(float lerpValue_)
			{
				Vector3 newPosition = Vector3.Lerp(_startPosition, _endPosition, lerpValue_);

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

			public RotationTweenHelper(Transform transform_, Vector3 startRotation_, Vector3 endRotation_, bool isLocalRotation_, bool muteX_, bool muteY_, bool muteZ_, float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
				: base(transform_, muteX_, muteY_, muteZ_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, useFixedDeltaTime_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_)
			{
				_startRotationX = startRotation_.x;
				_startRotationY = startRotation_.y;
				_startRotationZ = startRotation_.z;
				_endRotationX = endRotation_.x;
				_endRotationY = endRotation_.y;
				_endRotationZ = endRotation_.z;
				_isLocalRotation = isLocalRotation_;
			}
			
			protected override void ModifyTarget(float lerpValue_)
			{
				float xRotation = _muteX ? _transform.rotation.eulerAngles.x : Mathf.Lerp(_startRotationX, _endRotationX, lerpValue_),
					  yRotation = _muteY ? _transform.rotation.eulerAngles.y : Mathf.Lerp(_startRotationY, _endRotationY, lerpValue_),
					  zRotation = _muteZ ? _transform.rotation.eulerAngles.z : Mathf.Lerp(_startRotationZ, _endRotationZ, lerpValue_);

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

			public ScaleTweenHelper(Transform transform_, Vector3 startScale_, Vector3 endScale_, bool muteX_, bool muteY_, bool muteZ_, float pingDuration_, float pongDuration_, int loopCount_, bool useUnscaledTime_, bool useFixedDeltaTime_, AnimationCurve pingCurve_, AnimationCurve pongCurve_, UnityEvent onTweenEndEvent_, UnityEvent onPingEndEvent_, UnityEvent onPongEndEvent_)
				: base(transform_, muteX_, muteY_, muteZ_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, useFixedDeltaTime_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_)
			{
				_startScale = startScale_;
				_endScale = endScale_;
			}
			
			protected override void ModifyTarget(float lerpValue_)
			{
				Vector3 newScale = Vector3.Lerp(_startScale, _endScale, lerpValue_);

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
		private static void UpdateTweenHelpers<T>(Dictionary<T, TweenHelper_Base> tweenHelpers_, List<T> tweensToRemove_)
		{
			if (tweenHelpers_.Count > 0)
			{
				float deltaTime = Time.deltaTime;

				foreach (KeyValuePair<T, TweenHelper_Base> tweenHelper in tweenHelpers_)
				{
					if (!tweensToRemove_.Contains(tweenHelper.Key) && tweenHelper.Value.UpdateLerp())
					{
						tweenHelper.Value.OnFinished();
						tweensToRemove_.Add(tweenHelper.Key);
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
		public static void ColorLerp(SpriteRenderer renderer_, Color32 startColor_, Color32 endColor_, float duration_, int loopCount_ = 1, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			ColorLerp(renderer_, startColor_, endColor_, duration_, 0, loopCount_, useUnscaledTime_, updateType_, lerpCurve_, onTweenEndEvent_, onLerpEndEvent_);
		}

		/// <summary>
		/// Lerps a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorLerp(SpriteRenderer renderer_, Color32 startColor_, Color32 endColor_, LerpData lerpData_)
		{
			ColorLerp(renderer_, startColor_, endColor_, lerpData_._lerpDuration, 0, lerpData_._loopCount, lerpData_._useUnscaledTime, lerpData_._updateType, lerpData_._lerpCurve, lerpData_._onTweenEndEvent, lerpData_._onLerpEndEvent);
		}

		/// <summary>
		/// PingPong a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times the PingPong will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ColorPingPong(SpriteRenderer renderer_, Color32 startColor_, Color32 endColor_, float pingDuration_, float pongDuration_, int loopCount_ = 1, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			ColorPingPong(renderer_, startColor_, endColor_, pingDuration_, pongDuration_, 0, loopCount_, useUnscaledTime_, updateType_, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_);
		}

		/// <summary>
		/// PingPong a SpriteRenderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorPingPong(SpriteRenderer renderer_, Color32 startColor_, Color32 endColor_, PingPongData pingPongData_)
		{
			ColorPingPong(renderer_, startColor_, endColor_, pingPongData_._pingDuration, pingPongData_._pongDuration, 0, pingPongData_._loopCount, pingPongData_._useUnscaledTime, pingPongData_._updateType, pingPongData_._pingCurve, pingPongData_._pongCurve, pingPongData_._onTweenEndEvent, pingPongData_._onPingEndEvent, pingPongData_._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="materialIndex_">Index of the material to set the Color32.</param>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ColorLerp(Renderer renderer_, Color32 startColor_, Color32 endColor_, float duration_, int materialIndex_ = 0, int loopCount_ = 1, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			ColorPingPong(renderer_, startColor_, endColor_, duration_, 0.0f, materialIndex_, loopCount_, useUnscaledTime_, updateType_, lerpCurve_, null, onTweenEndEvent_, onLerpEndEvent_, null);
		}

		/// <summary>
		/// Lerps a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorLerp(Renderer renderer_, Color32 startColor_, Color32 endColor_, LerpData lerpData_, int materialIndex_ = 0)
		{
			ColorLerp(renderer_, startColor_, endColor_, lerpData_._lerpDuration, materialIndex_, lerpData_._loopCount, lerpData_._useUnscaledTime, lerpData_._updateType, lerpData_._lerpCurve, lerpData_._onTweenEndEvent, lerpData_._onLerpEndEvent);
		}

		/// <summary>
		/// PingPong a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="materialIndex_">Index of the material to set the Color32.</param>
		/// <param name="loopCount_">Number of times the PingPong will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ColorPingPong(Renderer renderer_, Color32 firstColor_, Color32 secondColor_, float pingDuration_, float pongDuration_, int materialIndex_ = 0, int loopCount_ = 1, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;
			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}
			
			StopColorTween(renderer_, materialIndex_, updateType_, true);
			
			tweenHelpers.Add(new KeyValuePair<Renderer, int>(renderer_, materialIndex_), new ColorTweenHelper_Renderer(renderer_, firstColor_, secondColor_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, updateType_ == TweenUpdateType.FixedUpdate, pingCurve_, pongCurve_, materialIndex_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_));
		}

		/// <summary>
		/// PingPong a Renderer's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorPingPong(Renderer renderer_, Color32 startColor_, Color32 endColor_, PingPongData pingPongData_, int materialIndex_ = 0)
		{
			ColorPingPong(renderer_, startColor_, endColor_, pingPongData_._pingDuration, pingPongData_._pongDuration, materialIndex_, pingPongData_._loopCount, pingPongData_._useUnscaledTime, pingPongData_._updateType, pingPongData_._pingCurve, pingPongData_._pongCurve, pingPongData_._onTweenEndEvent, pingPongData_._onPingEndEvent, pingPongData_._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a UI Graphic's Color32 value, with optional looping, callbacks and AnimationCurves. UI Graphics include many elements, such as Image and Text Components.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ColorLerp(Graphic graphic_, Color32 startColor_, Color32 endColor_, float duration_, int loopCount_ = 1, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			ColorPingPong(graphic_, startColor_, endColor_, duration_, 0.0f, loopCount_, useUnscaledTime_, updateType_, lerpCurve_, null, onTweenEndEvent_, onLerpEndEvent_, null);
		}

		/// <summary>
		/// Lerps a Graphic's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorLerp(Graphic graphic_, Color32 startColor_, Color32 endColor_, LerpData lerpData_)
		{
			ColorLerp(graphic_, startColor_, endColor_, lerpData_._lerpDuration, lerpData_._loopCount, lerpData_._useUnscaledTime, lerpData_._updateType, lerpData_._lerpCurve, lerpData_._onTweenEndEvent, lerpData_._onLerpEndEvent);
		}

		/// <summary>
		/// PingPong a UI Graphic's Color32 value, with optional looping, callbacks and AnimationCurves. UI Graphics include many elements, such as Image and Text Components.
		/// </summary>
		/// <param name="loopCount_">Number of times the PingPong will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ColorPingPong(Graphic graphic_, Color32 startColor_, Color32 endColor_, float pingDuration_, float pongDuration_, int loopCount_ = 1, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;
			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}
			
			StopColorTween(graphic_, updateType_, true);

			tweenHelpers.Add(graphic_, new ColorTweenHelper_Graphic(graphic_, startColor_, endColor_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, updateType_ == TweenUpdateType.FixedUpdate, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_));
		}

		/// <summary>
		/// PingPong a Graphic's Color32 value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ColorPingPong(Graphic graphic_, Color32 startColor_, Color32 endColor_, PingPongData pingPongData_)
		{
			ColorPingPong(graphic_, startColor_, endColor_, pingPongData_._pingDuration, pingPongData_._pongDuration, pingPongData_._loopCount, pingPongData_._useUnscaledTime, pingPongData_._updateType, pingPongData_._pingCurve, pingPongData_._pongCurve, pingPongData_._onTweenEndEvent, pingPongData_._onPingEndEvent, pingPongData_._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void PositionLerp(Transform transform_, Vector3 startPosition_, Vector3 endPosition_, bool isLocalPosition_, float duration_, int loopCount_ = 1, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			PositionPingPong(transform_, startPosition_, endPosition_, isLocalPosition_, duration_, 0.0f, loopCount_, muteX_, muteY_, muteZ_, useUnscaledTime_, updateType_, lerpCurve_, null, onTweenEndEvent_, onLerpEndEvent_, null);
		}

		/// <summary>
		/// Lerps a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void PositionLerp(Transform transform_, Vector3 startPosition_, Vector3 endPosition_, bool isLocalPosition_, LerpData lerpData_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false)
		{
			PositionLerp(transform_, startPosition_, endPosition_, isLocalPosition_, lerpData_._lerpDuration, lerpData_._loopCount, muteX_, muteY_, muteZ_, lerpData_._useUnscaledTime, lerpData_._updateType, lerpData_._lerpCurve, lerpData_._onTweenEndEvent, lerpData_._onLerpEndEvent);
		}

		/// <summary>
		/// PingPongs a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void PositionPingPong(Transform transform_, Vector3 startPosition_, Vector3 endPosition_, bool isLocalPosition_, float pingDuration_, float pongDuration_, int loopCount_ = 1, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}
			
			StopPositionTween(transform_, updateType_, true);

			tweenHelpers.Add(transform_, new PositionTweenHelper(transform_, startPosition_, endPosition_, isLocalPosition_, muteX_, muteY_, muteZ_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, updateType_ == TweenUpdateType.FixedUpdate, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_));
		}

		/// <summary>
		/// PingPongs a Transform's position value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void PositionPingPong(Transform transform_, Vector3 startPosition_, Vector3 endPosition_, bool isLocalPosition_, PingPongData pingPongData_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false)
		{
			PositionPingPong(transform_, startPosition_, endPosition_, isLocalPosition_, pingPongData_._pingDuration, pingPongData_._pongDuration, pingPongData_._loopCount, muteX_, muteY_, muteZ_, pingPongData_._useUnscaledTime, pingPongData_._updateType, pingPongData_._pingCurve, pingPongData_._pongCurve, pingPongData_._onTweenEndEvent, pingPongData_._onPingEndEvent, pingPongData_._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void RotationLerp(Transform transform_, Vector3 startRotation_, Vector3 endRotation_, bool isLocalRotation_, float duration_, int loopCount_ = 1, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			RotationPingPong(transform_, startRotation_, endRotation_, isLocalRotation_, duration_, 0.0f, loopCount_, muteX_, muteY_, muteZ_, useUnscaledTime_, updateType_, lerpCurve_, null, onTweenEndEvent_, onLerpEndEvent_, null);
		}

		/// <summary>
		/// Lerps a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void RotationLerp(Transform transform_, Vector3 startRotation_, Vector3 endRotation_, bool isLocalRotation_, LerpData lerpData_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false)
		{
			RotationLerp(transform_, startRotation_, endRotation_, isLocalRotation_, lerpData_._lerpDuration, lerpData_._loopCount, muteX_, muteY_, muteZ_, lerpData_._useUnscaledTime, lerpData_._updateType, lerpData_._lerpCurve, lerpData_._onTweenEndEvent, lerpData_._onLerpEndEvent);
		}

		/// <summary>
		/// PingPongs a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void RotationPingPong(Transform transform_, Vector3 startRotation_, Vector3 endRotation_, bool isLocalRotation_, float pingDuration_, float pongDuration_, int loopCount_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			StopRotationTween(transform_, updateType_, true);

			tweenHelpers.Add(transform_, new RotationTweenHelper(transform_, startRotation_, endRotation_, isLocalRotation_, muteX_, muteY_, muteZ_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, updateType_ == TweenUpdateType.FixedUpdate, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_));
		}

		/// <summary>
		/// PingPongs a Transform's rotation value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void RotationPingPong(Transform transform_, Vector3 startRotation_, Vector3 endRotation_, bool isLocalRotation_, PingPongData pingPongData_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false)
		{
			PositionPingPong(transform_, startRotation_, endRotation_, isLocalRotation_, pingPongData_._pingDuration, pingPongData_._pongDuration, pingPongData_._loopCount, muteX_, muteY_, muteZ_, pingPongData_._useUnscaledTime, pingPongData_._updateType, pingPongData_._pingCurve, pingPongData_._pongCurve, pingPongData_._onTweenEndEvent, pingPongData_._onPingEndEvent, pingPongData_._onPongEndEvent);
		}

		/// <summary>
		/// Lerps a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onLerpEndEvent_">Functions called when the Lerp ends (in each loop of the Tween).</param>
		public static void ScaleLerp(Transform transform_, Vector3 startScale_, Vector3 endScale_, float duration_, int loopCount_ = 1, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			ScalePingPong(transform_, startScale_, endScale_, duration_, 0.0f, loopCount_, muteX_, muteY_, muteZ_, useUnscaledTime_, updateType_, lerpCurve_, null, onTweenEndEvent_, onLerpEndEvent_, null);
		}

		/// <summary>
		/// Lerps a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ScaleLerp(Transform transform_, Vector3 startScale_, Vector3 endScale_, LerpData lerpData_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false)
		{
			ScaleLerp(transform_, startScale_, endScale_, lerpData_._lerpDuration, lerpData_._loopCount, muteX_, muteY_, muteZ_, lerpData_._useUnscaledTime, lerpData_._updateType, lerpData_._lerpCurve, lerpData_._onTweenEndEvent, lerpData_._onLerpEndEvent);
		}

		/// <summary>
		/// PingPongs a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		/// <param name="loopCount_">Number of times to Lerp will execute. Set to 0 or less to loop infinitely (or until stopped manually).</param>
		/// <param name="onTweenEndEvent_">Functions called when the Tween stops (including when stopped manually).</param>
		/// <param name="onPingEndEvent_">Functions called when the Ping half of the Tween ends(in each loop of the Tween).</param>
		/// <param name="onPongEndEvent_">Functions called when the Pong half of the Tween ends (in each loop of the Tween).</param>
		public static void ScalePingPong(Transform transform_, Vector3 startScale_, Vector3 endScale_, float pingDuration_, float pongDuration_, int loopCount_ = 1, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}
			
			StopScaleTween(transform_, updateType_, true);

			tweenHelpers.Add(transform_, new ScaleTweenHelper(transform_, startScale_, endScale_, muteX_, muteY_, muteZ_, pingDuration_, pongDuration_, loopCount_, useUnscaledTime_, updateType_ == TweenUpdateType.FixedUpdate, pingCurve_, pongCurve_, onTweenEndEvent_, onPingEndEvent_, onPongEndEvent_));
		}

		/// <summary>
		/// PingPongs a Transform's scale value, with optional looping, callbacks and AnimationCurves.
		/// </summary>
		public static void ScalePingPong(Transform transform_, Vector3 startScale_, Vector3 endScale_, PingPongData pingPongData_, bool muteX_ = false, bool muteY_ = false, bool muteZ_ = false)
		{
			ScalePingPong(transform_, startScale_, endScale_, pingPongData_._pingDuration, pingPongData_._pongDuration, pingPongData_._loopCount, muteX_, muteY_, muteZ_, pingPongData_._useUnscaledTime, pingPongData_._updateType, pingPongData_._pingCurve, pingPongData_._pongCurve, pingPongData_._onTweenEndEvent, pingPongData_._onPingEndEvent, pingPongData_._onPongEndEvent);
		}

		public static void ResumeColorTween(SpriteRenderer spriteRenderer_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			ResumeColorTween(spriteRenderer_, updateType_);
		}

		public static void ResumeColorTween(Renderer renderer_, int materialIndex_ = 0, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer_, materialIndex_);
			if (tweenHelpers.ContainsKey(rendererMaterial))
			{
				tweenHelpers[rendererMaterial].TogglePaused(false);
			}
		}

		public static void ResumeColorTween(Graphic graphic_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(graphic_))
			{
				tweenHelpers[graphic_].TogglePaused(false);
			}
		}

		public static void ResumePositionTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].TogglePaused(false);
			}
		}

		public static void ResumeRotationTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].TogglePaused(false);
			}
		}

		public static void ResumeScaleTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].TogglePaused(false);
			}
		}

		public static void PauseColorTween(SpriteRenderer spriteRenderer_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			PauseColorTween(spriteRenderer_, updateType_);
		}

		public static void PauseColorTween(Renderer renderer_, int materialIndex_ = 0, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer_, materialIndex_);
			if (tweenHelpers.ContainsKey(rendererMaterial))
			{
				tweenHelpers[rendererMaterial].TogglePaused(true);
			}
		}

		public static void PauseColorTween(Graphic graphic_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(graphic_))
			{
				tweenHelpers[graphic_].TogglePaused(true);
			}
		}

		public static void PausePositionTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].TogglePaused(true);
			}
		}

		public static void PauseRotationTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].TogglePaused(true);
			}
		}

		public static void PauseScaleTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].TogglePaused(true);
			}
		}

		public static void StopColorTween(SpriteRenderer spriteRenderer_, TweenUpdateType updateType_ = TweenUpdateType.Update, bool cleanUpImmediately_ = false)
		{
			StopColorTween(spriteRenderer_, 0, updateType_, cleanUpImmediately_);
		}

		public static void StopColorTween(Renderer renderer_, int materialIndex_ = 0, TweenUpdateType updateType_ = TweenUpdateType.Update, bool cleanUpImmediately_ = false)
		{
			Dictionary<KeyValuePair<Renderer, int>, TweenHelper_Base> tweenHelpers = null;
			List<KeyValuePair<Renderer, int>> tweensToRemove = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer_, materialIndex_);
			if (tweenHelpers.ContainsKey(rendererMaterial))
			{
				tweenHelpers[rendererMaterial].OnFinished();
				tweensToRemove.Add(rendererMaterial);

				if (cleanUpImmediately_)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopColorTween(Graphic graphic_, TweenUpdateType updateType_ = TweenUpdateType.Update, bool cleanUpImmediately_ = false)
		{
			Dictionary<Graphic, TweenHelper_Base> tweenHelpers = null;
			List<Graphic> tweensToRemove = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(graphic_))
			{
				tweenHelpers[graphic_].OnFinished();
				tweensToRemove.Add(graphic_);

				if (cleanUpImmediately_)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopPositionTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update, bool cleanUpImmediately_ = false)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			List<Transform> tweensToRemove = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].OnFinished();
				tweensToRemove.Add(transform_);

				if (cleanUpImmediately_)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopRotationTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update, bool cleanUpImmediately_ = false)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			List<Transform> tweensToRemove = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].OnFinished();
				tweensToRemove.Add(transform_);

				if (cleanUpImmediately_)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopScaleTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update, bool cleanUpImmediately_ = false)
		{
			Dictionary<Transform, TweenHelper_Base> tweenHelpers = null;
			List<Transform> tweensToRemove = null;

			switch (updateType_)
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
					DebugUtility.PrintSwitchDefaultError();
					break;
			}

			if (tweenHelpers.ContainsKey(transform_))
			{
				tweenHelpers[transform_].OnFinished();
				tweensToRemove.Add(transform_);

				if (cleanUpImmediately_)
				{
					CleanUpFinishedTweens();
				}
			}
		}

		public static void StopAllTweens(bool cleanUpImmediately_ = false)
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

			if (cleanUpImmediately_)
			{
				CleanUpFinishedTweens();
			}
		}

		public static bool IsPlayingColorTween(SpriteRenderer spriteRenderer_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			return IsPlayingColorTween(spriteRenderer_, 0, updateType_);
		}

		public static bool IsPlayingColorTween(Renderer renderer_, int materialIndex_ = 0, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			KeyValuePair<Renderer, int> rendererMaterial = new KeyValuePair<Renderer, int>(renderer_, materialIndex_);

			switch (updateType_)
			{
				case TweenUpdateType.Update:
					return _colorTweenHelpersRenderer_Update.ContainsKey(rendererMaterial);
				case TweenUpdateType.LateUpdate:
					return _colorTweenHelpersRenderer_LateUpdate.ContainsKey(rendererMaterial);
				case TweenUpdateType.FixedUpdate:
					return _colorTweenHelpersRenderer_FixedUpdate.ContainsKey(rendererMaterial);
				default:
					DebugUtility.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingColorTween(Graphic graphic_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			switch (updateType_)
			{
				case TweenUpdateType.Update:
					return _colorTweenHelpersGraphic_Update.ContainsKey(graphic_);
				case TweenUpdateType.LateUpdate:
					return _colorTweenHelpersGraphic_LateUpdate.ContainsKey(graphic_);
				case TweenUpdateType.FixedUpdate:
					return _colorTweenHelpersGraphic_FixedUpdate.ContainsKey(graphic_);
				default:
					DebugUtility.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingPositionTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			switch (updateType_)
			{
				case TweenUpdateType.Update:
					return _positionTweenHelpers_Update.ContainsKey(transform_);
				case TweenUpdateType.LateUpdate:
					return _positionTweenHelpers_LateUpdate.ContainsKey(transform_);
				case TweenUpdateType.FixedUpdate:
					return _positionTweenHelpers_FixedUpdate.ContainsKey(transform_);
				default:
					DebugUtility.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingRotationTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			switch (updateType_)
			{
				case TweenUpdateType.Update:
					return _rotationTweenHelpers_Update.ContainsKey(transform_);
				case TweenUpdateType.LateUpdate:
					return _rotationTweenHelpers_LateUpdate.ContainsKey(transform_);
				case TweenUpdateType.FixedUpdate:
					return _rotationTweenHelpers_FixedUpdate.ContainsKey(transform_);
				default:
					DebugUtility.PrintSwitchDefaultError();
					return false;
			}
		}

		public static bool IsPlayingScaleTween(Transform transform_, TweenUpdateType updateType_ = TweenUpdateType.Update)
		{
			switch (updateType_)
			{
				case TweenUpdateType.Update:
					return _scaleTweenHelpers_Update.ContainsKey(transform_);
				case TweenUpdateType.LateUpdate:
					return _scaleTweenHelpers_LateUpdate.ContainsKey(transform_);
				case TweenUpdateType.FixedUpdate:
					return _scaleTweenHelpers_FixedUpdate.ContainsKey(transform_);
				default:
					DebugUtility.PrintSwitchDefaultError();
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

		public LerpData(float lerpDuration_ = 0.0f, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, int loopCount_ = 1,
						 AnimationCurve lerpCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onLerpEndEvent_ = null)
		{
			_lerpDuration = lerpDuration_;
			_useUnscaledTime = useUnscaledTime_;
			_updateType = updateType_;
			_loopCount = loopCount_;
			_lerpCurve = lerpCurve_ == null ? AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f) : lerpCurve_;
			_onTweenEndEvent = onTweenEndEvent_;
			_onLerpEndEvent = onLerpEndEvent_;
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

		public PingPongData(float pingDuration_ = 0.0f, float pongDuration_ = 0.0f, bool useUnscaledTime_ = false, TweenUpdateType updateType_ = TweenUpdateType.Update, int loopCount_ = 1,
							 AnimationCurve pingCurve_ = null, AnimationCurve pongCurve_ = null, UnityEvent onTweenEndEvent_ = null, UnityEvent onPingEndEvent_ = null, UnityEvent onPongEndEvent_ = null)
		{
			_pingDuration = pingDuration_;
			_pongDuration = pongDuration_;
			_useUnscaledTime = useUnscaledTime_;
			_updateType = updateType_;
			_loopCount = loopCount_;
			_pingCurve = pingCurve_ == null ? AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f) : pingCurve_;
			_pongCurve = pongCurve_ == null ? AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f) : pongCurve_;
			_onTweenEndEvent = onTweenEndEvent_;
			_onPingEndEvent = onPingEndEvent_;
			_onPongEndEvent = onPongEndEvent_;
		}
	}
}