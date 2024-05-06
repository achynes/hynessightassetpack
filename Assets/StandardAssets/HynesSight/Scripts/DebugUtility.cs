using UnityEngine;

namespace HynesSight
{
	public static class DebugUtility
	{
		public static void PrintSwitchDefaultError()
		{
			Debug.LogError("This 'default' case should never be hit.");
		}
	}
}