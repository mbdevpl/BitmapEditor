
namespace GraphicsManipulation.Filters
{
    /// <summary>
    /// A special kind of filter, which applies the same function to all three color channels.
    /// </summary>
    public abstract class SingleFilterBrush : FilterBrush
    {
        public SingleFilterBrush()
        {
            //
        }

        protected override double FilterRed(double r, double g, double b)
        {
            return Filter(r, g, b);
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return Filter(r, g, b);
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return Filter(r, g, b);
        }

        /// <summary>
        /// Filters the red, green and blue components using the same function.
        /// </summary>
        /// <param name="r">red component value, from 0 to 1</param>
        /// <param name="g">green component value, from 0 to 1</param>
        /// <param name="b">blue component value, from 0 to 1</param>
        /// <returns>updated value of the blue component, from 0 to 1</returns>
        protected abstract double Filter(double r, double g, double b);
    }
}
