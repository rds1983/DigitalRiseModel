using System;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Base class for animation tree nodes that support recursive blending of animation clips.
	/// </summary>
	public abstract class AnimationTreeNode
	{
		/// <summary>
		/// Gets or sets whether this animation node loops when time exceeds duration.
		/// </summary>
		public bool IsLooped { get; set; }

		/// <summary>
		/// Gets the duration of this animation tree node.
		/// </summary>
		public abstract TimeSpan Duration { get; }

		/// <summary>
		/// Samples the animation at the specified time and applies the resulting pose to the skeleton.
		/// </summary>
		/// <param name="skeleton">The skeleton to apply the animation to.</param>
		/// <param name="time">The current playback time.</param>
		/// <param name="weight">The blend weight for this node (0.0 to 1.0).</param>
		public abstract void Sample(ISkeleton skeleton, TimeSpan time, float weight);

		/// <summary>
		/// Gets the effective time, handling looping behavior.
		/// </summary>
		/// <param name="time">The requested time.</param>
		/// <returns>The effective time, clamped or looped based on IsLooped setting.</returns>
		protected TimeSpan GetEffectiveTime(TimeSpan time) => time.GetEffectiveTime(Duration, IsLooped);
	}
}
