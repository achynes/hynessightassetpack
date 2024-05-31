using UnityEngine;
using UnityEditor;
using HynesSight.Tweening;

namespace HynesSightEditor.Tweener
{
	[CustomEditor(typeof(SpriteColorTweenerComponent))][CanEditMultipleObjects]
	public sealed class SpriteColorTweenerComponentEditor : TweenerComponentEditor_Base
	{
		private SerializedProperty _spriteRenderer;

		protected override void OnEnable()
		{
			base.OnEnable();

			_spriteRenderer = serializedObject.FindProperty("_spriteRenderer");

			if (_spriteRenderer.objectReferenceValue == null)
			{
				Component targetComp = (Component)target;

				serializedObject.Update();
				_spriteRenderer.objectReferenceValue = targetComp == null ? null : targetComp.GetComponent<SpriteRenderer>();
				serializedObject.ApplyModifiedProperties();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			label = new GUIContent("Sprite Renderer:");
			EditorGUILayout.PropertyField(_spriteRenderer, label);
			
			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}