using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsManipulation.Dithering
{
	public class OrderedDithering
	{
		private static readonly Dictionary<int, int[][]> Matrices;

		static OrderedDithering()
		{
			Matrices = new Dictionary<int, int[][]>();

			Matrices.Add(2, new int[][]
				{
					new int[] {1, 3},
					new int[] {4, 2}
				});

			Matrices.Add(3, new int[][]
				{
					new int[] {3, 7, 4},
					new int[] {6, 1, 9},
					new int[] {2, 8, 5}
				});

			Matrices.Add(4, new int[][]
				{
					new int[] {1, 9, 3, 11},
					new int[] {13, 5, 15, 7},
					new int[] {4, 12, 2, 10},
					new int[] {16, 8, 14, 6}
				});

			Matrices.Add(8, new int[][]
				{
					new int[] {1, 49, 13, 61, 4, 52, 16, 64},
					new int[] {33, 17, 45, 29, 36, 20, 48, 32},
					new int[] {9, 57, 5, 53, 12, 60, 8, 56},
					new int[] {41, 25, 37, 21, 44, 28, 40, 24},
					new int[] {3, 51, 15, 63, 2, 50, 14, 62},
					new int[] {35, 19, 47, 31, 34, 18, 46, 30},
					new int[] {11, 59, 7, 55, 10, 58, 6, 54},
					new int[] {43, 27, 39, 23, 42, 26, 38, 22}
				});
		}

		public OrderedDithering()
		{
		}

		public FastBitmapArray Process(FastBitmapArray array, int matrixSize)
		{
			// old approach:
			var processed = new FastBitmapArray(array.Width * matrixSize, array.Height * matrixSize);

			int procXmin = 0;
			int procXmax = 0;
			int procYmin = 0;
			int procYmax = 0;

			var matrix = Matrices[matrixSize];
			var matrixCoef = matrixSize * matrixSize + 1;

			for (int x = 0; x < array.Width; ++x)
			{
				procXmin = x * matrixSize;
				procXmax = procXmin + matrixSize;
				for (int y = 0; y < array.Height; ++y)
				{
					double red = array.GetRed(x, y);
					double green = array.GetGreen(x, y);
					double blue = array.GetBlue(x, y);

					procYmin = y * matrixSize;
					procYmax = procYmin + matrixSize;
					for (int xx = procXmin, xxx = 0; xx < procXmax; ++xx, ++xxx)
						for (int yy = procYmin, yyy = 0; yy < procYmax; ++yy, ++yyy)
						{
							double matrixValue = ((double)matrix[xxx][yyy]) / matrixCoef;
							processed.SetRed(xx, yy, red > matrixValue ? 1 : 0);
							processed.SetGreen(xx, yy, green > matrixValue ? 1 : 0);
							processed.SetBlue(xx, yy, blue > matrixValue ? 1 : 0);
						}
				}
			}

			//// new approach:
			//var processed = new FastBitmapArray(array.Width, array.Height);

			//int procXmin = 0;
			//int procXmax = 0;
			//int procYmin = 0;
			//int procYmax = 0;

			//var matrix = Matrices[matrixSize];
			//var matrixCoef = matrixSize * matrixSize + 1;

			//for (int x = 0; x < array.Width; ++x)
			//{
			//	procXmin = x * matrixSize;
			//	procXmax = procXmin + matrixSize;
			//	for (int y = 0; y < array.Height; ++y)
			//	{
			//		double red = array.GetRed(x, y);
			//		double green = array.GetGreen(x, y);
			//		double blue = array.GetBlue(x, y);

			//		procYmin = y * matrixSize;
			//		procYmax = procYmin + matrixSize;
			//		for (int xx = procXmin, xxx = 0; xx < procXmax; ++xx, ++xxx)
			//			for (int yy = procYmin, yyy = 0; yy < procYmax; ++yy, ++yyy)
			//			{
			//				double matrixValue = ((double)matrix[xxx][yyy]) / matrixCoef;
			//				processed.SetRed(xx, yy, red > matrixValue ? 1 : 0);
			//				processed.SetGreen(xx, yy, green > matrixValue ? 1 : 0);
			//				processed.SetBlue(xx, yy, blue > matrixValue ? 1 : 0);
			//			}
			//	}
			//}

			return processed;
		}

	}
}
