using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class ObjectExtensions
	{
		public static bool Is<T>(this object o)
		{
			return o is T;
		}

		public static T As<T>(this object o)
			where T : class
		{
			return o as T;
		}

		public static T To<T>(this object o)
		{
			return (T)o;
		}
	}
}
