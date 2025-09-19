using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Utility
{
	internal static class XNA
	{
		public static int CalculateStride(this IEnumerable<VertexElement> elements)
		{
			var result = 0;

			foreach (var channel in elements)
			{
				result += channel.VertexElementFormat.GetSize();
			}

			return result;
		}

		public static int GetSize(this VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}

			throw new Exception($"Unknown vertex element format {elementFormat}");
		}

		public static int CalculateElementsCount(this IEnumerable<VertexElement> elements)
		{
			var result = 0;

			foreach (var channel in elements)
			{
				result += channel.VertexElementFormat.GetElementsCount();
			}

			return result;
		}

		public static int GetElementsCount(this VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 1;
				case VertexElementFormat.Vector2:
					return 2;
				case VertexElementFormat.Vector3:
					return 3;
				case VertexElementFormat.Vector4:
					return 4;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 2;
				case VertexElementFormat.Short4:
					return 4;
			}

			throw new Exception($"Unknown vertex element format {elementFormat}");
		}

		/// <summary>
		/// Used for debugging purposes
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static object[][] To2DArray(this VertexBuffer buffer)
		{
			var elements = buffer.VertexDeclaration.GetVertexElements();
			var elementsCount = elements.CalculateElementsCount();

			var result = new object[buffer.VertexCount][];
			for (var i = 0; i < result.Length; ++i)
			{
				result[i] = new object[elementsCount];
			}

			var stride = elements.CalculateStride();
			var data = new byte[buffer.VertexCount * stride];

			buffer.GetData(data);

			unsafe
			{
				fixed (byte* sptr = data)
				{
					var ptr = sptr;

					for (var i = 0; i < buffer.VertexCount; ++i)
					{
						var idx = 0;
						for (var j = 0; j < elements.Length; ++j)
						{
							var element = elements[j];
							var format = element.VertexElementFormat;

							for (var k = 0; k < format.GetElementsCount(); ++k)
							{
								if (format == VertexElementFormat.Byte4 || format == VertexElementFormat.Color)
								{
									var b = *ptr;
									result[i][idx] = b;

									++ptr;
								}
								else if (format == VertexElementFormat.Short2 || format == VertexElementFormat.Short4)
								{
									short s;
									Buffer.MemoryCopy(ptr, &s, 2, 2);
									result[i][idx] = s;

									ptr += 2;
								}
								else
								{
									float f;
									Buffer.MemoryCopy(ptr, &f, 4, 4);
									result[i][idx] = f;

									ptr += 4;
								}

								++idx;
							}
						}
					}
				}

				return result;
			}
		}
	}
}
