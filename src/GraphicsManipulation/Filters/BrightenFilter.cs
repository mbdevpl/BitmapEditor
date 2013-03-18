
namespace GraphicsManipulation.Filters
{
    /// <summary>
    /// Brightens the pixel.
    /// </summary>
    class BrightenFilter : FilterBrush
    {
        protected override double FilterRed(double r, double g, double b)
        {
            return r * 1.2;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return g * 1.2;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return b * 1.2;
        }
    }
}
