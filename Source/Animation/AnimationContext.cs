using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Accumulates and blends animation transforms from the animation tree.
	/// As nodes contribute transforms via SetTransform(), the context blends them with proper weighting.
	/// </summary>
	internal class AnimationContext
	{
		/// Per-bone data: accumulated transform and total blend weight (null if unmodified).
		private struct BoneData
		{
			public SrtTransform Transform;
			public float? Weight;
		}

		private BoneData[] _bones;

		public ISkeleton Skeleton { get; }

		/// <summary>
		/// Gets or sets an optional filter to restrict which bones can be modified.
		/// If set, only bones in this filter will accept animation transforms.
		/// </summary>
		public HashSet<int> BoneFilter { get; set; }

		public AnimationContext(ISkeleton skeleton)
		{
			Skeleton = skeleton ?? throw new ArgumentNullException(nameof(skeleton));
		}

		/// Updates weights for all modified bones (used by blend nodes to scale contributions).
		/// <param name="value">The new weight to apply, or null to clear weights.</param>
		public void SetWeights(float? value)
		{
			for (var i = 0; i < _bones.Length; i++)
			{
				if (_bones[i].Weight == null)
				{
					continue;
				}

				_bones[i].Weight = value;
			}
		}

		/// Clears accumulated bone data for a new evaluation pass. Allocates on first use.
		public void Reset()
		{
			// Allocate or resize bone array to match skeleton size
			if (_bones == null || _bones.Length != Skeleton.BonesCount)
			{
				_bones = new BoneData[Skeleton.BonesCount];
			}

			// Clear all weights to mark bones as unmodified
			for (var i = 0; i < _bones.Length; i++)
			{
				_bones[i].Weight = null;
			}
		}

		/// <summary>
		/// Accumulates a bone transform, blending with existing transforms using normalized weights.
		/// First call stores as-is; subsequent calls interpolate proportionally regardless of order.
		/// Respects the active bone filter if set, ignoring transforms for filtered-out bones.
		/// </summary>
		public void SetTransform(int boneIndex, SrtTransform transform, float weight)
		{
			if (BoneFilter != null && !BoneFilter.Contains(boneIndex))
			{
				return;
			}

			var curWeight = _bones[boneIndex].Weight;
			if (curWeight == null)
			{
				// First contribution: store transform and weight directly
				_bones[boneIndex].Transform = transform;
				_bones[boneIndex].Weight = weight;
			}
			else
			{
				// Blend with existing transform using normalized weights
				var newWeight = curWeight.Value + weight;
				var normalizedWeight = weight / newWeight;

				// Interpolate between existing and new transform proportionally
				_bones[boneIndex].Transform = SrtTransform.Interpolate(_bones[boneIndex].Transform, transform, normalizedWeight,
								InterpolationMode.Linear, InterpolationMode.Linear, InterpolationMode.Linear);
				_bones[boneIndex].Weight = newWeight;
			}
		}

		/// Applies accumulated bone transforms to the skeleton (final step of the pipeline).
		public void SetPoses()
		{
			for (var i = 0; i < _bones.Length; ++i)
			{
				// Skip bones that haven't been modified by any animation
				if (_bones[i].Weight == null)
				{
					continue;
				}

				// Apply the final blended transform to the skeleton
				Skeleton.SetPose(i, _bones[i].Transform);
			}
		}
	}
}
