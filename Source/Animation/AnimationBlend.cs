using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Represents a blend of multiple animation clips with individual weights.
	/// </summary>
	public class AnimationBlend
	{
		private class BlendedClip
		{
			public AnimationClip Clip { get; set; }
			public float Weight { get; set; }
			public TimeSpan Time { get; set; }
		}

		private List<BlendedClip> _clips = new List<BlendedClip>();
		private float _totalWeight;

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
		}

		/// <summary>
		/// Gets the clip at the specified index.
		/// </summary>
		public AnimationClip GetClip(int index) => _clips[index].Clip;

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
		}

		/// <summary>
		/// Advances the animation time by the specified elapsed time.
		/// </summary>
		public void UpdateTime(TimeSpan elapsedTime)
		{
			for (int i = 0; i < _clips.Count; i++)
			{
				_clips[i].Time += elapsedTime;
			}
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
		}
	}
}
