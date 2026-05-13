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
		/// Gets the name of the animation clip.
		/// </summary>
		public override string Name => _clip.Name;

		/// <summary>
		/// Gets the duration of this animation node.
		/// </summary>
		public override TimeSpan Duration => _clip.Duration;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationClipNode"/> class.
		/// </summary>
		/// <param name="clip">The animation clip to play.</param>
		/// <param name="isLooped">Whether the animation loops when time exceeds duration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		public AnimationClipNode(AnimationClip clip, bool isLooped = false)
		{
			_clip = clip ?? throw new ArgumentNullException(nameof(clip));
			IsLooped = isLooped;
		}

		/// <summary>
		/// Samples the animation clip at the specified time and blends it into the skeleton.
		/// </summary>
		/// <param name="skeleton">The skeleton to apply the animation to.</param>
		/// <param name="time">The current playback time.</param>
		/// <param name="weight">The blend weight for this animation (0.0 to 1.0).</param>
		public override void Sample(ISkeleton skeleton, TimeSpan time, float weight)
		{
			if (skeleton == null)
				throw new ArgumentNullException(nameof(skeleton));

			if (weight < 0 || weight > 1)
				throw new ArgumentException("Weight must be between 0.0 and 1.0 inclusive.", nameof(weight));

			TimeSpan effectiveTime = GetEffectiveTime(time);

			foreach (var channel in _clip.Channels)
			{
				SrtTransform pose = SampleChannel(channel, effectiveTime);

				if (weight < 1.0f)
				{
					SrtTransform defaultPose = skeleton.GetDefaultPose(channel.BoneIndex);
					pose = BlendPoses(defaultPose, pose, weight);
				}

				skeleton.SetPose(channel.BoneIndex, pose);
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

		/// <summary>
		/// Blends between two poses using the specified weight.
		/// </summary>
		private static SrtTransform BlendPoses(SrtTransform pose1, SrtTransform pose2, float weight)
		{
			return SrtTransform.Interpolate(
				pose1,
				pose2,
				weight,
				InterpolationMode.Linear,
				InterpolationMode.Linear,
				InterpolationMode.Linear
			);
		}
	}
}
