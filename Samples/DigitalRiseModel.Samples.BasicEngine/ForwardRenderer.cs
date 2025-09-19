using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Samples.BasicEngine
{
	public class ForwardRenderer
	{
		private RenderContext RenderContext { get; }
		private GraphicsDevice GraphicsDevice => RenderContext.GraphicsDevice;
		public Texture2D WhiteTexture => RenderContext.WhiteTexture;

		public ForwardRenderer(GraphicsDevice graphicsDevice)
		{
			RenderContext = new RenderContext(graphicsDevice);
		}

		private void RenderNode(SceneNode node)
		{
			node.Render(RenderContext);
		}

		public void Render(CameraNode camera, SceneNode rootNode)
		{
			var oldRasterizer = GraphicsDevice.RasterizerState;
			var oldDepthStencilState = GraphicsDevice.DepthStencilState;
			var oldBlendState = GraphicsDevice.BlendState;

			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;

			RenderContext.Prepare(camera);

			rootNode.IterateRecursive(RenderNode);

			GraphicsDevice.RasterizerState = oldRasterizer;
			GraphicsDevice.DepthStencilState = oldDepthStencilState;
			GraphicsDevice.BlendState = oldBlendState;
		}
	}
}
