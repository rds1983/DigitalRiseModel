using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace BatchConverter
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private readonly GraphicsDeviceManager _graphics;

		public Game1(string folder)
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;
			Content.RootDirectory = folder;
		}

		protected override void LoadContent()
		{
			var files = Directory.GetFiles(Content.RootDirectory, "*.xnb", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				var contentFile = file.Substring(Content.RootDirectory.Length + 1);

				// Remove extension
				var dotPosition = contentFile.LastIndexOf('.');
				if (dotPosition != -1)
				{
					contentFile = contentFile.Substring(0, dotPosition);
				}

				Model model;
				try
				{
					model = Content.Load<Model>(contentFile);
				}
				catch(Exception)
				{
					continue;
				}

				Console.WriteLine($"Processing model: {contentFile}");

				var glbFile = Path.ChangeExtension(Path.Combine(Content.RootDirectory, contentFile), "glb");
				Converter.SaveGlb(model, glbFile);
			}

			Exit();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			base.Draw(gameTime);
		}
	}
}