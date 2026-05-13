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
		private AnimationBlend _blendAnimation;
		private bool _isBlending;

		/// <summary>
		/// Model Node
		/// </summary>
		public ISkeleton Skeleton { get; }

		/// <summary>
		/// Gets the animation clip being played.
		/// </summary>
		public AnimationClip AnimationClip { get; private set; }

		/// <summary>
		/// Gets the animation blend being played.
		/// </summary>
		public AnimationBlend BlendAnimation => _blendAnimation;

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
				if (_speed < 0)
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

			HasFinished = false;
			IsPlaying = false;
		}

		/// <summary>
		/// Starts the playback of an animation clip from the beginning.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		public void StartClip(string name)
		{
			// Stop any active blend when starting a single clip
			if (_isBlending)
			{
				StopBlend();
			}

			AnimationClip = Skeleton.GetClip(name);
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
			_isBlending = false;
			_blendAnimation = null;

			Skeleton.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Plays an animation clip.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		public void PlayClip(string name)
		{
			AnimationClip = Skeleton.GetClip(name);

			if (_time < AnimationClip.Duration)
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

			_blendAnimation = blend;
			AnimationClip = blend.GetClip(0);
			_isBlending = true;
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
			if (_blendAnimation == null)
				throw new InvalidOperationException("No blend is currently active");

			_blendAnimation.SetClipWeight(clipIndex, weight);
		}

		/// <summary>
		/// Stops the blended animation playback.
		/// </summary>
		public void StopBlend()
		{
			_blendAnimation = null;
			_isBlending = false;
		}

		/// <summary>
		/// Updates the animation clip time and calculates the new skeleton's bone pose.
		/// </summary>
		/// <param name="elapsedTime">Time elapsed since the last update.</param>
		public void Update(TimeSpan elapsedTime)
		{
			if (!IsPlaying)
			{
				return;
			}

			if (HasFinished && !_isBlending)
			{
				return;
			}

			// Scale the elapsed time
			TimeSpan scaledElapsedTime = TimeSpan.FromTicks((long)(elapsedTime.Ticks * _speed));
			if (PlaybackMode == PlaybackMode.Backward)
			{
				scaledElapsedTime = -scaledElapsedTime;
			}

			// Update blend animation times
			if (_isBlending && _blendAnimation != null)
			{
				_blendAnimation.UpdateTime(scaledElapsedTime);
			}

			Time += scaledElapsedTime;
		}

		private void UpdateAll()
		{
			if (AnimationClip == null)
			{
				return;
			}

			UpdateAnimationTime();
			UpdateChannelPoses();
		}

		/// <summary>
		/// Updates the animation clip time.
		/// </summary>
		private void UpdateAnimationTime()
		{
			// Skip looping logic when blending - each clip handles its own looping
			if (_isBlending)
			{
				return;
			}

			if (AnimationClip == null)
			{
				return;
			}

			var fDuration = (float)AnimationClip.Duration.TotalSeconds;
			if (fDuration.IsZero())
			{
				return;
			}

			// Animation finished
			if (_time < TimeSpan.Zero || _time > AnimationClip.Duration)
			{
				if (_loopEnabled)
				{
					if (_time > AnimationClip.Duration)
					{
						while (_time > AnimationClip.Duration)
							_time -= AnimationClip.Duration;
					}
					else
					{
						while (_time < TimeSpan.Zero)
							_time += AnimationClip.Duration;
					}

					// Copy bind pose on animation restart
					Skeleton.ResetTransforms();
				}
				else
				{
					_time = (_time > AnimationClip.Duration) ? AnimationClip.Duration : TimeSpan.Zero;

					IsPlaying = false;
					HasFinished = true;
				}
			}
		}

		/// <summary>
		/// Updates the pose of all skeleton's bones.
		/// </summary>
		private void UpdateChannelPoses()
		{
			// Handle blended animations
			if (_isBlending && _blendAnimation != null)
			{
				UpdateBlendedPoses();
				return;
			}

			// Handle single animation clip
			for (int i = 0; i < AnimationClip.Channels.Length; i++)
			{
				var animationChannel = AnimationClip.Channels[i];

				SrtTransform pose;
				InterpolateChannelPose(animationChannel, _time, out pose);
				Skeleton.SetPose(animationChannel.BoneIndex, pose);
			}
		}

		/// <summary>
		/// Updates the pose of all skeleton's bones using blended animations.
		/// </summary>
		private void UpdateBlendedPoses()
		{
			// Collect all unique bone indices from all clips in the blend
			var boneIndices = new System.Collections.Generic.HashSet<int>();
			for (int i = 0; i < _blendAnimation.ClipCount; i++)
			{
				var clip = _blendAnimation.GetClip(i);
				foreach (var channel in clip.Channels)
				{
					boneIndices.Add(channel.BoneIndex);
				}
			}

			// Update poses for all bones affected by any clip in the blend
			foreach (var boneIndex in boneIndices)
			{
				SrtTransform blendedPose = Skeleton.GetDefaultPose(boneIndex);
				float totalWeight = 0;

				// Blend all clips that have a channel for this bone
				for (int i = 0; i < _blendAnimation.ClipCount; i++)
				{
					var clip = _blendAnimation.GetClip(i);
					var time = _blendAnimation.GetClipTime(i);
					var weight = _blendAnimation.GetClipWeight(i);

					// Handle looping for each clip independently
					var loopedTime = HandleClipLooping(clip, time);

					if (clip.TryGetChannelByBoneIndex(boneIndex, out var animationChannel))
					{
						SrtTransform pose;
						InterpolateChannelPose(animationChannel, loopedTime, out pose);

						if (totalWeight == 0)
						{
							blendedPose = pose;
						}
						else
						{
							// Blend with accumulated pose
							float normalizedWeight = weight / (totalWeight + weight);
							blendedPose = SrtTransform.Interpolate(blendedPose, pose, normalizedWeight,
								animationChannel.TranslationMode, animationChannel.RotationMode, animationChannel.ScaleMode);
						}

						totalWeight += weight;
					}
				}

				Skeleton.SetPose(boneIndex, blendedPose);
			}
		}

		/// <summary>
		/// Handles looping for a single animation clip time.
		/// </summary>
		private TimeSpan HandleClipLooping(AnimationClip clip, TimeSpan time)
		{
			var duration = clip.Duration;
			var fDuration = (float)duration.TotalSeconds;

			if (fDuration.IsZero())
			{
				return time;
			}

			// If time is within bounds, return as-is
			if (time >= TimeSpan.Zero && time <= duration)
			{
				return time;
			}

			// Handle looping
			if (_loopEnabled)
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
				// Clamp to duration if not looping
				return time > duration ? duration : TimeSpan.Zero;
			}
		}

		/// <summary>
		/// Retrieves and interpolates the pose of an animation channel.
		/// </summary>
		/// <param name="animationChannel">Name of the animation channel.</param>
		/// <param name="animationTime">Current animation clip time.</param>
		/// <param name="outPose">The output interpolated pose.</param>
		private void InterpolateChannelPose(AnimationChannel animationChannel, TimeSpan animationTime,
			out SrtTransform outPose)
		{
			if (animationChannel.TranslationMode == InterpolationMode.None &&
				animationChannel.RotationMode == InterpolationMode.None &&
				animationChannel.ScaleMode == InterpolationMode.None)
			{
				CurrentKeyFrame = animationChannel.GetKeyframeIndexByTime(animationTime);
				outPose = animationChannel.Keyframes[CurrentKeyFrame].Pose;
			}
			else
			{
				CurrentKeyFrame = animationChannel.GetKeyframeIndexByTime(animationTime);
				int nextKeyframeIndex;

				// If we are looping then the next frame may wrap around to 
				// the beginning. If not we should just clamp it at the last frame
				if (_loopEnabled)
				{
					nextKeyframeIndex = (CurrentKeyFrame + 1) % animationChannel.Keyframes.Length;
				}
				else
				{
					nextKeyframeIndex = Math.Min(CurrentKeyFrame + 1, animationChannel.Keyframes.Length - 1);
				}

				var keyframe1 = animationChannel.Keyframes[CurrentKeyFrame];
				var keyframe2 = animationChannel.Keyframes[nextKeyframeIndex];

				// Calculate the time between the keyframes considering loop
				long keyframeDuration;
				if (CurrentKeyFrame == (animationChannel.Keyframes.Length - 1))
					keyframeDuration = AnimationClip.Duration.Ticks - keyframe1.Time.Ticks;
				else
					keyframeDuration = keyframe2.Time.Ticks - keyframe1.Time.Ticks;

				// Interpolate when duration higher than zero
				if (keyframeDuration > 0)
				{
					long elapsedKeyframeTime = animationTime.Ticks - keyframe1.Time.Ticks;
					float lerpFactor = MathHelper.Clamp(elapsedKeyframeTime / (float)keyframeDuration, 0, 1);

					outPose = SrtTransform.Interpolate(keyframe1.Pose, keyframe2.Pose, lerpFactor,
								animationChannel.TranslationMode, animationChannel.RotationMode, animationChannel.ScaleMode);
				}
				// Otherwise don't interpolate
				else
				{
					outPose = keyframe1.Pose;
				}
			}
		}
	}
}
