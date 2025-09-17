using System.Collections;
using System.Collections.Generic;

namespace DigitalRiseModel.Morph
{
	public class MorphTargetCollection: IEnumerable<MorphTarget>
	{
		private readonly Dictionary<string, MorphTarget> _data = new Dictionary<string, MorphTarget>();

		internal DrSubmesh Submesh { get; set; }

		public int Count => _data.Count;

		public IEnumerator<MorphTarget> GetEnumerator() => _data.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _data.Values.GetEnumerator();
	}
}
