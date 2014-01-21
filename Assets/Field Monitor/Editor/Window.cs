using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FieldMonitor
{
	class Window : EditorWindow
	{
		const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		static readonly string[] assemblyNames = { "UnityScript", "Assembly-CSharp", "Assembly-CSharp-firstpass" };
		static readonly Regex propertyNameRegex = new Regex("<(.+)>k__BackingField");
		static Dictionary<Type, FieldInfo[]> monitored;

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

				foreach (var field in group.Value) {
					var label = string.Format("{0}: {1}", GetFieldName(field), field.GetValue(null));
					GUILayout.Label(label);
				}
			}
		}

		void OnFocus ()
		{
			monitored = GetMonitoredFields();
		}

		/// <summary>
		/// Gets all monitored fields, grouped by their declaring type.
		/// </summary>
		/// <returns>Dictionary of types and their monitored fields.</returns>
		static Dictionary<Type, FieldInfo[]> GetMonitoredFields ()
		{
			var types = GetTypes(assemblyNames);
			var fieldsInMonitoredTypes = GetFieldsInMonitoredTypes(types);
			var otherMonitoredFields = GetOtherMonitoredFields(types);

			var monitoredFields = fieldsInMonitoredTypes.Union(otherMonitoredFields)
				.GroupBy(field => field.DeclaringType)
				.ToDictionary(group => group.Key, group => group.ToArray());

			return monitoredFields;
		}

		/// <summary>
		/// Finds all types in the given assemblies.
		/// </summary>
		/// <param name="assemblyNames">Assemblies to search inside.</param>
		/// <returns>Array of types.</returns>
		static Type[] GetTypes (string[] assemblyNames)
		{
			var types = AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(assembly => assemblyNames.Contains(assembly.GetName().Name))
				.SelectMany(assembly => assembly.GetTypes())
				.ToArray();
			return types;
		}

		/// <summary>
		/// Finds all fields in classes that are being monitored.
		/// </summary>
		/// <param name="types">All types.</param>
		/// <returns>Array of fields in monitored class.</returns>
		static FieldInfo[] GetFieldsInMonitoredTypes (Type[] types)
		{
			var fields = types
				.Where(type => HasAttribute(type))
				.SelectMany(type => type.GetFields(staticFlags))
				.ToArray();
			return fields;
		}

		/// <summary>
		/// Finds all fields explicitly marked to be monitored.
		/// </summary>
		/// <param name="types">All types.</param>
		/// <returns>Array of monitored fields.</returns>
		static FieldInfo[] GetOtherMonitoredFields (Type[] types)
		{
			var fields = types
				.SelectMany(type => type.GetFields(staticFlags)
										.Where(field => HasAttribute(field)))
				.ToArray();
			return fields;
		}

		/// <summary>
		/// Checks if a type or field has a VariableWatcher attribute attached.
		/// </summary>
		/// <returns>True if member is monitored.</returns>
		static bool HasAttribute (MemberInfo member)
		{
			return member.GetCustomAttributes(false)
						 .Any(attr => attr is Monitor);
		}

		/// <summary>
		/// Gets a human-readable name of the given field.
		/// </summary>
		/// <param name="field">Field.</param>
		/// <returns>Name suitable for display.</returns>
		static string GetFieldName (FieldInfo field)
		{
			if (propertyNameRegex.IsMatch(field.Name)) {
				return propertyNameRegex.Split(field.Name)[1];
			} else {
				return field.Name;
			}
		}
	}
}
