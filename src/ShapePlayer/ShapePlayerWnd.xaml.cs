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

		#region polygons data

		Color3Ch backgroundcolor = new Color3Ch(0.9, 0.9, 0.95);

		//private Color3Ch waves1color = new Color3Ch(0.3, 0.3, 1);
		//private Point2D waves1offset = new Point2D();
		//private bool waves1offsetsign = true;
		//private List<Point2D> waves1 = new List<Point2D>
		//{
		//	new Point2D(0, 0),
		//	new Point2D(20, 50),
		//	new Point2D(40, 0),
		//	new Point2D(60, 50),
		//	new Point2D(80, 0),
		//	new Point2D(100, 50),
		//	new Point2D(120, 0),
		//	new Point2D(140, 50),
		//	new Point2D(160, 0),
		//	new Point2D(180, 50),
		//	new Point2D(200, 0),
		//	new Point2D(220, 50),
		//	new Point2D(240, 0),
		//	new Point2D(260, 50),
		//	new Point2D(280, 0),
		//	new Point2D(300, 50),
		//	new Point2D(320, 0),
		//	new Point2D(340, 50),
		//	new Point2D(360, 0),
		//	new Point2D(380, 50),
		//	new Point2D(400, 0),
		//	new Point2D(420, 50),
		//	new Point2D(440, 0),
		//	new Point2D(440, 170),
		//	new Point2D(0, 160)
		//};

		private Color3Ch waves2color = new Color3Ch(0, 0, 1);
		private Point2D waves2offset = new Point2D();
		private bool waves2offsetsign = false;
		private List<Point2D> waves2 = new List<Point2D>
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
			new Point2D(460, 50),
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
			new Point2D(80, 20),
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

		//private Color3Ch suncolor = new Color3Ch(0.99, 0.99, 0.1);
		//private List<Point2D> sun = new List<Point2D>
		//{
		//	new Point2D(50, 0),
		//	new Point2D(100, 50),
		//	new Point2D(50, 100),
		//	new Point2D(0, 50)
		//};

		#endregion

		private Object bitmapArrayLock;
		private FastBitmapArray bitmapArray;

		public ShapePlayerWnd()
		{
			bitmapArrayLock = new object();

			random = new Random();
			watch = new Stopwatch();
			stopwatch = new Stopwatch();
			stopwatch2 = new Stopwatch();

			InitializeComponent();

			bitmapArray = new FastBitmapArray(500, 500, 0.9, 0.9, 0.9);

			//sun.Offset(new Point2D(200, 200));
			//waves1.Offset(new Point2D(0, 250));
			waves2.Offset(new Point2D(40, 280));
			dolphin.Offset(new Point2D(10, 140));

			RedrawImage();

			timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 50), DispatcherPriority.Background,
				TimerTick, this.Dispatcher);
			timer.Start();
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
			stopwatch.Start();

			//sun.Offset(new Point2D(random.Next() % 5 - 2, random.Next() % 5 - 2));

			//if (waves1offset.X > 4)
			//	waves1offsetsign = false;
			//else if (waves1offset.X < -4)
			//	waves1offsetsign = true;

			//if (waves1offsetsign)
			//	waves1offset.X++;
			//else
			//	waves1offset.X--;

			//waves1.Offset(waves1offset);

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

			RedrawAllShapes();

			stopwatch.Stop();

			long calc = stopwatch.ElapsedMilliseconds;
			long drawing = stopwatch2.ElapsedMilliseconds;
			long total = calc + drawing;
			Console.Out.WriteLine("calc={0:0000000}ms drawing={1:0000000}ms total={2:0000000}ms free={3:0000000}ms {4}",
				calc, drawing, total, watch.ElapsedMilliseconds - total, globalN++);
		}

		private void RedrawImage()
		{
			lock (bitmapArrayLock)
			{
				PlayerImage.Source = bitmapArray.GetBitmap();
				PlayerImage.Width = bitmapArray.Width;
				PlayerImage.Height = bitmapArray.Height;
			}
		}

		private void RedrawAllShapes()
		{
			stopwatch2.Start();

			lock (bitmapArrayLock)
			{
				bitmapArray.Fill(backgroundcolor);

				//bitmapArray.DrawPolygon(sun, suncolor, true);
				//bitmapArray.DrawPolygon(waves1, waves1color, true);
				bitmapArray.DrawPolygon(dolphin, dolphincolor, true);
				bitmapArray.DrawPolygon(waves2, waves2color, true);
				bitmapArray.DrawPolygon(dolphin, dolphincolorother, true, 0, waves2);

				bitmapArray.RefreshBitmap(Mask.Disabled);
			}

			stopwatch2.Stop();
		}

		private void OptionExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

	}
}
