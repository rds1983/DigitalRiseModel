using AssetManagementBase;
using DigitalRiseModel.Animation;
using DigitalRiseModel.Samples.ModelViewer.UI;
using DigitalRiseModel.Samples.ModelViewer.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Events;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.ColorPicker;
using Myra.Graphics2D.UI.File;
using System;
using System.IO;

namespace DigitalRiseModel.Samples.ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private readonly DrModelInstance _model = new DrModelInstance();
		private AnimationController _player = null;
		private CameraInputController _controller;
		private MainPanel _mainPanel;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private Desktop _desktop;
		private bool _isAnimating;
		private SkinnedEffect _skinnedEffect;
		private string _path;
		private Texture2D _white;

		public ViewerGame(string path)
		{
			_path = path;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		private void ResetAnimation()
		{
			_mainPanel._sliderTime.Value = _mainPanel._sliderTime.Minimum;
		}

		private void LoadModel(string file)
		{
			try
			{
				if (!string.IsNullOrEmpty(file))
				{
					DrModel model;
					if (file.EndsWith(".jdrm"))
					{
						var folder = Path.GetDirectoryName(file);
						model = DrModel.CreateFromJson(GraphicsDevice,
							File.ReadAllText(file),
							s => File.OpenRead(Path.Combine(folder, s)));
					}
					else
					{
						using (var stream = File.OpenRead(file))
						{
							model = DrModel.CreateFromBinary(GraphicsDevice, stream);
						}
					}

					_model.Model = model;

					_mainPanel._comboAnimations.Widgets.Clear();
					_mainPanel._comboAnimations.Widgets.Add(new Label());
					foreach (var pair in model.Animations)
					{
						_mainPanel._comboAnimations.Widgets.Add(
							new Label
							{
								Text = pair.Key,
								Tag = pair.Value
							});
					}
				}

				// Reset camera
				var camera = _controller.Camera;
				if (_model.Model != null)
				{
					var bb = _model.BoundingBox.Value;
					var min = bb.Min;
					var max = bb.Max;
					var center = (min + max) / 2;
					var cameraPosition = (max - center) * 3.0f + center;

					camera.SetLookAt(cameraPosition, center);

					var size = Math.Max(max.X - min.X, max.Y - min.Y);
					size = Math.Max(size, max.Z - min.Z);

					camera.NearPlaneDistance = size / 1000.0f;
					camera.FarPlaneDistance = size * 10.0f;
				}
				else
				{
					camera.SetLookAt(Vector3.One, Vector3.Zero);
				}

				ResetAnimation();
			}
			catch (Exception ex)
			{
				var messageBox = Dialog.CreateMessageBox("Error", ex.Message);
				messageBox.ShowModal(_desktop);
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();
			_mainPanel._comboAnimations.Widgets.Clear();
			_mainPanel._comboAnimations.SelectedIndexChanged += _comboAnimations_SelectedIndexChanged;

			_mainPanel._comboPlaybackMode.SelectedIndex = 0;
			_mainPanel._comboPlaybackMode.SelectedIndexChanged += (s, a) =>
			{
				_player.PlaybackMode = (PlaybackMode)_mainPanel._comboPlaybackMode.SelectedIndex.Value;
			};

			_mainPanel._sliderSpeed.ValueChanged += (s, a) =>
			{
				_mainPanel._labelSpeed.Text = _mainPanel._sliderSpeed.Value.ToString("0.00");
				_player.Speed = _mainPanel._sliderSpeed.Value;
			};


			_mainPanel._buttonChangeTexture.Click += _buttonChangeTexture;
			_mainPanel._buttonChangeColor.Click += _buttonChangeColor_Click;

			_mainPanel._sliderTime.ValueChangedByUser += _sliderTime_ValueChanged;
			_mainPanel._sliderTime.ValueChanged += (s, a) =>
			{
				_mainPanel._labelTime.Text = _mainPanel._sliderTime.Value.ToString("0.00");
			};

			_mainPanel._buttonPlayStop.Click += _buttonPlayStop_Click;

			_desktop = new Desktop
			{
				Root = _mainPanel
			};

			_player = new AnimationController(_model);
			_player.TimeChanged += (s, a) =>
			{
				if (_player.AnimationClip == null)
				{
					return;
				}

				var k = (float)(_player.Time / _player.AnimationClip.Duration);

				var slider = _mainPanel._sliderTime;
				slider.Value = slider.Minimum + k * (slider.Maximum - slider.Minimum);
			};

			var camera = new Camera();
			_controller = new CameraInputController(camera);

			_skinnedEffect = new SkinnedEffect(GraphicsDevice)
			{
				DiffuseColor = Vector3.One
			};

			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData([Color.White]);
			_skinnedEffect.Texture = _white;
			_skinnedEffect.PreferPerPixelLighting = true;

			_skinnedEffect.DirectionalLight0.DiffuseColor = Vector3.One;
			_skinnedEffect.DirectionalLight0.Direction = new Vector3(0, -1, -1);
			_skinnedEffect.DirectionalLight0.Enabled = true;

			_skinnedEffect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.2f, 1.0f);
			_skinnedEffect.DirectionalLight1.Direction = new Vector3(0, 0, 1);
			_skinnedEffect.DirectionalLight1.Enabled = true;

			_mainPanel._imageColor.Renderable = new TextureRegion(_white);
			_mainPanel._imageColor.Color = Color.White;

			LoadModel(_path);
		}

		private void _buttonPlayStop_Click(object sender, EventArgs e)
		{
			_isAnimating = !_isAnimating;

			var label = (Label)_mainPanel._buttonPlayStop.Content;
			label.Text = _isAnimating ? "Stop" : "Play";
		}

		private void _sliderTime_ValueChanged(object sender, ValueChangedEventArgs<float> e)
		{
			if (!_player.IsPlaying)
			{
				return;
			}

			var k = (e.NewValue - _mainPanel._sliderTime.Minimum) / (_mainPanel._sliderTime.Maximum - _mainPanel._sliderTime.Minimum);
			var passed = _player.AnimationClip.Duration * k;
			_player.Time = passed;
		}

		private void _buttonChangeTexture(object sender, EventArgs e)
		{
			FileDialog dialog = new FileDialog(FileDialogMode.OpenFile)
			{
				Filter = "*.png|*.jpg|*.bmp|*.dds"
			};

			if (!string.IsNullOrEmpty(_mainPanel._textPath.Text))
			{
				dialog.Folder = Path.GetDirectoryName(_mainPanel._textPath.Text);
			}

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				var folder = Path.GetDirectoryName(dialog.FilePath);
				var assetManager = AssetManager.CreateFileAssetManager(folder);
				var texture = assetManager.LoadTexture2D(GraphicsDevice, Path.GetFileName(dialog.FilePath));

				_skinnedEffect.Texture = texture;
				_mainPanel._textPath.Text = dialog.FilePath;
			};

			dialog.ShowModal(_desktop);
		}

		private void _buttonChangeColor_Click(object sender, EventArgs e)
		{
			var dialog = new ColorPickerDialog
			{
				Color = _mainPanel._imageColor.Color
			};

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				_mainPanel._imageColor.Color = dialog.Color;
				_skinnedEffect.DiffuseColor = dialog.Color.ToVector3();
			};

			dialog.ShowModal(_desktop);
		}

		private void _comboAnimations_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_mainPanel._comboAnimations.SelectedItem == null || string.IsNullOrEmpty(((Label)_mainPanel._comboAnimations.SelectedItem).Text))
			{
				_player.StopClip();
			}
			else
			{
				var clipName = ((Label)_mainPanel._comboAnimations.SelectedItem).Text;
				if (_mainPanel._checkCrossfade.IsChecked)
				{
					_player.CrossFade(clipName, TimeSpan.FromSeconds(0.5f));
				}
				else
				{
					_player.StartClip(clipName);
				}
			}

			ResetAnimation();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_fpsCounter.Update(gameTime);

			var keyboardState = Keyboard.GetState();

			// Manage camera input controller
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Left, keyboardState.IsKeyDown(Keys.A));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Right, keyboardState.IsKeyDown(Keys.D));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Forward, keyboardState.IsKeyDown(Keys.W));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Backward, keyboardState.IsKeyDown(Keys.S));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Up, keyboardState.IsKeyDown(Keys.Up));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Down, keyboardState.IsKeyDown(Keys.Down));

			if (!_desktop.IsTouchOverGUI)
			{
				var mouseState = Mouse.GetState();
				_controller.SetTouchState(CameraInputController.TouchType.Rotate, mouseState.RightButton == ButtonState.Pressed);
				_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));
			}

			_controller.Update();
		}

		private void DrawModel(GameTime gameTime)
		{
			if (_model.Model == null)
			{
				return;
			}

			if (_isAnimating)
			{
				_player.Update(gameTime.ElapsedGameTime);
			}

			var oldRasterizer = GraphicsDevice.RasterizerState;
			var oldDepthStencilState = GraphicsDevice.DepthStencilState;
			var oldBlendState = GraphicsDevice.BlendState;

			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;

			var camera = _controller.Camera;
			_skinnedEffect.View = camera.View;
			_skinnedEffect.Projection = camera.CalculateProjection(GraphicsDevice.Viewport.AspectRatio);

			foreach (var bone in _model.Model.Bones)
			{
				if (bone.Mesh == null)
				{
					continue;
				}

				foreach (var pass in _skinnedEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					foreach (var submesh in bone.Mesh.Submeshes)
					{
						if (submesh.Skin != null)
						{
							var skinTransforms = _model.GetSkinTransforms(submesh.Skin.SkinIndex);
							_skinnedEffect.SetBoneTransforms(skinTransforms);
						}

						submesh.Draw();
					}
				}
			}

			GraphicsDevice.RasterizerState = oldRasterizer;
			GraphicsDevice.DepthStencilState = oldDepthStencilState;
			GraphicsDevice.BlendState = oldBlendState;
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawModel(gameTime);

			_desktop.Render();

			_fpsCounter.Draw(gameTime);
		}
	}
}