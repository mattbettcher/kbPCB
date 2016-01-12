using DigitalRune.Animation;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Input;
using System;
//using MathHelper = DigitalRune.Mathematics.MathHelper;

namespace kbPCB
{
    public class Editor2DCameraObject : GameObject
    {
        private readonly IServiceLocator _services;
        private readonly IInputService _inputService;
        private readonly IAnimationService _animationService;

        private float _zoomPercent = 1f;  // 0-1+

        // Position of camera.
        private Vector3F _defaultPosition = new Vector3F(0, 0, 1);


        // This property is null while the CameraObject is not added to the game
        // object service.
        public CameraNode CameraNode { get; private set; }

        public bool IsEnabled { get; set; }
        

        public Editor2DCameraObject(IServiceLocator services)
        {
            Name = "Editor2DCamera";

            _services = services;
            _inputService = services.GetInstance<IInputService>();
            _animationService = services.GetInstance<IAnimationService>();

            IsEnabled = true;
        }


        // OnLoad() is called when the GameObject is added to the IGameObjectService.
        protected override void OnLoad()
        {
            // Create a camera node.
            CameraNode = new CameraNode(new Camera(new OrthographicProjection()))
            {
                Name = "Editor2DCamera"
            };

            // Add to scene.
            // (This is usually optional. Since cameras do not have a visual representation,
            // it  makes no difference if the camera is actually part of the scene graph or
            // not. - Except when other scene nodes are attached to the camera. In this case
            // the camera needs to be in the scene.)

            // TODO should camera be added to the scene?
            var scene = _services.GetInstance<IScene>();
            if (scene != null)
                scene.Children.Add(CameraNode);

            ResetPose();
            ResetProjection();
        }


        // OnUnload() is called when the GameObject is removed from the IGameObjectService.
        protected override void OnUnload()
        {
            if (CameraNode.Parent != null)
                CameraNode.Parent.Children.Remove(CameraNode);

            CameraNode.Dispose(false);
            CameraNode = null;
        }

        
        public void ResetPose()
        {
            if (IsLoaded)
            {
                // Also update SceneNode.LastPose - this is required for some effect, like
                // object motion blur. 
                CameraNode.SetLastPose(true);

                CameraNode.PoseWorld = new Pose(
                  _defaultPosition,
                  QuaternionF.Identity);
            }
        }


        public void ResetProjection()
        {
            if (IsLoaded)
            {
                var graphicsService = _services.GetInstance<IGraphicsService>();
                var projection = (OrthographicProjection)CameraNode.Camera.Projection;
                // TODO(matt) need to determine how to change zoom of the ortho camera. Seems like changing the size of the projection is the easiest way.
                projection.Set(40 * _zoomPercent * graphicsService.GraphicsDevice.Viewport.AspectRatio, 40 * _zoomPercent, 0, 10);
            }
        }

        Vector3F mouseWorldPosOld;
        Matrix44F originalCameraMat;
        Vector3F originalCameraPos;

        // OnUpdate() is called once per frame.
        protected override void OnUpdate(TimeSpan deltaTime)
        {
            if (!IsEnabled)
                return;

            // Reset camera position if <Home> is pressed.
            if (_inputService.IsPressed(Keys.Home, false))
            {
                ResetPose();
            }

            // Zoom control
            //TODO(matt) this needs lots of work!
            _zoomPercent += 0.001f * _inputService.MouseWheelDelta;
            _zoomPercent = MathHelper.Clamp<float>(_zoomPercent, 0.1f, 25);

            ResetProjection();

            if (_inputService.IsDown(MouseButtons.Middle))
            {
                var graphicsService = _services.GetInstance<IGraphicsService>();
                var mousePos = -_inputService.MousePosition;

                if (_inputService.IsPressed(MouseButtons.Middle, false))
                {
                    originalCameraMat = CameraNode.PoseWorld.Inverse.ToMatrix44F();
                    originalCameraPos = CameraNode.PoseWorld.Position;
                    mouseWorldPosOld = GraphicsHelper.Unproject(graphicsService.GraphicsDevice.Viewport, new Vector3F(mousePos.X, mousePos.Y, 0),
                        CameraNode.Camera.Projection.ToMatrix44F(), originalCameraMat);
                }
                else
                {
                    var worldPos = GraphicsHelper.Unproject(graphicsService.GraphicsDevice.Viewport, new Vector3F(mousePos.X, mousePos.Y, 0),
                        CameraNode.Camera.Projection.ToMatrix44F(), originalCameraMat);
                    
                    CameraNode.PoseWorld = new Pose(
                      originalCameraPos + new Vector3F(worldPos.X, worldPos.Y, 1) - new Vector3F(mouseWorldPosOld.X, mouseWorldPosOld.Y, 0),
                      QuaternionF.Identity);
                }
            }
        }
    }
}
