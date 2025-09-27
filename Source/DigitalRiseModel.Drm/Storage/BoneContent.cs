using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DigitalRiseModel.Storage
{
	internal class BoneContent
	{
		public string Name { get; set; }
		public MeshContent Mesh { get; set; }
		public Vector3 Scale = Vector3.One;
		public Quaternion Rotation = Quaternion.Identity;
		public Vector3 Translation = Vector3.Zero;
		public SkinContent Skin { get; set; }

		public List<int> Children { get; set; }
	}
}
