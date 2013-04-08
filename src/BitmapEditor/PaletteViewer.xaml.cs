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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BitmapEditor
{
	/// <summary>
	/// Interaction logic for PaletteViewer.xaml
	/// </summary>
	public partial class PaletteViewer : UserControl
	{
		public PaletteViewer()
		{
			InitializeComponent();
		}

		public PaletteViewer(int levels, bool monochrome)
			: this()
		{
			double jump = ((double)255) / (levels - 1);

			List<Color> colors = new List<Color>();

			if (monochrome)
			{
				for (int c = 0; c < levels; ++c)
				{
					byte value = (byte)(c * jump);
					colors.Add(Color.FromRgb(value, value, value));
				}
			}
			else
			{
				for (int r = 0; r < levels; ++r)
					for (int g = 0; g < levels; ++g)
						for (int b = 0; b < levels; ++b)
							colors.Add(Color.FromRgb((byte)(r * jump), (byte)(g * jump), (byte)(b * jump)));
			}

			colors.Sort(ColorCompare);

			int i = 0;
			foreach (var color in colors)
			{
				var rect = new Rectangle();
				rect.Fill = new SolidColorBrush(color);
				Grid.SetColumn(rect, i++);
				ColorsList.Children.Add(rect);
				ColorsList.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}

		/// <summary>
		/// Less than zero: x is less than y.
		/// Zero: x equals y.
		/// Greater than zero: x is greater than y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int ColorCompare(Color x, Color y)
		{
			int diff = (x.R + x.G + x.B) - (y.R + y.G + y.B);
			if (Math.Abs(diff) < 128)
			{
				if (x.B < y.B)
					return -1;
				if (x.B > y.B)
					return 1;

				if (x.G < y.G)
					return -1;
				if (x.G > y.G)
					return 1;

				if (x.R < y.R)
					return -1;
				if (x.R > y.R)
					return 1;

				return diff;
			}
			return diff;
		}

	}
}
