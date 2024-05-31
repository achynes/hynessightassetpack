using System;
using System.Collections;
using System.Collections.Generic;

public static class EnumerableExtensions
{
	public static string EnumerableString(this IEnumerable enumerable)
	{
		string result = "{";

		foreach (object item in enumerable)
		{
			result += item.ToString();
			result += ", ";
		}

		// Remove trailing comma
		if (result.Length > 2)
			result = result.Remove(result.Length - 2);

		result += "}";

		return result;
	}

	public static string EnumerableString(this IEnumerable<float> enumerable, string format)
	{
		string result = "{";

		foreach (float item in enumerable)
		{
			result += item.ToString(format);
			result += ", ";
		}

		// Remove trailing comma
		if (result.Length > 2)
			result = result.Remove(result.Length - 2);

		result += "}";

		return result;
	}

	public static string EnumerableString(this IEnumerable<double> enumerable, string format)
	{
		string result = "{";

		foreach (float item in enumerable)
		{
			result += item.ToString(format);
			result += ", ";
		}

		// Remove trailing comma
		if (result.Length > 2)
			result = result.Remove(result.Length - 2);

		result += "}";

		return result;
	}

	public static string EnumerableString(this IEnumerable<IConvertible> enumerable, IFormatProvider formatProvider)
	{
		string result = "{";

		foreach (float item in enumerable)
		{
			result += item.ToString(formatProvider);
			result += ", ";
		}

		// Remove trailing comma
		if (result.Length > 2)
			result = result.Remove(result.Length - 2);

		result += "}";

		return result;
	}

	public static string EnumerableString(this IEnumerable<IFormattable> enumerable, string format, IFormatProvider formatProvider)
	{
		string result = "{";
		
		foreach (float item in enumerable)
		{
			result += item.ToString(format, formatProvider);
			result += ", ";
		}

		// Remove trailing comma
		if (result.Length > 2)
			result = result.Remove(result.Length - 2);

		result += "}";

		return result;
	}
}
