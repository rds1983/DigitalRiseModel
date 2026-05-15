using AssetManagementBase;
using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	/// <summary>
	/// Manages character animation and physics-based movement.
	/// Handles animation state transitions, skeletal animation blending, jumping physics, and weapon state.
	/// </summary>
	internal class CharacterService
	{
		// Physics constants for jump arc calculation
		private const float Gravity = 0.015f;           // Gravity acceleration applied each frame
		private const float DefaultY = 0.0f;            // Ground level (Y position when not jumping)
		private const float JumpForce = 0.5f;           // Initial upward velocity when jumping

		/// <summary>
		/// Character locomotion state machine.
		/// Idle: Not moving, standing or in idle animation.
		/// Run: Moving horizontally with run animation.
		/// Jump: In mid-air, either ascending, at peak, or descending.
		/// </summary>
		private enum LocomotionState
		{
			Idle,
			Run,
			Jump,
			DrawingWeapon,
			SheathingWeapon
		}

		/// <summary>
		/// Jump animation phase state machine.
		/// Used to coordinate animation playback with physics during jump.
		/// Start: Jump startup animation (before airtime).
		/// Loop: In-air animation (apex or descent).
		/// Land: Landing animation (played as character returns to ground).
		/// Finished: Jump complete, ready for next input.
		/// </summary>
		private enum JumpState
		{
			Start,
			Loop,
			Land,
			Finished
		}

		// Animation controller manages skeletal animation playback and blending
		private readonly AnimationController _player;

		// Reference to the model node for position and rotation updates
		private readonly ModelInstanceNode _modelNode = new ModelInstanceNode
		{
			ModelInstance = new DrModelInstance()
		};
		private readonly ModelBoneAttachment _weaponAttachment = new ModelBoneAttachment();

		public ModelInstanceNode ModelNode => _modelNode;

		// Current movement state (idle, running, or jumping)
		private LocomotionState _locomotionState = LocomotionState.Idle;

		// Current jump phase (start, loop, land, finished)
		private JumpState _jumpState = JumpState.Start;

		// Current vertical velocity during jump (changes due to gravity)
		private float _jumpVelocity = 0.0f;

		/// <summary>Indicates whether the character is currently holding a weapon (armed state).</summary>
		public bool WeaponDrawn { get; private set; }

		/// <summary>
		/// Determines if the character is currently busy with an animation that cannot be interrupted.
		/// Returns false (not busy) when:
		/// - Non-jump animations are looping or finished (can transition to new animation)
		/// - Jump is in finished state (can start new action)
		/// Returns true (busy) when animation is still playing and should block interruptions.
		/// </summary>
		private bool IsBusy
		{
			get
			{
				// For non-jump states: check if animation is looping or finished (can be interrupted)
				if (_locomotionState != LocomotionState.Jump && (_player.RootNode.IsLooped || _player.HasFinished))
				{
					return false;
				}

				// For jump state: only not busy when jump is completely finished
				if (_locomotionState == LocomotionState.Jump && _jumpState == JumpState.Finished)
				{
					return false;
				}

				// All other cases: character is busy and cannot be interrupted
				return true;
			}
		}

		/// <summary>
		/// Initializes the controller with a reference to the character model.
		/// Sets up initial animation state and position.
		/// </summary>
		public CharacterService(GraphicsDevice graphicsDevice, AssetManager assetManager)
		{
			if (assetManager == null)
			{
				throw new ArgumentNullException(nameof(assetManager));
			}

			// Load and add the animated character model (mixamo format)
			var characterModel = assetManager.LoadModel(graphicsDevice, "Models/mixamo.gltf");
			_modelNode.ModelInstance.Model = characterModel;

			var swordModel = assetManager.LoadModel(graphicsDevice, "Models/sword.gltf");
			_weaponAttachment.Model = new DrModelInstance(swordModel);
			_modelNode.BonesAttachments.Add(_weaponAttachment);
			SetSheathedState();

			// Create animation controller for this character model
			_player = new AnimationController(_modelNode.ModelInstance);

			// Start with idle animation (non-looping initial state)
			_player.StartClip("Idle", true);

			// Position character at ground level at world origin
			_modelNode.Translation = new Vector3(0, DefaultY, 0);
		}

		private void SetSheathedState()
		{
			_weaponAttachment.Bone = _modelNode.ModelInstance.Model.FindBoneByName("mixamorig:Spine");

			var transform = new SrtTransform
			{
				Translation = new Vector3(-0.6f, 0, -1.4f),
				Scale = new Vector3(16),
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(180.0f))
			};
			_weaponAttachment.Transform = transform.ToMatrix();
		}

		private void SetDrawnState()
		{
			_weaponAttachment.Bone = _modelNode.ModelInstance.Model.FindBoneByName("mixamorig:RightHand");

			var transform = new SrtTransform
			{
				Translation = new Vector3(3.5f, 0f, 0f),
				Scale = new Vector3(16),
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(270.0f))
			};
			_weaponAttachment.Transform = transform.ToMatrix();
		}

		/// <summary>
		/// Internal idle state handler. Transitions to appropriate idle animation based on weapon state.
		/// Avoids redundant animation switches if already in the target idle state.
		/// </summary>
		private void InternalIdle(bool weaponDrawn)
		{
			// Skip if character is busy with another animation or already in the desired idle state
			if (IsBusy || (_locomotionState == LocomotionState.Idle && WeaponDrawn == weaponDrawn))
			{
				return;
			}

			// Crossfade to appropriate idle animation (with or without weapon)
			if (weaponDrawn)
			{
				_player.CrossfadeToClip("IdleGreatSword", TimeSpan.FromSeconds(0.1), true);
			}
			else
			{
				_player.CrossfadeToClip("Idle", TimeSpan.FromSeconds(0.1), true);
			}

			_locomotionState = LocomotionState.Idle;
			WeaponDrawn = weaponDrawn;
		}

		/// <summary>
		/// Internal run state handler. Applies movement velocity and transitions to run animation.
		/// Movement is applied each frame while running.
		/// </summary>
		private void InternalRun(Vector3 velocity, bool weaponDrawn)
		{
			// Apply horizontal movement velocity to character position
			_modelNode.Translation += velocity;

			// Skip if character is busy or already running with same weapon state
			if (IsBusy || (_locomotionState == LocomotionState.Run && WeaponDrawn == weaponDrawn))
			{
				return;
			}

			// Crossfade to appropriate run animation (with or without weapon)
			if (weaponDrawn)
			{
				_player.CrossfadeToClip("RunGreatSword", TimeSpan.FromSeconds(0.1), true);
			}
			else
			{
				_player.CrossfadeToClip("Run", TimeSpan.FromSeconds(0.1), true);
			}

			_locomotionState = LocomotionState.Run;
			WeaponDrawn = weaponDrawn;
		}

		/// <summary>
		/// Transitions the character to idle state while preserving current weapon state.
		/// </summary>
		public void Idle() => InternalIdle(WeaponDrawn);

		/// <summary>
		/// Transitions the character to running state while applying movement velocity.
		/// Preserves current weapon state during movement.
		/// </summary>
		public void Run(Vector3 velocity) => InternalRun(velocity, WeaponDrawn);

		/// <summary>
		/// Draws the character's weapon. Plays the draw animation (non-looping).
		/// Cannot be interrupted while animation is playing.
		/// </summary>
		public void DrawWeapon()
		{
			// Skip if character is busy or weapon is already drawn
			if (IsBusy || WeaponDrawn)
			{
				return;
			}

			// Play weapon draw animation (0.1s blend-in, non-looping)
			_player.CrossfadeToClip("DrawGreatSword", TimeSpan.FromSeconds(0.1), false);
			WeaponDrawn = true;
			SetDrawnState();
		}

		/// <summary>
		/// Sheathes the character's weapon. Plays the sheathe animation (non-looping).
		/// Cannot be interrupted while animation is playing.
		/// </summary>
		public void SheathWeapon()
		{
			// Skip if character is busy or weapon is already sheathed
			if (IsBusy || !WeaponDrawn)
			{
				return;
			}

			// Play weapon sheathe animation (0.1s blend-in, non-looping)
			_player.CrossfadeToClip("SheathGreatSword", TimeSpan.FromSeconds(0.1), false);
			WeaponDrawn = false;
			SetSheathedState();
		}

		public void Slash()
		{
			if (IsBusy || !WeaponDrawn)
			{
				return;
			}

			_player.CrossfadeToClip("SlashGreatSword", TimeSpan.FromSeconds(0.1), false);
		}

		/// <summary>
		/// Initiates a jump. Sets up the jump state machine and starts the jump animation.
		/// Cannot jump if character is busy or already jumping.
		/// </summary>
		public void Jump(Vector3 velocity)
		{
			// Skip if character is busy or already in mid-jump
			if (IsBusy || _locomotionState == LocomotionState.Jump)
			{
				return;
			}

			// Initialize jump velocity (will be modified by gravity each frame)
			_jumpVelocity = JumpForce;

			// Set up jump state machine
			_locomotionState = LocomotionState.Jump;
			_jumpState = JumpState.Start;

			// Play jump start animation (0.2s blend-in, non-looping one-shot)
			_player.CrossfadeToClip("JumpStart", TimeSpan.FromSeconds(0.2), false);
		}

		/// <summary>
		/// Updates character animation and jump physics.
		/// Handles jump state machine, gravity simulation, and animation playback.
		/// Should be called once per frame.
		/// </summary>
		public void Update(TimeSpan elapsed)
		{
			// Process jump physics and animation states
			if (_locomotionState == LocomotionState.Jump)
			{
				switch (_jumpState)
				{
					case JumpState.Start:
						// Wait for jump start animation to finish, then transition to airtime phase
						if (_player.HasFinished)
						{
							_jumpState = JumpState.Loop;
						}
						break;

					case JumpState.Loop:
						// Detect landing: when character's Y position drops below threshold (6 units)
						// This accounts for the arc of the jump - character rises then falls
						if (_modelNode.Translation.Y <= 6f)
						{
							_jumpState = JumpState.Land;
							// Play landing animation (0.2s blend-in, non-looping)
							_player.CrossfadeToClip("JumpEnd", TimeSpan.FromSeconds(0.2), false);
						}
						break;
				}

				// === Apply gravity and update position ===
				// Physics are applied for all jump phases except the landing animation
				if (_jumpState != JumpState.Finished)
				{
					// Apply gravity to vertical velocity (constant acceleration downward)
					_jumpVelocity -= Gravity;

					// Update character position by applying velocity
					_modelNode.Translation += Vector3.Up * _jumpVelocity;
				}

				// === Detect ground contact: end jump when character reaches ground level ===
				if (_modelNode.Translation.Y <= DefaultY)
				{
					// Snap to ground to prevent floating-point drift below ground
					_modelNode.Translation = new Vector3(_modelNode.Translation.X, DefaultY, _modelNode.Translation.Z);
					_jumpState = JumpState.Finished;
				}
			}

			// Update animation controller (handles animation blending, frame advancement, etc.)
			_player.Update(elapsed);
		}
	}
}
