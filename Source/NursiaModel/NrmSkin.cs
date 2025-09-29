using System;
using Microsoft.Xna.Framework;

namespace NursiaModel
{
	public class NrmSkinJoint
	{
		public NrmModelBone Bone { get; }
		public Matrix InverseBindTransform { get; }

		public NrmSkinJoint(NrmModelBone bone, Matrix inverseBindTransform)
		{
			Bone = bone ?? throw new ArgumentNullException(nameof(bone));
			InverseBindTransform = inverseBindTransform;
		}

		public override string ToString() => Bone.ToString();
	}

	public class NrmSkin
	{
		public string Name { get; set; }
		public NrmSkinJoint[] Joints { get; }
		public int SkinIndex { get; private set; }

		/// <summary>
		/// Creates a skin from array of joints
		/// </summary>
		/// <param name="skinIndex"></param>
		/// <param name="joints"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public NrmSkin(int skinIndex, NrmSkinJoint[] joints)
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