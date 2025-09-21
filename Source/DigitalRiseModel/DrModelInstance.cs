using DigitalRiseModel.Animation;
using DigitalRiseModel.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel
{
	public class DrModelInstance : ISkeleton
	{
		private class SkinInfo
		{
			public DrSkin Skin { get; }
			public Matrix[] Transforms { get; }

			public SkinInfo(DrSkin skin)
			{
				Skin = skin ?? throw new ArgumentNullException(nameof(skin));
				Transforms = new Matrix[skin.Joints.Length];
			}
		}

		private bool _transformsDirty = true;
		private Matrix[] _localTransforms;
		private Matrix[] _worldTransforms;
		private SkinInfo[] _skinInfos;

		private DrModel _model;

		public BoundingBox? BoundingBox { get; private set; }

		public DrModel Model
		{
			get => _model;

			set
			{
				if (value == _model)
				{
					return;
				}

				_model = value;

				_localTransforms = null;
				_worldTransforms = null;
				_skinInfos = null;
				if (_model != null)
				{
					_localTransforms = new Matrix[_model.Bones.Length];
					_worldTransforms = new Matrix[_model.Bones.Length];

					var skinInfos = new List<SkinInfo>();
					foreach (var n in _model.Bones)
					{
						if (n.Skin == null)
						{
							continue;
						}

						skinInfos.Add(new SkinInfo(n.Skin));
					}

					_skinInfos = skinInfos.ToArray();

					ResetTransforms();

					BoundingBox = CalculateBoundingBox();
				}
				else
				{
					BoundingBox = null;
				}
			}
		}

		public object Tag { get; set; }

		public DrModelInstance()
		{
		}

		public DrModelInstance(DrModel model)
		{
			Model = model;
		}

		public void ResetTransforms()
		{
			if (Model == null)
			{
				return;
			}

			Model.CopyBoneTransformsTo(_localTransforms);
			_transformsDirty = true;
		}

		private void UpdateTransforms()
		{
			if (!_transformsDirty)
			{
				return;
			}

			for (var i = 0; i < Model.Bones.Length; i++)
			{
				var bone = Model.Bones[i];

				if (bone.Parent == null)
				{
					_worldTransforms[bone.Index] = _localTransforms[bone.Index];
				}
				else
				{
					_worldTransforms[bone.Index] = _localTransforms[bone.Index] * _worldTransforms[bone.Parent.Index];
				}
			}

			// Update skin transforms
			if (_skinInfos != null)
			{
				for (var i = 0; i < _skinInfos.Length; ++i)
				{
					var skinInfo = _skinInfos[i];
					for (var j = 0; j < skinInfo.Skin.Joints.Length; ++j)
					{
						var joint = skinInfo.Skin.Joints[j];

						skinInfo.Transforms[j] = joint.InverseBindTransform * _worldTransforms[joint.Bone.Index];
					}
				}
			}

			_transformsDirty = false;
		}

		private BoundingBox CalculateBoundingBox()
		{
			UpdateTransforms();

			var boundingBox = new BoundingBox();
			foreach (var bone in _model.MeshBones)
			{
				if (bone.Mesh == null)
				{
					continue;
				}

				var m = bone.Skin != null ? Matrix.Identity : _worldTransforms[bone.Index];
				var bb = bone.Mesh.BoundingBox.Transform(ref m);
				boundingBox = Microsoft.Xna.Framework.BoundingBox.CreateMerged(boundingBox, bb);
			}

			return boundingBox;
		}

		public Matrix GetBoneLocalTransform(int boneIndex) => _localTransforms[boneIndex];

		public void SetBoneLocalTransform(int boneIndex, Matrix transform)
		{
			_localTransforms[boneIndex] = transform;
			_transformsDirty = true;
		}

		public Matrix GetBoneGlobalTransform(int boneIndex)
		{
			UpdateTransforms();

			return _worldTransforms[boneIndex];
		}

		public Matrix[] GetSkinTransforms(int skinIndex)
		{
			UpdateTransforms();

			return _skinInfos[skinIndex].Transforms;
		}

		public DrModelInstance Clone()
		{
			var result = new DrModelInstance
			{
				Model = Model
			};

			return result;
		}

		public AnimationClip GetClip(string name) => Model.Animations[name];

		public SrtTransform GetDefaultPose(int boneIndex) => Model.Bones[boneIndex].DefaultPose;

		public void SetPose(int boneIndex, SrtTransform pose) => SetBoneLocalTransform(boneIndex, pose.ToMatrix());
	}
}
