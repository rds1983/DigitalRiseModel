using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NursiaModel.Utility
{
	internal class SkinnedVertexInfo
	{
		private readonly Vector3[] _positions;
		private readonly byte[,] _indices;
		private readonly float[,] _weights;

		public int Length => _positions.Length;

		public SkinnedVertexInfo(int count)
		{
			_positions = new Vector3[count];
			_indices = new byte[count, 4];
			_weights = new float[count, 4];
		}

		public void SetPosition(int index, Vector3 position)
		{
			_positions[index] = position;
		}

		public void SetIndices(int index, Vector4 v)
		{
			_indices[index, 0] = (byte)v.X;
			_indices[index, 1] = (byte)v.Y;
			_indices[index, 2] = (byte)v.Z;
			_indices[index, 3] = (byte)v.W;
		}

		public void SetIndex(int index, int bone, byte val)
		{
			_indices[index, bone] = val;
		}

		public void SetWeights(int index, Vector4 w)
		{
			_weights[index, 0] = w.X;
			_weights[index, 1] = w.Y;
			_weights[index, 2] = w.Z;
			_weights[index, 3] = w.W;
		}

		public void SetWeight(int index, int bone, float val)
		{
			_weights[index, bone] = val;
		}

		public BoundingBox CalculateBoundingBox(NrmModel model, NrmMeshPart part, Matrix[] absoluteTransforms)
		{
			if (absoluteTransforms == null)
			{
				absoluteTransforms = new Matrix[model.Bones.Length];
				model.CopyAbsoluteBoneTransformsTo(absoluteTransforms);
			}

			var skinMatrices = (from j in part.Skin.Joints select j.InverseBindTransform * absoluteTransforms[j.Bone.Index]).ToArray();
			var positions = new List<Vector3>();

			var skinnedVertexesInfo = (SkinnedVertexInfo)part.VertexBuffer.Tag;
			var indices = (uint[])part.IndexBuffer.Tag;

			for (var i = 0; i < indices.Length; ++i)
			{
				var idx = indices[i];

				var transform = new Matrix();
				transform += skinMatrices[_indices[idx, 0]] * _weights[idx, 0];
				transform += skinMatrices[_indices[idx, 1]] * _weights[idx, 1];
				transform += skinMatrices[_indices[idx, 2]] * _weights[idx, 2];
				transform += skinMatrices[_indices[idx, 3]] * _weights[idx, 3];

				var pos = _positions[idx];
				var v = Vector3.Transform(pos, transform);
				positions.Add(v);
			}

			return BoundingBox.CreateFromPoints(positions);
		}
	}

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

		public static void UpdateBoundingBoxesForSkinnedModel(this NrmModel model)
		{
			Matrix[] absoluteTransforms = null;
			foreach (var mesh in model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					if (part.Skin == null)
					{
						continue;
					}

					var skinnedVertexesInfo = (SkinnedVertexInfo)part.VertexBuffer.Tag;
					part.BoundingBox = skinnedVertexesInfo.CalculateBoundingBox(model, part, absoluteTransforms);
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
