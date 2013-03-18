
namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Enables creation of a negative of an image.
	/// </summary>
	class InverseFilter : FilterBrush
	{
		protected override double FilterRed(double r, double g, double b)
		{
			return 1 - r;
		}

		protected override double FilterGreen(double r, double g, double b)
		{
			return 1 - g;
		}

		protected override double FilterBlue(double r, double g, double b)
		{
			return 1 - b;
		}
	}
}
