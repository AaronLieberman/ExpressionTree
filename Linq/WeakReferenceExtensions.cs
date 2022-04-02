using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class WeakReferenceExtensions
	{
		public static T TryGetTarget<T>(this WeakReference<T> reference)
			where T : class
		{
			T value;
			return reference.TryGetTarget(out value) ? value : null;
		}
	}
}
