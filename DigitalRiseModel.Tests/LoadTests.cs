using Xunit;
using System;
using System.Linq;

namespace DigitalRiseModel.Tests
{
	public sealed class LoadTests
	{
		private static void VerifyBoneHierarchy(DrModelBone bone, int depth = 0)
		{
			Assert.NotNull(bone);
			Assert.NotNull(bone.Name);

			if (bone.Children != null)
			{
				foreach (var child in bone.Children)
				{
					Assert.NotNull(child);
					Assert.Equal(bone, child.Parent);
					VerifyBoneHierarchy(child, depth + 1);
				}
			}
		}

		private static void TestDudeModel(DrModel model, string rootName, int bonesCount, bool readableBuffers)
		{
			Assert.NotNull(model.Root);
			Assert.Equal(rootName, model.Root.Name);
			Assert.NotNull(model.Bones);
			Assert.Equal(bonesCount, model.Bones.Length);
			Assert.NotNull(model.Meshes);
			Assert.Single(model.Meshes);
			Assert.NotNull(model.Animations);
			Assert.Single(model.Animations);

			var firstPart = model.Meshes[0].MeshParts[0];
			var data = new byte[firstPart.VertexBuffer.VertexCount * firstPart.VertexBuffer.VertexDeclaration.VertexStride];

			if (readableBuffers)
			{
				firstPart.VertexBuffer.GetData(data);
			}
			else
			{
				Assert.Throws<NotSupportedException>(() =>
				{
					firstPart.VertexBuffer.GetData(data);
				});
			}
		}

		[Theory]
		[InlineData("dude.gltf")]
		[InlineData("dude.glb")]
		public void LoadDudeModel(string file, string rootName = "RootNode", int bonesCount = 60)
		{
			var manager = Utility.CreateAssetManager();
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, ModelLoadFlags.IgnoreMaterials);
			TestDudeModel(model, rootName, bonesCount, false);

			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, ModelLoadFlags.IgnoreMaterials | ModelLoadFlags.ReadableBuffers);
			TestDudeModel(model, rootName, bonesCount, true);
		}

		[Fact]
		public void TestIgnoreMaterials()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "CesiumMilkTruck.gltf");

			var mat = model.Meshes[0].MeshParts[0].Material;
			Assert.NotNull(mat.DiffuseTexture);
			Assert.Equal("CesiumMilkTruck.jpg", mat.DiffuseTexture.Name);
			Assert.Equal(2048, mat.DiffuseTexture.Width);

			// No materials
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "CesiumMilkTruck.gltf", ModelLoadFlags.IgnoreMaterials);
			mat = model.Meshes[0].MeshParts[0].Material;
			Assert.Null(mat.DiffuseTexture);

			// Since different settings produce different cache keys
			// manager should hold 2 models
			var modelCount = (from pair in manager.Cache where pair.Value is DrModel select pair.Key).Count();
			Assert.Equal(2, modelCount);
		}

		[Fact]
		public void TestEnsureUVs()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "BrainStem.glb", ModelLoadFlags.IgnoreMaterials);

			// Make sure there isn't uv
			var el = model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.FindElement(Microsoft.Xna.Framework.Graphics.VertexElementUsage.TextureCoordinate);
			Assert.Null(el);

			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "BrainStem.glb", ModelLoadFlags.IgnoreMaterials | ModelLoadFlags.EnsureUVs);

			// Now there should be uv
			el = model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.FindElement(Microsoft.Xna.Framework.Graphics.VertexElementUsage.TextureCoordinate);
			Assert.NotNull(el);
		}

		[Fact]
		public void TestEraseRootParent()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "alien.glb", ModelLoadFlags.IgnoreMaterials);

			Assert.Null(model.Root.Parent);
		}

		[Fact]
		public void TestEmbeddedTexture()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "orange_flower.glb");
		}

		[Fact]
		public void LoadSinbad3RootsModel()
		{
			var manager = Utility.CreateAssetManager();

			// Load Sinbad model with multiple roots
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "Sinbad3Roots.glb", ModelLoadFlags.IgnoreMaterials);

			// Verify model loaded successfully
			Assert.NotNull(model);
			Assert.NotNull(model.Root);
			Assert.NotNull(model.Bones);
			Assert.NotNull(model.Meshes);

			// Sinbad has 3 root nodes (70, 71, 72 in the scene), so FixRoot creates a synthetic "_Root" node
			Assert.Equal("_Root", model.Root.Name);

			// Verify root has 3 children (the original root nodes)
			Assert.NotNull(model.Root.Children);
			Assert.Equal(3, model.Root.Children.Length);

			// Verify total bone count (73 nodes + 1 synthetic root = 74)
			Assert.Equal(74, model.Bones.Length);

			// Verify all bones are properly indexed
			for (int i = 0; i < model.Bones.Length; i++)
			{
				Assert.Equal(i, model.Bones[i].Index);
			}

			// Verify root has no parent
			Assert.Null(model.Root.Parent);

			// Verify meshes are loaded
			Assert.True(model.Meshes.Length > 0);

			// Verify all meshes have mesh parts
			foreach (var mesh in model.Meshes)
			{
				Assert.NotNull(mesh.MeshParts);
				Assert.True(mesh.MeshParts.Count > 0);

				// Verify vertex buffers
				foreach (var part in mesh.MeshParts)
				{
					Assert.NotNull(part.VertexBuffer);
					Assert.True(part.VertexBuffer.VertexCount > 0);
					Assert.NotNull(part.VertexBuffer.VertexDeclaration);

					// Verify vertex elements
					var elements = part.VertexBuffer.VertexDeclaration.GetVertexElements();
					Assert.True(elements.Length > 0);

					// Verify expected vertex elements exist
					var hasPosition = false;
					foreach (var element in elements)
					{
						if (element.VertexElementUsage == Microsoft.Xna.Framework.Graphics.VertexElementUsage.Position)
						{
							hasPosition = true;
							break;
						}
					}
					Assert.True(hasPosition);
				}
			}

			// Verify bone transforms can be copied
			var boneTransforms = new Microsoft.Xna.Framework.Matrix[model.Bones.Length];
			model.CopyBoneTransformsTo(boneTransforms);

			Assert.NotNull(boneTransforms);

			// Verify absolute transforms can be calculated
			var absoluteTransforms = new Microsoft.Xna.Framework.Matrix[model.Bones.Length];
			model.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

			Assert.NotNull(absoluteTransforms);

			// Verify animations (if any)
			if (model.Animations != null && model.Animations.Count > 0)
			{
				foreach (var animation in model.Animations.Values)
				{
					Assert.NotNull(animation);
					Assert.NotNull(animation.Channels);
				}
			}

			// Verify bone hierarchy - check that all children are properly linked
			VerifyBoneHierarchy(model.Root);

			// Verify that all bones except root have a parent
			for (int i = 1; i < model.Bones.Length; i++)
			{
				var bone = model.Bones[i];
				if (bone.Parent == null)
				{
					// Only the three original root nodes should have no parent
					Assert.True(bone.Parent != null || model.Root.Children.Contains(bone));
				}
			}

			// Verify bone transforms have valid data
			for (int i = 0; i < model.Bones.Length; i++)
			{
				var bone = model.Bones[i];
				var localTransform = bone.CalculateDefaultLocalTransform();

				// Verify transform is not NaN or Infinity
				Assert.False(float.IsNaN(localTransform.M11));
				Assert.False(float.IsInfinity(localTransform.M11));
			}

			// Verify mesh parts have valid bounding boxes
			foreach (var mesh in model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					var bb = part.BoundingBox;
					Assert.False(float.IsNaN(bb.Min.X));
					Assert.False(float.IsInfinity(bb.Max.Y));
				}
			}

			// Load with readable buffers flag and verify data can be read
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "Sinbad3Roots.glb",
				ModelLoadFlags.IgnoreMaterials | ModelLoadFlags.ReadableBuffers);

			foreach (var mesh in model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					var vertexCount = part.VertexBuffer.VertexCount;
					var vertexStride = part.VertexBuffer.VertexDeclaration.VertexStride;
					var data = new byte[vertexCount * vertexStride];

					// Should not throw with ReadableBuffers flag
					part.VertexBuffer.GetData(data);

					// Verify data is not empty
					bool hasNonZeroData = false;
					foreach (var b in data)
					{
						if (b != 0)
						{
							hasNonZeroData = true;
							break;
						}
					}
					Assert.True(hasNonZeroData);
				}
			}
		}

		[Fact]
		public void TestIgnoreExternalMaterials()
		{
			var manager = Utility.CreateAssetManager();

			// The model has external materials
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "CesiumMilkTruck.gltf", ModelLoadFlags.IgnoreEmbeddedMaterials);
			var mat = model.Meshes[0].MeshParts[0].Material;
			Assert.NotNull(mat.DiffuseTexture);
			Assert.Equal("CesiumMilkTruck.jpg", mat.DiffuseTexture.Name);

			// Ignore external materials only
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "CesiumMilkTruck.gltf", ModelLoadFlags.IgnoreExternalMaterials);
			mat = model.Meshes[0].MeshParts[0].Material;
			Assert.Null(mat.DiffuseTexture);

			// Since different settings produce different cache keys
			// manager should hold 2 models
			var modelCount = (from pair in manager.Cache where pair.Value is DrModel select pair.Key).Count();
			Assert.Equal(2, modelCount);
		}

		[Fact]
		public void TestIgnoreEmbeddedMaterials()
		{
			var manager = Utility.CreateAssetManager();

			// orange_flower.glb has embedded textures
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "orange_flower.glb", ModelLoadFlags.IgnoreExternalMaterials);
			var mat = model.Meshes[0].MeshParts[0].Material;
			Assert.NotNull(mat.DiffuseTexture);

			// Load with IgnoreEmbeddedMaterials - embedded textures should be null
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "orange_flower.glb", ModelLoadFlags.IgnoreEmbeddedMaterials);
			mat = model.Meshes[0].MeshParts[0].Material;
			Assert.Null(mat.DiffuseTexture);

			// Since different settings produce different cache keys
			// manager should hold 2 models
			var modelCount = (from pair in manager.Cache where pair.Value is DrModel select pair.Key).Count();
			Assert.Equal(2, modelCount);
		}
	}
}
