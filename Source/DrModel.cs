using DigitalRiseModel.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel
{
	public partial class DrModel
	{
		public DrModelBone Root { get; }
		public DrModelBone[] Bones { get; }
		public DrModelBone[] MeshBones { get; }

		public Dictionary<string, AnimationClip> Animations { get; internal set; }
		public DrMaterial[] Materials { get; internal set; }

		internal DrModel(DrModelBone root)
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
			MeshBones = (from bone in Bones where bone.Mesh != null select bone).ToArray();
		}

		private void TraverseBones(DrModelBone root, Action<DrModelBone> action)
		{
			action(root);

			foreach (var child in root.Children)
			{
				TraverseBones(child, action);
			}
		}

		public void TraverseBones(Action<DrModelBone> action)
		{
			TraverseBones(Root, action);
		}
	}
}