using System.Collections.Generic;

public static class DictionaryExtensions
{
	// Returns the first Key found that points to the given value.
	public static TKey KeyOf<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
		{
			if (EqualityComparer<TValue>.Default.Equals(keyValuePair.Value, value))
				return keyValuePair.Key;
		}

		return default(TKey);
	}
}
