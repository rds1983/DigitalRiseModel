using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DigitalRiseModel.Samples;

internal static class MouseService
{
	private static Point? _oldPosition;
	private static Point? _position;

	public static Point OldPosition
	{
		get
		{
			if (_oldPosition == null)
			{
				throw new Exception("Update() wasn't called.");
			}

			return _oldPosition.Value;
		}
	}

	public static Point Position
	{
		get
		{
			if (_position == null)
			{
				throw new Exception("Update() wasn't called.");
			}

			return _position.Value;
		}
	}

	public static Point Delta => new Point(Position.X - OldPosition.X, Position.Y - OldPosition.Y);

	public static bool Locked
	{
		get => !MyGame.Instance.IsMouseVisible;

		set
		{
			if (value == Locked)
				return;

			MyGame.Instance.IsMouseVisible = !value;
			_position = null;
			UpdateLockedMouse();
		}
	}

	public static void Update()
	{
		var mouseState = Mouse.GetState();
		var newPosition = new Point(mouseState.X, mouseState.Y);

		if (_position == null)
		{
			_oldPosition = _position = newPosition;
		}
		else
		{
			if (!Locked)
			{
				_oldPosition = _position;
			}
			else
			{
				var cb = MyGame.Instance.Window.ClientBounds;
				var center = new Point(cb.Width / 2, cb.Height / 2);

				_oldPosition = center;
			}

			_position = newPosition;
		}

		UpdateLockedMouse();
	}

	private static void UpdateLockedMouse()
	{
		if (!Locked)
			return;

		var cb = MyGame.Instance.Window.ClientBounds;
		var center = new Point(cb.Width / 2, cb.Height / 2);
		Mouse.SetPosition(center.X, center.Y);
	}
}
