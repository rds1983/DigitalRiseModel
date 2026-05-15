using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents an attachment of a model instance to a bone of a parent model.
	/// Used to attach accessories or child models that follow bone animations.
	/// </summary>
	public class ModelBoneAttachment
	{
		/// <summary>
		/// Gets or sets the model instance to attach.
		/// </summary>
		public DrModelInstance Model { get; set; }

		/// <summary>
		/// Gets or sets the target bone on the parent model to attach to.
		/// </summary>
		public DrModelBone Bone { get; set; }

		/// <summary>
		/// Gets or sets the local transform applied to the attached model relative to the bone.
		/// Default is identity (no additional transform).
		/// </summary>
		public Matrix Transform { get; set; } = Matrix.Identity;
	}

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
		/// Gets the collection of models attached to bones of this model instance.
		/// </summary>
		public List<ModelBoneAttachment> BonesAttachments { get; } = new List<ModelBoneAttachment>();

		/// <summary>
		/// Renders a model instance with the specified transform.
		/// Determines for each mesh part whether to use skeletal animation or basic rendering.
		/// </summary>
		/// <param name="context">The render context used for rendering.</param>
		/// <param name="model">The model instance to render.</param>
		/// <param name="transform">The world transformation matrix for the model.</param>
		private void RenderModel(RenderContext context, DrModelInstance model, Matrix transform)
		{
			// Return early if there's nothing to render
			if (model == null || model.Model == null)
			{
				return;
			}

			// Render each mesh in the model
			foreach (var mesh in model.Model.Meshes)
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
						var skinTransforms = model.GetSkinTransforms(part.Skin.SkinIndex);
						context.Render(part, EffectType.Skinned, transform, texture, color, skinTransforms);
					}
					else
					{
						// Render using the Basic effect, transforming by the bone's current transform
						var boneTransform = model.GetBoneGlobalTransform(mesh.ParentBone.Index) * transform;
						context.Render(part, EffectType.Basic, boneTransform, texture, color, null);
					}
				}
			}
		}

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

			RenderModel(context, ModelInstance, GlobalTransform);

			// Render attachments
			foreach (var attachment in BonesAttachments)
			{
				if (attachment.Model == null || attachment.Bone == null)
				{
					continue;
				}

				var transform = attachment.Transform * ModelInstance.GetBoneGlobalTransform(attachment.Bone.Index) * GlobalTransform;
				RenderModel(context, attachment.Model, transform);
			}
		}
	}
}
