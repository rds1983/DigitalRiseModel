using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	internal static class AnimationUtility
	{
		/// <summary>
		/// Gets the index of the keyframe at or just before the specified time using binary search.
		/// </summary>
		/// <param name="keyframes">The keyframe collection (must be sorted by time).</param>
		/// <param name="time">The time to search for.</param>
		/// <returns>The index of the keyframe at or before the specified time, or null if the time is before the first keyframe.</returns>
		/// <remarks>
		/// This method uses binary search to find the keyframe in O(log n) time.
		/// It assumes the keyframes are sorted by time in ascending order.
		/// If the exact time is found, it returns that index.
		/// If the time falls between keyframes, it returns the index of the keyframe before it.
		/// If the time is after the last keyframe, it returns the index of the last keyframe.
		/// </remarks>
		public static int? GetKeyframeIndexByTime(this IReadOnlyList<AnimationChannelKeyframe> keyframes, TimeSpan time)
		{
			// Handle empty collection
			if (keyframes.Count == 0)
				return null;

			// Handle time before first keyframe - return null instead of clamping to 0
			if (time < keyframes[0].Time)
				return null;

			// Binary search: find the keyframe at or just before the specified time
			int startIndex = 0;
			int endIndex = keyframes.Count - 1;

			while (startIndex <= endIndex)
			{
				int middleIndex = (startIndex + endIndex) / 2;
				TimeSpan middleTime = keyframes[middleIndex].Time;

				if (middleTime == time)
					// Exact match found
					return middleIndex;
				else if (middleTime < time)
					// Target time is after middle, search right half
					startIndex = middleIndex + 1;
				else
					// Target time is before middle, search left half
					endIndex = middleIndex - 1;
			}

			// When loop exits, endIndex points to the keyframe at or just before the time
			return endIndex;
		}

		/// <summary>
		/// Gets the effective time within an animation cycle, handling looping and negative time.
		/// </summary>
		/// <param name="time">The requested time (can be negative or exceed duration).</param>
		/// <param name="duration">The duration of one animation cycle.</param>
		/// <param name="isLooped">Whether the animation loops or clamps to duration.</param>
		/// <returns>The effective time clamped or wrapped to [0, duration).</returns>
		public static TimeSpan GetEffectiveTime(this TimeSpan time, TimeSpan duration, bool isLooped)
		{
			// Invalid duration - clamp to zero
			if (duration.TotalSeconds <= 0)
			{
				return TimeSpan.Zero;
			}

			// Time is already within valid range - return as-is
			if (TimeSpan.Zero <= time && time <= duration)
			{
				return time;
			}

			if (isLooped)
			{
				// Handle negative time by wrapping backwards through cycles
				if (time < TimeSpan.Zero)
				{
					double cycles = Math.Ceiling(-time.TotalSeconds / duration.TotalSeconds);
					return TimeSpan.FromSeconds((cycles * duration.TotalSeconds) + time.TotalSeconds);
				}
				else
				{
					// Handle positive time beyond duration by wrapping forward through cycles
					double cycles = Math.Floor(time.TotalSeconds / duration.TotalSeconds);
					return TimeSpan.FromSeconds(time.TotalSeconds - (cycles * duration.TotalSeconds));
				}
			}

			// Non-looped animations clamp at boundaries
			if (time < TimeSpan.Zero)
			{
				return TimeSpan.Zero;
			}

			return duration;
		}
	}
}
