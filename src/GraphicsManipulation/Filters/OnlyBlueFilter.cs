
namespace GraphicsManipulation.Filters
{
    /// <summary>
    /// Allows only the blue channel to pass through.
    /// </summary>
    class OnlyBlueFilter : FilterBrush
    {
        protected override double FilterRed(double r, double g, double b)
        {
            return 0;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return 0;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return b;
        }
    }
}
