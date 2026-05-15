using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DigitalRiseModel.Samples.Character
{
	/// <summary>
	/// Event args for input value changes, containing the old and new values.
	/// </summary>
	public class InputEventArgs<T> : EventArgs
	{
		/// <summary>Gets the previous value.</summary>
		public T OldValue { get; }

		/// <summary>Gets the new value.</summary>
		public T NewValue { get; }

		/// <summary>
		/// Initializes a new instance of the InputEventArgs class.
		/// </summary>
		public InputEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	/// <summary>
	/// Event args for keyboard input events.
	/// </summary>
	public class KeyEventsArgs : EventArgs
	{
		/// <summary>Gets the key that was pressed or released.</summary>
		public Keys Key { get; }

		/// <summary>
		/// Initializes a new instance of the KeyEventsArgs class.
		/// </summary>
		public KeyEventsArgs(Keys key)
		{
			Key = key;
		}
	}

	/// <summary>
	/// Manages keyboard and mouse input for the game.
	/// Provides events for key state changes and mouse movement.
	/// </summary>
	public class InputService
	{
		// Array tracking which keys are currently down (indexed by key code)
		private readonly bool[] _keysDown = new bool[256];

		/// <summary>
		/// Gets the current mouse position in screen coordinates. Nullable to indicate uninitialized state.
		/// </summary>
		public Point? MousePosition { get; private set; }

		/// <summary>
		/// Gets or sets whether the mouse is locked (confined and hidden).
		/// When locked, the mouse is centered each frame and hidden from view.
		/// When unlocked, the mouse is visible and can move freely.
		/// </summary>
		public bool MouseLocked
		{
			get => !ViewerGame.Instance.IsMouseVisible;

			set
			{
				// Avoid redundant operations if state hasn't changed
				if (value == MouseLocked)
				{
					return;
				}

				// Toggle mouse visibility based on lock state
				ViewerGame.Instance.IsMouseVisible = !value;

				MousePosition = null;
				UpdateLockedMouse();
			}
		}

		/// <summary>
		/// Raised when the mouse moves. Event args contain old and new mouse positions.
		/// </summary>
		public event EventHandler<InputEventArgs<Point>> MouseMoved;

		/// <summary>
		/// Raised when a key is pressed (transition from not-pressed to pressed).
		/// </summary>
		public event EventHandler<KeyEventsArgs> KeyDown;

		/// <summary>
		/// Raised when a key is released (transition from pressed to not-pressed).
		/// </summary>
		public event EventHandler<KeyEventsArgs> KeyUp;

		/// <summary>
		/// Checks if a specific key is currently pressed.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>true if the key is currently pressed; false otherwise.</returns>
		public bool IsKeyDown(Keys key) => _keysDown[(int)key];

		/// <summary>
		/// Updates the input service with current keyboard and mouse states.
		/// Detects state changes and raises appropriate events.
		/// Should be called once per frame in the game update loop.
		///
		/// Mouse behavior:
		/// - When unlocked: Reports absolute movement from old to new position (free mouse)
		/// - When locked: Reports relative movement from window center to current position,
		///   then recenters the mouse for next frame (enables smooth continuous FPS-style rotation)
		///
		/// Keyboard behavior:
		/// - Detects transitions: KeyDown event when key is pressed, KeyUp event when released
		/// - IsKeyDown() can be used to check if a key is currently held
		/// </summary>
		public void Update()
		{
			// Skip input processing when window is inactive (e.g., alt-tab, minimized, losing focus)
			// This prevents unwanted input from being processed while the game is not in focus
			if (!ViewerGame.Instance.IsActive)
			{
				// Reset mouse position so transitions don't fire when window regains focus
				MousePosition = null;
				return;
			}

			// === Update mouse input ===
			var mouseState = Mouse.GetState();
			var newPosition = new Point(mouseState.X, mouseState.Y);

			if (MousePosition == null)
			{
				// First update: initialize mouse position without firing events
				MousePosition = newPosition;
			}
			else
			{
				var oldPosition = MousePosition.Value;
				MousePosition = newPosition;

				// When mouse is unlocked (free mode): report absolute position changes
				if (!MouseLocked)
				{
					if (newPosition != oldPosition)
					{
						MouseMoved?.Invoke(this, new InputEventArgs<Point>(oldPosition, newPosition));
					}
				}
				else
				{
					// When mouse is locked (FPS mode): calculate relative motion from window center
					// This enables continuous camera rotation without the cursor reaching screen edges
					var cb = ViewerGame.Instance.Window.ClientBounds;
					var center = new Point(cb.Width / 2, cb.Height / 2);

					// Report movement delta from center to current position (for relative camera rotation)
					if (newPosition != center)
					{
						MouseMoved?.Invoke(this, new InputEventArgs<Point>(center, newPosition));
					}
				}
			}

			// Recenter the mouse if locked (prepares for next frame's relative motion calculation)
			UpdateLockedMouse();

			// === Update keyboard input ===
			// Track state transitions (key down/up events) by comparing current frame state with previous frame
			var keyboardState = Keyboard.GetState();
			for (var i = 0; i < _keysDown.Length; ++i)
			{
				var key = (Keys)i;
				var isDown = keyboardState.IsKeyDown(key);

				// Detect key press: transition from not-pressed to pressed
				if (!_keysDown[i] && isDown)
				{
					KeyDown?.Invoke(this, new KeyEventsArgs(key));
				}
				// Detect key release: transition from pressed to not-pressed
				else if (_keysDown[i] && !isDown)
				{
					KeyUp?.Invoke(this, new KeyEventsArgs(key));
				}

				// Store current state for next frame comparison
				_keysDown[i] = isDown;
			}
		}

		/// <summary>
		/// Centers the mouse cursor at the window center when in locked mode.
		/// This is called after mouse movement is processed, recentering the cursor for the next frame.
		/// This technique allows continuous FPS-style rotation without the cursor ever reaching screen edges.
		/// </summary>
		private void UpdateLockedMouse()
		{
			// Only recenter when mouse is locked
			if (!MouseLocked)
			{
				return;
			}

			// Calculate window center and recenter the mouse for next frame
			var cb = ViewerGame.Instance.Window.ClientBounds;
			var center = new Point(cb.Width / 2, cb.Height / 2);
			Mouse.SetPosition(center.X, center.Y);
		}
	}
}
