using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	public class DrModelBone
	{
		private DrModelBone[] _children;

		public int Index { get; internal set; }
		public string Name { get; }
		public DrModelBone Parent { get; private set; }

		public DrModelBone[] Children
		{
			get => _children;

			internal set
			{
				if (value != null)
				{
					foreach (var b in value)
					{
						b.Parent = this;
					}
				}

				_children = value;
			}
		}

		public DrMesh Mesh { get; internal set; }

		public SrtTransform DefaultPose = SrtTransform.Identity;

		internal DrModelBone(string name)
		{
			Name = name;
		}
		
		public override string ToString() => Name;

		public Matrix CalculateDefaultLocalTransform() => DefaultPose.ToMatrix();
		public Matrix CalculateDefaultAbsoluteTransform()
		{
			if (Parent == null)
			{
				return CalculateDefaultLocalTransform();
			}

			return CalculateDefaultLocalTransform() * Parent.CalculateDefaultAbsoluteTransform();
		}
	}
}
