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

			foreach (var bone in ModelInstance.Model.MeshBones)
			{
				if (bone.Mesh == null)
				{
					continue;
				}

				foreach (var meshpart in bone.Mesh.MeshParts)
				{
					var texture = GetTextureForMeshPart(meshpart, context.WhiteTexture);
					var color = GetColorForMeshPart(meshpart);

					var boneTransform = ModelInstance.GetBoneGlobalTransform(bone.Index) * GlobalTransform;
					var boundingBox = meshpart.BoundingBox.Transform(ref boneTransform);

					if (bone.Skin != null)
					{
						var skinTransforms = ModelInstance.GetSkinTransforms(bone.Skin.SkinIndex);
						context.Render(meshpart, boundingBox, EffectType.Skinned, GlobalTransform, texture, color, skinTransforms);
					}
					else
					{
						context.Render(meshpart, boundingBox, EffectType.Basic, boneTransform, texture, color, null);
					}
				}
			}
		}
	}
}
