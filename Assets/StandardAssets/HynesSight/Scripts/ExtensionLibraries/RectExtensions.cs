using UnityEngine;

public static class RectExtensions
{
	// Moves the given Rect from local space of one Transform to another and returns the result.
	public static Rect InterTransformRect(this Rect rect, Transform originalTransform, Transform targetTransform)
	{
		Vector3 topLeft;
		Vector3 bottomLeft;
		Vector3 topRight;

		if (originalTransform != null)
		{
			topLeft = originalTransform.TransformPoint(new Vector2(rect.xMin, rect.yMin));
			bottomLeft = originalTransform.TransformPoint(new Vector2(rect.xMin, rect.yMax));
			topRight = originalTransform.TransformPoint(new Vector2(rect.xMax, rect.yMin));
		}
		else
		{
			topLeft = new Vector2(rect.xMin, rect.yMin);
			bottomLeft = new Vector2(rect.xMin, rect.yMax);
			topRight = new Vector2(rect.xMax, rect.yMin);
		}

		if (targetTransform != null)
		{
			topLeft = targetTransform.InverseTransformPoint(topLeft);
			bottomLeft = targetTransform.InverseTransformPoint(bottomLeft);
			topRight = targetTransform.InverseTransformPoint(topRight);
		}

		// Note that the height is calculated opposite to what you'd expect, because Unity Rects are measured from the top-left corner downwards, unlike other systems.
		return new Rect(topLeft.x, topLeft.y, topRight.x - topLeft.x, bottomLeft.y - topLeft.y);
	}
}
