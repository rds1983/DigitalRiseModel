using DigitalRiseModel.Samples.BasicEngine;

namespace DigitalRiseModel
{
	public class MeshNode : VisualNode
	{
		public DrMesh Mesh { get; set; }

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

				var transform = GlobalTransform;
				var boundingBox = meshpart.BoundingBox.Transform(ref transform);
				context.Render(meshpart, boundingBox, EffectType.Basic, GlobalTransform, texture, color, null);
			}
		}
	}
}
