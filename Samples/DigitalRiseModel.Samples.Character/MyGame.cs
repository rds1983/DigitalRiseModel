using AssetManagementBase;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Primitives;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace DigitalRiseModel.Samples;

public class MyGame : Game
{
	// Animation states for the hero character
	private enum AnimationState
	{
		Idle,      // Standing still
		Running,   // Moving
		Jumping,   // Jump startup
		Landing    // Falling/landing
	}

	private enum WeaponState
	{
		Sheathed,  // Sword on back
		Drawing,   // Draw animation playing
		Drawn,     // Sword in hand
		Sheathing, // Sheath animation playing
		Slashing   // Slash attack animation playing
	}

	// Mouse look sensitivity multiplier
	private const float MouseSensitivity = 0.2f;
	// Movement speed per frame
	private const float MovementSpeed = 0.1f;
	// Camera near clipping plane
	private const float NearPlaneDistance = 0.1f;
	// Camera far clipping plane
	private const float FarPlaneDistance = 1000.0f;
	// Camera field of view in degrees
	private const float ViewAngle = 60.0f;
	// Hero ground height
	private const float DefaultY = 0;
	// Jump gravity acceleration per second
	private const float Gravity = 12f;
	// Jump initial velocity
	private const float JumpForce = 10f;
	// Duration for animation transitions between clips
	private static readonly TimeSpan AnimationCrossfadeDelay = TimeSpan.FromSeconds(0.2f);
	// Sword local transform: position offset (-12, 0, -20), scale 16x, rotated 180 degrees on Z axis (sheathed on back)
	private static readonly Matrix SwordSheathedTransform = ToMatrix(new Vector3(-12, 0, -20), new Vector3(16), 0, 0, 180);
	// Sword local transform: position offset (50, 0, 0), scale 16x, rotated 270 degrees on Z axis (drawn in hand)
	private static readonly Matrix SwordDrawnTransform = ToMatrix(new Vector3(50f, 0f, 0f), new Vector3(16), 0, 0, 270);
	private const int TextLineHeight = 24;

	private readonly GraphicsDeviceManager _graphics;

	// Stock effect with directional lighting and texturing
	private BasicEffect _basicEffect;

	// Effect for rendering skeletal mesh with bone transformations
	private SkinnedEffect _skinnedEffect;

	// Solid white texture for models without material textures
	private Texture2D _textureWhite;

	// Ground plane texture
	private Texture2D _textureGround;

	// Ground plane mesh
	private DrMesh _meshGround;

	// Hero character model instance
	private DrModelInstance _modelHero;
	// Sword model instance
	private DrModelInstance _modelSword;

	// Bones for sword attachment (spine for sheathed, right hand for drawn)
	private DrModelBone _boneSpine, _boneHand;

	// Blended animations: lower body runs while upper body performs weapon action
	private AnimationBlendNode _runDrawAnimation;
	private AnimationBlendNode _runSheathAnimation;
	private AnimationBlendNode _runSlashAnimation;

	// Whether sword is in right hand (vs sheathed on back)
	private bool _swordInHand = false;

	// Animation state machine for playing and transitioning clips
	private AnimationController _player;

	// Current animation state
	private AnimationState _animationState = AnimationState.Idle;
	// Current weapon state for draw/sheath transitions
	private WeaponState _weaponState = WeaponState.Sheathed;

	// Hero position in world space
	private Vector3 _heroPosition;

	// Hero body yaw rotation in degrees
	private float _heroYaw;

	// Camera mount pitch rotation in degrees
	private float _cameraMountPitch;

	// Jump state and physics
	private DateTime? _jumpStarted;
	private Vector3 _jumpMovement;

	private SpriteBatch _spriteBatch;
	private SpriteFontBase _font;
	private bool _wasEscapeDown = false;

	public static MyGame Instance { get; private set; }

	/// <summary>Initializes the game with graphics and input configuration.</summary>
	public MyGame()
	{
		Instance = this;

		// Set up graphics device with preferred window resolution
		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = 1200,
			PreferredBackBufferHeight = 800
		};

		// Show the mouse cursor for this UI
		IsMouseVisible = true;
		// Allow the player to resize the window
		Window.AllowUserResizing = true;
	}

	protected override void LoadContent()
	{
		base.LoadContent();

		// Create solid white texture for untextured mesh parts
		_textureWhite = new Texture2D(GraphicsDevice, 1, 1);
		_textureWhite.SetData(new Color[] { Color.White });

		// Load ground texture
		var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(AppContext.BaseDirectory, "Assets"));
		_textureGround = assetManager.LoadTexture2D(GraphicsDevice, "Textures/checker.dds");

		// Create ground and hero meshes
		_meshGround = MeshPrimitives.CreatePlaneMesh(GraphicsDevice, uScale: 50, vScale: 50, normalDirection: NormalDirection.UpY);

		// Load hero model
		DrModel model = assetManager.LoadModel(GraphicsDevice, "Models/mixamo.gltf");
		_modelHero = new DrModelInstance(model);
		_player = new AnimationController(_modelHero);
		_player.StartClip("Idle", AnimationFlags.Looped);

		// Look up bones for sword attachment points
		_boneSpine = model.FindBoneByName("mixamorig:Spine");
		_boneHand = model.FindBoneByName("mixamorig:RightHand");

		// Bone filters to isolate upper body (spine and above) for weapon animations
		var topFilter = model.CreateBoneFilter("mixamorig:Spine");
		var bottomFilter = model.CreateInverseBoneFilter(topFilter);

		// Blended animation: run with draw (lower body runs, upper body draws)
		_runDrawAnimation = new AnimationBlendNode();
		_runDrawAnimation.AddLayer(model.Animations["Run"], AnimationFlags.Looped).BoneFilter = bottomFilter;
		var runDrawLayer = _runDrawAnimation.AddLayer(model.Animations["DrawGreatSword"]);
		runDrawLayer.BoneFilter = topFilter;

		// Blended animation: run with sheath (lower body runs with greatsword, upper body sheaths)
		_runSheathAnimation = new AnimationBlendNode();
		_runSheathAnimation.AddLayer(model.Animations["RunGreatSword"], AnimationFlags.Looped).BoneFilter = bottomFilter;
		var runSheathLayer = _runSheathAnimation.AddLayer(model.Animations["DrawGreatSword"], AnimationFlags.PlayBackwards);
		runSheathLayer.BoneFilter = topFilter;

		// Blended animation: run with slash (lower body runs with greatsword, upper body slashes)
		_runSlashAnimation = new AnimationBlendNode();
		_runSlashAnimation.AddLayer(model.Animations["RunGreatSword"], AnimationFlags.Looped).BoneFilter = bottomFilter;
		var runSlashLayer = _runSlashAnimation.AddLayer(model.Animations["SlashGreatSword"]);
		runSlashLayer.BoneFilter = topFilter;

		// Load sword model
		model = assetManager.LoadModel(GraphicsDevice, "Models/sword.gltf");
		_modelSword = new DrModelInstance(model);

		// Set up rendering effect with lighting
		_basicEffect = new BasicEffect(GraphicsDevice) { LightingEnabled = true };
		_basicEffect.EnableDefaultLighting();

		// Effect for rendering skeletal meshes with bone transformations
		_skinnedEffect = new SkinnedEffect(GraphicsDevice);
		_skinnedEffect.EnableDefaultLighting();

		// Start hero at world center
		_heroPosition = new Vector3(0, DefaultY, 0);

		_spriteBatch = new SpriteBatch(GraphicsDevice);

		var fontSystem = assetManager.LoadFontSystem("Fonts/DroidSans.ttf");
		_font = fontSystem.GetFont(24);

		MouseService.Locked = true;
	}

	// Handle mouse input for camera rotation and slash attack
	private void ProcessMouse()
	{
		MouseService.Update();

		var delta = MouseService.Delta;

		// Rotate hero by mouse X delta
		var horizontalRotation = -(int)(delta.X * MouseSensitivity);
		_heroYaw += horizontalRotation;

		// Tilt camera by mouse Y delta
		var verticalRotation = -(int)(delta.Y * MouseSensitivity);
		_cameraMountPitch += verticalRotation;

		// Clamp pitch to valid range (5 to 90 degrees)
		_cameraMountPitch = MathHelper.Clamp(_cameraMountPitch, 5, 90);

		// Left click to slash when sword is drawn
		var mouse = Mouse.GetState();
		if (mouse.LeftButton == ButtonState.Pressed && _weaponState == WeaponState.Drawn)
		{
			AnimateWeapon(WeaponState.Slashing);
		}
	}

	// Transition to idle animation based on current weapon state
	private void AnimateIdle()
	{
		if (_animationState == AnimationState.Idle)
		{
			return;
		}

		switch (_weaponState)
		{
			case WeaponState.Sheathed:
				// Play standard idle
				_player.CrossfadeToClip("Idle", AnimationCrossfadeDelay, AnimationFlags.Looped);
				break;

			case WeaponState.Drawing:
				// Continue drawing from where we left off
				_player.CrossfadeToClip("DrawGreatSword", AnimationCrossfadeDelay, AnimationFlags.KeepTime);
				break;

			case WeaponState.Drawn:
				// Idle with sword in hand
				_player.CrossfadeToClip("IdleGreatSword", AnimationCrossfadeDelay, AnimationFlags.Looped);
				break;

			case WeaponState.Sheathing:
				// Continue sheathing from where we left off
				_player.CrossfadeToClip("DrawGreatSword", AnimationCrossfadeDelay, AnimationFlags.PlayBackwards | AnimationFlags.KeepTime);
				break;

			case WeaponState.Slashing:
				// Continue slash from where we left off
				_player.CrossfadeToClip("SlashGreatSword", AnimationCrossfadeDelay, AnimationFlags.KeepTime);
				break;
		}

		_animationState = AnimationState.Idle;
	}

	// Transition to running animation based on current weapon state
	private void AnimateRunning()
	{
		if (_animationState == AnimationState.Running)
		{
			return;
		}

		switch (_weaponState)
		{
			case WeaponState.Sheathed:
				// Run with empty hands
				_player.CrossfadeToClip("Run", AnimationCrossfadeDelay, AnimationFlags.Looped);
				break;

			case WeaponState.Drawing:
				// Blend run (lower) with draw (upper), synced to current time
				_runDrawAnimation.Layers[1].TimeOffset = _player.Time;
				_player.CrossfadeToClip(_runDrawAnimation, AnimationCrossfadeDelay);
				break;

			case WeaponState.Drawn:
				// Run with sword in hand
				_player.CrossfadeToClip("RunGreatSword", AnimationCrossfadeDelay, AnimationFlags.Looped);
				break;

			case WeaponState.Sheathing:
				// Blend run with greatsword (lower) and reversed draw (upper), synced
				_runSheathAnimation.Layers[1].TimeOffset = _player.Time;
				_player.CrossfadeToClip(_runSheathAnimation, AnimationCrossfadeDelay);
				break;

			case WeaponState.Slashing:
				// Blend run with greatsword (lower) and slash (upper), synced
				_runSlashAnimation.Layers[1].TimeOffset = _player.Time;
				_player.CrossfadeToClip(_runSlashAnimation, AnimationCrossfadeDelay);
				break;
		}

		_animationState = AnimationState.Running;
	}

	// Transition weapon state with appropriate animation for idle or running
	private void AnimateWeapon(WeaponState newWeaponState)
	{
		if (_weaponState == newWeaponState)
		{
			return;
		}

		if (_animationState == AnimationState.Running)
		{
			// Transitions while maintaining running lower body
			switch (newWeaponState)
			{
				case WeaponState.Sheathed:
					_player.CrossfadeToClip("Run", AnimationCrossfadeDelay, AnimationFlags.Looped);
					break;

				case WeaponState.Drawing:
					// Start draw from beginning, keep running below
					_runDrawAnimation.Layers[1].TimeOffset = TimeSpan.Zero;
					_player.CrossfadeToClip(_runDrawAnimation, AnimationCrossfadeDelay);
					break;

				case WeaponState.Drawn:
					_player.CrossfadeToClip("RunGreatSword", AnimationCrossfadeDelay, AnimationFlags.Looped);
					break;

				case WeaponState.Sheathing:
					// Start sheath from beginning, keep running below
					_runSheathAnimation.Layers[1].TimeOffset = TimeSpan.Zero;
					_player.CrossfadeToClip(_runSheathAnimation, AnimationCrossfadeDelay);
					break;

				case WeaponState.Slashing:
					// Start slash from beginning, keep running below
					_runSlashAnimation.Layers[1].TimeOffset = TimeSpan.Zero;
					_player.CrossfadeToClip(_runSlashAnimation, AnimationCrossfadeDelay);
					break;
			}
		}
		else
		{
			// Transitions while standing idle
			switch (newWeaponState)
			{
				case WeaponState.Sheathed:
					_player.CrossfadeToClip("Idle", AnimationCrossfadeDelay, AnimationFlags.Looped);
					break;

				case WeaponState.Drawing:
					_player.CrossfadeToClip("DrawGreatSword", AnimationCrossfadeDelay);
					break;

				case WeaponState.Drawn:
					_player.CrossfadeToClip("IdleGreatSword", AnimationCrossfadeDelay, AnimationFlags.Looped);
					break;

				case WeaponState.Sheathing:
					_player.CrossfadeToClip("DrawGreatSword", AnimationCrossfadeDelay, AnimationFlags.PlayBackwards);
					break;

				case WeaponState.Slashing:
					_player.CrossfadeToClip("SlashGreatSword", AnimationCrossfadeDelay);
					break;
			}
		}

		_weaponState = newWeaponState;
	}

	// Handle keyboard input for movement, weapon draw/sheath, and jump
	private void ProcessKeyboard()
	{
		var keyboard = Keyboard.GetState();
		if (_jumpStarted == null)
		{
			// Calculate movement velocity based on hero orientation
			var velocity = Vector3.Zero;
			var heroTransform = ToMatrix(_heroPosition, Vector3.One, _heroYaw, 0, 0);

			// Track if hero is moving (for animation transitions)
			var isRunning = true;
			if (keyboard.IsKeyDown(Keys.W))
				velocity = heroTransform.Forward * -MovementSpeed;
			else if (keyboard.IsKeyDown(Keys.S))
				velocity = heroTransform.Forward * MovementSpeed;
			else if (keyboard.IsKeyDown(Keys.A))
				velocity = heroTransform.Right * MovementSpeed;
			else if (keyboard.IsKeyDown(Keys.D))
				velocity = heroTransform.Right * -MovementSpeed;
			else
				isRunning = false;

			// Apply velocity to hero position
			_heroPosition += velocity;

			if (isRunning)
			{
				AnimateRunning();
			}
			else
			{
				AnimateIdle();
			}

			// Press R to toggle weapon draw/sheath
			if (keyboard.IsKeyDown(Keys.R))
			{
				if (_weaponState == WeaponState.Sheathed)
				{
					AnimateWeapon(WeaponState.Drawing);
				}
				else if (_weaponState == WeaponState.Drawn)
				{
					AnimateWeapon(WeaponState.Sheathing);
				}
			}

			// Initiate jump with momentum preservation
			if ((_weaponState == WeaponState.Sheathed || _weaponState == WeaponState.Drawn) && keyboard.IsKeyDown(Keys.Space))
			{
				_jumpStarted = DateTime.Now;
				_animationState = AnimationState.Jumping;
				_jumpMovement = velocity;
				_player.CrossfadeToClip("JumpStart", AnimationCrossfadeDelay);
			}
		} else
		{
			UpdateJump();
		}

		var isEscapeDown = keyboard.IsKeyDown(Keys.Escape);
		if (isEscapeDown && !_wasEscapeDown)
		{
			MouseService.Locked = !MouseService.Locked;
		}

		_wasEscapeDown = isEscapeDown;
	}

	// Update hero position and animation during jump using projectile motion
	private void UpdateJump()
	{
		// Time elapsed since jump started (seconds)
		var t = (float)(DateTime.Now - _jumpStarted.Value).TotalSeconds;

		// Height from kinematic equation: h = v0*t - 0.5*g*t^2
		var jumpHeight = JumpForce * t - (0.5f * Gravity * t * t);

		// Apply height and preserve horizontal momentum
		_heroPosition.Y = jumpHeight;
		_heroPosition += _jumpMovement;

		// Vertical velocity: v = v0 - g*t (positive = upward, negative = falling)
		var jumpVelocity = JumpForce - Gravity * t;

		// Start falling animation once we fall below height 2
		if (jumpVelocity < 0 && _heroPosition.Y < 2 && _animationState != AnimationState.Landing)
		{
			_player.CrossfadeToClip("JumpEnd", AnimationCrossfadeDelay);
			_animationState = AnimationState.Landing;
		}

		// Land when reaching ground
		if (_heroPosition.Y <= DefaultY)
		{
			_heroPosition.Y = DefaultY;
			_jumpStarted = null;
		}
	}

	// Monitor and progress weapon animation states (draw/sheath/slash)
	private void UpdateAnimations()
	{
		if (_weaponState == WeaponState.Drawing)
		{
			if (_player.HasFinished)
			{
				// Draw animation finished, switch to Drawn state
				AnimateWeapon(WeaponState.Drawn);
			}
			else if (!_swordInHand && _player.Time >= _player.RootNode.Duration / 3)
			{
				// Move sword to hand at one-third through the draw animation
				_swordInHand = true;
			}
		}
		else if (_weaponState == WeaponState.Sheathing && _player.HasFinished)
		{
			// Sheathing finished, return to Sheathed state
			_swordInHand = false;
			AnimateWeapon(WeaponState.Sheathed);
		}
		else if (_weaponState == WeaponState.Slashing && _player.HasFinished)
		{
			// Slash finished, return to Drawn state
			AnimateWeapon(WeaponState.Drawn);
		}
	}

	protected override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!IsActive)
		{
			return;
		}

		ProcessMouse();
		ProcessKeyboard();
		UpdateAnimations();

		_player.Update(gameTime.ElapsedGameTime);
	}

	// Draw a single mesh part with the given effect
	private void DrawMeshPart(Effect effect, DrMeshPart part)
	{
		GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
		GraphicsDevice.Indices = part.IndexBuffer;

		foreach (var pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
#if MONOGAME
			GraphicsDevice.DrawIndexedPrimitives(part.PrimitiveType, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
#else
			GraphicsDevice.DrawIndexedPrimitives(part.PrimitiveType, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
#endif
		}
	}


	/// <summary>Render a mesh with color and texture.</summary>
	private void DrawMesh(DrMesh mesh, Matrix world, Color color, Texture2D texture)
	{
		_basicEffect.DiffuseColor = color.ToVector3();
		_basicEffect.TextureEnabled = texture != null;
		_basicEffect.Texture = texture;
		_basicEffect.World = world;

		foreach (var part in mesh.MeshParts)
		{
			DrawMeshPart(_basicEffect, part);
		}
	}

	// Render model with material colors and textures, handling both skinned and static meshes
	private void DrawModel(DrModelInstance model, Matrix world)
	{
		foreach (var mesh in model.Model.Meshes)
		{
			foreach (var part in mesh.MeshParts)
			{
				// Extract material properties or use defaults if no material assigned
				var color = Color.White;
				var texture = _textureWhite;
				if (part.Material != null)
				{
					color = part.Material.DiffuseColor;

					if (part.Material.DiffuseTexture != null)
					{
						texture = part.Material.DiffuseTexture;
					}
				}

				if (part.Skin != null)
				{
					// Skinned mesh: bone transforms applied per-vertex in shader via SetBoneTransforms
					// World matrix only positions the entire model in world space
					_skinnedEffect.DiffuseColor = color.ToVector3();
					_skinnedEffect.Texture = texture;
					_skinnedEffect.World = world;
					_skinnedEffect.SetBoneTransforms(model.GetSkinTransforms(part.Skin.SkinIndex));

					DrawMeshPart(_skinnedEffect, part);
				}
				else
				{
					// Static mesh: must include bone transform in World matrix since GPU doesn't apply skeletal deformation
					// Bone transform positions this mesh part relative to the model, then world positions the whole model
					_basicEffect.DiffuseColor = color.ToVector3();
					_basicEffect.Texture = texture;
					_basicEffect.World = model.GetBoneGlobalTransform(mesh.ParentBone.Index) * world;

					DrawMeshPart(_basicEffect, part);
				}
			}
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);

		var device = GraphicsDevice;
		device.Clear(Color.Black);

		// Set GPU states
		device.DepthStencilState = DepthStencilState.Default;
		device.RasterizerState = RasterizerState.CullCounterClockwise;
		device.BlendState = BlendState.AlphaBlend;
		device.SamplerStates[0] = SamplerState.LinearWrap;

		// Set projection
		var projection = Matrix.CreatePerspectiveFieldOfView(
			MathHelper.ToRadians(ViewAngle),
			device.Viewport.AspectRatio,
			NearPlaneDistance, FarPlaneDistance);
		_basicEffect.Projection = projection;
		_skinnedEffect.Projection = projection;

		// Build camera hierarchy: hero body -> camera mount (head) -> camera
		var heroTransform = ToMatrix(_heroPosition, Vector3.One, _heroYaw, 0, 0);
		var cameraMountTransform = ToMatrix(new Vector3(0, 1f, 0), Vector3.One, 0, _cameraMountPitch, 0) * heroTransform;
		var cameraTransform = ToMatrix(new Vector3(0, 0, -5), Vector3.One, 180, 0, 0) * cameraMountTransform;

		var view = Matrix.Invert(cameraTransform);
		_basicEffect.View = view;
		_skinnedEffect.View = view;

		// Draw ground and hero
		DrawMesh(_meshGround, Matrix.CreateScale(200, 1, 200), Color.White, _textureGround);
		DrawModel(_modelHero, heroTransform);

		// Attach sword to spine (sheathed) or right hand (drawn)
		Matrix swordTransform;
		if (!_swordInHand)
		{
			swordTransform = SwordSheathedTransform * _modelHero.GetBoneGlobalTransform(_boneSpine.Index) * heroTransform;
		}
		else
		{
			swordTransform = SwordDrawnTransform * _modelHero.GetBoneGlobalTransform(_boneHand.Index) * heroTransform;
		}

		// Draw sword
		DrawModel(_modelSword, swordTransform);

		// Draw Help
		_spriteBatch.Begin();

		var y = 0;
		_spriteBatch.DrawString(_font, "WASD - Move", new Vector2(0, 0), Color.White);

		y += TextLineHeight;
		_spriteBatch.DrawString(_font, "Space - Jump", new Vector2(0, y), Color.White);

		y += TextLineHeight;
		_spriteBatch.DrawString(_font, "R - Draw/Sheath Weapon", new Vector2(0, y), Color.White);

		y += TextLineHeight;
		_spriteBatch.DrawString(_font, "Left Click - Slash Weapon", new Vector2(0, y), Color.White);

		y += TextLineHeight;
		_spriteBatch.DrawString(_font, "Escape - Turn Mouse Lock On/Off", new Vector2(0, y), Color.White);

		_spriteBatch.End();
	}

	/// <summary>Build transform matrix from position, scale, and rotation (TRS order).</summary>
	private static Matrix ToMatrix(Vector3 position, Vector3 scale, float yaw, float pitch, float roll)
	{
		var scaleTransform = Matrix.CreateScale(scale);
		var rotation = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), MathHelper.ToRadians(roll));
		var translation = Matrix.CreateTranslation(position);

		return scaleTransform * rotation * translation;
	}
}