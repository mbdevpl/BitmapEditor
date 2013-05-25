using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MBdev.Extensions;

namespace GraphicsManipulation
{
	/// <summary>
	/// Double precision 2D polygon stored as a list of consequtive vertices.
	/// </summary>
	public class Polygon
	{

		public IList<Point> Points { get { return points; } }
		private IList<Point> points;

		/// <summary>
		/// Number of vertices of this polygon.
		/// </summary>
		public int PointsCount { get { return points.Count; } }

		/// <summary>
		/// n-th element contains:
		///  1) instersection location,
		///  2) distance to n-th vertex in THIS polygon,
		///  3) index of vertex in OTHER polygon that has this intersection
		/// </summary>
		private IList<Tuple<Point, double, int>>[] intersections;

		/// <summary>
		/// Information about all points from polygon:
		/// location, index in polygon, index in intersections, tuple index in intersections.
		/// </summary>
		private IList<Tuple<Point, int, int, int>> pointsAndIntersections;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Polygon()
		{
			points = new List<Point>();
		}

		private Polygon(IList<Point> points)
		{
			this.points = points;
		}

		public Polygon(params Point[] points)
			: this()
		{
			for (int i = 0; i < points.Length; ++i)
				this.points.Add(points[i]);
		}

		public Polygon Copy()
		{
			IList<Point> copy = new List<Point>();

			foreach (var pt in points)
				copy.Add(new Point(pt.X, pt.Y));

			return new Polygon(copy);
		}

		public void Offset(Point offset)
		{
			for (int i = 0; i < PointsCount; ++i)
			{
				Point pt = points[i];
				pt.Offset(offset.X, offset.Y);
				points[i] = pt;
			}
		}

		public void Offset(double offsetX, double offsetY)
		{
			for (int i = 0; i < PointsCount; ++i)
			{
				Point pt = points[i];
				pt.Offset(offsetX, offsetY);
				points[i] = pt;
			}
		}

		//public void Rotate(double originX, double originY, double angle);

		public void Rotate(Point origin, double angle)
		{
			double distanceToOrigin;
			double angleToOrigin;
			for (int i = 0; i < PointsCount; ++i)
			{
				Point pt = points[i];

				distanceToOrigin = pt.Distance(origin);
				angleToOrigin = pt.Angle(origin);

				points[i] = origin.MoveTo(angleToOrigin - 180 + angle, distanceToOrigin);
			}
		}

		//public static bool IsInside(this Point2D point, IList<Point2D> polygon)
		//{
		//	bool[] intersections = new bool[polygon.Count];

		//	Point e21 = new Point(point.X, point.Y), e22 = new Point(point.X - 500, point.Y - 500);

		//	//if (polygon.Count < 10)
		//	//{
		//	Point e11 = new Point(), e12 = new Point();
		//	int count = polygon.Count - 1;
		//	for (int i = 0; i < count; ++i)
		//	{
		//		e11.X = polygon[i].X;
		//		e11.Y = polygon[i].Y;
		//		e12.X = polygon[i + 1].X;
		//		e12.Y = polygon[i + 1].Y;
		//		intersections[i] = e11.Intersects(e12, e21, e22);
		//	}
		//	e11.X = polygon[0].X;
		//	e11.Y = polygon[0].Y;
		//	e12.X = polygon[count].X;
		//	e12.Y = polygon[count].Y;
		//	intersections[count] = e11.Intersects(e12, e21, e22);

		//	//}
		//	//else
		//	//	Parallel.For(0, polygon.Count, (int i) =>
		//	//		{
		//	//			Point e11 = new Point(), e12 = new Point();
		//	//			e11.X = polygon[i].X;
		//	//			e11.Y = polygon[i].Y;
		//	//			if (i == polygon.Count - 1)
		//	//			{
		//	//				e12.X = polygon[0].X;
		//	//				e12.Y = polygon[0].Y;
		//	//			}
		//	//			else
		//	//			{
		//	//				e12.X = polygon[i + 1].X;
		//	//				e12.Y = polygon[i + 1].Y;
		//	//			}

		//	//			intersections[i] = CheckIfIntersects(e11, e12, e21, e22);
		//	//		});

		//	return (intersections.Count(x => x == true) % 2) != 0;
		//}

		private Polygon other;

		private bool[] isInOther;

		/// <summary>
		/// unordered list of all points that will be then sorted
		/// and possibly divided into fragments that will then be returned;
		/// this list includes all points that make up the clipped shape
		/// </summary>
		private IList<Point> allPoints;

		public Polygon[] Clip(Polygon clip)
		{
			other = clip;

			FindAllIntersections();

			if (intersections.All(x => null == x))
			{
				for (int i = 0; i < PointsCount; ++i)
					if (points[i].IsInside(other.points))
						return new Polygon[] { this.Copy() };
				return new Polygon[] { };
			}

			CheckContainmentOfPoints();

			ComposePointsAndIntersections();
			other.ComposePointsAndIntersections();

			#region allPoints
			allPoints = new List<Point>();
			for (int i1 = 0; i1 < PointsCount; ++i1)
			{
				if (isInOther[i1])
					//if (!allPoints.Contains(polygon[i1]))
					allPoints.Add(points[i1]);
				if (intersections[i1] != null)
					foreach (var tuple in intersections[i1])
					{
						//if (!allPoints.Contains(tuple.Item1))
						allPoints.Add(tuple.Item1);
						//if (points.Any(x => x.Equals(tuple.Item1)))
						//	allPoints.Add(tuple.Item1);
					}
			}

			for (int i2 = 0; i2 < other.PointsCount; ++i2)
			{
				if (other.isInOther[i2])
				{
					//if (!allPoints.Contains(clip[i2]))
					//if (allPoints.Any(x => x.Equals(other.points[i2])))
					//	allPoints.Add(other.points[i2]);
					allPoints.Add(other.points[i2]);
				}
				//if (intersectionsInClip[i2] != null)
				//	foreach (var tuple in intersectionsInClip[i2])
				//		allPoints.Add(tuple.Item1);
			}
			#endregion

			IList<Polygon> fragments = new List<Polygon>();
			IList<Point> newFragment = new List<Point>(new Point[] { allPoints[0] });
			IList<Point> usedPoints = new List<Point>(newFragment);
			allPoints.RemoveAt(0);

			CreateAllFragments(ref fragments, ref newFragment, ref usedPoints, ref allPoints);

			return fragments.ToArray();
		}

		private void FindAllIntersections()
		{
			Point e12 = new Point(), e22 = new Point();

			intersections = new IList<Tuple<Point, double, int>>[PointsCount];
			other.intersections = new IList<Tuple<Point, double, int>>[other.PointsCount];

			for (int i1 = 0; i1 < PointsCount; ++i1)
			{
				if (i1 == PointsCount - 1)
				{
					e12.X = points[0].X;
					e12.Y = points[0].Y;
				}
				else
				{
					e12.X = points[i1 + 1].X;
					e12.Y = points[i1 + 1].Y;
				}

				for (int i2 = 0; i2 < other.PointsCount; ++i2)
				{
					if (i2 == other.PointsCount - 1)
					{
						e22.X = other.points[0].X;
						e22.Y = other.points[0].Y;
					}
					else
					{
						e22.X = other.points[i2 + 1].X;
						e22.Y = other.points[i2 + 1].Y;
					}

					if (!points[i1].Intersects(e12, other.points[i2], e22))
						continue;

					Point intersection = points[i1].FindIntersectionAssumingItExists(e12, other.points[i2], e22);
					//int intersectionX = (int)Math.Round(intersection.X);
					//int intersectionY = (int)Math.Round(intersection.Y);
					//Point2D intrsct = new Point2D(intersectionX, intersectionY);

					// modify clipped polygon intersections list
					if (intersections[i1] == null)
					{
						// initialize current edge's intersections list
						intersections[i1] = new List<Tuple<Point, double, int>>();
						intersections[i1].Add(new Tuple<Point, double, int>(intersection, points[i1].Distance(intersection), i2));
					}
					else
					{
						// insert intersection in proper place
						double distance = points[i1].Distance(intersection);
						int k = 0;
						for (; k < intersections[i1].Count; ++k)
							if (intersections[i1][k].Item2 >= distance)
								break;
						intersections[i1].Insert(k, new Tuple<Point, double, int>(intersection, distance, i2));
					}

					// modify clipping polygon intersections list
					if (other.intersections[i2] == null)
					{
						// initialize current edge's intersections list
						other.intersections[i2] = new List<Tuple<Point, double, int>>();
						other.intersections[i2].Add(new Tuple<Point, double, int>(intersection, other.points[i2].Distance(intersection), i1));
					}
					else
					{
						// insert intersection in proper place
						double distance = other.points[i2].Distance(intersection);
						int k = 0;
						for (; k < other.intersections[i2].Count; ++k)
							if (other.intersections[i2][k].Item2 >= distance)
								break;
						other.intersections[i2].Insert(k, new Tuple<Point, double, int>(intersection, distance, i1));
					}
				}
			}
		}

		private void CheckContainmentOfPoints()
		{
			isInOther = new bool[PointsCount];
			other.isInOther = new bool[other.PointsCount];

			//int firstInside = -1;
			for (int i1 = 0; i1 < PointsCount; ++i1)
				//{
				isInOther[i1] = points[i1].IsInside(other.points);
			//if (firstInside == -1 && isInClip[i1])
			//	firstInside = i1;
			//}

			for (int i2 = 0; i2 < other.PointsCount; ++i2)
				other.isInOther[i2] = other.points[i2].IsInside(points);
		}

		private void ComposePointsAndIntersections()
		{
			pointsAndIntersections = new List<Tuple<Point, int, int, int>>();
			for (int i1 = 0; i1 < PointsCount; ++i1)
			{
				if (isInOther[i1])
					pointsAndIntersections.Add(new Tuple<Point, int, int, int>(points[i1], i1, -1, -1));
				if (intersections[i1] != null)
				{
					int ii1 = 0;
					foreach (var tuple in intersections[i1])
						pointsAndIntersections.Add(new Tuple<Point, int, int, int>(tuple.Item1, -1, i1, ii1++));
				}
			}
		}

		private int CreateAllFragments(ref IList<Polygon> fragments, ref IList<Point> newFragment,
			ref IList<Point> usedPoints, ref IList<Point> remainingPoints)
		{
			while (remainingPoints.Count > 0)
			{
				//Point start = newFragment == null ? remainingPoints[0] : newFragment[0];
				Point current = newFragment == null ? remainingPoints[0] : newFragment[newFragment.Count - 1];

				if (newFragment == null)
					newFragment = new List<Point>();

				bool foundNext = false;
				do
				{
					foundNext = false;
					Point next = new Point();

					IList<int> allNext = FindAllNextIndices(newFragment, current, remainingPoints);
					if (allNext.Count == 1 || (newFragment.Count == 0 && allNext.Count == 2))
					{
						//int allNextIndex = 0;
						//if (newFragment.Count == 0 && allNext[0] == 0 && allNext.Count > 1)
						//	allNextIndex++;
						foundNext = true;
						next = remainingPoints[allNext[0]];
						remainingPoints.RemoveAt(allNext[0]);
						if (newFragment.Count == 0 && allNext[0] != 0)
						{
							//if (allNext[0] == 0)
							//	throw new NotImplementedException("this is just wrong");
							if (!remainingPoints.Remove(current))
								throw new NotImplementedException("well this is embarrasing");
							usedPoints.Add(current);
							newFragment.Add(current);
						}
						usedPoints.Add(next);
						newFragment.Add(next);
					}
					else if (allNext.Count == 0)
					{
						if (newFragment.Count < 3)
							return -1;
						continue;
					}
					else
					{
						IList<Polygon>[] fragmentsResults = new IList<Polygon>[allNext.Count];
						IList<Point>[] newFragmentResults = new IList<Point>[allNext.Count];
						IList<Point>[] usedPointsResults = new IList<Point>[allNext.Count];
						IList<Point>[] remainingPointsResults = new IList<Point>[allNext.Count];
						bool[] gotResults = new bool[allNext.Count];

						IList<Polygon> frags = fragments;
						IList<Point> newfrag = newFragment;
						IList<Point> usdpts = usedPoints;
						IList<Point> rempts = remainingPoints;
						Parallel.For(0, allNext.Count, (int i) =>
						//for (int i = 0; i < allNext.Count; ++i)
						{
							Point nextPoint = rempts[allNext[i]];
							IList<Polygon> fragmentsCopy = new List<Polygon>(frags);
							IList<Point> newFragmentCopy = new List<Point>(newfrag);
							newFragmentCopy.Add(nextPoint);
							IList<Point> usedPointsCopy = new List<Point>(usdpts);
							usedPointsCopy.Add(nextPoint);
							IList<Point> remainingPointsCopy = new List<Point>(rempts);
							remainingPointsCopy.RemoveAt(allNext[i]);
							int result = CreateAllFragments(ref fragmentsCopy, ref newFragmentCopy,
								ref usedPointsCopy, ref remainingPointsCopy);
							if (result >= 0)
							{
								fragmentsResults[i] = fragmentsCopy;
								newFragmentResults[i] = newFragmentCopy;
								usedPointsResults[i] = usedPointsCopy;
								remainingPointsResults[i] = remainingPointsCopy;
								gotResults[i] = true;
							}
						}
						);
						int goodResult = gotResults.IndexOfTrue();
						if (goodResult < 0)
							return -1;
						fragments = fragmentsResults[goodResult];
						newFragment = newFragmentResults[goodResult];
						usedPoints = usedPointsResults[goodResult];
						remainingPoints = remainingPointsResults[goodResult];
						continue;
					}

					if (remainingPoints.Count == 0)
						break;

					current = next;
				}
				while (foundNext);

				if (newFragment != null)
				{
					fragments.Add(new Polygon(newFragment));
					newFragment = null;
				}
			}

			return 0;
		}

		private IList<int> FindAllNextIndices(IList<Point> newFragment, Point current, IList<Point> remainingPoints)
		{
			IList<int> nextIndices = new List<int>();
			IList<int> tempNext = new List<int>();
			IList<int> tempNextOther = new List<int>();

			int nextIndex = -1;
			do
			{
				int currentIndex = pointsAndIntersections.IndexOf(x => x.Item1.Equals(current));
				nextIndex = -1;

				if (currentIndex >= 0)
				{
					nextIndex = FindNextIndex(currentIndex, current, remainingPoints, newFragment, tempNext);
					if (nextIndex >= 0)
						tempNext.Add(nextIndex);
				}

				//if (nextIndex != -2)
				if (nextIndex == -1)
				{
					currentIndex = other.pointsAndIntersections.IndexOf(x => x.Item1.Equals(current));
					if (currentIndex >= 0)
					{
						nextIndex = other.FindNextIndex(currentIndex, current, remainingPoints, newFragment, tempNextOther);
						if (nextIndex >= 0)
							tempNextOther.Add(nextIndex);
					}
				}

				if (nextIndex >= 0)
				{
					//tempNext = tempCandidates[nextIndex];
					nextIndices.Add(nextIndex);
					//next = tempCandidates[nextIndex];
					//tempCandidates.Remove(tempNext);
					//remainingPoints.RemoveAt(nextIndex);
				}
			}
			while (nextIndex >= 0);

			return nextIndices;
		}

		private int FindNextIndex(int currentIndex, Point currentPoint, IList<Point> allPoints,
			IList<Point> fragment, IList<int> nextIndices)
		{
			bool currentPointIsVertex = pointsAndIntersections[currentIndex].Item2 >= 0;
			bool currentPointIsIntersection = pointsAndIntersections[currentIndex].Item3 >= 0;

			#region traverse forward
			// try to traverse forward
			int searchedIndex = currentIndex < pointsAndIntersections.Count - 1 ? currentIndex + 1 : 0;
			int nextIndex = allPoints.IndexOf(pointsAndIntersections[searchedIndex].Item1);

			if (nextIndices.Contains(nextIndex))
				return -1;

			if (nextIndex >= 0)
			{
				Point nextPoint = allPoints[nextIndex];

				// validate the result
				bool validated = false;
				if (currentPointIsVertex)
				{
					// current point is a vertex
					int index = pointsAndIntersections[currentIndex].Item2;
					int indexInc = index == PointsCount - 1 ? 0 : index + 1;
					if (intersections[index] == null)
					{
						if (points.IndexOf(nextPoint) == indexInc)
							validated = true;
					}
					else
					{
						if (nextPoint.Equals(intersections[index][0].Item1))
							validated = true;
					}

				}
				else if (currentPointIsIntersection)
				{
					// current point is an intersection
					int index = pointsAndIntersections[currentIndex].Item3;
					int tupleIndex = pointsAndIntersections[currentIndex].Item4;

					if (isInOther[index])
					{
						if (tupleIndex == intersections[index].Count - 1)
						{
							//int indexInc = index == polygon.Count - 1 ? 0 : index + 1;
							//if (isInClip[indexInc])
							//	validated = true;
							// above is prob. wrong

							if (tupleIndex % 2 != 0)
							{
								int indexInc = index == PointsCount - 1 ? 0 : index + 1;
								if (nextPoint.Equals(points[indexInc]))
									validated = true;
							}
						}
						else if (tupleIndex % 2 != 0)
						{
							if (nextPoint.Equals(intersections[index][tupleIndex + 1].Item1))
								validated = true;
						}
					}
					else
					{
						if (tupleIndex == intersections[index].Count - 1)
						{
							if (tupleIndex % 2 == 0)
							{
								int indexInc = index == PointsCount - 1 ? 0 : index + 1;
								if (nextPoint.Equals(points[indexInc]))
									validated = true;
							}
						}
						else if (tupleIndex % 2 == 0)
						{
							if (nextPoint.Equals(intersections[index][tupleIndex + 1].Item1))
								validated = true;
						}
					}

				}
				if (!validated)
					nextIndex = -1;
				else if (fragment.Count >= 3 && fragment.Any(x => x.Equals(nextPoint)))
					nextIndex = -2;
			}
			#endregion

			if (nextIndex >= 0)
				return nextIndex;

			#region traverse back
			// try to traverse back
			searchedIndex = currentIndex > 0 ? currentIndex - 1 : pointsAndIntersections.Count - 1;
			nextIndex = allPoints.IndexOf(pointsAndIntersections[searchedIndex].Item1);

			if (nextIndices.Contains(nextIndex))
				return -1;

			if (nextIndex >= 0)
			{
				Point nextPoint = allPoints[nextIndex];

				// validate the result
				bool validated = false;
				if (currentPointIsVertex)
				{
					// current point is a vertex
					int index = pointsAndIntersections[currentIndex].Item2;
					int indexDec = index == 0 ? PointsCount - 1 : index - 1;
					if (intersections[indexDec] == null)
					{
						if (points.IndexOf(allPoints[nextIndex]) == indexDec)
							validated = true;
					}
					else
					{
						if (nextPoint.Equals(intersections[indexDec][intersections[indexDec].Count - 1].Item1))
							validated = true;
					}

				}
				else if (currentPointIsIntersection)
				{
					// current point is an intersection
					int index = pointsAndIntersections[currentIndex].Item3;
					int tupleIndex = pointsAndIntersections[currentIndex].Item4;

					if (isInOther[index])
					{
						if (tupleIndex == 0)
						{
							//if (tupleIndex % 2 != 0)
							//{
							//int indexDec = index == 0 ? polygon.Count - 1 : index - 1;
							if (nextPoint.Equals(points[index]))
								validated = true;
							//}
						}
						else if (tupleIndex % 2 == 0)
						{
							if (nextPoint.Equals(intersections[index][tupleIndex - 1].Item1))
								validated = true;
						}
						else
						{
							// nothing needed here
						}
					}
					else
					{
						//if (tupleIndex == intersectionsInPolygon[index].Count - 1)
						//{
						//	if (tupleIndex % 2 == 0)
						//	{
						//		int indexInc = index == polygon.Count - 1 ? 0 : index + 1;
						//		if (nextPoint.Equals(polygon[indexInc]))
						//			validated = true;
						//	}
						//}
						//else
						if (tupleIndex % 2 != 0)
						{
							if (nextPoint.Equals(intersections[index][tupleIndex - 1].Item1))
								validated = true;
						}
						else
						{
							// nothing needed here
						}
					}

				}
				if (!validated)
					nextIndex = -1;
				else if (fragment.Count >= 3 && fragment.Any(x => x.Equals(nextPoint)))
					nextIndex = -2;
			}

			#endregion

			return nextIndex;
		}

	}
}
