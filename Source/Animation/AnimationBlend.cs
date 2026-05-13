using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Represents a weighted blend of multiple animation clips.
	/// Caches transforms and invalidates on weight/time changes for efficiency.
	/// </summary>
	public class AnimationBlend : IAnimationClip
	{
		private class BlendedClip
		{
			public IAnimationClip Clip { get; set; }
			public float Weight { get; set; }
			public TimeSpan Time { get; set; }
			public Dictionary<int, SrtTransform> Transforms { get; set; }
		}

		private List<BlendedClip> _clips = new List<BlendedClip>();
		private float _totalWeight;
		private readonly Dictionary<int, SrtTransform> _transforms = new Dictionary<int, SrtTransform>();
		private bool _transformsDirty = true;

		/// <summary>
		/// Gets the name of the animation blend.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the number of animation clips in this blend.
		/// </summary>
		public int ClipCount => _clips.Count;

		/// <summary>
		/// Initializes a new instance of the AnimationBlend class.
		/// </summary>
		/// <param name="name">The name of the animation blend.</param>
		public AnimationBlend(string name)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		/// <summary>
		/// Adds an animation clip to the blend with a specified weight.
		/// </summary>
		/// <param name="clip">The animation clip to add.</param>
		/// <param name="weight">The weight of the clip (default is 1.0).</param>
		public void AddClip(AnimationClip clip, float weight = 1.0f)
		{
			if (clip == null)
				throw new ArgumentNullException(nameof(clip));

			if (weight < 0)
				throw new ArgumentException("Weight must be non-negative", nameof(weight));

			_clips.Add(new BlendedClip { Clip = clip, Weight = weight, Time = TimeSpan.Zero });
			_totalWeight += weight;
			InvalidateTransforms();
		}

		/// <summary>
		/// Sets the weight for a clip at the specified index.
		/// </summary>
		/// <param name="index">The index of the clip.</param>
		/// <param name="weight">The new weight.</param>
		public void SetClipWeight(int index, float weight)
		{
			if (index < 0 || index >= _clips.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (weight < 0)
				throw new ArgumentException("Weight must be non-negative", nameof(weight));

			_totalWeight -= _clips[index].Weight;
			_clips[index].Weight = weight;
			_totalWeight += weight;
			InvalidateTransforms();
		}

		/// <summary>
		/// Gets the clip at the specified index.
		/// </summary>
		public IAnimationClip GetClip(int index) => _clips[index].Clip;

		/// <summary>
		/// Gets the weight of the clip at the specified index.
		/// </summary>
		public float GetClipWeight(int index) => _clips[index].Weight;

		/// <summary>
		/// Gets the time for the clip at the specified index.
		/// </summary>
		public TimeSpan GetClipTime(int index) => _clips[index].Time;

		/// <summary>
		/// Sets the time for the clip at the specified index.
		/// </summary>
		public void SetClipTime(int index, TimeSpan time)
		{
			if (index < 0 || index >= _clips.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			_clips[index].Time = time;
			InvalidateTransforms();
		}

		/// <summary>
		/// Gets the normalized weight for a clip (dividing by total weight).
		/// </summary>
		public float GetNormalizedWeight(int index) => _totalWeight > 0 ? _clips[index].Weight / _totalWeight : 0;

		/// <summary>
		/// Clears all clips from the blend.
		/// </summary>
		public void Clear()
		{
			_clips.Clear();
			_totalWeight = 0;
			InvalidateTransforms();
		}

		private void InvalidateTransforms()
		{
			// Mark cached transforms as stale; GetTransforms will recompute on next call
			_transforms.Clear();
			_transformsDirty = true;
		}

		#region IAnimationClip Implementation (Extensions)

		TimeSpan IAnimationClip.Duration => _clips.Count > 0 ? _clips[0].Clip.Duration : TimeSpan.Zero;

		public Dictionary<int, SrtTransform> GetTransforms(TimeSpan time2)
		{
			// Get transforms from each clip at the requested time
			foreach (var clip in _clips)
			{
				clip.Transforms = clip.Clip.GetTransforms(time2);
			}

			// Collect all affected bone indices on first call or after weight/time changes
			if (_transformsDirty)
			{
				_transforms.Clear();
				foreach (var clip in _clips)
				{
					foreach (var ct in clip.Transforms)
					{
						_transforms[ct.Key] = ct.Value;
					}
				}

				_transformsDirty = false;
			}

			// Blend poses for all bones using weighted interpolation
			foreach (var pair in _transforms)
			{
				float totalWeight = 0;
				SrtTransform blendedPose = SrtTransform.Identity;

				foreach (var clip in _clips)
				{
					if (clip.Transforms.TryGetValue(pair.Key, out var pose))
					{
						if (totalWeight == 0)
						{
							blendedPose = pose;
						}
						else
						{
							float normalizedWeight = clip.Weight / (totalWeight + clip.Weight);
							blendedPose = SrtTransform.Interpolate(blendedPose, pose, normalizedWeight,
								InterpolationMode.Linear, InterpolationMode.Linear, InterpolationMode.Linear);
						}

						totalWeight += clip.Weight;
					}
				}

				_transforms[pair.Key] = blendedPose;
			}

			return _transforms;
		}

		#endregion
	}
}
