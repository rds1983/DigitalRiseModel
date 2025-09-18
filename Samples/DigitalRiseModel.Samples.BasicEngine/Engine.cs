using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DigitalRiseModel.Samples.BasicEngine
{
	public class Engine
	{
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SkinnedEffect _skinnedEffect;
		private BasicEffect _basicEffect;
		private Effect[] _effects = new Effect[2];

		public GraphicsDevice GraphicsDevice { get; }
		public Camera Camera { get; } = new Camera();

		public List<DrModelInstance> Models { get; } = new List<DrModelInstance>();
		public Texture2D CustomTexture { get; set; }
		public Color? CustomColor { get; set; }
		public Texture2D WhiteTexture { get; }

		public Engine(GraphicsDevice device)
		{
			GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));

			WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
			WhiteTexture.SetData(new Color[] { Color.White });

			_skinnedEffect = new SkinnedEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				PreferPerPixelLighting = true
			};
			_effects[0] = _skinnedEffect;

			_basicEffect = new BasicEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				TextureEnabled = true,
				PreferPerPixelLighting = true
			};
			_effects[1] = _basicEffect;

			foreach (IEffectLights effect in _effects)
			{
				effect.DirectionalLight0.DiffuseColor = Vector3.One;
				effect.DirectionalLight0.Direction = new Vector3(0, -1, -1);
				effect.DirectionalLight0.Enabled = true;

				effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.2f, 1.0f);
				effect.DirectionalLight1.Direction = new Vector3(0, 0, 1);
				effect.DirectionalLight1.Enabled = true;
			}
		}

		private Texture2D GetCurrentTexture(DrSubmesh submesh)
		{
			if (CustomTexture != null)
			{
				return CustomTexture;
			}

			if (submesh.Material == null || submesh.Material.DiffuseTexture == null)
			{
				return WhiteTexture;
			}

			return submesh.Material.DiffuseTexture;
		}

		private Color GetCurrentColor(DrSubmesh submesh)
		{
			if (CustomColor != null)
			{
				return CustomColor.Value;
			}

			if (submesh.Material == null || submesh.Material.DiffuseColor == null)
			{
				return Color.White;
			}

			return submesh.Material.DiffuseColor.Value;
		}

		private void DrawModel(DrModelInstance model)
		{
			if (model.Model == null)
			{
				return;
			}

			var proj = Camera.CalculateProjection(GraphicsDevice.Viewport.AspectRatio);
			foreach (IEffectMatrices effect in _effects)
			{
				effect.View = Camera.View;
				effect.Projection = proj;
			}

			foreach (var bone in model.Model.Bones)
			{
				if (bone.Mesh == null)
				{
					continue;
				}

				foreach (var submesh in bone.Mesh.Submeshes)
				{
					Effect effect;
					if (submesh.Skin != null)
					{
						_skinnedEffect.Texture = GetCurrentTexture(submesh);
						_skinnedEffect.DiffuseColor = GetCurrentColor(submesh).ToVector3();

						var skinTransforms = model.GetSkinTransforms(submesh.Skin.SkinIndex);
						_skinnedEffect.SetBoneTransforms(skinTransforms);
						effect = _skinnedEffect;
					}
					else
					{
						_basicEffect.Texture = GetCurrentTexture(submesh);
						_basicEffect.DiffuseColor = GetCurrentColor(submesh).ToVector3();

						var boneTransform = model.GetBoneGlobalTransform(bone.Index);
						_basicEffect.World = boneTransform;
						effect = _basicEffect;
					}

					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();

						submesh.Draw();
					}
				}
			}
		}

		public void Render()
		{
			if (Models.Count == 0)
			{
				return;
			}

			var oldRasterizer = GraphicsDevice.RasterizerState;
			var oldDepthStencilState = GraphicsDevice.DepthStencilState;
			var oldBlendState = GraphicsDevice.BlendState;

			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;

			foreach (var model in Models)
			{
				DrawModel(model);
			}

			GraphicsDevice.RasterizerState = oldRasterizer;
			GraphicsDevice.DepthStencilState = oldDepthStencilState;
			GraphicsDevice.BlendState = oldBlendState;
		}
	}
}
