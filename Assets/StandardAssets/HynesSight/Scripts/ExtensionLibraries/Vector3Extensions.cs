using UnityEngine;

public static class Vector3Extensions
{
	public static Vector3 DivideElementWise(this Vector3 vectorNumerator, Vector3 vectorDivisor)
	{
		return new Vector3(vectorNumerator.x / vectorDivisor.x, vectorNumerator.y / vectorDivisor.y, vectorNumerator.z / vectorDivisor.z);
	}
}

