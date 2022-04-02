using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class ArrayExtensions
	{
		public static T[] Linearize<T>(this T[,] multidimensionalArray)
		{
			var width = multidimensionalArray.GetLength(0);
			var height = multidimensionalArray.GetLength(1);
			var result = new T[width * height];

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					result[y * width + x] = multidimensionalArray[x, y];
				}
			}

			return result;
		}

		public static T[,] Delinearize<T>(this T[] linearArray, int width, int height)
		{
			var result = new T[width, height];

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					result[x, y] = linearArray[y * width + x];
				}
			}

			return result;
		}
	}
}
