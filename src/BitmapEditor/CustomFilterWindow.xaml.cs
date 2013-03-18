using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BitmapEditor
{
	/// <summary>
	/// Interaction logic for CustomLab1.xaml
	/// </summary>
	public partial class CustomFilterWindow : Window
	{
		private Dictionary<string, List<Point>> exampleFilterFunctions;

		public List<Point> Points
		{
			get
			{
				if (CustomFilterCreator == null)
					return null;
				return CustomFilterCreator.PointCollection;
			}
		}

		public CustomFilterWindow()
		{
			InitializeComponent();

			exampleFilterFunctions = new Dictionary<string, List<Point>>();
			var d = exampleFilterFunctions;
			var l = new List<Point>();
			d.Add("Clear", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(1, 1));
			d.Add("Identity", l);

			l = new List<Point>();
			l.Add(new Point(0, 1));
			l.Add(new Point(1, 0));
			d.Add("Inverse", l);

			l = new List<Point>();
			l.Add(new Point(0, 0.1));
			l.Add(new Point(0.9, 1));
			l.Add(new Point(1, 1));
			d.Add("Brighten", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.1, 0));
			l.Add(new Point(1, 0.9));
			d.Add("Darken", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.05, 0.171));
			l.Add(new Point(0.1, 0.324));
			l.Add(new Point(0.2, 0.576));
			l.Add(new Point(0.3, 0.756));
			l.Add(new Point(0.4, 0.864));
			l.Add(new Point(0.45, 0.891));
			l.Add(new Point(0.5, 0.9));
			l.Add(new Point(0.55, 0.891));
			l.Add(new Point(0.6, 0.864));
			l.Add(new Point(0.7, 0.756));
			l.Add(new Point(0.8, 0.576));
			l.Add(new Point(0.9, 0.324));
			l.Add(new Point(0.95, 0.171));
			l.Add(new Point(1, 0));
			d.Add("Parabola", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.05, 0));
			l.Add(new Point(0.050000001, 0.1));
			l.Add(new Point(0.15, 0.1));
			l.Add(new Point(0.150000001, 0.2));
			l.Add(new Point(0.25, 0.2));
			l.Add(new Point(0.250000001, 0.3));
			l.Add(new Point(0.35, 0.3));
			l.Add(new Point(0.350000001, 0.4));
			l.Add(new Point(0.45, 0.4));
			l.Add(new Point(0.450000001, 0.5));
			l.Add(new Point(0.55, 0.5));
			l.Add(new Point(0.550000001, 0.6));
			l.Add(new Point(0.65, 0.6));
			l.Add(new Point(0.650000001, 0.7));
			l.Add(new Point(0.75, 0.7));
			l.Add(new Point(0.750000001, 0.8));
			l.Add(new Point(0.85, 0.8));
			l.Add(new Point(0.850000001, 0.9));
			l.Add(new Point(0.95, 0.9));
			l.Add(new Point(0.950000001, 1));
			l.Add(new Point(1, 1));
			d.Add("MicroStairs", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.1, 0));
			l.Add(new Point(0.100000001, 0.2));
			l.Add(new Point(0.3, 0.2));
			l.Add(new Point(0.300000001, 0.4));
			l.Add(new Point(0.5, 0.4));
			l.Add(new Point(0.500000001, 0.6));
			l.Add(new Point(0.7, 0.6));
			l.Add(new Point(0.700000001, 0.8));
			l.Add(new Point(0.9, 0.8));
			l.Add(new Point(0.900000001, 1));
			l.Add(new Point(1, 1));
			d.Add("MacroStairs", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.01, 0.049));
			l.Add(new Point(0.02, 0.088));
			l.Add(new Point(0.03, 0.11));
			l.Add(new Point(0.04, 0.112));
			l.Add(new Point(0.05, 0.097));
			l.Add(new Point(0.06, 0.07));
			l.Add(new Point(0.07, 0.041));
			l.Add(new Point(0.08, 0.018));
			l.Add(new Point(0.09, 0.011));
			l.Add(new Point(0.1, 0.024));
			l.Add(new Point(0.11, 0.055));
			l.Add(new Point(0.12, 0.1));
			l.Add(new Point(0.13, 0.15));
			l.Add(new Point(0.14, 0.195));
			l.Add(new Point(0.15, 0.226));
			l.Add(new Point(0.16, 0.239));
			l.Add(new Point(0.17, 0.232));
			l.Add(new Point(0.18, 0.209));
			l.Add(new Point(0.19, 0.18));
			l.Add(new Point(0.2, 0.153));
			l.Add(new Point(0.21, 0.138));
			l.Add(new Point(0.22, 0.14));
			l.Add(new Point(0.23, 0.162));
			l.Add(new Point(0.24, 0.201));
			l.Add(new Point(0.25, 0.25));
			l.Add(new Point(0.26, 0.299));
			l.Add(new Point(0.27, 0.338));
			l.Add(new Point(0.28, 0.36));
			l.Add(new Point(0.29, 0.362));
			l.Add(new Point(0.3, 0.347));
			l.Add(new Point(0.31, 0.32));
			l.Add(new Point(0.32, 0.291));
			l.Add(new Point(0.33, 0.268));
			l.Add(new Point(0.34, 0.261));
			l.Add(new Point(0.35, 0.274));
			l.Add(new Point(0.36, 0.305));
			l.Add(new Point(0.37, 0.35));
			l.Add(new Point(0.38, 0.4));
			l.Add(new Point(0.39, 0.445));
			l.Add(new Point(0.4, 0.476));
			l.Add(new Point(0.41, 0.489));
			l.Add(new Point(0.42, 0.482));
			l.Add(new Point(0.43, 0.459));
			l.Add(new Point(0.44, 0.43));
			l.Add(new Point(0.45, 0.403));
			l.Add(new Point(0.46, 0.388));
			l.Add(new Point(0.47, 0.39));
			l.Add(new Point(0.48, 0.412));
			l.Add(new Point(0.49, 0.451));
			l.Add(new Point(0.5, 0.5));
			l.Add(new Point(0.51, 0.549));
			l.Add(new Point(0.52, 0.588));
			l.Add(new Point(0.53, 0.61));
			l.Add(new Point(0.54, 0.612));
			l.Add(new Point(0.55, 0.597));
			l.Add(new Point(0.56, 0.57));
			l.Add(new Point(0.57, 0.541));
			l.Add(new Point(0.58, 0.518));
			l.Add(new Point(0.59, 0.511));
			l.Add(new Point(0.6, 0.524));
			l.Add(new Point(0.61, 0.555));
			l.Add(new Point(0.62, 0.6));
			l.Add(new Point(0.63, 0.65));
			l.Add(new Point(0.64, 0.695));
			l.Add(new Point(0.65, 0.726));
			l.Add(new Point(0.66, 0.739));
			l.Add(new Point(0.67, 0.732));
			l.Add(new Point(0.68, 0.709));
			l.Add(new Point(0.69, 0.68));
			l.Add(new Point(0.7, 0.653));
			l.Add(new Point(0.71, 0.638));
			l.Add(new Point(0.72, 0.64));
			l.Add(new Point(0.73, 0.662));
			l.Add(new Point(0.74, 0.701));
			l.Add(new Point(0.75, 0.75));
			l.Add(new Point(0.76, 0.799));
			l.Add(new Point(0.77, 0.838));
			l.Add(new Point(0.78, 0.86));
			l.Add(new Point(0.79, 0.862));
			l.Add(new Point(0.8, 0.847));
			l.Add(new Point(0.81, 0.82));
			l.Add(new Point(0.82, 0.791));
			l.Add(new Point(0.83, 0.768));
			l.Add(new Point(0.84, 0.761));
			l.Add(new Point(0.85, 0.774));
			l.Add(new Point(0.86, 0.805));
			l.Add(new Point(0.87, 0.85));
			l.Add(new Point(0.88, 0.9));
			l.Add(new Point(0.89, 0.945));
			l.Add(new Point(0.9, 0.976));
			l.Add(new Point(0.91, 0.989));
			l.Add(new Point(0.92, 0.982));
			l.Add(new Point(0.93, 0.959));
			l.Add(new Point(0.94, 0.93));
			l.Add(new Point(0.95, 0.903));
			l.Add(new Point(0.96, 0.888));
			l.Add(new Point(0.97, 0.89));
			l.Add(new Point(0.98, 0.912));
			l.Add(new Point(0.99, 0.951));
			l.Add(new Point(1, 1));

			d.Add("Wave", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.499999999, 0.0));
			l.Add(new Point(0.5, 0.5));
			l.Add(new Point(1, 1));
			d.Add("LowerCrop", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.5, 0.5));
			l.Add(new Point(0.500000001, 0.0));
			l.Add(new Point(1, 0));
			d.Add("UpperCrop", l);

			l = new List<Point>();
			l.Add(new Point(0, 0));
			l.Add(new Point(0.25, 0.25));
			l.Add(new Point(0.250000001, 0.0));
			l.Add(new Point(0.749999999, 0.0));
			l.Add(new Point(0.75, 0.75));
			l.Add(new Point(1, 1));
			d.Add("CenterCrop", l);
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			if (Points == null || Points.Count == 0)
				return;
			DialogResult = true;
			Close();
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void ButtonClear_Click(object sender, RoutedEventArgs e)
		{
			if (CustomFilterCreator == null)
				return;
			CustomFilterCreator.PointCollection = new List<Point>();
		}

		private void OptionHelp_Click(object sender, RoutedEventArgs e)
		{
			//if (CustomFilterCreator == null)
			//	return;
			//CustomFilterCreator.PointCollection = new List<Point>();
		}

		private void ButtonFilter_Click(object sender, RoutedEventArgs e)
		{
			if (sender == null || !(sender is MenuItem))
				return;
			var item = (MenuItem)sender;
			List<Point> pts = null;
			if (exampleFilterFunctions.TryGetValue(item.Header.ToString(), out pts))
				CustomFilterCreator.PointCollection = pts;
		}

	}
}
