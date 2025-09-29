using Microsoft.Xna.Framework;
using System;

namespace NursiaModel.Samples.BasicEngine
{
	public class CameraNode: SceneNode
	{
		private Vector3 _up, _right, _direction;
		private Matrix _view;
		private float _nearPlaneDistance = 0.1f;
		private float _farPlaneDistance = 1000f;

		public float NearPlaneDistance
		{
			get => _nearPlaneDistance;

			set
			{
				if (value.EpsilonEquals(_nearPlaneDistance))
				{
					return;
				}

				_nearPlaneDistance = value;
			}
		}

		public float FarPlaneDistance
		{
			get => _farPlaneDistance;

			set
			{
				if (value.EpsilonEquals(_farPlaneDistance))
				{
					return;
				}

				_farPlaneDistance = value;
			}
		}

		public float ViewAngle { get; set; } = 90.0f;

		public Vector3 Direction
		{
			get
			{
				UpdateGlobalTransform();

				return _direction;
			}
		}

		public Vector3 Target => Translation + Direction;

		public Vector3 Up
		{
			get
			{
				UpdateGlobalTransform();

				return _up;
			}
		}

		public Vector3 Right
		{
			get
			{
				UpdateGlobalTransform();

				return _right;
			}
		}

		public Matrix View
		{
			get
			{
				UpdateGlobalTransform();

				return _view;
			}
		}

		public CameraNode()
		{
		}

		public void SetLookAt(Vector3 position, Vector3 target)
		{
			Translation = position;

			var direction = target - Translation;
			direction.Normalize();

			var rotation = Rotation;
			rotation.X = 360 - MathHelper.ToDegrees((float)Math.Asin(direction.Y));
			rotation.Y = MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));

			Rotation = rotation;
		}

		protected override void OnGlobalTransformUpdated()
		{
			base.OnGlobalTransformUpdated();
		
			Vector3 scale, translation;
			Quaternion quaternion;
			GlobalTransform.Decompose(out scale, out quaternion, out translation);

			_direction = Vector3.Transform(Vector3.Backward, quaternion);
			_up = Vector3.Transform(Vector3.Up, quaternion);
			_right = Vector3.Cross(_direction, _up);
			_right.Normalize();

			_view = Matrix.CreateLookAt(translation, translation + _direction, _up);
		}

		public Matrix CalculateProjection(float aspectRatio) =>
			Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ViewAngle),
				aspectRatio, NearPlaneDistance, FarPlaneDistance);
	}
}