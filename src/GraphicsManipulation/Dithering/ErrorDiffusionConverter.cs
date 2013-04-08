using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsManipulation.Dithering
{
	/// <summary>
	/// Performs error diffusion on a given bitmap array.
	/// </summary>
	public class ErrorDiffusionConverter
	{
		private static readonly Dictionary<ErrorDiffusionKernelName, int[][]> Matrices;

		private static readonly Dictionary<ErrorDiffusionKernelName, int> MatricesSums;

		static ErrorDiffusionConverter()
		{
			Matrices = new Dictionary<ErrorDiffusionKernelName, int[][]>();
			MatricesSums = new Dictionary<ErrorDiffusionKernelName, int>();

			Matrices.Add(ErrorDiffusionKernelName.FloydSteinberg, new int[][]
				{
					new int[] {0, 0, 7},
					new int[] {3, 5, 1}
				});
			MatricesSums.Add(ErrorDiffusionKernelName.FloydSteinberg, 16);

			Matrices.Add(ErrorDiffusionKernelName.JarvisJudiceNinke, new int[][]
				{
					new int[] {0, 0, 0, 7, 5},
					new int[] {3, 5, 7, 5, 3},
					new int[] {1, 3, 5, 3, 1}
				});
			MatricesSums.Add(ErrorDiffusionKernelName.JarvisJudiceNinke, 48);

			Matrices.Add(ErrorDiffusionKernelName.Burke, new int[][]
				{
					new int[] {0, 0, 0, 8, 4},
					new int[] {2, 4, 8, 4, 2}
				});
			MatricesSums.Add(ErrorDiffusionKernelName.Burke, 32);

			Matrices.Add(ErrorDiffusionKernelName.Stucky, new int[][]
				{
					new int[] {0, 0, 0, 8, 4},
					new int[] {2, 4, 8, 4, 2},
					new int[] {1, 2, 4, 2, 1}
				});
			MatricesSums.Add(ErrorDiffusionKernelName.Stucky, 42);

		}

		public FastBitmapArray Process(FastBitmapArray array, ErrorDiffusionKernelName kernelName, int levelsCount)
		{
			if (levelsCount < 2)
				throw new ArgumentException("at least 2 levels are needed");

			var processed = new FastBitmapArray(array.Width, array.Height);

			int levelsCountLess = levelsCount - 1;

			double[] levels = new double[levelsCount];
			// upper bounds for all levels (except for the last level, cos this one is always 1.0)
			double[] levelsBounds = new double[levelsCountLess];
			for (int i = 0; i < levelsCount; ++i)
			{
				levels[i] = ((double)i) / levelsCountLess;
				if (i > 0)
					levelsBounds[i - 1] = (levels[i - 1] + levels[i]) / 2;
			}

			var matrix = Matrices[kernelName];
			var matrixSum = MatricesSums[kernelName];
			var matrixWidth = matrix[0].Length;
			var matrixHeight = matrix.Length;
			var currentPixelX = (matrixWidth - 1) / 2;

			for (int y = 0; y < array.Height; ++y)
			{
				for (int x = 0; x < array.Width; ++x)
				{
					// get color value
					double red = array.GetRed(x, y);
					double green = array.GetGreen(x, y);
					double blue = array.GetBlue(x, y);

					// add error accumulated from previous pixels
					red += processed.GetRed(x, y);
					green += processed.GetGreen(x, y);
					blue += processed.GetBlue(x, y);

					// approximate
					double redApprox = levels[levelsCountLess];
					for (int i = 0; i < levelsCountLess; ++i)
						if (red < levelsBounds[i])
						{
							redApprox = levels[i];
							break;
						}
					double greenApprox = levels[levelsCountLess];
					for (int i = 0; i < levelsCountLess; ++i)
						if (green < levelsBounds[i])
						{
							greenApprox = levels[i];
							break;
						}
					double blueApprox = levels[levelsCountLess];
					for (int i = 0; i < levelsCountLess; ++i)
						if (blue < levelsBounds[i])
						{
							blueApprox = levels[i];
							break;
						}

					// calculate error
					double redError = red - redApprox;
					double greenError = green - greenApprox;
					double blueError = blue - blueApprox;

					// set current pixel value
					processed.SetRedBatch(x, y, redApprox);
					processed.SetGreenBatch(x, y, greenApprox);
					processed.SetBlueBatch(x, y, blueApprox);

					// diffuse the arror accross other pixels
					for (int xx = 0; xx < matrixWidth; ++xx)
					{
						for (int yy = 0; yy < matrixHeight; ++yy)
						{
							if (yy == 0 && xx <= currentPixelX)
								continue;

							int xxx = x + xx - currentPixelX;
							int yyy = y + yy;

							if (xxx < 0 || xxx >= array.Width || yyy >= array.Height)
								continue;

							double level = ((double)matrix[yy][xx]) / matrixSum;
							processed.SetRedBatch(xxx, yyy,
								processed.GetRed(xxx, yyy) + redError * level);
							processed.SetGreenBatch(xxx, yyy,
								processed.GetGreen(xxx, yyy) + greenError * level);
							processed.SetBlueBatch(xxx, yyy,
								processed.GetBlue(xxx, yyy) + blueError * level);
						}
					}

				}
			}

			processed.SetBatchArea();

			return processed;
		}
	}
}
