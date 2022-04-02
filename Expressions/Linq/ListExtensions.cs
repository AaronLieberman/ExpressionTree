using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;
using JetBrains.Annotations;

namespace Core.Linq
{
	public static class ListExtensions
	{
		// viewModels and models must be sorted in the same sort order
		public static void SetRange<T>(this IList<T> list, IEnumerable<T> values)
		{
			list.Clear();
			list.AddRange(values);
		}

		// viewModels and models must be sorted in the same sort order
		public static void UpdateFrom<TVM, TM>(this IList<TVM> viewModels, IReadOnlyList<TM> models,
			Func<TVM, TM, int> compare, Func<TVM> newVM, Action<TVM, TM> updateVM)
		{
			UpdateFrom(viewModels, models, compare, m => { var vm = newVM(); updateVM(vm, m); return vm; }, updateVM);
		}

		// viewModels and models must be sorted in the same sort order
		public static void UpdateFrom<TVM, TM>(this IList<TVM> viewModels, IReadOnlyList<TM> models,
			Func<TVM, TM, int> compare, Func<TM, TVM> newVM, Action<TVM, TM> updateVM)
		{
			var modelIndex = 0;
			var vmIndex = 0;

			while (modelIndex < models.Count || vmIndex < viewModels.Count)
			{
				//var compareResult = viewModels.Count > vmIndex ? compare(viewModels[vmIndex], models[modelIndex]) : 1;

				int compareResult;

				if (vmIndex >= viewModels.Count)
				{
					compareResult = 1;
				}
				else if (modelIndex >= models.Count)
				{
					compareResult = -1;
				}
				else
				{
					compareResult = compare(viewModels[vmIndex], models[modelIndex]);
				}

				if (compareResult == 0)
				{
					updateVM(viewModels[vmIndex], models[modelIndex]);
					modelIndex++;
					vmIndex++;
				}
				// is there a VM that we need to add?
				else if (compareResult > 0)
				{
					var vm = newVM(models[modelIndex]);
					viewModels.Insert(vmIndex, vm);
					modelIndex++;
					vmIndex++;
				}
				// is there a VM that we need to remove?
				else
				{
					viewModels.RemoveAt(vmIndex);
				}
			}
		}
	}
}
