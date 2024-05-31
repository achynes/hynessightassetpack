using UnityEngine;
using UnityEditor;
using HynesSight.Tweening;

namespace HynesSightEditor.Tweener
{
	[CustomEditor(typeof(PositionTweenerComponent))][CanEditMultipleObjects]
	public sealed class PositionTweenerComponentEditor : TweenerComponentEditor_Base
	{
		private SerializedProperty _localPosition;
		private SerializedProperty _muteX;
		private SerializedProperty _muteY;
		private SerializedProperty _muteZ;
		private SerializedProperty _startType;
		private SerializedProperty _endType;

		protected override string StartValueName
		{
			get
			{
				switch ((TransformTweenStartType)_startType.enumValueIndex)
				{
					case TransformTweenStartType.Transform:
						return "_startTransform";
					case TransformTweenStartType.Vector:
						return "_startVector";
					case TransformTweenStartType.Current:
					default:
						return null;
				}
			}
		}

		protected override string EndValueName
		{
			get
			{
				switch ((TransformTweenEndType)_endType.enumValueIndex)
				{
					case TransformTweenEndType.Transform:
						return "_endTransform";
					case TransformTweenEndType.Vector:
						return "_endVector";
					default:
						return null;
				}
			}
		}

		protected override void OnEnable()
		{
			// These must be handled before the base.OnEnable, as its SerializedProperty vars rely on them.
			_startType = serializedObject.FindProperty("_startType");
			_endType = serializedObject.FindProperty("_endType");

			base.OnEnable();

			_localPosition = serializedObject.FindProperty("_localPosition");
			_muteX = serializedObject.FindProperty("_muteX");
			_muteY = serializedObject.FindProperty("_muteY");
			_muteZ = serializedObject.FindProperty("_muteZ");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_startType, new GUIContent("Start Type"));
			EditorGUILayout.PropertyField(_endType, new GUIContent("End Type"));

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(170.0f));
			EditorGUILayout.LabelField("Mute:", GUILayout.Width(50.0f));
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("X:", GUILayout.Width(15.0f));
			_muteX.boolValue = EditorGUILayout.Toggle(_muteX.boolValue, GUILayout.Width(25.0f));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Y:", GUILayout.Width(15.0f));
			_muteY.boolValue = EditorGUILayout.Toggle(_muteY.boolValue, GUILayout.Width(25.0f));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Z:", GUILayout.Width(15.0f));
			_muteZ.boolValue = EditorGUILayout.Toggle(_muteZ.boolValue, GUILayout.Width(25.0f));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();

			if ((TransformTweenStartType)_startType.enumValueIndex == TransformTweenStartType.Vector || (TransformTweenEndType)_endType.enumValueIndex == TransformTweenEndType.Vector)
			{
				EditorGUILayout.Space();

				label = new GUIContent("Local Position:");
				EditorGUILayout.PropertyField(_localPosition, label);
			}
			
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}