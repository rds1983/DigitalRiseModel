/*
 * AnimationController.cs
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
using DigitalRiseModel.Utility;
using Microsoft.Xna.Framework;
using System;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Specifies how an animation clip is played.
	/// </summary>
	public enum PlaybackMode
	{
		/// <summary>
		/// Plays the animation clip in the forward way.
		/// </summary>
		Forward,

		/// <summary>
		/// Plays the animation clip in the backward way.
		/// </summary>
		Backward
	};

	/// <summary>
	/// Controls how animations are played and interpolated.
	/// </summary>
	public class AnimationController
	{
		private TimeSpan _time;
		private float _speed;
		private bool _loopEnabled;

		/// <summary>
		/// Model Node
		/// </summary>
		public ISkeleton Skeleton { get; }

		/// <summary>
		/// Gets the animation source being played (either a single clip or a blend).
		/// </summary>
		public IAnimationClip AnimationClip { get; private set; }

		/// <summary>
		/// Gets os sets the current animation playback time.
		/// </summary>
		public TimeSpan Time
		{
			get { return _time; }
			set
			{
				if (value == _time)
				{
					return;
				}

				_time = value;
				UpdateAll();

				TimeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets os sets the animation playback speed.
		/// </summary>
		public float Speed
		{
			get { return _speed; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("Speed must be a positive value");
				}

				_speed = value;
			}
		}

		/// <summary>
		/// Enables animation looping.
		/// </summary>
		public bool LoopEnabled
		{
			get { return _loopEnabled; }
			set
			{
				_loopEnabled = value;

				if (HasFinished && _loopEnabled)
				{
					HasFinished = false;
				}
			}
		}

		/// <summary>
		/// Gets os sets the animation playback mode.
		/// </summary>
		public PlaybackMode PlaybackMode { get; set; }

		/// <summary>
		/// Returns whether the animation has finished.
		/// </summary>
		public bool HasFinished { get; private set; }

		/// <summary>
		/// Returns whether the animation is playing.
		/// </summary>
		public bool IsPlaying { get; private set; }

		public int CurrentKeyFrame { get; private set; }

		public event EventHandler TimeChanged;

		/// <summary>Initializes a new instance of the 
		/// <see cref="T:XNAnimation.Controllers.AnimationController" />
		/// class.
		/// </summary>
		/// <param name="skeleton">The skeleton of the model to be animated</param>
		public AnimationController(ISkeleton skeleton)
		{
			Skeleton = skeleton ?? throw new ArgumentNullException(nameof(skeleton));
			skeleton.ResetTransforms();

			_time = TimeSpan.Zero;
			_speed = 1.0f;
			_loopEnabled = true;
			PlaybackMode = PlaybackMode.Forward;

			AnimationClip = null;
			HasFinished = false;
			IsPlaying = false;
		}

		/// <summary>
		/// Starts the playback of an animation clip from the beginning.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		public void StartClip(string name)
		{
			var clip = Skeleton.GetClip(name);
			if (clip == null)
				throw new ArgumentException($"Clip '{name}' not found", nameof(name));

			AnimationClip = clip;

			HasFinished = false;
			IsPlaying = true;

			Skeleton.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Stops the playback
		/// </summary>
		public void StopClip()
		{
			AnimationClip = null;
			HasFinished = false;
			IsPlaying = false;

			Skeleton.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Plays an animation clip.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		public void PlayClip(string name)
		{
			var clip = Skeleton.GetClip(name);
			if (clip == null)
				throw new ArgumentException($"Clip '{name}' not found", nameof(name));

			AnimationClip = clip;

			if (_time < clip.Duration)
			{
				HasFinished = false;
				IsPlaying = true;
			}
		}

		/// <summary>
		/// Starts a blended animation with multiple clips.
		/// </summary>
		/// <param name="blendName">Name of the blend.</param>
		/// <param name="clipNames">Names of the animation clips to blend.</param>
		/// <param name="weights">Weights for each clip (optional, defaults to 1.0 for each).</param>
		public void StartBlend(string blendName, string[] clipNames, float[] weights = null)
		{
			if (clipNames == null || clipNames.Length == 0)
				throw new ArgumentException("Must provide at least one clip", nameof(clipNames));

			var blend = new AnimationBlend(blendName);

			for (int i = 0; i < clipNames.Length; i++)
			{
				var clip = Skeleton.GetClip(clipNames[i]);
				if (clip == null)
					throw new ArgumentException($"Clip '{clipNames[i]}' not found", nameof(clipNames));

				float weight = weights != null && i < weights.Length ? weights[i] : 1.0f;
				blend.AddClip(clip, weight);
			}

			AnimationClip = blend;
			HasFinished = false;
			IsPlaying = true;

			Skeleton.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Sets the weight of a clip in the current blend.
		/// </summary>
		/// <param name="clipIndex">Index of the clip in the blend.</param>
		/// <param name="weight">New weight value.</param>
		public void SetBlendWeight(int clipIndex, float weight)
		{
			if (AnimationClip == null)
				throw new InvalidOperationException("No animation source is currently active");

			var blend = AnimationClip as AnimationBlend;
			if (blend == null)
				throw new InvalidOperationException("Current animation source is not a blend");

			blend.SetClipWeight(clipIndex, weight);
		}

		/// <summary>
		/// Updates the animation clip time and calculates the new skeleton's bone pose.
		/// </summary>
		/// <param name="elapsedTime">Time elapsed since the last update.</param>
		public void Update(TimeSpan elapsedTime)
		{
			if (!IsPlaying || AnimationClip == null)
			{
				return;
			}

			if (HasFinished)
			{
				return;
			}

			// Scale the elapsed time
			TimeSpan scaledElapsedTime = TimeSpan.FromTicks((long)(elapsedTime.Ticks * _speed));
			if (PlaybackMode == PlaybackMode.Backward)
			{
				scaledElapsedTime = -scaledElapsedTime;
			}

			Time += scaledElapsedTime;
		}

		private void UpdateAll()
		{
			if (AnimationClip == null)
			{
				return;
			}

			// Check if animation has finished
			var duration = AnimationClip.Duration;
			var fDuration = (float)duration.TotalSeconds;

			if (!fDuration.IsZero() && _time > duration)
			{
				if (_loopEnabled)
				{
					while (_time > duration)
						_time -= duration;

					// Copy bind pose on animation restart
					Skeleton.ResetTransforms();
				}
				else
				{
					_time = duration;
					IsPlaying = false;
					HasFinished = true;
				}
			}

			// Let the animation source update all skeleton poses
			var loopedTime = AnimationInterpolationUtility.HandleLooping(_time, AnimationClip.Duration, LoopEnabled);
			var transforms = AnimationClip.GetTransforms(loopedTime);

			foreach (var pair in transforms)
			{
				Skeleton.SetPose(pair.Key, pair.Value);
			}
		}
	}
}
