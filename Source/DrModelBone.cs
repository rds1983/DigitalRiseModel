using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents a bone in a model's skeleton.
	/// </summary>
	public class DrModelBone
	{
		private DrModelBone[] _children;

		/// <summary>
		/// Gets the name of this bone.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the index of this bone in the model's bones array.
		/// </summary>
		public int Index { get; internal set; }

		/// <summary>
		/// Gets or sets the default pose of this bone.
		/// </summary>
		public SrtTransform DefaultPose = SrtTransform.Identity;

		/// <summary>
		/// Gets the parent bone of this bone, or null if this is the root bone.
		/// </summary>
		public DrModelBone Parent { get; internal set; }

		/// <summary>
		/// Gets or sets the child bones of this bone.
		/// </summary>
		public DrModelBone[] Children
		{
			get => _children;

			set
			{
				if (_children != null)
				{
					foreach (var c in _children)
					{
						c.Parent = null;
					}
				}

				_children = value;

				if (_children != null)
				{
					foreach (var c in _children)
					{
						c.Parent = this;
					}
				}
			}
		}

		/// <summary>
		/// Gets the mesh attached to this bone, if any.
		/// </summary>
		public DrMesh Mesh { get; }

		/// <summary>
		/// Gets or sets an arbitrary object associated with this bone.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DrModelBone"/> class.
		/// </summary>
		/// <param name="name">The name of the bone.</param>
		/// <param name="mesh">The mesh to attach to this bone, or null if no mesh should be attached.</param>
		public DrModelBone(string name, DrMesh mesh = null)
		{
			Name = name;
			if (mesh != null)
			{
				Mesh = mesh;
				Mesh.ParentBone = this;
			}
		}

		/// <summary>
		/// Calculates the default local transformation matrix for this bone.
		/// </summary>
		/// <returns>The transformation matrix of the default pose.</returns>
		public Matrix CalculateDefaultLocalTransform() => DefaultPose.ToMatrix();

		/// <summary>
		/// Recursively adds this bone and its descendants to a bone filter set.
		/// </summary>
		/// <param name="filter">The filter set to add bone indices to.</param>
		internal void InternalCreateBoneFilterRecursive(HashSet<int> filter)
		{
			filter.Add(Index);

			if (Children != null)
			{
				foreach (var child in Children)
				{
					child.InternalCreateBoneFilterRecursive(filter);
				}
			}
		}

		/// <summary>
		/// Creates a bone filter containing this bone and optionally all its descendants.
		/// </summary>
		/// <param name="recursive">If true, includes all descendants; if false, only this bone.</param>
		/// <returns>A set of bone indices for use with animation layer filters.</returns>
		public HashSet<int> CreateBoneFilter(bool recursive = true)
		{
			var result = new HashSet<int>();

			if (!recursive)
			{
				result.Add(Index);
			}
			else
			{
				InternalCreateBoneFilterRecursive(result);
			}

			return result;
		}

		/// <summary>
		/// Returns the name of this bone.
		/// </summary>
		/// <returns>The name of this bone.</returns>
		public override string ToString() => Name;
	}
}
