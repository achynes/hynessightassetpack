using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static int AddUnique<T>(this List<T> list, T newEntry)
	{
		if (newEntry == null)
			return -1;

		int index = list.IndexOf(newEntry);
		if (index >= 0)
			return index;
			
		list.Add(newEntry);
		return list.Count - 1;
	}
}
