/*
 * AnimationClip.cs
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
using System.Collections.Generic;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Represents a single animation clip containing channels for individual bones.
	/// Computes transforms on-demand for any given playback time without maintaining state.
	/// </summary>
	public class AnimationClip
	{
		private readonly Dictionary<int, SrtTransform> _transforms = new Dictionary<int, SrtTransform>();

		/// <summary>
		/// Gets the name of this animation clip.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the duration of this animation clip.
		/// </summary>
		public TimeSpan Duration { get; }

		/// <summary>
		/// Gets the animation channels in this clip.
		/// </summary>
		public AnimationChannel[] Channels { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationClip"/> class.
		/// </summary>
		/// <param name="name">The name of the animation clip.</param>
		/// <param name="duration">The duration of the animation clip.</param>
		/// <param name="channels">The animation channels in this clip. Must contain at least one channel.</param>
		/// <exception cref="ArgumentNullException"><paramref name="channels"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="channels"/> is empty.</exception>
		public AnimationClip(string name, TimeSpan duration, AnimationChannel[] channels)
		{
			if (channels == null)
			{
				throw new ArgumentNullException(nameof(channels));
			}

			if (channels.Length == 0)
			{
				throw new ArgumentException("no channels", nameof(channels));
			}

			Name = name;
			Duration = duration;
			Channels = channels;

			foreach (var channel in channels)
			{
				_transforms[channel.BoneIndex] = new SrtTransform();
			}
		}
	}
}