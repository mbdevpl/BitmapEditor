using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphicsManipulation.Filters;

namespace BitmapEditor
{
	/// <summary>
	/// Interaction logic for UserControl2.xaml
	/// </summary>
	public partial class CustomFilterEditor : UserControl
	{
		private List<Point> defaultFilterPointCollection = null;

		private bool plotChanged = true;

		private CustomFilter filter;

		private List<Point> FilterPointCollection
		{
			get
			{
				if (filter.FilterFunction == null)
				{
					if (defaultFilterPointCollection == null)
					{
						defaultFilterPointCollection = new List<Point>();
						defaultFilterPointCollection.Add(new Point(0, 0));
						defaultFilterPointCollection.Add(new Point(1, 1));
					}
					return defaultFilterPointCollection;
				}
				return new List<Point>(filter.FilterFunction);
			}
			set { filter.FilterFunction = value; }
		}

		private List<Point> pointCollection;
		public List<Point> PointCollection
		{
			get
			{
				if (plotChanged)
				{
					pointCollection = FilterPointCollection;
					plotChanged = false;
				}
				return pointCollection;
			}
			set
			{
				pointCollection = value;
				FilterPointCollection = value;
				plotChanged = false;
				RedrawPlot();
			}
		}

		public double CanvasWidth { get { return FilterFunctionCanvas.ActualWidth; } }
		public double CanvasHeight { get { return FilterFunctionCanvas.ActualHeight; } }

		public CustomFilterEditor()
		{
			filter = new CustomFilter();
			pointCollection = new List<Point>();

			InitializeComponent();
		}

		private void RedrawPlot()
		{
			int count = PointCollection.Count;

			PointCollection pc = new PointCollection();
			for (int i = 0; i < count; ++i)
			{
				Point pt = PointCollection[i];
				pc.Add(new Point(pt.X * CanvasWidth, CanvasHeight - pt.Y * CanvasHeight));
			}
			FilterFunctionPolyline.Points = pc;
		}

		private void FilterFunctionCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			return;
		}

		private void FilterFunctionCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var obj = (IInputElement)sender;

			double x = e.GetPosition(obj).X;
			double y = e.GetPosition(obj).Y;
			if (x < 0 || y < 0)
				return;

			Point pt = new Point(x / CanvasWidth, 1 - y / CanvasHeight);
			filter.AddFilterFunctionPoint(pt);
			plotChanged = true;

			RedrawPlot();
		}
	}
}
