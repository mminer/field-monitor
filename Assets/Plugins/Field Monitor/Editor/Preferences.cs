//
// Author: Matthew Miner
//         matthew@matthewminer.com
//         http://matthewminer.com
//
// Copyright (c) 2013
//

using UnityEditor;
using UnityEngine;

namespace FieldMonitor
{
	/// <summary>
	/// Options for controlling when the Field Monitor window updates values.
	/// </summary>
	static class Preferences
	{
		internal enum UpdateValues { OnFocus, OnFocusAndHover, Always }

		const string whenToUpdateKey = "fieldmonitor_whentoupdate";
		const string updateAlwaysWarning = "If you experience editor slowdowns, consider only updating the Field Monitor window when it's in focus or when the mouse hovers over it.";
		static readonly GUIContent whenToUpdateLabel = new GUIContent("Update Values", "When to refresh values displayed in the Field Monitor editor window.");

		internal static UpdateValues whenToUpdate { get; private set; }

		/// <summary>
		/// Loads options stored in EditorPrefs.
		/// </summary>
		static Preferences ()
		{
			whenToUpdate = (UpdateValues)EditorPrefs.GetInt(whenToUpdateKey, (int)UpdateValues.OnFocusAndHover);
		}

		/// <summary>
		/// Displays preferences GUI.
		/// </summary>
		[PreferenceItem("Field Monitor")]
		static void OnGUI ()
		{
			whenToUpdate = (UpdateValues)EditorGUILayout.EnumPopup(whenToUpdateLabel, whenToUpdate);

			if (whenToUpdate == UpdateValues.Always) {
				EditorGUILayout.HelpBox(updateAlwaysWarning, MessageType.Warning);
			}

			if (GUI.changed) {
				// Save preferences.
				EditorPrefs.SetInt(whenToUpdateKey, (int)whenToUpdate);
			}
		}
	}
}
