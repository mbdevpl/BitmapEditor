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

	}
}
