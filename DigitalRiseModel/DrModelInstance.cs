using DigitalRiseModel.Animation;
using DigitalRiseModel.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents an instance of a model that can be animated and rendered.
	/// </summary>
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
		private Dictionary<int, SkinInfo> _skinInfos;

		private DrModel _model;

		/// <summary>
		/// Gets the bounding box of this model instance in its current pose.
		/// </summary>
		public BoundingBox? BoundingBox { get; private set; }

		/// <summary>
		/// Gets or sets the model that this instance represents.
		/// </summary>
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

		int ISkeleton.BonesCount
		{
			get
			{
				if (_model == null)
				{
					return 0;
				}

				return _model.Bones.Length;
			}
		}

		/// <summary>
		/// Gets or sets an arbitrary object associated with this model instance.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DrModelInstance"/> class with no model.
		/// </summary>
		public DrModelInstance()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrModelInstance"/> class with the specified model.
		/// </summary>
		/// <param name="model">The model to instantiate.</param>
		public DrModelInstance(DrModel model)
		{
			Model = model;
		}

		/// <summary>
		/// Resets all bone transforms to their default poses.
		/// </summary>
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
				foreach (var pair in _skinInfos)
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

			if (_model != null)
			{
				foreach (var mesh in _model.Meshes)
				{
					var bone = mesh.ParentBone;
					foreach (var part in mesh.MeshParts)
					{
						var m = part.Skin != null ? Matrix.Identity : _worldTransforms[bone.Index];
						var bb = part.BoundingBox.Transform(ref m);
						boundingBox = Microsoft.Xna.Framework.BoundingBox.CreateMerged(boundingBox, bb);
					}

				}
			}

			return boundingBox;
		}

		/// <summary>
		/// Gets the local transformation matrix of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <returns>The local transformation matrix of the bone.</returns>
		public Matrix GetBoneLocalTransform(int boneIndex) => _localTransforms[boneIndex];

		/// <summary>
		/// Sets the local transformation matrix of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <param name="transform">The local transformation matrix to set.</param>
		public void SetBoneLocalTransform(int boneIndex, Matrix transform)
		{
			_localTransforms[boneIndex] = transform;
			_transformsDirty = true;
		}

		/// <summary>
		/// Gets the global (world-space) transformation matrix of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <returns>The global transformation matrix of the bone.</returns>
		public Matrix GetBoneGlobalTransform(int boneIndex)
		{
			UpdateTransforms();

			return _worldTransforms[boneIndex];
		}

		/// <summary>
		/// Gets the skin transformation matrices for the specified skin.
		/// </summary>
		/// <param name="skinIndex">The index of the skin.</param>
		/// <returns>An array of transformation matrices for the skin's joints.</returns>
		public Matrix[] GetSkinTransforms(int skinIndex)
		{
			UpdateTransforms();

			return _skinInfos[skinIndex].Transforms;
		}

		/// <summary>
		/// Creates a copy of this model instance.
		/// </summary>
		/// <returns>A new <see cref="DrModelInstance"/> with the same model.</returns>
		public DrModelInstance Clone()
		{
			var result = new DrModelInstance
			{
				Model = Model
			};

			return result;
		}

		/// <summary>
		/// Gets an animation clip by name.
		/// </summary>
		/// <param name="name">The name of the animation clip.</param>
		/// <returns>The animation clip with the specified name, or null if not found.</returns>
		public AnimationClip GetClip(string name) => Model.Animations[name];

		/// <summary>
		/// Gets the default pose of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <returns>The default pose of the bone.</returns>
		public SrtTransform GetDefaultPose(int boneIndex) => Model.Bones[boneIndex].DefaultPose;

		/// <summary>
		/// Sets the transformation pose of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <param name="pose">The transformation pose to set.</param>
		public void SetPose(int boneIndex, SrtTransform pose) => SetBoneLocalTransform(boneIndex, pose.ToMatrix());
	}
}
