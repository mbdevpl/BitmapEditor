using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MBdev.Extensions;

namespace GraphicsManipulation
{
	public static class Extensions
	{
		public static IList<Point2D> Copy(this IList<Point2D> points)
		{
			IList<Point2D> copy = new List<Point2D>();

			foreach (var pt in points)
				copy.Add(new Point2D(pt.X, pt.Y));

			return copy;
		}

		public static void Offset(this IList<Point2D> points, Point2D offset)
		{
			foreach (var pt in points)
			{
				pt.X += offset.X;
				pt.Y += offset.Y;
			}
		}

		public static IList<Point> ToPoints(this IList<Point2D> points)
		{
			IList<Point> copy = new List<Point>();

			foreach (var pt in points)
				copy.Add(new Point(pt.X, pt.Y));

			return copy;
		}

		public static IList<Point2D> ToPoints2D(this IList<Point> points)
		{
			IList<Point2D> copy = new List<Point2D>();

			foreach (var pt in points)
				copy.Add(new Point2D((int)Math.Round(pt.X), (int)Math.Round(pt.Y)));

			return copy;
		}

	}
}
