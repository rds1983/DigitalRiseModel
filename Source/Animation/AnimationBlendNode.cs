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
		private string _name;

		/// <summary>
		/// Gets or sets the name of this blend node.
		/// </summary>
		public override string Name => _name;

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
			_children = new List<(AnimationTreeNode, float)>();
			IsLooped = isLooped;
		}

		/// <summary>
		/// Adds a child animation clip with the specified weight.
		/// </summary>
		/// <param name="clip">The animation clip to add.</param>
		/// <param name="weight">The blend weight for this child (0.0 to 1.0). Weights are normalized.</param>
		/// <param name="isLooped">Whether the clip should loop when it reaches the end.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="weight"/> is negative or greater than 1.0.</exception>
		public void AddChild(AnimationClip clip, float weight = 1.0f, bool isLooped = false)
		{
			if (clip == null)
				throw new ArgumentNullException(nameof(clip));

			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			var clipNode = new AnimationClipNode(clip, isLooped);
			_children.Add((clipNode, weight));
		}

		/// <summary>
		/// Adds a child animation node with the specified weight.
		/// </summary>
		/// <param name="node">The child animation node to add.</param>
		/// <param name="weight">The blend weight for this child (0.0 to 1.0). Weights are normalized.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="weight"/> is negative or greater than 1.0.</exception>
		public void AddChild(AnimationTreeNode node, float weight = 1.0f)
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

		internal override void Process(AnimationContext context, TimeSpan time, float weight)
		{
			if (_children.Count == 0)
				return;

			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			TimeSpan effectiveTime = GetEffectiveTime(time);

			foreach (var (child, childWeight) in _children)
			{
				child.Process(context, effectiveTime, childWeight);
			}

			context.SetWeights(weight);
		}
	}
}
