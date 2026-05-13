using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Tests
{
	public class TestsEnvironment
	{
		private static TestGame _game;

		public static GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

		static TestsEnvironment()
		{
			_game = new TestGame();
		}
	}
}
