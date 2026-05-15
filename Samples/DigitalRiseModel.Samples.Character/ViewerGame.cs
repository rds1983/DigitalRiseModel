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
		private ControllerService _controllerService;
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

		/// <summary>Singleton instance of the ViewerGame for global access.</summary>
		public static ViewerGame Instance { get; private set; }

		/// <summary>
		/// Initializes the game with graphics settings and input configuration.
		/// </summary>
		public ViewerGame()
		{
			// Register singleton instance for global access throughout the application
			Instance = this;

			// Configure graphics: 1200x800 resolution with vsync enabled by default
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			// Allow user to resize the game window and show the mouse cursor initially
			// Note: Mouse confinement and visibility is dynamically managed by InputService during gameplay
			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			// When /nf flag is used, disable fixed timestep to run at maximum performance
			// (useful for performance profiling and benchmarking)
			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		/// <summary>
		/// Loads all game content including assets, scene nodes, and UI elements.
		/// This is called once at game startup after the graphics device is initialized.
		/// </summary>
		protected override void LoadContent()
		{
			base.LoadContent();

			// Initialize the asset manager to load textures, models, and other resources from the Assets folder
			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));

			// === Build scene hierarchy ===

			// Create a textured ground plane (checkerboard pattern, 200x200 units)
			var planeTexture = assetManager.LoadTexture2D(GraphicsDevice, "Textures/checker.dds");
			var planeMesh = MeshPrimitives.CreatePlaneMesh(GraphicsDevice, uScale: 50, vScale: 50, normalDirection: NormalDirection.UpY);
			var planeNode = new MeshNode
			{
				Mesh = planeMesh,
				Texture = planeTexture,
				Scale = new Vector3(200, 1, 200)
			};
			_rootNode.Children.Add(planeNode);

			// Load and add the animated character model (mixamo format)
			var model = assetManager.LoadModel(GraphicsDevice, "Models/mixamo.gltf");
			_modelNode.ModelInstance.Model = model;
			_rootNode.Children.Add(_modelNode);

			// === Camera setup ===
			// Position camera mount on the model's head (1.3 units up from model origin)
			// This allows the camera to follow the character's head position as the model moves
			_cameraMount.Translation = new Vector3(0, 1.3f, 0);
			_modelNode.Children.Add(_cameraMount);

			// Position camera 2 units behind the camera mount (first-person view)
			_mainCamera.Translation = new Vector3(0, 0, -2);
			_cameraMount.Children.Add(_mainCamera);

			// Add a white capsule mesh as a debug/demo object in the scene
			var capsule = MeshPrimitives.CreateCapsuleMesh(GraphicsDevice);
			var capsuleNode = new MeshNode
			{
				Mesh = capsule,
				Color = Color.White,
				Translation = new Vector3(8, 2, 10)
			};
			_rootNode.Children.Add(capsuleNode);

			// === Initialize animation controller ===
			// The controller manages character animations (idle, run, jump, weapon draw/sheathe)
			_controllerService = new ControllerService(_modelNode);

			// === Initialize input system ===
			// Handles keyboard and mouse input with event-based feedback
			// Mouse is initially locked (confined to window and hidden) for first-person camera control
			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;
			_inputService.KeyDown += _inputService_KeyDown;
			_inputService.MouseLocked = true;

			// === Initialize rendering ===
			// Create the forward renderer and configure directional lighting
			_renderer = new ForwardRenderer(GraphicsDevice);
			_renderer.DirectionalLight0.Enabled = true;
			_renderer.DirectionalLight0.Direction = new Vector3(1, -1, 0); // Light from top-right
			_renderer.DirectionalLight0.DiffuseColor = Color.White;

			// === Initialize UI ===
			// Create the UI panel that displays FPS and rendering statistics
			MyraEnvironment.Game = this;
			_desktop = new Desktop();
			_mainPanel = new MainPanel();
			_desktop.Root = _mainPanel;
		}

		/// <summary>
		/// Handles keyboard input events.
		/// Escape key toggles mouse lock to allow switching between first-person camera control and free mouse.
		/// </summary>
		private void _inputService_KeyDown(object sender, KeyEventsArgs e)
		{
			// Escape toggles between locked mouse (FPS camera control) and free mouse (UI interaction)
			if (e.Key == Keys.Escape)
			{
				_inputService.MouseLocked = !_inputService.MouseLocked;
			}
		}

		/// <summary>
		/// Handles mouse movement events to rotate the character and camera.
		/// Only processes input when mouse is locked for first-person camera control.
		/// </summary>
		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			// Only apply rotation when mouse is locked (confined to window for FPS-style control)
			if (!_inputService.MouseLocked)
			{
				return;
			}

			// Horizontal mouse movement rotates the character around the Y axis (look left/right)
			var playerRotation = _modelNode.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			_modelNode.Rotation = playerRotation;

			// Vertical mouse movement rotates the camera mount around the X axis (look up/down)
			// This creates a standard first-person camera that pitches while the character yaws
			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		/// <summary>
		/// Updates game logic each frame.
		/// Processes input, updates character animations and position, and updates the FPS counter.
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// Update the input service to process current keyboard and mouse states
			_inputService.Update();

			// === Process WASD movement input ===
			// Calculate movement direction relative to character's current facing direction
			var isRunning = false;
			var velocity = Vector3.Zero;

			if (_inputService.IsKeyDown(Keys.W))
			{
				// W moves forward in the direction the character is facing
				velocity = _modelNode.GlobalTransform.Forward * -MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				// S moves backward
				velocity = _modelNode.GlobalTransform.Forward * MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.A))
			{
				// A strafes left
				velocity = _modelNode.GlobalTransform.Right * MovementSpeed;
				isRunning = true;
			}
			else if (_inputService.IsKeyDown(Keys.D))
			{
				// D strafes right
				velocity = _modelNode.GlobalTransform.Right * -MovementSpeed;
				isRunning = true;
			}

			// === Process Space for jumping ===
			if (_inputService.IsKeyDown(Keys.Space))
			{
				_controllerService.Jump(velocity);
			}

			// === Process R key to toggle weapon draw/sheathe ===
			if (_inputService.IsKeyDown(Keys.R))
			{
				if (_controllerService.WeaponDrawn)
				{
					_controllerService.SheathWeapon();
				}
				else
				{
					_controllerService.DrawWeapon();
				}
			}

			// === Update character animation state ===
			// Pass movement velocity and running state to the animation controller
			if (isRunning)
			{
				_controllerService.Run(velocity);
			}
			else
			{
				_controllerService.Idle();
			}

			// Update the animation controller (handles animation blending and transitions)
			_controllerService.Update(gameTime.ElapsedGameTime);

			// Update the FPS counter for display
			_fpsCounter.Update(gameTime);
		}

		/// <summary>
		/// Renders the frame using forward rendering with lighting and UI overlay.
		/// </summary>
		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			// Clear the backbuffer to black
			GraphicsDevice.Clear(Color.Black);

			// Render the 3D scene from the main camera perspective
			_renderer.Render(_mainCamera, _rootNode);

			// Update and draw the FPS counter
			_fpsCounter.Draw(gameTime);

			// === Update UI with runtime statistics ===
			// Display FPS (frames per second)
			_mainPanel._labelFPS.Text = $"FPS: {_fpsCounter.FramesPerSecond}";

			// Display rendering statistics for performance monitoring
			var stats = _renderer.Statistics;
			_mainPanel._labelDrawCalls.Text = stats.DrawCalls.ToString();
			_mainPanel._labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_mainPanel._labelMeshesDrawn.Text = stats.MeshesDrawn.ToString();
			_mainPanel._labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_mainPanel._labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();

			// Render the UI on top of the 3D scene
			_desktop.Render();
		}
	}
}
