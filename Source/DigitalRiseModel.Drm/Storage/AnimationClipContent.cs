using System.Collections.Generic;

namespace DigitalRiseModel.Storage
{
	internal class AnimationClipContent
	{
		public string Name { get; set; }
		public List<AnimationChannelContent> Channels { get; set; } = new List<AnimationChannelContent>();
	}
}
