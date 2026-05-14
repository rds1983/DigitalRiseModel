using Newtonsoft.Json.Linq;
using System;

namespace DigitalRiseModel.Animation
{
	internal class AnimationContext
	{
		private struct BoneData
		{
			public SrtTransform Transform;
			public float? Weight;
		}

		private readonly BoneData[] _bones;

		public ISkeleton Skeleton { get; }

		public AnimationContext(ISkeleton skeleton)
		{
			Skeleton = skeleton ?? throw new ArgumentNullException(nameof(skeleton));
			_bones = new BoneData[skeleton.BonesCount];
		}

		public void SetWeights(float? value)
		{
			for (var i = 0; i < _bones.Length; i++)
			{
				if (_bones[i].Weight == null)
				{
					continue;
				}

				_bones[i].Weight = value;
			}
		}

		public void Reset()
		{
			for (var i = 0; i < _bones.Length; i++)
			{
				_bones[i].Weight = null;
			}
		}

		public void SetTransform(int boneIndex, SrtTransform transform, float weight)
		{
			var curWeight = _bones[boneIndex].Weight;
			if (curWeight == null)
			{
				_bones[boneIndex].Transform = transform;
				_bones[boneIndex].Weight = weight;
			}
			else
			{
				var newWeight = curWeight.Value + weight;
				var normalizedWeight = weight / newWeight;

				_bones[boneIndex].Transform = SrtTransform.Interpolate(_bones[boneIndex].Transform, transform, normalizedWeight,
								InterpolationMode.Linear, InterpolationMode.Linear, InterpolationMode.Linear);
				_bones[boneIndex].Weight = newWeight;
			}
		}

		public void SetPoses()
		{
			for (var i = 0; i < _bones.Length; ++i)
			{
				if (_bones[i].Weight == null)
				{
					continue;
				}

				Skeleton.SetPose(i, _bones[i].Transform);
			}
		}
	}
}
