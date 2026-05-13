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
		private readonly List<(AnimationTreeNode Node, float Weight)> _children;

		/// <summary>
		/// Gets the child nodes and their weights.
		/// </summary>
		public IReadOnlyList<(AnimationTreeNode Node, float Weight)> Children => _children.AsReadOnly();

		/// <summary>
		/// Gets the duration of this blend node (the maximum duration of all children).
		/// </summary>
		public override TimeSpan Duration
		{
			get
			{
				if (_children.Count == 0)
					return TimeSpan.Zero;

				return _children.Max(c => c.Node.Duration);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationBlendNode"/> class.
		/// </summary>
		/// <param name="isLooped">Whether the animation loops when time exceeds duration.</param>
		public AnimationBlendNode(bool isLooped = false)
		{
			_children = new List<(AnimationTreeNode, float)>();
			IsLooped = isLooped;
		}

		/// <summary>
		/// Adds a child animation node with the specified weight.
		/// </summary>
		/// <param name="node">The child animation node to add.</param>
		/// <param name="weight">The blend weight for this child (0.0 to 1.0). Weights are normalized.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="weight"/> is negative or greater than 1.0.</exception>
		public void AddChild(AnimationTreeNode node, float weight)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			_children.Add((node, weight));
		}

		/// <summary>
		/// Removes a child animation node.
		/// </summary>
		/// <param name="node">The child node to remove.</param>
		/// <returns>true if the node was removed; otherwise, false.</returns>
		public bool RemoveChild(AnimationTreeNode node)
		{
			int index = _children.FindIndex(c => c.Node == node);
			if (index >= 0)
			{
				_children.RemoveAt(index);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Clears all child nodes.
		/// </summary>
		public void ClearChildren()
		{
			_children.Clear();
		}

		/// <summary>
		/// Sets the weight for a specific child node.
		/// </summary>
		/// <param name="node">The child node to update.</param>
		/// <param name="weight">The new weight (0.0 to 1.0).</param>
		/// <exception cref="ArgumentException">The child node is not found or weight is invalid.</exception>
		public void SetChildWeight(AnimationTreeNode node, float weight)
		{
			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			int index = _children.FindIndex(c => c.Node == node);
			if (index < 0)
				throw new ArgumentException("Child node not found.", nameof(node));

			_children[index] = (node, weight);
		}

		/// <summary>
		/// Samples all child animation nodes and blends them together.
		/// </summary>
		/// <param name="skeleton">The skeleton to apply the animation to.</param>
		/// <param name="time">The current playback time.</param>
		/// <param name="weight">The blend weight for this blend node (0.0 to 1.0).</param>
		public override void Sample(ISkeleton skeleton, TimeSpan time, float weight)
		{
			if (skeleton == null)
				throw new ArgumentNullException(nameof(skeleton));

			if (_children.Count == 0)
				return;

			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			TimeSpan effectiveTime = GetEffectiveTime(time);

			float totalWeight = _children.Sum(c => c.Weight);
			if (totalWeight <= 0)
				return;

			float normalizedWeight = weight / totalWeight;

			foreach (var (child, childWeight) in _children)
			{
				float blendWeight = childWeight * normalizedWeight;
				child.Sample(skeleton, effectiveTime, blendWeight);
			}
		}
	}
}
