using DigitalRiseModel.Primitives;
using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	/// <summary>
	/// Specifies which effect/shader to use for rendering mesh parts.
	/// </summary>
	public enum EffectType
	{
		/// <summary>Basic effect with no bone/skeletal animation support.</summary>
		Basic,
		/// <summary>Skinned effect for skeletal animation with bone transforms.</summary>
		Skinned
	}

	/// <summary>
	/// Manages rendering state, effects, lighting, and statistics for the application.
	/// Provides methods to render mesh parts with different effect types and handles camera setup.
	/// </summary>
	public class RenderContext
	{
		/// <summary>
		/// Wrapper for a directional light that keeps track of when properties change.
		/// When properties are modified, it invalidates the lighting cache so effects are updated on next render.
		/// </summary>
		public class DirectionalLightWrapper
		{
			private readonly RenderContext _context;

			private Color _diffuseColor;
			private Vector3 _direction;
			private Color specularColor;
			private bool enabled;

			/// <summary>
			/// Initializes a new instance of the DirectionalLightWrapper class.
			/// </summary>
			internal DirectionalLightWrapper(RenderContext context)
			{
				_context = context ?? throw new ArgumentNullException(nameof(context));
			}

			/// <summary>Gets or sets the diffuse color of this directional light.</summary>
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

			/// <summary>Gets or sets the direction this light is shining from (typically normalized).</summary>
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

			/// <summary>Gets or sets the specular color of this directional light.</summary>
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

			/// <summary>Gets or sets whether this directional light is enabled.</summary>
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

		// Built-in effects used for rendering different types of geometry
		private SkinnedEffect _skinnedEffect;
		private BasicEffect _basicEffect;
		private BasicEffect _lineEffect;
		private Effect[] _effects = new Effect[2];
		// Flag indicating that light properties have changed and need to be synced to effects
		private bool _lightsDirty = true;
		// Tracks the last effect used to optimize effect switches
		private EffectType? _lastEffect = null;
		private RenderStatistics _statistics;

		/// <summary>
		/// Gets rendering statistics (draw calls, primitives drawn, etc.) from the last render frame.
		/// </summary>
		public RenderStatistics Statistics => _statistics;

		/// <summary>
		/// Gets or sets the camera used for this render context.
		/// </summary>
		public CameraNode Camera { get; internal set; }

		/// <summary>
		/// Gets the graphics device used for rendering.
		/// </summary>
		public GraphicsDevice GraphicsDevice { get; }

		/// <summary>
		/// Gets a 1x1 white texture used as a default when no texture is specified.
		/// </summary>
		public Texture2D WhiteTexture { get; }

		/// <summary>
		/// Gets the first directional light. Used in shader calculations.
		/// </summary>
		public DirectionalLightWrapper DirectionalLight0 { get; }

		/// <summary>
		/// Gets the second directional light. Used in shader calculations.
		/// </summary>
		public DirectionalLightWrapper DirectionalLight1 { get; }

		/// <summary>
		/// Gets the third directional light. Used in shader calculations.
		/// </summary>
		public DirectionalLightWrapper DirectionalLight2 { get; }

		/// <summary>
		/// Gets the view frustum used for culling. Updated each frame based on camera.
		/// </summary>
		public BoundingFrustum BoundingFrustum { get; private set; }

		/// <summary>
		/// Gets or sets whether bounding boxes should be drawn for debug visualization.
		/// </summary>
		public bool DrawBoundingBoxes { get; set; } = false;

		/// <summary>
		/// Mesh used for drawing bounding boxes in wireframe.
		/// </summary>
		private DrMeshPart BoundingBoxMesh { get; }

		/// <summary>
		/// Initializes a new instance of the RenderContext class.
		/// Sets up graphics effects and default resources.
		/// </summary>
		internal RenderContext(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

			// Create a 1x1 white texture for default texture when none is specified
			WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
			WhiteTexture.SetData(new Color[] { Color.White });

			// Set up basic effect for non-skinned geometry
			_basicEffect = new BasicEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				TextureEnabled = true,
				PreferPerPixelLighting = true,
				LightingEnabled = true,
			};
			_effects[0] = _basicEffect;

			// Set up skinned effect for skeletal animation
			_skinnedEffect = new SkinnedEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One,
				PreferPerPixelLighting = true,
			};
			_effects[1] = _skinnedEffect;

			// Set up line effect for debug wireframe rendering
			_lineEffect = new BasicEffect(GraphicsDevice)
			{
				DiffuseColor = Color.GreenYellow.ToVector3()
			};

			// Create mesh for rendering bounding box debug geometry
			BoundingBoxMesh = MeshPrimitives.CreateBoxLinesMeshPart(GraphicsDevice, new BoundingBox(Vector3.Zero, Vector3.One));

			// Initialize the three directional lights
			DirectionalLight0 = new DirectionalLightWrapper(this);
			DirectionalLight1 = new DirectionalLightWrapper(this);
			DirectionalLight2 = new DirectionalLightWrapper(this);
		}

		/// <summary>
		/// Synchronizes the light wrapper properties to the graphics effects.
		/// This is called once per frame to update lights if they've changed.
		/// </summary>
		private void UpdateLights()
		{
			if (!_lightsDirty)
			{
				return; // Lights haven't changed, nothing to do
			}

			// Apply light parameters to all effects that support lighting
			foreach (IEffectLights effect in _effects)
			{
				// Update first directional light
				effect.DirectionalLight0.DiffuseColor = DirectionalLight0.DiffuseColor.ToVector3();
				effect.DirectionalLight0.SpecularColor = DirectionalLight0.SpecularColor.ToVector3();
				effect.DirectionalLight0.Direction = DirectionalLight0.Direction;
				effect.DirectionalLight0.Enabled = DirectionalLight0.Enabled;

				// Update second directional light
				effect.DirectionalLight1.DiffuseColor = DirectionalLight1.DiffuseColor.ToVector3();
				effect.DirectionalLight1.SpecularColor = DirectionalLight1.SpecularColor.ToVector3();
				effect.DirectionalLight1.Direction = DirectionalLight1.Direction;
				effect.DirectionalLight1.Enabled = DirectionalLight1.Enabled;

				// Update third directional light
				effect.DirectionalLight2.DiffuseColor = DirectionalLight2.DiffuseColor.ToVector3();
				effect.DirectionalLight2.SpecularColor = DirectionalLight2.SpecularColor.ToVector3();
				effect.DirectionalLight2.Direction = DirectionalLight2.Direction;
				effect.DirectionalLight2.Enabled = DirectionalLight2.Enabled;
			}

			_lightsDirty = false;
		}

		/// <summary>
		/// Prepares the render context for rendering using the specified camera.
		/// Updates view/projection matrices, view frustum, and light parameters.
		/// </summary>
		internal void Prepare(CameraNode camera)
		{
			// Update all effect light properties if they've changed
			UpdateLights();

			// Calculate and apply projection matrix
			var proj = camera.CalculateProjection(GraphicsDevice.Viewport.AspectRatio);
			foreach (IEffectMatrices effect in _effects)
			{
				effect.View = camera.View;
				effect.Projection = proj;
			}

			// Also set view/projection for line drawing effect
			_lineEffect.View = camera.View;
			_lineEffect.Projection = proj;

			// Update the view frustum for culling
			if (BoundingFrustum == null)
			{
				BoundingFrustum = new BoundingFrustum(camera.View * proj);
			}
			else
			{
				BoundingFrustum.Matrix = camera.View * proj;
			}

			// Reset statistics for this frame
			_statistics.Reset();
			_lastEffect = null;
		}

		/// <summary>
		/// Renders a mesh part with the specified parameters.
		/// Handles frustum culling, effect setup, bone transforms, and statistics tracking.
		/// </summary>
		/// <param name="part">The mesh part to render.</param>
		/// <param name="effectType">Which effect to use (Basic for static, Skinned for animated).</param>
		/// <param name="transform">The world transformation matrix for this mesh.</param>
		/// <param name="texture">The texture to apply (can be null).</param>
		/// <param name="color">The color to apply to the mesh.</param>
		/// <param name="boneTransforms">Bone transforms for skinned rendering (null for basic rendering).</param>
		public void Render(DrMeshPart part, EffectType effectType, Matrix transform, Texture2D texture, Color color, Matrix[] boneTransforms)
		{
			// Transform the mesh's bounding box to world space
			var boundingBox = part.BoundingBox.Transform(ref transform);

			// Optionally draw bounding box for debug visualization
			if (DrawBoundingBoxes)
			{
				var scale = Matrix.CreateScale(boundingBox.ToScale()) * Matrix.CreateTranslation(boundingBox.Min);
				_lineEffect.World = scale;
				foreach (var pass in _lineEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					BoundingBoxMesh.Draw(GraphicsDevice);
				}
			}

			// Skip rendering if the mesh is completely outside the view frustum
			if (BoundingFrustum.Contains(boundingBox) == ContainmentType.Disjoint)
			{
				return;
			}

			// Track effect switches for performance analysis
			if (_lastEffect != effectType)
			{
				++_statistics.EffectsSwitches;
			}

			// Set up the appropriate effect and apply parameters
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
				// Skinned effect: apply bone transforms for skeletal animation
				_skinnedEffect.World = transform;
				_skinnedEffect.SetBoneTransforms(boneTransforms);
				_skinnedEffect.Texture = texture;
				_skinnedEffect.DiffuseColor = color.ToVector3();
				effect = _skinnedEffect;
			}

			// Execute all passes of the effect and draw the mesh
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				part.Draw(GraphicsDevice);

				// Update render statistics
				++_statistics.DrawCalls;
				_statistics.VerticesDrawn += part.NumVertices;
				_statistics.PrimitivesDrawn += part.PrimitiveCount;
			}

			++_statistics.MeshesDrawn;
			_lastEffect = effectType;
		}

		/// <summary>
		/// Marks lights as needing to be updated. Called when light properties change.
		/// The lights will be synced to effects on the next Prepare call.
		/// </summary>
		private void InvalidateLights()
		{
			_lightsDirty = true;
		}
	}
}
