using System;
using UnityEngine;

namespace FieldMonitor
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
	public class Monitor : Attribute {}
}
