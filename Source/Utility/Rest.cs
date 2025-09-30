using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace NursiaModel.Utility
{
	internal static class Rest
	{
		/// <summary>
		/// Safely disposes the object.
		/// </summary>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <param name="obj">The object to dispose. Can be <see langword="null"/>.</param>
		/// <remarks>
		/// The method calls <see cref="IDisposable.Dispose"/> if the <paramref name="obj"/> is not null
		/// and implements the interface <see cref="IDisposable"/>.
		/// </remarks>
		public static void SafeDispose<T>(this T obj) where T : class
		{
			var disposable = obj as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

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

		public static NrmModelBone FixRoot(this List<NrmModelBone> roots, NrmModelBone currentRoot)
		{
			if (roots.Count < 2)
			{
				return currentRoot;
			}

			// Multiple roots
			// Create one root to store it
			var newRoot = new NrmModelBone("_Root");

			var children = new List<NrmModelBone>();
			foreach (var root in roots)
			{
				children.Add(root);
			}

			newRoot.Children = children.ToArray();

			return newRoot;
		}

		public static void UpdateBoundingBoxes(this NrmModel model)
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
