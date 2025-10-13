using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace DigitalRiseModel.Utility
{
	internal class TangentsGenerator
	{
		private struct InputRow
		{
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 UV;
		}

		private readonly InputRow[] _rows;
		private uint[] _indices;

		public TangentsGenerator(int size)
		{
			_rows = new InputRow[size];
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

		public void CalculateTangentFrames(out Vector3[] tangents, out Vector3[] bitangents)
		{
			// Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. 
			// Terathon Software 3D Graphics Library, 2001.
			// http://www.terathon.com/code/tangent.html

			// Hegde, Siddharth. "Messing with Tangent Space". Gamasutra, 2007. 
			// http://www.gamasutra.com/view/feature/129939/messing_with_tangent_space.php

			var numVerts = _rows.Length;
			var numIndices = _indices.Length;

			var tan1 = new Vector3[numVerts];
			var tan2 = new Vector3[numVerts];

			for (var index = 0; index < numIndices; index += 3)
			{
				var i1 = _indices[index + 0];
				var i2 = _indices[index + 1];
				var i3 = _indices[index + 2];

				var w1 = _rows[i1].UV;
				var w2 = _rows[i2].UV;
				var w3 = _rows[i3].UV;

				var s1 = w2.X - w1.X;
				var s2 = w3.X - w1.X;
				var t1 = w2.Y - w1.Y;
				var t2 = w3.Y - w1.Y;

				var denom = s1 * t2 - s2 * t1;
				if (Math.Abs(denom) < float.Epsilon)
				{
					// The triangle UVs are zero sized one dimension.
					//
					// So we cannot calculate the vertex tangents for this
					// one trangle, but maybe it can with other trangles.
					continue;
				}

				var r = 1.0f / denom;
				Debug.Assert(r.IsFinite(), "Bad r!");

				var v1 = _rows[i1].Position;
				var v2 = _rows[i2].Position;
				var v3 = _rows[i3].Position;

				var x1 = v2.X - v1.X;
				var x2 = v3.X - v1.X;
				var y1 = v2.Y - v1.Y;
				var y2 = v3.Y - v1.Y;
				var z1 = v2.Z - v1.Z;
				var z2 = v3.Z - v1.Z;

				var sdir = new Vector3()
				{
					X = (t2 * x1 - t1 * x2) * r,
					Y = (t2 * y1 - t1 * y2) * r,
					Z = (t2 * z1 - t1 * z2) * r,
				};

				var tdir = new Vector3()
				{
					X = (s1 * x2 - s2 * x1) * r,
					Y = (s1 * y2 - s2 * y1) * r,
					Z = (s1 * z2 - s2 * z1) * r,
				};

				tan1[i1] += sdir;
				Debug.Assert(tan1[i1].IsFinite(), "Bad tan1[i1]!");
				tan1[i2] += sdir;
				Debug.Assert(tan1[i2].IsFinite(), "Bad tan1[i2]!");
				tan1[i3] += sdir;
				Debug.Assert(tan1[i3].IsFinite(), "Bad tan1[i3]!");

				tan2[i1] += tdir;
				Debug.Assert(tan2[i1].IsFinite(), "Bad tan2[i1]!");
				tan2[i2] += tdir;
				Debug.Assert(tan2[i2].IsFinite(), "Bad tan2[i2]!");
				tan2[i3] += tdir;
				Debug.Assert(tan2[i3].IsFinite(), "Bad tan2[i3]!");
			}

			tangents = new Vector3[numVerts];
			bitangents = new Vector3[numVerts];

			// At this point we have all the vectors accumulated, but we need to average
			// them all out. So we loop through all the final verts and do a Gram-Schmidt
			// orthonormalize, then make sure they're all unit length.
			for (var i = 0; i < numVerts; i++)
			{
				var n = _rows[i].Normal;
				Debug.Assert(n.IsFinite(), "Bad normal! Normal vector must be finite.");
				Debug.Assert(n.Length() >= 0.9999f, "Bad normal! Normal vector must be normalized. (Actual length = " + n.Length() + ")");

				var t = tan1[i];
				if (t.LengthSquared() < float.Epsilon)
				{
					// TODO: Ideally we could spit out a warning to the
					// content logging here!

					// We couldn't find a good tanget for this vertex.
					//
					// Rather than set them to zero which could produce
					// errors in other parts of the pipeline, we just take        
					// a guess at something that may look ok.

					t = Vector3.Cross(n, Vector3.UnitX);
					if (t.LengthSquared() < float.Epsilon)
						t = Vector3.Cross(n, Vector3.UnitY);

					tangents[i] = Vector3.Normalize(t);
					bitangents[i] = Vector3.Cross(n, tangents[i]);
					continue;
				}

				// Gram-Schmidt orthogonalize
				// TODO: This can be zero can cause NaNs on 
				// normalize... how do we fix this?
				var tangent = t - n * Vector3.Dot(n, t);
				tangent = Vector3.Normalize(tangent);
				Debug.Assert(tangent.IsFinite(), "Bad tangent!");
				tangents[i] = tangent;

				// Calculate handedness
				var w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0F) ? -1.0F : 1.0F;
				Debug.Assert(w.IsFinite(), "Bad handedness!");

				// Calculate the bitangent
				var bitangent = Vector3.Cross(n, tangent) * w;
				Debug.Assert(bitangent.IsFinite(), "Bad bitangent!");
				bitangents[i] = bitangent;
			}
		}
	}
}
