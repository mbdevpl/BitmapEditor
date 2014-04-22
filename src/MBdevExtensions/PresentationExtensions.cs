using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MBdev.Extensions
{
	/// <summary>
	/// Extensions for Windows Presentation Foundation controls.
	/// </summary>
	public static class PresentationExtensions
	{
		/// <summary>
		/// If the new value differs from the current private field value, this method will change
		/// the given private field value and will raise the PropertyChanged event.
		/// 
		/// Checks for equality between object using Equals()
		/// </summary>
		/// <typeparam name="T">a struct type</typeparam>
		/// <param name="sender">an object that implements INotifyPropertyChanged</param>
		/// <param name="handler">PropertyChanged event handler</param>
		/// <param name="privateField">a struct</param>
		/// <param name="newValue">a struct</param>
		/// <param name="propertyName">name of the public property</param>
		public static void ChangeProperty<T>(this INotifyPropertyChanged sender, PropertyChangedEventHandler handler,
			ref T privateField, ref T newValue, string propertyName) where T : struct
		{
			if (privateField.Equals(newValue))
				return;
			privateField = newValue;
			sender.InvokePropertyChanged(handler, propertyName);
		}

		/// <summary>
		/// If the new value is a different object than (and it is not equal to) the current private field value,
		/// this method will change the given private field value and will raise the PropertyChanged event.
		/// 
		/// Checks for equality between objects using == and then Equals()
		/// </summary>
		/// <typeparam name="T">a class</typeparam>
		/// <param name="sender">an object that implements INotifyPropertyChanged</param>
		/// <param name="handler">PropertyChanged event handler</param>
		/// <param name="privateField">instance of a class</param>
		/// <param name="newValue">instance of a class</param>
		/// <param name="propertyName">name of the public property</param>
		public static void ChangeProperty<T>(this INotifyPropertyChanged sender, PropertyChangedEventHandler handler,
			ref T privateField, T newValue, string propertyName) where T : class
		{
			if (privateField == newValue)
				return;
			if (privateField != null && privateField.Equals(newValue))
				return;
			privateField = newValue;
			sender.InvokePropertyChanged(handler, propertyName);
		}

		/// <summary>
		/// Raises the PropertyChanged event handler on an object that implements INotifyPropertyChanged.
		/// </summary>
		/// <param name="sender">an object that implements INotifyPropertyChanged</param>
		/// <param name="handler">PropertyChanged event handler</param>
		/// <param name="propertyName">name of the public property</param>
		public static void InvokePropertyChanged(this INotifyPropertyChanged sender, PropertyChangedEventHandler handler,
			string propertyName)
		{
			if (handler != null)
				handler(sender, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="window"></param>
		/// <returns></returns>
		public static string GetCompanyName(this Window window)
		{
			var assembly = window.GetType().Assembly;
			var customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
			var companyAttribute = customAttributes[0] as AssemblyCompanyAttribute;
			return companyAttribute.Company;
		}

		/// <summary>
		/// Adds a control to the canvas.
		/// </summary>
		/// <param name="canvas"></param>
		/// <param name="element"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="zIndex"></param>
		/// <returns></returns>
		public static int Add(this Canvas canvas, UIElement element, double left, double top, int zIndex)
		{
			Canvas.SetLeft(element, left);
			Canvas.SetTop(element, top);
			Canvas.SetZIndex(element, zIndex);
			return canvas.Children.Add(element);
		}

		/// <summary>
		/// Removes all controls from the canvas.
		/// </summary>
		/// <param name="canvas"></param>
		public static void Clear(this Canvas canvas)
		{
			canvas.Children.Clear();
		}

		/// <summary>
		/// Finds an element in the data grid.
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="element"></param>
		/// <param name="columnIndex"></param>
		/// <param name="rowIndex"></param>
		public static void FindElementLocation(this DataGrid grid, DependencyObject element,
			out int columnIndex, out int rowIndex)
		{
			columnIndex = -1;
			rowIndex = -1;

			if (grid == null)
				return;

			if (element == null)
				return;

			while (element != null && element is DataGridCell == false)
				element = VisualTreeHelper.GetParent(element);

			if (element == null || element is DataGridCell == false)
				return;

			DataGridCell cell = (DataGridCell)element;

			DataGridColumn column = cell.Column;

			columnIndex = grid.Columns.IndexOf(column);
			rowIndex = grid.SelectedIndex;
		}

	}
}
