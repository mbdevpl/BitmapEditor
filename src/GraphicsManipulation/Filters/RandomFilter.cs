using System;

namespace GraphicsManipulation.Filters
{
    /// <summary>
    /// Creates a random effect.
    /// </summary>
    class RandomFilter : FilterBrush
    {
        private Random rand;

        public RandomFilter()
        {
            rand = new Random();
        }

        protected override double FilterRed(double r, double g, double b)
        {
            return r + rand.NextDouble() - 0.5;
        }

        protected override double FilterGreen(double r, double g, double b)
        {
            return g + rand.NextDouble() - 0.5;
        }

        protected override double FilterBlue(double r, double g, double b)
        {
            return b + rand.NextDouble() - 0.5;
        }
    }
}
