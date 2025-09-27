using DigitalRiseModel.Animation;
using DigitalRiseModel.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRiseModel
{
	internal static class DrmLoader
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
			public DrModelBone[] Bones { get; set; }
			public DrMaterial[] Materials;
			public int SkinIndex = 0;

			public LoadContext(GraphicsDevice device)
			{
				Device = device ?? throw new ArgumentNullException(nameof(device));
			}
		}

		private static void LoadBuffers(LoadContext context, ModelContent modelContent)
		{
			for (var i = 0; i < modelContent.VertexBuffers.Count; ++i)
			{
				var vertexBufferContent = modelContent.VertexBuffers[i];
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

			if (modelContent.IndexBuffer != null)
			{
				context.IndexBuffer = new IndexBuffer(context.Device, modelContent.IndexBuffer.IndexType, modelContent.IndexBuffer.IndexCount, BufferUsage.None);
				context.IndexBuffer.SetData(modelContent.IndexBuffer.Data);
			}
		}

		private static void LoadMaterials(LoadContext context, ModelContent modelContent, Func<GraphicsDevice, string, Texture2D> textureLoader)
		{
			if (modelContent.Materials == null || textureLoader == null)
			{
				return;
			}

			var materials = new List<DrMaterial>();
			foreach (var materialContent in modelContent.Materials)
			{
				var material = new DrMaterial
				{
					Name = materialContent.Name,
					DiffuseColor = materialContent.DiffuseColor,
					SpecularColor = materialContent.SpecularColor,
					SpecularFactor = materialContent.SpecularFactor,
					SpecularPower = materialContent.SpecularPower
				};

				if (!string.IsNullOrEmpty(materialContent.DiffuseTexturePath))
				{
					material.DiffuseTexture = textureLoader(context.Device, materialContent.DiffuseTexturePath);
				}

				if (!string.IsNullOrEmpty(materialContent.NormalTexturePath))
				{
					material.NormalTexture = textureLoader(context.Device, materialContent.NormalTexturePath);
				}

				if (!string.IsNullOrEmpty(materialContent.SpecularTexturePath))
				{
					material.SpecularTexture = textureLoader(context.Device, materialContent.SpecularTexturePath);
				}

				materials.Add(material);
			}

			context.Materials = materials.ToArray();
		}

		private static void LoadBones(LoadContext context, ModelContent modelContent)
		{
			// First run: load all bones
			var bones = new List<DrModelBone>();
			for (var i = 0; i < modelContent.Bones.Count; ++i)
			{
				var boneContent = modelContent.Bones[i];
				DrModelBone bone;
				if (boneContent.Mesh != null)
				{
					var mesh = new DrMesh();
					foreach (var meshPartContent in boneContent.Mesh.MeshParts)
					{
						var part = new DrMeshPart(context.VertexBuffers[meshPartContent.VertexBufferIndex], context.IndexBuffer, meshPartContent.BoundingBox,
							 meshPartContent.PrimitiveType, meshPartContent.VertexCount, meshPartContent.PrimitiveCount, meshPartContent.StartVertex, meshPartContent.StartIndex);

						if (context.Materials != null)
						{
							part.Material = context.Materials[meshPartContent.MaterialIndex];
						}

						if (boneContent.Skin != null)
						{
						}

						mesh.MeshParts.Add(part);
					}

					bone = new DrModelBone(boneContent.Name, mesh);
				}
				else
				{
					bone = new DrModelBone(boneContent.Name);
				}

				bone.DefaultPose = new SrtTransform(boneContent.Translation, boneContent.Rotation, boneContent.Scale);

				bones.Add(bone);
			}

			context.Bones = bones.ToArray();

			// Second run: set children and skins
			for (var i = 0; i < modelContent.Bones.Count; ++i)
			{
				var bone = context.Bones[i];
				var boneContent = modelContent.Bones[i];

				if (boneContent.Children != null)
				{
					var children = new List<DrModelBone>();
					foreach (var child in boneContent.Children)
					{
						children.Add(context.Bones[child]);
					}

					bone.Children = children.ToArray();
				}

				if (boneContent.Skin != null)
				{
					var joints = new List<DrSkinJoint>();
					foreach (var skinJointContent in boneContent.Skin.Data)
					{
						joints.Add(new DrSkinJoint(context.Bones[skinJointContent.BoneIndex], skinJointContent.InverseBindTransform));
					}

					bone.Skin = new DrSkin(context.SkinIndex, joints.ToArray());

					++context.SkinIndex;
				}
			}
		}

		private static Dictionary<string, AnimationClip> LoadAnimations(LoadContext context, ModelContent modelContent)
		{
			if (modelContent.Animations == null)
			{
				return null;
			}

			var animations = new Dictionary<string, AnimationClip>();
			foreach (var animationContent in modelContent.Animations)
			{
				var channels = new List<AnimationChannel>();
				double time = 0;
				foreach (var channelContent in animationContent.Value.Channels)
				{
					var animationData = new SortedDictionary<double, SrtTransformOptional>();

					var bone = context.Bones[channelContent.BoneIndex];

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
				animations[id] = animation;
			}

			return animations;
		}

		private static DrModel Load(GraphicsDevice device, ModelContent modelContent, Func<GraphicsDevice, string, Texture2D> textureLoader)
		{
			var context = new LoadContext(device);

			LoadBuffers(context, modelContent);
			LoadMaterials(context, modelContent, textureLoader);
			LoadBones(context, modelContent);

			var result = new DrModel(context.Bones[modelContent.RootBoneIndex]);

			result.Animations = LoadAnimations(context, modelContent);

			return result;
		}

		public static DrModel CreateFromJson(GraphicsDevice device, string json,
			Func<string, Stream> binaryOpener, Func<GraphicsDevice, string, Texture2D> textureLoader)
		{
			var content = ModelContent.LoadJsonFromString(json, binaryOpener);

			return Load(device, content, textureLoader);
		}

		public static DrModel CreateFromBinary(GraphicsDevice device, Stream stream,
			Func<GraphicsDevice, string, Texture2D> textureLoader)
		{
			ModelContent content;
			using (var reader = new BinaryReader(stream))
			{
				content = ModelContent.LoadBinary(reader);
			}

			return Load(device, content, textureLoader);
		}
	}
}
