using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FieldMonitor
{
	class Window : EditorWindow
	{
		static Dictionary<Type, MemberInfo[]> monitored;

		[MenuItem("Window/Field Monitor")]
		static void Init ()
		{
			EditorWindow.GetWindow<Window>("Field Monitor");
		}

		void OnGUI ()
		{
			if (monitored == null) {
				return;
			}

			foreach (var group in monitored) {
				GUILayout.Label(group.Key.Name, EditorStyles.boldLabel);

				foreach (var member in group.Value) {
					var label = member.Name + ": ";

					if (member is FieldInfo) {
						var field = member as FieldInfo;
						label += field.GetValue(null);
					} else if (member is PropertyInfo) {
						var property = member as PropertyInfo;
						label += property.GetGetMethod().Invoke(null, null).ToString();
					}

					GUILayout.Label(label);
				}
			}
		}

		void OnFocus ()
		{
			monitored = MemberFinder.GetMonitoredMembers();
		}
	}
}
