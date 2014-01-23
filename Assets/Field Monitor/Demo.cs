//#define SHOW_FIELD_MONITOR_DEMO

using UnityEngine;

namespace FieldMonitor
{
	/// <summary>
	/// Sample class demonstrating monitored fields, properties, and classes.
	///
	/// Uncomment the line at the top of this class then open the Field Monitor
	/// window to see it in action.
	///
	/// Attach this script to a game object (i.e. create an instance of the
	/// class) to see instance fields.
	/// </summary>
	public class Demo : MonoBehaviour
	{
		#if SHOW_FIELD_MONITOR_DEMO

		static string hiddenField;

		[FieldMonitor.Monitor]
		static float monitoredField = 42;

		[FieldMonitor.Monitor]
		public static int monitoredProperty { get; set; }

		[FieldMonitor.Monitor]
		public float increasingInstanceField = 1337;

		void Update ()
		{
			increasingInstanceField += 0.1f;
		}

		[FieldMonitor.Monitor]
		class AnotherDemo
		{
			static string anotherField = "Hello beautiful.";
			public static bool anotherProperty { get; set; }
		}

		#endif
	}
}
