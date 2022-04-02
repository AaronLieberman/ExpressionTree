using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class XmlExtensions
	{
		public static T GetAttributeValue<T>(this XmlReader reader, string name, T defaultValue = default(T))
		{
			var result = defaultValue;
			var valueString = reader.GetAttribute(name);

			if (valueString != null)
			{
				result = (T)Convert.ChangeType(valueString, typeof(T));
			}

			return result;
		}
	}
}
