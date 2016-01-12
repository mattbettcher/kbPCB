using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Input;
using System;
using MathHelper = DigitalRune.Mathematics.MathHelper;

namespace kbPCB
{
    public class CameraObject : GameObject
    {
        // Some constants for motion control.
        private const float LinearVelocityMagnitude = 5f;
        private const float SpeedBoost = 20;

        private readonly IServiceLocator _services;
        private readonly IInputService _inputService;

        private float _farDistance;

        // Position and Orientation of camera.
        private Vector3F _defaultPosition = new Vector3F(0, 0, 15);


        // This property is null while the CameraObject is not added to the game
        // object service.
        public CameraNode CameraNode { get; private set; }

        public bool IsEnabled { get; set; }


        public CameraObject(IServiceLocator services)
          : this(services, 1000)
        {
        }


        public CameraObject(IServiceLocator services, float farDistance)
        {
            Name = "Camera";

            _services = services;
            _inputService = services.GetInstance<IInputService>();

            IsEnabled = true;
            _farDistance = farDistance;
        }


        // OnLoad() is called when the GameObject is added to the IGameObjectService.
        protected override void OnLoad()
        {
            // Create a camera node.
            CameraNode = new CameraNode(new Camera(new OrthographicProjection()))
            {
                Name = "PlayerCamera"
            };

            // Add to scene.
            // (This is usually optional. Since cameras do not have a visual representation,
            // it  makes no difference if the camera is actually part of the scene graph or
            // not. - Except when other scene nodes are attached to the camera. In this case
            // the camera needs to be in the scene.)
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


        public void ResetPose(Vector3F position, float yaw, float pitch)
        {
            _defaultPosition = position;

            ResetPose();
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
                projection.Set(10, 10, 0, 1000);
            }
        }


        // OnUpdate() is called once per frame.
        protected override void OnUpdate(TimeSpan deltaTime)
        {
            // Mouse centering (controlled by the MenuComponent) is disabled if the game
            // is inactive or if the GUI is active. In these cases, we do not want to move
            // the player.
            //if (!_inputService.EnableMouseCentering)
            //    return;

            if (!IsEnabled)
                return;

            float deltaTimeF = (float)deltaTime.TotalSeconds;

            // Compute new orientation from mouse movement, gamepad and touch.
            Vector2F mousePositionDelta = _inputService.MousePositionDelta;
            GamePadState gamePadState = _inputService.GetGamePadState(LogicalPlayerIndex.One);
            Vector2F touchDelta = Vector2F.Zero;
          
            // Reset camera position if <Home> or <Right Stick> is pressed.
            if (_inputService.IsPressed(Keys.Home, false)
                || _inputService.IsPressed(Buttons.RightStick, false, LogicalPlayerIndex.One))
            {
                ResetPose();
            }

            // Create velocity from <W>, <A>, <S>, <D> and <R>, <F> keys. 
            // <R> or DPad up is used to move up ("rise"). 
            // <F> or DPad down is used to move down ("fall").
            Vector3F velocity = Vector3F.Zero;
            KeyboardState keyboardState = _inputService.KeyboardState;
            //if (keyboardState.IsKeyDown(Keys.W))
            //    velocity.Z--;
            //if (keyboardState.IsKeyDown(Keys.S))
            //    velocity.Z++;
            if (keyboardState.IsKeyDown(Keys.A))
                velocity.X--;
            if (keyboardState.IsKeyDown(Keys.D))
                velocity.X++;
            if (keyboardState.IsKeyDown(Keys.R) || gamePadState.DPad.Up == ButtonState.Pressed)
                velocity.Y++;
            if (keyboardState.IsKeyDown(Keys.F) || gamePadState.DPad.Down == ButtonState.Pressed)
                velocity.Y--;

            // Add velocity from gamepad sticks.
            velocity.X += gamePadState.ThumbSticks.Left.X;
            velocity.Z -= gamePadState.ThumbSticks.Left.Y;
            
            if (keyboardState.IsKeyDown(Keys.LeftShift))
                velocity *= SpeedBoost;

            // Multiply the velocity by time to get the translation for this frame.
            Vector3F translation = velocity * LinearVelocityMagnitude * deltaTimeF;

            // Update SceneNode.LastPoseWorld - this is required for some effects, like
            // camera motion blur. 
            CameraNode.LastPoseWorld = CameraNode.PoseWorld;

            // Set the new camera pose.
            CameraNode.PoseWorld = new Pose(
              CameraNode.PoseWorld.Position + translation,
              QuaternionF.Identity);
        }
    }
}
