using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class StringExtensions
	{
		[DebuggerNonUserCode]
		[StringFormatMethod("format")]
		public static string FormatWith(this string format, params object[] args)
		{
			return String.Format(format, args);
		}

		[DebuggerNonUserCode]
		// ReSharper disable InconsistentNaming
		public static bool IEquals(this string a, string b)
		// ReSharper restore InconsistentNaming
		{
			return a.Equals(b, StringComparison.OrdinalIgnoreCase);
		}
	}
}
