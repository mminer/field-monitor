using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FieldMonitor
{
	/// <summary>
	/// The primary Field Monitor editor window.
	/// Open this by selecting the Window > Field Monitor menu item.
	/// </summary>
	class MainWindow : EditorWindow
	{
		static readonly GUIContent[] tabLabels =
		{
			new GUIContent("Instance Members", "Fields and properties being monitored in objects."),
			new GUIContent("Static Members", "Static fields and properties being monitored."),
		};

		const string noInstanceMembersMessage = "No instance members found. Attach the FieldMonitor.Monitor attribute to instance fields or properties or a class. See the Read Me for more information.";
		const string noStaticMembersMessage = "No static members found. Attach the FieldMonitor.Monitor attribute to static fields or properties or a class. See the Read Me for more information.";

		static Dictionary<Object, MemberInfo[]> instanceMembers;
		static Dictionary<System.Type, MemberInfo[]> staticMembers;

		Vector2 instanceMembersScrollPos;
		Vector2 staticMembersScrollPos;
		int tab;

		/// <summary>
		/// Displays the Field Monitor GUI.
		/// </summary>
		void OnGUI ()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

				GUILayout.FlexibleSpace();
				tab = GUILayout.Toolbar(tab, tabLabels, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();

			switch (tab) {
				case 0:
					DisplayInstanceMembers();
					break;
				case 1:
					DisplayStaticMembers();
					break;
			}
		}

		/// <summary>
		/// Forces the window to redraw whenever it's active.
		/// </summary>
		void Update ()
		{
			// Force the window to redraw depending on user's preferences.
			switch (Preferences.whenToUpdate) {
				case Preferences.UpdateValues.OnFocus:
					if (focusedWindow == this) {
						Repaint();
					}

					break;

				case Preferences.UpdateValues.OnFocusAndHover:
					if (focusedWindow == this || mouseOverWindow == this) {
						Repaint();
					}

					break;

				case Preferences.UpdateValues.Always:
					Repaint();
					break;
			}
		}

		/// <summary>
		/// Forces members to refresh when the window is clicked on.
		/// </summary>
		void OnFocus ()
		{
			instanceMembers = null;
			staticMembers = null;
		}

		/// <summary>
		/// Renders a list of monitored instance members with their values.
		/// </summary>
		void DisplayInstanceMembers ()
		{
			if (instanceMembers == null) {
				instanceMembers = InstanceMemberFinder.GetMonitoredMembers();
			}

			if (instanceMembers.Count == 0) {
				EditorGUILayout.HelpBox(noInstanceMembersMessage, MessageType.Warning);
				return;
			}

			instanceMembersScrollPos = EditorGUILayout.BeginScrollView(instanceMembersScrollPos);

			foreach (var kvp in	instanceMembers) {
				var heading = string.Format("{0}: {1}", kvp.Key.name, kvp.Key.GetType().Name);

				// Make heading a clickable label that shows game object that component is attached to.
				if (GUILayout.Button(heading, EditorStyles.boldLabel)) {
					Selection.activeObject = kvp.Key;
				}

				DisplayMembers(kvp.Value, kvp.Key);
				EditorGUILayout.Space();
			}

			EditorGUILayout.EndScrollView();
		}

		/// <summary>
		/// Renders a list of monitored static members with their values.
		/// </summary>
		void DisplayStaticMembers ()
		{
			if (staticMembers == null) {
				staticMembers = StaticMemberFinder.GetMonitoredMembers();
			}

			if (staticMembers.Count == 0) {
				EditorGUILayout.HelpBox(noStaticMembersMessage, MessageType.Warning);
				return;
			}

			staticMembersScrollPos = EditorGUILayout.BeginScrollView(staticMembersScrollPos);

			foreach (var kvp in staticMembers) {
				GUILayout.Label(kvp.Key.Name, EditorStyles.boldLabel);
				DisplayMembers(kvp.Value);
				EditorGUILayout.Space();
			}

			EditorGUILayout.EndScrollView();
		}

		[MenuItem("Window/Field Monitor")]
		static void Init ()
		{
			EditorWindow.GetWindow<MainWindow>("Field Monitor");
		}

		/// <summary>
		/// Renders a list of fields and properties.
		/// <param name="member">Fields and properties.</param>
		/// <param name="obj">Instance object (optional).</param>
		static void DisplayMembers (MemberInfo[] members, Object obj = null)
		{
			foreach (var member in members) {
				var displayValue = GetDisplayValue(member, obj);
				EditorGUILayout.LabelField(member.Name, displayValue);
			}
		}

		/// <summary>
		/// Gets a displayable value of the member.
		/// </summary>
		/// <param name="member">Field or property.</param>
		/// <param name="obj">Instance object.</param>
		/// <returns>Display value.</returns>
		static string GetDisplayValue (MemberInfo member, Object obj)
		{
			var displayValue = "";

			if (member is FieldInfo) {
				displayValue += (member as FieldInfo).GetValue(obj);
			} else if (member is PropertyInfo) {
				var getMethod = (member as PropertyInfo).GetGetMethod();

				if (getMethod == null) {
					displayValue += "N/A";
				} else {
					displayValue += getMethod.Invoke(obj, null);
				}
			}

			return displayValue;
		}
	}
}
