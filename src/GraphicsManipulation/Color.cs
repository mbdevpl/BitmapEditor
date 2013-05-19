using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsManipulation
{
	public class Color3Ch
	{
		public double Red;
		public double Green;
		public double Blue;

		public Color3Ch() { Red = 0; Green = 0; Blue = 0; }
		public Color3Ch(double red, double green, double blue) { Red = red; Green = green; Blue = blue; }

	}

	public class Color4Ch : Color3Ch
	{

		public double Alpha;

		public Color4Ch(double red, double green, double blue, double alpha)
			: base(red, green, blue) { Alpha = alpha; }
	}

	public static class Colors4Ch
	{
		public static readonly Color4Ch Black = new Color4Ch(0, 0, 0, 1);
		public static readonly Color4Ch White = new Color4Ch(1, 1, 1, 1);
		public static readonly Color4Ch Red = new Color4Ch(1, 0, 0, 1);
		public static readonly Color4Ch Green = new Color4Ch(0, 1, 0, 1);
		public static readonly Color4Ch Blue = new Color4Ch(0, 0, 1, 1);
	}

}
