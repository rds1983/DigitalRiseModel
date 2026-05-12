/*
 * AnimationChannel.cs
 * Author: Bruno Evangelista
 * Copyright (c) 2008 Bruno Evangelista. All rights reserved.
 *
 * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Represents an animation channel for a single bone in an animation clip.
	/// </summary>
	public class AnimationChannel
	{
		/// <summary>
		/// Gets the index of the bone that this channel animates.
		/// </summary>
		public int BoneIndex { get; }

		/// <summary>
		/// Gets the keyframes in this animation channel.
		/// </summary>
		public AnimationChannelKeyframe[] Keyframes { get; }

		/// <summary>
		/// Gets or sets the interpolation mode for the translation component. Default is Linear.
		/// </summary>
		public InterpolationMode TranslationMode { get; set; } = InterpolationMode.Linear;

		/// <summary>
		/// Gets or sets the interpolation mode for the rotation component. Default is Linear.
		/// </summary>
		public InterpolationMode RotationMode { get; set; } = InterpolationMode.Linear;

		/// <summary>
		/// Gets or sets the interpolation mode for the scale component. Default is Linear.
		/// </summary>
		public InterpolationMode ScaleMode { get; set; } = InterpolationMode.Linear;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationChannel"/> class.
		/// </summary>
		/// <param name="boneIndex">The index of the bone that this channel animates. Must be non-negative.</param>
		/// <param name="keyframes">The keyframes in this channel. Must contain at least one keyframe.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="boneIndex"/> is negative.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="keyframes"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="keyframes"/> is empty.</exception>
		public AnimationChannel(int boneIndex, AnimationChannelKeyframe[] keyframes)
		{
			if (boneIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(boneIndex));
			}

			if (keyframes == null)
			{
				throw new ArgumentNullException(nameof(keyframes));
			}

			if (keyframes.Length == 0)
			{
				throw new ArgumentException("no keyframes", nameof(keyframes));
			}

			BoneIndex = boneIndex;
			Keyframes = keyframes;
		}

		/// <summary>
		/// Gets the index of the keyframe at or just before the specified time.
		/// </summary>
		/// <param name="time">The time to search for.</param>
		/// <returns>The index of the nearest keyframe at or before the specified time.</returns>
		public int GetKeyframeIndexByTime(TimeSpan time) => Keyframes.GetKeyframeIndexByTime(time) ?? 0;

		/// <summary>
		/// Gets the keyframe at or just before the specified time.
		/// </summary>
		/// <param name="time">The time to search for.</param>
		/// <returns>The keyframe at or before the specified time.</returns>
		public AnimationChannelKeyframe GetKeyframeByTime(TimeSpan time)
		{
			int index = GetKeyframeIndexByTime(time);
			return Keyframes[index];
		}

		/// <summary>
		/// Returns a string representation of this animation channel.
		/// </summary>
		/// <returns>A string in the format "BoneIndex:KeyframeCount".</returns>
		public override string ToString() => $"{BoneIndex}:{Keyframes.Length}";
	}
}