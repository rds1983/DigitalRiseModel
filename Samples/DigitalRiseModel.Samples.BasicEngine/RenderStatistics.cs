namespace DigitalRiseModel
{
	/// <summary>
	/// Tracks rendering statistics for performance monitoring and debugging.
	/// Records the number of draw calls, effect switches, and primitive/vertex counts per frame.
	/// </summary>
	public struct RenderStatistics
	{
		/// <summary>The number of times the active effect was switched during this frame.</summary>
		public int EffectsSwitches;
		/// <summary>The total number of draw calls made during this frame.</summary>
		public int DrawCalls;
		/// <summary>The total number of vertices rendered during this frame.</summary>
		public int VerticesDrawn;
		/// <summary>The total number of primitives rendered during this frame.</summary>
		public int PrimitivesDrawn;
		/// <summary>The total number of mesh parts rendered during this frame.</summary>
		public int MeshesDrawn;

		/// <summary>
		/// Resets all statistics counters to zero.
		/// Called at the beginning of each frame before rendering.
		/// </summary>
		public void Reset()
		{
			EffectsSwitches = 0;
			DrawCalls = 0;
			VerticesDrawn = 0;
			PrimitivesDrawn = 0;
			MeshesDrawn = 0;
		}
	}
}
