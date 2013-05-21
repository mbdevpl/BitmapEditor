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

			throw new InvalidOperationException("these lines do not intersect");
		}

		public static bool IsInside(this Point2D point, IList<Point2D> polygon)
		{
			bool[] intersections = new bool[polygon.Count];

			Point e21 = new Point(point.X, point.Y), e22 = new Point(point.X - 500, point.Y - 10);

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

		public static IList<Point2D>[] Clip(this IList<Point2D> polygon, IList<Point2D> clip)
		{
			/// <summary>
			/// n-th element contains:
			///  1) instersection location,
			///  2) distance to n-th vertex in THIS polygon,
			///  3) index of vertex in OTHER polygon that has this intersection
			/// </summary>
			IList<Tuple<Point2D, double, int>>[] intersectionsInPolygon; // = new IList<Tuple<Point2D, double, int>>[polygon.Count];
			IList<Tuple<Point2D, double, int>>[] intersectionsInClip; // = new IList<Tuple<Point2D, double, int>>[clip.Count];

			FindAllIntersections(polygon, clip, out intersectionsInPolygon, out intersectionsInClip);

			if (intersectionsInPolygon.All(x => ReferenceEquals(x, null)))
			{
				for (int i1 = 0; i1 < polygon.Count - 1; ++i1)
					if (polygon[i1].IsInside(clip))
						return new IList<Point2D>[] { polygon };
				return new IList<Point2D>[] { };
			}

			bool[] isInClip; // = new bool[polygon.Count];
			bool[] isInPolygon; // = new bool[clip.Count];

			/*int firstInside =*/ CheckContainmentOfPoints(polygon, clip, out isInClip, out isInPolygon);

			// unordered list of all points that will be then sorted
			// and possibly divided into fragments that will then be returned;
			// this list includes all points that make up the clipped shape
			IList<Point2D> allPoints = new List<Point2D>();

			for (int i1 = 0; i1 < polygon.Count; ++i1)
			{
				if (isInClip[i1])
					//if (!allPoints.Contains(polygon[i1]))
					allPoints.Add(polygon[i1]);
				if (intersectionsInPolygon[i1] != null)
					foreach (var tuple in intersectionsInPolygon[i1])
						//if (!allPoints.Contains(tuple.Item1))
						allPoints.Add(tuple.Item1);
			}

			for (int i2 = 0; i2 < clip.Count; ++i2)
			{
				if (isInPolygon[i2])
					//if (!allPoints.Contains(clip[i2]))
					allPoints.Add(clip[i2]);
				//if (intersectionsInClip[i2] != null)
				//	foreach (var tuple in intersectionsInClip[i2])
				//		allPoints.Add(tuple.Item1);
			}

			IList<IList<Point2D>> fragments = new List<IList<Point2D>>();

			while (allPoints.Count > 0)
			{
				Point2D start = allPoints[0];
				Point2D current = start;
				IList<Point2D> fragment = new List<Point2D>();
				do
				{
					fragment.Add(current);

					Point2D next = null;

					allPoints.Remove(current);

					int polygonIndex = polygon.IndexOf(current);
					if (polygonIndex >= 0)
					{
						#region moving from polygon's vertex
						// try to traverse forward
						if (intersectionsInPolygon[polygonIndex] == null)
						{
							if (polygonIndex < polygon.Count - 1)
							{
								if (allPoints.Contains(polygon[polygonIndex + 1]))
									next = polygon[polygonIndex + 1];
							}
							else if (allPoints.Contains(polygon[0]))
								next = polygon[0];
						}
						else
						{
							if (allPoints.Contains(intersectionsInPolygon[polygonIndex][0].Item1))
								next = intersectionsInPolygon[polygonIndex][0].Item1;
						}

						// try to traverse backwards
						if (next == null)
						{
							var prevIntersections = polygonIndex > 0 ? intersectionsInPolygon[polygonIndex - 1] : intersectionsInPolygon[polygon.Count - 1];
							if (prevIntersections == null)
							{
								if (polygonIndex > 0)
								{
									if (allPoints.Contains(polygon[polygonIndex - 1]))
										next = polygon[polygonIndex - 1];
								}
								else if (allPoints.Contains(polygon[polygon.Count - 1]))
									next = polygon[polygon.Count - 1];
							}
							else
							{
								if (allPoints.Contains(prevIntersections[prevIntersections.Count - 1].Item1))
									next = prevIntersections[prevIntersections.Count - 1].Item1;
							}
						}
						#endregion
					}

					if (next == null)
					{
						int clipIndex = clip.IndexOf(current);
						if (clipIndex >= 0)
						{
							#region moving from clip's vertex
							// try to traverse forward
							if (intersectionsInClip[clipIndex] == null)
							{
								if (clipIndex < clip.Count - 1)
								{
									if (allPoints.Contains(clip[clipIndex + 1]))
										next = clip[clipIndex + 1];
								}
								else if (allPoints.Contains(clip[0]))
									next = clip[0];
							}
							else
							{
								if (allPoints.Contains(intersectionsInClip[clipIndex][0].Item1))
									next = intersectionsInClip[clipIndex][0].Item1;
							}

							// try to traverse backwards
							if (next == null)
							{
								var prevIntersections = clipIndex > 0 ? intersectionsInClip[clipIndex - 1] : intersectionsInClip[clip.Count - 1];
								if (prevIntersections == null)
								{
									if (clipIndex > 0)
									{
										if (allPoints.Contains(clip[clipIndex - 1]))
											next = clip[clipIndex - 1];
									}
									else if (allPoints.Contains(clip[clip.Count - 1]))
										next = clip[clip.Count - 1];
								}
								else
								{
									if (allPoints.Contains(prevIntersections[prevIntersections.Count - 1].Item1))
										next = prevIntersections[prevIntersections.Count - 1].Item1;
								}
							}
							#endregion
						}

					}

					if (next == null)
					{
						bool foundInPolygonIntersections = false;
						Tuple<Point2D, double, int> tuple = null;
						int polygonIntersectionsIndex = -1;
						int tupleIndex = -1;
						foreach (var tuples in intersectionsInPolygon)
						{
							++polygonIntersectionsIndex;
							if (tuples == null)
								continue;
							for (int i = 0; i < tuples.Count; ++i)
								if (current == tuples[i].Item1)
								{
									foundInPolygonIntersections = true;
									tuple = tuples[i];
									tupleIndex = i;
									break;
								}
							if (foundInPolygonIntersections)
								break;
						}
						if (foundInPolygonIntersections)
						{
							#region moving from an intersection
							if (isInClip[polygonIntersectionsIndex])
							{
								if (tupleIndex == 0)
								{
									if (allPoints.Contains(polygon[polygonIntersectionsIndex]))
										next = polygon[polygonIntersectionsIndex];
								}
								else if (tupleIndex % 2 == 0)
								{
									if (allPoints.Contains(intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex - 1].Item1))
										next = intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex - 1].Item1;
								}
								else
								{
									if (tupleIndex < intersectionsInPolygon[polygonIntersectionsIndex].Count - 1)
									{
										if (allPoints.Contains(intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex + 1].Item1))
											next = intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex + 1].Item1;
									}
									else if (polygonIntersectionsIndex < polygon.Count - 1)
									{
										if (allPoints.Contains(polygon[polygonIntersectionsIndex + 1]))
											next = polygon[polygonIntersectionsIndex + 1];
									}
									else if (allPoints.Contains(polygon[0]))
										next = polygon[0];
								}
							}
							else
							{
								if (tupleIndex % 2 == 0)
								{
									if (tupleIndex < intersectionsInPolygon[polygonIntersectionsIndex].Count - 1)
									{
										if (allPoints.Contains(intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex + 1].Item1))
											next = intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex + 1].Item1;
									}
									else if (polygonIntersectionsIndex < polygon.Count - 1)
									{
										if (allPoints.Contains(polygon[polygonIntersectionsIndex + 1]))
											next = polygon[polygonIntersectionsIndex + 1];
									}
									else if (allPoints.Contains(polygon[0]))
										next = polygon[0];
								}
								else if (allPoints.Contains(intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex - 1].Item1))
									next = intersectionsInPolygon[polygonIntersectionsIndex][tupleIndex - 1].Item1;
							}
							#endregion
						}
					}

					if (next == null)
					{
						bool foundInClipIntersections = false;
						Tuple<Point2D, double, int> tuple = null;
						int clipIntersectionsIndex = -1;
						int tupleIndex = -1;
						foreach (var tuples in intersectionsInClip)
						{
							++clipIntersectionsIndex;
							if (tuples == null)
								continue;
							for (int i = 0; i < tuples.Count; ++i)
								if (current == tuples[i].Item1)
								{
									foundInClipIntersections = true;
									tuple = tuples[i];
									tupleIndex = i;
									break;
								}
							if (foundInClipIntersections)
								break;
						}
						if (foundInClipIntersections)
						{
							#region moving from an intersection
							if (isInPolygon[clipIntersectionsIndex])
							{
								if (tupleIndex == 0)
								{
									if (allPoints.Contains(clip[clipIntersectionsIndex]))
										next = clip[clipIntersectionsIndex];
								}
								else if (tupleIndex % 2 == 0)
								{
									if (allPoints.Contains(intersectionsInClip[clipIntersectionsIndex][tupleIndex - 1].Item1))
										next = intersectionsInClip[clipIntersectionsIndex][tupleIndex - 1].Item1;
								}
								else
								{
									if (tupleIndex < intersectionsInClip[clipIntersectionsIndex].Count - 1)
									{
										if (allPoints.Contains(intersectionsInClip[clipIntersectionsIndex][tupleIndex + 1].Item1))
											next = intersectionsInClip[clipIntersectionsIndex][tupleIndex + 1].Item1;
									}
									else if (clipIntersectionsIndex < clip.Count - 1)
									{
										if (allPoints.Contains(clip[clipIntersectionsIndex + 1]))
											next = clip[clipIntersectionsIndex + 1];
									}
									else if (allPoints.Contains(clip[0]))
										next = clip[0];
								}
							}
							else
							{
								if (tupleIndex % 2 == 0)
								{
									if (tupleIndex < intersectionsInClip[clipIntersectionsIndex].Count - 1)
									{
										if (allPoints.Contains(intersectionsInClip[clipIntersectionsIndex][tupleIndex + 1].Item1))
											next = intersectionsInClip[clipIntersectionsIndex][tupleIndex + 1].Item1;
									}
									else if (clipIntersectionsIndex < clip.Count - 1)
									{
										if (allPoints.Contains(clip[clipIntersectionsIndex + 1]))
											next = clip[clipIntersectionsIndex + 1];
									}
									else if (allPoints.Contains(clip[0]))
										next = clip[0];
								}
								else if (allPoints.Contains(intersectionsInClip[clipIntersectionsIndex][tupleIndex - 1].Item1))
									next = intersectionsInClip[clipIntersectionsIndex][tupleIndex - 1].Item1;
							}
							#endregion
						}
					}

					current = next;
				}
				while (current != null);

				fragments.Add(fragment);
			}

			return fragments.ToArray();
		}

		private static void FindAllIntersections(IList<Point2D> polygon, IList<Point2D> clip,
			out IList<Tuple<Point2D, double, int>>[] intersectionsInPolygon,
			out IList<Tuple<Point2D, double, int>>[] intersectionsInClip)
		{
			Point e11 = new Point(), e12 = new Point(), e21 = new Point(), e22 = new Point();

			intersectionsInPolygon = new IList<Tuple<Point2D, double, int>>[polygon.Count];
			intersectionsInClip = new IList<Tuple<Point2D, double, int>>[clip.Count];

			for (int i1 = 0; i1 < polygon.Count; ++i1)
			{
				e11.X = polygon[i1].X;
				e11.Y = polygon[i1].Y;
				if (i1 == polygon.Count - 1)
				{
					e12.X = polygon[0].X;
					e12.Y = polygon[0].Y;
				}
				else
				{
					e12.X = polygon[i1 + 1].X;
					e12.Y = polygon[i1 + 1].Y;
				}

				for (int i2 = 0; i2 < clip.Count; ++i2)
				{
					e21.X = clip[i2].X;
					e21.Y = clip[i2].Y;
					if (i2 == clip.Count - 1)
					{
						e22.X = clip[0].X;
						e22.Y = clip[0].Y;
					}
					else
					{
						e22.X = clip[i2 + 1].X;
						e22.Y = clip[i2 + 1].Y;
					}

					if (!CheckIfIntersects(e11, e12, e21, e22))
						continue;

					Point intersection = FindIntersection(e11, e12, e21, e22, true);
					int intersectionX = (int)Math.Round(intersection.X);
					int intersectionY = (int)Math.Round(intersection.Y);
					Point2D intrsct = new Point2D(intersectionX, intersectionY);

					// modify clipped polygon intersections list
					if (intersectionsInPolygon[i1] == null)
					{
						// initialize current edge's intersections list
						intersectionsInPolygon[i1] = new List<Tuple<Point2D, double, int>>();
						intersectionsInPolygon[i1].Add(new Tuple<Point2D, double, int>(intrsct, e11.Distance(intersection), i2));
					}
					else
					{
						// insert intersection in proper place
						double distance = e11.Distance(intersection);
						int k = 0;
						for (; k < intersectionsInPolygon[i1].Count; ++k)
							if (intersectionsInPolygon[i1][k].Item2 >= distance)
								break;
						intersectionsInPolygon[i1].Insert(k, new Tuple<Point2D, double, int>(intrsct, distance, i2));
					}

					// modify clipping polygon intersections list
					if (intersectionsInClip[i2] == null)
					{
						// initialize current edge's intersections list
						intersectionsInClip[i2] = new List<Tuple<Point2D, double, int>>();
						intersectionsInClip[i2].Add(new Tuple<Point2D, double, int>(intrsct, e21.Distance(intersection), i1));
					}
					else
					{
						// insert intersection in proper place
						double distance = e21.Distance(intersection);
						int k = 0;
						for (; k < intersectionsInClip[i2].Count; ++k)
							if (intersectionsInClip[i2][k].Item2 >= distance)
								break;
						intersectionsInClip[i2].Insert(k, new Tuple<Point2D, double, int>(intrsct, distance, i1));
					}
				}
			}
		}

		private static int CheckContainmentOfPoints(IList<Point2D> polygon, IList<Point2D> clip, out bool[] isInClip, out bool[] isInPolygon)
		{
			isInClip = new bool[polygon.Count];
			isInPolygon = new bool[clip.Count];

			int firstInside = -1;
			for (int i1 = 0; i1 < polygon.Count; ++i1)
			{
				isInClip[i1] = polygon[i1].IsInside(clip);
				if (firstInside == -1 && isInClip[i1])
					firstInside = i1;
			}

			for (int i2 = 0; i2 < clip.Count; ++i2)
				isInPolygon[i2] = clip[i2].IsInside(polygon);

			return firstInside;
		}

	}
}
