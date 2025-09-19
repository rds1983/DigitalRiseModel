using AssetManagementBase;
using DigitalRiseModel;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Primitives;
using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using System;
using System.IO;
using System.Reflection;

namespace SimpleScene
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.05f;

		private readonly GraphicsDeviceManager _graphics;
		private AnimationController _player;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SpriteBatch _spriteBatch;
		private InputService _inputService;
		private readonly SceneNode _rootNode = new SceneNode();
		private readonly ModelInstanceNode _modelNode = new ModelInstanceNode
		{
			ModelInstance = new DrModelInstance()
		};
		private readonly SceneNode _cameraMount = new SceneNode();
		private readonly CameraNode _mainCamera = new CameraNode();
		private ForwardRenderer _renderer;

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public ViewerGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = false;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory, "Assets"));

			// Build up the scene
			// Plane
			var planeTexture = assetManager.LoadTexture2D(GraphicsDevice, "Textures/checker.dds");
			var planeMesh = MeshPrimitives.CreatePlaneMesh(GraphicsDevice, uScale: 50, vScale: 50, normalDirection: NormalDirection.UpY);
			var planeNode = new MeshNode
			{
				Mesh = planeMesh,
				Texture = planeTexture,
				Scale = new Vector3(200, 1, 200)
			};
			_rootNode.Children.Add(planeNode);

			// Model
			var model = assetManager.LoadGltf(GraphicsDevice, "Models/mixamo_base.glb");
			_modelNode.ModelInstance.Model = model;

			_rootNode.Children.Add(_modelNode);

			// Camera
			_cameraMount.Translation = new Vector3(0, 1.3f, 0);
			_modelNode.Children.Add(_cameraMount);

			_mainCamera.Translation = new Vector3(0, 0, -5);
			_cameraMount.Children.Add(_mainCamera);

			// Capsule
			var capsule = MeshPrimitives.CreateCapsuleMesh(GraphicsDevice);
			var capsuleNode = new MeshNode
			{
				Mesh = capsule,
				Color = Color.White,
				Translation = new Vector3(8, 2, 10)
			};
			_rootNode.Children.Add(capsuleNode);


			// Start animation
			_player = new AnimationController(_modelNode.ModelInstance);
			_player.StartClip("idle");

			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;

			_renderer = new ForwardRenderer(GraphicsDevice);
			_renderer.DirectionalLight0.Enabled = true;
			_renderer.DirectionalLight0.Direction = new Vector3(1, -1, 0);
			_renderer.DirectionalLight0.DiffuseColor = Color.White;

			_spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			var playerRotation = _modelNode.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			_modelNode.Rotation = playerRotation;

			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			var movement = 0;
			if (_inputService.IsKeyDown(Keys.W))
			{
				movement = -1;
			} else if (_inputService.IsKeyDown(Keys.S))
			{
				movement = 1;
			}

			if (_inputService.IsKeyDown(Keys.LeftShift) || _inputService.IsKeyDown(Keys.RightShift))
			{
				movement *= 2;
			}

			// Set animation
			switch(movement)
			{
				case 0:
					if (_player.AnimationClip.Name != "idle")
					{
						_player.StartClip("idle");
					}
					break;

				case 1:
				case -1:
					if (_player.AnimationClip.Name != "walking")
					{
						_player.StartClip("walking");
					}
					break;

				case 2:
				case -2:
					if (_player.AnimationClip.Name != "running")
					{
						_player.StartClip("running");
					}
					break;
			}

			// Perform the movement
			var velocity = _modelNode.GlobalTransform.Forward * movement * MovementSpeed;
			_modelNode.Translation += velocity;


			_fpsCounter.Update(gameTime);
			_player.Update(gameTime.ElapsedGameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_renderer.Render(_mainCamera, _rootNode);

			_fpsCounter.Draw(gameTime);
			_spriteBatch.Begin();

			/*			_spriteBatch.Draw(_light.ShadowMap, 
							new Rectangle(0, 0, 256, 256), 
							Color.White);*/

			_spriteBatch.End();
		}
	}
}
