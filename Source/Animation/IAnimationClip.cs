using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Interface for animation sources that can be played by AnimationController.
	/// Provides a stateless way to compute transforms for any given playback time.
	/// Encapsulates both single clips and blended clips uniformly.
	/// </summary>
	public interface IAnimationClip
	{
		/// <summary>
		/// Gets the total duration of the animation.
		/// </summary>
		TimeSpan Duration { get; }

		/// <summary>
		/// Computes bone transforms for the specified playback time.
		/// Returns a dictionary mapping bone indices to their interpolated poses.
		/// </summary>
		Dictionary<int, SrtTransform> GetTransforms(TimeSpan time);
	}
}
