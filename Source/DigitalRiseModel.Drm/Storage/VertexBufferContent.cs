using DigitalRiseModel.Storage.Binary;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace DigitalRiseModel.Storage
{
	internal class VertexBufferContent
	{
		private int? _vertexStride;
		private byte[] _data;
		private readonly MemoryStream _stream = new MemoryStream();

		public int BufferId { get; set; }

		[JsonIgnore]
		public int VertexStride
		{
			get
			{
				Update();
				return _vertexStride.Value;
			}
		}

		[JsonIgnore]
		public int MemorySizeInBytes => (int)_stream.Length;

		[JsonIgnore]
		public int MemoryVertexCount => MemorySizeInBytes / VertexStride;

		public int VertexCount { get; set; }

		[JsonIgnore]
		public byte[] Data
		{
			get
			{
				if (_data == null)
				{
					_data = _stream.ToArray();
				}

				return _data;
			}

			set
			{
				_data = value;
			}
		}

		public ObservableCollection<VertexElementContent> Elements { get; set; } = new ObservableCollection<VertexElementContent>();

		public VertexBufferContent()
		{
			Elements.CollectionChanged += Channels_CollectionChanged;
		}

		private void Channels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_vertexStride = null;
		}

		public bool HasChannel(VertexElementUsage usage)
		{
			return (from c in Elements where c.Usage == usage select c).Count() > 0;
		}

		private void Update()
		{
			if (_vertexStride != null)
			{
				return;
			}

			_vertexStride = Elements.CalculateStride();
		}

		public void Write(byte[] data)
		{
			_stream.Write(data, 0, data.Length);
			_data = null;
		}

		public byte[] GetMemoryData() => _stream.GetBuffer();

		internal void LoadBinaryData(ReadContext ctx)
		{
			Data = ctx.ReadByteArray(BufferId);

			if (Data.Length % VertexStride != 0)
			{
				throw new Exception($"Inconsistent data size. Data.Length={Data.Length}, VertexStride={VertexStride}");
			}

			VertexCount = Data.Length / VertexStride;
		}

		internal void SaveBinaryData(WriteContext ctx)
		{
			VertexCount = MemoryVertexCount;
			BufferId = ctx.WriteData(Data);
		}
	}
}
