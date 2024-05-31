using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using HynesSight.Tweening;

namespace HynesSightEditor.Tweener
{
	[CustomEditor(typeof(UIGraphicColorTweenerComponent))][CanEditMultipleObjects]
	public sealed class UIGraphicColorTweenerComponentEditor : TweenerComponentEditor_Base
	{
		private SerializedProperty _graphic;

		protected override void OnEnable()
		{
			base.OnEnable();

			_graphic = serializedObject.FindProperty("_graphic");

			if (_graphic.objectReferenceValue == null)
			{
				Component targetComp = (Component)target;

				serializedObject.Update();
				_graphic.objectReferenceValue = targetComp == null ? null : targetComp.GetComponent<Graphic>();
				serializedObject.ApplyModifiedProperties();
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			label = new GUIContent("Graphic:");
			EditorGUILayout.PropertyField(_graphic, label);

			EditorGUILayout.Space();

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}