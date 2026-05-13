using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DigitalRiseModel.Tests
{
	[TestClass]
	public sealed class LoadTests
	{
		private static void VerifyBoneHierarchy(DrModelBone bone, int depth = 0)
		{
			Assert.IsNotNull(bone, "Bone should not be null");
			Assert.IsNotNull(bone.Name, "Bone should have a name");

			if (bone.Children != null)
			{
				foreach (var child in bone.Children)
				{
					Assert.IsNotNull(child, "Child bone should not be null");
					Assert.AreEqual(bone, child.Parent, $"Child bone '{child.Name}' should have correct parent reference");
					VerifyBoneHierarchy(child, depth + 1);
				}
			}
		}

		private static void TestDudeModel(DrModel model, string rootName, int bonesCount, bool readableBuffers)
		{
			Assert.IsNotNull(model.Root);
			Assert.AreEqual(rootName, model.Root.Name);
			Assert.IsNotNull(model.Bones);
			Assert.AreEqual(bonesCount, model.Bones.Length);
			Assert.IsNotNull(model.Meshes);
			Assert.AreEqual(1, model.Meshes.Length);
			Assert.IsNotNull(model.Animations);
			Assert.AreEqual(1, model.Animations.Count);

			var firstPart = model.Meshes[0].MeshParts[0];
			var data = new byte[firstPart.VertexBuffer.VertexCount * firstPart.VertexBuffer.VertexDeclaration.VertexStride];

			if (readableBuffers)
			{
				firstPart.VertexBuffer.GetData(data);
			}
			else
			{
				Assert.ThrowsException<NotSupportedException>(() =>
				{
					firstPart.VertexBuffer.GetData(data);
				});
			}
		}

		[TestMethod]
		[DataRow("dude.gltf")]
		[DataRow("dude.glb")]
		public void LoadDudeModel(string file, string rootName = "RootNode", int bonesCount = 60)
		{
			var manager = Utility.CreateAssetManager();
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, ModelLoadFlags.IgnoreMaterials);
			TestDudeModel(model, rootName, bonesCount, false);

			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, ModelLoadFlags.IgnoreMaterials | ModelLoadFlags.ReadableBuffers);
			TestDudeModel(model, rootName, bonesCount, true);
		}

		[TestMethod]
		public void TestIgnoreMaterials()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "CesiumMilkTruck.gltf");

			var mat = model.Meshes[0].MeshParts[0].Material;
			Assert.IsNotNull(mat.DiffuseTexture);
			Assert.AreEqual("CesiumMilkTruck.jpg", mat.DiffuseTexture.Name);
			Assert.AreEqual(2048, mat.DiffuseTexture.Width);

			// No materials
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "CesiumMilkTruck.gltf", ModelLoadFlags.IgnoreMaterials);
			mat = model.Meshes[0].MeshParts[0].Material;
			Assert.IsNull(mat.DiffuseTexture);

			// Since different settings produce different cache keys
			// manager should hold 2 models
			var modelCount = (from pair in manager.Cache where pair.Value is DrModel select pair.Key).Count();
			Assert.AreEqual(2, modelCount);
		}

		[TestMethod]
		public void TestEnsureUVs()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "BrainStem.glb", ModelLoadFlags.IgnoreMaterials);

			// Make sure there isn't uv
			var el = model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.FindElement(Microsoft.Xna.Framework.Graphics.VertexElementUsage.TextureCoordinate);
			Assert.IsNull(el, "This model should lack uv channel");

			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "BrainStem.glb", ModelLoadFlags.IgnoreMaterials | ModelLoadFlags.EnsureUVs);

			// Now there should be uv
			el = model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.FindElement(Microsoft.Xna.Framework.Graphics.VertexElementUsage.TextureCoordinate);
			Assert.IsNotNull(el, "This model should have uv now");
		}

		[TestMethod]
		public void TestEraseRootParent()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "alien.glb", ModelLoadFlags.IgnoreMaterials);

			Assert.IsNull(model.Root.Parent);
		}

		[TestMethod]
		public void TestEmbeddedTexture()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "orange_flower.glb");
		}

		[TestMethod]
		public void LoadSinbadModel()
		{
			var manager = Utility.CreateAssetManager();

			// Load Sinbad model with multiple roots
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "Sinbad.glb", ModelLoadFlags.IgnoreMaterials);

			// Verify model loaded successfully
			Assert.IsNotNull(model, "Model should load successfully");
			Assert.IsNotNull(model.Root, "Model should have a root bone");
			Assert.IsNotNull(model.Bones, "Model should have bones");
			Assert.IsNotNull(model.Meshes, "Model should have meshes");

			// Sinbad has 3 root nodes (70, 71, 72 in the scene), so FixRoot creates a synthetic "_Root" node
			Assert.AreEqual("_Root", model.Root.Name, "Root should be synthetic _Root due to multiple roots");

			// Verify root has 3 children (the original root nodes)
			Assert.IsNotNull(model.Root.Children, "Root should have children");
			Assert.AreEqual(3, model.Root.Children.Length, "Root should have exactly 3 children (original root nodes)");

			// Verify total bone count (73 nodes + 1 synthetic root = 74)
			Assert.AreEqual(74, model.Bones.Length, "Model should have 74 bones (73 nodes + 1 synthetic root)");

			// Verify all bones are properly indexed
			for (int i = 0; i < model.Bones.Length; i++)
			{
				Assert.AreEqual(i, model.Bones[i].Index, $"Bone at position {i} should have index {i}");
			}

			// Verify root has no parent
			Assert.IsNull(model.Root.Parent, "Root bone should have no parent");

			// Verify meshes are loaded
			Assert.IsTrue(model.Meshes.Length > 0, "Model should have at least one mesh");

			// Verify all meshes have mesh parts
			foreach (var mesh in model.Meshes)
			{
				Assert.IsNotNull(mesh.MeshParts, "Mesh should have mesh parts");
				Assert.IsTrue(mesh.MeshParts.Count > 0, "Mesh should have at least one mesh part");

				// Verify vertex buffers
				foreach (var part in mesh.MeshParts)
				{
					Assert.IsNotNull(part.VertexBuffer, "Mesh part should have vertex buffer");
					Assert.IsTrue(part.VertexBuffer.VertexCount > 0, "Vertex buffer should have vertices");
					Assert.IsNotNull(part.VertexBuffer.VertexDeclaration, "Vertex buffer should have vertex declaration");

					// Verify vertex elements
					var elements = part.VertexBuffer.VertexDeclaration.GetVertexElements();
					Assert.IsTrue(elements.Length > 0, "Vertex declaration should have elements");

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
					Assert.IsTrue(hasPosition, "Vertex declaration should include Position element");
				}
			}

			// Verify bone transforms can be copied
			var boneTransforms = new Microsoft.Xna.Framework.Matrix[model.Bones.Length];
			model.CopyBoneTransformsTo(boneTransforms);

			for (int i = 0; i < boneTransforms.Length; i++)
			{
				Assert.IsNotNull(boneTransforms[i], $"Bone transform at index {i} should not be null");
			}

			// Verify absolute transforms can be calculated
			var absoluteTransforms = new Microsoft.Xna.Framework.Matrix[model.Bones.Length];
			model.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

			for (int i = 0; i < absoluteTransforms.Length; i++)
			{
				Assert.IsNotNull(absoluteTransforms[i], $"Absolute transform at index {i} should not be null");
			}

			// Verify animations (if any)
			if (model.Animations != null && model.Animations.Count > 0)
			{
				foreach (var animation in model.Animations.Values)
				{
					Assert.IsNotNull(animation, "Animation should not be null");
					Assert.IsNotNull(animation.Channels, "Animation should have channels");
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
					Assert.IsTrue(bone.Parent != null || model.Root.Children.Contains(bone),
						$"Bone '{bone.Name}' should either have a parent or be a direct child of root");
				}
			}

			// Verify bone transforms have valid data
			for (int i = 0; i < model.Bones.Length; i++)
			{
				var bone = model.Bones[i];
				var localTransform = bone.CalculateDefaultLocalTransform();

				// Verify transform is not NaN or Infinity
				Assert.IsFalse(float.IsNaN(localTransform.M11), $"Bone '{bone.Name}' transform contains NaN");
				Assert.IsFalse(float.IsInfinity(localTransform.M11), $"Bone '{bone.Name}' transform contains Infinity");
			}

			// Verify mesh parts have valid bounding boxes
			foreach (var mesh in model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					var bb = part.BoundingBox;
					Assert.IsFalse(float.IsNaN(bb.Min.X), "Bounding box min contains NaN");
					Assert.IsFalse(float.IsInfinity(bb.Max.Y), "Bounding box max contains Infinity");
				}
			}

			// Load with readable buffers flag and verify data can be read
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "Sinbad.glb",
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
					Assert.IsTrue(hasNonZeroData, "Vertex buffer should contain non-zero data");
				}
			}
		}
	}
}
