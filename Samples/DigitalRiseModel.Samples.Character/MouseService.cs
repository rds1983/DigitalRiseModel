using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DigitalRiseModel.Samples;

internal static class MouseService
{
	private static Point? _lastPosition = null;

	public static Point Delta { get; private set; }

	public static bool Locked
	{
		get => !MyGame.Instance.IsMouseVisible;

		set
		{
			if (value == Locked)
				return;

			MyGame.Instance.IsMouseVisible = !value;
			_lastPosition = null;
			Delta = Point.Zero;
			UpdateLockedMouse();
		}
	}

	public static void Update()
	{
		var mouseState = Mouse.GetState();
		var newPosition = new Point(mouseState.X, mouseState.Y);

		if (_lastPosition != null)
		{
			Point oldPosition;
			if (!Locked)
			{
				oldPosition = _lastPosition.Value;
			}
			else
			{
				var cb = MyGame.Instance.Window.ClientBounds;
				var center = new Point(cb.Width / 2, cb.Height / 2);

				oldPosition = center;
			}

			Delta = new Point(newPosition.X - oldPosition.X, newPosition.Y - oldPosition.Y);
		}

		_lastPosition = newPosition;

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
