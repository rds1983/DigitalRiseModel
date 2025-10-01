using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DigitalRiseModel.Utility
{
	internal static class Rest
	{
		/// <summary>
		/// Swaps the content of two variables.
		/// </summary>
		/// <typeparam name="T">The type of the objects.</typeparam>
		/// <param name="obj1">First variable.</param>
		/// <param name="obj2">Second variable.</param>
		public static void Swap<T>(ref T obj1, ref T obj2)
		{
			T temp = obj1;
			obj1 = obj2;
			obj2 = temp;
		}

		public static DrModelBone FixRoot(this List<DrModelBone> roots, DrModelBone currentRoot)
		{
			if (roots.Count < 2)
			{
				return currentRoot;
			}

			// Multiple roots
			// Create one root to store it
			var newRoot = new DrModelBone("_Root");

			var children = new List<DrModelBone>();
			foreach (var root in roots)
			{
				children.Add(root);
			}

			newRoot.Children = children.ToArray();

			return newRoot;
		}

		public static void UpdateBoundingBoxes(this DrModel model)
		{
			Matrix[] absoluteTransforms = null;
			foreach (var mesh in model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					var boundingBoxData = (VertexBufferBoundingBoxData)part.VertexBuffer.Tag;
					var indices = (uint[])part.IndexBuffer.Tag;
					if (part.Skin == null)
					{
						part.BoundingBox = boundingBoxData.CalculateNonSkinned(indices);
					}
					else
					{
						if (absoluteTransforms == null)
						{
							absoluteTransforms = new Matrix[model.Bones.Length];
							model.CopyAbsoluteBoneTransformsTo(absoluteTransforms);
						}

						part.BoundingBox = boundingBoxData.CalculateSkinned(indices, part.Skin, absoluteTransforms);
					}
				}
			}
		}

		public static uint[] ToUnsignedIntArray(this ushort[] data)
		{
			var result = new uint[data.Length];
			for (var i = 0; i < data.Length; ++i)
			{
				result[i] = data[i];
			}

			return result;
		}

		public static uint[] ToUnsignedIntArray(this short[] data)
		{
			var result = new uint[data.Length];
			for (var i = 0; i < data.Length; ++i)
			{
				result[i] = (uint)data[i];
			}

			return result;
		}
	}
}
