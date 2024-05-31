using UnityEngine;
using UnityEditor;
using HynesSight.Attributes;

namespace HynesSightEditor.Attributes
{
	[CustomPropertyDrawer(typeof(DisplayNameAttribute))]
	public class DisplayNamePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position_, SerializedProperty property_, GUIContent label_)
		{
			DisplayNameAttribute displayNameAttribute = (DisplayNameAttribute)attribute;
			label_.text = displayNameAttribute._nameToDisplay;
			
			EditorGUI.PropertyField(position_, property_, label_, true);
		}
	}
}