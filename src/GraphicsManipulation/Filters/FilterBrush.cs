using System.Windows;

namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Filters the bitmap (or its part) pixel by pixel using a function defined via 
	/// implementation of abstract methods.
	/// </summary>
	public abstract class FilterBrush
	{
		public FilterBrush() { }

		public void Apply(FastBitmapArray array, bool[][] mask)
		{
			ApplyAt(array, BrushShapes.Fill, new Point(array.Width / 2, array.Width / 2), 20, mask);
		}

		/// <summary>
		/// Applies the filter at given point of a given FastBitmapArray, using given shape of given size.
		/// </summary>
		/// <param name="array">array on which the filter will be applied</param>
		/// <param name="shape">shape of the brush</param>
		/// <param name="point">central point of the brush</param>
		/// <param name="size"> diameter of the brush</param>
		/// <param name="mask">masking array</param>
		public void ApplyAt(FastBitmapArray array, BrushShapes shape, Point point, int size,
			 bool[][] mask)
		{
			switch (shape)
			{
				case BrushShapes.Fill: 
					Fill(array, mask);
					break;
				case BrushShapes.Square: 
					PaintRect(array, new Point(point.X - size / 2, point.Y - size / 2), size, size, mask);
					break;
				case BrushShapes.Circle:
					PaintCircle(array, point, size, mask);
					break;
			}
		}
		/// <summary>
		/// Applies this filter on the whole given FastBitmapArray.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="mask"></param>
		protected void Fill(FastBitmapArray array, bool[][] mask)
		{
			PaintRect(array, new Point(0, 0), array.Width, array.Height, mask);
		}

		/// <summary>
		/// Applies this filter in a given circular area of the given FastBitmapArray.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="point"></param>
		/// <param name="diameter"></param>
		/// <param name="mask"></param>
		protected void PaintCircle(FastBitmapArray array, Point point,
			 int diameter, bool[][] mask)
		{
			int centerX = (int)point.X;
			int centerY = (int)point.Y;
			double radius = diameter / 2;
			int x = centerX - (int)radius;
			int y = centerY - (int)radius;
			int xEnd = x + diameter;
			int yEnd = y + diameter;
			bool inBounds = false;

			array.SetBatchArea(x, y, xEnd - 1, yEnd - 1);
			for (int i = x; i < xEnd; i++)
			{
				for (int j = y; j < yEnd; j++)
				{
					inBounds = i >= 0 && i < array.Width
						 && j >= 0 && j < array.Height
						 && (i - centerX) * (i - centerX)
							  + (j - centerY) * (j - centerY) <= radius * radius;

					if (inBounds && mask[i][j])
					{
						FilterWithCorrection(array, i, j);

						mask[i][j] = false;
					}
				}
			}
		}

		/// <summary>
		/// Applies this filter in a given rectangular area of the given FastBitmapArray.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="point"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="mask"></param>
		protected void PaintRect(FastBitmapArray array, Point point,
			 int width, int height, bool[][] mask)
		{
			int x = (int)point.X;
			int y = (int)point.Y;
			int xEnd = x + width;
			int yEnd = y + height;
			bool inBounds = false;

			array.SetBatchArea(x, y, xEnd - 1, yEnd - 1);
			for (int i = x; i < xEnd; i++)
			{
				for (int j = y; j < yEnd; j++)
				{
					inBounds = i >= 0 && i < array.Width
						 && j >= 0 && j < array.Height;

					if (inBounds && mask[i][j])
					{
						FilterWithCorrection(array, i, j);

						mask[i][j] = false;
					}
				}
			}
		}

		private void FilterWithCorrection(FastBitmapArray /*bitmap*/array, int x, int y)
		{
			double red = array.GetRed(x, y);
			double green = array.GetGreen(x, y);
			double blue = array.GetBlue(x, y);

			double redNew = FilterRed(red, green, blue);
			double greenNew = FilterGreen(red, green, blue);
			double blueNew = FilterBlue(red, green, blue);

			if (redNew < 0) redNew = 0;
			else if (redNew > 1) redNew = 1;
			if (greenNew < 0) greenNew = 0;
			else if (greenNew > 1) greenNew = 1;
			if (blueNew < 0) blueNew = 0;
			else if (blueNew > 1) blueNew = 1;

			array.SetPixelBatch(x, y, redNew, greenNew, blueNew);
		}

		/// <summary>
		/// Filters the red component.
		/// </summary>
		/// <param name="r">red component value, from 0 to 1</param>
		/// <param name="g">green component value, from 0 to 1</param>
		/// <param name="b">blue component value, from 0 to 1</param>
		/// <returns>updated value of the red component, from 0 to 1</returns>
		protected abstract double FilterRed(double r, double g, double b);

		/// <summary>
		/// Filters the green component.
		/// </summary>
		/// <param name="r">red component value, from 0 to 1</param>
		/// <param name="g">green component value, from 0 to 1</param>
		/// <param name="b">blue component value, from 0 to 1</param>
		/// <returns>updated value of the green component, from 0 to 1</returns>
		protected abstract double FilterGreen(double r, double g, double b);

		/// <summary>
		/// Filters the blue component.
		/// </summary>
		/// <param name="r">red component value, from 0 to 1</param>
		/// <param name="g">green component value, from 0 to 1</param>
		/// <param name="b">blue component value, from 0 to 1</param>
		/// <returns>updated value of the blue component, from 0 to 1</returns>
		protected abstract double FilterBlue(double r, double g, double b);
	}
}
