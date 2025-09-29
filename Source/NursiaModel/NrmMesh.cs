using Microsoft.Xna.Framework;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NursiaModel
{
	public class NrmMesh : NrmDisposable
	{
		private BoundingBox? _boundingBox;

		public string Name { get; set; }
		public NrmModelBone ParentBone { get; internal set; }

		public BoundingBox BoundingBox
		{
			get
			{
				if (_boundingBox == null)
				{
					var bb = new BoundingBox();
					foreach (var meshPart in MeshParts)
					{
						bb = BoundingBox.CreateMerged(bb, meshPart.BoundingBox);
					}

					_boundingBox = bb;
				}

				return _boundingBox.Value;
			}
		}

		public ObservableCollection<NrmMeshPart> MeshParts { get; } = new ObservableCollection<NrmMeshPart>();

		public object Tag { get; set; }

		public NrmMesh()
		{
			MeshParts.CollectionChanged += MeshParts_CollectionChanged;
		}

		public NrmMesh(NrmMeshPart part): this()
		{
			if (part == null)
			{
				throw new ArgumentNullException(nameof(part));
			}

			MeshParts.Add(part);
		}

		private void MeshParts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (NrmMeshPart n in args.NewItems)
				{
					n.Mesh = this;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (NrmMeshPart n in args.OldItems)

					n.Mesh = null;
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var w in MeshParts)
				{
					w.Mesh = null;
				}
			}

			_boundingBox = null;
		}

		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach(var part in MeshParts)
				{
					part.Dispose();
				}
			}
		}
	}
}
