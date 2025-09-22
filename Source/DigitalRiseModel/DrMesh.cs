using Microsoft.Xna.Framework;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DigitalRiseModel
{
	public class DrMesh : DrDisposable
	{
		private BoundingBox? _boundingBox;

		public string Name { get; set; }
		public DrModelBone Bone { get; internal set; }

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

		public ObservableCollection<DrMeshPart> MeshParts { get; } = new ObservableCollection<DrMeshPart>();

		public object Tag { get; set; }

		public DrMesh()
		{
			MeshParts.CollectionChanged += MeshParts_CollectionChanged;
		}

		public DrMesh(DrMeshPart part): this()
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
				foreach (DrMeshPart n in args.NewItems)
				{
					n.Mesh = this;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (DrMeshPart n in args.OldItems)

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
