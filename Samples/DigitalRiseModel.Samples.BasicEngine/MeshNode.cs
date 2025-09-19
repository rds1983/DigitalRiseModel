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

			foreach (var submesh in Mesh.Submeshes)
			{
				var texture = GetTextureForSubmesh(submesh, context.WhiteTexture);
				var color = GetColorForSubmesh(submesh);

				var transform = GlobalTransform;
				var boundingBox = submesh.BoundingBox.Transform(ref transform);
				context.Render(submesh, boundingBox, EffectType.Basic, GlobalTransform, texture, color, null);
			}
		}
	}
}
