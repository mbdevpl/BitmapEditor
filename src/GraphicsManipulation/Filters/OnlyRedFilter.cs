
namespace GraphicsManipulation.Filters
{
    class OnlyRedFilter : FilterBrush
    {
        protected override double FilterRed(double r, double g, double b)
        {
            return r;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return 0;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return 0;
        }
    }
}
