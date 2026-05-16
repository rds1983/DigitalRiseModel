using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel.Utility
{
	internal class VertexBufferBoundingBoxData
	{
		private readonly Vector3[] _positions;
		private readonly byte[,] _bonesIndices;
		private readonly float[,] _bonesWeights;

		public int Length => _positions.Length;

		public VertexBufferBoundingBoxData(int count, bool skinned)
		{
			_positions = new Vector3[count];
			if (skinned)
			{
				_bonesIndices = new byte[count, 4];
				_bonesWeights = new float[count, 4];
			}
		}

		public void SetPosition(int index, Vector3 position)
		{
			_positions[index] = position;
		}

		public void SetBonesIndices(int index, Vector4 v)
		{
			_bonesIndices[index, 0] = (byte)v.X;
			_bonesIndices[index, 1] = (byte)v.Y;
			_bonesIndices[index, 2] = (byte)v.Z;
			_bonesIndices[index, 3] = (byte)v.W;
		}

		public void SetBoneIndex(int index, int bone, byte val)
		{
			_bonesIndices[index, bone] = val;
		}

		public void SetBonesWeights(int index, Vector4 w)
		{
			_bonesWeights[index, 0] = w.X;
			_bonesWeights[index, 1] = w.Y;
			_bonesWeights[index, 2] = w.Z;
			_bonesWeights[index, 3] = w.W;
		}

		public void SetBoneWeight(int index, int bone, float val)
		{
			_bonesWeights[index, bone] = val;
		}

		public BoundingBox CalculateNonSkinned(uint[] indices)
		{
			var positions = new List<Vector3>();
			for (var i = 0; i < indices.Length; ++i)
			{
				var idx = indices[i];
				var pos = _positions[idx];
				positions.Add(pos);
			}

			return BoundingBox.CreateFromPoints(positions);
		}

		public BoundingBox CalculateSkinned(uint[] indices, DrSkin skin, Matrix[] absoluteTransforms)
		{
			var skinMatrices = (from j in skin.Joints select j.InverseBindTransform * absoluteTransforms[j.Bone.Index]).ToArray();

			var positions = new List<Vector3>();
			for (var i = 0; i < indices.Length; ++i)
			{
				var idx = indices[i];

				var transform = new Matrix();
				transform += skinMatrices[_bonesIndices[idx, 0]] * _bonesWeights[idx, 0];
				transform += skinMatrices[_bonesIndices[idx, 1]] * _bonesWeights[idx, 1];
				transform += skinMatrices[_bonesIndices[idx, 2]] * _bonesWeights[idx, 2];
				transform += skinMatrices[_bonesIndices[idx, 3]] * _bonesWeights[idx, 3];

				var pos = _positions[idx];
				var v = Vector3.Transform(pos, transform);
				positions.Add(v);
			}

			return BoundingBox.CreateFromPoints(positions);
		}
	}
}
