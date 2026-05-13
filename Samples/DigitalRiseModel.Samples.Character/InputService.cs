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
		// Flag to indicate if mouse position has been initialized
		private bool _mouseSet;
		// Array tracking which keys are currently down (indexed by key code)
		private readonly bool[] _keysDown = new bool[256];

		/// <summary>
		/// Gets the current mouse position in screen coordinates.
		/// </summary>
		public Point MousePosition { get; private set; }

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
		/// </summary>
		public void Update()
		{
			// Update mouse input
			var mouseState = Mouse.GetState();
			var newPosition = new Point(mouseState.X, mouseState.Y);

			if (!_mouseSet)
			{
				// Initialize mouse position on first update
				MousePosition = newPosition;
				_mouseSet = true;
			}
			else
			{
				// Check if mouse position has changed and raise event if so
				var oldPosition = MousePosition;
				MousePosition = newPosition;
				if (newPosition != oldPosition)
				{
					MouseMoved?.Invoke(this, new InputEventArgs<Point>(oldPosition, newPosition));
				}
			}

			// Update keyboard input
			var keyboardState = Keyboard.GetState();
			for (var i = 0; i < _keysDown.Length; ++i)
			{
				var key = (Keys)i;
				var isDown = keyboardState.IsKeyDown(key);

				// Detect key press (transition from up to down)
				if (!_keysDown[i] && isDown)
				{
					KeyDown?.Invoke(this, new KeyEventsArgs(key));
				}
				// Detect key release (transition from down to up)
				else if (_keysDown[i] && !isDown)
				{
					KeyUp?.Invoke(this, new KeyEventsArgs(key));
				}

				// Update key state
				_keysDown[i] = isDown;
			}
		}
	}
}
