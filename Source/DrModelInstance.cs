using DigitalRiseModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel
{
	public class DrModelInstance : ISkeleton
	{
		private class SkinInfo
		{
			public Skin Skin { get; }
			public Matrix[] Transforms { get; }

			public SkinInfo(Skin skin)
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
					_model.TraverseBones(n =>
					{
						if (n.Mesh == null)
						{
							return;
						}

						foreach (var submesh in n.Mesh.Submeshes)
						{
							if (submesh.Skin != null)
							{
								skinInfos.Add(new SkinInfo(submesh.Skin));
							}
						}
					});

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

		public void ResetTransforms()
		{
			if (Model == null)
			{
				return;
			}

			for (var i = 0; i < Model.Bones.Length; i++)
			{
				var bone = Model.Bones[i];
				_localTransforms[bone.Index] = bone.CalculateDefaultLocalTransform();
			}

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

						skinInfo.Transforms[j] = joint.InverseBindTransform * _worldTransforms[joint.BoneIndex];
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
				foreach (var submesh in bone.Mesh.Submeshes)
				{
					var m = submesh.Skin != null ? Matrix.Identity : _worldTransforms[bone.Index];
					var bb = submesh.BoundingBox.Transform(ref m);
					boundingBox = Microsoft.Xna.Framework.BoundingBox.CreateMerged(boundingBox, bb);
				}
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
