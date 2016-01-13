using System;
using System.Diagnostics;
using DigitalRune.Animation;
using DigitalRune.Diagnostics;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.ServiceLocation;
using DigitalRune.Storages;
using DigitalRune.Threading;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;

namespace kbPCB
{
    /// <summary>
    /// Provides a bunch of services for Editor's to use.
    /// </summary>
    public class EditorBase : GameComponent
    {
        // Services which can be used in derived classes.
        protected readonly ServiceContainer Services;
        protected readonly ContentManager UIContentManager;
        protected readonly IInputService InputService;
        protected readonly IAnimationService AnimationService;
        protected readonly IGraphicsService GraphicsService;
        protected readonly IGameObjectService GameObjectService;
        protected readonly IUIService UIService;

        // renderers
        protected readonly FigureRenderer FigureRenderer;
        protected readonly DebugRenderer DebugRenderer;
        protected readonly UIRenderer UIRenderer;

        // scene
        protected Scene Scene;

        // ui
        protected UIScreen UIScreen;

        public EditorBase(Microsoft.Xna.Framework.Game game)
            : base(game)
        {
            // Get services from the global service container.
            var services = (ServiceContainer)ServiceLocator.Current;
            UIContentManager = services.GetInstance<ContentManager>("UIContent");
            InputService = services.GetInstance<IInputService>();
            AnimationService = services.GetInstance<IAnimationService>();
            GraphicsService = services.GetInstance<IGraphicsService>();
            GameObjectService = services.GetInstance<IGameObjectService>();
            UIService = services.GetInstance<IUIService>();

            // Create a local service container which can be modified in samples:
            // The local service container is a child container, i.e. it inherits the
            // services of the global service container. Samples can add new services
            // or override existing entries without affecting the global services container
            // or other samples.
            Services = services.CreateChildContainer();

            // Load a UI theme, which defines the appearance and default values of UI controls.
            Theme theme = UIContentManager.Load<Theme>("BlendBlue/Theme");

            FigureRenderer = new FigureRenderer(GraphicsService, 2000);
            DebugRenderer = new DebugRenderer(GraphicsService, UIContentManager.Load<SpriteFont>("BlendBlue/Default"));
            UIRenderer = new UIRenderer(GraphicsService.GraphicsDevice, theme);

            UIScreen = new UIScreen("Main Screen", UIRenderer)
            {
                Background = Color.TransparentBlack,
            };

            UIService.Screens.Add(UIScreen);

            Scene = new Scene();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose of anything we create here!
            }

            base.Dispose(disposing);
        }
    }
}
