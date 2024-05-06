using UnityEngine;
using System;

namespace HynesSight.Attributes
{
	public enum ConditionType
	{
		EqualTo,
		NotEqualTo,
		GreaterThan,
		LessThan,
		GreaterThanOrEqualTo,
		LessThanOrEqualTo
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
	   AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public sealed class ConditionalShowAttribute : PropertyAttribute
	{
#if UNITY_EDITOR
		public string _field = "",
					  _relativeField = "";

		public object _relativeValue = null;

		public ConditionType _condition;
#endif

		public ConditionalShowAttribute(string boolField, bool relativeBoolValue)
		{
#if UNITY_EDITOR
			_field = boolField;
			_relativeValue = relativeBoolValue;
			_condition = ConditionType.EqualTo;
#endif
		}

		public ConditionalShowAttribute(string field, object relativeValue, ConditionType condition)
		{
#if UNITY_EDITOR
			_field = field;
			_relativeValue = relativeValue;
			_condition = condition;
#endif
		}

		public ConditionalShowAttribute(string field, string relativeField, ConditionType condition)
		{
#if UNITY_EDITOR
			_field = field;
			_relativeField = relativeField;
			_condition = condition;
#endif
		}
	}
}