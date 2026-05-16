using System;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Flags controlling animation playback behavior.
	/// </summary>
	[Flags]
	public enum AnimationFlags
	{
		/// <summary>
		/// No flags set; animation plays once forward.
		/// </summary>
		None = 0,

		/// <summary>
		/// Animation loops when time exceeds duration.
		/// </summary>
		Looped = 1,

		/// <summary>
		/// Animation plays backward instead of forward.
		/// </summary>
		PlayBackwards = 2
	}

	/// <summary>
	/// Base class for animation tree nodes that support recursive blending of animation clips.
	/// </summary>
	public abstract class AnimationTreeNode
	{
		/// <summary>
		/// Gets or sets the animation playback flags (looping, playback direction, etc.).
		/// </summary>
		public AnimationFlags Flags { get; set; }

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
		/// Gets the effective time, handling looping and backward playback.
		/// </summary>
		/// <param name="time">The requested time.</param>
		/// <returns>The effective time, adjusted for looping and playback direction.</returns>
		protected TimeSpan GetEffectiveTime(TimeSpan time) => time.GetEffectiveTime(Duration, Flags);
	}
}
