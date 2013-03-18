using System.Collections.Generic;
using System.Windows;

namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Enables application of a completely arbitrary filter.
	/// </summary>
	public class CustomFilter : FilterBrush
	{
		private static int ComparePointsByX(Point pt1, Point pt2)
		{
			if (pt1.X < pt2.X)
				return -1;
			if (pt1.X > pt2.X)
				return 1;
			return 0;
		}

		public Point DefaultStart = new Point(0, 0);

		public Point DefaultEnd = new Point(1, 1);

		public IEnumerable<Point> FilterFunction
		{
			set
			{
				filterFunction = new List<Point>(value);
				filterFunction.Sort(ComparePointsByX);
			}
			get { return filterFunction; }
		}
		private List<Point> filterFunction;

		public bool AddFilterFunctionPoint(Point point)
		{
			if (point.X < 0 || point.X > 1 || point.Y < 0 || point.Y > 1)
				return false;

			if (filterFunction == null)
			{
				filterFunction = new List<Point>();
				filterFunction.Add(point);
				return true;
			}

			if (filterFunction.FindAll(pt => pt.X == point.X).Count > 0)
				return false;

			// inserting the point into the list so that it remains sorted
			int i = 0;
			for (; i < filterFunction.Count; ++i)
			{
				if (filterFunction[i].X > point.X)
					break;
			}
			filterFunction.Insert(i, point);

			return true;
		}

		/// <summary>
		/// Gets exact value of the filter function using interpolation.
		/// </summary>
		/// <param name="x">x coordinate of the point</param>
		/// <returns>y coordinate of a hypothetical point</returns>
		private double GetExactValue(double x)
		{
			// finding exact matches
			var points = filterFunction.FindAll(pt => pt.X == x);
			if (points.Count > 0)
				return points[0].Y;
			points = null;

			// interpolation
			Point pt1 = DefaultStart, pt2 = DefaultEnd;
			bool foundGreater = false;
			for (int i = 0; i < filterFunction.Count; ++i)
				if (filterFunction[i].X > x)
				{
					pt2 = filterFunction[i];
					foundGreater = true;
					if (i > 0)
						pt1 = filterFunction[i - 1];
					break;
				}
			if (!foundGreater && filterFunction.Count > 0)
				pt1 = filterFunction[filterFunction.Count - 1];

			double distX = pt2.X - pt1.X;
			double distY = pt2.Y - pt1.Y;

			double percentX = (x - pt1.X) / distX; // from 0 to 1

			// final result is:
			// value of earlier point
			// +
			// percentage of the value distance to the later point
			return percentX * distY + pt1.Y;
		}

		protected override double FilterRed(double r, double g, double b)
		{
			return GetExactValue(r);
		}

		protected override double FilterGreen(double r, double g, double b)
		{
			return GetExactValue(g);
		}

		protected override double FilterBlue(double r, double g, double b)
		{
			return GetExactValue(b);
		}
	}
}
