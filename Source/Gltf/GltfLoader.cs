using AssetManagementBase;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Utility;
using glTFLoader;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static glTFLoader.Schema.Accessor;
using static glTFLoader.Schema.AnimationChannelTarget;
using AnimationChannel = DigitalRiseModel.Animation.AnimationChannel;

namespace DigitalRiseModel
{
	internal class GltfLoader
	{
		private delegate void PoseSetter<T>(ref SrtTransform pose, T data);

		private struct VertexElementInfo
		{
			public VertexElementFormat Format;
			public VertexElementUsage Usage;
			public int UsageIndex;
			public int AccessorIndex;

			public VertexElementInfo(VertexElementFormat format, VertexElementUsage usage, int accessorIndex, int usageIndex)
			{
				Format = format;
				Usage = usage;
				AccessorIndex = accessorIndex;
				UsageIndex = usageIndex;
			}
		}

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

		private AssetManager _assetManager;
		private string _assetName;
		private Gltf _gltf;
		private GraphicsDevice _device;
		private readonly Dictionary<int, byte[]> _bufferCache = new Dictionary<int, byte[]>();
		private readonly List<DrMesh> _meshes = new List<DrMesh>();
		private readonly List<DrModelBone> _allBones = new List<DrModelBone>();
		private readonly List<DrSkin> _skins = new List<DrSkin>();
		private int _lastSkinIndex = 0;
		private ModelLoadingSettings _loadingSettings;
		private bool _ignoreMaterials;

		private byte[] FileResolver(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				using (var stream = _assetManager.Open(_assetName))
				{
					return Interface.LoadBinaryBuffer(stream);
				}
			}

			return _assetManager.ReadAsByteArray(path);
		}

		private byte[] GetBuffer(int index)
		{
			byte[] result;
			if (_bufferCache.TryGetValue(index, out result))
			{
				return result;
			}

			result = _gltf.LoadBinaryBuffer(index, path => FileResolver(path));
			_bufferCache[index] = result;

			return result;
		}

		private ArraySegment<byte> GetAccessorData(int accessorIndex)
		{
			var accessor = _gltf.Accessors[accessorIndex];
			if (accessor.BufferView == null)
			{
				throw new NotSupportedException("Accessors without buffer index arent supported");
			}

			var bufferView = _gltf.BufferViews[accessor.BufferView.Value];
			var buffer = GetBuffer(bufferView.Buffer);

			var size = accessor.Type.GetComponentCount() * accessor.ComponentType.GetComponentSize();
			return new ArraySegment<byte>(buffer, bufferView.ByteOffset + accessor.ByteOffset, accessor.Count * size);
		}

		private T[] GetAccessorAs<T>(int accessorIndex)
		{
			var type = typeof(T);
			if (type != typeof(float) && type != typeof(Vector3) && type != typeof(Vector4) && type != typeof(Quaternion) && type != typeof(Matrix))
			{
				throw new NotSupportedException("Only float/Vector3/Vector4 types are supported");
			}

			var accessor = _gltf.Accessors[accessorIndex];
			if (accessor.Type == TypeEnum.SCALAR && type != typeof(float))
			{
				throw new NotSupportedException("Scalar type could be converted only to float");
			}

			if (accessor.Type == TypeEnum.VEC3 && type != typeof(Vector3))
			{
				throw new NotSupportedException("VEC3 type could be converted only to Vector3");
			}

			if (accessor.Type == TypeEnum.VEC4 && type != typeof(Vector4) && type != typeof(Quaternion))
			{
				throw new NotSupportedException("VEC4 type could be converted only to Vector4 or Quaternion");
			}

			if (accessor.Type == TypeEnum.MAT4 && type != typeof(Matrix))
			{
				throw new NotSupportedException("MAT4 type could be converted only to Matrix");
			}

			var bytes = GetAccessorData(accessorIndex);

			var count = bytes.Count / Marshal.SizeOf(typeof(T));
			var result = new T[count];

			GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
			try
			{
				var pointer = handle.AddrOfPinnedObject();
				Marshal.Copy(bytes.Array, bytes.Offset, pointer, bytes.Count);
			}
			finally
			{
				if (handle.IsAllocated)
				{
					handle.Free();
				}
			}

			return result;
		}

		private VertexElementFormat GetAccessorFormat(int index)
		{
			var accessor = _gltf.Accessors[index];

			switch (accessor.Type)
			{
				case TypeEnum.VEC2:
					if (accessor.ComponentType == ComponentTypeEnum.FLOAT)
					{
						return VertexElementFormat.Vector2;
					}
					break;
				case TypeEnum.VEC3:
					if (accessor.ComponentType == ComponentTypeEnum.FLOAT)
					{
						return VertexElementFormat.Vector3;
					}
					break;
				case TypeEnum.VEC4:
					if (accessor.ComponentType == ComponentTypeEnum.FLOAT)
					{
						return VertexElementFormat.Vector4;
					}
					else if (accessor.ComponentType == ComponentTypeEnum.UNSIGNED_BYTE)
					{
						return VertexElementFormat.Byte4;
					}
					else if (accessor.ComponentType == ComponentTypeEnum.UNSIGNED_SHORT)
					{
						return VertexElementFormat.Short4;
					}
					break;
			}

			throw new NotSupportedException($"Accessor of type {accessor.Type} and component type {accessor.ComponentType} isn't supported");
		}

		private void LoadAnimationTransforms<T>(SrtTransform defaultPose, SortedDictionary<float, SrtTransform> poses, PoseSetter<T> poseSetter, float[] times, AnimationSampler sampler)
		{
			var data = GetAccessorAs<T>(sampler.Output);
			if (times.Length != data.Length)
			{
				throw new NotSupportedException("Translation length is different from times length");
			}

			for (var i = 0; i < times.Length; ++i)
			{
				var time = times[i];

				SrtTransform pose;
				if (!poses.TryGetValue(time, out pose))
				{
					pose = defaultPose;
				}

				poseSetter(ref pose, data[i]);

				poses[time] = pose;
			}
		}

		private IndexBuffer CreateIndexBuffer(MeshPrimitive primitive, TangentsGenerator tangentsGenerator)
		{
			if (primitive.Indices == null)
			{
				throw new NotSupportedException("Meshes without indices arent supported");
			}

			var indexAccessor = _gltf.Accessors[primitive.Indices.Value];
			if (indexAccessor.Type != TypeEnum.SCALAR)
			{
				throw new NotSupportedException("Only scalar index buffer are supported");
			}

			if (indexAccessor.ComponentType != ComponentTypeEnum.SHORT &&
				indexAccessor.ComponentType != ComponentTypeEnum.UNSIGNED_SHORT &&
				indexAccessor.ComponentType != ComponentTypeEnum.UNSIGNED_INT)
			{
				throw new NotSupportedException($"Index of type {indexAccessor.ComponentType} isn't supported");
			}

			var indexData = GetAccessorData(primitive.Indices.Value);

			var elementSize = indexAccessor.ComponentType == ComponentTypeEnum.SHORT ||
				indexAccessor.ComponentType == ComponentTypeEnum.UNSIGNED_SHORT ?
				IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits;

			var indexBuffer = new IndexBuffer(_device, elementSize, indexAccessor.Count, _loadingSettings.BufferUsage);
			indexBuffer.SetData(0, indexData.Array, indexData.Offset, indexData.Count);

			uint[] uintIndices;
			// Since gltf uses ccw winding by default
			// We need to unwind it
			if (indexAccessor.ComponentType == ComponentTypeEnum.UNSIGNED_SHORT)
			{
				var data = new ushort[indexData.Count / 2];
				System.Buffer.BlockCopy(indexData.Array, indexData.Offset, data, 0, indexData.Count);
				data.Unwind();
				tangentsGenerator?.SetIndices(data);
				indexBuffer.SetData(data);
				uintIndices = data.ToUnsignedIntArray();
			}
			else if (indexAccessor.ComponentType == ComponentTypeEnum.SHORT)
			{
				var data = new short[indexData.Count / 2];
				System.Buffer.BlockCopy(indexData.Array, indexData.Offset, data, 0, indexData.Count);
				data.Unwind();
				tangentsGenerator?.SetIndices(data);
				indexBuffer.SetData(data);
				uintIndices = data.ToUnsignedIntArray();
			}
			else
			{
				var data = new uint[indexData.Count / 4];
				System.Buffer.BlockCopy(indexData.Array, indexData.Offset, data, 0, indexData.Count);
				data.Unwind();
				tangentsGenerator?.SetIndices(data);
				indexBuffer.SetData(data);
				uintIndices = data;
			}

			indexBuffer.Tag = uintIndices;

			return indexBuffer;
		}

		private Texture2D LoadTexture(int index)
		{
			var gltfTexture = _gltf.Textures[index];
			if (gltfTexture.Source != null)
			{
				var image = _gltf.Images[gltfTexture.Source.Value];

				if (image.BufferView.HasValue)
				{
					throw new Exception("Embedded images arent supported.");
				}
				else if (image.Uri.StartsWith("data:image/"))
				{
					throw new Exception("Embedded images with uri arent supported.");
				}
				else
				{
					return _assetManager.LoadTexture2D(_device, image.Uri);
				}
			}

			return null;
		}

		private void LoadMeshes()
		{
			foreach (var gltfMesh in _gltf.Meshes)
			{
				var mesh = new DrMesh
				{
					Name = gltfMesh.Name
				};

				foreach (var primitive in gltfMesh.Primitives)
				{
					if (primitive.Mode != MeshPrimitive.ModeEnum.TRIANGLES)
					{
						throw new NotSupportedException($"Primitive mode {primitive.Mode} isn't supported.");
					}

					// Read vertex declaration
					var vertexInfos = new List<VertexElementInfo>();
					int? vertexCount = null;
					var hasSkinning = false;
					var hasTangents = false;
					var hasNormals = false;
					var hasUV = false;
					foreach (var pair in primitive.Attributes)
					{
						var accessor = _gltf.Accessors[pair.Value];
						var newVertexCount = accessor.Count;
						if (vertexCount != null && vertexCount.Value != newVertexCount)
						{
							throw new NotSupportedException($"Vertex count changed. Previous value: {vertexCount}. New value: {newVertexCount}");
						}

						vertexCount = newVertexCount;

						var element = new VertexElementInfo
						{
							Format = GetAccessorFormat(pair.Value),
							AccessorIndex = pair.Value
						};

						if (pair.Key == "POSITION")
						{
							if (element.Format != VertexElementFormat.Vector3)
							{
								throw new NotSupportedException($"Positions only in Vector3 format are supported");
							}

							element.Usage = VertexElementUsage.Position;
						}
						else if (pair.Key == "NORMAL")
						{
							element.Usage = VertexElementUsage.Normal;
							hasNormals = true;
						}
						else if (pair.Key == "TANGENT" || pair.Key == "_TANGENT")
						{
							element.Usage = VertexElementUsage.Tangent;
							hasTangents = element.Format == VertexElementFormat.Vector4;
						}
						else if (pair.Key == "_BINORMAL")
						{
							element.Usage = VertexElementUsage.Binormal;
						}
						else if (pair.Key.StartsWith("TEXCOORD_"))
						{
							element.Usage = VertexElementUsage.TextureCoordinate;
							element.UsageIndex = int.Parse(pair.Key.Substring(9));
							hasUV = true;
						}
						else if (pair.Key.StartsWith("JOINTS_"))
						{
							if (element.Format != VertexElementFormat.Byte4 && element.Format != VertexElementFormat.Short4)
							{
								throw new NotSupportedException($"Blend indices only in Byte4 format are supported");
							}

							element.Usage = VertexElementUsage.BlendIndices;
							element.UsageIndex = int.Parse(pair.Key.Substring(7));
							hasSkinning = true;
						}
						else if (pair.Key.StartsWith("WEIGHTS_"))
						{
							if (element.Format != VertexElementFormat.Vector4)
							{
								throw new NotSupportedException($"Blend weights only in Vector4 format are supported");
							}

							element.Usage = VertexElementUsage.BlendWeight;
							element.UsageIndex = int.Parse(pair.Key.Substring(8));
							hasSkinning = true;
						}
						else if (pair.Key.StartsWith("COLOR_"))
						{
							element.Usage = VertexElementUsage.Color;
							element.UsageIndex = int.Parse(pair.Key.Substring(6));
						}
						else
						{
							throw new Exception($"Attribute of type '{pair.Key}' isn't supported.");
						}

						vertexInfos.Add(element);
					}

					if (vertexCount == null)
					{
						throw new NotSupportedException("Vertex count is not set");
					}

					var generateTangents = _loadingSettings.GenerateTangents == TangentsGeneration.Always ||
						(_loadingSettings.GenerateTangents == TangentsGeneration.IfDoesntExist && (!hasTangents));

					if (generateTangents && !hasNormals)
					{
						throw new Exception("Can't generate tangents without normals");
					}

					if (generateTangents && !hasUV)
					{
						throw new Exception("Can't generate tangents without uvs");
					}

					var vertexElements = new List<VertexElement>();
					var offset = 0;
					for (var i = 0; i < vertexInfos.Count; ++i)
					{
						var usage = vertexInfos[i].Usage;
						if (generateTangents && (usage == VertexElementUsage.Tangent || usage == VertexElementUsage.Binormal))
						{
							continue;
						}

						vertexElements.Add(new VertexElement(offset, vertexInfos[i].Format, vertexInfos[i].Usage, vertexInfos[i].UsageIndex));
						offset += vertexInfos[i].Format.GetSize();
					}

					if (generateTangents)
					{
						vertexElements.Add(new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0));
					}

					var vd = new VertexDeclaration(vertexElements.ToArray());
					var vertexBuffer = new VertexBuffer(_device, vd, vertexCount.Value, _loadingSettings.BufferUsage);

					// Set vertex data
					var vertexData = new byte[vertexCount.Value * vd.VertexStride];
					var boundingBoxData = new VertexBufferBoundingBoxData(vertexCount.Value, hasSkinning);

					TangentsGenerator tangentsGenerator = null;
					if (generateTangents)
					{
						tangentsGenerator = new TangentsGenerator(vertexCount.Value);
					}

					offset = 0;
					for (var i = 0; i < vertexInfos.Count; ++i)
					{
						var sz = vertexInfos[i].Format.GetSize();
						var data = GetAccessorData(vertexInfos[i].AccessorIndex);

						for (var j = 0; j < vertexCount.Value; ++j)
						{
							Array.Copy(data.Array, data.Offset + j * sz, vertexData, j * vd.VertexStride + offset, sz);

							switch (vertexInfos[i].Usage)
							{
								case VertexElementUsage.Position:
									unsafe
									{
										fixed (byte* bptr = &data.Array[data.Offset + j * sz])
										{
											Vector3* vptr = (Vector3*)bptr;
											var v = *vptr;
											boundingBoxData.SetPosition(j, v);
											tangentsGenerator?.SetPosition(j, v);
										}
									}
									break;

								case VertexElementUsage.BlendIndices:
									unsafe
									{
										fixed (byte* bptr = &data.Array[data.Offset + j * sz])
										{
											Vector4 indices;
											if (vertexInfos[i].Format == VertexElementFormat.Byte4)
											{
												indices = (*(Byte4*)bptr).ToVector4();
											}
											else
											{
												indices = (*(Short4*)bptr).ToVector4();
											}

											boundingBoxData.SetBonesIndices(j, indices);
										}
									}
									break;

								case VertexElementUsage.BlendWeight:
									unsafe
									{
										fixed (byte* bptr = &data.Array[data.Offset + j * sz])
										{
											boundingBoxData.SetBonesWeights(j, *(Vector4*)bptr);
										}
									}
									break;

								case VertexElementUsage.Normal:
									if (tangentsGenerator != null)
									{
										unsafe
										{
											fixed (byte* bptr = &data.Array[data.Offset + j * sz])
											{
												Vector3* vptr = (Vector3*)bptr;
												var v = *vptr;
												tangentsGenerator.SetNormal(j, v);
											}
										}
									}
									break;

								case VertexElementUsage.TextureCoordinate:
									if (tangentsGenerator != null && vertexInfos[i].UsageIndex == 0)
									{
										unsafe
										{
											fixed (byte* bptr = &data.Array[data.Offset + j * sz])
											{
												Vector2* vptr = (Vector2*)bptr;
												var v = *vptr;
												tangentsGenerator.SetUV(j, v);
											}
										}
									}
									break;
							}
						}

						offset += sz;
					}

					// Create index buffer now in order to update tangentsGenerator with indices data
					var indexBuffer = CreateIndexBuffer(primitive, tangentsGenerator);

					if (tangentsGenerator != null)
					{
						var tangents = tangentsGenerator.CalculateTangentFrames();

						// Set new tangents
						var ve = vd.EnsureElement(VertexElementUsage.Tangent);
						offset = ve.Offset;
						var sz = ve.VertexElementFormat.GetSize();
						for (var j = 0; j < vertexCount.Value; ++j)
						{
							unsafe
							{
								fixed (byte* bptr = &vertexData[offset + j * vd.VertexStride])
								{
									Vector4* vptr = (Vector4*)bptr;

									*vptr = tangents[j];
								}
							}
						}
					}

					vertexBuffer.SetData(vertexData);

					var material = new DrMaterial();

					// Use empty bounding box for now
					// It'll be recalculated in the end
					var meshPart = new DrMeshPart(vertexBuffer, indexBuffer, new BoundingBox())
					{
						Material = material
					};

					// Store for later bounding boxes calculation
					vertexBuffer.Tag = boundingBoxData;

					if (!_ignoreMaterials && primitive.Material != null)
					{
						var gltfMaterial = _gltf.Materials[primitive.Material.Value];
						material.Name = gltfMaterial.Name;
						if (gltfMaterial.PbrMetallicRoughness != null)
						{
							material.DiffuseColor = new Color(gltfMaterial.PbrMetallicRoughness.BaseColorFactor.ToVector4());
							if (gltfMaterial.PbrMetallicRoughness.BaseColorTexture != null)
							{
								material.DiffuseTexture = LoadTexture(gltfMaterial.PbrMetallicRoughness.BaseColorTexture.Index);
							}

							if (gltfMaterial.NormalTexture != null)
							{
								material.NormalTexture = LoadTexture(gltfMaterial.NormalTexture.Index);
							}

						}
					}

					mesh.MeshParts.Add(meshPart);
				}

				_meshes.Add(mesh);
			}
		}

		private DrSkin LoadSkin(int skinId)
		{
			var gltfSkin = _gltf.Skins[skinId];
			var transforms = GetAccessorAs<Matrix>(gltfSkin.InverseBindMatrices.Value);
			if (gltfSkin.Joints.Length != transforms.Length)
			{
				throw new Exception($"Skin {gltfSkin.Name} inconsistency. Joints amount: {gltfSkin.Joints.Length}, Inverse bind matrices amount: {transforms.Length}");
			}

			var joints = new List<DrSkinJoint>();
			for (var i = 0; i < gltfSkin.Joints.Length; ++i)
			{
				var jointIndex = gltfSkin.Joints[i];
				joints.Add(new DrSkinJoint(_allBones[jointIndex], transforms[i]));
			}

			var result = new DrSkin(_lastSkinIndex, joints.ToArray());
			Debug.WriteLine($"Skin {gltfSkin.Name} has {gltfSkin.Joints.Length} joints");

			++_lastSkinIndex;

			return result;
		}

		private void LoadAllNodes()
		{
			// First run - load all nodes
			for (var i = 0; i < _gltf.Nodes.Length; ++i)
			{
				var gltfNode = _gltf.Nodes[i];

				var pose = new SrtTransform
				{
					Translation = gltfNode.Translation != null ? gltfNode.Translation.ToVector3() : Vector3.Zero,
					Scale = gltfNode.Scale != null ? gltfNode.Scale.ToVector3() : Vector3.One,
					Rotation = gltfNode.Rotation != null ? gltfNode.Rotation.ToQuaternion() : Quaternion.Identity
				};

				if (gltfNode.Matrix != null)
				{
					var matrix = gltfNode.Matrix.ToMatrix();

					if (matrix != Matrix.Identity)
					{
						matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);

						pose.Translation = translation;
						pose.Scale = scale;
						pose.Rotation = rotation;
					}
				}

				DrMesh mesh = null;
				if (gltfNode.Mesh != null)
				{
					mesh = _meshes[gltfNode.Mesh.Value];
				}

				var bone = new DrModelBone(gltfNode.Name, mesh)
				{
					DefaultPose = pose
				};
				_allBones.Add(bone);
			}

			// Second run - build skins
			if (_gltf.Skins != null)
			{
				for (var i = 0; i < _gltf.Skins.Length; ++i)
				{
					var skin = LoadSkin(i);
					_skins.Add(skin);
				}
			}

			// Second run - set children and skins
			for (var i = 0; i < _gltf.Nodes.Length; ++i)
			{
				var gltfNode = _gltf.Nodes[i];
				var bone = _allBones[i];

				if (gltfNode.Children != null)
				{
					var children = new List<DrModelBone>();
					foreach (var childIndex in gltfNode.Children)
					{
						children.Add(_allBones[childIndex]);
					}

					bone.Children = children.ToArray();
				}

				if (gltfNode.Skin != null)
				{
					if (bone.Mesh != null)
					{
						foreach (var part in bone.Mesh.MeshParts)
						{
							part.Skin = _skins[gltfNode.Skin.Value];
						}
					}
				}
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

				var channels = new List<AnimationChannel>();
				float time = 0;
				foreach (var pair in channelsDict)
				{
					var bone = _allBones[pair.Key];
					var animationData = new SortedDictionary<float, SrtTransform>();

					var translationMode = InterpolationMode.None;
					var rotationMode = InterpolationMode.None;
					var scaleMode = InterpolationMode.None;
					foreach (var pathInfo in pair.Value)
					{
						var sampler = gltfAnimation.Samplers[pathInfo.Sampler];
						var times = GetAccessorAs<float>(sampler.Input);

						switch (pathInfo.Path)
						{
							case PathEnum.translation:
								LoadAnimationTransforms(bone.DefaultPose, animationData,
									(ref SrtTransform p, Vector3 d) => p.Translation = d,
									times, sampler);
								translationMode = sampler.Interpolation.ToInterpolationMode();
								break;
							case PathEnum.rotation:
								LoadAnimationTransforms(bone.DefaultPose, animationData,
									(ref SrtTransform p, Quaternion d) => p.Rotation = d,
									times, sampler);
								rotationMode = sampler.Interpolation.ToInterpolationMode();
								break;
							case PathEnum.scale:
								LoadAnimationTransforms(bone.DefaultPose, animationData,
									(ref SrtTransform p, Vector3 d) => p.Scale = d,
									times, sampler);
								scaleMode = sampler.Interpolation.ToInterpolationMode();
								break;
							case PathEnum.weights:
								break;
						}
					}

					var keyframes = new List<AnimationChannelKeyframe>();
					foreach (var pair2 in animationData)
					{
						keyframes.Add(new AnimationChannelKeyframe(TimeSpan.FromSeconds(pair2.Key), pair2.Value));

						if (pair2.Key > time)
						{
							time = pair2.Key;
						}
					}

					var animationChannel = new AnimationChannel(bone.Index, keyframes.ToArray())
					{
						TranslationMode = translationMode,
						RotationMode = rotationMode,
						ScaleMode = scaleMode
					};

					channels.Add(animationChannel);
				}

				var id = gltfAnimation.Name ?? "(default)";
				var animation = new AnimationClip(id, TimeSpan.FromSeconds(time), channels.ToArray());
				model.Animations[id] = animation;
			}
		}

		public DrModel Load(GraphicsDevice device, AssetManager manager, string assetName, ModelLoadingSettings loadingSettings, bool ignoreMaterials)
		{
			_device = device ?? throw new ArgumentNullException(nameof(device));
			_loadingSettings = loadingSettings ?? throw new ArgumentNullException(nameof(loadingSettings));
			_ignoreMaterials = ignoreMaterials;

			_meshes.Clear();
			_allBones.Clear();
			_skins.Clear();

			_assetManager = manager;
			_assetName = assetName;
			using (var stream = manager.Open(assetName))
			{
				_gltf = Interface.LoadModel(stream);
			}

			LoadMeshes();
			LoadAllNodes();

			// Fix root
			var scene = _gltf.Scenes[_gltf.Scene.Value];
			var roots = (from idx in scene.Nodes select _allBones[idx]).ToList();
			var root = roots.FixRoot(_allBones[scene.Nodes[0]]);

			// Create the model
			var model = new DrModel(root);

			model.UpdateBoundingBoxes();

			LoadAnimations(model);

			model.ClearAllTags();

			return model;
		}
	}
}