using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FieldMonitor
{
	/// <summary>
	/// Handles finding static members with a Monitor attribute attached.
	/// </summary>
	static class StaticMemberFinder
	{
		const BindingFlags reflectionOptions = BindingFlags.Static |
		                                       BindingFlags.Public |
		                                       BindingFlags.NonPublic |
		                                       BindingFlags.DeclaredOnly;
		static readonly string[] assemblyNames =
		{
			"UnityScript",
			"Assembly-CSharp",
			"Assembly-CSharp-firstpass",
		};

		/// <summary>
		/// Gets all monitored fields, grouped by their declaring type.
		/// </summary>
		/// <returns>Dictionary of types and their monitored fields.</returns>
		internal static Dictionary<Type, MemberInfo[]> GetMonitoredMembers ()
		{
			var types = GetTypes(assemblyNames);
			var membersInMonitoredTypes = GetMembersInMonitoredTypes(types);
			var otherMonitoredMembers = GetOtherMonitoredMembers(types);

			// Merge two arrays into a dictionary.
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
			var members = types
				.Where(type => MemberFinder.HasAttribute(type))
				.SelectMany(type => MemberFinder.GetMembers(reflectionOptions, type))
				.ToArray();

			return members;
		}

		/// <summary>
		/// Finds all fields and properties explicitly marked to be monitored.
		/// </summary>
		/// <param name="types">All types.</param>
		/// <returns>Array of monitored members.</returns>
		static MemberInfo[] GetOtherMonitoredMembers (Type[] types)
		{
			var members = types
				.SelectMany(type => MemberFinder.GetMembers(reflectionOptions, type))
				.Where(member => MemberFinder.HasAttribute(member))
				.ToArray();

			return members;
		}
	}
}
