using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public class ModelInstanceNode : SceneNode
	{
		public DrModelInstance ModelInstance { get; set; }
		public Texture2D CustomTexture { get; set; }
		public Color? CustomColor { get; set; }

		private Texture2D GetTextureForSubmesh(DrSubmesh submesh, Texture2D defaultTexture)
		{
			if (CustomTexture != null)
			{
				return CustomTexture;
			}

			if (submesh.Material == null || submesh.Material.DiffuseTexture == null)
			{
				return defaultTexture;
			}

			return submesh.Material.DiffuseTexture;
		}

		private Color GetColorForSubmesh(DrSubmesh submesh)
		{
			if (CustomColor != null)
			{
				return CustomColor.Value;
			}

			if (submesh.Material == null)
			{
				return Color.White;
			}

			return submesh.Material.DiffuseColor;
		}

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
