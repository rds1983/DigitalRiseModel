using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Tests
{
	[TestClass]
	public class TestsEnvironment
	{
		private static TestGame _game;

		public static GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

		[AssemblyInitialize]
		public static void SetUp(TestContext testContext)
		{
			_game = new TestGame();
		}
	}
}
