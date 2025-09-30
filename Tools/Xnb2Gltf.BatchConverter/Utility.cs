using System.Collections.Generic;
using System;
using glTFLoader.Schema;
using static glTFLoader.Schema.Accessor;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace BatchConverter
{
	internal static class Utility
	{
		private unsafe static int WriteData<T>(this Stream output, List<BufferView> bufferViews, List<Accessor> accessors, void* ptr, int length, ComponentTypeEnum te, TypeEnum te2)
		{
			var pos = (int)output.Position;

			// Write data to the binary buffer
			var bytes = new byte[length * Marshal.SizeOf(typeof(T))];
			Marshal.Copy(new IntPtr(ptr), bytes, 0, bytes.Length);

			output.Write(bytes);

			// Create new buffer view
			bufferViews.Add(new BufferView { ByteOffset = pos, ByteLength = bytes.Length });

			// Create new accessor
			accessors.Add(new Accessor
			{
				ComponentType = te,
				Type = te2,
				Count = length,
				BufferView = bufferViews.Count - 1,
			});

			return accessors.Count - 1;
		}

		public unsafe static int WriteData(this Stream output, List<BufferView> bufferViews, List<Accessor> accessors, Vector2[] data)
		{
			fixed (void* ptr = data)
			{
				return output.WriteData<Vector2>(bufferViews, accessors, ptr, data.Length, ComponentTypeEnum.FLOAT, TypeEnum.VEC2);
			}
		}

		public unsafe static int WriteData(this Stream output, List<BufferView> bufferViews, List<Accessor> accessors, Vector3[] data)
		{
			fixed (void* ptr = data)
			{
				return output.WriteData<Vector3>(bufferViews, accessors, ptr, data.Length, ComponentTypeEnum.FLOAT, TypeEnum.VEC3);
			}
		}

		public unsafe static int WriteData(this Stream output, List<BufferView> bufferViews, List<Accessor> accessors, Vector4[] data)
		{
			fixed (void* ptr = data)
			{
				return output.WriteData<Vector4>(bufferViews, accessors, ptr, data.Length, ComponentTypeEnum.FLOAT, TypeEnum.VEC4);
			}
		}

		public unsafe static int WriteData(this Stream output, List<BufferView> bufferViews, List<Accessor> accessors, ushort[] data)
		{
			fixed (void* ptr = data)
			{
				return output.WriteData<ushort>(bufferViews, accessors, ptr, data.Length, ComponentTypeEnum.UNSIGNED_SHORT, TypeEnum.SCALAR);
			}
		}

		public static float[] ToFloats(this Matrix m) =>
			new float[] {
						m.M11, m.M12, m.M13, m.M14,
						m.M21, m.M22, m.M23, m.M24,
						m.M31, m.M32, m.M33, m.M34,
						m.M41, m.M42, m.M43, m.M44,
			};

		public static void Unwind<T>(this T[] data) where T : struct
		{
			for (var i = 0; i < data.Length / 3; i++)
			{
				var temp = data[i * 3];
				data[i * 3] = data[i * 3 + 2];
				data[i * 3 + 2] = temp;
			}
		}
	}
}