using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text.Json;
using glTFLoader.Schema;
using glTFLoader;

namespace BatchConverter
{
	class Converter
	{
		static void RecursiveAction(ModelBone root, Action<ModelBone> action)
		{
			action(root);

			foreach (var child in root.Children)
			{
				RecursiveAction(child, action);
			}
		}

		public static unsafe void SaveGlb(Model model, string outputFile)
		{
			// Gather nodes & meshes
			var sourceNodes = new List<ModelBone>();
			var sourceMeshes = new List<ModelMesh>();

			var root = model.Root;
			RecursiveAction(root, n =>
			{
				sourceNodes.Add(n);

				if (n.Meshes != null)
				{
					foreach (var mesh in n.Meshes)
					{
						sourceMeshes.Add(mesh);
					}
				}
			});

			var gltfMeshes = new List<Mesh>();
			var bufferViews = new List<BufferView>();
			var accessors = new List<Accessor>();
			var nodesMeshes = new Dictionary<string, int>();

			var totalVertices = 0;
			byte[] buffer;

			var materialInfo = new Dictionary<string, Dictionary<int, Dictionary<string, string>>>();
			using (var ms = new MemoryStream())
			{
				foreach (var mesh in sourceMeshes)
				{
					var primitives = new List<MeshPrimitive>();
					for(var partIndex = 0; partIndex < mesh.MeshParts.Count; ++partIndex)
					{
						var part = mesh.MeshParts[partIndex];
						var primitive = new MeshPrimitive
						{
							Attributes = new Dictionary<string, int>()
						};

						var vertexBuffer = part.VertexBuffer;
						totalVertices += part.NumVertices;

						var vertexStride = vertexBuffer.VertexDeclaration.VertexStride;
						var data = new byte[part.NumVertices * vertexStride];

						vertexBuffer.GetData(part.VertexOffset * vertexStride, data, 0, part.NumVertices * vertexStride);

						var partOffset = 0;

						var elements = vertexBuffer.VertexDeclaration.GetVertexElements();


						for (var i = 0; i < elements.Length; ++i)
						{
							var element = elements[i];

							int? accessor = null;
							fixed (byte* cptr = &data[partOffset + element.Offset])
							{
								var ptr = cptr;

								switch (element.VertexElementFormat)
								{
									case VertexElementFormat.Vector2:
										var v2 = new List<Vector2>();
										for (var j = 0; j < part.NumVertices; ++j)
										{
											v2.Add(*(Vector2*)ptr);
											ptr += vertexStride;

										}

										accessor = ms.WriteData(bufferViews, accessors, v2.ToArray());
										break;
									case VertexElementFormat.Vector3:
										var v3 = new List<Vector3>();
										for (var j = 0; j < part.NumVertices; ++j)
										{
											v3.Add(*(Vector3*)ptr);
											ptr += vertexStride;

										}

										accessor = ms.WriteData(bufferViews, accessors, v3.ToArray());
										break;
									case VertexElementFormat.Vector4:
										var v4 = new List<Vector4>();
										for (var j = 0; j < part.NumVertices; ++j)
										{
											v4.Add(*(Vector4*)ptr);
											ptr += vertexStride;

										}

										accessor = ms.WriteData(bufferViews, accessors, v4.ToArray());
										break;
									case VertexElementFormat.Color:
										var cc = new List<Vector4>();
										for (var j = 0; j < part.NumVertices; ++j)
										{
											var v = *(Color*)ptr;
											cc.Add(v.ToVector4());
											ptr += vertexStride;

										}

										accessor = ms.WriteData(bufferViews, accessors, cc.ToArray());
										break;

									default:
										throw new Exception($"Can't process {element.VertexElementFormat}");
								}

							}

							switch (element.VertexElementUsage)
							{
								case VertexElementUsage.Position:
									primitive.Attributes["POSITION"] = accessor.Value;
									break;
								case VertexElementUsage.Color:
									primitive.Attributes["COLOR_" + element.UsageIndex] = accessor.Value;
									break;
								case VertexElementUsage.TextureCoordinate:
									primitive.Attributes["TEXCOORD_" + element.UsageIndex] = accessor.Value;
									break;
								case VertexElementUsage.Normal:
									primitive.Attributes["NORMAL"] = accessor.Value;
									break;

								// Since TANGENT/BINORMAL arent part of spec, it should start with '_'
								// Well, actually TANGENT is part of spec, but it requires VEC4, while we have VEC3
								case VertexElementUsage.Tangent:
									primitive.Attributes["_TANGENT"] = accessor.Value;
									break;
								case VertexElementUsage.Binormal:
									primitive.Attributes["_BINORMAL"] = accessor.Value;
									break;


								default:
									throw new Exception($"Can't process {element.VertexElementUsage}");
							}
						}

						// Convert to short
						var indicesShort = new ushort[part.PrimitiveCount * 3];
						part.IndexBuffer.GetData(part.StartIndex * sizeof(short), indicesShort, 0, part.PrimitiveCount * 3);

						indicesShort.Unwind();
						primitive.Indices = ms.WriteData(bufferViews, accessors, indicesShort.ToArray());

						if (part.Effect != null)
						{
							foreach (var par in part.Effect.Parameters)
							{
								if (par.ParameterType == EffectParameterType.Texture2D)
								{
									// Material
									var val = par.GetValueTexture2D();

									Dictionary<int, Dictionary<string, string>> meshMaterials;
									if (!materialInfo.TryGetValue(mesh.Name, out meshMaterials))
									{
										meshMaterials = new Dictionary<int, Dictionary<string, string>>();
										materialInfo[mesh.Name] = meshMaterials;
									}

									Dictionary<string, string> partMaterials;
									if (!meshMaterials.TryGetValue(partIndex, out partMaterials))
									{
										partMaterials = new Dictionary<string, string>();
										meshMaterials[partIndex] = partMaterials;
									}

									partMaterials[par.Name] = val.Name;
								}
							}
						}

						primitives.Add(primitive);
					}

					var gltfMesh = new Mesh
					{
						Name = mesh.Name,
						Primitives = primitives.ToArray()
					};

					nodesMeshes[mesh.Name] = gltfMeshes.Count;

					gltfMeshes.Add(gltfMesh);
				}

				buffer = ms.ToArray();
			}

			var nodes = new List<Node>();
			foreach (var bone in sourceNodes)
			{
				var gltfNode = new Node
				{
					Name = bone.Name,
					Matrix = bone.Transform.ToFloats(),
				};

				nodes.Add(gltfNode);
			}

			// Set children
			foreach (var bone in sourceNodes)
			{
				var node = (from n in nodes where n.Name == bone.Name select n).First();

				var children = new List<int>();
				foreach (var child in bone.Children)
				{
					int? index = null;
					for (var i = 0; i < nodes.Count; ++i)
					{
						if (child.Name == nodes[i].Name)
						{
							index = i;
							break;
						}
					}

					if (index == null)
					{
						throw new Exception($"Could not find node {child.Name}");
					}

					children.Add(index.Value);
				}

				if (children.Count > 0)
				{
					node.Children = children.ToArray();
				}
			}

			// Set nodes meshes
			foreach (var pair in nodesMeshes)
			{
				var node = (from n in nodes where n.Name == pair.Key select n).First();

				node.Mesh = pair.Value;
			}

			var buf = new glTFLoader.Schema.Buffer
			{
				ByteLength = buffer.Length
			};

			var scene = new Scene
			{
				Nodes = [0]
			};

			var gltf = new Gltf
			{
				Asset = new Asset
				{
					Generator = "GltfImporter",
					Version = "2.0",
				},
				Buffers = [buf],
				BufferViews = bufferViews.ToArray(),
				Accessors = accessors.ToArray(),
				Nodes = nodes.ToArray(),
				Meshes = gltfMeshes.ToArray(),
				Scenes = [scene],
				Scene = 0
			};


			var output = Path.ChangeExtension(outputFile, "glb");
			Logger.LogMessage($"Writing {output}");
			Interface.SaveBinaryModel(gltf, buffer, output);

			output = Path.ChangeExtension(outputFile, "material");
			Logger.LogMessage($"Writing {output}");

			var json = JsonSerializer.Serialize(materialInfo, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(output, json);
		}
	}
}