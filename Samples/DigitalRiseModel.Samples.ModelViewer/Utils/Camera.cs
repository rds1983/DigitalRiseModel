using Microsoft.Xna.Framework;
using System;

namespace DigitalRiseModel.Samples.ModelViewer.Utils
{
	public class Camera
	{
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;
		private Matrix? _transform = null;
		private Vector3 _up, _right, _direction;
		private Matrix _view;
		private float _nearPlaneDistance = 0.1f;
		private float _farPlaneDistance = 1000f;

		public Vector3 Translation
		{
			get => _translation;

			set
			{
				if (value == _translation)
				{
					return;
				}

				_translation = value;
				Invalidate();
			}
		}

		public Vector3 Scale
		{
			get => _scale;

			set
			{
				if (value == _scale)
				{
					return;
				}

				_scale = value;
				Invalidate();
			}
		}

		public Vector3 Rotation
		{
			get => _rotation;

			set
			{
				value.X = value.X.ClampDegree();
				value.Y = value.Y.ClampDegree();
				value.Z = value.Z.ClampDegree();

				if (value == _rotation)
				{
					return;
				}

				_rotation = value;
				Invalidate();
			}
		}

		public Matrix Transform
		{
			get
			{
				if (_transform == null)
				{
					var quaternion = Quaternion.CreateFromYawPitchRoll(
											MathHelper.ToRadians(_rotation.Y),
											MathHelper.ToRadians(_rotation.X),
											MathHelper.ToRadians(_rotation.Z));
					_transform = Utility.CreateTransform(Translation, Scale, quaternion);
				}

				return _transform.Value;
			}
		}

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
				Update();
				return _direction;
			}
		}

		public Vector3 Target => Translation + Direction;


		public Vector3 Up
		{
			get
			{
				Update();
				return _up;
			}
		}

		public Vector3 Right
		{
			get
			{
				Update();
				return _right;
			}
		}

		public Matrix View
		{
			get
			{
				Update();
				return _view;
			}
		}

		public Camera()
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

		private void Update()
		{
			Vector3 scale, translation;
			Quaternion quaternion;
			Transform.Decompose(out scale, out quaternion, out translation);

			_direction = Vector3.Transform(Vector3.Backward, quaternion);
			_up = Vector3.Transform(Vector3.Up, quaternion);
			_right = Vector3.Cross(_direction, _up);
			_right.Normalize();

			_view = Matrix.CreateLookAt(translation, translation + _direction, _up);
		}

		public Matrix CalculateProjection(float aspectRatio) =>
			Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ViewAngle),
				aspectRatio, NearPlaneDistance, FarPlaneDistance);

		private void Invalidate()
		{
			_transform = null;
		}
	}
}