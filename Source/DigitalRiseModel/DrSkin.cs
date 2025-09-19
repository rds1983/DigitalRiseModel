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
	}

	public class DrSkin
	{
		public string Name { get; set; }
		public DrSkinJoint[] Joints { get; }
		public int SkinIndex { get; internal set; }

		/// <summary>
		/// Creates a skin from array of joints
		/// </summary>
		/// <param name="joints"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public DrSkin(DrSkinJoint[] joints)
		{
			if (joints == null)
			{
				throw new ArgumentNullException(nameof(joints));
			}

			if (joints.Length == 0)
			{
				throw new ArgumentException(nameof(joints), "no joints");
			}

			Joints = joints;
		}
	}
}