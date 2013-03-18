
namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Does nothing - by applying identity function. Useless, but it it a valid filter.
	/// </summary>
	class IdentityFilter : FilterBrush
	{
		protected override double FilterRed(double r, double g, double b)
		{
			return r;
		}

		protected override double FilterGreen(double r, double g, double b)
		{
			return g;
		}

		protected override double FilterBlue(double r, double g, double b)
		{
			return b;
		}
	}
}
