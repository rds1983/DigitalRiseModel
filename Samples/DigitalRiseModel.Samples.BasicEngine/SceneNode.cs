using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DigitalRiseModel.Samples.BasicEngine
{
	/// <summary>
	/// Represents a node in the scene hierarchy. Each node has a transform (position, rotation, scale) and can have child nodes.
	/// This class is the foundation for building a hierarchical scene graph used in rendering and game logic.
	/// </summary>
	public class SceneNode
	{
		// Stores the local position, rotation (in Euler angles), and scale of this node.
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;

		// Cached transformation matrices. Set to null when invalidated to force recalculation.
		private Matrix? _globalTransform = null, _localTransform = null;

		/// <summary>
		/// Gets or sets the position of this node relative to its parent.
		/// </summary>
		public Vector3 Translation
		{
			get => _translation;

			set
			{
				if (value == _translation)
				{
					return;
				}

				_translation = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets or sets the scale of this node relative to its parent. Default is (1, 1, 1).
		/// </summary>
		public Vector3 Scale
		{
			get => _scale;

			set
			{
				if (value == _scale)
				{
					return;
				}

				_scale = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets or sets the rotation of this node in Euler angles (pitch, yaw, roll) in degrees, relative to its parent.
		/// Angles are automatically clamped to 0-360 degrees.
		/// </summary>
		public Vector3 Rotation
		{
			get => _rotation;

			set
			{
				// Clamp rotation angles to valid degree range [0, 360)
				value.X = value.X.ClampDegree();
				value.Y = value.Y.ClampDegree();
				value.Z = value.Z.ClampDegree();

				if (value == _rotation)
				{
					return;
				}

				_rotation = value;
				InvalidateTransform();
			}
		}

		/// <summary>
		/// Gets the local transformation matrix (relative to parent) composed from Translation, Rotation, and Scale.
		/// This matrix is cached and only recalculated when the transform is invalidated.
		/// </summary>
		public Matrix LocalTransform
		{
			get
			{
				if (_localTransform == null)
				{
					// Convert Euler angles (in degrees) to a quaternion
					var quaternion = Quaternion.CreateFromYawPitchRoll(
											MathHelper.ToRadians(_rotation.Y),
											MathHelper.ToRadians(_rotation.X),
											MathHelper.ToRadians(_rotation.Z));
					// Create the transformation matrix from scale, rotation, and translation
					_localTransform = SrtTransform.CreateMatrix(Translation, Scale, quaternion);
				}

				return _localTransform.Value;
			}
		}


		/// <summary>
		/// Gets the global (world-space) transformation matrix for this node.
		/// For root nodes, this is the same as LocalTransform.
		/// For child nodes, this is LocalTransform * Parent.GlobalTransform.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Matrix GlobalTransform
		{
			get
			{
				UpdateGlobalTransform();
				return _globalTransform.Value;
			}
		}

		/// <summary>
		/// Gets the parent node in the hierarchy, or null if this is a root node.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Parent { get; internal set; }

		/// <summary>
		/// Gets the collection of child nodes. Children are transformed relative to their parent.
		/// </summary>
		[Browsable(false)]
		public ObservableCollection<SceneNode> Children { get; } = new ObservableCollection<SceneNode>();

		/// <summary>
		/// Gets or sets an arbitrary tag object that can be used to store application-specific data.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public object Tag { get; set; }

		/// <summary>
		/// Initializes a new instance of the SceneNode class.
		/// Subscribes to collection changes to handle child node addition/removal.
		/// </summary>
		public SceneNode()
		{
			Children.CollectionChanged += ChildrenOnCollectionChanged;
		}

		/// <summary>
		/// Called when the Children collection changes (items added, removed, or cleared).
		/// Ensures proper parent-child relationships are maintained.
		/// </summary>
		private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				// When children are added, notify them they have a new parent
				foreach (SceneNode n in args.NewItems)
				{
					OnChildAdded(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				// When children are removed, clear their parent reference
				foreach (SceneNode n in args.OldItems)
				{
					OnChildRemoved(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				// When the collection is cleared, remove all children
				foreach (var w in Children)
				{
					OnChildRemoved(w);
				}
			}
		}

		/// <summary>
		/// Called when a child node is added to this node.
		/// Override to perform custom logic when children are added.
		/// </summary>
		protected virtual void OnChildAdded(SceneNode n)
		{
			n.Parent = this;
		}

		/// <summary>
		/// Called when a child node is removed from this node.
		/// Override to perform custom logic when children are removed.
		/// </summary>
		protected virtual void OnChildRemoved(SceneNode n)
		{
			n.Parent = null;
		}

		/// <summary>
		/// Renders this node and potentially its children. Override in derived classes to implement rendering logic.
		/// This is called from the RenderContext during the render pass.
		/// </summary>
		protected internal virtual void Render(RenderContext context)
		{
		}

		/// <summary>
		/// Invalidates the cached transformation matrices for this node and all its children.
		/// This forces the matrices to be recalculated on next access.
		/// Call this after modifying Translation, Rotation, or Scale.
		/// </summary>
		public void InvalidateTransform()
		{
			_localTransform = null;
			_globalTransform = null;

			// Recursively invalidate all children since their global transforms will change
			foreach (var child in Children)
			{
				child.InvalidateTransform();
			}
		}

		/// <summary>
		/// Recalculates the global transformation matrix if it's been invalidated.
		/// For root nodes: GlobalTransform = LocalTransform
		/// For child nodes: GlobalTransform = LocalTransform * Parent.GlobalTransform
		/// </summary>
		protected void UpdateGlobalTransform()
		{
			if (_globalTransform != null)
			{
				return;
			}

			if (Parent != null)
			{
				// Child node: multiply local by parent's global transform
				_globalTransform = LocalTransform * Parent.GlobalTransform;
			}
			else
			{
				// Root node: global transform is the same as local
				_globalTransform = LocalTransform;
			}

			// Allow derived classes to react to the transform update
			OnGlobalTransformUpdated();
		}

		/// <summary>
		/// Called when the global transform has been updated.
		/// Override in derived classes to react to transform changes (e.g., update camera view matrix).
		/// </summary>
		protected virtual void OnGlobalTransformUpdated()
		{
		}

		/// <summary>
		/// Recursively iterates through this node and all descendants, calling the provided action for each.
		/// </summary>
		private static void IterateInternal(SceneNode node, Action<SceneNode> action)
		{
			action(node);

			// Recursively visit all children
			foreach (var child in node.Children)
			{
				IterateInternal(child, action);
			}
		}

		/// <summary>
		/// Iterates through all nodes in the subtree rooted at this node (including this node).
		/// The provided action is called for each node in depth-first order.
		/// </summary>
		public void IterateRecursive(Action<SceneNode> action)
		{
			IterateInternal(this, action);
		}
	}
}
