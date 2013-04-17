using System;
using System.Windows;
using System.Windows.Media;

namespace GraphicsManipulation
{
	public class Line
	{
		public int StartX;
		public int StartY;
		public int EndX;
		public int EndY;

		public double Red;
		public double Green;
		public double Blue;

		public int Thickness;

		public string Start { get { return String.Format("({0},{1})", StartX, StartY); } }
		public string End { get { return String.Format("({0},{1})", EndX, EndY); } }
		public string Width { get { return String.Format("{0}", Thickness); } }
		public string RGB
		{
			get
			{
				return String.Format("{0},{1},{2}", Math.Round(Red, 2), Math.Round(Green, 2), Math.Round(Blue, 2));
			}
		}

		public Line(int startX, int startY, int endX, int endY,
			double red, double green, double blue, int thickness)
		{
			StartX = startX;
			StartY = startY;
			EndX = endX;
			EndY = endY;
			Red = red;
			Green = green;
			Blue = blue;
			Thickness = thickness;
		}

		public LineGeometry ToLineGeometry()
		{
			return new LineGeometry(new Point(StartX, StartY), new Point(EndX, EndY));
		}

		public override string ToString()
		{
			return String.Format("({0},{1})->({2},{3})", StartX, StartY, EndX, EndY);
		}

	}

}
