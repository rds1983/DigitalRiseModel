using DigitalRiseModel.Samples.BasicEngine;

namespace DigitalRiseModel
{
	public class ModelInstanceNode : VisualNode
	{
		public DrModelInstance ModelInstance { get; set; }

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			if (ModelInstance == null || ModelInstance.Model == null)
			{
				return;
			}

			foreach (var mesh in ModelInstance.Model.Meshes)
			{
				var bone = mesh.ParentBone;
				foreach (var meshpart in bone.Mesh.MeshParts)
				{
					var texture = GetTextureForMeshPart(meshpart, context.WhiteTexture);
					var color = GetColorForMeshPart(meshpart);

					if (bone.Skin != null)
					{
						var skinTransforms = ModelInstance.GetSkinTransforms(bone.Skin.SkinIndex);
						context.Render(meshpart, EffectType.Skinned, GlobalTransform, texture, color, skinTransforms);
					}
					else
					{
						var boneTransform = ModelInstance.GetBoneGlobalTransform(bone.Index) * GlobalTransform;
						context.Render(meshpart, EffectType.Basic, boneTransform, texture, color, null);
					}
				}
			}
		}
	}
}
