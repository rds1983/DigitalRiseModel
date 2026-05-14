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

		/// <summary>
		/// Evaluates this node and contributes its transforms to the animation context.
		/// Each node type implements this to add its animation data with proper weighting.
		/// </summary>
		/// <param name="context">The context accumulating transforms from the entire tree.</param>
		/// <param name="time">Current playback time.</param>
		/// <param name="weight">Blend weight [0.0, 1.0] for this node's contribution.</param>
		internal abstract void Process(AnimationContext context, TimeSpan time, float weight);

		/// <summary>
		/// Gets the effective time, handling looping behavior.
		/// </summary>
		/// <param name="time">The requested time.</param>
		/// <returns>The effective time, clamped or looped based on IsLooped setting.</returns>
		protected TimeSpan GetEffectiveTime(TimeSpan time) => time.GetEffectiveTime(Duration, IsLooped);
	}
}
