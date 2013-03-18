
namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Converts an image to the grayscale.
	/// </summary>
	public class GrayscaleFilter : SingleFilterBrush
	{
		protected override double Filter(double r, double g, double b)
		{
			return r * 0.299 + g * 0.589 + b * 0.112;
		}
	}
}
