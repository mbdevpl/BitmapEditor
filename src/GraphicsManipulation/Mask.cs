using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsManipulation {

	/// <summary>
	/// Possible kinds of mask applied when refreshing the underlying image for a bitmap array.
	/// </summary>
	public enum Mask {
		Disabled = 0,
		Rectangle = 1,
		Circle = 2,
		PerPixel = 4
	}

}
