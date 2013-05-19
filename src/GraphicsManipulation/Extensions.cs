using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GraphicsManipulation
{
	public static class Extensions
	{
		public static int[][] MatrixAdd(this int[][] input, int value)
		{
			int height = input.Length;
			int width = input[0].Length;
			int[][] resultMatrix = new int[height][];

			for (int i = 0; i < height; ++i)
			{
				resultMatrix[i] = new int[width];
				for (int j = 0; j < width; ++j)
				{
					resultMatrix[i][j] = input[i][j] + value;
				}
			}

			return resultMatrix;
		}

		public static int[][] MatrixMultiply(this int[][] input, int value)
		{
			int height = input.Length;
			int width = input[0].Length;
			int[][] resultMatrix = new int[height][];

			for (int i = 0; i < height; ++i)
			{
				resultMatrix[i] = new int[width];
				for (int j = 0; j < width; ++j)
				{
					resultMatrix[i][j] = input[i][j] * value;
				}
			}

			return resultMatrix;
		}

		public static int[][] MatrixMultiply(this int[][] m1, int[][] m2)
		{
			int height = m1.Length;
			int width = m2[0].Length;
			int[][] resultMatrix = new int[height][];

			int m1Width = m1[0].Length;

			for (int i = 0; i < height; ++i)
			{
				resultMatrix[i] = new int[width];
				for (int j = 0; j < width; ++j)
				{
					resultMatrix[i][j] = 0;
					for (int k = 0; k < m1Width; ++k)
					{
						resultMatrix[i][j] += m1[i][k] * m2[k][j];
					}
				}
			}

			return resultMatrix;
		}

		public static void Offset(this IList<Point2D> polygon, Point2D offset)
		{
			foreach (var pt in polygon)
			{
				pt.X += offset.X;
				pt.Y += offset.Y;
			}
		}

		public static IList<Point2D> Copy(this IList<Point2D> polygon)
		{
			List<Point2D> copy = new List<Point2D>();

			foreach (var pt in polygon)
				copy.Add(new Point2D(pt.X, pt.Y));

			return copy;
		}

		/// <summary>
		/// Makes a copy of this point.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <returns></returns>
		public static Point Copy(this Point thisPoint)
		{
			return new Point(thisPoint.X, thisPoint.Y);
		}

		/// <summary>
		/// Computes cross product of two vectors, AB x AC, where: A = this Point, B = point1, C = point2.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <returns></returns>
		public static double CrossProduct(this Point thisPoint, Point point1, Point point2)
		{
			return (point1.X - thisPoint.X) * (point2.Y - thisPoint.Y)
				- (point1.Y - thisPoint.Y) * (point2.X - thisPoint.X);
		}

		private static bool CheckBoundsX(Point p1, Point p2, Point min, Point max)
		{
			// preliminary bounds are checked outside
			//if (p1.X <= min.X || p1.X >= max.X)
			//	return null;

			double ratio = (max.Y - min.Y) / (max.X - min.X);

			double d1 = max.Y - (max.X - p1.X) * ratio;
			bool below = p1.Y > d1;

			double d2 = max.Y - (max.X - p2.X) * ratio;
			bool above = p2.Y < d2;

			if (below != above)
				return false;

			if (p2.Equals(min) || p2.Equals(max))
			{
				// TODO: case that one endpoint is equal, but other lays on edge
				return false;
			}
			else if (p2.X >= min.X && p2.X <= max.X)
			{
				return true;
			}
			else if (p2.X < min.X)
			{
				double ratioAlt = (p1.Y - min.Y) / (p1.X - min.X);

				double dAlt = p1.Y - (p1.X - p2.X) * ratioAlt;
				bool aboveAlt = p2.Y < dAlt;

				if (below == aboveAlt)
					return true;
				else
					return false;
			}
			else
			{
				double ratioAlt = (max.Y - p1.Y) / (max.X - p1.X);

				double dAlt = max.Y - (max.X - p2.X) * ratioAlt;
				bool aboveAlt = p2.Y < dAlt;

				if (below == aboveAlt)
					return true;
				else
					return false;
			}

			throw new ArgumentException("please checked preliminary bounds outside of this method");
		}

		private static bool CheckBoundsY(Point p1, Point p2, Point min, Point max)
		{
			// TODO: thic can probably be merged with CheckBoundsX

			double ratio = (max.X - min.X) / (max.Y - min.Y);

			double d1 = max.Y - p1.Y;
			bool toRight = p1.X > max.X - d1 * ratio;

			double d2 = max.Y - p2.Y;
			bool toLeft = p2.X < max.X - d2 * ratio;

			if (toRight != toLeft)
				return false;

			if (p2.Equals(min) || p2.Equals(max))
			{
				return false;
			}
			else if (p2.Y >= min.Y && p2.Y <= max.Y)
			{
				return true;
			}
			else if (p2.Y < min.Y)
			{
				double ratioAlt = (p1.X - min.X) / (p1.Y - min.Y);

				double dAlt = p1.Y - p2.Y;
				bool toLeftAlt = p2.X < p1.X - dAlt * ratioAlt;

				if (toRight == toLeftAlt)
					return true;
				else
					return false;
			}
			else
			{
				double ratioAlt = (max.X - p1.X) / (max.Y - p1.Y);

				double dAlt = max.Y - p2.Y;
				bool toLeftAlt = p2.X < max.X - dAlt * ratioAlt;

				if (toRight == toLeftAlt)
					return true;
				else
					return false;
			}

			throw new ArgumentException("please checked preliminary bounds outside of this method");
		}

		private static bool CheckIfIntersects(Point p11, Point p12, Point p21, Point p22)
		{
			//var p1Min = new Point(Math.Min(p11.X, p12.X), Math.Min(p11.Y, p12.Y));
			var p1MinXval = Math.Min(p11.X, p12.X);
			var p1MinYval = Math.Min(p11.Y, p12.Y);

			//var p2Max = new Point(Math.Max(p21.X, p22.X), Math.Max(p21.Y, p22.Y));
			var p2MaxXval = Math.Max(p21.X, p22.X);
			var p2MaxYval = Math.Max(p21.Y, p22.Y);

			if (p2MaxXval < p1MinXval || p2MaxYval < p1MinYval)
				return false;

			//var p1Max = new Point(Math.Max(p11.X, p12.X), Math.Max(p11.Y, p12.Y));
			var p1MaxXval = Math.Max(p11.X, p12.X);
			var p1MaxYval = Math.Max(p11.Y, p12.Y);

			//var p2Min = new Point(Math.Min(p21.X, p22.X), Math.Min(p21.Y, p22.Y));
			var p2MinXval = Math.Min(p21.X, p22.X);
			var p2MinYval = Math.Min(p21.Y, p22.Y);

			if (p2MinXval > p1MaxXval || p2MinYval > p1MaxYval)
				return false;

			// TODO: DRY this code...

			var p1MinX = p1MinXval == p11.X ? p11 : p12;
			var p1MaxX = p1MaxXval == p11.X ? p11 : p12;

			if (p21.X > p1MinX.X && p21.X < p1MaxX.X)
				return CheckBoundsX(p21, p22, p1MinX, p1MaxX);

			if (p22.X > p1MinX.X && p22.X < p1MaxX.X)
				return CheckBoundsX(p22, p21, p1MinX, p1MaxX);

			var p1MinY = p1MinYval == p11.Y ? p11 : p12;
			var p1MaxY = p1MaxYval == p11.Y ? p11 : p12;

			if (p21.Y > p1MinY.Y && p21.Y < p1MaxY.Y)
				return CheckBoundsY(p21, p22, p1MinY, p1MaxY);

			if (p22.Y > p1MinY.Y && p22.Y < p1MaxY.Y)
				return CheckBoundsY(p22, p21, p1MinY, p1MaxY);

			var p2MinX = p2MinXval == p21.X ? p21 : p22;
			var p2MaxX = p2MaxXval == p21.X ? p21 : p22;

			if (p11.X > p2MinX.X && p11.X < p2MaxX.X)
				return CheckBoundsX(p11, p12, p2MinX, p2MaxX);

			if (p12.X > p2MinX.X && p12.X < p2MaxX.X)
				return CheckBoundsX(p12, p11, p2MinX, p2MaxX);

			var p2MinY = p2MinYval == p21.Y ? p21 : p22;
			var p2MaxY = p2MaxYval == p21.Y ? p21 : p22;

			if (p11.Y > p2MinY.Y && p11.Y < p2MaxY.Y)
				return CheckBoundsX(p11, p12, p2MinY, p2MaxY);

			if (p12.Y > p2MinY.Y && p12.Y < p2MaxY.Y)
				return CheckBoundsX(p12, p11, p2MinY, p2MaxY);

			if ((p11.Equals(p21) && p12.Equals(p22)) || (p11.Equals(p22) && p12.Equals(p21)))
				return true;

			return false;
		}

		/// <summary>
		/// Computes the Euclidean distance between points.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		public static double Distance(this Point thisPoint, Point point)
		{
			return Math.Sqrt(Math.Pow(point.X - thisPoint.X, 2)
				+ Math.Pow(point.Y - thisPoint.Y, 2));
		}

		/// <summary>
		/// Computes dot product of two vectors, AB * BC, where: A = this Point, B = point1, C = point2.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <returns></returns>
		public static double DotProduct(this Point thisPoint, Point point1, Point point2)
		{
			return (point1.X - thisPoint.X) * (point2.X - point1.X)
				+ (point1.Y - thisPoint.Y) * (point2.Y - point1.Y);
		}

		/// <summary>
		/// Computes Euclidean distance from this point to a given line.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="lineStart"></param>
		/// <param name="lineEnd"></param>
		/// <param name="segment"></param>
		/// <returns></returns>
		public static double DistanceToLine(this Point thisPoint, Point lineStart, Point lineEnd,
			bool segment = true)
		{
			if (segment)
			{
				if (lineStart.DotProduct(lineEnd, thisPoint) > 0)
					return thisPoint.Distance(lineEnd);
				if (lineEnd.DotProduct(lineStart, thisPoint) > 0)
					return thisPoint.Distance(lineStart);
			}
			return Math.Abs(lineStart.CrossProduct(lineEnd, thisPoint) / lineStart.Distance(lineEnd));
		}

		public static Point MoveTo(this Point thisPoint, Point target, double distance)
		{
			double distX = target.X - thisPoint.X;
			double distY = target.Y - thisPoint.Y;

			if (distY == 0)
			{
				if (distX > 0)
					thisPoint.X += distance;
				else
					thisPoint.X -= distance;
			}
			else if (distX == 0)
			{
				if (distY > 0)
					thisPoint.Y += distance;
				else
					thisPoint.Y -= distance;
			}
			else
			{
				double ratio = distX / distY;

				double moveY = Math.Sqrt(Math.Pow(distance, 2) / (Math.Pow(ratio, 2) + 1));

				if (distY > 0)
					thisPoint.Y += moveY;
				else
					thisPoint.Y -= moveY;

				double moveX = Math.Abs(moveY * ratio);

				if (distX > 0)
					thisPoint.X += moveX;
				else
					thisPoint.X -= moveX;
			}

			return thisPoint;
		}

		public static Point FindIntersection(this Point thisPoint, Point endPoint,
			Point otherLineStartPoint, Point otherLineEndPoint, bool intersectionExistsForSure)
		{
			if (intersectionExistsForSure || CheckIfIntersects(thisPoint, endPoint, otherLineStartPoint, otherLineEndPoint))
				return thisPoint.Copy().MoveTo(endPoint, thisPoint.DistanceToLine(otherLineStartPoint, otherLineEndPoint, false));

			throw new InvalidOperationException("these lines do not intersect");
		}

		public static bool IsInside(this IList<Point2D> polygon, Point2D point)
		{
			bool[] intersections = new bool[polygon.Count];

			Point e21 = new Point(point.X, point.Y), e22 = new Point(point.X - 500, point.Y-10);

			//if (polygon.Count < 10)
			//{
			Point e11 = new Point(), e12 = new Point();
			int count = polygon.Count - 1;
			for (int i = 0; i < count; ++i)
			{
				e11.X = polygon[i].X;
				e11.Y = polygon[i].Y;
				e12.X = polygon[i + 1].X;
				e12.Y = polygon[i + 1].Y;
				intersections[i] = CheckIfIntersects(e11, e12, e21, e22);
			}
			e11.X = polygon[0].X;
			e11.Y = polygon[0].Y;
			e12.X = polygon[count].X;
			e12.Y = polygon[count].Y;
			intersections[count] = CheckIfIntersects(e11, e12, e21, e22);

			//}
			//else
			//	Parallel.For(0, polygon.Count, (int i) =>
			//		{
			//			Point e11 = new Point(), e12 = new Point();
			//			e11.X = polygon[i].X;
			//			e11.Y = polygon[i].Y;
			//			if (i == polygon.Count - 1)
			//			{
			//				e12.X = polygon[0].X;
			//				e12.Y = polygon[0].Y;
			//			}
			//			else
			//			{
			//				e12.X = polygon[i + 1].X;
			//				e12.Y = polygon[i + 1].Y;
			//			}

			//			intersections[i] = CheckIfIntersects(e11, e12, e21, e22);
			//		});

			return (intersections.Count(x => x == true) % 2) != 0;
		}

		public static void Clip(this IList<Point2D> polygon, IList<Point2D> clip)
		{
			var polygonCopy = polygon.Copy();

			var clipCopy = clip.Copy();

			Point e11 = new Point(), e12 = new Point(), e21 = new Point(), e22 = new Point();

			for (int i1 = 0; i1 < polygon.Count - 1; ++i1)
			{
				e11.X = polygon[i1].X;
				e11.Y = polygon[i1].Y;
				e12.X = polygon[i1 + 1].X;
				e12.Y = polygon[i1 + 1].Y;

				//bool e11i = IsInside(clip, polygon[i1]);
				//bool e12i = IsInside(clip, polygon[i1 + 1]);

				for (int i2 = 0; i2 < clip.Count - 1; ++i2)
				{
					e21.X = clip[i2].X;
					e21.Y = clip[i2].Y;
					e22.X = clip[i2 + 1].X;
					e22.Y = clip[i2 + 1].Y;

					if (!CheckIfIntersects(e11, e12, e21, e22))
						continue;

					var intersection = FindIntersection(e11, e12, e21, e22, true);

					bool e21i = IsInside(polygon, clip[i1]);
					bool e22i = IsInside(polygon, clip[i1 + 1]);

					throw new NotImplementedException();
				}
			}


		}

	}
}
