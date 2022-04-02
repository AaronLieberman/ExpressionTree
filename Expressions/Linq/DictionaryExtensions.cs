using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class DictionaryExtensions
	{
		public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
		{
			var result = defaultValue;
			TValue temp;

			if (dictionary.TryGetValue(key, out temp))
			{
				result = temp;
			}

			return result;
		}

		public static T TryGetAs<TKey, T>(this IDictionary<TKey, object> dictionary, TKey key)
			where T : class
		{
			T result = null;
			object temp;

			if (dictionary.TryGetValue(key, out temp))
			{
				result = temp as T;
			}

			return result;
		}
	}
}
