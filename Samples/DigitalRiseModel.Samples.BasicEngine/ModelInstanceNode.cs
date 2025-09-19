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

					if (bone.Skin != null)
					{
						var skinTransforms = ModelInstance.GetSkinTransforms(bone.Skin.SkinIndex);
						context.Render(submesh, EffectType.Skinned, GlobalTransform, texture, color, skinTransforms);
					}
					else
					{
						var boneTransform = ModelInstance.GetBoneGlobalTransform(bone.Index);
						var transform = boneTransform * GlobalTransform;

						context.Render(submesh, EffectType.Basic, transform, texture, color, null);
					}
				}
			}
		}
	}
}
