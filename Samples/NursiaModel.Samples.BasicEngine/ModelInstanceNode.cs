using NursiaModel.Samples.BasicEngine;

namespace NursiaModel
{
	public class ModelInstanceNode : VisualNode
	{
		public NrmModelInstance ModelInstance { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (ModelInstance == null || ModelInstance.Model == null)
			{
				return;
			}

			foreach (var mesh in ModelInstance.Model.Meshes)
			{
				foreach (var part in mesh.MeshParts)
				{
					var texture = GetTextureForMeshPart(part, context.WhiteTexture);
					var color = GetColorForMeshPart(part);

					if (part.Skin != null)
					{
						var skinTransforms = ModelInstance.GetSkinTransforms(part.Skin.SkinIndex);
						context.Render(part, EffectType.Skinned, GlobalTransform, texture, color, skinTransforms);
					}
					else
					{
						var boneTransform = ModelInstance.GetBoneGlobalTransform(mesh.ParentBone.Index) * GlobalTransform;
						context.Render(part, EffectType.Basic, boneTransform, texture, color, null);
					}
				}
			}
		}
	}
}
