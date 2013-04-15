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
using GraphicsManipulation.Dithering;
using System.Windows.Data;

namespace BitmapEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private static readonly FilterTypes initialFilter = FilterTypes.Inverse;
		private static readonly BrushShapes initialShape = BrushShapes.Square;
		private static readonly System.Reflection.Assembly filtersAssembly;

		static MainWindow()
		{
			filtersAssembly = System.Reflection.Assembly.GetAssembly(typeof(FilterBrush));
		}

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

		private Dictionary<Button, ErrorDiffusionKernelName> dictionaryButtonToErrorDiffusionKernelName = null;
		private Dictionary<ErrorDiffusionKernelName, Button> dictionaryErrorDiffusionKernelNameToButton = null;

		private Point currentLineStart;

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

		private decimal errorDiffusionColorLevels = 2;
		public decimal ErrorDiffusionColorLevels
		{
			get { return errorDiffusionColorLevels; }
			set
			{
				if (errorDiffusionColorLevels == value)
					return;
				errorDiffusionColorLevels = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("ErrorDiffusionColorLevels"));
			}
		}

		private decimal sizeOfOrderedDitheringArray = 2;
		public decimal SizeOfOrderedDitheringArray
		{
			get { return sizeOfOrderedDitheringArray; }
			set
			{
				if (sizeOfOrderedDitheringArray == value)
					return;
				sizeOfOrderedDitheringArray = value;

				if (isGlobalMain && OrderedDithering != null)
				{
					if (OrderedDitheringConverter.MatrixSizeIsAccepted[(int)sizeOfOrderedDitheringArray])
					{
						if (!OrderedDithering.IsEnabled)
							OrderedDithering.IsEnabled = true;
					}
					else
					{
						if (OrderedDithering.IsEnabled)
							OrderedDithering.IsEnabled = false;
					}
				}


				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("SizeOfOrderedDitheringArray"));
			}
		}

		private decimal maxSizeOfOrderedDitheringArray = OrderedDitheringConverter.MaxMatrixSize;
		public decimal MaxSizeOfOrderedDitheringArray
		{
			get { return maxSizeOfOrderedDitheringArray; }
			set
			{
				if (maxSizeOfOrderedDitheringArray == value)
					return;
				maxSizeOfOrderedDitheringArray = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("MaxSizeOfOrderedDitheringArray"));
			}
		}

		private decimal orderedDitheringColorLevels = 2;
		public decimal OrderedDitheringColorLevels
		{
			get { return orderedDitheringColorLevels; }
			set
			{
				if (orderedDitheringColorLevels == value)
					return;
				orderedDitheringColorLevels = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("OrderedDitheringColorLevels"));
			}
		}

		private string toolKindName = "";
		public string ToolKindName
		{
			get { return toolKindName; }
			set
			{
				if (toolKindName == value)
					return;
				toolKindName = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("ToolKindName"));
			}
		}

		private string toolName = "";
		public string ToolName
		{
			get { return toolName; }
			set
			{
				if (toolName == value)
					return;
				toolName = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("ToolName"));
			}
		}

		private decimal lineThickness = 1;
		public decimal LineThickness
		{
			get { return lineThickness; }
			set
			{
				if (lineThickness == value)
					return;
				lineThickness = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("LineThickness"));
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

		public MainWindow(string initialImagePath, int levels = 0, bool monochrome = false)
		{
			this.DataContext = this;

			SizeOfBrush = 25;
			ErrorDiffusionColorLevels = 2;
			SizeOfOrderedDitheringArray = 3;
			OrderedDitheringColorLevels = 2;
			Status = "ready";
			Filter = initialFilter;
			Shape = initialShape;

			isGlobalMain = false;

			InitializeComponent();

			InitializeDictionaries();

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

			FilterType_Checked(dictionaryFilterTypeToRadioButton[filter], null);
			BrushShape_Checked(dictionaryBrushShapeToRadioButton[shape], null);
			FeaturesTabControl.SelectedIndex = 0;

			ReinitializeBitmapArray((BitmapSource)DrawingImage.Source);

			OptionLoadTest.IsEnabled = false;
			OptionLoadTest.Visibility = Visibility.Collapsed;

			if (!isGlobalMain)
			{
				OptionLoad.IsEnabled = false;
				OptionReload.IsEnabled = false;
				ErrorDiffusionGroup.IsEnabled = false;
				OrderedDitheringGroup.IsEnabled = false;
				OptionGeneratePalette.IsEnabled = false;

				OptionLoad.Visibility = Visibility.Collapsed;
				OptionReload.Visibility = Visibility.Collapsed;
				ErrorDiffusionGroup.Visibility = Visibility.Collapsed;
				OrderedDitheringGroup.Visibility = Visibility.Collapsed;
				OptionGeneratePalette.Visibility = Visibility.Collapsed;

				if (levels > 1)
				{
					var pv = new PaletteViewer(levels, monochrome);
					Grid.SetRow(pv, 1);
					CanvasGrid.Children.Add(pv);
				}
			}
		}

		private void InitializeDictionaries()
		{
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
			foreach (var pair in dictionaryFilterTypeToRadioButton)
				dictionaryRadioButtonToFilterType.Add(pair.Value, pair.Key);

			dictionaryBrushShapeToRadioButton = new Dictionary<BrushShapes, RadioButton>();
			var d2 = dictionaryBrushShapeToRadioButton;
			d2.Add(BrushShapes.Fill, BrushFiller);
			d2.Add(BrushShapes.Square, BrushSquare);
			d2.Add(BrushShapes.Circle, BrushCircle);

			dictionaryRadioButtonToBrushShape = new Dictionary<RadioButton, BrushShapes>();
			foreach (var pair in dictionaryBrushShapeToRadioButton)
				dictionaryRadioButtonToBrushShape.Add(pair.Value, pair.Key);

			dictionaryErrorDiffusionKernelNameToButton = new Dictionary<ErrorDiffusionKernelName, Button>();
			var d3 = dictionaryErrorDiffusionKernelNameToButton;
			d3.Add(ErrorDiffusionKernelName.FloydSteinberg, ErrorDiffusionFloydSteinberg);
			d3.Add(ErrorDiffusionKernelName.JarvisJudiceNinke, ErrorDiffusionJarvisJudiceNinke);
			d3.Add(ErrorDiffusionKernelName.Burke, ErrorDiffusionBurke);
			d3.Add(ErrorDiffusionKernelName.Stucky, ErrorDiffusionStucky);

			dictionaryButtonToErrorDiffusionKernelName = new Dictionary<Button, ErrorDiffusionKernelName>();
			foreach (var pair in dictionaryErrorDiffusionKernelNameToButton)
				dictionaryButtonToErrorDiffusionKernelName.Add(pair.Value, pair.Key);
		}

		private void ReinitializeBitmapArray(BitmapSource source)
		{
			SetBitmapArray(new FastBitmapArray(source));
		}

		public void SetBitmapArray(FastBitmapArray array)
		{
			bitmapArray = array;
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

		public MainWindow Clone(int levels = 0, bool monochrome = false)
		{
			MainWindow w = new MainWindow(latestFileName == null ? "" : latestFileName, levels, monochrome);

			w.Width = this.ActualWidth;
			w.Height = this.ActualHeight;

			return w;
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

		private void OptionLoadTest_Click(object sender, RoutedEventArgs e)
		{
			//LoadFromDisk(dlg.FileName);
			if (OptionReload.IsEnabled == true)
				OptionReload.IsEnabled = false;
		}

		private void OptionGeneratePalette_Click(object sender, RoutedEventArgs e)
		{
			bitmapArray = new FastBitmapArray((int)DrawingScrollView.ViewportWidth, (int)DrawingScrollView.ViewportHeight);

			bitmapArray.SetBatchArea(0, 0, bitmapArray.Width - 1, bitmapArray.Height - 1);

			double xJump = 1.0 / bitmapArray.Width;
			int ySegment = bitmapArray.Height / (1 + 3 + 3 + 3 + 3);
			//double xJump = 1.0 / bitmapArray.Width;

			for (int x = 0; x < bitmapArray.Width; ++x)
			{
				int y = 0;
				int yMax = ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
					bitmapArray.SetGreenBatch(x, y, x * xJump);
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetGreenBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
					bitmapArray.SetGreenBatch(x, y, x * xJump);
					//bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					//bitmapArray.SetRedBatch(x, y, x * xJump);
					bitmapArray.SetGreenBatch(x, y, x * xJump);
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
					//bitmapArray.SetGreenBatch(x, y, x * xJump);
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
					bitmapArray.SetGreenBatch(x, y, 1);
					bitmapArray.SetBlueBatch(x, y, 1);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, 1);
					bitmapArray.SetGreenBatch(x, y, x * xJump);
					bitmapArray.SetBlueBatch(x, y, 1);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, 1);
					bitmapArray.SetGreenBatch(x, y, 1);
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
					bitmapArray.SetGreenBatch(x, y, x * xJump);
					bitmapArray.SetBlueBatch(x, y, 1);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, 1);
					bitmapArray.SetGreenBatch(x, y, x * xJump);
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

				yMax += ySegment;
				for (; y < yMax; ++y)
				{
					bitmapArray.SetRedBatch(x, y, x * xJump);
					bitmapArray.SetGreenBatch(x, y, 1);
					bitmapArray.SetBlueBatch(x, y, x * xJump);
				}

			}

			DrawingImage.Source = bitmapArray.GetBitmap(Mask.Disabled);
			DrawingImage.Width = bitmapArray.Width;
			DrawingImage.Height = bitmapArray.Height;
			ReinitializeMask();

			if (OptionReload.IsEnabled == true)
				OptionReload.IsEnabled = false;
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
			if (toolKindName.Equals("Filter"))
			{
				keyIsDown = true;
				Status = "painting";
			}
			else if (toolKindName.Equals("Line"))
			{
				keyIsDown = true;
				Status = "adding line";

				var canvas = (Canvas)sender;
				currentLineStart = new Point(e.GetPosition(canvas).X, e.GetPosition(canvas).Y);

			}

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

			if (toolKindName.Equals("Filter"))
			{
				Status = "painting";

				if (Shape == BrushShapes.Fill)
					return;
			}
			else if (toolKindName.Equals("Line"))
			{
				Status = "adding line";
			}

			DrawingArea_MouseDownOrMove(sender, e);
		}

		private void DrawingArea_MouseDownOrMove(object sender, MouseEventArgs e)
		{
			if (sender == null || !(sender is Canvas))
				throw new ArgumentException("expected only Canvas", "sender");

			var canvas = (Canvas)sender;
			double x = e.GetPosition(canvas).X;
			double y = e.GetPosition(canvas).Y;

			if (x < 0 || y < 0 || x > bitmapArray.Width || y > bitmapArray.Height)
				return;

			if (toolKindName.Equals("Filter"))
			{
				// brush border
				if (Shape != BrushShapes.Fill)
				{
					if (DrawingBrush.Visibility != Visibility.Visible)
						DrawingBrush.Visibility = Visibility.Visible;
					double half = ((double)sizeOfBrush) / 2;
					DrawingBrush.Margin = new Thickness(x - half, y - half, 0, 0);
				}

				//System.Text.StringBuilder s = new System.Text.StringBuilder();
				//s.Append("left click at: ").Append(new Point(x, y));
				//s.Append(' ').Append(Filter).Append(' ').Append(Shape);
				//s.Append(' ').Append((int)BrushSize.Value);
				//MessageBox.Show(s.ToString(), sender.GetType().ToString());

				// get filter instance using reflection
				var typeName = new StringBuilder("GraphicsManipulation.Filters.").Append(Filter).Append("Filter").ToString();
				FilterBrush brush = (FilterBrush)filtersAssembly.CreateInstance(typeName);

				if (brush is CustomFilter)
					((CustomFilter)brush).FilterFunction = latestCustomFilter;

				brush.ApplyAt(bitmapArray, Shape, new Point(x, y), (int)SizeOfBrush, mask);

				if (Shape == BrushShapes.Fill)
					bitmapArray.RefreshBitmap(Mask.Disabled);
				else
					bitmapArray.RefreshBitmap(Mask.Rectangle);
			}
			else if (toolKindName.Equals("Line"))
			{
				//bitmapArray.DrawLine((int)currentLineStart.X, (int)currentLineStart.Y, (int)x, (int)y, 1, 1, 1);

				//bitmapArray.RefreshBitmap(Mask.Rectangle);
			}
		}

		private void DrawingArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Status = "ready";

			if (toolKindName.Equals("Filter"))
			{
				if (keyIsDown)
				{
					keyIsDown = false;
					ReinitializeMask();
					DrawingBrush.Visibility = Visibility.Hidden;
				}
			}
			else if (toolKindName.Equals("Line"))
			{
				if (!keyIsDown)
					return;

				var canvas = (Canvas)sender;
				double x = e.GetPosition(canvas).X;
				double y = e.GetPosition(canvas).Y;

				bitmapArray.DrawLine((int)currentLineStart.X, (int)currentLineStart.Y, (int)x, (int)y, 1, 1, 1, (int)lineThickness);
				bitmapArray.RefreshBitmap(Mask.Rectangle);

				keyIsDown = false;
			}
		}

		#endregion

		#region dithering menu handlers

		private void ErrorDiffusion_Apply(object sender, RoutedEventArgs e)
		{
			if (!isGlobalMain)
				return;

			if (sender is Button == false)
				return;
			var obj = (Button)sender;

			var conv = new ErrorDiffusionConverter();
			var convertedArray = conv.Process(bitmapArray, dictionaryButtonToErrorDiffusionKernelName[obj],
				(int)errorDiffusionColorLevels);

			MainWindow w = Clone((int)errorDiffusionColorLevels, false);
			w.SetBitmapArray(convertedArray);
			w.Show();
		}

		private void OrderedDithering_Apply(object sender, RoutedEventArgs e)
		{
			if (!isGlobalMain)
				return;

			var conv = new OrderedDitheringConverter();
			var convertedArray = conv.Process(bitmapArray, (int)sizeOfOrderedDitheringArray, (int)orderedDitheringColorLevels);

			MainWindow w = Clone((int)orderedDitheringColorLevels, false);
			w.SetBitmapArray(convertedArray);
			w.Show();
		}

		#endregion

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.Source is TabControl == false)
				return;

			var control = ((TabControl)e.Source);
			//var tabs = control.Items;
			var selected = control.SelectedIndex;

			if (selected == 0)
			{
				ToolKindName = "Filter";
				//ToolName = Filter.ToString();
				Binding b = new Binding();
				b.Source = this;
				b.Path = new PropertyPath("Filter");
				b.Mode = BindingMode.OneWay;
				b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(ToolInfo, TextBlock.TextProperty, b);
			}
			else
				BindingOperations.ClearBinding(ToolInfo, TextBlock.TextProperty);

			if (selected == 1)
			{
				ToolKindName = "Dithering";
				ToolName = String.Empty;
			}
			else if (selected == 2)
			{
				ToolKindName = "Line";
				ToolName = String.Empty;
			}
		}

	}
}
