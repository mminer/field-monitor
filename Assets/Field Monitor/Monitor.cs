using System;
using UnityEngine;

namespace FieldMonitor
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
	public class Monitor : Attribute {}
}
