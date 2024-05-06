using UnityEngine;
using UnityEditor;
using HynesSight.Attributes;

namespace HynesSightEditor.Attributes
{
	[CustomPropertyDrawer(typeof(DisplayNameAttribute))]
	public class DisplayNamePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)attribute;
			label.text = displayNameAttribute._nameToDisplay;
			
			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}