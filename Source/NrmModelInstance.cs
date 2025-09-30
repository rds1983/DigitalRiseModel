using NursiaModel.Animation;
using NursiaModel.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace NursiaModel
{
	public class NrmModelInstance : ISkeleton
	{
		private class SkinInfo
		{
			public NrmSkin Skin { get; }
			public Matrix[] Transforms { get; }

			public SkinInfo(NrmSkin skin)
			{
				Skin = skin ?? throw new ArgumentNullException(nameof(skin));
				Transforms = new Matrix[skin.Joints.Length];
			}
		}

		private bool _transformsDirty = true;
		private Matrix[] _localTransforms;
		private Matrix[] _worldTransforms;
		private Dictionary<int, SkinInfo> _skinInfos;

		private NrmModel _model;

		public BoundingBox? BoundingBox { get; private set; }

		public NrmModel Model
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
					foreach (var mesh in _model.Meshes)
					{
						foreach (var part in mesh.MeshParts)
						{
							if (part.Skin == null)
							{
								continue;
							}

							if (_skinInfos == null)
							{
								_skinInfos = new Dictionary<int, SkinInfo>();
							}

							_skinInfos[part.Skin.SkinIndex] = new SkinInfo(part.Skin);
						}
					}

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

		public NrmModelInstance()
		{
		}

		public NrmModelInstance(NrmModel model)
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
				foreach(var pair in _skinInfos)
				{
					var skinInfo = pair.Value;
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
			foreach (var mesh in _model.Meshes)
			{
				var bone = mesh.ParentBone;

				foreach(var part in mesh.MeshParts)
				{
					var m = part.Skin != null ? Matrix.Identity : _worldTransforms[bone.Index];
					var bb = bone.Mesh.BoundingBox.Transform(ref m);
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

		public Matrix[] GetSkinTransforms(int skinIndex)
		{
			UpdateTransforms();

			return _skinInfos[skinIndex].Transforms;
		}

		public NrmModelInstance Clone()
		{
			var result = new NrmModelInstance
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
