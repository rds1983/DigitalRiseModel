using DigitalRiseModel.Samples.BasicEngine;

namespace DigitalRiseModel
{
	/// <summary>
	/// A visual node that renders an animated model instance.
	/// Handles both skinned (skeletal animation) and non-skinned mesh parts.
	/// Each mesh part's rendering is determined by whether it has a skin attachment.
	/// </summary>
	public class ModelInstanceNode : VisualNode
	{
		/// <summary>
		/// Gets or sets the model instance to be rendered by this node.
		/// The instance tracks the current pose of the model's skeleton.
		/// </summary>
		public DrModelInstance ModelInstance { get; set; }

		/// <summary>
		/// Renders this model instance node and all its child nodes.
		/// Handles both skinned (with bone transforms) and non-skinned (basic) rendering.
		/// </summary>
		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			// Return early if there's nothing to render
			if (ModelInstance == null || ModelInstance.Model == null)
			{
				return;
			}

			// Render each mesh in the model
			foreach (var mesh in ModelInstance.Model.Meshes)
			{
				// Render each mesh part
				foreach (var part in mesh.MeshParts)
				{
					// Determine the texture and color to use
					var texture = GetTextureForMeshPart(part, context.WhiteTexture);
					var color = GetColorForMeshPart(part);

					// Check if this mesh part uses skeletal animation (has a skin)
					if (part.Skin != null)
					{
						// Render using the Skinned effect with bone transforms
						var skinTransforms = ModelInstance.GetSkinTransforms(part.Skin.SkinIndex);
						context.Render(part, EffectType.Skinned, GlobalTransform, texture, color, skinTransforms);
					}
					else
					{
						// Render using the Basic effect, transforming by the bone's current transform
						var boneTransform = ModelInstance.GetBoneGlobalTransform(mesh.ParentBone.Index) * GlobalTransform;
						context.Render(part, EffectType.Basic, boneTransform, texture, color, null);
					}
				}
			}
		}
	}
}
