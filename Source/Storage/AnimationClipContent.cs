using System.Collections.Generic;

namespace DigitalRiseModel.Storage
{
	public class AnimationClipContent
	{
		public string Name { get; set; }
		public List<AnimationChannelContent> Channels { get; set; } = new List<AnimationChannelContent>();
	}
}
