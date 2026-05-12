using Microsoft.Xna.Framework;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents a mesh that contains one or more mesh parts.
	/// </summary>
	public class DrMesh : DrDisposable
	{
		private BoundingBox? _boundingBox;

		/// <summary>
		/// Gets or sets the name of this mesh.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the bone that this mesh is attached to.
		/// </summary>
		public DrModelBone ParentBone { get; internal set; }

		/// <summary>
		/// Gets the bounding box that contains all mesh parts in this mesh.
		/// </summary>
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

		/// <summary>
		/// Gets the collection of mesh parts that comprise this mesh.
		/// </summary>
		public ObservableCollection<DrMeshPart> MeshParts { get; } = new ObservableCollection<DrMeshPart>();

		/// <summary>
		/// Gets or sets an arbitrary object associated with this mesh.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMesh"/> class.
		/// </summary>
		public DrMesh()
		{
			MeshParts.CollectionChanged += MeshParts_CollectionChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMesh"/> class with the specified mesh part.
		/// </summary>
		/// <param name="part">The mesh part to add to this mesh.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="part"/> is null.</exception>
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
