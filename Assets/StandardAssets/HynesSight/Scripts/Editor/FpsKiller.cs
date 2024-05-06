using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class FpsKiller : EditorWindow
{
	static FpsKiller activeWindow = null;

	int millisecondsAdded = 0;

	[MenuItem("Dreamfeel/FPS Killer")]
	static void Init()
	{
		activeWindow = (FpsKiller)GetWindow(typeof(FpsKiller), utility: false, "FPS Killer");
		activeWindow.Show();
	}

	void OnGUI()
	{
		GUILayout.BeginVertical(EditorStyles.helpBox);

		using (new GUILayout.HorizontalScope())
		{
			GUIStyle _textStyle = new GUIStyle(EditorStyles.label);
			if (millisecondsAdded > 0)
				_textStyle.normal.textColor = Color.red;

			EditorGUILayout.LabelField("Add Milliseconds:", _textStyle);

			millisecondsAdded = EditorGUILayout.DelayedIntField(millisecondsAdded);
			millisecondsAdded = Mathf.Clamp(millisecondsAdded, 0, 2000);
		}

		GUILayout.EndVertical();
	}

	void OnEnable()
	{
		EditorApplication.update -= OnUpdate;
		EditorApplication.update += OnUpdate;
	}

	void OnDisable()
	{
		EditorApplication.update -= OnUpdate;
	}

	void OnUpdate()
	{
		if (!EditorApplication.isPlaying || millisecondsAdded <= 0)
			return;

		Stopwatch _stopwatch = new Stopwatch();
		_stopwatch.Start();
		while (_stopwatch.ElapsedMilliseconds < millisecondsAdded)
		{
			// Kill the requested time here
		}

		UnityEngine.Debug.LogFormat("FPS Killer is active, killed {0} milliseconds this frame.", millisecondsAdded);
	}
}
