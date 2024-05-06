using UnityEngine;

public static class ComponentExtensions
{
	public static string GetFullScenePath(this Component component)
	{
		return component.gameObject.GetFullScenePath();
	}
}
