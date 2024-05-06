using UnityEngine;

public static class Vector2Extensions
{
	public static Vector2 Abs(this Vector2 vector)
	{
		return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}

	public static Vector2 DivideElementWise(this Vector2 vectorNumerator, Vector2 vectorDivisor)
	{
		return new Vector2(vectorNumerator.x / vectorDivisor.x, vectorNumerator.y / vectorDivisor.y);
	}
}
