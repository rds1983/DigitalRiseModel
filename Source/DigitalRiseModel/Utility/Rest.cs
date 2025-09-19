using System;

namespace DigitalRiseModel.Utility
{
	internal static class Rest
	{
		/// <summary>
		/// Safely disposes the object.
		/// </summary>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <param name="obj">The object to dispose. Can be <see langword="null"/>.</param>
		/// <remarks>
		/// The method calls <see cref="IDisposable.Dispose"/> if the <paramref name="obj"/> is not null
		/// and implements the interface <see cref="IDisposable"/>.
		/// </remarks>
		public static void SafeDispose<T>(this T obj) where T : class
		{
			var disposable = obj as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		/// <summary>
		/// Swaps the content of two variables.
		/// </summary>
		/// <typeparam name="T">The type of the objects.</typeparam>
		/// <param name="obj1">First variable.</param>
		/// <param name="obj2">Second variable.</param>
		public static void Swap<T>(ref T obj1, ref T obj2)
		{
			T temp = obj1;
			obj1 = obj2;
			obj2 = temp;
		}
	}
}
