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

				foreach (var submesh in bone.Mesh.Submeshes)
				{
					var texture = GetTextureForSubmesh(submesh, context.WhiteTexture);
					var color = GetColorForSubmesh(submesh);

					var boneTransform = ModelInstance.GetBoneGlobalTransform(bone.Index) * GlobalTransform;
					var boundingBox = submesh.BoundingBox.Transform(ref boneTransform);

					if (bone.Skin != null)
					{
						var skinTransforms = ModelInstance.GetSkinTransforms(bone.Skin.SkinIndex);
						context.Render(submesh, boundingBox, EffectType.Skinned, GlobalTransform, texture, color, skinTransforms);
					}
					else
					{
						context.Render(submesh, boundingBox, EffectType.Basic, boneTransform, texture, color, null);
					}
				}
			}
		}
	}
}
