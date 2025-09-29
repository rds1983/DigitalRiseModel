using System;
using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	public class DrSkinJoint
	{
		public DrModelBone Bone { get; }
		public Matrix InverseBindTransform { get; }

		public DrSkinJoint(DrModelBone bone, Matrix inverseBindTransform)
		{
			Bone = bone ?? throw new ArgumentNullException(nameof(bone));
			InverseBindTransform = inverseBindTransform;
		}

		public override string ToString() => Bone.ToString();
	}

	public class DrSkin
	{
		public string Name { get; set; }
		public DrSkinJoint[] Joints { get; }
		public int SkinIndex { get; private set; }

		/// <summary>
		/// Creates a skin from array of joints
		/// </summary>
		/// <param name="skinIndex"></param>
		/// <param name="joints"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public DrSkin(int skinIndex, DrSkinJoint[] joints)
		{
			if (skinIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(skinIndex));
			}

			if (joints == null)
			{
				throw new ArgumentNullException(nameof(joints));
			}

			if (joints.Length == 0)
			{
				throw new ArgumentException(nameof(joints), "no joints");
			}

			SkinIndex = skinIndex;
			Joints = joints;
		}
	}
}