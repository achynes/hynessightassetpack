using UnityEngine;
using UnityEditor;
using HynesSight.Tweening;

namespace HynesSightEditor.Tweener
{
	public abstract class TweenerComponentEditor_Base : Editor
	{
		private SerializedProperty _startValue,
								   _endValue,
								   _playOnStart,
								   _useUnscaledTime,
								   _shouldPingPong,
								   _shouldLoopIndefinitely,
								   _pingDuration,
								   _pongDuration,
								   _loopCount,
								   _pingCurve,
								   _pongCurve,
								   _onTweenEndEvent,
								   _onPingEndEvent,
								   _onPongEndEvent;

		protected GUIContent label;

		protected virtual void OnEnable()
		{
			_startValue = serializedObject.FindProperty("_startValue");
			_endValue = serializedObject.FindProperty("_endValue");
			_playOnStart = serializedObject.FindProperty("_playOnStart");
			_useUnscaledTime = serializedObject.FindProperty("_useUnscaledTime");
			_shouldPingPong = serializedObject.FindProperty("_shouldPingPong");
			_shouldLoopIndefinitely = serializedObject.FindProperty("_shouldLoopIndefinitely");
			_pingDuration = serializedObject.FindProperty("_pingDuration");
			_pongDuration = serializedObject.FindProperty("_pongDuration");
			_loopCount = serializedObject.FindProperty("_loopCount");
			_pingCurve = serializedObject.FindProperty("_pingCurve");
			_pongCurve = serializedObject.FindProperty("_pongCurve");
			_onTweenEndEvent = serializedObject.FindProperty("_onTweenEndEvent");
			_onPingEndEvent = serializedObject.FindProperty("_onPingEndEvent");
			_onPongEndEvent = serializedObject.FindProperty("_onPongEndEvent");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			label = new GUIContent("Unscaled Time:");
			EditorGUILayout.PropertyField(_useUnscaledTime, label);

			label = new GUIContent("Play on Start:");
			EditorGUILayout.PropertyField(_playOnStart,	label);

			label = new GUIContent("Ping-Pong:");
			EditorGUILayout.PropertyField(_shouldPingPong, label);

			EditorGUILayout.Space();

			label = new GUIContent(_shouldPingPong.boolValue ? "Ping Value:" : "Start Value:");
			EditorGUILayout.PropertyField(_startValue, label);

			label = new GUIContent(_shouldPingPong.boolValue ? "Pong Value:" : "End Value:");
			EditorGUILayout.PropertyField(_endValue, label);

			EditorGUILayout.Space();

			label = new GUIContent(_shouldPingPong.boolValue ? "Ping Duration:" : "Duration:");
			EditorGUILayout.PropertyField(_pingDuration, label);
			_pingDuration.floatValue = Mathf.Max(0.0f, _pingDuration.floatValue);

			if (_shouldPingPong.boolValue)
			{
				label = new GUIContent("Pong Duration:");
				EditorGUILayout.PropertyField(_pongDuration, label);
				_pongDuration.floatValue = Mathf.Max(0.0f, _pongDuration.floatValue);
			}
			else
			{
				// If not ping-ponging, set the component's ping duration to 0 to reflect that.
				_pongDuration.floatValue = 0.0f;
			}

			EditorGUILayout.Space();

			label = new GUIContent("Loop Indefinitely:");
			EditorGUILayout.PropertyField(_shouldLoopIndefinitely, label);

			if (!_shouldLoopIndefinitely.boolValue)
			{
				label = new GUIContent("Loop Count:");
				EditorGUILayout.PropertyField(_loopCount, label);
				_loopCount.intValue = Mathf.Max(1, _loopCount.intValue);
			}
			else
			{
				_loopCount.intValue = 0;
			}

			EditorGUILayout.Space();

			label = new GUIContent(_shouldPingPong.boolValue ? "Ping Curve:" : "Curve:");
			EditorGUILayout.PropertyField(_pingCurve, label);
			if (_shouldPingPong.boolValue)
			{
				label = new GUIContent("Pong Curve:");
				EditorGUILayout.PropertyField(_pongCurve, label);
			}

			EditorGUILayout.Space();

			label = new GUIContent("OnTweenEndEvent");
			EditorGUILayout.PropertyField(_onTweenEndEvent, label);
			label = new GUIContent(_shouldPingPong.boolValue ? "OnPingEndEvent:" : "OnLerpEndEvent:");
			EditorGUILayout.PropertyField(_onPingEndEvent, label);
			if (_shouldPingPong.boolValue)
			{
				label = new GUIContent("OnPongEndEvent:");
				EditorGUILayout.PropertyField(_onPongEndEvent, label);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}