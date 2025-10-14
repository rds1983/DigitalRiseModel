using AssetManagementBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DigitalRiseModel.Tests
{
	internal static class Utility
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		/// <summary>
		/// Compares two floating point numbers based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="left">The first number to compare.</param>
		/// <param name="right">The second number to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is within epsilon of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this float left, float right, float epsilon = ZeroTolerance)
		{
			return Math.Abs(left - right) <= epsilon;
		}

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static AssetManager CreateAssetManager()
		{
			return AssetManager.CreateFileAssetManager(Path.Combine(Utility.ExecutingAssemblyDirectory, "Assets/Models"));
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


		public static int CalculateStride(this IEnumerable<VertexElement> elements)
		{
			var result = 0;

			foreach (var channel in elements)
			{
				result += channel.VertexElementFormat.GetSize();
			}

			return result;
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

		public static int GetSize(this IndexElementSize indexType)
		{
			switch (indexType)
			{
				case IndexElementSize.SixteenBits:
					return 2;
				case IndexElementSize.ThirtyTwoBits:
					return 4;
			}

			throw new Exception($"Unknown index buffer type {indexType}");
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

		public static void CompareData(object[][] data1, object[][] data2)
		{
			Assert.AreEqual(data1.Length, data2.Length);

			for (var i = 0; i < data1.Length; ++i)
			{
				var row1 = data1[i];
				var row2 = data2[i];

				Assert.AreEqual(row1.Length, row2.Length);

				for (var j = 0; j < row1.Length; ++j)
				{
					var v1 = (float)row1[j];
					var v2 = (float)row1[j];

					Assert.AreEqual(v1, v2, ZeroTolerance);
				}
			}
		}

		public static VertexElement? FindElement(this VertexDeclaration vd, VertexElementUsage usage)
		{
			var ve = vd.GetVertexElements();
			for (var i = 0; i < ve.Length; ++i)
			{
				if (ve[i].VertexElementUsage == usage)
				{
					return ve[i];
				}
			}

			return null;
		}

		public static VertexElement EnsureElement(this VertexDeclaration vd, VertexElementUsage usage)
		{
			var result = vd.FindElement(usage);

			if (result == null)
			{
				throw new Exception($"Could not find vertex element with usage {usage}");
			}

			return result.Value;
		}
	}
}
