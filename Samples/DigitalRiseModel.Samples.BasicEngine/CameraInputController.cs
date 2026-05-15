using System;
using Microsoft.Xna.Framework;

namespace DigitalRiseModel.Samples.BasicEngine
{
	/// <summary>
	/// Handles user input (keyboard, mouse, and touch) to control a camera in 3D space.
	/// Supports camera movement and rotation through various input methods with configurable sensitivity.
	/// </summary>
	public class CameraInputController
	{
		/// <summary>
		/// Enumeration of control keys that can be mapped to camera movement directions.
		/// </summary>
		public enum ControlKeys
		{
			/// <summary>Move camera to the left.</summary>
			Left,
			/// <summary>Move camera to the right.</summary>
			Right,
			/// <summary>Move camera forward.</summary>
			Forward,
			/// <summary>Move camera backward.</summary>
			Backward,
			/// <summary>Move camera upward.</summary>
			Up,
			/// <summary>Move camera downward.</summary>
			Down
		}

		/// <summary>
		/// Enumeration of touch input types for mobile/touch-screen input.
		/// </summary>
		public enum TouchType
		{
			/// <summary>Touch input for camera movement.</summary>
			Move,
			/// <summary>Touch input for camera rotation.</summary>
			Rotate
		}

		private Point? _touchStart;
		public bool _moveTouchDown, _rotateTouchDown;
		private bool _leftKeyPressed, _rightKeyPressed, _forwardKeyPressed, _backwardKeyPressed, _upKeyPressed, _downKeyPressed;
		private DateTime? _keyboardLastTime;

		/// <summary>
		/// Gets the camera controlled by this input handler.
		/// </summary>
		public CameraNode Camera { get; }

		/// <summary>
		/// Gets or sets the rotation sensitivity for mouse/touch input.
		/// Default is 0.1 degrees per pixel.
		/// </summary>
		public float RotateDelta { get; set; }

		/// <summary>
		/// Gets or sets the movement sensitivity for mouse/touch input.
		/// Default is 0.03 units per pixel.
		/// </summary>
		public float MoveDelta { get; set; }

		/// <summary>
		/// Gets or sets the camera movement speed for keyboard input.
		/// Default is 45 units per second.
		/// </summary>
		public float KeyboardMovementSpeed { get; set; }

		/// <summary>
		/// Initializes a new instance of the CameraInputController class.
		/// </summary>
		/// <param name="camera">The camera node to control. Must not be null.</param>
		/// <exception cref="ArgumentNullException">Thrown if camera is null.</exception>
		public CameraInputController(CameraNode camera)
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}

			Camera = camera;

			RotateDelta = 0.1f;
			MoveDelta = 0.03f;

			KeyboardMovementSpeed = 45.0f;
		}

		/// <summary>
		/// Updates the state of a touch input (down or up).
		/// </summary>
		/// <param name="touch">The type of touch input to update.</param>
		/// <param name="isDown">True if the touch is down, false if released.</param>
		public void SetTouchState(TouchType touch, bool isDown)
		{
			switch (touch)
			{
				case TouchType.Move:
					_moveTouchDown = isDown;
					break;
				case TouchType.Rotate:
					_rotateTouchDown = isDown;
					break;
			}

			if (!_moveTouchDown && !_rotateTouchDown)
			{
				_touchStart = null;
			}
		}

		/// <summary>
		/// Updates camera position/rotation based on mouse or touch position.
		/// Calculates the delta from the touch start position and applies movement or rotation accordingly.
		/// </summary>
		/// <param name="position">The current mouse or touch position in screen coordinates.</param>
		public void SetMousePosition(Point position)
		{
			if (_touchStart == null)
			{
				_touchStart = position;
			}

			var delta = position - _touchStart.Value;

			if (_rotateTouchDown)
			{
				var rotation = Camera.Rotation;
				if (delta.Y != 0)
				{
					rotation.X += delta.Y * RotateDelta;
				}

				if (delta.X != 0)
				{
					rotation.Y += -delta.X * RotateDelta;
				}

				Camera.Rotation = rotation;
			}

			if (_moveTouchDown)
			{
				var cameraPosition = Camera.Translation;

				if (delta.Y != 0)
				{
					var up = Camera.Up;

					up *= delta.Y * MoveDelta;
					cameraPosition += up;
				}

				if (delta.X != 0)
				{
					var right = Camera.Right;
					right *= -delta.X * MoveDelta;
					cameraPosition += right;
				}

				Camera.Translation = cameraPosition;
			}

			_touchStart = position;
		}

		/// <summary>
		/// Updates camera position based on mouse wheel input.
		/// Moves the camera forward (positive delta) or backward (negative delta) along its direction vector.
		/// </summary>
		/// <param name="delta">The mouse wheel delta value (positive for forward, negative for backward).</param>
		public void OnWheel(int delta)
		{
			if (delta == 0)
			{
				return;
			}

			var cameraPosition = Camera.Translation;
			cameraPosition += Camera.Direction * delta * MoveDelta;
			Camera.Translation = cameraPosition;
		}

		/// <summary>
		/// Updates the state of a keyboard control key (pressed or released).
		/// </summary>
		/// <param name="key">The control key to update.</param>
		/// <param name="isDown">True if the key is pressed, false if released.</param>
		public void SetControlKeyState(ControlKeys key, bool isDown)
		{
			switch (key)
			{
				case ControlKeys.Left:
					_leftKeyPressed = isDown;
					break;
				case ControlKeys.Right:
					_rightKeyPressed = isDown;
					break;
				case ControlKeys.Forward:
					_forwardKeyPressed = isDown;
					break;
				case ControlKeys.Backward:
					_backwardKeyPressed = isDown;
					break;
				case ControlKeys.Up:
					_upKeyPressed = isDown;
					break;
				case ControlKeys.Down:
					_downKeyPressed = isDown;
					break;
			}

			if (!_forwardKeyPressed && !_leftKeyPressed && !_rightKeyPressed && !_backwardKeyPressed && !_upKeyPressed && !_downKeyPressed)
			{
				_keyboardLastTime = null;
			}
		}

		/// <summary>
		/// Resets all keyboard control key states to unpressed and clears the keyboard timer.
		/// Should be called when the input focus is lost (e.g., window deactivation).
		/// </summary>
		public void ResetKeyboard()
		{
			_forwardKeyPressed = _leftKeyPressed =
								 _rightKeyPressed = _backwardKeyPressed =
								 _upKeyPressed = _downKeyPressed = false;
			_keyboardLastTime = null;
		}

		/// <summary>
		/// Updates camera position based on currently pressed keyboard keys.
		/// Should be called once per game update frame.
		/// </summary>
		public void Update()
		{
			if (_keyboardLastTime == null)
			{
				_keyboardLastTime = DateTime.Now;
			}

			var delta = (float)(DateTime.Now - _keyboardLastTime.Value).TotalSeconds * KeyboardMovementSpeed;

			if (delta < 0.01)
			{
				return;
			}

			if (_forwardKeyPressed)
			{
				Camera.Translation += delta * Camera.Direction;
			}

			if (_leftKeyPressed)
			{
				Camera.Translation -= delta * Camera.Right;
			}

			if (_rightKeyPressed)
			{
				Camera.Translation += delta * Camera.Right;
			}

			if (_backwardKeyPressed)
			{
				Camera.Translation -= delta * Camera.Direction;
			}

			if (_upKeyPressed)
			{
				Camera.Translation += delta * Camera.Up;
			}

			if (_downKeyPressed)
			{
				Camera.Translation -= delta * Camera.Up;
			}

			_keyboardLastTime = DateTime.Now;
		}
	}
}