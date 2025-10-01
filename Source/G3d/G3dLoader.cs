using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using DigitalRiseModel;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel.G3d
{
	internal static class G3dLoader
	{
		private class LoadContext
		{
			public GraphicsDevice GraphicsDevice { get; }
			public JObject Root { get; }
			public Dictionary<string, NrmMeshPart> Meshes { get; } = new Dictionary<string, NrmMeshPart>();
			public Dictionary<string, NrmMaterial> Materials { get; } = new Dictionary<string, NrmMaterial>();

			public LoadContext(GraphicsDevice graphicsDevice, JObject root)
			{
				GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
				Root = root ?? throw new ArgumentNullException(nameof(root));
			}
		}

		private class DataArray
		{
			private readonly JArray _array;
			private int _index = 0;

			public DataArray(JArray arr)
			{
				_array = arr ?? throw new ArgumentNullException(nameof(arr));
			}

			public float GetFloat()
			{
				var result = (float)_array[_index];
				++_index;

				return result;
			}

			public Vector3 GetVector3() => new Vector3(GetFloat(), GetFloat(), GetFloat());
			public Vector4 GetVector4() => new Vector4(GetFloat(), GetFloat(), GetFloat(), GetFloat());

			public int GetInt()
			{
				var result = (int)_array[_index];
				++_index;

				return result;
			}
		}

		private static SrtTransform LoadTransform(JObject data)
		{
			var result = SrtTransform.Identity;
			JToken token;
			if (data.TryGetValue("scale", out token))
			{
				result.Scale = token.ToVector3();
			}

			var translation = Vector3.Zero;
			if (data.TryGetValue("translation", out token))
			{
				result.Translation = token.ToVector3();
			}

			var rotation = Vector4.Zero;
			if (data.TryGetValue("rotation", out token))
			{
				result.Rotation = token.ToQuaternion();
			}

			return result;
		}

		private static void LoadColor(byte[] dest, ref int destIdx, Color data)
		{
			var byteData = BitConverter.GetBytes(data.PackedValue);
			Array.Copy(byteData, 0, dest, destIdx, byteData.Length);
			destIdx += byteData.Length;
		}


		private static float LoadFloat(byte[] dest, ref int destIdx, float data)
		{
			var byteData = BitConverter.GetBytes(data);

			Array.Copy(byteData, 0, dest, destIdx, byteData.Length);
			destIdx += byteData.Length;

			return data;
		}

		private static byte LoadByte(byte[] dest, ref int destIdx, int data)
		{
			if (data > byte.MaxValue)
			{
				throw new Exception(string.Format("Only byte NrmModelBone indices suported. {0}", data));
			}

			var b = (byte)data;
			dest[destIdx] = b;
			++destIdx;

			return b;
		}

		private static VertexBuffer LoadVertexBuffer(GraphicsDevice graphicsDevice, G3dAttribute[] attributes, JArray data)
		{
			var dataArray = new DataArray(data);
			var elementsPerRow = attributes.CalculateElementsPerRow();
			var rowsCount = data.Count / elementsPerRow;

			var declaration = attributes.BuildDeclaration();
			var blendWeightOffset = 0;
			var hasBlendWeight = false;

			var elements = declaration.GetVertexElements();
			for (var i = 0; i < declaration.GetVertexElements().Length; ++i)
			{
				var el = elements[i];
				if (el.VertexElementUsage == VertexElementUsage.BlendIndices)
				{
					blendWeightOffset = el.Offset;
					hasBlendWeight = true;
					break;
				}
			}

			var byteData = new byte[rowsCount * declaration.VertexStride];

			VertexBufferBoundingBoxData vertexInfo = new VertexBufferBoundingBoxData(rowsCount, hasBlendWeight);
			for (var i = 0; i < rowsCount; ++i)
			{
				var destIdx = i * declaration.VertexStride;
				var weightsCount = 0;
				for (var j = 0; j < attributes.Length; ++j)
				{
					var attribute = attributes[j];

					var usage = attribute.GetUsage();
					if (usage == VertexElementUsage.BlendWeight)
					{
						// Convert from libgdx multiple vector2 blendweight
						// to single int4 blendindices/vector4 blendweight
						var offset = i * declaration.VertexStride + blendWeightOffset + weightsCount;
						var boneIndex = LoadByte(byteData, ref offset, (int)dataArray.GetFloat());
						vertexInfo.SetBoneIndex(i, weightsCount, boneIndex);

						offset = i * declaration.VertexStride + blendWeightOffset + 4 + weightsCount * 4;
						var boneWeight = LoadFloat(byteData, ref offset, dataArray.GetFloat());
						vertexInfo.SetBoneWeight(i, weightsCount, boneWeight);
						++weightsCount;
						continue;
					}

					var format = attribute.GetFormat();
					switch (format)
					{
						case VertexElementFormat.Vector2:
							LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							break;
						case VertexElementFormat.Vector3:
							{
								var v = dataArray.GetVector3();

								if (usage == VertexElementUsage.Position)
								{
									vertexInfo.SetPosition(i, v);
								}

								LoadFloat(byteData, ref destIdx, v.X);
								LoadFloat(byteData, ref destIdx, v.Y);
								LoadFloat(byteData, ref destIdx, v.Z);
							}
							break;
						case VertexElementFormat.Vector4:
							LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							break;
						case VertexElementFormat.Byte4:
							LoadByte(byteData, ref destIdx, dataArray.GetInt());
							LoadByte(byteData, ref destIdx, dataArray.GetInt());
							LoadByte(byteData, ref destIdx, dataArray.GetInt());
							LoadByte(byteData, ref destIdx, dataArray.GetInt());
							break;
						case VertexElementFormat.Color:
							var elementsCount = attribute.GetElementsCount();
							if (elementsCount == 1)
							{
								// Color packed in one float value
								LoadFloat(byteData, ref destIdx, dataArray.GetFloat());
							} else
							{
								// Color is presented as 4 float values
								var v = dataArray.GetVector4();
								var c = new Color(v);

								LoadColor(byteData, ref destIdx, c);
							}

							break;
						default:
							throw new Exception($"{format} not supported");
					}
				}
			}

			var result = new VertexBuffer(graphicsDevice, declaration, rowsCount, BufferUsage.None);
			result.SetData(byteData);

			result.Tag = vertexInfo;

			return result;
		}

		private static void LoadMeshData(LoadContext context)
		{
			var meshesData = context.Root["meshes"];
			foreach (JObject meshData in meshesData)
			{
				var attributes = (from d in (JArray)meshData["attributes"] select G3dUtility.FromName(d.ToString())).ToArray();
				var vertices = (JArray)meshData["vertices"];

				var vb = LoadVertexBuffer(context.GraphicsDevice, attributes, vertices);
				var vl = (VertexBufferBoundingBoxData)vb.Tag;
				var partsData = (JArray)meshData["parts"];
				foreach (JObject partData in partsData)
				{
					var id = partData.GetId();

					// var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), partData.EnsureString("type"));
					var indicesData = (JArray)partData["indices"];
					var indices = new ushort[indicesData.Count];

					// IntIndices are required to calculate proper bounding box for the mesh part
					var uintIndices = new uint[indicesData.Count];
					for (var i = 0; i < indicesData.Count; ++i)
					{
						var idx = Convert.ToUInt16(indicesData[i]);
						indices[i] = idx;
						uintIndices[i] = idx;
					}

					indices.Unwind();

					var indexBuffer = new IndexBuffer(context.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
					indexBuffer.SetData(indices);
					indexBuffer.Tag = uintIndices;

					// Use empty bounding box for now
					// It'll be recalculated in the end
					var part = new NrmMeshPart(vb, indexBuffer, new BoundingBox());
					context.Meshes[id] = part;
				}
			}
		}

		private static void LoadMaterials(LoadContext context, Func<string, Texture2D> textureGetter)
		{
			var materialsData = (JArray)context.Root["materials"];
			foreach (JObject materialData in materialsData)
			{
				var material = new NrmMaterial
				{
					Name = materialData.GetId(),
					DiffuseColor = Color.White,
					SpecularFactor = 0.0f,
					SpecularPower = 250.0f
				};

				JToken obj;
				if (materialData.TryGetValue("diffuse", out obj) && obj != null)
				{
					material.DiffuseColor = new Color(obj.ToVector4(1.0f));
				}

				var texturesData = (JArray)materialData["textures"];
				if (texturesData != null)
				{
					var name = texturesData[0]["filename"].ToString();
					if (!string.IsNullOrEmpty(name))
					{
						material.DiffuseTexture = textureGetter(name);
					}
				}

				context.Materials[material.Name] = material;
			}
		}

		private static NrmModelBone LoadNode(LoadContext context, JObject data)
		{
			NrmModelBone result;
			if (data.ContainsKey("parts"))
			{
				// Mesh
				var mesh = new NrmMesh();

				var partsData = (JArray)data["parts"];
				foreach (JObject partData in partsData)
				{
					var meshPart = context.Meshes[partData["meshpartid"].ToString()].Clone();
					meshPart.Material = context.Materials[partData["materialid"].ToString()].Clone();

					mesh.MeshParts.Add(meshPart);

					if (partData.ContainsKey("bones"))
					{
						var jointsData = (JArray)partData["bones"];

						var jointsDict = new Dictionary<string, SrtTransform>();
						foreach (JObject jointData in jointsData)
						{
							jointsDict[jointData.EnsureString("node")] = LoadTransform(jointData);
						}

						meshPart.Tag = jointsDict;
					}
				}

				result = new NrmModelBone(data.GetId(), mesh);
			}
			else
			{
				result = new NrmModelBone(data.GetId());
			}

			result.DefaultPose = LoadTransform(data);

			var childNodes = new List<NrmModelBone>();
			if (data.ContainsKey("children"))
			{
				var children = (JArray)data["children"];
				foreach (JObject child in children)
				{
					var childNode = LoadNode(context, child);
					childNodes.Add(childNode);
				}
			}

			result.Children = childNodes.ToArray();

			return result;
		}

		private static NrmModelBone LoadRootNode(LoadContext context)
		{
			// Load roots
			var nodesData = (JArray)context.Root["nodes"];

			var roots = new List<NrmModelBone>();
			foreach (JObject data in nodesData)
			{
				var root = LoadNode(context, data);
				roots.Add(root);
			}

			var oneRoot = roots.FixRoot(roots[0]);

			// Set skins
			return oneRoot;
		}

		private static void ProcessSkins(NrmModel model)
		{
			var skinIndex = 0;
			foreach (var mesh in model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					if (part.Tag == null)
					{
						continue;
					}

					var jointsDict = (Dictionary<string, SrtTransform>)part.Tag;
					var joints = new List<NrmSkinJoint>();
					foreach (var pair in jointsDict)
					{
						var joint = new NrmSkinJoint(model.FindBoneByName(pair.Key), Matrix.Invert(pair.Value.ToMatrix()));
						joints.Add(joint);
					}

					var skin = new NrmSkin(skinIndex, joints.ToArray());
					part.Skin = skin;

					++skinIndex;
				}
			}
		}

		private static void LoadAnimations(LoadContext context, NrmModel model)
		{
			if (!context.Root.ContainsKey("animations"))
			{
				return;
			}

			var animationsData = (JArray)context.Root["animations"];
			foreach (JObject animationData in animationsData)
			{
				var bonesData = (JArray)animationData["bones"];

				TimeSpan maxTime = TimeSpan.Zero;
				var channels = new List<AnimationChannel>();
				foreach (JObject boneData in bonesData)
				{
					var boneId = boneData["boneId"].ToString();
					var bone = model.FindBoneByName(boneId);
					if (bone == null)
					{
						throw new Exception(string.Format("Could not find bone '{0}'.", boneId));
					}

					var pose = bone.DefaultPose;

					var keyframes = new List<AnimationChannelKeyframe>();
					var keyframesData = (JArray)boneData["keyframes"];

					foreach (JObject keyframeData in keyframesData)
					{
						JToken token;
						if (keyframeData.TryGetValue("scale", out token))
						{
							pose.Scale = token.ToVector3();
						}

						if (keyframeData.TryGetValue("translation", out token))
						{
							pose.Translation = token.ToVector3();
						}

						if (keyframeData.TryGetValue("rotation", out token))
						{
							pose.Rotation = token.ToQuaternion();
						}

						var time = TimeSpan.FromMilliseconds(keyframeData["keytime"].ToFloat());

						var keyframe = new AnimationChannelKeyframe(time, pose);
						keyframes.Add(keyframe);

						if (time > maxTime)
						{
							maxTime = time;
						}
					}

					var channel = new AnimationChannel(bone.Index, keyframes.ToArray());
					channels.Add(channel);
				}

				if (channels.Count == 0)
				{
					continue;
				}

				var clip = new AnimationClip(animationData.GetId(), maxTime, channels.ToArray());

				if (model.Animations == null)
				{
					model.Animations = new Dictionary<string, AnimationClip>();
				}
				model.Animations[animationData.GetId()] = clip;
			}
		}

		public static NrmModel LoadFromJObject(GraphicsDevice graphicsDevice, JObject root, Func<string, Texture2D> textureGetter)
		{
			var context = new LoadContext(graphicsDevice, root);
			LoadMeshData(context);
			LoadMaterials(context, textureGetter);

			var rootNode = LoadRootNode(context);

			// Create the model
			var result = new NrmModel(rootNode);

			// Process skins
			ProcessSkins(result);

			result.UpdateBoundingBoxes();

			result.ClearAllTags();

			LoadAnimations(context, result);

			return result;
		}
	}
}
