using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBdev.Extensions
{
	/// <summary>
	/// Extensions for System.Collections.Generic.IEnumerable
	/// </summary>
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Returns index of first encountered instance that satisfies a given condition,
		/// -1 if no such object was found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">a collection</param>
		/// <param name="predicate">function to be executed for each object until matching instance is found
		/// or end of collection is reached</param>
		/// <returns>index of object, -1 if none was found</returns>
		public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> predicate)
		{
			if (list == null)
				return -1;
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			int i = 0;
			foreach (T element in list)
			{
				if (predicate.Invoke(element))
					return i;
				++i;
			}
			return -1;
		}

		/// <summary>
		/// Tries to find the first matching element, returns null in case of failure.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">a collection</param>
		/// <param name="predicate"></param>
		/// <returns>first object that satisfies the given condition,
		/// or null if list is empty or no matching object was found</returns>
		public static T FirstOrNull<T>(this IEnumerable<T> list, Func<T, bool> predicate) where T : class
		{
			if (list == null)
				return null;
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			foreach (T element in list)
			{
				if (predicate.Invoke(element))
					return element;
			}
			return null;
		}

	}
}
