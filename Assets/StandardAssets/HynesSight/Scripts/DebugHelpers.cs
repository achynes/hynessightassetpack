using UnityEngine;

namespace HynesSight
{
	public static class DebugHelpers
	{
		public static void PrintSwitchDefaultError()
		{
			Debug.LogError("This 'default' case should never be hit.");
		}
	}
}