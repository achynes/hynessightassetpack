using UnityEngine;
using UnityEditor;
using HynesSight.Tweening;

namespace HynesSightEditor.Tweener
{
	[CustomEditor(typeof(RendererColorTweenerComponent))][CanEditMultipleObjects]
	public sealed class RendererColorTweenerComponentEditor : TweenerComponentEditor_Base
	{
		private SerializedProperty _renderer,
								   _materialIndex;

		protected override void OnEnable()
		{
			base.OnEnable();

			_renderer = serializedObject.FindProperty("_renderer");
			_materialIndex = serializedObject.FindProperty("_materialIndex");

			if (_renderer.objectReferenceValue == null)
			{
				Component targetComp = (Component)target;

				serializedObject.Update();
				_renderer.objectReferenceValue = targetComp == null ? null : targetComp.GetComponent<Renderer>();
				serializedObject.ApplyModifiedProperties();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			label = new GUIContent("Renderer:");
			EditorGUILayout.PropertyField(_renderer, label);

			label = new GUIContent("Material Index:");
			EditorGUILayout.PropertyField(_materialIndex, label);
			_materialIndex.intValue = Mathf.Max(0, _materialIndex.intValue);

			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}