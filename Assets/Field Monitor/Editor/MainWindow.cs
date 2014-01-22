using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FieldMonitor
{
	/// <summary>
	/// The primary Field Monitor editor window.
	/// </summary>
	class MainWindow : EditorWindow
	{
		static Dictionary<Type, MemberInfo[]> monitored;
		Vector2 scrollPos;

		[MenuItem("Window/Field Monitor")]
		static void Init ()
		{
			EditorWindow.GetWindow<MainWindow>("Field Monitor");
		}

		void OnGUI ()
		{
			if (monitored == null) {
				RefreshMonitored();
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			foreach (var group in monitored) {
				GUILayout.Label(group.Key.Name, EditorStyles.boldLabel);

				foreach (var member in group.Value) {
					var displayValue = GetDisplayValue(member);
					EditorGUILayout.LabelField(member.Name, displayValue);
				}
			}

			EditorGUILayout.EndScrollView();
		}

		void OnFocus ()
		{
			RefreshMonitored();
		}

		/// <summary>
		/// Refreshes the types that are being monitored.
		/// </summary>
		static void RefreshMonitored ()
		{
			monitored = MemberFinder.GetMonitoredMembers();
		}

		/// <summary>
		/// Gets a displayable value of the member.
		/// </summary>
		/// <param name="member">Field or property.</param>
		/// <returns>Display value.</returns>
		static string GetDisplayValue (MemberInfo member)
		{
			string displayValue = "";
			object val = null;

			if (member is FieldInfo) {
				val = (member as FieldInfo).GetValue(null);
			} else if (member is PropertyInfo) {
				var getMethod = (member as PropertyInfo).GetGetMethod();

				if (getMethod != null) {
					val = getMethod.Invoke(null, null);
				}
			}

			if (val != null) {
				displayValue += val;
			}

			return displayValue;
		}
	}
}
