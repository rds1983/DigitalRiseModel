using System;
using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents a single joint in a skin, mapping a bone to an inverse bind transform.
	/// </summary>
	public class DrSkinJoint
	{
		/// <summary>
		/// Gets the bone that this joint maps to.
		/// </summary>
		public DrModelBone Bone { get; }

		/// <summary>
		/// Gets the inverse bind transform matrix for this joint.
		/// </summary>
		public Matrix InverseBindTransform { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DrSkinJoint"/> class.
		/// </summary>
		/// <param name="bone">The bone that this joint maps to.</param>
		/// <param name="inverseBindTransform">The inverse bind transform matrix.</param>
		/// <exception cref="ArgumentNullException"><paramref name="bone"/> is null.</exception>
		public DrSkinJoint(DrModelBone bone, Matrix inverseBindTransform)
		{
			Bone = bone ?? throw new ArgumentNullException(nameof(bone));
			InverseBindTransform = inverseBindTransform;
		}

		/// <summary>
		/// Returns the name of the bone that this joint maps to.
		/// </summary>
		/// <returns>The name of the bone.</returns>
		public override string ToString() => Bone.ToString();
	}

	/// <summary>
	/// Represents a skin that defines skeletal animation information for mesh deformation.
	/// </summary>
	public class DrSkin
	{
		/// <summary>
		/// Gets or sets the name of this skin.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the joints in this skin.
		/// </summary>
		public DrSkinJoint[] Joints { get; }

		/// <summary>
		/// Gets the index of this skin in the model.
		/// </summary>
		public int SkinIndex { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DrSkin"/> class.
		/// </summary>
		/// <param name="skinIndex">The index of this skin. Must be non-negative.</param>
		/// <param name="joints">The joints in this skin. Must contain at least one joint.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="skinIndex"/> is negative.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="joints"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="joints"/> is empty.</exception>
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