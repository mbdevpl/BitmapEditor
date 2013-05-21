using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsManipulation
{
	public class Point2D
	{

		public static readonly Point2D Zero = new Point2D(0, 0);

		public int X;

		public int Y;

		public Point2D() { X = 0; Y = 0; }

		public Point2D(int x, int y) { X = x; Y = y; }

		public override string ToString()
		{
			return String.Format("({0},{1})", X, Y);
		}

		public override bool Equals(object obj)
		{
			if (obj is Point2D == false)
				return false;
			Point2D point = (Point2D)obj;
			return X == point.X && Y == point.Y;
		}

		public override int GetHashCode()
		{
			return 7 * X.GetHashCode() + 11 * Y.GetHashCode() + base.GetHashCode();
		}

	}
}
