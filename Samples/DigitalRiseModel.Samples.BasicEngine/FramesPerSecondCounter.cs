using System;
using Microsoft.Xna.Framework;

namespace DigitalRiseModel.Samples.BasicEngine
{
	/// <summary>
	/// Tracks and calculates the frames per second (FPS) for performance monitoring.
	/// Updates the FPS count once per second based on the number of frames drawn.
	/// </summary>
	public class FramesPerSecondCounter
	{
		private static readonly TimeSpan _oneSecondTimeSpan = new TimeSpan(0, 0, 1);
		private int _framesCounter;
		private TimeSpan _timer = _oneSecondTimeSpan;

		/// <summary>
		/// Initializes a new instance of the FramesPerSecondCounter class.
		/// </summary>
		public FramesPerSecondCounter()
		{
		}

		/// <summary>
		/// Gets the current frames per second value. Updated once per second.
		/// </summary>
		public int FramesPerSecond { get; private set; }

		/// <summary>
		/// Updates the internal timer. Should be called once per game update.
		/// When one second has elapsed, updates the FramesPerSecond value.
		/// </summary>
		/// <param name="gameTime">The elapsed game time.</param>
		public void Update(GameTime gameTime)
		{
			_timer += gameTime.ElapsedGameTime;
			if (_timer <= _oneSecondTimeSpan)
				return;

			FramesPerSecond = _framesCounter;
			_framesCounter = 0;
			_timer -= _oneSecondTimeSpan;
		}

		/// <summary>
		/// Increments the frame counter. Should be called once per game draw.
		/// </summary>
		/// <param name="gameTime">The elapsed game time.</param>
		public void Draw(GameTime gameTime)
		{
			_framesCounter++;
		}
	}
}