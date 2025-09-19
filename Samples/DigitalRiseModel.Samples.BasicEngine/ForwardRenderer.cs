using Microsoft.Xna.Framework.Graphics;
using static DigitalRiseModel.RenderContext;

namespace DigitalRiseModel.Samples.BasicEngine
{
	public class ForwardRenderer
	{
		private RenderContext RenderContext { get; }
		private GraphicsDevice GraphicsDevice => RenderContext.GraphicsDevice;
		public Texture2D WhiteTexture => RenderContext.WhiteTexture;

		public DirectionalLightWrapper DirectionalLight0 => RenderContext.DirectionalLight0;
		public DirectionalLightWrapper DirectionalLight1 => RenderContext.DirectionalLight1;
		public DirectionalLightWrapper DirectionalLight2 => RenderContext.DirectionalLight2;

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
			var oldRasterizerState = GraphicsDevice.RasterizerState;
			var oldDepthStencilState = GraphicsDevice.DepthStencilState;
			var oldBlendState = GraphicsDevice.BlendState;
			var oldSamplerState = GraphicsDevice.SamplerStates[0];

			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			RenderContext.Prepare(camera);

			rootNode.IterateRecursive(RenderNode);

			GraphicsDevice.RasterizerState = oldRasterizerState;
			GraphicsDevice.DepthStencilState = oldDepthStencilState;
			GraphicsDevice.BlendState = oldBlendState;
			GraphicsDevice.SamplerStates[0] = oldSamplerState;
		}
	}
}
