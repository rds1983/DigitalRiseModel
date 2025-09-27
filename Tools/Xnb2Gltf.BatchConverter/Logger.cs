using System;

namespace BatchConverter
{
	internal static class Logger
	{
		public static void LogMessage(string message) => Console.WriteLine(message);
		public static void LogWarning(string message) => Console.Write("Warning: " + message);
		public static void LogError(string message) => Console.Write("Error: " + message);
	}
}
