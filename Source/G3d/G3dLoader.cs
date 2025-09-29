using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using NursiaModel;
using NursiaModel.Animation;
using NursiaModel.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NursiaModel
{
	internal static class G3dLoader
	{
		private class AttributeInfo
		{
			public int Size { get; private set; }
			public int ElementsCount { get; private set; }
			public VertexElementFormat Format { get; private set; }
			public VertexElementUsage Usage { get; private set; }

			public AttributeInfo(int size, int elementsCount,
				VertexElementFormat format, VertexElementUsage usage)
			{
				Size = size;
				ElementsCount = elementsCount;
				Format = format;
				Usage = usage;
			}
		}

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

		private static readonly Dictionary<string, AttributeInfo> _attributes = new Dictionary<string, AttributeInfo>
		{
			["POSITION"] = new AttributeInfo(12, 3, VertexElementFormat.Vector3, VertexElementUsage.Position),
			["NORMAL"] = new AttributeInfo(12, 3, VertexElementFormat.Vector3, VertexElementUsage.Normal),
			["TEXCOORD"] = new AttributeInfo(8, 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate),
			["BLENDWEIGHT"] = new AttributeInfo(8, 2, VertexElementFormat.Vector2, VertexElementUsage.BlendWeight),
			["COLORPACKED"] = new AttributeInfo(4, 1, VertexElementFormat.Color, VertexElementUsage.Color)
		};

		internal const string IdName = "name";

		private static Stream EnsureOpen(Func<string, Stream> streamOpener, string name)
		{
			var result = streamOpener(name);
			if (result == null)
			{
				throw new Exception(string.Format("stream is null for name '{0}'", name));
			}

			return result;
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

		private static VertexDeclaration LoadDeclaration(JArray data, out int elementsPerData)
		{
			elementsPerData = 0;
			var elements = new List<VertexElement>();
			var offset = 0;
			foreach (var elementData in data)
			{
				var name = elementData.ToString();
				var usage = 0;

				// Remove last digit
				var lastChar = name[name.Length - 1];
				if (char.IsDigit(lastChar))
				{
					name = name.Substring(0, name.Length - 1);
					usage = int.Parse(lastChar.ToString());
				}

				AttributeInfo attributeInfo;
				if (!_attributes.TryGetValue(name, out attributeInfo))
				{
					throw new Exception(string.Format("Unknown attribute '{0}'", name));
				}

				var element = new VertexElement(offset,
					attributeInfo.Format,
					attributeInfo.Usage,
					usage);
				elements.Add(element);

				offset += attributeInfo.Size;
				elementsPerData += attributeInfo.ElementsCount;
			}

			return new VertexDeclaration(elements.ToArray());
		}

		private static void LoadFloat(byte[] dest, ref int destIdx, float data)
		{
			var byteData = BitConverter.GetBytes(data);

			var aaa = BitConverter.ToSingle(byteData, 0);
			Array.Copy(byteData, 0, dest, destIdx, byteData.Length);
			destIdx += byteData.Length;
		}

		private static void LoadByte(byte[] dest, ref int destIdx, int data)
		{
			if (data > byte.MaxValue)
			{
				throw new Exception(string.Format("Only byte NrmModelBone indices suported. {0}", data));
			}

			dest[destIdx] = (byte)data;
			++destIdx;
		}

		private static VertexBuffer LoadVertexBuffer(
			GraphicsDevice graphicsDevice,
			ref VertexDeclaration declaration,
			int elementsPerRow,
			JArray data,
			out List<Vector3> positions)
		{
			var rowsCount = data.Count / elementsPerRow;
			var elements = declaration.GetVertexElements();

			var blendWeightOffset = 0;
			var blendWeightCount = (from e in elements
									where e.VertexElementUsage == VertexElementUsage.BlendWeight
									select e).Count();
			var hasBlendWeight = blendWeightCount > 0;
			if (blendWeightCount > 4)
			{
				throw new Exception("4 is maximum amount of weights per bone");
			}
			if (hasBlendWeight)
			{
				blendWeightOffset = (from e in elements
									 where e.VertexElementUsage == VertexElementUsage.BlendWeight
									 select e).First().Offset;

				var newElements = new List<VertexElement>();
				newElements.AddRange(from e in elements
									 where e.VertexElementUsage != VertexElementUsage.BlendWeight
									 select e);
				newElements.Add(new VertexElement(blendWeightOffset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0));
				newElements.Add(new VertexElement(blendWeightOffset + 4, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));
				declaration = new VertexDeclaration(newElements.ToArray());
			}

			positions = new List<Vector3>();
			var byteData = new byte[rowsCount * declaration.VertexStride];

			for (var i = 0; i < rowsCount; ++i)
			{
				var destIdx = i * declaration.VertexStride;
				var srcIdx = i * elementsPerRow;
				var weightsCount = 0;
				for (var j = 0; j < elements.Length; ++j)
				{
					var element = elements[j];

					if (element.VertexElementUsage == VertexElementUsage.BlendWeight)
					{
						// Convert from libgdx multiple vector2 blendweight
						// to single int4 blendindices/vector4 blendweight
						if (element.VertexElementFormat != VertexElementFormat.Vector2)
						{
							throw new Exception("Only Vector2 format for BlendWeight supported.");
						}

						var offset = i * declaration.VertexStride + blendWeightOffset + weightsCount;
						LoadByte(byteData, ref offset, (int)(float)data[srcIdx++]);

						offset = i * declaration.VertexStride + blendWeightOffset + 4 + weightsCount * 4;
						LoadFloat(byteData, ref offset, (float)data[srcIdx++]);
						++weightsCount;
						continue;
					}

					switch (element.VertexElementFormat)
					{
						case VertexElementFormat.Vector2:
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							break;
						case VertexElementFormat.Vector3:
							var v = new Vector3((float)data[srcIdx++],
								(float)data[srcIdx++],
								(float)data[srcIdx++]);

							if (element.VertexElementUsage == VertexElementUsage.Position)
							{
								positions.Add(v);
							}

							LoadFloat(byteData, ref destIdx, v.X);
							LoadFloat(byteData, ref destIdx, v.Y);
							LoadFloat(byteData, ref destIdx, v.Z);
							break;
						case VertexElementFormat.Vector4:
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							break;
						case VertexElementFormat.Byte4:
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							LoadByte(byteData, ref destIdx, (int)data[srcIdx++]);
							break;
						case VertexElementFormat.Color:
							LoadFloat(byteData, ref destIdx, (float)data[srcIdx++]);
							break;
						default:
							throw new Exception(string.Format("{0} not supported", element.VertexElementFormat));
					}
				}
			}

			var result = new VertexBuffer(graphicsDevice, declaration, rowsCount, BufferUsage.None);
			result.SetData(byteData);

			return result;
		}

		private static void LoadMeshData(LoadContext context)
		{
			var meshesData = context.Root["meshes"];
			foreach (JObject meshData in meshesData)
			{
				// Determine vertex type
				int elementsPerRow;
				var declaration = LoadDeclaration((JArray)meshData["attributes"], out elementsPerRow);
				var vertices = (JArray)meshData["vertices"];

				int bonesCount = 0;
				foreach (var element in declaration.GetVertexElements())
				{
					if (element.VertexElementUsage != VertexElementUsage.BlendWeight)
					{
						continue;
					}

					if (element.UsageIndex + 1 > bonesCount)
					{
						bonesCount = element.UsageIndex + 1;
					}
				}

				if (bonesCount > 0 && bonesCount != 4)
				{
					throw new NotSupportedException("Only 4 bones per mesh are supported");
				}

				List<Vector3> positions;
				var vertexBuffer = LoadVertexBuffer(
					context.GraphicsDevice,
					ref declaration,
					elementsPerRow,
					vertices,
					out positions);

				var partsData = (JArray)meshData["parts"];
				foreach (JObject partData in partsData)
				{
					var id = partData.GetId();

					// var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), partData.EnsureString("type"));
					var partPositions = new List<Vector3>();
					var indicesData = (JArray)partData["indices"];
					var indices = new short[indicesData.Count];
					for (var i = 0; i < indicesData.Count; ++i)
					{
						var idx = Convert.ToInt16(indicesData[i]);
						indices[i] = idx;
						partPositions.Add(positions[idx]);
					}

					indices.Unwind();

					var indexBuffer = new IndexBuffer(context.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
					indexBuffer.SetData(indices);

					var boundingBox = BoundingBox.CreateFromPoints(partPositions);

					context.Meshes[id] = new NrmMeshPart(vertexBuffer, indexBuffer, boundingBox);
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
					DiffuseColor = Color.White
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
			Dictionary<string, SrtTransform> jointsDict = null;
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

						if (jointsDict == null)
						{
							jointsDict = new Dictionary<string, SrtTransform>();
						}
						foreach (JObject jointData in jointsData)
						{
							jointsDict[jointData.EnsureString("node")] = LoadTransform(jointData);
						}
					}
				}

				result = new NrmModelBone(data.GetId(), mesh);
			}
			else
			{
				result = new NrmModelBone(data.GetId());
			}

			result.DefaultPose = LoadTransform(data);
			result.Tag = jointsDict;

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
			foreach (var bone in model.Bones)
			{
				if (bone.Tag == null)
				{
					continue;
				}

				var jointsDict = (Dictionary<string, SrtTransform>)bone.Tag;
				var joints = new List<NrmSkinJoint>();
				foreach (var pair in jointsDict)
				{
					var joint = new NrmSkinJoint(model.FindBoneByName(pair.Key), Matrix.Invert(pair.Value.ToMatrix()));
					joints.Add(joint);
				}

				var skin = new NrmSkin(skinIndex, joints.ToArray());
				bone.Skin = skin;

				++skinIndex;
			}
		}

		private static void LoadAnimations(LoadContext context, NrmModel model)
		{
			if (!context.Root.ContainsKey("animations"))
			{
				return;
			}

			model.Animations = new Dictionary<string, AnimationClip>();

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

				var clip = new AnimationClip(animationData.GetId(), maxTime, channels.ToArray());
				model.Animations[animationData.GetId()] = clip;
			}
		}

		public static NrmModel LoadFromJson(GraphicsDevice graphicsDevice, string json, Func<string, Texture2D> textureGetter)
		{
			var root = JObject.Parse(json);

			var context = new LoadContext(graphicsDevice, root);
			LoadMeshData(context);
			LoadMaterials(context, textureGetter);

			var rootNode = LoadRootNode(context);

			// Create the model
			var result = new NrmModel(rootNode);

			// Process skins
			ProcessSkins(result);

			result.ClearAllTags();

			LoadAnimations(context, result);

			return result;
		}
	}
}
