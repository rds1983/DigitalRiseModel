using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel
{
	public class DrModel: DrDisposable
	{
		public DrModelBone Root { get; }

		public DrModelBone[] Bones { get; }

		public DrMesh[] Meshes { get; }

		public Dictionary<string, AnimationClip> Animations { get; set; }

		public object Tag { get; set; }

		/// <summary>
		/// Creates a new DrModel
		/// </summary>
		/// <param name="root">Root Bone</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public DrModel(DrModelBone root)
		{
			if (root == null)
			{
				throw new ArgumentNullException(nameof(root));
			}

			Root = root;

			// Set bone indexes and build correct traverse order starting from root
			var traverseOrder = new List<DrModelBone>();
			var boneIndex = 0;
			TraverseBones(bone =>
			{
				bone.Index = boneIndex;
				traverseOrder.Add(bone);
				++boneIndex;
			});

			Bones = traverseOrder.ToArray();
			Meshes = (from b in Bones where b.Mesh != null select b.Mesh).ToArray();
		}

		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Meshes != null)
				{
					foreach (var mesh in Meshes)
					{
						mesh.Dispose();
					}
				}
			}
		}

		private void TraverseBones(DrModelBone root, Action<DrModelBone> action)
		{
			action(root);

			if (root.Children == null)
			{
				return;
			}

			foreach (var child in root.Children)
			{
				TraverseBones(child, action);
			}
		}

		private void TraverseBones(Action<DrModelBone> action)
		{
			TraverseBones(Root, action);
		}

		private void ValidateTransforms(Matrix[] boneTransforms)
		{
			if (boneTransforms == null)
			{
				throw new ArgumentNullException(nameof(boneTransforms));
			}

			if (boneTransforms.Length < Bones.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(boneTransforms));
			}
		}

		public void CopyBoneTransformsTo(Matrix[] boneTransforms)
		{
			ValidateTransforms(boneTransforms);

			for (var i = 0; i < Bones.Length; i++)
			{
				var bone = Bones[i];
				boneTransforms[bone.Index] = bone.CalculateDefaultLocalTransform();
			}
		}

		public void CopyBoneTransformsFrom(Matrix[] boneTransforms)
		{
			ValidateTransforms(boneTransforms);

			for (var i = 0; i < Bones.Length; i++)
			{
				var bone = Bones[i];
				bone.DefaultPose = new SrtTransform(boneTransforms[i]);
			}
		}

		public void CopyAbsoluteBoneTransformsTo(Matrix[] boneTransforms)
		{
			if (boneTransforms == null)
			{
				throw new ArgumentNullException(nameof(boneTransforms));
			}

			if (boneTransforms.Length < Bones.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(boneTransforms));
			}

			for (var i = 0; i < Bones.Length; i++)
			{
				var bone = Bones[i];

				if (bone.Parent == null)
				{
					boneTransforms[bone.Index] = bone.CalculateDefaultLocalTransform();
				}
				else
				{
					boneTransforms[bone.Index] = bone.CalculateDefaultLocalTransform() * boneTransforms[bone.Parent.Index];
				}
			}
		}

		/// <summary>
		/// Returns null if bone could not be found
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public DrModelBone FindBoneByName(string name) => (from bone in Bones where bone.Name == name select bone).FirstOrDefault();
	}
}