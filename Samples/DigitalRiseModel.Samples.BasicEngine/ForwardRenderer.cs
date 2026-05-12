using Microsoft.Xna.Framework.Graphics;
using static DigitalRiseModel.RenderContext;

namespace DigitalRiseModel.Samples.BasicEngine
{
	/// <summary>
	/// Implements a forward rendering system that renders scenes using a camera and hierarchical scene graph.
	/// This class manages rendering state, delegates to the RenderContext, and iterates through the scene hierarchy.
	/// </summary>
	public class ForwardRenderer
	{
		// Internal render context that handles effects, lighting, and actual mesh rendering
		private RenderContext RenderContext { get; }

		/// <summary>
		/// Gets the graphics device used for rendering.
		/// </summary>
		private GraphicsDevice GraphicsDevice => RenderContext.GraphicsDevice;

		/// <summary>
		/// Gets the white texture used as a default when no texture is specified.
		/// </summary>
		public Texture2D WhiteTexture => RenderContext.WhiteTexture;

		/// <summary>Gets the first directional light.</summary>
		public DirectionalLightWrapper DirectionalLight0 => RenderContext.DirectionalLight0;

		/// <summary>Gets the second directional light.</summary>
		public DirectionalLightWrapper DirectionalLight1 => RenderContext.DirectionalLight1;

		/// <summary>Gets the third directional light.</summary>
		public DirectionalLightWrapper DirectionalLight2 => RenderContext.DirectionalLight2;

		/// <summary>
		/// Gets rendering statistics from the last frame (draw calls, vertices drawn, etc.).
		/// </summary>
		public RenderStatistics Statistics => RenderContext.Statistics;

		/// <summary>
		/// Gets or sets whether bounding boxes should be drawn for debug visualization.
		/// </summary>
		public bool DrawBoundingBoxes
		{
			get => RenderContext.DrawBoundingBoxes;
			set => RenderContext.DrawBoundingBoxes = value;
		}

		/// <summary>
		/// Initializes a new instance of the ForwardRenderer class.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used for rendering.</param>
		public ForwardRenderer(GraphicsDevice graphicsDevice)
		{
			RenderContext = new RenderContext(graphicsDevice);
		}

		/// <summary>
		/// Renders a single scene node using the current render context.
		/// Called recursively for all nodes in the scene hierarchy.
		/// </summary>
		private void RenderNode(SceneNode node)
		{
			node.Render(RenderContext);
		}

		/// <summary>
		/// Renders the entire scene from the specified camera viewpoint.
		/// Saves and restores graphics device state around the rendering process.
		/// </summary>
		/// <param name="camera">The camera defining the view and projection.</param>
		/// <param name="rootNode">The root of the scene hierarchy to render.</param>
		public void Render(CameraNode camera, SceneNode rootNode)
		{
			// Save current graphics device state to restore after rendering
			var oldRasterizerState = GraphicsDevice.RasterizerState;
			var oldDepthStencilState = GraphicsDevice.DepthStencilState;
			var oldBlendState = GraphicsDevice.BlendState;
			var oldSamplerState = GraphicsDevice.SamplerStates[0];

			// Set graphics device state for 3D rendering
			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise; // Cull back faces
			GraphicsDevice.DepthStencilState = DepthStencilState.Default; // Standard depth testing
			GraphicsDevice.BlendState = BlendState.Opaque; // No blending (opaque rendering)
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap; // Texture filtering

			// Prepare rendering context with camera view and projection
			RenderContext.Prepare(camera);

			// Recursively render all nodes in the scene hierarchy
			rootNode.IterateRecursive(RenderNode);

			// Restore previous graphics device state
			GraphicsDevice.RasterizerState = oldRasterizerState;
			GraphicsDevice.DepthStencilState = oldDepthStencilState;
			GraphicsDevice.BlendState = oldBlendState;
			GraphicsDevice.SamplerStates[0] = oldSamplerState;
		}
	}
}
