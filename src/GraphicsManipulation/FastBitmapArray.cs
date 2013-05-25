using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		/// <summary>
		/// Contains red channel of the whole bitmap.
		/// </summary>
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

		/// <summary>
		/// Contains green channel of the whole bitmap.
		/// </summary>
		public ReadOnlyCollection<ReadOnlyCollection<double>> Green
		{
			get
			{
				if (G == null)
					return null;
				ReadOnlyCollection<double>[] result = new ReadOnlyCollection<double>[width];
				for (int x = 0; x < width; x++)
				{
					result[x] = new ReadOnlyCollection<double>(G[x]);
				}
				return new ReadOnlyCollection<ReadOnlyCollection<double>>(result);
			}
		}
		private double[][] G;

		/// <summary>
		/// Contains blue channel of the whole bitmap.
		/// </summary>
		public ReadOnlyCollection<ReadOnlyCollection<double>> Blue
		{
			get
			{
				if (B == null)
					return null;
				ReadOnlyCollection<double>[] result = new ReadOnlyCollection<double>[width];
				for (int x = 0; x < width; x++)
				{
					result[x] = new ReadOnlyCollection<double>(B[x]);
				}
				return new ReadOnlyCollection<ReadOnlyCollection<double>>(result);
			}
		}
		private double[][] B;

		/// <summary>
		/// Contains alpha channel of the whole bitmap.
		/// </summary>
		public ReadOnlyCollection<ReadOnlyCollection<double>> Alpha
		{
			get
			{
				if (A == null)
					return null;
				ReadOnlyCollection<double>[] result = new ReadOnlyCollection<double>[width];
				for (int x = 0; x < width; x++)
				{
					result[x] = new ReadOnlyCollection<double>(A[x]);
				}
				return new ReadOnlyCollection<ReadOnlyCollection<double>>(result);
			}
		}
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

		public FastBitmapArray(int width, int height,
			double backgroundRed, double backgroundGreen, double backgroundBlue)
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
					R[x][y] = backgroundRed;
					G[x][y] = backgroundGreen;
					B[x][y] = backgroundBlue;
					A[x][y] = 1.0;
					changed[x][y] = true;
				}
			}

			// bitmap allocation
			bitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32, null);
			bytesPerPixel = 4;
			pixelBytes = new byte[height * width * bytesPerPixel];

			// indication that the whole bitmap has changed
			xMinChanged = 0;
			xMaxChanged = width - 1;
			yMinChanged = 0;
			yMaxChanged = height - 1;
			anythingChanged = true;
		}

		/// <summary>
		/// Constructs a new empty array. All pixels have color values equal to 0.0,
		/// and alpha equal to 1.0.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public FastBitmapArray(int width, int height)
			: this(width, height, 0.0, 0.0, 0.0)
		{
		}

		/// <summary>
		/// Copies the source bitmap.
		/// </summary>
		/// <param name="source"></param>
		public FastBitmapArray(BitmapSource source)
			: this(source.PixelWidth, source.PixelHeight, 1.0, 1.0, 1.0)
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

			xMinChanged = width - 1;
			xMaxChanged = 0;
			yMinChanged = height - 1;
			yMaxChanged = 0;
			anythingChanged = false;
		}

		/// <summary>
		/// Copies the source array.
		/// </summary>
		/// <param name="source"></param>
		public FastBitmapArray(FastBitmapArray source)
			: this(source.width, source.height)
		{
			CopyAreaBatch(source);
		}

		#region area copy

		/// <summary>
		/// Copies the whole source array into the current array without updating the changes array.
		/// </summary>
		/// <param name="source"></param>
		public void CopyAreaBatch(FastBitmapArray source)
		{
			CopyAreaBatch(source, 0, 0, source.width - 1, source.height - 1);
		}

		/// <summary>
		/// Copies the whole source array into the current array and updates the array with changes.
		/// </summary>
		/// <param name="source"></param>
		public void CopyArea(FastBitmapArray source)
		{
			CopyArea(source, 0, 0, source.width - 1, source.height - 1);
		}

		/// <summary>
		/// Copies the given area of source array into the same area of current array without updating the changes array.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="minX"></param>
		/// <param name="minY"></param>
		/// <param name="maxX"></param>
		/// <param name="maxY"></param>
		public void CopyAreaBatch(FastBitmapArray source, int minX, int minY, int maxX, int maxY)
		{
			if (minX < 0)
				minX = 0;
			if (minY < 0)
				minY = 0;
			if (maxX >= width)
				maxX = width - 1;
			if (maxY >= height)
				maxY = height - 1;

			for (int x = minX; x <= maxX; x++)
			{
				for (int y = minY; y <= maxY; y++)
				{
					R[x][y] = source.R[x][y];
					G[x][y] = source.G[x][y];
					B[x][y] = source.B[x][y];
					A[x][y] = source.A[x][y];
				}
			}
		}

		/// <summary>
		/// Copies the given area of source array into the same area of current array and updates the array with changes.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="minX"></param>
		/// <param name="minY"></param>
		/// <param name="maxX"></param>
		/// <param name="maxY"></param>
		public void CopyArea(FastBitmapArray source, int minX, int minY, int maxX, int maxY)
		{
			CopyAreaBatch(source, minX, minY, maxX, maxY);
			SetBatchArea(minX, minY, maxX, maxY);
		}

		#endregion

		#region pixel setters/getters

		/// <summary>
		/// Sets all or some channels of the pixel at given point and updates the array with changes.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		/// <param name="alpha"></param>
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

		/// <summary>
		/// Sets all channels of the pixel at given point and updates the array with changes.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		public void SetPixel(int x, int y, double red, double green, double blue)
		{
			R[x][y] = red;
			G[x][y] = green;
			B[x][y] = blue;
		}

		/// <summary>
		/// Gets red channel value from given point.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public double GetRed(int x, int y)
		{
			return R[x][y];
		}

		/// <summary>
		/// Gets green channel value from given point.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public double GetGreen(int x, int y)
		{
			return G[x][y];
		}

		/// <summary>
		/// Gets blue channel value from given point.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public double GetBlue(int x, int y)
		{
			return B[x][y];
		}

		/// <summary>
		/// Gets alpha channel value from given point.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public double GetAlpha(int x, int y)
		{
			return A[x][y];
		}

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

		/// <summary>
		/// Sets the red channel value at given point and updates the array with changes.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetRed(int x, int y, double value)
		{
			Change(x, y);
			R[x][y] = value;
		}

		/// <summary>
		/// Sets the green channel value at given point and updates the array with changes.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetGreen(int x, int y, double value)
		{
			Change(x, y);
			G[x][y] = value;
		}

		/// <summary>
		/// Sets the blue channel value at given point and updates the array with changes.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetBlue(int x, int y, double value)
		{
			Change(x, y);
			B[x][y] = value;
		}

		/// <summary>
		/// Sets the alpha channel value at given point and updates the array with changes.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetAlpha(int x, int y, double value)
		{
			Change(x, y);
			A[x][y] = value;
		}

		#endregion

		#region batched setting

		/// <summary>
		/// Sets the whole array to 'changed' status, affecting future bitmap creation.
		/// </summary>
		public void SetBatchArea()
		{
			SetBatchArea(0, 0, width - 1, height - 1);
		}

		/// <summary>
		/// Sets the status of given area to 'changed', affecting future bitmap creation.
		/// </summary>
		/// <param name="minX"></param>
		/// <param name="minY"></param>
		/// <param name="maxX"></param>
		/// <param name="maxY"></param>
		public void SetBatchArea(int minX, int minY, int maxX, int maxY)
		{
			if (minX < 0)
				minX = 0;
			if (minY < 0)
				minY = 0;
			if (maxX >= width)
				maxX = width - 1;
			if (maxY >= height)
				maxY = height - 1;

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

		public void SetBatchArea(Point2D leftTop, Point2D rightBottom)
		{
			SetBatchArea(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
		}

		/// <summary>
		/// Sets the red channel value at given point without updating the changes array.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetRedBatch(int x, int y, double value)
		{
			R[x][y] = value;
		}

		/// <summary>
		/// Sets the green channel value at given point without updating the changes array.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetGreenBatch(int x, int y, double value)
		{
			G[x][y] = value;
		}

		/// <summary>
		/// Sets the blue channel value at given point without updating the changes array.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetBlueBatch(int x, int y, double value)
		{
			B[x][y] = value;
		}

		/// <summary>
		/// Sets the alpha channel value at given point without updating the changes array.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="value"></param>
		public void SetAlphaBatch(int x, int y, double value)
		{
			A[x][y] = value;
		}

		/// <summary>
		/// Sets all or some channels of the pixel at given point without updating the changes array.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		/// <param name="alpha"></param>
		public void SetPixelBatch(int x, int y, double red, double green, double blue, double? alpha = null)
		{
			R[x][y] = red;
			G[x][y] = green;
			B[x][y] = blue;
			if (alpha != null)
				A[x][y] = alpha.Value;
		}

		public void SetPixelBatch(int x, int y, Color3Ch color)
		{
			SetPixelBatch(x, y, color.Red, color.Green, color.Blue);
		}

		#endregion

		/// <summary>
		/// Checks if given point lays on this bitmap array.
		/// </summary>
		/// <param name="x">horizontal coordinate</param>
		/// <param name="y">vertical coordinate</param>
		/// <returns>true if given coordinates denote a point inside this bitmap</returns>
		public bool IsInBounds(int x, int y)
		{
			return x >= 0 && x < width && y >= 0 && y < height;
		}

		/// <summary>
		/// Draws the line that has given parameters, using Bresenham's line algorithm.
		/// </summary>
		/// <param name="xStart"></param>
		/// <param name="yStart"></param>
		/// <param name="xEnd"></param>
		/// <param name="yEnd"></param>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		/// <param name="thickness"></param>
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

				// TODO: thickness handling below is not optimal
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
			{
				if (IsInBounds(x, y))
					SetPixelBatch(x, y, red, green, blue);
			}
			else if (IsInBounds(y, x))
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
				{
					if (IsInBounds(x, y))
						SetPixelBatch(x, y, red, green, blue);
				}
				else if (IsInBounds(y, x))
					SetPixelBatch(y, x, red, green, blue);

			}
		}

		public void DrawLine(Point2D start, Point2D end,
			Color3Ch color, int thickness = 1)
		{
			DrawLine(start.X, start.Y, end.X, end.Y, color.Red, color.Green, color.Blue, thickness);
		}

		public void Fill(Color3Ch color)
		{
			//DrawRectangle(Point2D.Zero, new Point2D(width - 1, height - 1), color);
			Change(0, 0);
			Change(width - 1, height - 1);
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
				{
					R[x][y] = color.Red;
					G[x][y] = color.Green;
					B[x][y] = color.Blue;
				}
		}

		public void DrawRectangle(Point2D leftTop, Point2D rightBottom, Color3Ch color)
		{
			Change(leftTop.X, leftTop.Y);
			Change(rightBottom.X, rightBottom.Y);
			for (int x = leftTop.X; x <= rightBottom.X; x++)
				for (int y = leftTop.Y; y <= rightBottom.Y; y++)
				{
					R[x][y] = color.Red;
					G[x][y] = color.Green;
					B[x][y] = color.Blue;
				}
		}

		public void Fill(Point2D from, Color3Ch color)
		{
			throw new NotImplementedException("use polygon with scan-line fill");
		}

		public void FillPolygon(Polygon polygon, Color3Ch[][] pattern)
		{
			if (polygon == null || polygon.PointsCount == 0)
				return;

			var points = polygon.Points.ToPoints2D();
			var max = polygon.PointsCount - 1;
			int minx;
			int maxx;

			IList<Tuple<Point2D, Point2D>> edges = PointsToEdges(points, out minx, out maxx);

			Change(Math.Max(minx, 0), Math.Max(edges[0].Item1.Y, 0));
			Change(Math.Min(maxx, width - 1), Math.Min(edges[max].Item2.Y, height - 1));

			bool[] active;
			double[] ratios;
			double[] xs;

			InitializeClipSupport(edges, out active, out ratios, out xs);

			int minActive = edges.Count;
			int maxActive = 0;

			int patternWidth = pattern[0].Length;
			int patternHeight = pattern.Length;
			int patternX = 0;
			int patternY = edges[0].Item1.Y % patternHeight;

			for (int y = Math.Max(edges[0].Item1.Y, 0); y < height; ++y)
			{
				bool noneActive = true;
				for (int i = 0; i < edges.Count; ++i)
				{
					// modify coordinates of active x coordinates
					if (active[i])
						xs[i] += ratios[i];

					// activate/deactivate edges
					active[i] = y >= edges[i].Item1.Y && y < edges[i].Item2.Y;
					if (!active[i])
						continue;

					// change active indices range
					noneActive = false;
					if (i < minActive)
						minActive = i;
					if (i > maxActive)
						maxActive = i;
				}
				if (noneActive)
					break;

				// compose a list of x coordinates of current borders
				List<int> xx = new List<int>();
				for (int i = minActive; i <= maxActive; ++i)
					if (active[i])
					{
						int j;
						for (j = 0; j < xx.Count; ++j)
							if (xx[j] >= xs[i])
								break;
						xx.Insert(j, Math.Max((int)Math.Round(xs[i]), 0));
					}

				#region draw row
				if (xx[0] < width)
				{
					bool isDrawing = true;
					int xchecked = 1;

					patternX = xx[0] % pattern[0].Length;
					if ((xx.Count == 1 && xx[0] > 0) || (xx.Count > 1 && xx[1] > 0))
					{
						R[xx[0]][y] = pattern[patternX][patternY].Red;
						G[xx[0]][y] = pattern[patternX][patternY].Green;
						B[xx[0]][y] = pattern[patternX][patternY].Blue;
					}

					for (int x = xx[0] + 1; x < width; ++x)
					{
						++patternX;
						if (patternX == patternWidth)
							patternX = 0;

						if (x >= xx[xchecked])
						{
							++xchecked;
							isDrawing = !isDrawing;
						}

						if (isDrawing)
						{
							R[x][y] = pattern[patternX][patternY].Red;
							G[x][y] = pattern[patternX][patternY].Green;
							B[x][y] = pattern[patternX][patternY].Blue;
						}
						else
						{
							if (xchecked == xx.Count)
								break;
							x = xx[xchecked] - 1;
							patternX = x % pattern[0].Length;
						}
					}
				}
				#endregion

				++patternY;
				if (patternY == patternHeight)
					patternY = 0;
			}
		}

		public void FillPolygon(Polygon polygon, Color3Ch color)
		{
			if (polygon == null || polygon.PointsCount == 0)
				return;

			var points = polygon.Points.ToPoints2D();
			var max = polygon.PointsCount - 1;
			int minx;
			int maxx;

			IList<Tuple<Point2D, Point2D>> edges = PointsToEdges(points, out minx, out maxx);

			Change(Math.Max(minx, 0), Math.Max(edges[0].Item1.Y, 0));
			Change(Math.Min(maxx, width - 1), Math.Min(edges[max].Item2.Y, height - 1));

			bool[] active;
			double[] ratios;
			double[] xs;

			InitializeClipSupport(edges, out active, out ratios, out xs);

			int minActive = edges.Count;
			int maxActive = 0;

			for (int y = Math.Max(edges[0].Item1.Y, 0); y < height; ++y)
			{
				bool noneActive = true;
				for (int i = 0; i < edges.Count; ++i)
				{
					// modify coordinates of active x coordinates
					if (active[i])
						xs[i] += ratios[i];

					// activate/deactivate edges
					active[i] = y >= edges[i].Item1.Y && y < edges[i].Item2.Y;
					if (!active[i])
						continue;

					// change active indices range
					noneActive = false;
					if (i < minActive)
						minActive = i;
					if (i > maxActive)
						maxActive = i;
				}
				if (noneActive)
					break;

				// compose a list of x coordinates of current borders
				List<int> xx = new List<int>();
				for (int i = minActive; i <= maxActive; ++i)
					if (active[i])
					{
						int j;
						for (j = 0; j < xx.Count; ++j)
							if (xx[j] >= xs[i])
								break;
						xx.Insert(j, Math.Max((int)Math.Round(xs[i]), 0));
					}

				#region draw row
				if (xx[0] < width)
				{
					bool isDrawing = true;
					int xchecked = 1;

					if ((xx.Count == 1 && xx[0] > 0) || (xx.Count > 1 && xx[1] > 0))
					{
						R[xx[0]][y] = color.Red;
						G[xx[0]][y] = color.Green;
						B[xx[0]][y] = color.Blue;
					}

					for (int x = xx[0] + 1; x < width; ++x)
					{
						if (x >= xx[xchecked])
						{
							++xchecked;
							isDrawing = !isDrawing;
						}

						if (isDrawing)
						{
							R[x][y] = color.Red;
							G[x][y] = color.Green;
							B[x][y] = color.Blue;
						}
						else
						{
							if (xchecked == xx.Count)
								break;
							x = xx[xchecked] - 1;
						}
					}
				}
				#endregion
			}
		}

		private void InitializeClipSupport(IList<Tuple<Point2D, Point2D>> edges,
			out bool[] active, out double[] ratios, out double[] xs)
		{
			active = new bool[edges.Count];
			ratios = new double[edges.Count];
			xs = new double[edges.Count];

			//Parallel.For(0, edges.Count, (int i) =>
			for (int i = 0; i < edges.Count; ++i)
			{
				active[i] = false;
				ratios[i] = ((double)(edges[i].Item2.X - edges[i].Item1.X)) / (edges[i].Item2.Y - edges[i].Item1.Y);
				xs[i] = edges[i].Item1.X;
				if (edges[i].Item1.Y < 0)
					xs[i] += ratios[i] * edges[i].Item1.Y;
			}
			//);

		}

		public void DrawPolygon(Polygon polygon, Color3Ch color, int lineThickness)
		{
			if (polygon == null || polygon.PointsCount == 0)
				return;

			var points = polygon.Points.ToPoints2D();
			var max = polygon.PointsCount - 1;

			for (int i = 0; i < max; ++i)
			{
				var pt1 = points[i];
				var pt2 = points[i + 1];

				DrawLine(pt1, pt2, color, lineThickness);
			}
			if (max > 0)
				DrawLine(points[max], points[0], color, lineThickness);
			else
				DrawLine(points[0], new Point2D(points[0].X + 1, points[0].Y), color, lineThickness);
		}

		private IList<Tuple<Point2D, Point2D>> PointsToEdges(IList<Point2D> points, out int minx, out int maxx)
		{
			var count = points.Count;
			var max = count - 1;
			IList<Tuple<Point2D, Point2D>> edges = new List<Tuple<Point2D, Point2D>>(count);

			if (points[max].Y > points[0].Y)
				edges.Add(new Tuple<Point2D, Point2D>(points[0], points[max]));
			else
				edges.Add(new Tuple<Point2D, Point2D>(points[max], points[0]));

			minx = width - 1;
			maxx = 0;

			for (int i = 0; i < max; ++i)
			{
				bool invert = points[i].Y > points[i + 1].Y;
				var pt1 = invert ? points[i + 1] : points[i];
				var pt2 = invert ? points[i] : points[i + 1];

				int tempx = Math.Min(pt1.X, pt2.X);
				if (tempx < minx) minx = tempx;
				tempx = Math.Max(pt1.X, pt2.X);
				if (tempx > maxx) maxx = tempx;

				int k;
				for (k = 0; k < edges.Count; ++k)
					if (edges[k].Item1.Y > pt1.Y)
						break;

				edges.Insert(k, new Tuple<Point2D, Point2D>(pt1, pt2));
			}

			return edges;
		}

		//public void DrawPolygon(Polygon polygon, Color3Ch color, bool fill, int lineThickness = 0)
		//{
		//	if (polygon == null || polygon.PointsCount == 0)
		//		return;

		//	var points = polygon.Points.ToPoints2D();
		//	var max = polygon.PointsCount - 1;

		//	if (points.Count < 3)
		//	{
		//		color = new Color3Ch(1, 1, 0);
		//		lineThickness += 2;
		//		fill = false;
		//	}

		//	if (!fill || lineThickness > 0)
		//	{
		//		// simple version
		//		for (int i = 0; i < max; ++i)
		//		{
		//			var pt1 = points[i];
		//			var pt2 = points[i + 1];

		//			DrawLine(pt1, pt2, color, lineThickness);
		//		}
		//		if (max > 0)
		//			DrawLine(points[max], points[0], color, lineThickness);
		//		else
		//			DrawLine(points[0], new Point2D(points[0].X + 1, points[0].Y), color, lineThickness);
		//	}

		//	if (!fill)
		//		return;


		//}

		/// <summary>
		/// Draws the given line, using Bresenham's line algorithm.
		/// </summary>
		/// <param name="line"></param>
		public void DrawLine(Line line)
		{
			DrawLine(line.StartX, line.StartY, line.EndX, line.EndY, line.Red, line.Green, line.Blue, line.Thickness);
		}

		public FastBitmapArray ScaleUp(int scaling)
		{
			var scaledUp = new FastBitmapArray(scaling * width, scaling * height);

			int xScaled = 0;
			int yScaled = 0;
			for (int x = 0; x < width; ++x)
			{
				yScaled = 0;
				for (int y = 0; y < height; ++y)
				{
					for (int yy = 0; yy < scaling; ++yy)
						for (int xx = 0; xx < scaling; ++xx)
						{
							int xxx = xScaled + xx;
							int yyy = yScaled + yy;
							scaledUp.R[xxx][yyy] = R[x][y];
							scaledUp.G[xxx][yyy] = G[x][y];
							scaledUp.B[xxx][yyy] = B[x][y];
							scaledUp.A[xxx][yyy] = A[x][y];
							scaledUp.changed[xxx][yyy] = true;
						}
					yScaled += scaling;
				}
				xScaled += scaling;
			}

			return scaledUp;
		}

		public FastBitmapArray ScaleDown(int scaling)
		{
			if (width % scaling > 0 || height % scaling > 0)
				throw new ArgumentException("such bitmap cannot be scaled down");
			var scaledDown = new FastBitmapArray(width / scaling, height / scaling);

			double divisor = scaling * scaling;

			int xScaled = 0;
			int yScaled = 0;
			for (int x = 0; x < scaledDown.width; ++x)
			{
				yScaled = 0;
				for (int y = 0; y < scaledDown.height; ++y)
				{
					double avgR = 0, avgG = 0, avgB = 0, avgA = 0;
					for (int yy = 0; yy < scaling; ++yy)
						for (int xx = 0; xx < scaling; ++xx)
						{
							int xxx = xScaled + xx;
							int yyy = yScaled + yy;
							avgR += R[xxx][yyy];
							avgG += G[xxx][yyy];
							avgB += B[xxx][yyy];
							avgA += A[xxx][yyy];
						}

					scaledDown.R[x][y] = avgR / divisor;
					scaledDown.G[x][y] = avgG / divisor;
					scaledDown.B[x][y] = avgB / divisor;
					scaledDown.A[x][y] = avgA / divisor;
					scaledDown.changed[x][y] = true;

					yScaled += scaling;
				}
				xScaled += scaling;
			}

			return scaledDown;
		}

		/// <summary>
		/// Refreshes the underlying bitmap using changes array, and applying given mask shape.
		/// </summary>
		/// <param name="mask"></param>
		public void RefreshBitmap(Mask mask = Mask.Rectangle)
		{
			if (mask == Mask.Disabled)
			{
				//mask is disabled, therefore the whole image will be repainted
				Int32Rect maskingRect = new Int32Rect(0, 0, width, height);

				WriteToPixelBytes();

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
					int delta = xSize * bytesPerPixel - 3;

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
							pixelsRect[index++] = (byte)(MaxByteValue * B[xx][yy]);
							pixelsRect[index++] = (byte)(MaxByteValue * G[xx][yy]);
							pixelsRect[index++] = (byte)(MaxByteValue * R[xx][yy]);
							pixelsRect[index] = (byte)(MaxByteValue * A[xx][yy]);
							index += delta;

							changed[xx][yy] = false;
						}
					}

					bitmap.Lock();
					bitmap.WritePixels(maskingRect, pixelsRect, delta + 3, 0);
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
					pixelBytes[index + 2] = (byte)(MaxByteValue * R[x][y]);
					pixelBytes[index + 1] = (byte)(MaxByteValue * G[x][y]);
					pixelBytes[index + 0] = (byte)(MaxByteValue * B[x][y]);
					pixelBytes[index + 3] = (byte)(MaxByteValue * A[x][y]);
					index += delta;
				}
			}
		}

		private void WriteToPixelBytes()
		{
			int index = -1;
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
				{
					// Bgra32
					pixelBytes[++index] = (byte)(MaxByteValue * B[x][y]);
					pixelBytes[++index] = (byte)(MaxByteValue * G[x][y]);
					pixelBytes[++index] = (byte)(MaxByteValue * R[x][y]);
					pixelBytes[++index] = (byte)(MaxByteValue * A[x][y]);
				}
		}

		/// <summary>
		/// Refreshes and returns the underlying bitmap.
		/// </summary>
		/// <param name="mask"></param>
		/// <returns></returns>
		public BitmapSource GetBitmap(Mask mask = Mask.Rectangle)
		{
			RefreshBitmap(mask);
			//bitmap.Freeze();
			//return (BitmapSource)bitmap.GetCurrentValueAsFrozen();
			return bitmap;
		}

	}

}
