using AssetManagementBase;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Primitives;
using DigitalRiseModel.Samples.BasicEngine;
using DigitalRiseModel.Samples.Character.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using System;
using System.IO;

namespace DigitalRiseModel.Samples.Character
{
	public class ViewerGame : Game
	{
		/// <summary>
		/// Represents the main character animation state.
		/// </summary>
		private enum AnimationState
		{
			/// <summary>Character is standing still (IdleBase + IdleTop).</summary>
			Idle,
			/// <summary>Character is moving (RunBase + RunTop).</summary>
			Running,
			/// <summary>Character is jumping (JumpStart → JumpLoop → JumpEnd).</summary>
			Jumping,
		}

		/// <summary>
		/// Represents the phase within the jumping animation sequence.
		/// </summary>
		private enum JumpState
		{
			/// <summary>Playing the initial jump animation.</summary>
			Start,
			/// <summary>Looping while in the air.</summary>
			Loop,
			/// <summary>Playing the landing animation.</summary>
			Land
		}

		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.1f;
		private const float JumpForce = 0.5f;
		private const float Gravity = 0.015f;
		private const float DefaultY = 0.0f;

		private readonly GraphicsDeviceManager _graphics;
		private AnimationController _player;
		private AnimationState _animationState = AnimationState.Idle;
		private JumpState _jumpState = JumpState.Start;
		private float _jumpVelocity = 0.0f;
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

		/// <summary>Singleton instance of the ViewerGame for global access.</summary>
		public static ViewerGame Instance { get; private set; }

		public ViewerGame()
		{
			// Register singleton instance
			Instance = this;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			// Mouse confinement and visibility is managed by InputService
			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));

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

			// Model - Load Sinbad model
			var model = assetManager.LoadModel(GraphicsDevice, "Models/mixamo.glb");
			_modelNode.ModelInstance.Model = model;
			_modelNode.Translation = new Vector3(0, DefaultY, 0);

			_rootNode.Children.Add(_modelNode);

			// Camera
			_cameraMount.Translation = new Vector3(0, 1.3f, 0);
			_modelNode.Children.Add(_cameraMount);

			_mainCamera.Translation = new Vector3(0, 0, -2);
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
			_player.StartClip("Idle", true);

			// Init input service with mouse confinement enabled
			// Mouse is initially locked to window bounds and hidden
			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;
			_inputService.KeyDown += _inputService_KeyDown;
			_inputService.MouseLocked = true;

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

		/// <summary>Handles keyboard input events. Press Escape to toggle mouse lock.</summary>
		private void _inputService_KeyDown(object sender, KeyEventsArgs e)
		{
			// Escape key toggles mouse confinement and visibility
			if (e.Key == Keys.Escape)
			{
				_inputService.MouseLocked = !_inputService.MouseLocked;
			}
		}

		/// <summary>Handles mouse movement events to rotate player and camera.</summary>
		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			// Only process mouse input when mouse is locked/confined to window
			if (!_inputService.MouseLocked)
			{
				return;
			}

			// Rotate player based on horizontal mouse movement
			var playerRotation = _modelNode.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			_modelNode.Rotation = playerRotation;

			// Rotate camera based on vertical mouse movement
			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		private void SetLandAnimation(bool isMoving)
		{
			if (isMoving && _animationState != AnimationState.Running)
			{
				_animationState = AnimationState.Running;
				_player.CrossfadeToClip("Running", TimeSpan.FromSeconds(0.1), true);
			}
			else if (!isMoving && _animationState != AnimationState.Idle)
			{
				_animationState = AnimationState.Idle;
				_player.CrossfadeToClip("Idle", TimeSpan.FromSeconds(0.1), true);
			}
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			// Determine movement direction and velocity based on input (WASD keys)
			var isMoving = false;
			var velocity = Vector3.Zero;

			if (_inputService.IsKeyDown(Keys.W))
			{
				velocity = _modelNode.GlobalTransform.Forward * -MovementSpeed;
				isMoving = true;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				velocity = _modelNode.GlobalTransform.Forward * MovementSpeed;
				isMoving = true;
			}
			else if (_inputService.IsKeyDown(Keys.A))
			{
				velocity = _modelNode.GlobalTransform.Right * MovementSpeed;
				isMoving = true;
			}
			else if (_inputService.IsKeyDown(Keys.D))
			{
				velocity = _modelNode.GlobalTransform.Right * -MovementSpeed;
				isMoving = true;
			}

			// Initiate jump sequence when Space is pressed and not already jumping
			if (_inputService.IsKeyDown(Keys.Space) && _animationState != AnimationState.Jumping)
			{
				_jumpVelocity = JumpForce;
				_animationState = AnimationState.Jumping;
				_jumpState = JumpState.Start;
				_player.CrossfadeToClip("JumpingStart", TimeSpan.FromSeconds(0.2), false);
			}

			// Handle jump physics and animation state transitions
			if (_animationState == AnimationState.Jumping)
			{
				// Apply gravity during jump phases (not during landing)
				if (_jumpState != JumpState.Land)
				{
					_jumpVelocity -= Gravity;
					_modelNode.Translation += Vector3.Up * _jumpVelocity;
				}

				// State machine for jump animation phases
				switch (_jumpState)
				{
					case JumpState.Start:
						// Transition to loop animation after start animation finishes
						if (_player.HasFinished)
						{
							_jumpState = JumpState.Loop;
							_player.CrossfadeToClip("JumpingLoop", TimeSpan.FromSeconds(0.1), true);
						}
						break;

					case JumpState.Loop:
						// Detect landing and transition to land animation
						if (_modelNode.Translation.Y <= DefaultY)
						{
							_modelNode.Translation = new Vector3(_modelNode.Translation.X, DefaultY, _modelNode.Translation.Z);
							_jumpState = JumpState.Land;
							_player.CrossfadeToClip("JumpingEnd", TimeSpan.FromSeconds(0.1), false);
						}
						break;

					case JumpState.Land:
						// Transition back to idle or running after landing animation completes
						if (_player.HasFinished)
						{
							SetLandAnimation(isMoving);
						}
						break;
				}
			}
			else
			{
				// Handle idle and running animation transitions with smooth crossfading
				SetLandAnimation(isMoving);
			}

			if (isMoving)
			{
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
