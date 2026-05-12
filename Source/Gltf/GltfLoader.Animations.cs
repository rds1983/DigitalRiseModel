using DigitalRiseModel.Animation;
using DigitalRiseModel.Utility;
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
			public TimeSpan Time;
			public Vector3? Translation;
			public Quaternion? Rotation;
			public Vector3? Scale;
		}

		private class AnimationBuilder
		{
			private List<AnimationRecord> Keyframes { get; } = new List<AnimationRecord>();

			public void AddPathInfo<T>(GltfLoader loader, AnimationSampler sampler, Action<AnimationRecord, T> poseSetter)
			{
				// Extract keyframe times and data from the glTF sampler
				var times = loader.GetAccessorAs<float>(sampler.Input);

				var data = loader.GetAccessorAs<T>(sampler.Output);
				if (times.Length != data.Length)
				{
					throw new NotSupportedException("Translation length is different from times length");
				}

				// Merge this animation path's data with existing keyframes, creating new keyframes where needed
				for (var i = 0; i < times.Length; ++i)
				{
					var time = TimeSpan.FromSeconds(times[i]);

					// Find the keyframe at or before this time, or null if this is the earliest time seen
					var frameIndex = Keyframes.GetKeyframeIndexByTime(r => r.Time, time);

					AnimationRecord record = null;
					if (frameIndex == null)
					{
						// No earlier keyframe exists; create a new one at the beginning
						record = new AnimationRecord
						{
							Time = time
						};
						Keyframes.Insert(0, record);
					}
					else
					{
						var frame = Keyframes[frameIndex.Value];
						var ftime = (float)frame.Time.TotalSeconds;

						if (times[i].EpsilonEquals(ftime))
						{
							// Keyframe exists at this exact time; reuse it
							record = frame;
						}
						else
						{
							// Keyframe exists before this time but not at it; create a new one after the nearest frame
							record = new AnimationRecord
							{
								Time = time
							};
							Keyframes.Insert(frameIndex.Value + 1, record);
						}
					}

					// Set this animation path's data (translation, rotation, or scale) on the record
					poseSetter(record, data[i]);
				}
			}

			public AnimationChannelKeyframe[] CreateKeyframes(SrtTransform currentPose)
			{
				// Convert merged AnimationRecords into final keyframes, accumulating pose state
				var result = new List<AnimationChannelKeyframe>();

				for (var i = 0; i < Keyframes.Count; ++i)
				{
					var frameSource = Keyframes[i];

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
					result.Add(new AnimationChannelKeyframe(frameSource.Time, currentPose));
				}

				return result.ToArray();
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
