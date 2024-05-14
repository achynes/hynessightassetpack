using UnityEngine;

namespace HynesSight
{
	public static class DebugHelpers
	{
		public static void PrintSwitchDefaultError()
		{
			Debug.LogError("This 'default' case should never be hit.");
		}

		public static void PrintSwitchUnusedCaseError<T>(T unusedCase)
		{
			Debug.LogErrorFormat("%s case should never be hit.", unusedCase);
		}

		public static void Assert(bool condition, string errorMessage, params object[] errorParams)
		{
			if (!condition)
			{
				if (errorParams.Length > 0)
					Debug.LogErrorFormat(errorMessage, errorParams);
				else
					Debug.LogError(errorMessage);
			}
		}
	}
}