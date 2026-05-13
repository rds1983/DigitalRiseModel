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
	}
}
