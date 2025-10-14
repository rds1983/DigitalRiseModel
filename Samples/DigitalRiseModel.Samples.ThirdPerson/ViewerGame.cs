using AssetManagementBase;
using DigitalRiseModel;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Primitives;
using DigitalRiseModel.Samples.BasicEngine;
using DigitalRiseModel.Samples.ThirdPerson.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using System;
using System.IO;
using System.Reflection;

namespace DigitalRiseModel.Samples.ThirdPerson
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.05f;

		private readonly GraphicsDeviceManager _graphics;
		private AnimationController _player;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private InputService _inputService;
		private readonly SceneNode _rootNode = new SceneNode();
		private readonly ModelInstanceNode _modelNode = new ModelInstanceNode
		{
			ModelInstance = new DrModelInstance()
		};
		private readonly SceneNode _cameraMount = new SceneNode();
		private readonly CameraNode _mainCamera = new CameraNode();
		private ForwardRenderer _renderer;
		private Desktop _desktop;
		private MainPanel _mainPanel;

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
			var model = assetManager.LoadModel(GraphicsDevice, "Models/mixamo_base.glb");
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

			// Init input service
			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;

			// Init forward renderer
			_renderer = new ForwardRenderer(GraphicsDevice);
			_renderer.DirectionalLight0.Enabled = true;
			_renderer.DirectionalLight0.Direction = new Vector3(1, -1, 0);
			_renderer.DirectionalLight0.DiffuseColor = Color.White;

			// Init ui
			MyraEnvironment.Game = this;

			_desktop = new Desktop();
			_mainPanel = new MainPanel();
			_desktop.Root = _mainPanel;
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

			var forward = true;
			var speed = 0;
			if (_inputService.IsKeyDown(Keys.W))
			{
				speed = -1;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				speed = 1;
			}
			else if (_inputService.IsKeyDown(Keys.A))
			{
				speed = 1;
				forward = false;
			}
			else if (_inputService.IsKeyDown(Keys.D))
			{
				speed = -1;
				forward = false;
			}

			if (_inputService.IsKeyDown(Keys.LeftShift) || _inputService.IsKeyDown(Keys.RightShift))
			{
				speed *= 2;
			}

			// Set animation
			switch(speed)
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

			if (speed != 0)
			{
				// Perform the movement
				Vector3 velocity;

				if (forward)
				{
					velocity = _modelNode.GlobalTransform.Forward * speed * MovementSpeed;
				}
				else
				{
					velocity = _modelNode.GlobalTransform.Right * speed * MovementSpeed;
				}

				_modelNode.Translation += velocity;
			}


			_fpsCounter.Update(gameTime);
			_player.Update(gameTime.ElapsedGameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_renderer.Render(_mainCamera, _rootNode);

			_fpsCounter.Draw(gameTime);

			_mainPanel._labelFPS.Text = $"FPS: {_fpsCounter.FramesPerSecond}";

			var stats = _renderer.Statistics;
			_mainPanel._labelDrawCalls.Text = stats.DrawCalls.ToString();
			_mainPanel._labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_mainPanel._labelMeshesDrawn.Text = stats.MeshesDrawn.ToString();
			_mainPanel._labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_mainPanel._labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();

			_desktop.Render();
		}
	}
}
