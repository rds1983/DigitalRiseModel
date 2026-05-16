using System;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Animation tree node that plays a single animation clip.
	/// </summary>
	public class AnimationClipNode : AnimationTreeNode
	{
		private readonly AnimationClip _clip;

		/// <summary>
		/// Gets the animation clip played by this node.
		/// </summary>
		public AnimationClip Clip => _clip;

		/// <summary>
		/// Gets the duration of this animation node.
		/// </summary>
		public override TimeSpan Duration => _clip.Duration;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationClipNode"/> class.
		/// </summary>
		/// <param name="clip">The animation clip to play.</param>
		/// <param name="flags">Animation playback flags.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		public AnimationClipNode(AnimationClip clip, AnimationFlags flags = AnimationFlags.None)
		{
			_clip = clip ?? throw new ArgumentNullException(nameof(clip));
			Flags = flags;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationClipNode"/> class.
		/// </summary>
		/// <param name="clip">The animation clip to play.</param>
		/// <param name="isLooped">Whether the animation loops when time exceeds duration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		public AnimationClipNode(AnimationClip clip, bool isLooped) : this(clip, isLooped ? AnimationFlags.Looped : AnimationFlags.None)
		{
		}

		/// Samples the clip at the given time and contributes bone transforms to the context.
		internal override void Process(AnimationContext context, TimeSpan time, float weight)
		{
			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			TimeSpan effectiveTime = GetEffectiveTime(time);

			foreach (var channel in _clip.Channels)
			{
				SrtTransform pose = SampleChannel(channel, effectiveTime);
				context.SetTransform(channel.BoneIndex, pose, weight);
			}
		}

		/// <summary>
		/// Samples a single animation channel at the specified time.
		/// </summary>
		private static SrtTransform SampleChannel(AnimationChannel channel, TimeSpan time)
		{
			var keyframes = channel.Keyframes;

			if (keyframes.Length == 1)
				return keyframes[0].Pose;

			int? keyframeIndex = keyframes.GetKeyframeIndexByTime(time);

			if (!keyframeIndex.HasValue)
				return keyframes[0].Pose;

			int index = keyframeIndex.Value;

			if (index >= keyframes.Length - 1)
				return keyframes[keyframes.Length - 1].Pose;

			var keyframe1 = keyframes[index];
			var keyframe2 = keyframes[index + 1];

			TimeSpan timeDiff = keyframe2.Time - keyframe1.Time;
			if (timeDiff.TotalSeconds <= 0)
				return keyframe1.Pose;

			float blend = (float)((time - keyframe1.Time).TotalSeconds / timeDiff.TotalSeconds);
			blend = Math.Max(0, Math.Min(1, blend));

			return SrtTransform.Interpolate(
				keyframe1.Pose,
				keyframe2.Pose,
				blend,
				channel.TranslationMode,
				channel.RotationMode,
				channel.ScaleMode
			);
		}
	}
}
