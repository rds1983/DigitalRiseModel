using DigitalRiseModel.Samples.BasicEngine;

namespace DigitalRiseModel
{
	public class MeshNode : VisualNode
	{
		public NrmMesh Mesh { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (Mesh == null)
			{
				return;
			}

			foreach (var meshpart in Mesh.MeshParts)
			{
				var texture = GetTextureForMeshPart(meshpart, context.WhiteTexture);
				var color = GetColorForMeshPart(meshpart);

				context.Render(meshpart, EffectType.Basic, GlobalTransform, texture, color, null);
			}
		}
	}
}
