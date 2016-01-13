using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DigitalRune.Animation;
using DigitalRune.Game;
using DigitalRune.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Game.UI;
using DigitalRune.ServiceLocation;
using Microsoft.Practices.ServiceLocation;
using DigitalRune.Storages;
using Microsoft.Xna.Framework.Content;
using DigitalRune.Threading;

namespace kbPCB
{
    public class kbPCB : Microsoft.Xna.Framework.Game
    {
        // The XNA GraphicsDeviceManager.
        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        private InputManager _inputManager;                   // Input
        private GraphicsManager _graphicsManager;             // Graphics
        private UIManager _uiManager;                         // GUI
        private AnimationManager _animationManager;           // Animation
        private GameObjectManager _gameObjectManager;         // Game logic
        private HierarchicalProfiler _profiler;               // Profiler for game loop

        // The IoC service container providing access to all services.
        private ServiceContainer _services;

        // The size of the current time step.
        private TimeSpan _deltaTime;

        public kbPCB()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
                PreferMultiSampling = false,
                SynchronizeWithVerticalRetrace = true,
            };

            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        static kbPCB()
        {
            // Add Non-commercial license
            // TODO(matt) - I'm using a file for the license key here so that it doesn't get uploaded to github.com 
            using (var licenseFile = File.OpenText(@"..\..\..\license.lic"))
            {
                DigitalRune.Licensing.AddSerialNumber(licenseFile.ReadToEnd());
            }
        }

        protected override void Initialize()
        {
            // create all application services and add to main services container.
            _services = new ServiceContainer();
            ServiceLocator.SetLocatorProvider(() => _services);

            var vfsStorage = new VfsStorage();
            var pathStorage = new FileSystemStorage("data");
            var uiAssetsStorage = new ZipStorage(pathStorage, "UI_Assets.zip");
            vfsStorage.MountInfos.Add(new VfsMountInfo(uiAssetsStorage, null));

            // Register the virtual file system as a service.
            _services.Register(typeof(IStorage), null, vfsStorage);

            // The GraphicsDeviceManager needs to be registered in the service container.
            // (This is required by the XNA content managers.)
            _services.Register(typeof(IGraphicsDeviceService), null, _graphicsDeviceManager);
            _services.Register(typeof(GraphicsDeviceManager), null, _graphicsDeviceManager);

            var uiContentManager = new StorageContentManager(_services, uiAssetsStorage);
            _services.Register(typeof(ContentManager), "UIContent", uiContentManager);

            // ----- Initialize Services
            // Register the game class.
            _services.Register(typeof(Microsoft.Xna.Framework.Game), null, this);
            _services.Register(typeof(kbPCB), null, this);

            // Input
            _inputManager = new InputManager(false);
            _services.Register(typeof(IInputService), null, _inputManager);

            // Graphics
            _graphicsManager = new GraphicsManager(GraphicsDevice, Window, uiContentManager);
            _services.Register(typeof(IGraphicsService), null, _graphicsManager);

            // GUI
            _uiManager = new UIManager(this, _inputManager);
            _services.Register(typeof(IUIService), null, _uiManager);

            // Animation
            _animationManager = new AnimationManager();
            _services.Register(typeof(IAnimationService), null, _animationManager);

            // Game logic
            _gameObjectManager = new GameObjectManager();
            _services.Register(typeof(IGameObjectService), null, _gameObjectManager);

            // Profiler
            _profiler = new HierarchicalProfiler("Main");
            _services.Register(typeof(HierarchicalProfiler), "Main", _profiler);

            // add more stuff here
            var editor2D = new Editor2D(this);
            Components.Add(editor2D);

            base.Initialize();
        }

        // Updates the different sub-systems (input, physics, game logic, ...).
        protected override void Update(GameTime gameTime)
        {
            _deltaTime = gameTime.ElapsedGameTime;

            // Tell the profiler that a new frame has begun.
            _profiler.NewFrame();
            _profiler.Start("Update");

            // Update input manager. The input manager gets the device states and performs other work.
            // (Note: XNA requires that the input service is run on the main thread!)
            _profiler.Start("InputManager.Update             ");
            _inputManager.Update(_deltaTime);
            _profiler.Stop();

            // Update animations.
            // (The animation results are stored internally but not yet applied).
            _profiler.Start("AnimationManger.Update          ");
            _animationManager.Update(_deltaTime);
            _profiler.Stop();

            // Apply animations.
            // (The animation results are written to the objects and properties that 
            // are being animated. ApplyAnimations() must be called at a point where 
            // it is thread-safe to change the animated objects and properties.)
            _profiler.Start("AnimationManager.ApplyAnimations");
            _animationManager.ApplyAnimations();
            _profiler.Stop();

            // Run any task completion callbacks that have been scheduled.
            _profiler.Start("Parallel.RunCallbacks           ");
            Parallel.RunCallbacks();
            _profiler.Stop();


            // Update XNA GameComponents.
            _profiler.Start("base.Update                     ");
            base.Update(gameTime);
            _profiler.Stop();

            // Update UI manager. The UI manager updates all registered UIScreens.
            _profiler.Start("UIManager.Update                ");
            _uiManager.Update(_deltaTime);
            _profiler.Stop();

            // Update DigitalRune GameObjects.
            _profiler.Start("GameObjectManager.Update        ");
            _gameObjectManager.Update(_deltaTime);
            _profiler.Stop();

            _profiler.Stop();
        }

        // Draws the game content.
        protected override void Draw(GameTime gameTime)
        {
            // Manually clear background. 
            // (This is not really necessary because the individual samples are 
            // responsible for rendering. However, if we skip this Clear() on Android 
            // then we can see trash in the back buffer when switching between samples.)
            GraphicsDevice.Clear(Color.Black);

            _profiler.Start("Draw");

            // Render all DrawableGameComponents registered in Components.
            _profiler.Start("base.Draw                       ");
            base.Draw(gameTime);
            _profiler.Stop();

            // Update the graphics (including graphics screens).
            // Important, if symbol EnableParallelGameLoop is true: Currently 
            // animation, physics and particles are running in parallel. Therefore, 
            // the GraphicsScreen.OnUpdate() methods must not influence the animation,
            // physics or particle state!
            _profiler.Start("GraphicsManager.Update          ");
            _graphicsManager.Update(gameTime.ElapsedGameTime);
            _profiler.Stop();

            // Render graphics screens to the back buffer.
            _profiler.Start("GraphicsManager.Render          ");
            _graphicsManager.Render(false);
            _profiler.Stop();

            _profiler.Stop();
        }
    }
}
