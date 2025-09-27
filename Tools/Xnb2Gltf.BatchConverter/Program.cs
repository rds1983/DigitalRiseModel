using System;

namespace BatchConverter
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: xnb2gltf-bc <folder>");
				return;
			}

			var folder = args[0];
			using (var game = new Game1(folder))
				game.Run();
		}
	}
}
