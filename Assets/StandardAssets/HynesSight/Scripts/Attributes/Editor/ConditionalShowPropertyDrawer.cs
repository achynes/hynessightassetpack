using UnityEngine;
using UnityEditor;
using HynesSight.Attributes;

namespace HynesSightEditor.Attributes
{
	[CustomPropertyDrawer(typeof(ConditionalShowAttribute))]
	public class ConditionalShowPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ConditionalShowAttribute showAttribute = (ConditionalShowAttribute)attribute;
			bool shouldShow = GetConditionalShowResult(showAttribute, property);
			
			bool wasEnabled = GUI.enabled;
			GUI.enabled = shouldShow;

			if (shouldShow)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}

			GUI.enabled = wasEnabled;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ConditionalShowAttribute showAttribute = (ConditionalShowAttribute)attribute;
			bool shouldShow = GetConditionalShowResult(showAttribute, property);

			if (shouldShow)
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				return -EditorGUIUtility.standardVerticalSpacing;
			}
		}

		private bool GetConditionalShowResult(ConditionalShowAttribute showAttribute, SerializedProperty property)
		{
			ConditionType condition = showAttribute._condition;

			// Get the property path of the property we want to apply the attribute to.
			// Then change it to the path to of fields being compared.
			string propertyPath = property.propertyPath,
				   fieldPath = propertyPath.Replace(property.name, showAttribute._field),
				   relativeFieldPath = propertyPath.Replace(property.name, showAttribute._relativeField);

			SerializedProperty field = property.serializedObject.FindProperty(fieldPath),
							   relativeField = property.serializedObject.FindProperty(relativeFieldPath);

			object relativeValue = showAttribute._relativeValue;

			switch (field.propertyType)
			{
				case SerializedPropertyType.Integer:
					switch (condition)
					{
						case ConditionType.EqualTo:
							if (null != relativeField)
							{
								return field.intValue == relativeField.intValue;
							}
							else
							{
								return field.intValue == (int)relativeValue;
							}

						case ConditionType.NotEqualTo:
							if (null != relativeField)
							{
								return field.intValue != relativeField.intValue;
							}
							else
							{
								return field.intValue != (int)relativeValue;
							}

						case ConditionType.GreaterThan:
							if (null != relativeField)
							{
								return field.intValue > relativeField.intValue;
							}
							else
							{
								return field.intValue > (int)relativeValue;
							}

						case ConditionType.LessThan:
							if (null != relativeField)
							{
								return field.intValue < relativeField.intValue;
							}
							else
							{
								return field.intValue < (int)relativeValue;
							}

						case ConditionType.GreaterThanOrEqualTo:
							if (null != relativeField)
							{
								return field.intValue >= relativeField.intValue;
							}
							else
							{
								return field.intValue >= (int)relativeValue;
							}

						case ConditionType.LessThanOrEqualTo:
							if (null != relativeField)
							{
								return field.intValue <= relativeField.intValue;
							}
							else
							{
								return field.intValue <= (int)relativeValue;
							}

						default:
							Debug.LogError("Should not hit this default case.");
							break;
					}
					break;
				case SerializedPropertyType.Float:
					switch (condition)
					{
						case ConditionType.EqualTo:
							if (null != relativeField)
							{
								return field.floatValue == relativeField.floatValue;
							}
							else
							{
								return field.floatValue == (float)relativeValue;
							}

						case ConditionType.NotEqualTo:
							if (null != relativeField)
							{
								return field.floatValue != relativeField.floatValue;
							}
							else
							{
								return field.floatValue != (float)relativeValue;
							}

						case ConditionType.GreaterThan:
							if (null != relativeField)
							{
								return field.floatValue > relativeField.floatValue;
							}
							else
							{
								return field.floatValue > (float)relativeValue;
							}

						case ConditionType.LessThan:
							if (null != relativeField)
							{
								return field.floatValue < relativeField.floatValue;
							}
							else
							{
								return field.floatValue < (float)relativeValue;
							}

						case ConditionType.GreaterThanOrEqualTo:
							if (null != relativeField)
							{
								return field.floatValue >= relativeField.floatValue;
							}
							else
							{
								return field.floatValue >= (float)relativeValue;
							}

						case ConditionType.LessThanOrEqualTo:
							if (null != relativeField)
							{
								return field.floatValue <= relativeField.floatValue;
							}
							else
							{
								return field.floatValue <= (float)relativeValue;
							}

						default:
							Debug.LogError("Should not hit this default case.");
							break;
					}
					break;
				case SerializedPropertyType.Enum:
					switch (condition)
					{
						case ConditionType.EqualTo:
							if (null != relativeField)
							{
								return field.enumValueIndex == relativeField.enumValueIndex;
							}
							else
							{
								return field.enumValueIndex == (int)relativeValue;
							}

						case ConditionType.NotEqualTo:
							if (null != relativeField)
							{
								return field.enumValueIndex != relativeField.enumValueIndex;
							}
							else
							{
								return field.enumValueIndex != (int)relativeValue;
							}

						case ConditionType.GreaterThan:
							if (null != relativeField)
							{
								return field.enumValueIndex > relativeField.enumValueIndex;
							}
							else
							{
								return field.enumValueIndex > (int)relativeValue;
							}

						case ConditionType.LessThan:
							if (null != relativeField)
							{
								return field.enumValueIndex < relativeField.enumValueIndex;
							}
							else
							{
								return field.enumValueIndex < (int)relativeValue;
							}

						case ConditionType.GreaterThanOrEqualTo:
							if (null != relativeField)
							{
								return field.enumValueIndex >= relativeField.enumValueIndex;
							}
							else
							{
								return field.enumValueIndex >= (int)relativeValue;
							}

						case ConditionType.LessThanOrEqualTo:
							if (null != relativeField)
							{
								return field.enumValueIndex <= relativeField.enumValueIndex;
							}
							else
							{
								return field.enumValueIndex <= (int)relativeValue;
							}

						default:
							Debug.LogError("Should not hit this default case.");
							break;
					}
					break;
				case SerializedPropertyType.Boolean:
					switch (condition)
					{
						case ConditionType.EqualTo:
							if (null != relativeField)
							{
								return field.boolValue == relativeField.boolValue;
							}
							else
							{
								return field.boolValue == (bool)relativeValue;
							}

						case ConditionType.NotEqualTo:
							if (null != relativeField)
							{
								return field.boolValue != relativeField.boolValue;
							}
							else
							{
								return field.boolValue != (bool)relativeValue;
							}
							
						default:
							Debug.LogError("Trying to compare bool values with wrong operator; defauting to ==.");
							if (null != relativeField)
							{
								return field.boolValue == relativeField.boolValue;
							}
							else
							{
								return field.boolValue == (bool)relativeValue;
							}
					}
			}

			Debug.LogError("No case defined for this field type, defaulting to true.");
			return true;
		}
	}
}