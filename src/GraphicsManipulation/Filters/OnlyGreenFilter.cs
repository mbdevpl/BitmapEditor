
namespace GraphicsManipulation.Filters
{
    class OnlyGreenFilter : FilterBrush
    {
        protected override double FilterRed(double r, double g, double b)
        {
            return 0;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return g;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return 0;
        }
    }
}
