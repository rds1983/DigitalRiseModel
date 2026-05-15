using DigitalRiseModel.Utility;
using System;

namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Specifies how animations are played back.
	/// </summary>
	public enum PlaybackMode
	{
		/// <summary>
		/// Play animation forward.
		/// </summary>
		Forward = 0,

		/// <summary>
		/// Play animation backward.
		/// </summary>
		Backward = 1
	}

	/// <summary>
	/// Manages skeletal animation playback with hierarchical blending and crossfading.
	/// Uses a recursive "Process" pipeline: tree → AnimationContext → blended poses → skeleton.
	/// </summary>
	public class AnimationController
	{
		private readonly AnimationContext _context;
		private AnimationTreeNode _rootNode;
		private AnimationTreeNode _currentClipNode;
		private AnimationBlendNode _transitionBlend;
		private AnimationTreeNode _transitionOldClip;
		private TimeSpan _currentTime;
		private TimeSpan _transitionDuration;
		private float _speed = 1.0f;
		private PlaybackMode _playbackMode = PlaybackMode.Forward;
		private bool _isPlaying;

		/// <summary>
		/// Raised when the playback time changes.
		/// </summary>
		public event EventHandler TimeChanged;

		/// <summary>
		/// Gets or sets the current playback time.
		/// When set, updates the skeleton pose and raises the <see cref="TimeChanged"/> event.
		/// </summary>
		public TimeSpan Time
		{
			get => _currentTime;
			set
			{
				if (_currentTime.EpsilonEquals(value))
				{
					return;
				}

				_currentTime = value;

				OnTimeChanged();

				TimeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets or sets the playback speed multiplier (1.0 = normal speed).
		/// </summary>
		public float Speed
		{
			get => _speed;
			set => _speed = Math.Max(0, value);
		}

		/// <summary>
		/// Gets or sets the playback mode (forward or backward).
		/// </summary>
		public PlaybackMode PlaybackMode
		{
			get => _playbackMode;
			set => _playbackMode = value;
		}

		/// <summary>
		/// Gets whether the controller is currently playing.
		/// </summary>
		public bool IsPlaying => _isPlaying;

		/// <summary>
		/// Gets whether the current animation has finished playing.
		/// Returns false if the animation is looped or if no animation is playing.
		/// </summary>
		public bool HasFinished
		{
			get
			{
				if (_rootNode == null || _rootNode.IsLooped)
					return false;

				return _currentTime >= _rootNode.Duration;
			}
		}

		/// <summary>
		/// Gets the root animation tree node.
		/// </summary>
		public AnimationTreeNode RootNode => _rootNode;

		private ISkeleton Skeleton => _context.Skeleton;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationController"/> class.
		/// </summary>
		/// <param name="skeleton">The skeleton to animate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="skeleton"/> is null.</exception>
		public AnimationController(ISkeleton skeleton)
		{
			_context = new AnimationContext(skeleton);
			_isPlaying = false;
		}

		/// <summary>
		/// Starts playing an animation tree node directly.
		/// </summary>
		/// <param name="node">The animation tree node to play.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
		public void StartClip(AnimationTreeNode node)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			_rootNode = node;
			_currentClipNode = node;
			_isPlaying = true;
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Starts playing a single animation clip.
		/// </summary>
		/// <param name="clip">The animation clip to play.</param>
		/// <param name="isLooped">Whether the clip should loop when it reaches the end.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		public void StartClip(AnimationClip clip, bool isLooped = false)
		{
			if (clip == null)
				throw new ArgumentNullException(nameof(clip));

			StartClip(new AnimationClipNode(clip, isLooped: isLooped));
		}

		/// <summary>
		/// Starts playing an animation clip by name.
		/// </summary>
		/// <param name="name">The name of the animation clip to play.</param>
		/// <param name="isLooped">Whether the clip should loop when it reaches the end.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
		/// <exception cref="ArgumentException">No clip found with the specified name.</exception>
		public void StartClip(string name, bool isLooped = false)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			var clip = Skeleton.GetClip(name);
			if (clip == null)
				throw new ArgumentException($"Animation clip '{name}' not found.", nameof(name));

			StartClip(clip, isLooped);
		}

		/// <summary>
		/// Stops the currently playing animation clip.
		/// </summary>
		public void StopClip()
		{
			_currentClipNode = null;
			_rootNode = null;
			_isPlaying = false;
			Skeleton.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Smoothly transitions from the current animation to a new animation tree node using crossfading.
		/// </summary>
		/// <param name="node">The animation tree node to transition to.</param>
		/// <param name="fadeDuration">The duration of the crossfade transition.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="fadeDuration"/> is negative.</exception>
		public void CrossfadeToClip(AnimationTreeNode node, TimeSpan fadeDuration)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			if (fadeDuration.TotalSeconds < 0)
				throw new ArgumentException("Fade duration cannot be negative.", nameof(fadeDuration));

			if (_currentClipNode == null)
			{
				// No current clip, just start the new one
				StartClip(node);
				return;
			}

			// Create transition blend
			_transitionBlend = new AnimationBlendNode("Crossfade", isLooped: node.IsLooped);
			_transitionOldClip = _currentClipNode;
			_transitionBlend.AddLayer(_transitionOldClip, weight: 1.0f).TimeOffset = Time;
			_transitionBlend.AddLayer(node, weight: 0.0f);

			_rootNode = _transitionBlend;
			_currentClipNode = node;
			_transitionDuration = fadeDuration;

			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Smoothly transitions from the current clip to a new clip using crossfading.
		/// </summary>
		/// <param name="clip">The animation clip to transition to.</param>
		/// <param name="fadeDuration">The duration of the crossfade transition.</param>
		/// <param name="isLooped">Whether the new clip should loop when it reaches the end.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clip"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="fadeDuration"/> is negative.</exception>
		public void CrossfadeToClip(AnimationClip clip, TimeSpan fadeDuration, bool isLooped = false)
		{
			if (clip == null)
				throw new ArgumentNullException(nameof(clip));

			var newClipNode = new AnimationClipNode(clip, isLooped: isLooped);
			CrossfadeToClip(newClipNode, fadeDuration);
		}

		/// <summary>
		/// Smoothly transitions to an animation clip by name using crossfading.
		/// </summary>
		/// <param name="clipName">The name of the animation clip to transition to.</param>
		/// <param name="fadeDuration">The duration of the crossfade transition.</param>
		/// <param name="isLooped">Whether the new clip should loop when it reaches the end.</param>
		/// <exception cref="ArgumentNullException"><paramref name="clipName"/> is null.</exception>
		/// <exception cref="ArgumentException">No clip found with the specified name, or fade duration is negative.</exception>
		public void CrossfadeToClip(string clipName, TimeSpan fadeDuration, bool isLooped = false)
		{
			if (clipName == null)
				throw new ArgumentNullException(nameof(clipName));

			var clip = Skeleton.GetClip(clipName);
			if (clip == null)
				throw new ArgumentException($"Animation clip '{clipName}' not found.", nameof(clipName));

			CrossfadeToClip(clip, fadeDuration, isLooped);
		}

		/// <summary>
		/// Plays the animation.
		/// </summary>
		public void Play()
		{
			_isPlaying = true;
		}

		/// <summary>
		/// Pauses the animation.
		/// </summary>
		public void Pause()
		{
			_isPlaying = false;
		}

		/// <summary>
		/// Stops the animation and resets playback time to zero.
		/// </summary>
		public void Stop()
		{
			_isPlaying = false;
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Resets the skeleton to its default pose.
		/// </summary>
		public void ResetPose()
		{
			Skeleton.ResetTransforms();
		}

		private void OnTimeChanged()
		{
			if (!_isPlaying || _rootNode == null)
			{
				return;
			}

			// Update crossfade: fade out old clip, fade in new clip
			if (_transitionBlend != null)
			{
				float progress = Math.Min(1.0f, (float)(_currentTime.TotalSeconds / _transitionDuration.TotalSeconds));

				// Adjust layer weights: outgoing clip fades out, incoming clip fades in
				_transitionBlend.Layers[0].Weight = 1.0f - progress;
				_transitionBlend.Layers[1].Weight = progress;

				// Complete transition when done
				if (progress >= 1.0f)
				{
					_rootNode = _currentClipNode;
					_transitionBlend = null;
					_transitionOldClip = null;
					_transitionDuration = TimeSpan.Zero;
				}
			}

			// Reset skeleton to default pose
			Skeleton.ResetTransforms();

			// Evaluate animation tree and apply poses
			_context.Reset();
			_rootNode.Process(_context, _currentTime, 1.0f);

			// Apply accumulated transforms to skeleton
			_context.SetPoses();
		}

		/// <summary>
		/// Advances animation time and updates the skeleton pose.
		/// Call once per frame. Automatically handles crossfading and pose evaluation via the Time property.
		/// </summary>
		/// <param name="elapsedTime">Elapsed time since last update.</param>
		public void Update(TimeSpan elapsedTime)
		{
			// Advance time with speed and direction
			float deltaSeconds = (float)elapsedTime.TotalSeconds * _speed;
			if (_playbackMode == PlaybackMode.Backward)
				deltaSeconds = -deltaSeconds;

			Time += TimeSpan.FromSeconds(deltaSeconds);
		}
	}
}
