using System.Windows;

namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Filters the bitmap (or its part) pixel by pixel using a function defined via 
	/// implementation of abstract methods.
	/// </summary>
	public abstract class FilterBrush
	{
		//private BrushShapes shape;
		//public BrushShapes Shape
		//{
		//    set { shape = value; }
		//}

		public FilterBrush()
		{
		}

		//public FilterBrush(BrushShapes shape)
		//{
		//    this.shape = shape;
		//}

		public void Apply(FastBitmapArray /*bitmap*/array, bool[][] mask)
		{
			ApplyAt(array, BrushShapes.Fill, new Point(array.Width / 2, array.Width / 2),
				 20, mask);
		}

		public void ApplyAt(FastBitmapArray /*bitmap*/array, BrushShapes shape, Point point, int size,
			 bool[][] mask)
		{
			if (shape == BrushShapes.Fill)
				Fill(array, mask);
			else if (shape == BrushShapes.Square)
				PaintRect(array, new Point(point.X - size / 2, point.Y - size / 2),
					 size, size, mask);
			else if (shape == BrushShapes.Circle)
				PaintCircle(array, point, size, mask);
		}

		protected void Fill(FastBitmapArray /*bitmap*/array, bool[][] mask)
		{
			PaintRect(array, new Point(0, 0), array.Width, array.Height, mask);
		}

		protected void PaintCircle(FastBitmapArray /*bitmap*/array, Point point,
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

		protected void PaintRect(FastBitmapArray /*bitmap*/array, Point point,
			 int width, int height, bool[][] mask)
		{
			int x = (int)point.X;
			int y = (int)point.Y;
			int xEnd = x + width;
			int yEnd = y + height;
			bool inBounds = false;

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
			//double red = ((double)bitmap.Red[x][y]) / 255;
			//double green = ((double)bitmap.Green[x][y]) / 255;
			//double blue = ((double)bitmap.Blue[x][y]) / 255;

			//if (red < 0) red = 0;
			//else if (red > 1) red = 1;
			//if (green < 0) green = 0;
			//else if (green > 1) green = 1;
			//if (blue < 0) blue = 0;
			//else if (blue > 1) blue = 1;

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

			array.SetRed(x, y, redNew);
			array.SetGreen(x, y, greenNew);
			array.SetBlue(x, y, blueNew);
			//bitmap.Red[x][y] = (byte)(redNew * 255);
			//bitmap.Green[x][y] = (byte)(greenNew * 255);
			//bitmap.Blue[x][y] = (byte)(blueNew * 255);
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
