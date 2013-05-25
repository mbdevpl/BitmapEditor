using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBdev.Extensions
{
	/// <summary>
	/// Extensions for System.Collections.Generic.IList
	/// </summary>
	public static class IListExtensions
	{
		/// <summary>
		/// Finds index of maximum value. Returns -1 if argument is empty. Throws if argument is null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static int IndexOfMax<T>(this IList<T> list) where T : IComparable
		{
			if (list == null)
				throw new ArgumentNullException("list");

			if (list.Count == 0)
				return -1;
			if (list.Count == 1)
				return 0;

			T max = list[0];
			int maxIndex = 0;
			int i = 0;
			foreach (T element in list)
			{
				if (element.CompareTo(max) > 0)
				{
					max = element;
					maxIndex = i;
				}
				++i;
			}
			return maxIndex;
		}

		/// <summary>
		/// Removes elements that are duplicates according to regular Equals() method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns>a new list, that has no duplicates</returns>
		public static IList<T> ToSet<T>(this IList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			if (list.Count <= 1)
				return new List<T>(list);

			IList<T> copy = new List<T>(list.Count);
			foreach (T element in list)
				if (!copy.Any(x => x.Equals(element)))
					copy.Add(element);

			return copy;
		}


	}
}
