using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GraphicsManipulation;

namespace ShapePlayer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class ShapePlayerWnd : Window
	{

		private Random random;
		private Stopwatch watch;
		private Stopwatch stopwatch;
		private Stopwatch stopwatch2;

		private DispatcherTimer timer;
		private int globalN = 0;
		private DispatcherTimer timerReport;
		private Object timersLock;

		#region polygons data

		Color3Ch backgroundcolor = new Color3Ch(0.9, 0.9, 0.95);

		private Color3Ch suncolor = new Color3Ch(0.99, 0.99, 0.1);
		private List<Point2D> sun = new List<Point2D>
		{
			new Point2D(50, 0),
			new Point2D(100, 50),
			new Point2D(50, 100),
			new Point2D(0, 50)
		};

		private Color3Ch waves1color = new Color3Ch(0.3, 0.3, 1);
		private Point2D waves1offset = new Point2D();
		private bool waves1offsetsign = true;
		private List<Point2D> waves1 = new List<Point2D>
		{
			new Point2D(0, 0),
			new Point2D(20, 50),
			new Point2D(40, 0),
			new Point2D(60, 50),
			new Point2D(80, 0),
			new Point2D(100, 50),
			new Point2D(120, 0),
			new Point2D(140, 50),
			new Point2D(160, 0),
			new Point2D(180, 50),
			new Point2D(200, 0),
			new Point2D(220, 50),
			new Point2D(240, 0),
			new Point2D(260, 50),
			new Point2D(280, 0),
			new Point2D(300, 50),
			new Point2D(320, 0),
			new Point2D(340, 50),
			new Point2D(360, 0),
			new Point2D(380, 50),
			new Point2D(400, 0),
			new Point2D(420, 50),
			new Point2D(440, 0),
			new Point2D(440, 170),
			new Point2D(0, 160)
		};

		private Color3Ch waves2color = new Color3Ch(0, 0, 1);
		private Point2D waves2offset = new Point2D();
		private bool waves2offsetsign = false;
		private List<Point2D> waves2 = new List<Point2D>
		{
			new Point2D(0, 0),
			new Point2D(20, 30),
			new Point2D(40, 0),
			new Point2D(60, 35),
			new Point2D(80, 5),
			new Point2D(100, 45),
			new Point2D(120, 10),
			new Point2D(140, 50),
			new Point2D(160, 10),
			new Point2D(180, 45),
			new Point2D(200, 5),
			new Point2D(220, 35),
			new Point2D(240, 0),
			new Point2D(260, 30),
			new Point2D(280, 0),
			new Point2D(300, 35),
			new Point2D(320, 5),
			new Point2D(340, 45),
			new Point2D(360, 10),
			new Point2D(380, 50),
			new Point2D(400, 10),
			new Point2D(420, 45),
			new Point2D(440, 5),
			new Point2D(460, 35),
			new Point2D(480, 0),
			new Point2D(480, 160),
			new Point2D(0, 170)
		};

		private Color3Ch dolphincolor = new Color3Ch(0.5, 0.5, 0.5);
		private Color3Ch dolphincolorother = new Color3Ch(0.9, 0.3, 0.3);
		private Point2D dolphinoffset = new Point2D(0, 1);
		private bool dolphinoffsetsign = true;
		private List<Point2D> dolphin = new List<Point2D>
		{
			new Point2D(0, 60),
			new Point2D(70, 28),
			new Point2D(90, 23),
			new Point2D(170, 20),
			new Point2D(210, 0),
			new Point2D(200, 25),
			new Point2D(370, 60),
			new Point2D(400, 55),
			new Point2D(365, 90),
			new Point2D(360, 65),
			new Point2D(140, 67),
			new Point2D(160, 80),
			new Point2D(110, 65)
		};
		private IList<Point2D>[] dolphinother;

		#endregion

		private Object bitmapArrayLock;
		private FastBitmapArray bitmapArray;

		private IList<Point2D> rect1, rect2, trig1, trig2;
		private IList<Point2D>[] rect3, trig3;

		public ShapePlayerWnd()
		{
			bitmapArrayLock = new object();
			timersLock = new object();

			random = new Random();
			watch = new Stopwatch();
			stopwatch = new Stopwatch();
			stopwatch2 = new Stopwatch();

			InitializeComponent();

			bitmapArray = new FastBitmapArray(500, 500, 0.9, 0.9, 0.9);

			sun.Offset(new Point2D(200, 200));
			waves1.Offset(new Point2D(0, 250));
			waves2.Offset(new Point2D(40, 280));
			dolphin.Offset(new Point2D(10, 140));

			rect1 = new Point2D[] { new Point2D(20, 20), new Point2D(20, 80), new Point2D(80, 80), new Point2D(80, 20) };
			rect2 = rect1.Copy();
			rect2.Offset(new Point2D(30, 30));
			rect3 = rect1.Clip(rect2);

			trig1 = new Point2D[] { new Point2D(220, 80), new Point2D(280, 20), new Point2D(280, 80) };
			trig2 = trig1.Copy();
			trig2.Offset(new Point2D(15, 15));
			trig3 = trig1.Clip(trig2);

			RedrawImage();

			timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 30), DispatcherPriority.Background,
				TimerTick, this.Dispatcher);
			timer.Start();

			timerReport = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 1970), DispatcherPriority.Background,
				TimerReportTick, this.Dispatcher);
			timerReport.Start();

			watch.Start();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			int width = (int)Math.Floor(e.NewSize.Width);
			int height = (int)Math.Floor(e.NewSize.Height);

			//UpdateImageSize(width, height);
		}

		private void UpdateImageSize(int width, int height)
		{
			lock (bitmapArrayLock)
			{
				int oldWidth = bitmapArray.Width;
				int oldHeight = bitmapArray.Height;

				var newBitmapArray = new FastBitmapArray(width, height);

				bool smallerWidth = width < oldWidth;
				bool smallerHeight = height < oldHeight;

				int diffWidth = width - oldWidth;
				int diffHeight = height - oldHeight;

				if (!smallerWidth && !smallerHeight)
					bitmapArray.CopyAreaBatch(bitmapArray);
			}

			RedrawImage();
		}

		private void TimerTick(object sender, EventArgs e)
		{
			lock (timersLock)
				stopwatch.Start();

			sun.Offset(new Point2D(random.Next() % 5 - 2, random.Next() % 5 - 2));

			if (waves1offset.X > 4)
				waves1offsetsign = false;
			else if (waves1offset.X < -4)
				waves1offsetsign = true;

			if (waves1offsetsign)
				waves1offset.X++;
			else
				waves1offset.X--;

			waves1.Offset(waves1offset);

			if (waves2offset.X > 3)
				waves2offsetsign = false;
			else if (waves2offset.X < -3)
				waves2offsetsign = true;

			if (waves2offsetsign)
				waves2offset.X++;
			else
				waves2offset.X--;

			waves2.Offset(waves2offset);

			if (dolphinoffset.Y > 14)
				dolphinoffsetsign = false;
			else if (dolphinoffset.Y < -14)
				dolphinoffsetsign = true;

			if (dolphinoffsetsign)
				dolphinoffset.Y++;
			else
				dolphinoffset.Y--;

			dolphin.Offset(dolphinoffset);

			dolphinother = dolphin.Clip(waves2);

			lock (timersLock)
				stopwatch.Stop();

			lock (timersLock)
				stopwatch2.Start();

			RedrawAllShapes();

			lock (timersLock)
				stopwatch2.Stop();
		}

		private void TimerReportTick(object sender, EventArgs e)
		{
			long calc, drawing, total, time;
			lock (timersLock)
			{
				calc = stopwatch.ElapsedMilliseconds;
				drawing = stopwatch2.ElapsedMilliseconds;
				total = calc + drawing;
				time = watch.ElapsedMilliseconds;
			}
			Trace.WriteLine(String.Format("calc={0:000000}ms drawing={1:000000}ms total={2:000000}ms free={3:000000}ms time={4:000000}ms {5}",
				calc, drawing, total, time - total, time, globalN++));
		}

		private void RedrawImage()
		{
			lock (bitmapArrayLock)
			{
				PlayerImage.Source = bitmapArray.GetBitmap(Mask.Disabled);
				PlayerImage.Width = bitmapArray.Width;
				PlayerImage.Height = bitmapArray.Height;
			}
		}

		private void RedrawAllShapes()
		{
			lock (bitmapArrayLock)
			{
				bitmapArray.Fill(backgroundcolor);

				bitmapArray.DrawPolygon(sun, suncolor, true);
				bitmapArray.DrawPolygon(waves1, waves1color, true);
				bitmapArray.DrawPolygon(dolphin, dolphincolor, true);
				bitmapArray.DrawPolygon(waves2, waves2color, true);
				foreach (var dolphinotherfragment in dolphinother)
					bitmapArray.DrawPolygon(dolphinotherfragment, dolphincolorother, true);

				bitmapArray.DrawPolygon(rect1, Colors4Ch.Red, true);
				bitmapArray.DrawPolygon(rect2, Colors4Ch.Blue, true);
				foreach (var rect in rect3)
					bitmapArray.DrawPolygon(rect, Colors4Ch.White, true);

				bitmapArray.DrawPolygon(trig1, Colors4Ch.Red, true);
				bitmapArray.DrawPolygon(trig2, Colors4Ch.Blue, true);
				foreach (var trig in trig3)
					bitmapArray.DrawPolygon(trig, Colors4Ch.White, true);

				bitmapArray.RefreshBitmap(Mask.Rectangle);
			}
		}

		private void OptionExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

	}
}
