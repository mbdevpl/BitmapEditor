
namespace GraphicsManipulation.Filters
{
    class SepiaFilter : FilterBrush
    {
        protected override double FilterRed(double r, double g, double b)
        {
            return r * 1.5;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return g * 1.2;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return b * 0.4;
        }
    }
}
