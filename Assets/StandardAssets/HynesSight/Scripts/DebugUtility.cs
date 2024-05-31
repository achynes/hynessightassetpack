using UnityEngine;

namespace HynesSight
{
	public static class DebugUtility
	{
		public static void PrintSwitchDefaultError()
		{
			Debug.LogError("This 'default' case should never be hit.");
		}

		public static bool Assert(bool condition_, string infoMessage_)
		{
			if (!condition_)
			{
				Debug.LogError(infoMessage_);
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
				return false;
			}

			return true;
		}
	}
}