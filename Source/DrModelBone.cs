using Microsoft.Xna.Framework;

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
		/// Returns the name of this bone.
		/// </summary>
		/// <returns>The name of this bone.</returns>
		public override string ToString() => Name;
	}
}
