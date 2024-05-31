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

		public ConditionalShowAttribute(string boolField_, bool relativeBoolValue_)
		{
#if UNITY_EDITOR
			_field = boolField_;
			_relativeValue = relativeBoolValue_;
			_condition = ConditionType.EqualTo;
#endif
		}

		public ConditionalShowAttribute(string field_, object relativeValue_, ConditionType condition_)
		{
#if UNITY_EDITOR
			_field = field_;
			_relativeValue = relativeValue_;
			_condition = condition_;
#endif
		}

		public ConditionalShowAttribute(string field_, string relativeField_, ConditionType condition_)
		{
#if UNITY_EDITOR
			_field = field_;
			_relativeField = relativeField_;
			_condition = condition_;
#endif
		}
	}
}