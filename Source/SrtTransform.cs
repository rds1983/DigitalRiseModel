/*
 * SrtTransform.cs
 * Author: Bruno Evangelista
 * Copyright (c) 2008 Bruno Evangelista. All rights reserved.
 *
 * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using System;
using Microsoft.Xna.Framework;
using System.Globalization;
using DigitalRiseModel.Utility;

namespace DigitalRiseModel
{
	/// <summary>
	/// Specifies how translations, orientations and scales are interpolated between keyframes.
	/// </summary>
	public enum InterpolationMode
	{
		/// <summary>
		/// Does not use interpolation.
		/// </summary>
		None,

		/// <summary>
		/// Linear interpolation. Supported on translations and scales.
		/// </summary>
		Linear,

		/// <summary>
		/// Cubic interpolation. Supported on translations and scales.
		/// </summary>
		Cubic,

		/// <summary>
		/// Spherical interpolation. Only supported on orientations.
		/// </summary>
		Spherical
	};

	/// <summary>
	/// Represents a scale, rotation, and translation transformation.
	/// </summary>
	[Serializable]
	public struct SrtTransform : IEquatable<SrtTransform>
	{
		/// <summary>
		/// Gets or sets the translation component of this transformation.
		/// </summary>
		public Vector3 Translation;

		/// <summary>
		/// Gets or sets the rotation component of this transformation.
		/// </summary>
		public Quaternion Rotation;

		/// <summary>
		/// Gets or sets the scale component of this transformation.
		/// </summary>
		public Vector3 Scale;

		/// <summary>
		/// Gets the identity transformation (no translation, no rotation, unit scale).
		/// </summary>
		public static readonly SrtTransform Identity;

		static SrtTransform()
		{
			Identity = new SrtTransform(Vector3.Zero, Quaternion.Identity, Vector3.One);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SrtTransform"/> struct with the specified translation, rotation, and scale.
		/// </summary>
		/// <param name="translation">The translation component.</param>
		/// <param name="rotation">The rotation component.</param>
		/// <param name="scale">The scale component.</param>
		public SrtTransform(Vector3 translation, Quaternion rotation, Vector3 scale)
		{
			Translation = translation;
			Rotation = rotation;
			Scale = scale;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SrtTransform"/> struct from a transformation matrix.
		/// </summary>
		/// <param name="m">The transformation matrix to decompose.</param>
		public SrtTransform(Matrix m)
		{
			m.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
			Translation = translation;
			Rotation = rotation;
			Scale = scale;
		}

		/// <summary>
		/// Converts this transformation to a transformation matrix.
		/// </summary>
		/// <returns>The transformation matrix.</returns>
		public Matrix ToMatrix() => CreateMatrix(Translation, Scale, Rotation);

		/// <summary>
		/// Interpolates between two transformations using the specified interpolation modes.
		/// </summary>
		/// <param name="pose1">The first transformation.</param>
		/// <param name="pose2">The second transformation.</param>
		/// <param name="amount">The blend amount between the two transformations. Must be between 0.0 and 1.0 inclusive.</param>
		/// <param name="translationInterpolation">The interpolation mode to use for the translation component.</param>
		/// <param name="orientationInterpolation">The interpolation mode to use for the rotation component.</param>
		/// <param name="scaleInterpolation">The interpolation mode to use for the scale component.</param>
		/// <returns>The interpolated transformation.</returns>
		/// <exception cref="ArgumentException"><paramref name="amount"/> is not between 0.0 and 1.0, or an unsupported interpolation mode is specified.</exception>
		public static SrtTransform Interpolate(SrtTransform pose1, SrtTransform pose2, float amount,
			InterpolationMode translationInterpolation, InterpolationMode orientationInterpolation,
			InterpolationMode scaleInterpolation)
		{
			SrtTransform resultPose;

			if (amount < 0 || amount > 1)
				throw new ArgumentException("Amount must be between 0.0 and 1.0 inclusive.");

			switch (translationInterpolation)
			{
				case InterpolationMode.None:
					resultPose.Translation = pose1.Translation;
					break;

				case InterpolationMode.Linear:
					Vector3.Lerp(ref pose1.Translation, ref pose2.Translation, amount,
						out resultPose.Translation);
					break;

				case InterpolationMode.Cubic:
					Vector3.SmoothStep(ref pose1.Translation, ref pose2.Translation, amount,
						out resultPose.Translation);
					break;

				default:
					throw new ArgumentException("Translation interpolation method not supported");
			}

			switch (orientationInterpolation)
			{
				case InterpolationMode.None:
					resultPose.Rotation = pose1.Rotation;
					break;

				case InterpolationMode.Linear:
					Quaternion.Lerp(ref pose1.Rotation, ref pose2.Rotation, amount,
						out resultPose.Rotation);
					break;

				case InterpolationMode.Spherical:
					Quaternion.Slerp(ref pose1.Rotation, ref pose2.Rotation, amount,
						out resultPose.Rotation);
					break;

				default:
					throw new ArgumentException("Orientation interpolation method not supported");
			}

			switch (scaleInterpolation)
			{
				case InterpolationMode.None:
					resultPose.Scale = pose1.Scale;
					break;

				case InterpolationMode.Linear:
					Vector3.Lerp(ref pose1.Scale, ref pose2.Scale, amount,
						out resultPose.Scale);
					break;

				case InterpolationMode.Cubic:
					Vector3.SmoothStep(ref pose1.Scale, ref pose2.Scale, amount,
						out resultPose.Scale);
					break;

				default:
					throw new ArgumentException("Scale interpolation method not supported");
			}

			return resultPose;
		}

		/// <summary>
		/// Creates a transformation matrix from the specified translation, scale, and rotation.
		/// </summary>
		/// <param name="translation">The translation component.</param>
		/// <param name="scale">The scale component.</param>
		/// <param name="rotation">The rotation component.</param>
		/// <returns>The transformation matrix.</returns>
		public static Matrix CreateMatrix(Vector3 translation, Vector3 scale, Quaternion rotation)
		{
			return Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation);
		}

		#region IEquatable<Pose> Members

		/// <summary>
		/// Returns the hash code of this transformation.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return (Translation.GetHashCode() + Rotation.GetHashCode() + Scale.GetHashCode());
		}

		/// <summary>
		/// Returns a string representation of this transformation.
		/// </summary>
		/// <returns>A string representation of this transformation.</returns>
		public override string ToString()
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			return string.Format(currentCulture,
				"{{Translation:{0}\n Orientation:{1}\n Scale:{2}\n}}", new object[]
				{ Translation.ToString(), Rotation.ToString(), Scale.ToString() });
		}

		/// <summary>
		/// Indicates whether the current transformation is equal to another transformation.
		/// </summary>
		/// <param name="other">Another transformation to compare to.</param>
		/// <returns>true if the current transformation is equal to the other transformation; otherwise, false.</returns>
		public bool Equals(SrtTransform other)
		{
			return (Translation == other.Translation &&
				Rotation == other.Rotation &&
				Scale == other.Scale);
		}

		/// <summary>
		/// Indicates whether the current transformation is equal to another object.
		/// </summary>
		/// <param name="obj">An object to compare to.</param>
		/// <returns>true if the current transformation is equal to the object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			bool result = false;

			if (obj is SrtTransform)
			{
				result = Equals((SrtTransform)obj);
			}

			return result;
		}

		/// <summary>
		/// Determines whether two transformations are equal.
		/// </summary>
		/// <param name="pose1">The first transformation.</param>
		/// <param name="pose2">The second transformation.</param>
		/// <returns>true if the transformations are equal; otherwise, false.</returns>
		public static bool operator ==(SrtTransform pose1, SrtTransform pose2)
		{
			return (pose1.Translation == pose2.Translation &&
				pose1.Rotation == pose2.Rotation &&
				pose1.Scale == pose2.Scale);
		}

		/// <summary>
		/// Determines whether two transformations are not equal.
		/// </summary>
		/// <param name="pose1">The first transformation.</param>
		/// <param name="pose2">The second transformation.</param>
		/// <returns>true if the transformations are not equal; otherwise, false.</returns>
		public static bool operator !=(SrtTransform pose1, SrtTransform pose2)
		{
			return (pose1.Translation != pose2.Translation ||
				pose1.Rotation != pose2.Rotation ||
				pose1.Scale != pose2.Scale);
		}

		#endregion
	}
}