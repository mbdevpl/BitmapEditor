using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MBdev.Extensions.Test
{
	[TestClass]
	public class PointExtensionsTest
	{
		[TestMethod]
		public void Intersects_Test1()
		{
			InvokeIntersects(new Point(0, 0), new Point(10, 10), new Point(1, 11), new Point(9, 19), false);
		}

		[TestMethod]
		public void Intersects_Test2()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(5, 10), true);
		}

		[TestMethod]
		public void Intersects_Test3()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(10, 10), true);
		}

		[TestMethod]
		public void Intersects_Test4()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(14, 10), true);
		}

		[TestMethod]
		public void Intersects_Test5()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(16, 10), false);
		}

		[TestMethod]
		public void Intersects_Test6()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(2, 10), true);
		}

		[TestMethod]
		public void Intersects_Test7()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(-4, 10), true);
		}

		[TestMethod]
		public void Intersects_Test8()
		{
			InvokeIntersects(new Point(0, 4), new Point(10, 5), new Point(5, 0), new Point(-9, 10), false);
		}

		[TestMethod]
		public void Intersects_Test9()
		{
			InvokeIntersects(new Point(6, 6), new Point(9, 9), new Point(10, 0), new Point(0, 10), false);
		}

		[TestMethod]
		public void Intersects_Test10()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, -5), false);
		}

		[TestMethod]
		public void Intersects_Test11()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, 2), true);
		}

		[TestMethod]
		public void Intersects_Test12()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, 5), true);
		}

		[TestMethod]
		public void Intersects_Test13()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, 7), true);
		}

		[TestMethod]
		public void Intersects_Test14()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, 10), true);
		}

		[TestMethod]
		public void Intersects_Test15()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, 13), true);
		}

		[TestMethod]
		public void Intersects_Test16()
		{
			InvokeIntersects(new Point(6, 0), new Point(5, 10), new Point(0, 5), new Point(10, 18), false);
		}

		[TestMethod]
		public void Intersects_Test17()
		{
			InvokeIntersects(new Point(8, 3), new Point(4, 9), new Point(6, 6), new Point(4, 9), true);
		}

		[TestMethod]
		public void Intersects_Test18()
		{
			InvokeIntersects(new Point(0, 0), new Point(5, 5), new Point(10, 0), new Point(5, 5), true);
		}

		[TestMethod]
		public void Intersects_Test19()
		{
			InvokeIntersects(new Point(0, 0), new Point(5, 5), new Point(0, 0), new Point(5, 5), true);
		}

		[TestMethod]
		public void Intersects_Test20()
		{
			InvokeIntersects(new Point(0, 0), new Point(6, 6), new Point(1, 1), new Point(5, 5), true);
		}

		[TestMethod]
		public void Intersects_Test21()
		{
			InvokeIntersects(new Point(0, 0), new Point(5, 5), new Point(0, 1), new Point(5, 5), true);
		}

		[TestMethod]
		public void Intersects_EfficiencyTest()
		{
			const int count = 5000000;
			const int count2 = 2 * count;

			Random r = new Random();
			Point[] starts = new Point[count2];
			Point[] ends = new Point[count2];
			for (int i = 0; i < count2; ++i)
			{
				starts[i] = new Point(r.NextDouble(), r.NextDouble());
				ends[i] = new Point(r.NextDouble(), r.NextDouble());
			}

			Stopwatch watch = new Stopwatch();
			watch.Restart();

			for (int i = 0; i < count2; ++i)
			{
				starts[i].Intersects(ends[i], starts[++i], ends[i]);
			}
			watch.Stop();

			double totalSeconds = ((double)watch.ElapsedMilliseconds) / 1000;
			double perIntersectionNano = (((double)watch.ElapsedMilliseconds) * 1000000) / count;

			string message = String.Format("total runtime: {0}s, per intersection: {1}ns",
				totalSeconds, perIntersectionNano);

			if (perIntersectionNano > 500)
				Assert.Fail(message);
			else if (perIntersectionNano > 200)
				Assert.Inconclusive(message);
			else
				Console.Out.WriteLine(message);

		}

		private void InvokeIntersects(Point pt11, Point pt12, Point pt21, Point pt22, bool expectedResult)
		{
			// Arrange
			bool[] results = new bool[8];
			string msg = String.Format("error in {0}-{1} vs. {2}-{3}", pt11, pt12, pt21, pt22);

			// Act
			results[0] = pt11.Intersects(pt12, pt21, pt22);
			results[1] = pt12.Intersects(pt11, pt21, pt22);
			results[2] = pt11.Intersects(pt12, pt22, pt21);
			results[3] = pt12.Intersects(pt11, pt22, pt21);
			results[4] = pt21.Intersects(pt22, pt11, pt12);
			results[5] = pt22.Intersects(pt21, pt11, pt12);
			results[6] = pt21.Intersects(pt22, pt12, pt11);
			results[7] = pt22.Intersects(pt21, pt12, pt11);

			// Assert
			List<int> errors = new List<int>();
			int i = 0;
			foreach (var result in results)
			{
				if (result != expectedResult)
					errors.Add(i);
				++i;
			}
			Assert.AreEqual(0, errors.Count, String.Format("{0} in variants: [{1}]", msg, String.Join(",", errors)));
		}

	}
}
