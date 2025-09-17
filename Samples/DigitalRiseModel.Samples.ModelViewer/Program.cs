using System;

namespace DigitalRiseModel.Samples.ModelViewer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length == 0)
				{
					Console.WriteLine("Usage: ModelViewer <filePath>");
					return;
				}

				using (var game = new ViewerGame(args[0]))
				{
					game.Run();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
