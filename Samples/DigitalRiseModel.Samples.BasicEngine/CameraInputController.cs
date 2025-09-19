using System;
using Microsoft.Xna.Framework;

namespace DigitalRiseModel.Samples.BasicEngine
{
	public class CameraInputController
	{
		public enum ControlKeys
		{
			Left,
			Right,
			Forward,
			Backward,
			Up,
			Down
		}

		public enum TouchType
		{
			Move,
			Rotate
		}

		private Point? _touchStart;
		public bool _moveTouchDown, _rotateTouchDown;
		private bool _leftKeyPressed, _rightKeyPressed, _forwardKeyPressed, _backwardKeyPressed, _upKeyPressed, _downKeyPressed;
		private DateTime? _keyboardLastTime;

		public CameraNode Camera { get; }
		public float RotateDelta { get; set; }
		public float MoveDelta { get; set; }
		public float KeyboardMovementSpeed { get; set; }

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

		public void ResetKeyboard()
		{
			_forwardKeyPressed = _leftKeyPressed =
								 _rightKeyPressed = _backwardKeyPressed =
								 _upKeyPressed = _downKeyPressed = false;
			_keyboardLastTime = null;
		}

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