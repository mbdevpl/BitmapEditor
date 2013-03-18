using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using GraphicsManipulation;
using GraphicsManipulation.Filters;
using System.Text;

namespace BitmapEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private static readonly FilterTypes initialFilter = FilterTypes.Inverse;
		private static readonly BrushShapes initialShape = BrushShapes.Square;

		//private BitmapSource originalImage = null;

		private FastBitmapArray bitmapArray = null;

		private bool isGlobalMain;

		private bool keyIsDown = false;

		/// <summary>
		/// Mask used while using brush to avoid ex. applying inverse filter 
		/// over and over again to the same pixels.
		/// 
		/// Mask is cleared after mouse button up, so that each new drawing stroke still
		/// can draw on the whole image.
		/// </summary>
		private bool[][] mask = null;

		private Dictionary<RadioButton, FilterTypes> dictionaryRadioButtonToFilterType = null;
		private Dictionary<FilterTypes, RadioButton> dictionaryFilterTypeToRadioButton = null;
		private Dictionary<RadioButton, BrushShapes> dictionaryRadioButtonToBrushShape = null;
		private Dictionary<BrushShapes, RadioButton> dictionaryBrushShapeToRadioButton = null;

		//private CustomFilterEditor customFilterCreator = null;

		//private List<double> customFilter = null;

		public event PropertyChangedEventHandler PropertyChanged;

		private string latestFileName = null;
		private List<Point> latestCustomFilter = null;

		private FilterTypes previousFilter = initialFilter;
		private FilterTypes filter = initialFilter;
		public FilterTypes Filter
		{
			get { return filter; }
			set
			{
				previousFilter = filter;
				if (filter == value)
					return;
				filter = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Filter"));
			}
		}

		private BrushShapes shape = initialShape;
		public BrushShapes Shape
		{
			get { return shape; }
			set
			{
				if (shape == value)
					return;
				shape = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Shape"));
			}
		}

		private decimal sizeOfBrush = 10;
		public decimal SizeOfBrush
		{
			get { return sizeOfBrush; }
			set
			{
				if (sizeOfBrush == value)
					return;
				sizeOfBrush = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("SizeOfBrush"));
			}
		}

		private string status = "";
		public string Status
		{
			get { return status; }
			set
			{
				if (status == value)
					return;
				status = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Status"));
			}
		}

		public MainWindow() : this(null) { }

		public MainWindow(string initialImagePath)
		{
			this.DataContext = this;

			SizeOfBrush = 25;
			Status = "ready";
			Filter = initialFilter;
			Shape = initialShape;

			isGlobalMain = false;

			InitializeComponent();

			dictionaryFilterTypeToRadioButton = new Dictionary<FilterTypes, RadioButton>();
			var d1 = dictionaryFilterTypeToRadioButton;
			d1.Add(FilterTypes.Identity, FilterIdentity);
			d1.Add(FilterTypes.Inverse, FilterInverse);
			d1.Add(FilterTypes.OnlyRed, FilterRed);
			d1.Add(FilterTypes.OnlyGreen, FilterGreen);
			d1.Add(FilterTypes.OnlyBlue, FilterBlue);
			d1.Add(FilterTypes.Grayscale, FilterGrayscale);
			d1.Add(FilterTypes.Sepia, FilterSepia);
			d1.Add(FilterTypes.Brighten, FilterBrighten);
			d1.Add(FilterTypes.Darken, FilterDarken);
			d1.Add(FilterTypes.Random, FilterRandom);
			d1.Add(FilterTypes.Custom, FilterCustomFunction);

			dictionaryRadioButtonToFilterType = new Dictionary<RadioButton, FilterTypes>();
			//var d2 = dictionaryRadioButtonToFilterType;
			foreach (var pair in dictionaryFilterTypeToRadioButton)
				dictionaryRadioButtonToFilterType.Add(pair.Value, pair.Key);
			//d2.Add(FilterIdentity, FilterTypes.Identity);
			//d2.Add(FilterInverse, FilterTypes.Inverse);
			//d2.Add(FilterRed, FilterTypes.OnlyRed);
			//d2.Add(FilterGreen, FilterTypes.OnlyGreen);
			//d2.Add(FilterBlue, FilterTypes.OnlyBlue);
			//d2.Add(FilterGrayscale, FilterTypes.Grayscale);
			//d2.Add(FilterSepia, FilterTypes.Sepia);
			//d2.Add(FilterBrighten, FilterTypes.Brighten);
			//d2.Add(FilterDarken, FilterTypes.Darken);
			//d2.Add(FilterRandom, FilterTypes.Random);
			//d2.Add(FilterCustomFunction, FilterTypes.Custom);

			dictionaryBrushShapeToRadioButton = new Dictionary<BrushShapes, RadioButton>();
			var d3 = dictionaryBrushShapeToRadioButton;
			d3.Add(BrushShapes.Fill, BrushFiller);
			d3.Add(BrushShapes.Square, BrushSquare);
			d3.Add(BrushShapes.Circle, BrushCircle);

			dictionaryRadioButtonToBrushShape = new Dictionary<RadioButton, BrushShapes>();
			//var d4 = dictionaryRadioButtonToBrushShape;
			foreach (var pair in dictionaryBrushShapeToRadioButton)
				dictionaryRadioButtonToBrushShape.Add(pair.Value, pair.Key);
			//d4.Add(BrushFiller, BrushShapes.Fill);
			//d4.Add(BrushSquare, BrushShapes.Square);
			//d4.Add(BrushCircle, BrushShapes.Circle);

			if (initialImagePath == null)
			{
				isGlobalMain = true;
				//LoadFromDisk("honda.jpg");
			}
			else
			{
				isGlobalMain = false;
				LoadFromDisk(initialImagePath);
			}

			//RefreshVars();

			FilterType_Checked(dictionaryFilterTypeToRadioButton[filter], null);
			BrushShape_Checked(dictionaryBrushShapeToRadioButton[shape], null);

			ReinitializeBitmapArray((BitmapSource)DrawingImage.Source);
		}

		private void ReinitializeBitmapArray(BitmapSource source)
		{
			bitmapArray = new FastBitmapArray(source);
			DrawingImage.Source = bitmapArray.GetBitmap(Mask.Disabled);
			DrawingImage.Width = bitmapArray.Width;
			DrawingImage.Height = bitmapArray.Height;
			ReinitializeMask();
		}

		private void ReinitializeMask()
		{
			if (bitmapArray == null)
				return;

			mask = new bool[bitmapArray.Width][];
			for (int i = 0; i < bitmapArray.Width; i++)
			{
				mask[i] = new bool[bitmapArray.Height];
				for (int j = 0; j < bitmapArray.Height; j++)
					mask[i][j] = true;
			}
		}

		private void SaveToDisk(string fileName)
		{
			FileStream stream = new FileStream(fileName, FileMode.Create);
			BmpBitmapEncoder encoder = new BmpBitmapEncoder();
			TextBlock myTextBlock = new TextBlock();
			myTextBlock.Text = "Codec Author is: " + encoder.CodecInfo.Author.ToString();
			//encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.DrawingImage.Source));
			encoder.Frames.Add(BitmapFrame.Create(bitmapArray.GetBitmap(Mask.Disabled)));
			encoder.Save(stream);
		}

		private void LoadFromDisk(string fileName)
		{
			if (fileName == null || fileName.Length == 0)
				return;

			//DrawingImage.Source = new BitmapImage(new Uri(fileName));
			try
			{
				var image = new BitmapImage(new Uri(fileName));
				ReinitializeBitmapArray((BitmapSource)image);
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e);
				return;
			}

			latestFileName = fileName;
			if (OptionReload.IsEnabled == false)
				OptionReload.IsEnabled = true;
			//RefreshVars(false);
		}

		//private void RefreshVars(bool reselectRadioButtons = true)
		//{
		//	if (reselectRadioButtons)
		//	{
		//		FilterType_Checked(dictionaryFilterTypeToRadioButton[filter], null);
		//		BrushShape_Checked(dictionaryBrushShapeToRadioButton[shape], null);
		//	}

		//	originalImage = (BitmapSource)this.DrawingImage.Source;
		//	DrawingImage.Width = originalImage.PixelWidth;
		//	DrawingImage.Height = originalImage.PixelHeight;
		//	ReinitializeMask();

		//	bitmapArray = new FastBitmapArray(originalImage);

		//	if (!isGlobalMain)
		//	{
		//		OptionLoad.IsEnabled = false;
		//		OptionReload.IsEnabled = false;

		//		isGlobalMain = true;
		//	}

		//	originalImage = bitmapArray.GetBitmap(Mask.Disabled);
		//	DrawingImage.Source = originalImage;
		//}

		#region main menu handlers

		private void OptionSaveAs_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Bitmaps (.bmp)|*.bmp";
			dlg.FileName = "image.bmp";

			if (dlg.ShowDialog() == true)
				SaveToDisk(dlg.FileName);
		}

		private void OptionLoad_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.FileName = ""; // Default file name
			dlg.DefaultExt = ".bmp"; // Default file extension
			dlg.Filter = "Bitmaps (.bmp)|*.bmp"; // Filter files by extension

			if (dlg.ShowDialog() == true)
				LoadFromDisk(dlg.FileName);
		}

		private void OptionReload_Click(object sender, RoutedEventArgs e)
		{
			LoadFromDisk(latestFileName);
		}

		private void OptionExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#endregion

		#region filters menu handlers

		private void FilterType_Checked(object sender, RoutedEventArgs e)
		{
			if (dictionaryRadioButtonToFilterType == null)
				return;
			if (sender == null || !(sender is RadioButton))
				return;

			RadioButton btn = (RadioButton)sender;

			Filter = dictionaryRadioButtonToFilterType[btn];

			if (Filter == FilterTypes.Custom)
			{
				var wnd = new CustomFilterWindow();
				if (wnd.ShowDialog() == true)
					latestCustomFilter = wnd.Points;
				else
				{
					Filter = previousFilter;
					dictionaryFilterTypeToRadioButton[Filter].IsChecked = true;
				}
			}
		}

		private void BrushShape_Checked(object sender, RoutedEventArgs e)
		{
			if (dictionaryRadioButtonToBrushShape == null)
				return;
			if (sender == null || !(sender is RadioButton))
				return;

			RadioButton btn = (RadioButton)sender;
			Shape = dictionaryRadioButtonToBrushShape[btn];
		}

		#endregion

		//private void DrawingArea_DragOver(object sender, DragEventArgs e)
		//{
		//	var obj = (IInputElement)sender;
		//	MessageBox.Show("drag over" + e.GetPosition(obj).ToString(),
		//		 sender.GetType().ToString());
		//}

		//private void DrawingArea_Drop(object sender, DragEventArgs e)
		//{
		//	var obj = (IInputElement)sender;
		//	MessageBox.Show("drop " + e.GetPosition(obj).ToString(),
		//		 sender.GetType().ToString());
		//}

		#region canvas handlers

		private void DrawingArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			keyIsDown = true;
			Status = "painting";

			DrawingArea_MouseDownOrMove(sender, e);
		}

		private void DrawingArea_MouseMove(object sender, MouseEventArgs e)
		{
			MouseButtonState btn = e.LeftButton;

			if (btn == MouseButtonState.Released)
			{
				Status = "ready";
				if (keyIsDown)
				{
					keyIsDown = false;
					ReinitializeMask();
				}

				return;
			}

			if (!keyIsDown)
			{
				Status = "ready";
				return;
			}

			Status = "painting";

			DrawingArea_MouseDownOrMove(sender, e);

		}

		private void DrawingArea_MouseDownOrMove(object sender, MouseEventArgs e)
		{
			if (sender == null || !(sender is Canvas))
				throw new ArgumentException("expected only Canvas", "sender");

			var canvas = (Canvas)sender;
			double x = e.GetPosition(canvas).X;
			double y = e.GetPosition(canvas).Y;

			//BitmapArray arr = new BitmapArray((BitmapSource)DrawingImage.Source);

			if (x < 0 || y < 0 || x > bitmapArray.Width || y > bitmapArray.Height)
				return;

			// brush border
			if (DrawingBrush.Visibility != Visibility.Visible)
				DrawingBrush.Visibility = Visibility.Visible;
			double half = ((double)sizeOfBrush) / 2;
			DrawingBrush.Margin = new Thickness(x - half, y - half, 0, 0);

			//System.Text.StringBuilder s = new System.Text.StringBuilder();
			//s.Append("left click at: ").Append(new Point(x, y));
			//s.Append(' ').Append(Filter).Append(' ').Append(Shape);
			//s.Append(' ').Append((int)BrushSize.Value);
			//MessageBox.Show(s.ToString(), sender.GetType().ToString());

			var typeName = new StringBuilder().Append("GraphicsManipulation.Filters.").Append(Filter).Append("Filter").ToString();

			var typeRef = System.Reflection.Assembly.GetAssembly(typeof(FilterBrush)).GetType(typeName);

			//var typeRef = Type.GetType(typeName, true, false);

			FilterBrush brush = (FilterBrush)System.Reflection.Assembly.GetAssembly(typeRef).CreateInstance(typeName);

			if(brush is CustomFilter)
				((CustomFilter)brush).FilterFunction = latestCustomFilter;

			brush.ApplyAt(bitmapArray, Shape, new Point(x, y), (int)SizeOfBrush, mask);

			//if (filter == FilterTypes.Custom)
			//{
			//	var fltr = new CustomFilter();
			//	fltr.FilterFunction = latestCustomFilter;
			//	fltr.ApplyAt(arr, Shape, new Point(x, y), (int)BrushSize.Value, mask);
			//}
			//else
			//	arr.Filter(Filter, Shape, new Point(x, y), (int)BrushSize.Value, mask);

			//var img = arr.GetImageCopy();
			//DrawingImage.Source = img;

			bitmapArray.RefreshBitmap(Mask.Disabled);
			//bitmapArray.RefreshBitmap(Mask.Rectangle);
		}

		private void DrawingArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Status = "ready";
			if (keyIsDown)
			{
				keyIsDown = false;
				ReinitializeMask();
				DrawingBrush.Visibility = Visibility.Hidden;
			}
		}

		#endregion

		#region dithering menu handlers

		private void ErrorDiffusion_Apply(object sender, RoutedEventArgs e)
		{
			if (!isGlobalMain)
				return;
			Window w = new MainWindow(latestFileName == null ? "" : latestFileName);

			w.Show();
		}

		private void OrderedDithering_Apply(object sender, RoutedEventArgs e)
		{
			Window w = new MainWindow(latestFileName == null ? "" : latestFileName);
			w.Show();
		}

		#endregion

		//private void Halftoning_Click(object sender, RoutedEventArgs e)
		//{
		//}

	}
}
