using UnityEngine;
using UnityEditor;
using HynesSight.Tweening;

namespace HynesSightEditor.Tweener
{
	[CustomEditor(typeof(ScaleTweenerComponent))][CanEditMultipleObjects]
	public sealed class ScaleTweenerComponentEditor : TweenerComponentEditor_Base
	{
		private SerializedProperty _muteX,
								   _muteY,
								   _muteZ;

		protected override void OnEnable()
		{
			base.OnEnable();

			_muteX = serializedObject.FindProperty("_muteX");
			_muteY = serializedObject.FindProperty("_muteY");
			_muteZ = serializedObject.FindProperty("_muteZ");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
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

			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}