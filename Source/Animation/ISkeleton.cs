namespace DigitalRiseModel.Animation
{
	/// <summary>
	/// Defines methods for managing an animated skeleton.
	/// </summary>
	public interface ISkeleton
	{
		/// <summary>
		/// Resets all bone transforms to their default poses.
		/// </summary>
		void ResetTransforms();

		/// <summary>
		/// Gets an animation clip by name.
		/// </summary>
		/// <param name="name">The name of the animation clip.</param>
		/// <returns>The animation clip with the specified name, or null if not found.</returns>
		AnimationClip GetClip(string name);

		/// <summary>
		/// Gets the default pose of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <returns>The default pose of the bone.</returns>
		SrtTransform GetDefaultPose(int boneIndex);

		/// <summary>
		/// Sets the current pose of a bone.
		/// </summary>
		/// <param name="boneIndex">The index of the bone.</param>
		/// <param name="pose">The transformation pose to set.</param>
		void SetPose(int boneIndex, SrtTransform pose);
	}
}
