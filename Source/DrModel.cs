using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents a 3D model with bones, meshes, and animations.
	/// </summary>
	public class DrModel : DrDisposable
	{
		/// <summary>
		/// Gets the root bone of the model's skeleton.
		/// </summary>
		public DrModelBone Root { get; }

		/// <summary>
		/// Gets an array of all bones in the model in depth-first traversal order.
		/// </summary>
		public DrModelBone[] Bones { get; }

		/// <summary>
		/// Gets an array of all meshes in the model.
		/// </summary>
		public DrMesh[] Meshes { get; }

		/// <summary>
		/// Gets or sets the collection of animations associated with this model.
		/// </summary>
		public Dictionary<string, AnimationClip> Animations { get; set; }

		/// <summary>
		/// Gets or sets an arbitrary object associated with this model.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DrModel"/> class with the specified root bone.
		/// </summary>
		/// <param name="root">The root bone of the skeleton.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is null.</exception>
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

		/// <summary>
		/// Releases the unmanaged resources used by this object, and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

		private static void TraverseBones(DrModelBone root, Action<DrModelBone> action)
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

		/// <summary>
		/// Copies the default local bone transforms from the model to the specified array.
		/// </summary>
		/// <param name="boneTransforms">The array to receive the bone transforms. Must have at least as many elements as the model has bones.</param>
		/// <exception cref="ArgumentNullException"><paramref name="boneTransforms"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="boneTransforms"/> is too small.</exception>
		public void CopyBoneTransformsTo(Matrix[] boneTransforms)
		{
			ValidateTransforms(boneTransforms);

			for (var i = 0; i < Bones.Length; i++)
			{
				var bone = Bones[i];
				boneTransforms[bone.Index] = bone.CalculateDefaultLocalTransform();
			}
		}

		/// <summary>
		/// Copies bone transforms from the specified array to the model's default poses.
		/// </summary>
		/// <param name="boneTransforms">The array containing bone transforms. Must have at least as many elements as the model has bones.</param>
		/// <exception cref="ArgumentNullException"><paramref name="boneTransforms"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="boneTransforms"/> is too small.</exception>
		public void CopyBoneTransformsFrom(Matrix[] boneTransforms)
		{
			ValidateTransforms(boneTransforms);

			for (var i = 0; i < Bones.Length; i++)
			{
				var bone = Bones[i];
				bone.DefaultPose = new SrtTransform(boneTransforms[i]);
			}
		}

		/// <summary>
		/// Copies the absolute (world-space) bone transforms from the model to the specified array.
		/// </summary>
		/// <param name="boneTransforms">The array to receive the absolute bone transforms. Must have at least as many elements as the model has bones.</param>
		/// <exception cref="ArgumentNullException"><paramref name="boneTransforms"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="boneTransforms"/> is too small.</exception>
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
		/// Finds a bone by name in the model's skeleton.
		/// </summary>
		/// <param name="name">The name of the bone to find.</param>
		/// <returns>The bone with the specified name, or null if no such bone exists.</returns>
		public DrModelBone FindBoneByName(string name) => (from bone in Bones where bone.Name == name select bone).FirstOrDefault();

		/// <summary>
		/// Creates a bone filter from bone names, including all descendants of matching bones.
		/// </summary>
		/// <param name="names">The names of bones to include in the filter.</param>
		/// <returns>A set of bone indices corresponding to the specified bones and their descendants.</returns>
		/// <exception cref="Exception">One of the specified bone names does not exist.</exception>
		public HashSet<int> CreateBoneFilter(params string[] names)
		{
			var result = new HashSet<int>();
			if (names == null || names.Length == 0)
			{
				return result;
			}

			foreach (var name in names)
			{
				var bone = FindBoneByName(name);

				if (bone == null)
				{
					throw new Exception($"Could not find bone '{name}'");
				}

				bone.InternalCreateBoneFilterRecursive(result);
			}

			return result;
		}

		/// <summary>
		/// Creates a bone filter containing all bones except those specified.
		/// </summary>
		/// <param name="excludedBones">The set of bone indices to exclude from the filter.</param>
		/// <returns>A set of bone indices containing all bones not in the excluded set.</returns>
		public HashSet<int> CreateInverseBoneFilter(HashSet<int> excludedBones)
		{
			var result = new HashSet<int>();

			for (var i = 0; i < Bones.Length; i++)
			{
				if (excludedBones == null || !excludedBones.Contains(i))
				{
					result.Add(i);
				}
			}

			return result;
		}

		/// <summary>
		/// Clears the tag property from all bones, meshes, and buffers in the model.
		/// </summary>
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