using System;

namespace DigitalRiseModel
{
	/// <summary>
	/// Base class for providing a standard implementation of the IDisposable pattern.
	/// </summary>
	public abstract class DrDisposable: IDisposable
	{
		/// <summary>
		/// Finalizer that ensures resources are cleaned up.
		/// </summary>
		~DrDisposable()
		{
			Dispose(false);
		}

		/// <summary>
		/// Releases all resources used by this object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by this object, and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		public abstract void Dispose(bool disposing);
	}
}
