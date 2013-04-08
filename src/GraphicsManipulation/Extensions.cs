using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsManipulation
{
	public static class Extensions
	{
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
						resultMatrix[i][j] += m1[i][ k] * m2[k][j];
					}
				}
			}

			return resultMatrix;
		}
	}
}
