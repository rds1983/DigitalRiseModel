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
	/// Main animation controller for managing skeletal animations with support for
	/// hierarchical blending, clip management, and playback control.
	/// </summary>
	public class AnimationController : IDisposable
	{
		private readonly ISkeleton _skeleton;
		private AnimationTreeNode _rootNode;
		private AnimationClipNode _currentClipNode;
		private BlendNode _transitionBlend;
		private TimeSpan _currentTime;
		private float _speed = 1.0f;
		private PlaybackMode _playbackMode = PlaybackMode.Forward;
		private bool _isPlaying;
		private bool _disposed;

		/// <summary>
		/// Raised when the playback time changes.
		/// </summary>
		public event EventHandler TimeChanged;

		/// <summary>
		/// Gets or sets the current playback time.
		/// </summary>
		public TimeSpan Time
		{
			get => _currentTime;
			set
			{
				_currentTime = value;
				OnTimeChanged();
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
		/// Gets the root animation tree node.
		/// </summary>
		public AnimationTreeNode RootNode => _rootNode;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationController"/> class.
		/// </summary>
		/// <param name="skeleton">The skeleton to animate.</param>
		/// <exception cref="ArgumentNullException"><paramref name="skeleton"/> is null.</exception>
		public AnimationController(ISkeleton skeleton)
		{
			_skeleton = skeleton ?? throw new ArgumentNullException(nameof(skeleton));
			_currentTime = TimeSpan.Zero;
			_isPlaying = false;
		}

		/// <summary>
		/// Sets the root animation tree node.
		/// </summary>
		/// <param name="node">The root animation node.</param>
		public void SetAnimationTree(AnimationTreeNode node)
		{
			_rootNode = node;
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

			_currentClipNode = new AnimationClipNode(clip, isLooped: isLooped);
			_rootNode = _currentClipNode;
			_currentTime = TimeSpan.Zero;
			_isPlaying = true;

			OnTimeChanged();
		}

		/// <summary>
		/// Stops the currently playing animation clip.
		/// </summary>
		public void StopClip()
		{
			_currentClipNode = null;
			_rootNode = null;
			_currentTime = TimeSpan.Zero;
			_isPlaying = false;
			_skeleton.ResetTransforms();

			OnTimeChanged();
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

			if (fadeDuration.TotalSeconds < 0)
				throw new ArgumentException("Fade duration cannot be negative.", nameof(fadeDuration));

			var newClipNode = new AnimationClipNode(clip, isLooped: isLooped);

			if (_currentClipNode == null)
			{
				// No current clip, just start the new one
				StartClip(clip, isLooped);
				return;
			}

			// Create transition blend
			_transitionBlend = new BlendNode(isLooped: isLooped);
			_transitionBlend.AddChild(_currentClipNode, weight: 1.0f);
			_transitionBlend.AddChild(newClipNode, weight: 0.0f);

			_rootNode = _transitionBlend;
			_currentClipNode = newClipNode;

			// Animate the blend over the fade duration
			if (fadeDuration.TotalSeconds > 0)
			{
				var transitionTime = TimeSpan.Zero;
				var transitionUpdate = new Action<TimeSpan>(deltaTime =>
				{
					transitionTime += deltaTime;
					float progress = Math.Min(1.0f, (float)(transitionTime.TotalSeconds / fadeDuration.TotalSeconds));

					_transitionBlend.SetChildWeight(_currentClipNode, progress);
					_transitionBlend.SetChildWeight(_currentClipNode, 1.0f - progress);
				});

				// For now, we'll complete the transition immediately in the next Update call
				// A more sophisticated system would use a separate animation for the transition
			}
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
			_currentTime = TimeSpan.Zero;

			OnTimeChanged();
		}

		/// <summary>
		/// Resets the skeleton to its default pose.
		/// </summary>
		public void ResetPose()
		{
			_skeleton.ResetTransforms();
		}

		/// <summary>
		/// Updates the animation for the specified elapsed time and samples the skeleton.
		/// </summary>
		/// <param name="elapsedTime">The elapsed time since the last update.</param>
		public void Update(TimeSpan elapsedTime)
		{
			ThrowIfDisposed();

			if (!_isPlaying || _rootNode == null)
				return;

			float deltaSeconds = (float)elapsedTime.TotalSeconds * _speed;

			if (_playbackMode == PlaybackMode.Backward)
				deltaSeconds = -deltaSeconds;

			_currentTime += TimeSpan.FromSeconds(deltaSeconds);

			_currentTime = _currentTime.GetEffectiveTime(_currentClipNode.Duration, _currentClipNode.IsLooped);

			// Clamp time for non-looping animations
			if (_currentClipNode != null && !_currentClipNode.IsLooped)
			{
				if (_currentTime.TotalSeconds < 0)
					_currentTime = TimeSpan.Zero;
				else if (_currentTime > _currentClipNode.Duration)
					_currentTime = _currentClipNode.Duration;
			}

			Sample();
			OnTimeChanged();
		}

		/// <summary>
		/// Samples the animation tree and applies the resulting pose to the skeleton.
		/// Uses weight 1.0 for the root node.
		/// </summary>
		public void Sample()
		{
			ThrowIfDisposed();

			if (_rootNode == null)
				return;

			_skeleton.ResetTransforms();
			_rootNode.Sample(_skeleton, _currentTime, 1.0f);
		}

		/// <summary>
		/// Disposes the controller.
		/// </summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
			}
		}

		private void OnTimeChanged()
		{
			TimeChanged?.Invoke(this, EventArgs.Empty);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}
	}
}
