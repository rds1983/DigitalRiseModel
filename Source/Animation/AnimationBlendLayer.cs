using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Represents an animation layer in a blend node, containing an animation tree node and its blend weight.
	/// </summary>
	public class AnimationBlendLayer
	{
		private float _weight;

		/// <summary>
		/// Gets the animation tree node for this layer.
		/// </summary>
		public AnimationTreeNode Node { get; }

		/// <summary>
		/// Gets or sets the blend weight for this layer (0.0 to 1.0). Weights are normalized.
		/// </summary>
		/// <exception cref="ArgumentException">Weight is negative or greater than 1.0.</exception>
		public float Weight
		{
			get => _weight;

			set
			{
				if (value < 0 || value > 1)
				{
					throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(value));
				}

				_weight = value;
			}
		}

		/// <summary>
		/// Gets or sets the time offset applied when processing this layer's animation.
		/// </summary>
		public TimeSpan TimeOffset { get; set; } = TimeSpan.Zero;

		/// <summary>
		/// Gets or sets an optional filter to restrict which bones this layer affects.
		/// If set, only bones in this filter will be modified by this layer's animation.
		/// </summary>
		public HashSet<int> BoneFilter { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationBlendLayer"/> class.
		/// </summary>
		/// <param name="node">The animation tree node for this layer.</param>
		/// <param name="weight">The blend weight (0.0 to 1.0). Defaults to 1.0.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="weight"/> is negative or greater than 1.0.</exception>
		public AnimationBlendLayer(AnimationTreeNode node, float weight = 1.0f)
		{
			Node = node ?? throw new ArgumentNullException(nameof(node));
			Weight = weight;
		}
	}
}
