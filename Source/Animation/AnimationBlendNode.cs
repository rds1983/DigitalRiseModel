using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Animation tree node that blends multiple child animation nodes.
	/// Supports recursive animation tree structures.
	/// </summary>
	public class AnimationBlendNode : AnimationTreeNode
	{
		private string _name;

		/// <summary>
		/// Gets or sets the name of this blend node.
		/// </summary>
		public override string Name => _name;

		/// <summary>
		/// Gets the list of animation blend layers.
		/// </summary>
		public List<AnimationBlendLayer> Layers { get; } = new List<AnimationBlendLayer>();

		/// <summary>
		/// Gets the duration of this blend node (the maximum duration of all layers).
		/// </summary>
		public override TimeSpan Duration
		{
			get
			{
				if (Layers.Count == 0)
					return TimeSpan.Zero;

				return Layers.Max(c => c.Node.Duration);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationBlendNode"/> class.
		/// </summary>
		/// <param name="isLooped">Whether the animation loops when time exceeds duration.</param>
		public AnimationBlendNode(bool isLooped = false)
			: this(null, isLooped)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationBlendNode"/> class with a name.
		/// </summary>
		/// <param name="name">The name of this blend node.</param>
		/// <param name="isLooped">Whether the animation loops when time exceeds duration.</param>
		public AnimationBlendNode(string name, bool isLooped = false)
		{
			_name = name;
			IsLooped = isLooped;
		}

		/// <summary>
		/// Adds an animation layer with a tree node and the specified weight.
		/// </summary>
		/// <param name="node">The animation tree node to add.</param>
		/// <param name="weight">The blend weight for this layer (0.0 to 1.0). Weights are normalized.</param>
		/// <returns>The newly created <see cref="AnimationBlendLayer"/> that can be further configured.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="weight"/> is negative or greater than 1.0.</exception>
		public AnimationBlendLayer AddLayer(AnimationTreeNode node, float weight = 1.0f)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			var result = new AnimationBlendLayer(node, weight);
			Layers.Add(result);

			return result;
		}

		/// <summary>
		/// Adds an animation layer with a clip and the specified weight.
		/// </summary>
		/// <param name="clip">The animation clip to add.</param>
		/// <param name="weight">The blend weight for this layer (0.0 to 1.0). Weights are normalized.</param>
		/// <param name="isLooped">Whether the clip should loop when it reaches the end.</param>
		/// <returns>The newly created <see cref="AnimationBlendLayer"/> that can be further configured.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="weight"/> is negative or greater than 1.0.</exception>
		public AnimationBlendLayer AddLayer(AnimationClip clip, float weight = 1.0f, bool isLooped = false)
		{
			if (clip == null)
			{
				throw new ArgumentNullException(nameof(clip));
			}

			return AddLayer(new AnimationClipNode(clip, isLooped), weight);
		}

		/// <summary>
		/// Clears all animation layers.
		/// </summary>
		public void ClearLayers()
		{
			Layers.Clear();
		}

		/// <summary>
		/// Recursively processes layers and blends their results with weighted mixing.
		/// Each layer's animation is offset by its <see cref="AnimationBlendLayer.TimeOffset"/> value.
		/// </summary>
		internal override void Process(AnimationContext context, TimeSpan time, float weight)
		{
			if (Layers.Count == 0)
				return;

			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			// Process each child with its weight
			foreach (var layer in Layers)
			{
				layer.Node.Process(context, time + layer.TimeOffset, layer.Weight);
			}

			context.SetWeights(weight);
		}
	}
}
