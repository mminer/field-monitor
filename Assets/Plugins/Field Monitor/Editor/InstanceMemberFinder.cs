using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FieldMonitor
{
	/// <summary>
	/// Handles finding members in instances with a Monitor attribute attached.
	/// </summary>
	static class InstanceMemberFinder
	{
		const BindingFlags reflectionOptions = BindingFlags.Instance |
		                                       BindingFlags.Public |
		                                       BindingFlags.NonPublic |
		                                       BindingFlags.DeclaredOnly;
		/// <summary>
		/// Gets all monitored fields, grouped by their declaring type.
		/// </summary>
		/// <returns>Dictionary of types and their monitored fields.</returns>
		internal static Dictionary<Object, MemberInfo[]> GetMonitoredMembers ()
		{
			var objects = Object.FindObjectsOfType(typeof(Object));
			var membersInMonitoredObjects = GetMembersInMonitoredObjects(objects);
			var otherMonitoredMembers = GetOtherMonitoredMembers(objects);

			// Merge two dictionaries into a new dictionary.
			var monitored = membersInMonitoredObjects.Concat(otherMonitoredMembers)
				.GroupBy(dictionary => dictionary.Key)
				// Skip objects that have no instance members being watched.
				.Where(group => group.Any(members => members.Value.Length > 0))
				.ToDictionary(
					group => group.Key,
					// Combine the two dictionary's members into one set.
					group => group.SelectMany(members => members.Value)
					              .Distinct()
					              .ToArray());
			return monitored;
		}

		/// <summary>
		/// Finds all fields and properties in objects being monitored.
		/// </summary>
		/// <param name="objects">All object instances.</param>
		/// <returns>Array of members, grouped by object.</returns>
		static Dictionary<Object, MemberInfo[]> GetMembersInMonitoredObjects (Object[] objects)
		{
			var members = objects
				.Where(obj => MemberFinder.HasAttribute(obj.GetType()))
				.ToDictionary(
					obj => obj,
					obj => MemberFinder.GetMembers(reflectionOptions, obj.GetType()));

			return members;
		}

		/// <summary>
		/// Finds all fields and properties in objects explicitly marked to be monitored.
		/// </summary>
		/// <param name="objects">All object instances.</param>
		/// <returns>Array of monitored members.</returns>
		static Dictionary<Object, MemberInfo[]> GetOtherMonitoredMembers (Object[] objects)
		{
			var members = objects
				.ToDictionary(
					obj => obj,
					obj => MemberFinder.GetMembers(reflectionOptions, obj.GetType())
					                   .Where(member => MemberFinder.HasAttribute(member))
					                   .ToArray());
			return members;
		}
	}
}
