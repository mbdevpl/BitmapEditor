using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GraphicsManipulation;
using MBdev.Extensions;

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

		#region background polygons data

		Color3Ch backgroundcolor = new Color3Ch(0.9, 0.9, 0.95);

		private Color3Ch suncolor = new Color3Ch(0.99, 0.99, 0.1);
		private Polygon sun = new Polygon(
			new Point(50, 0), new Point(100, 50), new Point(50, 100), new Point(0, 50));

		private Color3Ch waves1color = new Color3Ch(0.3, 0.3, 1);
		private Point waves1offset = new Point();
		private bool waves1offsetsign = true;
		private Polygon waves1 = new Polygon(
			new Point(0, 0),
			new Point(20, 50),
			new Point(40, 0),
			new Point(60, 50),
			new Point(80, 0),
			new Point(100, 50),
			new Point(120, 0),
			new Point(140, 50),
			new Point(160, 0),
			new Point(180, 50),
			new Point(200, 0),
			new Point(220, 50),
			new Point(240, 0),
			new Point(260, 50),
			new Point(280, 0),
			new Point(300, 50),
			new Point(320, 0),
			new Point(340, 50),
			new Point(360, 0),
			new Point(380, 50),
			new Point(400, 0),
			new Point(420, 50),
			new Point(440, 0),
			new Point(440, 170),
			new Point(0, 160)
		);

		private Color3Ch waves2color = new Color3Ch(0.2, 0.2, 0.8);
		private Point waves2offset = new Point();
		private bool waves2offsetsign = false;
		private Polygon waves2 = new Polygon(
			new Point(0, 0),
			new Point(20, 30),
			new Point(40, 0),
			new Point(60, 35),
			new Point(80, 5),
			new Point(100, 45),
			new Point(120, 10),
			new Point(140, 50),
			new Point(160, 10),
			new Point(180, 45),
			new Point(200, 5),
			new Point(220, 35),
			new Point(240, 0),
			new Point(260, 30),
			new Point(280, 0),
			new Point(300, 35),
			new Point(320, 5),
			new Point(340, 45),
			new Point(360, 10),
			new Point(380, 50),
			new Point(400, 10),
			new Point(420, 45),
			new Point(440, 5),
			new Point(460, 35),
			new Point(480, 0),
			new Point(480, 160),
			new Point(0, 170)
		);
		#endregion

		#region user controlled polygon data

		private Color3Ch dolphincolor = new Color3Ch(0.5, 0.5, 0.5);
		private Color3Ch dolphincolorother1 = new Color3Ch(0.6, 0.0, 0.4);
		private Color3Ch dolphincolorother2 = new Color3Ch(0.9, 0.8, 0.0);
		private Color3Ch dolphincolorother3 = new Color3Ch(0.0, 0.7, 0.7);
		private Color3Ch[][] dolphinpatternother = new Color3Ch[][]
			{
				new Color3Ch[] { new Color3Ch(1.0, 0.0, 0.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 0.0, 0.0) },
				new Color3Ch[] { new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(0.0, 0.0, 0.0), new Color3Ch(0.0, 0.0, 0.0), new Color3Ch(0.0, 0.0, 0.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0) },
				new Color3Ch[] { new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(0.0, 0.0, 0.0), new Color3Ch(0.0, 0.0, 0.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(0.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0) },
				new Color3Ch[] { new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(0.0, 0.0, 0.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(0.0, 1.0, 1.0), new Color3Ch(0.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0) },
				new Color3Ch[] { new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(0.0, 1.0, 1.0), new Color3Ch(0.0, 1.0, 1.0), new Color3Ch(0.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0) },
				new Color3Ch[] { new Color3Ch(1.0, 0.0, 0.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 1.0, 1.0), new Color3Ch(1.0, 0.0, 0.0) }
			};
		private Point origin = new Point(0, 0);
		private double dolphinAngle = 270;
		private double dolphinAnglePrevious = 270;
		private double dolphinSpeed = 0;
		private Point dolphinoffset = new Point(0, 0);
		//private bool dolphinoffsetsign = true;
		private Polygon dolphin = new Polygon(
			new Point(0, 60),
			new Point(70, 28),
			new Point(90, 23),
			new Point(170, 20),
			new Point(210, 0),
			new Point(200, 25),
			new Point(370, 60),
			new Point(400, 55),
			new Point(365, 90),
			new Point(360, 65),
			new Point(140, 67),
			new Point(160, 80),
			new Point(110, 65)
		);
		private Polygon[] dolphinother;

		#endregion

		private Object bitmapArrayLock;
		private FastBitmapArray bitmapArray;
		private bool reinitializeDisplayedBitmap;

		private Polygon rect1, rect2, trig1, trig2;
		private Polygon[] rect3, trig3;

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

			sun.Offset(200, 200);
			waves1.Offset(0, 250);
			waves2.Offset(40, 280);
			dolphin.Offset(70, 140);

			try
			{
				rect1 = new Polygon(new Point(20, 20), new Point(20, 80), new Point(80, 80), new Point(80, 20));
				rect2 = rect1.Copy();
				rect2.Offset(30, 30);
				rect3 = rect1.Clip(rect2);

				trig1 = new Polygon(new Point(220, 80), new Point(280, 20), new Point(280, 80));
				trig2 = trig1.Copy();
				trig2.Offset(15, 15);
				trig3 = trig1.Clip(trig2);
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.ToString());
			}

			reinitializeDisplayedBitmap = true;

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

			UpdateImageSize(width, height);
		}

		private void UpdateImageSize(int width, int height)
		{
			lock (bitmapArrayLock)
			{
				int oldWidth = bitmapArray.Width;
				int oldHeight = bitmapArray.Height;

				var newBitmapArray = new FastBitmapArray(width, height);

				//bool smallerWidth = width < oldWidth;
				//bool smallerHeight = height < oldHeight;

				//int diffWidth = width - oldWidth;
				//int diffHeight = height - oldHeight;

				//if (!smallerWidth && !smallerHeight)
				//	bitmapArray.CopyAreaBatch(bitmapArray);

				bitmapArray = newBitmapArray;

				reinitializeDisplayedBitmap = true;
			}
		}

		private void TimerTick(object sender, EventArgs e)
		{
			lock (timersLock)
				stopwatch.Start();

			sun.Offset(random.Next() % 5 - 2, random.Next() % 5 - 2);

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

			//if (dolphinoffset.Y > 14)
			//	dolphinoffsetsign = false;
			//else if (dolphinoffset.Y < -14)
			//	dolphinoffsetsign = true;

			//if (dolphinoffsetsign)
			//	dolphinoffset.Y++;
			//else
			//	dolphinoffset.Y--;

			if (dolphinAnglePrevious != dolphinAngle)
			{
				Point middle = dolphin.Points[0].MoveHalfwayTo(dolphin.Points[7]);
				dolphin.Rotate(middle, (dolphinAngle - 270) - (dolphinAnglePrevious - 270));
				dolphinAnglePrevious = dolphinAngle;
			}
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

		private void RedrawAllShapes()
		{
			lock (bitmapArrayLock)
			{
				bitmapArray.Fill(backgroundcolor);

				bitmapArray.FillPolygon(sun, suncolor);
				bitmapArray.FillPolygon(waves1, waves1color);
				bitmapArray.FillPolygon(dolphin, dolphincolor);
				bitmapArray.FillPolygon(waves2, waves2color);
				foreach (var dolphinotherfragment in dolphinother)
					if (dolphinotherfragment.PointsCount == 3)
						bitmapArray.FillPolygon(dolphinotherfragment, dolphincolorother1);
					else if (dolphinotherfragment.PointsCount == 4)
						bitmapArray.FillPolygon(dolphinotherfragment, dolphincolorother2);
					else if (dolphinotherfragment.PointsCount == 5)
						bitmapArray.FillPolygon(dolphinotherfragment, dolphincolorother3);
					else
						bitmapArray.FillPolygon(dolphinotherfragment, dolphinpatternother);


				bitmapArray.FillPolygon(rect1, Colors4Ch.Red);
				bitmapArray.FillPolygon(rect2, Colors4Ch.Blue);
				foreach (var rect in rect3)
					bitmapArray.FillPolygon(rect, Colors4Ch.White);

				bitmapArray.FillPolygon(trig1, Colors4Ch.Red);
				bitmapArray.FillPolygon(trig2, Colors4Ch.Blue);
				foreach (var trig in trig3)
					bitmapArray.FillPolygon(trig, Colors4Ch.White);

				bitmapArray.RefreshBitmap(Mask.Disabled);

				if (reinitializeDisplayedBitmap)
				{
					PlayerImage.Source = bitmapArray.GetBitmap(Mask.Disabled);
					PlayerImage.Width = bitmapArray.Width;
					PlayerImage.Height = bitmapArray.Height;

					reinitializeDisplayedBitmap = false;
				}
			}
		}

		private void OptionExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.W:
					++dolphinSpeed;
					break;
				case Key.S:
					--dolphinSpeed;
					break;
				case Key.A:
					dolphinAngle -= 2;
					break;
				case Key.D:
					dolphinAngle += 2;
					break;
				case Key.Escape:
					dolphinSpeed = 0;
					break;
				default:
					return;
			}
			dolphinoffset = origin.MoveTo(dolphinAngle, dolphinSpeed);
			//MessageBox.Show(e.Key.ToString());
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			//switch (e.Key)
			//{
			//	case Key.W:
			//	case Key.S:
			//	case Key.A:
			//		//dolphinoffset = origin.MoveTo(++dolphinAngle, dolphinSpeed);
			//		break;
			//	case Key.D:
			//		//dolphinoffset = origin.MoveTo(--dolphinAngle, dolphinSpeed);
			//		break;
			//	case Key.Escape:
			//	default:
			//		break;
			//}
			//MessageBox.Show(e.Key.ToString());
		}

	}
}
