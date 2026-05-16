using DigitalRiseModel.Animation;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using static glTFLoader.Schema.AnimationChannelTarget;
using AnimationChannel = DigitalRiseModel.Animation.AnimationChannel;

namespace DigitalRiseModel
{
	partial class GltfLoader
	{
		private struct PathInfo
		{
			public int Sampler;
			public PathEnum Path;

			public PathInfo(int sampler, PathEnum path)
			{
				Sampler = sampler;
				Path = path;
			}
		}

		private class AnimationRecord
		{
			/// <summary>
			/// Stores optional pose components for a single keyframe.
			/// Only components that are explicitly keyed in the glTF file are set; others remain null.
			/// Missing components are filled in during pose accumulation using previous values.
			/// </summary>
			public Vector3? Translation;
			public Quaternion? Rotation;
			public Vector3? Scale;
		}

		/// <summary>
		/// Merges multiple animation samplers (translation, rotation, scale) into unified keyframes.
		/// Uses SortedDictionary with float keys. Floating-point precision issues are resolved in CreateKeyframes
		/// by merging any keyframes whose times are EpsilonEquals before building the final result.
		/// </summary>
		private class AnimationBuilder
		{
			/// <summary>
			/// Epsilon tolerance for comparing floating-point keyframe times.
			/// Times within this tolerance are considered equal and will be merged in CreateKeyframes.
			/// </summary>
			private const float TimeEpsilon = 1e-6f;

			/// <summary>
			/// Keyframes sorted by time (in seconds). When multiple animation paths have keyframes at the same time,
			/// they are merged into a single AnimationRecord.
			/// Near-identical times due to floating-point precision are merged in CreateKeyframes.
			/// </summary>
			private SortedDictionary<float, AnimationRecord> Keyframes { get; } = new SortedDictionary<float, AnimationRecord>();

			/// <summary>
			/// Adds keyframe data from a single animation sampler (translation, rotation, or scale) to the merged keyframes.
			/// If a keyframe already exists at a time (from another sampler), the data is added to it.
			/// If not, a new keyframe is created and automatically inserted in sorted order.
			/// </summary>
			public void AddPathInfo<T>(GltfLoader loader, AnimationSampler sampler, Action<AnimationRecord, T> poseSetter)
			{
				// Extract keyframe times and data from the glTF sampler
				var times = loader.GetAccessorAs<float>(sampler.Input);

				var data = loader.GetAccessorAs<T>(sampler.Output);
				if (times.Length != data.Length)
				{
					throw new NotSupportedException("Translation length is different from times length");
				}

				// Merge this animation path's data with existing keyframes
				for (var i = 0; i < times.Length; ++i)
				{
					var time = times[i];

					// Get or create keyframe at this time
					if (!Keyframes.TryGetValue(time, out AnimationRecord record))
					{
						record = new AnimationRecord();
						Keyframes[time] = record;  // SortedDictionary auto-inserts in sorted order
					}

					// Set this animation path's data (translation, rotation, or scale) on the record
					poseSetter(record, data[i]);
				}
			}

			/// <summary>
			/// Converts merged AnimationRecords into final animation keyframes.
			/// First merges any keyframes with times that are EpsilonEquals to handle floating-point precision issues.
			/// Then accumulates pose state as we iterate through sorted keyframes, so each output keyframe has
			/// a complete pose (translation, rotation, scale) even if only some components were keyed at that time.
			/// </summary>
			public AnimationChannelKeyframe[] CreateKeyframes(SrtTransform currentPose)
			{
				// First pass: merge keyframes with EpsilonEquals times to handle floating-point precision issues
				var mergedKeyframes = MergeNearIdenticalTimes();

				var result = new List<AnimationChannelKeyframe>();

				// Second pass: accumulate pose and build final keyframes
				foreach (var pair in mergedKeyframes)
				{
					var frameSource = pair.Value;
					var time = pair.Key;

					// Update pose with any animation data present in this keyframe
					// (each animation path may have different keyframe times, so we only update the components that were keyed)
					if (frameSource.Translation != null)
					{
						currentPose.Translation = frameSource.Translation.Value;
					}

					if (frameSource.Rotation != null)
					{
						currentPose.Rotation = frameSource.Rotation.Value;
					}

					if (frameSource.Scale != null)
					{
						currentPose.Scale = frameSource.Scale.Value;
					}

					// Create a keyframe with the accumulated pose at this time
					// Unkeyed components carry forward from previous keyframes or use the default pose
					result.Add(new AnimationChannelKeyframe(TimeSpan.FromSeconds(time), currentPose));
				}

				return result.ToArray();
			}

			/// <summary>
			/// Merges keyframes whose times are within TimeEpsilon of each other.
			/// This handles floating-point precision issues where different samplers might have times like
			/// 0.333333f and 0.333334f that should be treated as the same keyframe time.
			/// </summary>
			private SortedDictionary<float, AnimationRecord> MergeNearIdenticalTimes()
			{
				if (Keyframes.Count == 0)
					return new SortedDictionary<float, AnimationRecord>();

				var merged = new SortedDictionary<float, AnimationRecord>();
				AnimationRecord currentMergedRecord = null;
				float currentTime = float.NegativeInfinity;

				foreach (var pair in Keyframes)
				{
					var time = pair.Key;
					var record = pair.Value;

					// Check if this time is close enough to the previous time to merge
					if (currentMergedRecord != null && Math.Abs(time - currentTime) < TimeEpsilon)
					{
						// Merge this record into the current merged record
						if (record.Translation.HasValue)
							currentMergedRecord.Translation = record.Translation;
						if (record.Rotation.HasValue)
							currentMergedRecord.Rotation = record.Rotation;
						if (record.Scale.HasValue)
							currentMergedRecord.Scale = record.Scale;
					}
					else
					{
						// Start a new merged entry
						currentTime = time;
						currentMergedRecord = new AnimationRecord
						{
							Translation = record.Translation,
							Rotation = record.Rotation,
							Scale = record.Scale
						};
						merged[time] = currentMergedRecord;
					}
				}

				return merged;
			}
		}

		private void LoadAnimations(DrModel model)
		{
			if (_gltf.Animations == null)
			{
				return;
			}

			model.Animations = new Dictionary<string, AnimationClip>();
			foreach (var gltfAnimation in _gltf.Animations)
			{
				// Group animation channels by target node (bone), collecting all animation paths (translation, rotation, scale) per bone
				var channelsDict = new Dictionary<int, List<PathInfo>>();
				foreach (var channel in gltfAnimation.Channels)
				{
					if (!channelsDict.TryGetValue(channel.Target.Node.Value, out List<PathInfo> targets))
					{
						targets = new List<PathInfo>();
						channelsDict[channel.Target.Node.Value] = targets;
					}

					targets.Add(new PathInfo(channel.Sampler, channel.Target.Path));
				}

				// Build animation channels for each bone
				var channels = new List<AnimationChannel>();
				var time = TimeSpan.Zero;
				foreach (var pair in channelsDict)
				{
					var bone = _allBones[pair.Key];

					// Merge all animation paths for this bone, handling cases where translation/rotation/scale have different keyframe times
					var animationData = new AnimationBuilder();

					var translationMode = InterpolationMode.None;
					var rotationMode = InterpolationMode.None;
					var scaleMode = InterpolationMode.None;
					foreach (var pathInfo in pair.Value)
					{
						var sampler = gltfAnimation.Samplers[pathInfo.Sampler];

						// Load each animation path (translation, rotation, scale) and track its interpolation mode
						switch (pathInfo.Path)
						{
							case PathEnum.translation:
								animationData.AddPathInfo<Vector3>(this, sampler, (r, d) => r.Translation = d);
								translationMode = sampler.Interpolation.ToInterpolationMode();
								break;
							case PathEnum.rotation:
								animationData.AddPathInfo<Quaternion>(this, sampler, (r, d) => r.Rotation = d);
								rotationMode = sampler.Interpolation.ToInterpolationMode();
								break;
							case PathEnum.scale:
								animationData.AddPathInfo<Vector3>(this, sampler, (r, d) => r.Scale = d);
								scaleMode = sampler.Interpolation.ToInterpolationMode();
								break;
							case PathEnum.weights:
								break;
						}
					}

					// Create the animation channel with the merged keyframes
					var animationChannel = new AnimationChannel(bone.Index, animationData.CreateKeyframes(bone.DefaultPose))
					{
						TranslationMode = translationMode,
						RotationMode = rotationMode,
						ScaleMode = scaleMode
					};

					channels.Add(animationChannel);

					// Track the animation's total duration from all bones
					foreach (var frame in animationChannel.Keyframes)
					{
						if (frame.Time > time)
						{
							time = frame.Time;
						}
					}
				}

				// Create and register the animation clip
				var id = gltfAnimation.Name ?? "(default)";
				var animation = new AnimationClip(id, time, channels.ToArray());
				model.Animations[id] = animation;
			}
		}
	}
}
