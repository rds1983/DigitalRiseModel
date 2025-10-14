using Microsoft.Xna.Framework;
using System;
using static MikkTSpaceSharp.MikkTSpace;

namespace DigitalRiseModel.Utility
{
	internal struct VertexElementData
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 UV;
	}

	internal static class TangentsCalc
	{
		public static SVec2 ToSVec2(this Vector2 v) => new SVec2(v.X, v.Y);
		public static SVec3 ToSVec3(this Vector3 v) => new SVec3(v.X, v.Y, v.Z);

		public static Vector4[] Calculate(VertexElementData[] vertices, uint[] indices)
		{
			var result = new Vector4[vertices.Length];
			Func<int, int, uint> indexCalc = (face, vertex) => indices[face * 3 + vertex];
			var ctx = new SMikkTSpaceContext
			{
				m_getNumFaces = () => indices.Length / 3,
				m_getNumVerticesOfFace = face => 3,
				m_getPosition = (face, vertex) => vertices[indexCalc(face, vertex)].Position.ToSVec3(),
				m_getNormal = (face, vertex) => vertices[indexCalc(face, vertex)].Normal.ToSVec3(),
				m_getTexCoord = (face, vertex) => vertices[indexCalc(face, vertex)].UV.ToSVec2(),
				m_setTSpaceBasic = (SVec3 tangent, float orient, int face, int vertex) =>
				{
					var idx = indexCalc(face, vertex);
					result[idx] = new Vector4(tangent.x, tangent.y, tangent.z, orient);
				}
			};

			var r = genTangSpaceDefault(ctx);
			if (r == 0)
			{
				throw new Exception("Tangents generation failed");
			}

			return result;
		}
	}

	internal class TangentsGenerator
	{
		private readonly VertexElementData[] _rows;
		private uint[] _indices;

		public TangentsGenerator(int size)
		{
			_rows = new VertexElementData[size];
		}

		public void SetPosition(int i, Vector3 position)
		{
			_rows[i].Position = position;
		}

		public void SetNormal(int i, Vector3 normal)
		{
			_rows[i].Normal = normal;
		}

		public void SetUV(int i, Vector2 uv)
		{
			_rows[i].UV = uv;
		}

		public void SetIndices(ushort[] data)
		{
			_indices = new uint[data.Length];

			for (var i = 0; i < data.Length; ++i)
			{
				_indices[i] = data[i];
			}
		}

		public void SetIndices(short[] data)
		{
			_indices = new uint[data.Length];

			for (var i = 0; i < data.Length; ++i)
			{
				_indices[i] = (uint)data[i];
			}
		}

		public void SetIndices(uint[] data)
		{
			_indices = data;
		}

		public Vector4[] CalculateTangentFrames() => TangentsCalc.Calculate(_rows, _indices);
	}
}
