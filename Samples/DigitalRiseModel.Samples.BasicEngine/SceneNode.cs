using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DigitalRiseModel.Samples.BasicEngine
{
	public class SceneNode
	{
		private Vector3 _translation = Vector3.Zero;
		private Vector3 _rotation = Vector3.Zero;
		private Vector3 _scale = Vector3.One;
		private Matrix? _globalTransform = null, _localTransform = null;

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

		public Vector3 Rotation
		{
			get => _rotation;

			set
			{
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

		public Matrix LocalTransform
		{
			get
			{
				if (_localTransform == null)
				{
					var quaternion = Quaternion.CreateFromYawPitchRoll(
											MathHelper.ToRadians(_rotation.Y),
											MathHelper.ToRadians(_rotation.X),
											MathHelper.ToRadians(_rotation.Z));
					_localTransform = Utility.CreateTransform(Translation, Scale, quaternion);
				}

				return _localTransform.Value;
			}
		}


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

		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Parent { get; internal set; }

		[Browsable(false)]
		public ObservableCollection<SceneNode> Children { get; } = new ObservableCollection<SceneNode>();

		[Browsable(false)]
		[JsonIgnore]
		public object Tag { get; set; }

		public SceneNode()
		{
			Children.CollectionChanged += ChildrenOnCollectionChanged;
		}

		private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (SceneNode n in args.NewItems)
				{
					OnChildAdded(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (SceneNode n in args.OldItems)
				{
					OnChildRemoved(n);
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (var w in Children)
				{
					OnChildRemoved(w);
				}
			}
		}

		protected virtual void OnChildAdded(SceneNode n)
		{
			n.Parent = this;
		}

		protected virtual void OnChildRemoved(SceneNode n)
		{
			n.Parent = null;
		}

		protected internal virtual void Render(RenderContext context)
		{
		}

		public void InvalidateTransform()
		{
			_localTransform = null;
			_globalTransform = null;

			foreach (var child in Children)
			{
				child.InvalidateTransform();
			}
		}

		protected void UpdateGlobalTransform()
		{
			if (_globalTransform != null)
			{
				return;
			}

			if (Parent != null)
			{
				_globalTransform = LocalTransform * Parent.GlobalTransform;
			}
			else
			{
				_globalTransform = LocalTransform;
			}

			OnGlobalTransformUpdated();
		}

		protected virtual void OnGlobalTransformUpdated()
		{
		}

		private static void IterateInternal(SceneNode node, Action<SceneNode> action)
		{
			action(node);

			foreach (var child in node.Children)
			{
				IterateInternal(child, action);
			}
		}

		public void IterateRecursive(Action<SceneNode> action)
		{
			IterateInternal(this, action);
		}
	}
}
