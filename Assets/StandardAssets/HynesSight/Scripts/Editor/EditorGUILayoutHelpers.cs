using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HynesSightEditor
{
	public static class EditorGUILayoutHelpers
	{
		public static void RowOfToggles(string label0, ref bool toggle0, string label1, ref bool toggle1)
		{
			string[] labels = new string[] { label0, label1 };
			bool[] toggles = new bool[] { toggle0, toggle1 };

			RowOfTogglesInternal(labels, ref toggles);

			toggle0 = toggles[0];
			toggle1 = toggles[1];
		}

		public static void RowOfToggles(string label0, ref bool toggle0, string label1, ref bool toggle1, string label2, ref bool toggle2)
		{
			string[] labels = new string[] { label0, label1, label2 };
			bool[] toggles = new bool[] { toggle0, toggle1, toggle2 };

			RowOfTogglesInternal(labels, ref toggles);

			toggle0 = toggles[0];
			toggle1 = toggles[1];
			toggle2 = toggles[2];
		}

		public static void RowOfToggles(string label0, ref bool toggle0, string label1, ref bool toggle1, string label2, ref bool toggle2, string label3, ref bool toggle3)
		{
			string[] labels = new string[] { label0, label1, label2, label3 };
			bool[] toggles = new bool[] { toggle0, toggle1, toggle2, toggle3 };

			RowOfTogglesInternal(labels, ref toggles);

			toggle0 = toggles[0];
			toggle1 = toggles[1];
			toggle2 = toggles[2];
			toggle3 = toggles[3];
		}

		static void RowOfTogglesInternal(string[] labels, ref bool[] toggles)
		{
			EditorGUILayout.BeginHorizontal();

			for (int i = 0; i < toggles.Length; ++i)
			{
				string label = labels.Length > i ? labels[i] : string.Empty;
				toggles[i] = EditorGUILayout.Toggle(label, toggles[i]);

				if (i < toggles.Length - 1)
					EditorGUILayout.Space();
			}

			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();
		}

		public static void RowOfIntFields(bool delayed, string label0, ref int int0, string label1, ref int int1)
		{
			string[] labels = new string[] { label0, label1 };
			int[] ints = new int[] { int0, int1 };

			RowOfIntFieldsInternal(delayed, labels, ref ints);

			int0 = ints[0];
			int1 = ints[1];
		}

		public static void RowOfIntFields(bool delayed, string label0, ref int int0, string label1, ref int int1, string label2, ref int int2)
		{
			string[] labels = new string[] { label0, label1, label2 };
			int[] ints = new int[] { int0, int1, int2 };

			RowOfIntFieldsInternal(delayed, labels, ref ints);

			int0 = ints[0];
			int1 = ints[1];
			int2 = ints[2];
		}

		public static void RowOfIntFields(bool delayed, string label0, ref int int0, string label1, ref int int1, string label2, ref int int2, string label3, ref int int3)
		{
			string[] labels = new string[] { label0, label1, label2, label3 };
			int[] ints = new int[] { int0, int1, int2, int3 };

			RowOfIntFieldsInternal(delayed, labels, ref ints);

			int0 = ints[0];
			int1 = ints[1];
			int2 = ints[2];
			int3 = ints[3];
		}

		static void RowOfIntFieldsInternal(bool delayed, string[] labels, ref int[] ints)
		{
			EditorGUILayout.BeginHorizontal();

			for (int i = 0; i < ints.Length; ++i)
			{
				string label = labels.Length > i ? labels[i] : string.Empty;
				
				if (delayed)
					ints[i]	= EditorGUILayout.DelayedIntField(label, ints[i]);
				else
					ints[i]	= EditorGUILayout.IntField(label, ints[i]);

				if (i < ints.Length - 1)
					EditorGUILayout.Space();
			}

			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();
		}
	}
}
