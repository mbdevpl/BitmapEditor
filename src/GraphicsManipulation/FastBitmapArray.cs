using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Windows;
using System.Collections.ObjectModel;

namespace GraphicsManipulation
{
	/// <summary>
	/// Contains four two-dimensional arrays containing red, green, blue and alpha channels
	/// of a source image. Color values are stored as double values from 0.0 to 1.0.
	/// Moreover the array remembers which pixels where changed since last conversion
	/// to a .NET object.
	/// </summary>
	public class FastBitmapArray
	{
		private static readonly double MaxByteValue = 255;

		/// <summary>
		/// Width of the bitmap array in pixels.
		/// </summary>
		public int Width { get { return width; } }
		private int width;

		/// <summary>
		/// Height of the bitmap array in pixels.
		/// </summary>
		public int Height { get { return height; } }
		private int height;

		public ReadOnlyCollection<ReadOnlyCollection<double>> Red
		{
			get
			{
				if (R == null)
					return null;
				ReadOnlyCollection<double>[] result = new ReadOnlyCollection<double>[width];
				for (int x = 0; x < width; x++)
				{
					result[x] = new ReadOnlyCollection<double>(R[x]);
				}
				return new ReadOnlyCollection<ReadOnlyCollection<double>>(result);
			}
		}
		private double[][] R;

		//public double[][] Green { get { return G; } }
		private double[][] G;

		//public double[][] Blue { get { return B; } }
		private double[][] B;

		//public double[][] Alpha { get { return A; } }
		private double[][] A;

		private WriteableBitmap bitmap;

		private int bytesPerPixel;

		private byte[] pixelBytes;

		private int xMinChanged;

		private int xMaxChanged;

		private int yMinChanged;

		private int yMaxChanged;

		private bool anythingChanged;

		private bool[][] changed;

		private FastBitmapArray()
		{
			width = 0;
			height = 0;

			R = null;
			G = null;
			B = null;
			A = null;

			bitmap = null;
			bytesPerPixel = 0;
			pixelBytes = null;

			xMinChanged = -1;
			xMaxChanged = -1;
			yMinChanged = -1;
			yMaxChanged = -1;
			anythingChanged = false;
			changed = null;
		}

		/// <summary>
		/// Constructs a new empty array. All pixels have color values equal to 0.0,
		/// and alpha equal to 1.0.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public FastBitmapArray(int width, int height)
			: this()
		{
			this.width = width;
			this.height = height;

			R = new double[width][];
			G = new double[width][];
			B = new double[width][];
			A = new double[width][];
			changed = new bool[width][];

			for (int x = 0; x < width; x++)
			{
				R[x] = new double[height];
				G[x] = new double[height];
				B[x] = new double[height];
				A[x] = new double[height];
				changed[x] = new bool[height];

				for (int y = 0; y < height; y++)
				{
					R[x][y] = 0;
					G[x][y] = 0;
					B[x][y] = 0;
					A[x][y] = 1;
					changed[x][y] = false;
				}
			}

			// bitmap allocation
			bitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32, null);
			bytesPerPixel = 4;
			pixelBytes = new byte[height * width * bytesPerPixel];

			xMinChanged = width - 1;
			xMaxChanged = 0;
			yMinChanged = height - 1;
			yMaxChanged = 0;
			anythingChanged = false;
		}

		public FastBitmapArray(BitmapSource source)
			: this(source.PixelWidth, source.PixelHeight)
		{
			if (source == null)
				throw new NullReferenceException("source of the bitmap cannot be null");

			if (!source.Format.Equals(PixelFormats.Pbgra32))
				source = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);

			if (bytesPerPixel != 4)
			{
				bytesPerPixel = source.Format.BitsPerPixel / 8;
				pixelBytes = new byte[height * width * bytesPerPixel];
			}
			source.CopyPixels(pixelBytes, width * bytesPerPixel, 0);

			WriteToChannels(0, 0, width, height);
		}

		#region pixel setters/getters

		public void SetPixel(int x, int y, double? red, double? green, double? blue,
			double? alpha = null)
		{
			if (red != null)
				R[x][y] = (double)red;
			if (green != null)
				G[x][y] = (double)green;
			if (blue != null)
				B[x][y] = (double)blue;

			if (alpha == null)
				return;

			A[x][y] = (double)alpha;
		}

		public void SetPixel(int x, int y, double red, double green, double blue)
		{
			R[x][y] = red;
			G[x][y] = green;
			B[x][y] = blue;
		}

		public double GetRed(int x, int y)
		{
			return R[x][y];
		}

		public double GetGreen(int x, int y)
		{
			return G[x][y];
		}

		public double GetBlue(int x, int y)
		{
			return B[x][y];
		}

		public double GetAlpha(int x, int y)
		{
			return A[x][y];
		}

		public void SetRed(int x, int y, double value)
		{
			Change(x, y);
			R[x][y] = value;
		}

		public void SetGreen(int x, int y, double value)
		{
			Change(x, y);
			G[x][y] = value;
		}

		public void SetBlue(int x, int y, double value)
		{
			Change(x, y);
			B[x][y] = value;
		}

		public void SetAlpha(int x, int y, double value)
		{
			Change(x, y);
			A[x][y] = value;
		}

		#endregion

		#region batched setting

		public void SetBatchArea()
		{
			SetBatchArea(0, 0, width - 1, height - 1);
		}

		public void SetBatchArea(int minX, int minY, int maxX, int maxY)
		{
			if (maxX > xMaxChanged)
				xMaxChanged = maxX;
			if (minX < xMinChanged)
				xMinChanged = minX;
			if (maxY > yMaxChanged)
				yMaxChanged = maxY;
			if (minY < yMinChanged)
				yMinChanged = minY;

			for (int x = minX; x <= maxX; ++x)
				for (int y = minY; y <= maxY; ++y)
					changed[x][y] = true;

			if (anythingChanged)
				return;

			anythingChanged = true;
		}

		public void SetRedBatch(int x, int y, double value)
		{
			//Change(x, y);
			R[x][y] = value;
		}

		public void SetGreenBatch(int x, int y, double value)
		{
			//Change(x, y);
			G[x][y] = value;
		}

		public void SetBlueBatch(int x, int y, double value)
		{
			//Change(x, y);
			B[x][y] = value;
		}

		public void SetAlphaBatch(int x, int y, double value)
		{
			//Change(x, y);
			A[x][y] = value;
		}

		public void SetPixelBatch(int x, int y, double red, double green, double blue, double? alpha = null)
		{
			SetRedBatch(x, y, red);
			SetGreenBatch(x, y, green);
			SetBlueBatch(x, y, blue);
			if (alpha != null)
				SetAlphaBatch(x, y, alpha.Value);
		}

		#endregion

		private void Change(int x, int y)
		{
			if (x > xMaxChanged)
				xMaxChanged = x;
			if (x < xMinChanged)
				xMinChanged = x;
			if (y > yMaxChanged)
				yMaxChanged = y;
			if (y < yMinChanged)
				yMinChanged = y;
			changed[x][y] = true;

			if (anythingChanged)
				return;

			anythingChanged = true;
		}

		public void DrawLine(int xStart, int yStart, int xEnd, int yEnd,
			double red, double green, double blue, int thickness = 1)
		{
			int dx = xEnd - xStart;
			int dy = yEnd - yStart;

			bool primaryX = true;
			if (Math.Abs(dy) > Math.Abs(dx))
				primaryX = false;

			if (thickness > 1)
			{

				double tangent = 0;
				//if (primaryX && dy == 0 || !primaryX && dx == 0)
				//	tangent = 1;
				//else
				tangent = primaryX ? ((double)dy) / dx : ((double)dx) / dy;

				double xThicknessDouble = thickness * Math.Abs(primaryX ? Math.Sin(tangent) : Math.Cos(tangent));
				double yThicknessDouble = thickness * Math.Abs(primaryX ? Math.Cos(tangent) : Math.Sin(tangent));

				int xThickness = (int)Math.Round(xThicknessDouble);
				double xRem = xThicknessDouble - xThickness;
				int yThickness = (int)Math.Round(yThicknessDouble);
				double yRem = yThicknessDouble - yThickness;

				double thickRem = xRem + yRem;

				if (thickRem > 0.5 && xRem < 0.5 && yRem < 0.5)
				{
					if (xThickness > 0)
						++xThickness;
					else
						++yThickness;
				}
				
				int xOffset = xThickness / 2;
				int yOffset = yThickness / 2;

				if (xThickness <= 1)
				{
					for (int i = 0; i < yThickness; ++i)
						DrawLine(xStart, yStart - yOffset + i, xEnd, yEnd - yOffset + i, red, green, blue, 1);
					return;
				}

				if (yThickness <= 1)
				{
					for (int i = 0; i < xThickness; ++i)
						DrawLine(xStart - xOffset + i, yStart, xEnd - xOffset + i, yEnd, red, green, blue, 1);
					return;
				}

				for (int i = 0; i < yThickness; ++i)
					DrawLine(xStart, yStart + i, xEnd, yEnd + i, red, green, blue, 1);

				int sign = 1;
				if (dx * dy < 0)
					sign = -1;

				for (int i = 0; i < xThickness; ++i)
					DrawLine(xStart + (i * sign), yStart, xEnd + (i * sign), yEnd, red, green, blue, 1);

				return;
			}
			bool invertX = false;
			if (primaryX && xStart > xEnd)
				invertX = true;
			if (!primaryX && yStart > yEnd)
				invertX = true;

			if (invertX)
			{
				int temp = xStart;
				xStart = xEnd;
				xEnd = temp;
				temp = yStart;
				yStart = yEnd;
				yEnd = temp;

				dx = xEnd - xStart;
				dy = yEnd - yStart;
			}

			SetBatchArea(Math.Min(xStart, xEnd), Math.Min(yStart, yEnd),
				Math.Max(xStart, xEnd), Math.Max(yStart, yEnd));

			if (!primaryX)
			{
				int temp = dy;
				dy = dx;
				dx = temp;
			}

			int signY = 1;
			if (dy < 0)
			{
				dy *= -1;
				signY = -1;
			}

			int d = 2 * dy - dx;
			int dE = 2 * dy;
			int dNE = 2 * (dy - dx);

			int x = primaryX ? xStart : yStart;
			int y = primaryX ? yStart : xStart;

			if (primaryX)
				SetPixelBatch(x, y, red, green, blue);
			else
				SetPixelBatch(y, x, red, green, blue);

			if (!primaryX)
				xEnd = yEnd;

			while (x < xEnd)
			{
				if (d < 0)
				{
					d += dE;
				}
				else
				{
					d += dNE;
					y += signY;
				}
				++x;

				if (primaryX)
					SetPixelBatch(x, y, red, green, blue);
				else
					SetPixelBatch(y, x, red, green, blue);
			}
		}

		public void RefreshBitmap(Mask mask = Mask.Rectangle)
		{
			if (mask == Mask.Disabled)
			{
				//mask is disabled, therefore the whole image will be repainted
				Int32Rect maskingRect = new Int32Rect(0, 0, width, height);

				WriteToPixelBytes(0, 0, width, height);

				int delta = width * bytesPerPixel;

				bitmap.Lock();
				bitmap.WritePixels(maskingRect, pixelBytes, delta, 0);
				bitmap.Unlock();
			}
			else if (anythingChanged)
			{
				if (mask == Mask.Rectangle)
				{
					int xSize = xMaxChanged - xMinChanged + 1, ySize = yMaxChanged - yMinChanged + 1;
					int delta = xSize * bytesPerPixel;

					Int32Rect maskingRect = new Int32Rect(xMinChanged, yMinChanged, xSize, ySize);

					byte[] pixelsRect = new byte[xSize * ySize * bytesPerPixel];
					int index;
					for (int x = 0; x < xSize; x++)
					{
						int xx = xMinChanged + x;
						index = x * bytesPerPixel;
						for (int y = 0; y < ySize; y++)
						{
							int yy = yMinChanged + y;
							pixelsRect[index + 2] = (byte)(MaxByteValue * R[xx][yy]);
							pixelsRect[index + 1] = (byte)(MaxByteValue * G[xx][yy]);
							pixelsRect[index + 0] = (byte)(MaxByteValue * B[xx][yy]);
							pixelsRect[index + 3] = (byte)(MaxByteValue * A[xx][yy]);
							index += delta;

							changed[xx][yy] = false;
						}
					}

					bitmap.Lock();
					bitmap.WritePixels(maskingRect, pixelsRect, delta, 0);
					bitmap.Unlock();
				}
				else if (mask == Mask.PerPixel)
				{
					Int32Rect maskingRect = new Int32Rect(0, 0, 1, 1);

					bitmap.Lock();

					byte[] pixel = new byte[bytesPerPixel];
					for (int y = yMinChanged; y <= yMaxChanged; y++)
					{
						for (int x = xMinChanged; x <= xMaxChanged; x++)
						{
							if (!changed[x][y])
								continue;

							maskingRect.X = x;
							maskingRect.Y = y;

							pixel[2] = (byte)(MaxByteValue * R[x][y]);
							pixel[1] = (byte)(MaxByteValue * G[x][y]);
							pixel[0] = (byte)(MaxByteValue * B[x][y]);
							pixel[3] = (byte)(MaxByteValue * A[x][y]);

							bitmap.WritePixels(maskingRect, pixel, bytesPerPixel, 0);

							changed[x][y] = false;
						}
					}

					bitmap.Unlock();
				}
				else if (mask == Mask.Circle)
				{
					throw new NotImplementedException("circle-shaped mask for FastBitmapArray is not implemented");
				}

				xMinChanged = width - 1;
				xMaxChanged = 0;
				yMinChanged = height - 1;
				yMaxChanged = 0;
				anythingChanged = false;
			}
		}

		//private void WriteToPixelBytes()
		//{
		//	WriteToPixelBytes(0, 0, width, height);
		//}

		private void WriteToChannels(int xMin, int yMin, int xMax, int yMax)
		{
			int index = 0;
			int delta = width * bytesPerPixel;
			for (int x = xMin; x < xMax; x++)
			{
				index = x * bytesPerPixel;
				for (int y = yMin; y < yMax; y++)
				{
					//index = (x + y * width) * bytesPerPixel;
					R[x][y] = ((double)pixelBytes[index + 2]) / MaxByteValue;
					G[x][y] = ((double)pixelBytes[index + 1]) / MaxByteValue;
					B[x][y] = ((double)pixelBytes[index + 0]) / MaxByteValue;
					A[x][y] = ((double)pixelBytes[index + 3]) / MaxByteValue;
					changed[x][y] = false;
					index += delta;
				}
			}
		}

		private void WriteToPixelBytes(int xMin, int yMin, int xMax, int yMax)
		{
			int index = 0;
			int delta = width * bytesPerPixel;
			for (int x = xMin; x < xMax; x++)
			{
				index = x * bytesPerPixel;
				for (int y = yMin; y < yMax; y++)
				{
					//int index = (i + j * width) * 4;
					pixelBytes[index + 2] = (byte)(MaxByteValue * R[x][y]);
					pixelBytes[index + 1] = (byte)(MaxByteValue * G[x][y]);
					pixelBytes[index + 0] = (byte)(MaxByteValue * B[x][y]);
					pixelBytes[index + 3] = (byte)(MaxByteValue * A[x][y]);
					index += delta;
				}
			}
		}

		public BitmapSource GetBitmap(Mask mask = Mask.Rectangle)
		{
			RefreshBitmap(mask);
			//bitmap.Freeze();
			//return (BitmapSource)bitmap.GetCurrentValueAsFrozen();
			return bitmap;
		}

	}

}
