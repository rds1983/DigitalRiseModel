using Microsoft.Xna.Framework;
using System;

namespace DigitalRiseModel.Samples.BasicEngine
{
	/// <summary>
	/// A scene node that represents a camera in 3D space.
	/// Provides view and projection matrices for rendering, and supports camera control via SetLookAt.
	/// </summary>
	public class CameraNode: SceneNode
	{
		// Cached direction, up, and right vectors derived from the camera's global transform
		private Vector3 _up, _right, _direction;
		// Cached view matrix, recalculated when transform is invalidated
		private Matrix _view;
		// Near and far clipping planes for the view frustum
		private float _nearPlaneDistance = 0.1f;
		private float _farPlaneDistance = 1000f;

		/// <summary>
		/// Gets or sets the distance from the camera to the near clipping plane.
		/// Default is 0.1. Objects closer than this are not rendered.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the distance from the camera to the far clipping plane.
		/// Default is 1000. Objects farther than this are not rendered.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the vertical field of view angle in degrees.
		/// Default is 90 degrees.
		/// </summary>
		public float ViewAngle { get; set; } = 90.0f;

		/// <summary>
		/// Gets the direction vector the camera is looking (forward direction).
		/// </summary>
		public Vector3 Direction
		{
			get
			{
				UpdateGlobalTransform();
				return _direction;
			}
		}

		/// <summary>
		/// Gets the world position the camera is pointing at (Position + Direction).
		/// </summary>
		public Vector3 Target => Translation + Direction;

		/// <summary>
		/// Gets the up vector for the camera (perpendicular to direction and right).
		/// </summary>
		public Vector3 Up
		{
			get
			{
				UpdateGlobalTransform();
				return _up;
			}
		}

		/// <summary>
		/// Gets the right vector for the camera (perpendicular to direction and up).
		/// </summary>
		public Vector3 Right
		{
			get
			{
				UpdateGlobalTransform();
				return _right;
			}
		}

		/// <summary>
		/// Gets the view matrix for this camera.
		/// This matrix transforms world coordinates into camera-relative coordinates.
		/// </summary>
		public Matrix View
		{
			get
			{
				UpdateGlobalTransform();
				return _view;
			}
		}

		/// <summary>
		/// Initializes a new instance of the CameraNode class.
		/// </summary>
		public CameraNode()
		{
		}

		/// <summary>
		/// Positions the camera at the specified position looking towards the target position.
		/// </summary>
		/// <param name="position">The camera's position in world space.</param>
		/// <param name="target">The point in world space the camera should look at.</param>
		public void SetLookAt(Vector3 position, Vector3 target)
		{
			Translation = position;

			// Calculate the direction vector from camera to target
			var direction = target - Translation;
			direction.Normalize();

			// Convert direction to Euler angles (pitch and yaw)
			// The pitch angle is derived from the Y component (vertical look)
			// The yaw angle is derived from the X and Z components (horizontal look)
			var rotation = Rotation;
			rotation.X = 360 - MathHelper.ToDegrees((float)Math.Asin(direction.Y));
			rotation.Y = MathHelper.ToDegrees((float)Math.Atan2(direction.X, direction.Y));

			Rotation = rotation;
		}

		/// <summary>
		/// Called when the camera's global transform is updated.
		/// Recalculates the direction, up, right vectors and the view matrix from the transform.
		/// </summary>
		protected override void OnGlobalTransformUpdated()
		{
			base.OnGlobalTransformUpdated();

			// Decompose the global transform matrix into its components
			Vector3 scale, translation;
			Quaternion quaternion;
			GlobalTransform.Decompose(out scale, out quaternion, out translation);

			// Extract camera basis vectors from the rotation quaternion
			// The camera looks along the negative Z axis (Backward direction)
			_direction = Vector3.Transform(Vector3.Backward, quaternion);
			_up = Vector3.Transform(Vector3.Up, quaternion);
			// Right vector is the cross product of forward and up
			_right = Vector3.Cross(_direction, _up);
			_right.Normalize();

			// Create the view matrix from the camera position and orientation
			_view = Matrix.CreateLookAt(translation, translation + _direction, _up);
		}

		/// <summary>
		/// Calculates the projection matrix for this camera with the specified aspect ratio.
		/// </summary>
		/// <param name="aspectRatio">The aspect ratio (width / height) of the viewport.</param>
		/// <returns>The perspective projection matrix.</returns>
		public Matrix CalculateProjection(float aspectRatio) =>
			Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(ViewAngle),
				aspectRatio, NearPlaneDistance, FarPlaneDistance);
	}
}