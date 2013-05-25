using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBdev.Extensions
{
	/// <summary>
	/// Extensions for arrays.
	/// </summary>
	public static class ArrayExtensions
	{
		/// <summary>
		/// Finds index of maximum value. Returns -1 if argument is empty. Throws if argument is null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static int IndexOfMax<T>(this T[] array) where T : IComparable
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Length == 0)
				return -1;
			if (array.Length == 1)
				return 0;

			T max = array[0];
			int maxIndex = 0;
			int i = 0;
			foreach (T element in array)
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
		/// Finds index of minimum value. Returns -1 if argument is empty. Throws if argument is null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static int IndexOfMin<T>(this T[] array) where T : IComparable
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Length == 0)
				return -1;
			if (array.Length == 1)
				return 0;

			T min = array[0];
			int minIndex = 0;
			int i = 0;
			foreach (T element in array)
			{
				if (element.CompareTo(min) < 0)
				{
					min = element;
					minIndex = i;
				}
				++i;
			}
			return minIndex;
		}

		/// <summary>
		/// Returns index of the first occurance of 'true' in this array,
		/// or -1 if array has only 'false' values.
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static int IndexOfTrue(this bool[] array)
		{
			int i = 0;
			foreach (bool element in array)
			{
				if (element)
					return i;
				++i;
			}
			return -1;
		}

		/// <summary>
		/// Returns index of the first occurance of 'false' in this array,
		/// or -1 if array has only 'true' values.
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static int IndexOfFalse(this bool[] array)
		{
			int i = 0;
			foreach (bool element in array)
			{
				if (!element)
					return i;
				++i;
			}
			return -1;
		}

		/// <summary>
		/// Adds corresponding elements of matrices.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int[][] MatrixAdd(this int[][] input, int value)
		{
			int height = input.Length;
			int width = input[0].Length;
			int[][] resultMatrix = new int[height][];

			for (int i = 0; i < height; ++i)
			{
				resultMatrix[i] = new int[width];
				for (int j = 0; j < width; ++j)
				{
					resultMatrix[i][j] = input[i][j] + value;
				}
			}

			return resultMatrix;
		}

		/// <summary>
		/// Multiplies each entry of a matrix by specific value.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int[][] MatrixMultiply(this int[][] input, int value)
		{
			int height = input.Length;
			int width = input[0].Length;
			int[][] resultMatrix = new int[height][];

			for (int i = 0; i < height; ++i)
			{
				resultMatrix[i] = new int[width];
				for (int j = 0; j < width; ++j)
				{
					resultMatrix[i][j] = input[i][j] * value;
				}
			}

			return resultMatrix;
		}

		/// <summary>
		/// Performs a matrix multiplication.
		/// </summary>
		/// <param name="m1"></param>
		/// <param name="m2"></param>
		/// <returns></returns>
		public static int[][] MatrixMultiply(this int[][] m1, int[][] m2)
		{
			int height = m1.Length;
			int width = m2[0].Length;
			int[][] resultMatrix = new int[height][];

			int m1Width = m1[0].Length;

			for (int i = 0; i < height; ++i)
			{
				resultMatrix[i] = new int[width];
				for (int j = 0; j < width; ++j)
				{
					resultMatrix[i][j] = 0;
					for (int k = 0; k < m1Width; ++k)
					{
						resultMatrix[i][j] += m1[i][k] * m2[k][j];
					}
				}
			}

			return resultMatrix;
		}

	}
}
