using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBdev.Extensions;

namespace GraphicsManipulation.Dithering
{
	/// <summary>
	/// Applies simple ordered dithering on a given bitmap array.
	/// </summary>
	public class OrderedDitheringConverter
	{
		private static readonly Dictionary<int, int[][]> Matrices;

		public static readonly int MaxMatrixSize = 32;

		public static readonly bool[] MatrixSizeIsAccepted;

		static OrderedDitheringConverter()
		{
			Matrices = new Dictionary<int, int[][]>();
			MatrixSizeIsAccepted = new bool[MaxMatrixSize + 1];

			MatrixSizeIsAccepted[0] = false;
			MatrixSizeIsAccepted[1] = false;

			Matrices.Add(2, new int[][]
				{
					new int[] {1, 3},
					new int[] {4, 2}
				});
			MatrixSizeIsAccepted[2] = true;

			Matrices.Add(3, new int[][]
				{
					new int[] {3, 7, 4},
					new int[] {6, 1, 9},
					new int[] {2, 8, 5}
				});
			MatrixSizeIsAccepted[3] = true;

			#region obsolete matrices that are derived from smaller ones
			//Matrices.Add(4, new int[][]
			//	{
			//		new int[] {1, 9, 3, 11},
			//		new int[] {13, 5, 15, 7},
			//		new int[] {4, 12, 2, 10},
			//		new int[] {16, 8, 14, 6}
			//	});

			//Matrices.Add(6, new int[][]
			//	{
			//		new int[] {9, 25, 13, 11, 27, 15},
			//		new int[] {21, 1, 33, 23, 3, 35},
			//		new int[] {5, 29, 17, 7, 31, 19},
			//		new int[] {12, 28, 16, 10, 26, 14},
			//		new int[] {24, 4, 36, 22, 2, 34},
			//		new int[] {8, 32, 20, 6, 30, 18}
			//	});

			//Matrices.Add(8, new int[][]
			//	{
			//		new int[] {1, 49, 13, 61, 4, 52, 16, 64},
			//		new int[] {33, 17, 45, 29, 36, 20, 48, 32},
			//		new int[] {9, 57, 5, 53, 12, 60, 8, 56},
			//		new int[] {41, 25, 37, 21, 44, 28, 40, 24},
			//		new int[] {3, 51, 15, 63, 2, 50, 14, 62},
			//		new int[] {35, 19, 47, 31, 34, 18, 46, 30},
			//		new int[] {11, 59, 7, 55, 10, 58, 6, 54},
			//		new int[] {43, 27, 39, 23, 42, 26, 38, 22}
			//	}); 
			#endregion

			for (int i = 4; i <= MaxMatrixSize; ++i)
			{
				MatrixSizeIsAccepted[i] = false;

				if (i % 2 != 0)
					continue;

				int[][] m = null;

				if (Matrices.TryGetValue(i, out m))
					continue; // matrix of this size was added manually

				int iDiv = i / 2;

				if (!Matrices.TryGetValue(iDiv, out m))
					continue; // n times smaller matrix must exist

				var module = m.MatrixAdd(-1).MatrixMultiply(4);

				int[][] matrix = new int[i][];
				for (int y = 0; y < i; ++y)
				{
					matrix[y] = new int[i];
					for (int x = 0; x < i; ++x)
					{
						int modifier = 1;
						if (y >= iDiv)
						{
							++modifier;
							if (x < iDiv)
								modifier += 2;
						}
						else if (x >= iDiv)
							modifier += 2;
						matrix[y][x] = module[y % iDiv][x % iDiv] + modifier;
					}
				}

				Matrices.Add(i, matrix);
				MatrixSizeIsAccepted[i] = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public OrderedDitheringConverter()
		{
		}

		private double processOnePixel(double color, double[] levels, double[] levelsBounds,
			int levelsCountLess, double currentDitherValue)
		{
			if (color < 0.7 && color > 0.6)
				color = color + 1 - 1;

			int colorLevel = levelsCountLess;
			for (int i = 1; i < levelsCountLess; ++i)
				if (color < levels/*Bounds*/[i])
				{
					//colorApprox = levels[i];
					colorLevel = i;
					break;
				}

			if (color < levels[colorLevel] - currentDitherValue)
				--colorLevel;

			return levels[colorLevel];
		}

		/// <summary>
		/// Processes the input array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="matrixSize"></param>
		/// <returns></returns>
		public FastBitmapArray Process(FastBitmapArray array, int matrixSize, int levelsCount)
		{
			#region old approach
			//var processed = new FastBitmapArray(array.Width * matrixSize, array.Height * matrixSize);

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
			#endregion

			// new approach:
			var processed = new FastBitmapArray(array.Width, array.Height);

			int levelsCountLess = levelsCount - 1;

			var matrix = Matrices[matrixSize];
			var matrixCoef = matrixSize * matrixSize + 1;
			double matrixLevel = 1.0 / levelsCountLess;

			double[] levels = new double[levelsCount];
			// upper bounds for all levels (except for the last level, cos this one is always 1.0)
			double[] levelsBounds = new double[levelsCountLess];
			for (int i = 0; i < levelsCount; ++i)
			{
				levels[i] = ((double)i) / levelsCountLess;
				if (i > 0)
					levelsBounds[i - 1] = (levels[i - 1] + levels[i]) / 2;
				if (i > 0 && i < levelsCount - 1)
					levels[i] += 0.5 / (levelsCountLess * (matrixCoef - 1) + 1);
			}

			int matrixX = 0;
			int matrixY = 0;

			for (int x = 0; x < array.Width; ++x)
			{
				for (int y = 0; y < array.Height; ++y)
				{
					double matrixValue = matrixLevel * ((double)matrix[matrixY][matrixX]) / matrixCoef;

					double red = array.GetRed(x, y);
					double green = array.GetGreen(x, y);
					double blue = array.GetBlue(x, y);

					// approximation
					double redApprox = processOnePixel(red, levels, levelsBounds, levelsCountLess, matrixValue);
					double greenApprox = processOnePixel(green, levels, levelsBounds, levelsCountLess, matrixValue);
					double blueApprox = processOnePixel(blue, levels, levelsBounds, levelsCountLess, matrixValue);

					processed.SetRedBatch(x, y, redApprox);
					processed.SetGreenBatch(x, y, greenApprox);
					processed.SetBlueBatch(x, y, blueApprox);

					++matrixY;
					if (matrixY == matrixSize)
						matrixY = 0;
				}
				++matrixX;
				if (matrixX == matrixSize)
					matrixX = 0;
			}

			processed.SetBatchArea();

			return processed;
		}

	}
}
