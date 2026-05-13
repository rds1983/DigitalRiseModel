using Xunit;
using DigitalRiseModel.Animation;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Tests
{
	public sealed class AnimationUtilityTests
	{
		private static List<AnimationChannelKeyframe> CreateKeyframes(params double[] timeSeconds)
		{
			var keyframes = new List<AnimationChannelKeyframe>();

			foreach (var seconds in timeSeconds)
			{
				keyframes.Add(new AnimationChannelKeyframe(TimeSpan.FromSeconds(seconds), SrtTransform.Identity));
			}

			return keyframes;
		}

		[Fact]
		public void GetKeyframeIndexByTime2_EmptyList_ReturnsNull()
		{
			var keyframes = new List<AnimationChannelKeyframe>();
			var result = keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(1));

			Assert.Null(result);
		}

		[Fact]
		public void GetKeyframeIndexByTime2_TimeBeforeFirstKeyframe_ReturnsNull()
		{
			var keyframes = CreateKeyframes(1.0, 2.0, 3.0);
			var result = keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(0.5));

			Assert.Null(result);
		}

		[Fact]
		public void GetKeyframeIndexByTime2_TimeAtExactKeyframe_ReturnsCorrectIndex()
		{
			var keyframes = CreateKeyframes(1.0, 2.0, 3.0, 4.0, 5.0);

			Assert.Equal(0, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(1.0)));
			Assert.Equal(2, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(3.0)));
			Assert.Equal(4, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(5.0)));
		}

		[Fact]
		public void GetKeyframeIndexByTime2_TimeBetweenKeyframes_ReturnsIndexBefore()
		{
			var keyframes = CreateKeyframes(1.0, 2.0, 3.0, 4.0, 5.0);

			Assert.Equal(0, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(1.5)));
			Assert.Equal(1, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(2.5)));
			Assert.Equal(3, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(4.7)));
		}

		[Fact]
		public void GetKeyframeIndexByTime2_TimeAfterAllKeyframes_ReturnsLastIndex()
		{
			var keyframes = CreateKeyframes(1.0, 2.0, 3.0);
			var result = keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(10.0));

			Assert.Equal(2, result);
		}

		[Fact]
		public void GetKeyframeIndexByTime2_SingleKeyframe_ReturnsZeroOrNull()
		{
			var keyframes = CreateKeyframes(2.0);

			Assert.Null(keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(1.0)));
			Assert.Equal(0, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(2.0)));
			Assert.Equal(0, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(3.0)));
		}

		[Fact]
		public void GetKeyframeIndexByTime2_TwoKeyframes_ReturnsCorrectIndex()
		{
			var keyframes = CreateKeyframes(1.0, 3.0);

			Assert.Null(keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(0.5)));
			Assert.Equal(0, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(1.0)));
			Assert.Equal(0, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(2.0)));
			Assert.Equal(1, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(3.0)));
			Assert.Equal(1, keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(4.0)));
		}

		[Fact]
		public void GetKeyframeIndexByTime2_VerifyBinarySearchComplexity()
		{
			// Create a list with 2^20 = 1,048,576 keyframes to verify O(log n) complexity
			var largeTimeValues = new double[1 << 20];
			for (int i = 0; i < largeTimeValues.Length; i++)
			{
				largeTimeValues[i] = i * 0.001;
			}

			var keyframes = CreateKeyframes(largeTimeValues);

			// Test a value in the middle of the range
			var result = keyframes.GetKeyframeIndexByTime(TimeSpan.FromSeconds(largeTimeValues[500000]));

			Assert.Equal(500000, result);
		}
	}
}
