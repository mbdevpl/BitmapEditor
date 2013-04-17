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
using System.Collections.ObjectModel;

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

		private FastBitmapArray bitmapArrayTemp = null;

		private FastBitmapArray bitmapArrayWithoutLines = null;

		//private bool isGlobalMain = true;

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
		private Point currentLineEnd;

		public event PropertyChangedEventHandler PropertyChanged;

		private string latestFileName = null;
		private List<Point> latestCustomFilter = null;

		private Tools tool;
		public Tools Tool
		{
			get { return tool; }
			set
			{
				if (tool == value)
					return;
				tool = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Tool"));
			}
		}

		//private string toolName = "";
		//public string ToolName
		//{
		//	get { return toolName; }
		//	set
		//	{
		//		if (toolName == value)
		//			return;
		//		toolName = value;

		//		if (PropertyChanged != null)
		//			PropertyChanged(this, new PropertyChangedEventArgs("ToolName"));
		//	}
		//}

		#region Filtering

		private BrushShapes shape;
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

		private decimal sizeOfBrush;
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

		private FilterTypes previousFilter;
		private FilterTypes filter;
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

		#endregion

		#region Dithering

		private decimal errorDiffusionColorLevels;
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

		private decimal sizeOfOrderedDitheringArray;
		public decimal SizeOfOrderedDitheringArray
		{
			get { return sizeOfOrderedDitheringArray; }
			set
			{
				if (sizeOfOrderedDitheringArray == value)
					return;
				sizeOfOrderedDitheringArray = value;

				if (/*isGlobalMain && */OrderedDithering != null)
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

		private decimal maxSizeOfOrderedDitheringArray;
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

		private decimal orderedDitheringColorLevels;
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

		#endregion

		#region Line

		private decimal lineThickness;
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

		private Color lineColor;
		public Color LineColor
		{
			get { return lineColor; }
			set
			{
				if (lineColor == value)
					return;
				lineColor = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("LineColor"));
			}
		}

		private ObservableCollection<Line> lines;
		public ObservableCollection<Line> Lines
		{
			get { return lines; }
			set
			{
				if (lines == value)
					return;
				lines = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Lines"));
			}
		}

		public decimal MaxPointX
		{
			get { return bitmapArray.Width - 1; }
		}

		public decimal MaxPointY
		{
			get { return bitmapArray.Height - 1; }
		}

		private decimal startPointX;
		public decimal StartPointX
		{
			get { return startPointX; }
			set
			{
				if (startPointX == value)
					return;
				startPointX = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("StartPointX"));
			}
		}

		private decimal startPointY;
		public decimal StartPointY
		{
			get { return startPointY; }
			set
			{
				if (startPointY == value)
					return;
				startPointY = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("StartPointY"));
			}
		}

		private decimal endPointX;
		public decimal EndPointX
		{
			get { return endPointX; }
			set
			{
				if (endPointX == value)
					return;
				endPointX = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("EndPointX"));
			}
		}

		private decimal endPointY;
		public decimal EndPointY
		{
			get { return endPointY; }
			set
			{
				if (endPointY == value)
					return;
				endPointY = value;

				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("EndPointY"));
			}
		}

		#endregion

		//private string status = "";
		//public string Status
		//{
		//	get { return status; }
		//	set
		//	{
		//		if (status == value)
		//			return;
		//		status = value;

		//		if (PropertyChanged != null)
		//			PropertyChanged(this, new PropertyChangedEventArgs("Status"));
		//	}
		//}

		public MainWindow()
		{
			// for bindings
			this.DataContext = this;

			Tool = Tools.Filtering;

			Shape = initialShape;
			SizeOfBrush = 25;
			Filter = initialFilter;

			ErrorDiffusionColorLevels = 2;
			SizeOfOrderedDitheringArray = 3;
			MaxSizeOfOrderedDitheringArray = OrderedDitheringConverter.MaxMatrixSize;
			OrderedDitheringColorLevels = 2;

			LineThickness = 1;
			LineColor = Colors.White;
			Lines = new ObservableCollection<Line>();

			//Status = "ready";

			InitializeComponent();

			InitializeDictionaries();

			FilterType_Checked(dictionaryFilterTypeToRadioButton[filter], null);
			BrushShape_Checked(dictionaryBrushShapeToRadioButton[shape], null);
			FeaturesTabControl.SelectedIndex = 0;

			ReinitializeBitmapArray((BitmapSource)DrawingImage.Source);

			OptionLoadTest.IsEnabled = false;
			OptionLoadTest.Visibility = Visibility.Collapsed;
		}

		public MainWindow(string initialImagePath, PaletteViewer paletteViewer)
			: this()
		{
			//isGlobalMain = false;

			if (initialImagePath != null)
			{
				//throw new ArgumentNullException("image path cannot be null");
				// it can be null

				if (initialImagePath.Length == 0)
					throw new ArgumentException("image path cannot be empty if it is not null");

				LoadFromDisk(initialImagePath);
			}

			if (paletteViewer != null)
			{
				Grid.SetRow(paletteViewer, 1);
				CanvasGrid.Children.Add(paletteViewer);
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

		private void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

			SendPropertyChanged("MaxPointX");
			SendPropertyChanged("MaxPointY");
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
			var pv = new PaletteViewer(levels, monochrome);

			MainWindow w = new MainWindow(latestFileName, pv);

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

		private void OptionReload_Click(object sender, RoutedEventArgs e)
		{
			LoadFromDisk(latestFileName);
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

			if (latestFileName != null)
			{
				latestFileName = null;

				OptionReload.IsEnabled = false;
			}
		}

		private void OptionAdjustWindowSize_Click(object sender, RoutedEventArgs e)
		{
			double widthChange = DrawingScrollView.ScrollableWidth;
			double heightChange = DrawingScrollView.ScrollableHeight;

			if (widthChange == 0)
				widthChange = DrawingArea.Width - DrawingScrollView.ViewportWidth;
			if (heightChange == 0)
				heightChange = DrawingArea.Height - DrawingScrollView.ViewportHeight;

			if (widthChange != 0)
				widthChange++;

			if (Math.Abs(widthChange) < 0.5 && Math.Abs(heightChange) < 0.5)
				return;

			Width += widthChange;
			Height += heightChange;
		}

		private void OptionExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#endregion

		#region canvas handlers

		private void DrawingArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (sender == null || !(sender is Canvas))
				throw new ArgumentException("expected only Canvas", "sender");

			var canvas = (Canvas)sender;
			if (!canvas.CaptureMouse())
				throw new InvalidOperationException("cannot draw if mouse cannot captured");

			switch (tool)
			{
				case Tools.Filtering:
					{
						//Status = "painting";
						bitmapArrayWithoutLines = null;
						Lines.Clear();
					} break;
				case Tools.Line:
					{
						//Status = "adding line";

						currentLineStart = new Point(e.GetPosition(canvas).X, e.GetPosition(canvas).Y);

						if (bitmapArrayTemp == null)
						{
							currentLineEnd = currentLineStart;
							bitmapArrayTemp = new FastBitmapArray(bitmapArray);
						}
					} break;
				default:
					return;
			}

			keyIsDown = true;

			DrawingArea_MouseDownOrMove(sender, e);
		}

		private void DrawingArea_MouseMove(object sender, MouseEventArgs e)
		{
			if (keyIsDown && e.LeftButton == MouseButtonState.Released)
			{
				//Status = "ready";
				keyIsDown = false;
				ReinitializeMask();
				return;
			}

			if (!keyIsDown)
				return;

			if (sender == null || !(sender is Canvas))
				throw new ArgumentException("expected only Canvas", "sender");

			switch (tool)
			{
				case Tools.Filtering:
					{
						//Status = "painting";
						if (Shape == BrushShapes.Fill)
							return;
					} break;
				case Tools.Line:
					{
						//Status = "adding line";
					} break;
				default: return;
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

			if (!bitmapArray.IsInBounds((int)x, (int)y))
				return;

			switch (tool)
			{
				case Tools.Filtering:
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
					} break;
				case Tools.Line:
					{
						int startX = (int)currentLineStart.X;
						int startY = (int)currentLineStart.Y;
						int endX = (int)x;
						int endY = (int)y;

						if (currentLineStart != currentLineEnd)
						{
							int minX = (int)Math.Min(currentLineStart.X, currentLineEnd.X);
							int maxX = (int)Math.Max(currentLineStart.X, currentLineEnd.X);
							int minY = (int)Math.Min(currentLineStart.Y, currentLineEnd.Y);
							int maxY = (int)Math.Max(currentLineStart.Y, currentLineEnd.Y);
							bitmapArray.CopyArea(bitmapArrayTemp, minX, minY, maxX, maxY);
						}
						currentLineEnd = new Point(x, y);

						bitmapArray.DrawLine(startX, startY, endX, endY,
							lineColor.ScR, lineColor.ScG, lineColor.ScB);
						bitmapArray.RefreshBitmap(Mask.Rectangle);
					} break;
			}
		}

		private void DrawingArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (sender == null || !(sender is Canvas))
				throw new ArgumentException("expected only Canvas", "sender");

			//Status = "ready";

			var canvas = (Canvas)sender;

			if (!keyIsDown)
				return;

			switch (tool)
			{
				case Tools.Filtering:
					{
						ReinitializeMask();
						DrawingBrush.Visibility = Visibility.Hidden;
					} break;
				case Tools.Line:
					{
						double x = e.GetPosition(canvas).X;
						double y = e.GetPosition(canvas).Y;

						int startX = (int)currentLineStart.X;
						int startY = (int)currentLineStart.Y;
						int endX = (int)x;
						int endY = (int)y;

						bitmapArray.CopyArea(bitmapArrayTemp);
						if (bitmapArrayWithoutLines == null)
							bitmapArrayWithoutLines = new FastBitmapArray(bitmapArray);
						bitmapArray.DrawLine(startX, startY, endX, endY,
							lineColor.ScR, lineColor.ScG, lineColor.ScB, (int)lineThickness);
						bitmapArray.RefreshBitmap(Mask.Disabled);
						bitmapArrayTemp = null;

						Lines.Add(new Line(startX, startY, endX, endY, lineColor.ScR, lineColor.ScG, lineColor.ScB, (int)lineThickness));

						//LineAdd.IsEnabled = true;
						//LineUpdate.IsEnabled = true;

					} break;
			}

			if (canvas.IsMouseCaptured)
				canvas.ReleaseMouseCapture();

			if (keyIsDown)
				keyIsDown = false;
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
				Tool = Tools.Filtering;
				//ToolName = Filter.ToString();
				//Binding b = new Binding();
				//b.Source = this;
				//b.Path = new PropertyPath("Filter");
				//b.Mode = BindingMode.OneWay;
				//b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				//BindingOperations.SetBinding(ToolInfo, TextBlock.TextProperty, b);
			}
			//else
			//BindingOperations.ClearBinding(ToolInfo, TextBlock.TextProperty);
			else if (selected == 1)
			{
				Tool = Tools.Dithering;
				//ToolName = String.Empty;
			}
			else if (selected == 2)
			{
				Tool = Tools.Line;
				//ToolName = String.Empty;
			}
		}

		#region filters tab handlers

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

		#region dithering tab handlers

		private void ErrorDiffusion_Apply(object sender, RoutedEventArgs e)
		{
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
			var conv = new OrderedDitheringConverter();
			var convertedArray = conv.Process(bitmapArray, (int)sizeOfOrderedDitheringArray, (int)orderedDitheringColorLevels);

			MainWindow w = Clone((int)orderedDitheringColorLevels, false);
			w.SetBitmapArray(convertedArray);
			w.Show();
		}

		#endregion

		#region lines tab handlers

		private void LineAdd_Click(object sender, RoutedEventArgs e)
		{
			if (bitmapArrayWithoutLines == null)
				bitmapArrayWithoutLines = new FastBitmapArray(bitmapArray);
			var l = new Line((int)StartPointX, (int)StartPointY, (int)EndPointX, (int)EndPointY,
				lineColor.ScR, lineColor.ScG, lineColor.ScB, (int)lineThickness);

			Lines.Add(l);
			bitmapArray.DrawLine(l);
		}

		private void LineUpdate_Click(object sender, RoutedEventArgs e)
		{
			var selected = LinesListView.SelectedIndex;
			if (selected < 0)
				return;

			Lines.RemoveAt(selected);
			Lines.Insert(selected, new Line((int)StartPointX, (int)StartPointY, (int)EndPointX, (int)EndPointY,
				lineColor.ScR, lineColor.ScG, lineColor.ScB, (int)lineThickness));

			var l = lines[selected];

			//redraw lines
			bitmapArray.CopyArea(bitmapArrayWithoutLines, Math.Min(l.StartX, l.EndX) - l.Thickness,
				Math.Min(l.StartY, l.EndY) - l.Thickness, Math.Max(l.StartX, l.EndX) + l.Thickness,
				Math.Max(l.StartY, l.EndY) + l.Thickness);
			//bitmapArray.SetBatchArea(Math.Min(l.StartX, l.EndX), Math.Min(l.StartY, l.EndY),
			//	Math.Max(l.StartX, l.EndX), Math.Max(l.StartY, l.EndY));
			foreach (var line in lines)
			{
				bitmapArray.DrawLine(line.StartX, line.StartY, line.EndX, line.EndY,
					line.Red, line.Green, line.Blue, line.Thickness);
			}
			bitmapArray.RefreshBitmap(Mask.Rectangle);
		}

		private void LineDelete_Click(object sender, RoutedEventArgs e)
		{
			var selected = LinesListView.SelectedIndex;
			if (selected < 0)
				return;

			var l = lines[selected];
			Lines.RemoveAt(selected);

			if (Lines.Count > 0)
				LinesListView.SelectedIndex = Math.Max(0, selected - 1);


			// redraw lines
			bitmapArray.CopyArea(bitmapArrayWithoutLines, Math.Min(l.StartX, l.EndX) - l.Thickness,
				Math.Min(l.StartY, l.EndY) - l.Thickness, Math.Max(l.StartX, l.EndX) + l.Thickness, 
				Math.Max(l.StartY, l.EndY) + l.Thickness);
			//bitmapArray.SetBatchArea(Math.Min(l.StartX, l.EndX), Math.Min(l.StartY, l.EndY),
			//	Math.Max(l.StartX, l.EndX), Math.Max(l.StartY, l.EndY));
			foreach (var line in lines)
			{
				bitmapArray.DrawLine(line.StartX, line.StartY, line.EndX, line.EndY,
					line.Red, line.Green, line.Blue, line.Thickness);
			}
			bitmapArray.RefreshBitmap(Mask.Rectangle);
		}

		private void LinesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selected = LinesListView.SelectedIndex;

			if (selected < 0)
			{
				LineUpdate.IsEnabled = false;
				LineDelete.IsEnabled = false;
				return;
			}

			var line = Lines[selected];

			StartPointX = line.StartX;
			StartPointY = line.StartY;
			EndPointX = line.EndX;
			EndPointY = line.EndY;

			lineColor.ScR = (float)line.Red;
			lineColor.ScG = (float)line.Green;
			lineColor.ScB = (float)line.Blue;
			SendPropertyChanged("LineColor");

			LineThickness = line.Thickness;

			LineUpdate.IsEnabled = true;
			LineDelete.IsEnabled = true;
		}

		#endregion

	}
}
