using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel
{
	public class NrmModel : NrmDisposable
	{
		public NrmModelBone Root { get; }

		public NrmModelBone[] Bones { get; }

		public NrmMesh[] Meshes { get; }

		public Dictionary<string, AnimationClip> Animations { get; set; }

		public object Tag { get; set; }

		/// <summary>
		/// Creates a new DrModel
		/// </summary>
		/// <param name="root">Root Bone</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public NrmModel(NrmModelBone root)
		{
			if (root == null)
			{
				throw new ArgumentNullException(nameof(root));
			}

			Root = root;

			// Set bone indexes and build correct traverse order starting from root
			var traverseOrder = new List<NrmModelBone>();
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

		private static void TraverseBones(NrmModelBone root, Action<NrmModelBone> action)
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

		private void TraverseBones(Action<NrmModelBone> action)
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
		public NrmModelBone FindBoneByName(string name) => (from bone in Bones where bone.Name == name select bone).FirstOrDefault();

		public void ClearAllTags()
		{
			foreach (var bone in Bones)
			{
				bone.Tag = null;
			}

			foreach (var mesh in Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					part.Tag = null;

					part.VertexBuffer.Tag = null;

					if (part.IndexBuffer != null)
					{
						part.IndexBuffer.Tag = null;
					}
				}

				mesh.Tag = null;
			}
		}
	}
}