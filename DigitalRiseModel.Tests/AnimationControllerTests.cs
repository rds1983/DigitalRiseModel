using DigitalRiseModel.Animation;
using System;
using Xunit;

namespace DigitalRiseModel.Tests
{
	public sealed class AnimationControllerTests
	{
		/// <summary>
		/// Makes sure there's exception if AnimationController.Update is called without setting an animation
		/// </summary>
		[Fact]
		public void UpdateWithoutAnimation()
		{
			var manager = Utility.CreateAssetManager();
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "dude.glb", ModelLoadFlags.IgnoreMaterials);

			var modelInstance = new DrModelInstance(model);
			var player = new AnimationController(modelInstance);

			Assert.Throws<Exception>(() => player.Update(TimeSpan.FromMilliseconds(1)));
		}
	}
}
