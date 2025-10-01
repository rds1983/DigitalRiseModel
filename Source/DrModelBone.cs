using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	public class NrmModelBone
	{
		private NrmModelBone[] _children;

		public string Name { get; }

		public int Index { get; internal set; }

		public SrtTransform DefaultPose = SrtTransform.Identity;

		public NrmModelBone Parent { get; private set; }

		public NrmModelBone[] Children
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

		public NrmMesh Mesh { get; }

		public object Tag { get; set; }

		public NrmModelBone(string name, NrmMesh mesh = null)
		{
			Name = name;
			if (mesh != null)
			{
				Mesh = mesh;
				Mesh.ParentBone = this;
			}
		}

		public Matrix CalculateDefaultLocalTransform() => DefaultPose.ToMatrix();

		public override string ToString() => Name;
	}
}
