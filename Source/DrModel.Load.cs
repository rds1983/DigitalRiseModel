using DigitalRiseModel.Animation;
using DigitalRiseModel.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRiseModel
{
	partial class DrModel
	{
		private struct SrtTransformOptional
		{
			public Vector3? Translation;
			public Vector3? Scale;
			public Quaternion? Rotation;
		}

		private class LoadContext
		{
			public GraphicsDevice Device { get; }
			public List<VertexBuffer> VertexBuffers { get; } = new List<VertexBuffer>();
			public IndexBuffer IndexBuffer;
			public int SkinIndex = 0;

			public LoadContext(GraphicsDevice device)
			{
				Device = device ?? throw new ArgumentNullException(nameof(device));
			}
		}

		private static void LoadBuffers(LoadContext context, ModelContent content)
		{
			for (var i = 0; i < content.VertexBuffers.Count; ++i)
			{
				var vertexBufferContent = content.VertexBuffers[i];
				var vertexElements = new List<VertexElement>();

				var offset = 0;
				foreach (var e in vertexBufferContent.Elements)
				{
					var vertexElement = new VertexElement(offset, e.Format, e.Usage, e.UsageIndex);
					vertexElements.Add(vertexElement);

					offset += e.Format.GetSize();
				}

				var vertexDeclaration = new VertexDeclaration(vertexElements.ToArray());

				var vertexBuffer = new VertexBuffer(context.Device, vertexDeclaration, vertexBufferContent.VertexCount, BufferUsage.None);
				vertexBuffer.SetData(vertexBufferContent.Data);

				context.VertexBuffers.Add(vertexBuffer);
			}

			if (content.IndexBuffer != null)
			{
				context.IndexBuffer = new IndexBuffer(context.Device, content.IndexBuffer.IndexType, content.IndexBuffer.IndexCount, BufferUsage.None);
				context.IndexBuffer.SetData(content.IndexBuffer.Data);
			}
		}

		private static DrModelBone LoadBone(LoadContext context, BoneContent bone)
		{
			var result = new DrModelBone(bone.Name)
			{
				DefaultPose = new SrtTransform(bone.Scale, bone.Rotation, bone.Translation)
			};

			if (bone.Mesh != null)
			{
				result.Mesh = new DrMesh();

				foreach (var submeshContent in bone.Mesh.Submeshes)
				{
					var submesh = new DrSubmesh(context.VertexBuffers[submeshContent.VertexBufferIndex], context.IndexBuffer, submeshContent.BoundingBox,
						submeshContent.PrimitiveType, submeshContent.VertexCount, submeshContent.PrimitiveCount)
					{
						StartVertex = submeshContent.StartVertex,
						StartIndex = submeshContent.StartIndex,
						MaterialId = submeshContent.MaterialIndex,
					};

					if (submeshContent.Skin != null)
					{
						var joints = new List<SkinJoint>();
						foreach (var skinJointContent in submeshContent.Skin.Data)
						{
							joints.Add(new SkinJoint(skinJointContent.BoneIndex, skinJointContent.InverseBindTransform));
						}

						submesh.Skin = new Skin(joints.ToArray())
						{
							SkinIndex = context.SkinIndex
						};

						++context.SkinIndex;
					}

					result.Mesh.Submeshes.Add(submesh);
				}
			}

			if (bone.Children != null)
			{
				var bones = new List<DrModelBone>();
				foreach (var child in bone.Children)
				{
					bones.Add(LoadBone(context, child));
				}

				result.Children = bones.ToArray();
			}

			return result;
		}


		private static void LoadAnimations(ModelContent content, DrModel model)
		{
			if (content.Animations == null)
			{
				return;
			}

			model.Animations = new Dictionary<string, AnimationClip>();
			foreach (var animationContent in content.Animations)
			{
				var channels = new List<AnimationChannel>();
				double time = 0;
				foreach (var channelContent in animationContent.Value.Channels)
				{
					var animationData = new SortedDictionary<double, SrtTransformOptional>();

					var bone = model.Bones[channelContent.BoneIndex];

					// First run: gather times and transforms
					if (channelContent.Translations != null)
					{
						for (var i = 0; i < channelContent.Translations.Data.Count; ++i)
						{
							var translation = channelContent.Translations.Data[i];

							SrtTransformOptional transform;
							animationData.TryGetValue(translation.Time, out transform);
							transform.Translation = translation.Value;
							animationData[translation.Time] = transform;
						}
					}

					if (channelContent.Scales != null)
					{
						for (var i = 0; i < channelContent.Scales.Data.Count; ++i)
						{
							var scale = channelContent.Scales.Data[i];

							SrtTransformOptional transform;
							animationData.TryGetValue(scale.Time, out transform);
							transform.Scale = scale.Value;
							animationData[scale.Time] = transform;
						}
					}

					if (channelContent.Rotations != null)
					{
						for (var i = 0; i < channelContent.Rotations.Data.Count; ++i)
						{
							var rotation = channelContent.Rotations.Data[i];

							SrtTransformOptional transform;
							animationData.TryGetValue(rotation.Time, out transform);
							transform.Rotation = rotation.Value;
							animationData[rotation.Time] = transform;
						}
					}

					// Second run: set key frames
					var keyframes = new List<AnimationChannelKeyframe>();

					var currentTransform = bone.DefaultPose;
					foreach (var pair2 in animationData)
					{
						var optionalTransform = pair2.Value;
						if (optionalTransform.Translation != null)
						{
							currentTransform.Translation = optionalTransform.Translation.Value;
						}

						if (optionalTransform.Scale != null)
						{
							currentTransform.Scale = optionalTransform.Scale.Value;
						}

						if (optionalTransform.Rotation != null)
						{
							currentTransform.Rotation = optionalTransform.Rotation.Value;
						}

						keyframes.Add(new AnimationChannelKeyframe(TimeSpan.FromMilliseconds(pair2.Key), currentTransform));

						if (pair2.Key > time)
						{
							time = pair2.Key;
						}
					}

					var animationChannel = new AnimationChannel(bone.Index, keyframes.ToArray())
					{
						TranslationMode = InterpolationMode.Linear,
						RotationMode = InterpolationMode.Linear,
						ScaleMode = InterpolationMode.Linear
					};

					channels.Add(animationChannel);
				}

				var animation = new AnimationClip(animationContent.Key, TimeSpan.FromMilliseconds(time), channels.ToArray());
				var id = animation.Name ?? "(default)";
				model.Animations[id] = animation;
			}
		}

		private static DrModel Load(GraphicsDevice device, ModelContent content)
		{
			var context = new LoadContext(device);

			LoadBuffers(context, content);

			var rootBoneDesc = LoadBone(context, content.RootBone);
			var result = new DrModel(rootBoneDesc);

			LoadAnimations(content, result);

			return result;
		}

		public static DrModel CreateFromJson(GraphicsDevice device, string json, Func<string, Stream> binaryOpener)
		{
			var content = ModelContent.LoadJsonFromString(json, binaryOpener);

			return Load(device, content);
		}

		public static DrModel CreateFromBinary(GraphicsDevice device, Stream stream)
		{
			ModelContent content;
			using (var reader = new BinaryReader(stream))
			{
				content = ModelContent.LoadBinary(reader);
			}

			return Load(device, content);
		}
	}
}
