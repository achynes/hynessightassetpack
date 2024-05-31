using UnityEngine;
using System;

namespace HynesSight.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
	   AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
	public class DisplayNameAttribute : PropertyAttribute
	{
#if UNITY_EDITOR
		public string _nameToDisplay;
#endif

		public DisplayNameAttribute(string nameToDisplay_)
		{
#if UNITY_EDITOR
			_nameToDisplay = nameToDisplay_;
#endif
		}
	}
}