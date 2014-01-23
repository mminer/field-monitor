using System;
using System.Linq;
using System.Reflection;

namespace FieldMonitor
{
	/// <summary>
	/// Shared functions for finding fields and properties through reflection.
	/// </summary>
	static class MemberFinder
	{
		/// <summary>
		/// Checks if a class or field or property is being monitored.
		/// </summary>
		/// <param name="member">Member to test.</param>
		/// <returns>True if member is monitored.</returns>
		internal static bool HasAttribute (MemberInfo member)
		{
			var result = member
				.GetCustomAttributes(false)
				.Any(attr => attr is Monitor);

			return result;
		}

		/// <summary>
		/// Gets all fields and properties for the given types.
		/// </summary>
		/// <param name="reflectionOptions">Reflection flags.</param>
		/// <param name="type">Type to look through.</param>
		/// <returns>Fields and properties as members.</returns>
		internal static MemberInfo[] GetMembers (BindingFlags reflectionOptions, Type type)
		{
			var fields = GetFields(reflectionOptions, type);
			var properties = GetProperties(reflectionOptions, type);
			var members = fields.Concat(properties).ToArray();
			return members;
		}

		/// <summary>
		/// Gets all fields given some types.
		/// </summary>
		/// <param name="reflectionOptions">Reflection flags.</param>
		/// <param name="type">Type to look through.</param>
		/// <returns>Fields as members.</returns>
		static MemberInfo[] GetFields (BindingFlags reflectionOptions, Type type)
		{
			var fields = type
				.GetFields(reflectionOptions)
				// Skip "invisible" fields that back up properties.
				.Where(field => !field.Name.EndsWith("k__BackingField"))
				.Cast<MemberInfo>()
				.ToArray();

			return fields;
		}

		/// <summary>
		/// Gets all properties given some types.
		/// </summary>
		/// <param name="reflectionOptions">Reflection flags.</param>
		/// <param name="type">Type to look through.</param>
		/// <returns>Properties as members.</returns>
		static MemberInfo[] GetProperties (BindingFlags reflectionOptions, Type type)
		{
			var properties = type
				.GetProperties(reflectionOptions)
				.Cast<MemberInfo>()
				.ToArray();

			return properties;
		}
	}
}
