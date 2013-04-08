using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Windows;

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

		//public double[][] Red { get { return R; } }
		private double[][] R;

		//public double[][] Green { get { return G; } }
		private double[][] G;

		//public double[][] Blue { get { return B; } }
		private double[][] B;

		//public double[][] Alpha { get { return A; } }
		private double[][] A;

		private WriteableBitmap bitmap;

		//private double[][] pixels = null;

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

			//width = source.PixelWidth;
			//height = source.PixelHeight;

			//bitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32, null);
			if (bytesPerPixel != 4)
			{
				bytesPerPixel = source.Format.BitsPerPixel / 8;
				//pixels = new double[height * width * bytesPerPixel];
				pixelBytes = new byte[height * width * bytesPerPixel];
			}
			source.CopyPixels(pixelBytes, width * bytesPerPixel, 0);

			//R = new double[width][];
			//G = new double[width][];
			//B = new double[width][];
			//A = new double[width][];

			//changed = new bool[width][];
			//xMinChanged = width - 1;
			//xMaxChanged = 0;
			//yMinChanged = height - 1;
			//yMaxChanged = 0;
			//anythingChanged = false;

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
					Int32Rect maskingRect = new Int32Rect(xMinChanged, yMinChanged,
						xMaxChanged - xMinChanged + 1, yMaxChanged - yMinChanged + 1);

					WriteToPixelBytes(xMinChanged, yMinChanged, xMaxChanged + 1, yMaxChanged + 1);

					int delta = width * bytesPerPixel;

					bitmap.Lock();
					bitmap.WritePixels(maskingRect, pixelBytes, delta,
						((yMinChanged * width) + xMinChanged) * bytesPerPixel);
					bitmap.Unlock();
				}
				else if (mask == Mask.PerPixel)
				{
					Int32Rect maskingRect = new Int32Rect(0, 0, width, height);

					for (int x = xMinChanged; x <= xMaxChanged; x++)
					{
						for (int y = yMinChanged; y <= yMaxChanged; y++)
						{
							if (!changed[x][y])
								continue;

							bitmap.Lock();
							//bitmap.WritePixels(maskingRect, pixelBytes, width * bytesPerPixel, 0);
							bitmap.Unlock();
						}
					}
				}

				anythingChanged = false;
				xMinChanged = width - 1;
				xMaxChanged = 0;
				yMinChanged = height - 1;
				yMaxChanged = 0;
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						changed[x][y] = false;
					}
				}

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
				//R[x] = new double[height];
				//G[x] = new double[height];
				//B[x] = new double[height];
				//A[x] = new double[height];
				//changed[x] = new bool[height];

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

