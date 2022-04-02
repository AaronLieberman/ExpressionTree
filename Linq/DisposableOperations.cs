using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class DisposableOperations
	{
		public static void DisposeIfNotNull(this IDisposable disposable)
		{
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		public static void DisposeAndNullOut(ref IDisposable disposable)
		{
			disposable.Dispose();
			disposable = null;
		}

		public static void DisposeIfNotNullAndNullOut(ref IDisposable disposable)
		{
			if (disposable != null)
			{
				disposable.Dispose();
				disposable = null;
			}
		}
	}
}
