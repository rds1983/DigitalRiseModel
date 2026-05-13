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
using System;
using System.Diagnostics;

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
		private IAnimationClip _crossfadeTarget;
		private TimeSpan _crossfadeDuration;
		private TimeSpan _crossfadeElapsedTime;
		private AnimationBlend _crossfadeBlend;
		private bool _crossfadeTargetLoop;

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
			}
		}

		/// <summary>
		/// Gets os sets the animation playback mode.
		/// </summary>
		public PlaybackMode PlaybackMode { get; set; }

		/// <summary>
		/// Returns whether the animation has finished.
		/// Determined by looping setting, current time, and animation duration.
		/// </summary>
		public bool HasFinished
		{
			get
			{
				if (_loopEnabled || AnimationClip == null)
					return false;

				return _time >= AnimationClip.Duration;
			}
		}

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
			IsPlaying = false;
		}

		/// <summary>
		/// Starts the playback of an animation clip from the beginning.
		/// </summary>
		/// <param name="clip">The animation clip to play.</param>
		/// <param name="loop">Whether the animation should loop.</param>
		public void StartClip(IAnimationClip clip, bool loop = true)
		{
			if (clip == null)
				throw new ArgumentNullException(nameof(clip));

			AnimationClip = clip;
			_loopEnabled = loop;

			IsPlaying = true;
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Stops the playback
		/// </summary>
		public void StopClip()
		{
			AnimationClip = null;
			IsPlaying = false;

			Skeleton.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Smoothly transitions from the current animation to another animation over a specified duration.
		/// </summary>
		/// <param name="clip">The target animation clip or blend.</param>
		/// <param name="duration">Duration of the crossfade transition.</param>
		/// <param name="loop">Whether the target animation should loop.</param>
		public void CrossfadeToClip(IAnimationClip clip, TimeSpan duration, bool loop = true)
		{
			if (clip == null)
				throw new ArgumentNullException(nameof(clip));

			if (AnimationClip == null)
			{
				StartClip(clip, loop);
				return;
			}

			_crossfadeTarget = clip;
			_crossfadeDuration = duration;
			_crossfadeElapsedTime = TimeSpan.Zero;
			_crossfadeTargetLoop = loop;

			_crossfadeBlend = new AnimationBlend("Crossfade");
			_crossfadeBlend.AddClip(AnimationClip, 1.0f, _loopEnabled);
			_crossfadeBlend.AddClip(_crossfadeTarget, 0.0f, loop);

			AnimationClip = _crossfadeBlend;

			IsPlaying = true;
			Time = TimeSpan.Zero;
		}


		/// <summary>
		/// Updates the animation clip time and calculates the new skeleton's bone pose.
		/// Handles crossfade transitions automatically.
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

			// Handle crossfade weight updates
			if (_crossfadeBlend != null && _crossfadeTarget != null)
			{
				_crossfadeElapsedTime += scaledElapsedTime;
				float crossfadeProgress = Math.Min(1.0f, (float)(_crossfadeElapsedTime.TotalSeconds / _crossfadeDuration.TotalSeconds));
				Debug.WriteLine($"{_crossfadeTarget.Name}:{_crossfadeElapsedTime}:{scaledElapsedTime}:{crossfadeProgress}");

				_crossfadeBlend.SetClipWeight(0, 1.0f - crossfadeProgress);
				_crossfadeBlend.SetClipWeight(1, crossfadeProgress);

				if (crossfadeProgress >= 1.0f)
				{
					AnimationClip = _crossfadeTarget;
					_loopEnabled = _crossfadeTargetLoop;
					_crossfadeTarget = null;
					_crossfadeBlend = null;
				}
			}
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
