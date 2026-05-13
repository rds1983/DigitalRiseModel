using System;
using Microsoft.Xna.Framework;
using DigitalRiseModel.Utility;

namespace DigitalRiseModel.Animation
{
	internal static class AnimationInterpolationUtility
	{
		public static TimeSpan HandleLooping(TimeSpan time, TimeSpan duration, bool loopEnabled)
		{
			var fDuration = (float)duration.TotalSeconds;

			if (fDuration.IsZero())
				return time;

			if (time >= TimeSpan.Zero && time <= duration)
				return time;

			if (loopEnabled)
			{
				if (time > duration)
				{
					while (time > duration)
						time -= duration;
				}
				else if (time < TimeSpan.Zero)
				{
					while (time < TimeSpan.Zero)
						time += duration;
				}
				return time;
			}
			else
			{
				return time > duration ? duration : TimeSpan.Zero;
			}
		}

		public static SrtTransform InterpolateChannelPose(AnimationChannel channel, TimeSpan time, TimeSpan duration)
		{
			if (channel.TranslationMode == InterpolationMode.None &&
				channel.RotationMode == InterpolationMode.None &&
				channel.ScaleMode == InterpolationMode.None)
			{
				int keyframeIndex = channel.GetKeyframeIndexByTime(time);
				return channel.Keyframes[keyframeIndex].Pose;
			}
			else
			{
				int currentKeyFrame = channel.GetKeyframeIndexByTime(time);
				int nextKeyframeIndex = (currentKeyFrame + 1) % channel.Keyframes.Length;

				var keyframe1 = channel.Keyframes[currentKeyFrame];
				var keyframe2 = channel.Keyframes[nextKeyframeIndex];

				long keyframeDuration;
				if (currentKeyFrame == (channel.Keyframes.Length - 1))
					keyframeDuration = duration.Ticks - keyframe1.Time.Ticks;
				else
					keyframeDuration = keyframe2.Time.Ticks - keyframe1.Time.Ticks;

				if (keyframeDuration > 0)
				{
					long elapsedKeyframeTime = time.Ticks - keyframe1.Time.Ticks;
					float lerpFactor = MathHelper.Clamp(elapsedKeyframeTime / (float)keyframeDuration, 0, 1);

					return SrtTransform.Interpolate(keyframe1.Pose, keyframe2.Pose, lerpFactor,
								channel.TranslationMode, channel.RotationMode, channel.ScaleMode);
				}
				else
				{
					return keyframe1.Pose;
				}
			}
		}
	}
}
