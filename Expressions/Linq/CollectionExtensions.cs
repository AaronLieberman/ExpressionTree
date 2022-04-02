using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Core.Linq
{
	public static class CollectionExtensions
	{
		public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				list.Add(item);
			}
		}
	}
}
