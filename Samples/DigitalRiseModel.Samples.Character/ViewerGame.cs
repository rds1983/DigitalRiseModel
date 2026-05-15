using AssetManagementBase;
using DigitalRiseModel.Primitives;
using DigitalRiseModel.Samples.BasicEngine;
using DigitalRiseModel.Samples.Character.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using System;
using System.Diagnostics;
using System.IO;

namespace DigitalRiseModel.Samples.Character
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.1f;

		private readonly GraphicsDeviceManager _graphics;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private InputService _inputService;
		private CharacterService _controllerService;
		private readonly SceneNode _rootNode = new SceneNode();
		private readonly SceneNode _cameraMount = new SceneNode();
		private readonly CameraNode _mainCamera = new CameraNode();
		private ForwardRenderer _renderer;
		private Desktop _desktop;
		private MainPanel _mainPanel;

		/// <summary>Singleton instance of the ViewerGame for global access.</summary>
		public static ViewerGame Instance { get; private set; }

		private ModelInstanceNode ModelNode => _controllerService.ModelNode;

		/// <summary>Initializes game with graphics and input configuration.</summary>
		public ViewerGame()
		{
			Instance = this;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		/// <summary>Loads all game content and scene setup.</summary>
		protected override void LoadContent()
		{
			base.LoadContent();

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));

			// Build scene hierarchy
			// Ground plane with checkerboard texture
			var planeTexture = assetManager.LoadTexture2D(GraphicsDevice, "Textures/checker.dds");
			var planeMesh = MeshPrimitives.CreatePlaneMesh(GraphicsDevice, uScale: 50, vScale: 50, normalDirection: NormalDirection.UpY);
			var planeNode = new MeshNode
			{
				Mesh = planeMesh,
				Texture = planeTexture,
				Scale = new Vector3(200, 1, 200)
			};
			_rootNode.Children.Add(planeNode);

			_controllerService = new CharacterService(GraphicsDevice, assetManager);
			_rootNode.Children.Add(_controllerService.ModelNode);

			// Setup camera on character's head
			_cameraMount.Translation = new Vector3(0, 1.3f, 0);
			ModelNode.Children.Add(_cameraMount);
			_mainCamera.Translation = new Vector3(0, 0, -2);
			_cameraMount.Children.Add(_mainCamera);

			// Demo capsule object
			var capsule = MeshPrimitives.CreateCapsuleMesh(GraphicsDevice);
			var capsuleNode = new MeshNode
			{
				Mesh = capsule,
				Color = Color.White,
				Translation = new Vector3(8, 2, 10)
			};
			_rootNode.Children.Add(capsuleNode);

			// Input system with locked mouse for FPS control
			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;
			_inputService.KeyDown += _inputService_KeyDown;
			_inputService.MouseLocked = true;

			// Rendering with directional lighting
			_renderer = new ForwardRenderer(GraphicsDevice);
			_renderer.DirectionalLight0.Enabled = true;
			_renderer.DirectionalLight0.Direction = new Vector3(1, -1, 0);
			_renderer.DirectionalLight0.DiffuseColor = Color.White;

			// UI panel
			MyraEnvironment.Game = this;
			_desktop = new Desktop();
			_mainPanel = new MainPanel();
			_desktop.Root = _mainPanel;
		}

		/// <summary>Handles keyboard events (Escape toggles mouse lock).</summary>
		private void _inputService_KeyDown(object sender, KeyEventsArgs e)
		{
			// Escape toggles between locked mouse (FPS camera control) and free mouse (UI interaction)
			if (e.Key == Keys.Escape)
			{
				_inputService.MouseLocked = !_inputService.MouseLocked;
			}
		}

		/// <summary>Rotates character/camera based on mouse movement (when locked).</summary>
		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			if (!_inputService.MouseLocked)
				return;

			var playerRotation = ModelNode.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			ModelNode.Rotation = playerRotation;

			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		/// <summary>Updates game logic: input, animations, FPS counter.</summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			// Process WASD movement
			var isRunning = false;
			var velocity = Vector3.Zero;

			if (_inputService.IsKeyDown(Keys.W))
			{
				velocity = ModelNode.GlobalTransform.Forward * -MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				velocity = ModelNode.GlobalTransform.Forward * MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.A))
			{
				velocity = ModelNode.GlobalTransform.Right * MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.D))
			{
				velocity = ModelNode.GlobalTransform.Right * -MovementSpeed;
				isRunning = true;
			}

			if (_inputService.IsKeyDown(Keys.Space))
				_controllerService.Jump(velocity);

			if (_inputService.IsKeyDown(Keys.LeftShift))
				_controllerService.Slash();

			if (_inputService.IsKeyDown(Keys.R))
			{
				if (_controllerService.WeaponDrawn)
					_controllerService.SheathWeapon();
				else
					_controllerService.DrawWeapon();
			}

			if (isRunning)
				_controllerService.Run(velocity);
			else
				_controllerService.Idle();

			_controllerService.Update(gameTime.ElapsedGameTime);
			_fpsCounter.Update(gameTime);
		}

		/// <summary>Renders 3D scene and UI overlay.</summary>
		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);
			_renderer.Render(_mainCamera, _rootNode);
			_fpsCounter.Draw(gameTime);

			// Update UI statistics
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
