using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	public enum EffectType
	{
		Basic,
		Skinned
	}

	public class RenderContext
	{
		public class DirectionalLightWrapper
		{
			private readonly RenderContext _context;

			private Color _diffuseColor;
			private Vector3 _direction;
			private Color specularColor;
			private bool enabled;

			internal DirectionalLightWrapper(RenderContext context)
			{
				_context = context ?? throw new ArgumentNullException(nameof(context));
			}

			public Color DiffuseColor
			{
				get => _diffuseColor;

				set
				{
					if (_diffuseColor == value)
					{
						return;
					}
					_diffuseColor = value;
					_context.InvalidateLights();
				}
			}

			public Vector3 Direction
			{
				get => _direction;

				set
				{
					if (value.EpsilonEquals(_direction))
					{
						return;
					}

					_direction = value;
					_context.InvalidateLights();
				}
			}

			public Color SpecularColor
			{
				get => specularColor;

				set
				{

					if (value == specularColor)
					{
						return;
					}

					specularColor = value;
					_context.InvalidateLights();
				}
			}

			public bool Enabled
			{
				get => enabled;

				set
				{
					if (enabled == value)
					{
						return;
					}

					enabled = value;
					_context.InvalidateLights();
				}
			}
		}

		private SkinnedEffect _skinnedEffect;
		private BasicEffect _basicEffect;
		private Effect[] _effects = new Effect[2];
		private bool _lightsDirty = true;

		public CameraNode Camera { get; internal set; }
		public GraphicsDevice GraphicsDevice { get; }
		public Texture2D WhiteTexture { get; }

		public DirectionalLightWrapper DirectionalLight0 { get; }
		public DirectionalLightWrapper DirectionalLight1 { get; }
		public DirectionalLightWrapper DirectionalLight2 { get; }

		internal RenderContext(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

			WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
			WhiteTexture.SetData(new Color[] { Color.White });

			_basicEffect = new BasicEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				TextureEnabled = true,
				PreferPerPixelLighting = true,
				LightingEnabled = true,
			};
			_effects[0] = _basicEffect;

			_skinnedEffect = new SkinnedEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				PreferPerPixelLighting = true,
			};
			_effects[1] = _skinnedEffect;

			DirectionalLight0 = new DirectionalLightWrapper(this);
			DirectionalLight1 = new DirectionalLightWrapper(this);
			DirectionalLight2 = new DirectionalLightWrapper(this);
		}

		private void UpdateLights()
		{
			if (!_lightsDirty)
			{
				return;
			}

			foreach (IEffectLights effect in _effects)
			{
				effect.DirectionalLight0.DiffuseColor = DirectionalLight0.DiffuseColor.ToVector3();
				effect.DirectionalLight0.SpecularColor = DirectionalLight0.SpecularColor.ToVector3();
				effect.DirectionalLight0.Direction = DirectionalLight0.Direction;
				effect.DirectionalLight0.Enabled = DirectionalLight0.Enabled;

				effect.DirectionalLight1.DiffuseColor = DirectionalLight1.DiffuseColor.ToVector3();
				effect.DirectionalLight1.SpecularColor = DirectionalLight1.SpecularColor.ToVector3();
				effect.DirectionalLight1.Direction = DirectionalLight1.Direction;
				effect.DirectionalLight1.Enabled = DirectionalLight1.Enabled;

				effect.DirectionalLight2.DiffuseColor = DirectionalLight2.DiffuseColor.ToVector3();
				effect.DirectionalLight2.SpecularColor = DirectionalLight2.SpecularColor.ToVector3();
				effect.DirectionalLight2.Direction = DirectionalLight2.Direction;
				effect.DirectionalLight2.Enabled = DirectionalLight2.Enabled;
			}

			_lightsDirty = false;
		}

		internal void Prepare(CameraNode camera)
		{
			UpdateLights();

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

		private void InvalidateLights()
		{
			_lightsDirty = true;
		}
	}
}
