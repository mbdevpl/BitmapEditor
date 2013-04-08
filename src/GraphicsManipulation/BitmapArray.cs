using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GraphicsManipulation.Filters;

namespace GraphicsManipulation
{

	///// <summary>
	///// Contains four two-dimensional arrays containing red, green and blue channels
	///// of a source image. In essence manipulation of such data is very easy, especially 
	///// with applying mathematical functions to pixel data.
	///// </summary>
	//public class BitmapArray
	//{

	//	private WriteableBitmap bitmap = null;

	//	private byte[] pixelBytes = null;

	//	private int bytesPerPixel = 0;

	//	private int _width = 0;
	//	private int width
	//	{
	//		get { return _width; }
	//		set
	//		{
	//			//Free
	//			//FreeRGB();
	//			_width = value;
	//			//AllocRGB();
	//		}
	//	}
	//	public int Width
	//	{
	//		get { return width; }
	//	}

	//	private int height = 0;
	//	public int Height
	//	{
	//		get { return height; }
	//	}

	//	private byte[][] R = null;
	//	public byte[][] Red
	//	{
	//		get { return R; }
	//	}

	//	private byte[][] G = null;
	//	public byte[][] Green
	//	{
	//		get { return G; }
	//	}

	//	private byte[][] B = null;
	//	public byte[][] Blue
	//	{
	//		get { return B; }
	//	}

	//	private byte[][] A = null;
	//	public byte[][] Alpha
	//	{
	//		get { return A; }
	//	}

	//	public BitmapArray(BitmapSource source)
	//	{
	//		if (!source.Format.Equals(PixelFormats.Pbgra32))
	//			source = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);

	//		UpdateData(source);
	//	}

	//	~BitmapArray()
	//	{
	//		FreePixelBytes();
	//		FreeRGB();
	//		height = 0;
	//		width = 0;
	//		bytesPerPixel = 0;
	//		bitmap = null;
	//	}

	//	private void UpdateData(BitmapSource source = null)
	//	{
	//		if (source == null)
	//		{
	//			//this means that simply RGB data was changed and the
	//			//pixel bytes array data needs to be updated
	//			UpdatePixelBytes();
	//		}
	//		else
	//		{
	//			//this means that the new image is loaded into this object,
	//			//and all data needs to be updated, first the pixel bytes
	//			//then RGB arrays
	//			if (width != source.PixelWidth || height != source.PixelHeight)
	//			{
	//				width = source.PixelWidth;
	//				height = source.PixelHeight;
	//				bytesPerPixel = source.Format.BitsPerPixel / 8;

	//				bitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY,
	//								source.Format, source.Palette);
	//			}
	//			UpdatePixelBytes(source);
	//			UpdateRGB();
	//		}

	//		//bitmap is NOT updated every time some data is change either in RGB arrays 
	//		//or pixel bytes
	//		//bitmap.Lock();
	//		//bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelBytes,
	//		//    width * bytesPerPixel, 0);
	//		//bitmap.Unlock();
	//	}

	//	private void AllocPixelBytes()
	//	{
	//		if (height == 0 || width == 0 || bytesPerPixel == 0)
	//			throw new Exception("Cannot allocate memory for pixel bytes.");

	//		if (pixelBytes != null)
	//			FreePixelBytes();

	//		pixelBytes = new byte[height * width * bytesPerPixel];
	//	}

	//	private void FreePixelBytes()
	//	{
	//		if (pixelBytes == null)
	//			return;
	//		//for (int i = 0; i < width; i++)
	//		//{
	//		//    for (int j = 0; j < height; j++)
	//		//    {

	//		//    }
	//		//}
	//		pixelBytes = null;
	//	}

	//	private void UpdatePixelBytes(BitmapSource source = null)
	//	{
	//		if (width == 0 || height == 0 || bytesPerPixel == 0)
	//			throw new Exception("no image loaded for conversion to pixel bytes");

	//		if (pixelBytes == null)
	//			AllocPixelBytes();

	//		if (pixelBytes.Length != width * height * bytesPerPixel)
	//		{
	//			FreePixelBytes();
	//			AllocPixelBytes();
	//		}

	//		if (source != null)
	//		{
	//			source.CopyPixels(pixelBytes, width * bytesPerPixel, 0);
	//		}
	//		else
	//		{
	//			for (int i = 0; i < width; i++)
	//			{
	//				for (int j = 0; j < height; j++)
	//				{
	//					int index = (i + j * width) * 4;
	//					pixelBytes[index + 2] = R[i][j];
	//					pixelBytes[index + 1] = G[i][j];
	//					pixelBytes[index + 0] = B[i][j];
	//					pixelBytes[index + 3] = A[i][j];
	//				}
	//			}
	//		}
	//	}

	//	private void AllocRGB()
	//	{
	//		if (width == 0 || height == 0)
	//			throw new Exception("cannot allocate RGB arrays for nonexisting image");

	//		R = new byte[width][];
	//		G = new byte[width][];
	//		B = new byte[width][];
	//		A = new byte[width][];

	//		for (int i = 0; i < width; i++)
	//		{
	//			R[i] = new byte[height];
	//			G[i] = new byte[height];
	//			B[i] = new byte[height];
	//			A[i] = new byte[height];
	//		}
	//	}

	//	private void FreeRGB()
	//	{
	//		if (R == null)
	//			return;

	//		int width = R.Length;
	//		for (int i = 0; i < width; i++)
	//		{
	//			//for (int j = 0; j < height; j++)
	//			//{
	//			//}
	//			R[i] = null;
	//			G[i] = null;
	//			B[i] = null;
	//			A[i] = null;
	//		}

	//		R = null;
	//		G = null;
	//		B = null;
	//		A = null;
	//	}

	//	private void UpdateRGB()
	//	{
	//		//if (pixelBytes.Length != width * height * bytesPerPixel)
	//		//	throw new Exception("Pixel bytes array is out of date.");

	//		if (width == 0 || height == 0)
	//			throw new Exception("no image loaded for conversion to RGB arrays");

	//		if (R == null || R.Length != width || R[0].Length != height)
	//		{
	//			FreeRGB();
	//			AllocRGB();
	//		}

	//		for (int i = 0; i < width; i++)
	//		{
	//			for (int j = 1; j < height; j++)
	//			{
	//				int index = (i + j * width) * 4;
	//				R[i][j] = pixelBytes[index + 2];
	//				G[i][j] = pixelBytes[index + 1];
	//				B[i][j] = pixelBytes[index + 0];
	//				A[i][j] = pixelBytes[index + 3];
	//			}
	//		}
	//	}

	//	//public void Filter(FilterTypes filter, BrushShapes shape, Point point, int size,
	//	//	 bool[][] mask)
	//	//{
	//	//	if (filter == FilterTypes.Identity)
	//	//		new IdentityFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.Inverse)
	//	//		new InverseFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.OnlyRed)
	//	//		new OnlyRedFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.OnlyGreen)
	//	//		new OnlyGreenFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.OnlyBlue)
	//	//		new OnlyBlueFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.Grayscale)
	//	//		new GrayscaleFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.Sepia)
	//	//		new SepiaFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.Brighten)
	//	//		new BrightenFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.Darken)
	//	//		new DarkenFilter().ApplyAt(this, shape, point, size, mask);
	//	//	else if (filter == FilterTypes.Random)
	//	//		new RandomFilter().ApplyAt(this, shape, point, size, mask);
	//	//}

	//	public BitmapSource GetImageCopy()
	//	{
	//		UpdateData();

	//		bitmap.Lock();
	//		bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelBytes,
	//			 width * bytesPerPixel, 0);
	//		bitmap.Unlock();

	//		WriteableBitmap copy = new WriteableBitmap(bitmap);
	//		copy.Freeze();

	//		//WriteableBitmap wb = new WriteableBitmap(pixWidth, pixHeight);
	//		//wb.LoadJpeg(//your image stream from IsolatedStorage);
	//		//MemoryStream ms = new MemoryStream();
	//		//bitmap.SaveJpeg(ms, (int)image1.Width, (int)image1.Height, 0, 100);
	//		//BitmapImage bmp = new BitmapImage();
	//		//bmp.StreamSource = ms;

	//		return copy;
	//	}

	//}

}
