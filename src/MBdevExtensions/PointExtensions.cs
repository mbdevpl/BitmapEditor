using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MBdev.Extensions
{
	/// <summary>
	/// Extensions for System.Windows.Point
	/// </summary>
	public static class PointExtensions
	{
		/// <summary>
		/// Ratio between one radian and one degree.
		/// </summary>
		public static readonly double RadiansToDegrees = 180.0 / Math.PI;

		/// <summary>
		/// Ratio between one degree and one radian.
		/// </summary>
		public static readonly double DegreesToRadians = Math.PI / 180;

		///// <summary>
		///// Makes a copy of this point.
		///// </summary>
		///// <param name="thisPoint"></param>
		///// <returns></returns>
		//public static Point Copy(this Point thisPoint)
		//{
		//	return new Point(thisPoint.X, thisPoint.Y);
		//}

		/// <summary>
		/// Returns a point that is offset a given distance in a given direction from
		/// the original point.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="angle">an angle in degrees</param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static Point MoveTo(this Point thisPoint, double angle, double distance)
		{
			double radiansAngle = angle * DegreesToRadians;

			thisPoint.X += distance * Math.Sin(radiansAngle);
			thisPoint.Y -= distance * Math.Cos(radiansAngle);

			return thisPoint;
		}

		/// <summary>
		/// Returns a point that is offset towards a given target. If distance
		/// to the target is less than given distance, the returned point
		/// goes beyond the target until this given distance is reached.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="target"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
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

		public static Point MoveHalfwayTo(this Point origin, Point target)
		{
			return new Point((target.X + origin.X) / 2.0, (target.Y + origin.Y) / 2.0);
		}

		/// <summary>
		/// Computes angle from this point to some other point. If the point are at the same location, <code>Double.NaN</code> is returned.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="point"></param>
		/// <returns>angle in degrees</returns>
		public static double Angle(this Point thisPoint, Point point)
		{
			if (thisPoint.X == point.X)
			{
				if (thisPoint.Y < point.Y)
					return 180.0;
				if (thisPoint.Y > point.Y)
					return 0.0;
				return Double.NaN;
			}
			if (thisPoint.Y == point.Y)
			{
				if (thisPoint.X < point.X)
					return 90.0;
				if (thisPoint.X > point.X)
					return 270.0;
				return Double.NaN;
			}

			double distX = point.X - thisPoint.X;
			double distY = point.Y - thisPoint.Y;
			//double dist = thisPoint.Distance(point);
			//double ratio = distY / dist;

			//double degrees = Math.Atan(ratio) * RadiansToDegrees;
			double degrees = Math.Atan2(distY, distX) * RadiansToDegrees + 90;

			if (degrees < 0)
				degrees += 360;

			return degrees;
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

			if (p1.Y == d1 || p2.Y == d2)
				return true;

			if (below != above)
				return p2.Equals(min) || p2.Equals(max);

			if (p2.X >= min.X && p2.X <= max.X)
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

			throw new ArgumentException("please check preliminary bounds outside of this method");
		}

		private static bool CheckBoundsY(Point p1, Point p2, Point min, Point max)
		{
			// TODO: this can probably be merged with CheckBoundsX

			double ratio = (max.X - min.X) / (max.Y - min.Y);

			double d1 = max.X - (max.Y - p1.Y) * ratio;
			bool toRight = p1.X > d1;

			double d2 = max.X - (max.Y - p2.Y) * ratio;
			bool toLeft = p2.X < d2;

			if (p1.X == d1 || p2.X == d2)
				return true;

			if (toRight != toLeft)
				return p2.Equals(min) || p2.Equals(max);

			if (p2.Y >= min.Y && p2.Y <= max.Y)
			{
				return true;
			}
			else if (p2.Y < min.Y)
			{
				double ratioAlt = (p1.X - min.X) / (p1.Y - min.Y);

				double dAlt = p1.X - (p1.Y - p2.Y) * ratioAlt;
				bool toLeftAlt = p2.X < dAlt;

				if (toRight == toLeftAlt)
					return true;
				else
					return false;
			}
			else
			{
				double ratioAlt = (max.X - p1.X) / (max.Y - p1.Y);

				double dAlt = max.X - (max.Y - p2.Y) * ratioAlt;
				bool toLeftAlt = p2.X < dAlt;

				if (toRight == toLeftAlt)
					return true;
				else
					return false;
			}

			throw new ArgumentException("please check preliminary bounds outside of this method");
		}

		/// <summary>
		/// Checks if given lines intersects. Intersection occurs if lines have at least one common point,
		/// and the endpoints are included in the checking process.
		/// </summary>
		/// <param name="p11">start point of first line</param>
		/// <param name="p12">end point of first line</param>
		/// <param name="p21">start point of second line</param>
		/// <param name="p22">end point of second line</param>
		/// <returns></returns>
		public static bool Intersects(this Point p11, Point p12, Point p21, Point p22)
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

			//if ((p11.Equals(p21) && p12.Equals(p22)) || (p11.Equals(p22) && p12.Equals(p21)))
			if (p11.Equals(p21) || p12.Equals(p22) || p11.Equals(p22) || p12.Equals(p21))
				return true;

			return false;
		}

		/// <summary>
		/// Checks if an intersection exists between the given lines, and finds coordinates if it does.
		/// Throws if there is no intersection.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="endPoint"></param>
		/// <param name="otherLineStartPoint"></param>
		/// <param name="otherLineEndPoint"></param>
		/// <exception cref="InvalidOperationException">if there is no intersection
		/// and the coordinates do not exist</exception>
		/// <returns></returns>
		public static Point FindIntersection(this Point thisPoint, Point endPoint,
			Point otherLineStartPoint, Point otherLineEndPoint)
		{
			if (Intersects(thisPoint, endPoint, otherLineStartPoint, otherLineEndPoint))
				return FindIntersectionAssumingItExists(thisPoint, endPoint, otherLineStartPoint, otherLineEndPoint);

			throw new InvalidOperationException("these lines do not intersect");
		}

		/// <summary>
		/// Assumes that the intersection exists and finds its coordinates.
		/// Unspecified behaviour in case there is no intersection.
		/// </summary>
		/// <param name="thisPoint"></param>
		/// <param name="endPoint"></param>
		/// <param name="otherLineStartPoint"></param>
		/// <param name="otherLineEndPoint"></param>
		/// <returns></returns>
		public static Point FindIntersectionAssumingItExists(this Point thisPoint, Point endPoint,
			Point otherLineStartPoint, Point otherLineEndPoint)
		{
			double a1 = endPoint.Y - thisPoint.Y;
			double b1 = thisPoint.X - endPoint.X;
			double c1 = a1 * thisPoint.X + b1 * thisPoint.Y;

			double a2 = otherLineEndPoint.Y - otherLineStartPoint.Y;
			double b2 = otherLineStartPoint.X - otherLineEndPoint.X;
			double c2 = a2 * otherLineStartPoint.X + b2 * otherLineStartPoint.Y;

			double denominator = a1 * b2 - a2 * b1;

			double x = (b2 * c1 - b1 * c2) / denominator;
			double y = (a1 * c2 - a2 * c1) / denominator;

			return new Point(x, y);
		}

		/// <summary>
		/// Checks if a given point is inside of a polygon defined by list of points.
		/// Edge of a polygon is also treated as inside.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="polygon"></param>
		/// <returns></returns>
		public static bool IsInside(this Point point, IList<Point> polygon)
		{
			int count = polygon.Count;
			int max = count - 1;

			double[] distances = new double[count];
			bool[] intersections = new bool[count];
			Point[] intersectionLocations = new Point[count];

			for (int n = 0; n < count; ++n)
			//Parallel.For(0, count, (int n) =>
			{
				distances[n] = point.Distance(polygon[n]);
			}
			//);

			int indexOfMax = distances.IndexOfMax();
			Point target = point.MoveTo(polygon[indexOfMax], distances[indexOfMax] + 10);
			target.X += 1;
			target.Y += 1;

			for (int n = 0; n < count; ++n)
			//Parallel.For(0, count, (int n) =>
			{
				int next = n == max ? 0 : n + 1;
				intersections[n] = point.Intersects(target, polygon[n], polygon[next]);
				if (intersections[n])
					intersectionLocations[n] = point.FindIntersectionAssumingItExists(target, polygon[n], polygon[next]);
			}
			//);

			for (int n = max - 1; n >= 0; --n)
			//Parallel.For(0, count, (int n) =>
			{
				if (!intersections[n])
					continue; //return;
				for (int i = n + 1; i < count; ++i)
				{
					if (!intersections[i])
						continue;
					//if (Math.Abs(intersectionLocations[n].X - intersectionLocations[i].X) < 0.001
					//	&& Math.Abs(intersectionLocations[n].Y - intersectionLocations[i].Y) < 0.001)
					if (intersectionLocations[n].X == intersectionLocations[i].X
						&& intersectionLocations[n].Y == intersectionLocations[i].Y)
					{
						intersections[i] = false;
						break;
					}
				}
			}
			//);

			return (intersections.Count(x => x == true) % 2) != 0;
		}

		//public static bool Intersects(this LineGeometry thisLine, LineGeometry line)
		//{
		//	if (thisLine == null)
		//		throw new ArgumentNullException("thisLine");
		//	if (line == null)
		//		throw new ArgumentNullException("line");

		//	var p11 = thisLine.StartPoint;
		//	var p12 = thisLine.EndPoint;
		//	var p21 = line.StartPoint;
		//	var p22 = line.EndPoint;

		//	return CheckIfIntersects(p11, p12, p21, p22);
		//}

	}
}
