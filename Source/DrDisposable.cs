using System;

namespace DigitalRiseModel
{
	public abstract class NrmDisposable: IDisposable
	{
		~NrmDisposable()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public abstract void Dispose(bool disposing);
	}
}
