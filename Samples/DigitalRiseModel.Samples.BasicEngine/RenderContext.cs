using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace DigitalRiseModel
{
	public enum EffectType
	{
		Basic,
		Skinned
	}

	public class RenderContext
	{
		private SkinnedEffect _skinnedEffect;
		private BasicEffect _basicEffect;
		private Effect[] _effects = new Effect[2];

		public CameraNode Camera { get; internal set; }
		public GraphicsDevice GraphicsDevice { get; }
		public Texture2D WhiteTexture { get; }

		internal RenderContext(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

			WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
			WhiteTexture.SetData(new Color[] { Color.White });

			_basicEffect = new BasicEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				TextureEnabled = true,
				PreferPerPixelLighting = true
			};
			_effects[0] = _basicEffect;

			_skinnedEffect = new SkinnedEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				PreferPerPixelLighting = true
			};
			_effects[1] = _skinnedEffect;

			foreach (IEffectLights effect in _effects)
			{
				effect.LightingEnabled = true;

				effect.DirectionalLight0.DiffuseColor = Vector3.One;
				effect.DirectionalLight0.Direction = new Vector3(0, -1, -1);
				effect.DirectionalLight0.Enabled = true;

				effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.2f, 1.0f);
				effect.DirectionalLight1.Direction = new Vector3(0, 0, 1);
				effect.DirectionalLight1.Enabled = true;
			}
		}

		internal void Prepare(CameraNode camera)
		{
			var proj = camera.CalculateProjection(GraphicsDevice.Viewport.AspectRatio);
			foreach (IEffectMatrices effect in _effects)
			{
				effect.View = camera.View;
				effect.Projection = proj;
			}
		}

		public void Render(DrSubmesh submesh, EffectType effectType, Matrix transform, Texture2D texture, Color color, Matrix[] boneTransforms)
		{
			Effect effect;
			if (effectType == EffectType.Basic)
			{
				_basicEffect.World = transform;
				_basicEffect.Texture = texture;
				_basicEffect.DiffuseColor = color.ToVector3();
				effect = _basicEffect;
			}
			else
			{
				_skinnedEffect.World = transform;
				_skinnedEffect.SetBoneTransforms(boneTransforms);
				_skinnedEffect.Texture = texture;
				_skinnedEffect.DiffuseColor = color.ToVector3();
				effect = _skinnedEffect;
			}

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				submesh.Draw(GraphicsDevice);
			}
		}
	}
}
