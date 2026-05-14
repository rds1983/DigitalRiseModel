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
		/// Gets the name of this animation tree node.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Gets the duration of this animation tree node.
		/// </summary>
		public abstract TimeSpan Duration { get; }

		internal abstract void Process(AnimationContext context, TimeSpan time, float weight);

		/// <summary>
		/// Gets the effective time, handling looping behavior.
		/// </summary>
		/// <param name="time">The requested time.</param>
		/// <returns>The effective time, clamped or looped based on IsLooped setting.</returns>
		protected TimeSpan GetEffectiveTime(TimeSpan time) => time.GetEffectiveTime(Duration, IsLooped);
	}
}
