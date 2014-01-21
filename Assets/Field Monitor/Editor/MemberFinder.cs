using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace FieldMonitor
{
	/// <summary>
	/// Handles finding fields and properties with a Monitor attribute attached.
	/// </summary>
	static class MemberFinder
	{
		const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		static readonly string[] assemblyNames = { "UnityScript", "Assembly-CSharp", "Assembly-CSharp-firstpass" };

		/// <summary>
		/// Gets all monitored fields, grouped by their declaring type.
		/// </summary>
		/// <returns>Dictionary of types and their monitored fields.</returns>
		internal static Dictionary<Type, MemberInfo[]> GetMonitoredMembers ()
		{
			var types = GetTypes(assemblyNames);
			var membersInMonitoredTypes = GetMembersInMonitoredTypes(types);
			var otherMonitoredMembers = GetOtherMonitoredMembers(types);

			var monitored = membersInMonitoredTypes.Union(otherMonitoredMembers)
				.GroupBy(member => member.DeclaringType)
				.ToDictionary(group => group.Key, group => group.ToArray());

			return monitored;
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
		/// Finds all fields and properties in classes being monitored.
		/// </summary>
		/// <param name="types">All types.</param>
		/// <returns>Array of members in monitored class.</returns>
		static MemberInfo[] GetMembersInMonitoredTypes (Type[] types)
		{
			var monitoredTypes = types
				.Where(type => HasAttribute(type))
				.ToArray();

			var fields = GetFields(monitoredTypes)
				.Where(member => !member.Name.EndsWith("k__BackingField"));

			var properties = GetProperties(monitoredTypes);
			var members = fields.Union(properties).ToArray();
			return members;
		}

		/// <summary>
		/// Finds all fields and properties explicitly marked to be monitored.
		/// </summary>
		/// <param name="types">All types.</param>
		/// <returns>Array of monitored members.</returns>
		static MemberInfo[] GetOtherMonitoredMembers (Type[] types)
		{
			var fields = GetFields(types);
			var properties = GetProperties(types);

			var members = fields.Union(properties)
				.Where(member => HasAttribute(member))
				.ToArray();

			return members;
		}

		/// <summary>
		/// Gets all fields given some types.
		/// </summary>
		/// <param name="types">Types to look through.</param>
		/// <returns>Fields as members.</returns>
		static MemberInfo[] GetFields (Type[] types)
		{
			var fields = types
				.SelectMany(type => type.GetFields(staticFlags))
				.Cast<MemberInfo>()
				.ToArray();

			return fields;
		}

		/// <summary>
		/// Gets all properties given some types.
		/// </summary>
		/// <param name="types">Types to look through.</param>
		/// <returns>Properties as members.</returns>
		static MemberInfo[] GetProperties (Type[] types)
		{
			var properties = types
				.SelectMany(type => type.GetProperties(staticFlags))
				.Cast<MemberInfo>()
				.ToArray();

			return properties;
		}

		/// <summary>
		/// Checks if a class or field or property is being monitored.
		/// </summary>
		/// <returns>True if member is monitored.</returns>
		static bool HasAttribute (MemberInfo member)
		{
			var result = member
				.GetCustomAttributes(false)
				.Any(attr => attr is Monitor);

			return result;
		}
	}
}
