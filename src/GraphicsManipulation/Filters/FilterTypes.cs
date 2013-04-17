
namespace GraphicsManipulation.Filters
{
	/// <summary>
	/// Enumerates available types of filters.
	/// </summary>
	public enum FilterTypes
	{
		/// <summary>
		/// Identity filter, does nothing.
		/// </summary>
		Identity,
		/// <summary>
		/// Creates the negative of the picture.
		/// </summary>
		Inverse,
		OnlyRed, OnlyGreen, OnlyBlue,
		/// <summary>
		/// Changes the picture into monochromatic one.
		/// </summary>
		Grayscale,
		Sepia,
		Brighten,
		Darken,
		Random,
		/// <summary>
		/// The filter is to be defined by the user at runtime.
		/// </summary>
		Custom
	}
}
