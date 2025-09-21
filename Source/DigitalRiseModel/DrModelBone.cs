using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	public class DrModelBone
	{
		private DrModelBone[] _children;

		public string Name { get; }

		public int Index { get; internal set; }

		public SrtTransform DefaultPose = SrtTransform.Identity;

		public DrModelBone Parent { get; private set; }

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

		public DrMesh Mesh { get; set; }

		public DrSkin Skin { get; set; }

		public object Tag { get; set; }

		public DrModelBone(string name)
		{
			Name = name;
		}

		public Matrix CalculateDefaultLocalTransform() => DefaultPose.ToMatrix();

		public override string ToString() => Name;
	}
}
