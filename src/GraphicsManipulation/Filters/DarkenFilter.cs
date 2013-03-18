
namespace GraphicsManipulation.Filters
{
    /// <summary>
    /// Darkens the image.
    /// </summary>
    public class DarkenFilter : FilterBrush
    {
        protected override double FilterRed(double r, double g, double b)
        {
            return r * 0.8;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return g * 0.8;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return b * 0.8;
        }
    }
}
