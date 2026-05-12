using DigitalRiseModel.Samples.BasicEngine;

namespace DigitalRiseModel
{
	/// <summary>
	/// A visual node that renders a static mesh (without skeletal animation).
	/// The mesh is rendered using the Basic effect type.
	/// </summary>
	public class MeshNode : VisualNode
	{
		/// <summary>
		/// Gets or sets the mesh to be rendered by this node.
		/// </summary>
		public DrMesh Mesh { get; set; }

		/// <summary>
		/// Renders this mesh node and all its child nodes.
		/// Uses the Basic effect (no bone transforms).
		/// </summary>
		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Mesh == null)
			{
				return; // Nothing to render
			}

			// Render each mesh part with the appropriate texture and color
			foreach (var meshpart in Mesh.MeshParts)
			{
				// Determine the texture to use (override, material, or white default)
				var texture = GetTextureForMeshPart(meshpart, context.WhiteTexture);
				// Determine the color to use (override or material)
				var color = GetColorForMeshPart(meshpart);

				// Render the mesh part using the Basic effect (no bones)
				context.Render(meshpart, EffectType.Basic, GlobalTransform, texture, color, null);
			}
		}
	}
}
