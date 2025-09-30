using Microsoft.Xna.Framework.Graphics;
using NursiaModel.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NursiaModel.G3d
{
	internal static class G3dUtility
	{
		private class AttributeInfo
		{
			public int ElementsCount { get; }
			public VertexElementFormat Format { get; }
			public VertexElementUsage Usage { get; }

			public AttributeInfo(VertexElementFormat format, VertexElementUsage usage, int? elementsCount = null)
			{
				Format = format;
				Usage = usage;
				ElementsCount = elementsCount ?? format.GetElementsCount();
			}
		}

		private static readonly AttributeInfo[] _attributes;

		static G3dUtility()
		{
			_attributes = new AttributeInfo[Enum.GetValues(typeof(G3dAttribute)).Length];
			_attributes[(int)G3dAttribute.Position] = new AttributeInfo(VertexElementFormat.Vector3, VertexElementUsage.Position);
			_attributes[(int)G3dAttribute.Normal] = new AttributeInfo(VertexElementFormat.Vector3, VertexElementUsage.Normal);
			_attributes[(int)G3dAttribute.TexCoord] = new AttributeInfo(VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate);
			_attributes[(int)G3dAttribute.ColorPacked] = new AttributeInfo(VertexElementFormat.Color, VertexElementUsage.Color, 1);
			_attributes[(int)G3dAttribute.Color] = new AttributeInfo(VertexElementFormat.Color, VertexElementUsage.Color, 4);
			_attributes[(int)G3dAttribute.BlendWeight] = new AttributeInfo(VertexElementFormat.Vector2, VertexElementUsage.BlendWeight);
		}

		public static G3dAttribute FromName(string name)
		{
			var usage = 0;

			// Remove last digit
			var lastChar = name[name.Length - 1];
			if (char.IsDigit(lastChar))
			{
				name = name.Substring(0, name.Length - 1);
				usage = int.Parse(lastChar.ToString());
			}

			G3dAttribute result;
			if (!Enum.TryParse(name, true, out result))
			{
				throw new Exception($"Unknown attribute {name}");
			}

			return result;
		}

		public static int GetElementsCount(this G3dAttribute attr) => _attributes[(int)attr].ElementsCount;

		public static int CalculateElementsPerRow(this G3dAttribute[] attributes)
		{
			var result = 0;
			foreach (var attr in attributes)
			{
				result += attr.GetElementsCount();
			}

			return result;
		}

		public static VertexElementFormat GetFormat(this G3dAttribute attr) => _attributes[(int)attr].Format;
		public static VertexElementUsage GetUsage(this G3dAttribute attr) => _attributes[(int)attr].Usage;

		public static VertexDeclaration BuildDeclaration(this G3dAttribute[] attributes)
		{
			var bonesCount = (from a in attributes where a == G3dAttribute.BlendWeight select a).Count();
			if (bonesCount > 0 && bonesCount != 4)
			{
				throw new NotSupportedException("Only 4 bones per mesh is supported");
			}

			var elements = new List<VertexElement>();

			var offset = 0;
			var usages = new Dictionary<G3dAttribute, int>();
			var blendAdded = false;
			foreach (var attr in attributes)
			{
				if (attr != G3dAttribute.BlendWeight)
				{
					var info = _attributes[(int)attr];

					var usageIndex = 0;
					if (usages.ContainsKey(attr))
					{
						usageIndex = usages[attr];
					}

					var element = new VertexElement(offset, info.Format, info.Usage, usageIndex);
					elements.Add(element);

					++usageIndex;
					usages[attr] = usageIndex;
					offset += info.Format.GetSize();
				}
				else if (!blendAdded)
				{
					elements.Add(new VertexElement(offset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0));
					elements.Add(new VertexElement(offset + 4, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));

					offset += VertexElementFormat.Byte4.GetSize();
					offset += VertexElementFormat.Vector4.GetSize();

					blendAdded = true;
				}
			}

			return new VertexDeclaration(elements.ToArray());
		}
	}
}
