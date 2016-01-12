using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DigitalRune.Graphics.Rendering;

namespace kbPCB
{
    public class Editor2D : EditorBase
    {
        Editor2DCameraObject _cameraObject;

        public Editor2D(Microsoft.Xna.Framework.Game game)
            : base(game)
        {
            // Create a simple delegate graphics screen to handle figure rendering
            var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
            {
                RenderCallback = Render,
            };
            // TODO - make sure we don't just want to put this under the controls screen?
            // make sure to insert it on top?         
            GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

            _cameraObject = new Editor2DCameraObject(Services);
            GameObjectService.Objects.Add(_cameraObject);

            CreateGrid();
            CreateGate();

            // Add a game object which handles the picking:
            GameObjectService.Objects.Add(new FigurePickerObject(GraphicsService, Scene, _cameraObject, DebugRenderer));
        }

        private void CreateGate()
        {
            // not gate
            var trianglePath = new Path2F
            {
                new PathKey2F
                {
                    Parameter = 0,
                    Interpolation = SplineInterpolation.Linear,
                    Point = new Vector2F(1, 0),
                },
                new PathKey2F
                {
                    Parameter = 1,
                    Interpolation = SplineInterpolation.Linear,
                    Point = new Vector2F(-0.25f, 0.5f),
                },
                new PathKey2F
                {
                    Parameter = 2,
                    Interpolation = SplineInterpolation.Linear,
                    Point = new Vector2F(-0.25f, -0.5f),
                },
                new PathKey2F
                {
                    Parameter = 3,
                    Interpolation = SplineInterpolation.Linear,
                    Point = new Vector2F(1, 0),
                }
            };

            var triangle = new PathFigure2F();
            triangle.Segments.Add(trianglePath);
            var outPinFigure = new PathFigure2F
            {
                Segments = {
                    new LineSegment2F() { Point1 = new Vector2F(1, 0), Point2 = new Vector2F(1.2f, 0) }
                }
            };

            // Add figure to the scene.
            var notGateNode = new FigureNode(triangle)
            {
                Name = "Not Gate",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(1, 0.2f, 0.2f),
                FillColor = new Vector3F(1, 0.5f, 0.5f),
                FillAlpha = 0.5f,
                PoseLocal = new Pose(new Vector3F(0, 0, 0)),
                ScaleLocal = new Vector3F(1),
                Children = new SceneNodeCollection(3),
            };

            var pinFigure = new PathFigure2F
            {
                Segments =
                {
                    new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(0.2f, 0) }
                }
            };
            var pinsFigure = new CompositeFigure();

            var output = new TransformedFigure(pinFigure)
            {
                Pose = new Pose(new Vector3F(1.0f, 0, 0))
            };
            pinsFigure.Children.Add(output);

            var ina = new TransformedFigure(pinFigure)
            {
                Pose = new Pose(new Vector3F(-0.25f - 0.2f, -0.25f, 0))
            };
            pinsFigure.Children.Add(ina);

            var inb = new TransformedFigure(pinFigure)
            {
                Pose = new Pose(new Vector3F(-0.25f - 0.2f, 0.25f, 0))
            };
            pinsFigure.Children.Add(inb);

            notGateNode.Children.Add(new FigureNode(pinsFigure)
            {
                Name = "Pins",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0, 0, 0),
                PoseLocal = new Pose(new Vector3F(0, 0, 0)),
                ScaleLocal = new Vector3F(1f),
            });


            Scene.Children.Add(notGateNode);
        }

        // TODO(matt) - grid creation needs to be able to be customized and should also be to a specific scale
        // TODO(matt) - should also have an intuitive control panel with presets and real time feedback
        // Add a grid with thick major grid lines and thin stroked minor grid lines.
        private void CreateGrid()
        {
            var majorGridLines = new PathFigure2F();
            for (int i = 0; i <= 100; i++)
            {
                majorGridLines.Segments.Add(new LineSegment2F
                {
                    Point1 = new Vector2F(-50, -50 + i),
                    Point2 = new Vector2F(50, -50 + i),
                });
                majorGridLines.Segments.Add(new LineSegment2F
                {
                    Point1 = new Vector2F(-50 + i, -50),
                    Point2 = new Vector2F(-50 + i, 50),
                });
            }

            var minorGridLines = new PathFigure2F();
            for (int i = 0; i < 100; i++)
            {
                minorGridLines.Segments.Add(new LineSegment2F
                {
                    Point1 = new Vector2F(-50, -40.5f + i),
                    Point2 = new Vector2F(50, -40.5f + i),
                });
                minorGridLines.Segments.Add(new LineSegment2F
                {
                    Point1 = new Vector2F(-40.5f + i, -50),
                    Point2 = new Vector2F(-40.5f + i, 50),
                });
            }

            var majorLinesNode = new FigureNode(majorGridLines)
            {
                Name = "Major grid lines",
                PoseLocal = Pose.Identity,
                StrokeThickness = 1,
                StrokeColor = new Vector3F(0.1f),
                StrokeAlpha = 1f,
            };
            var minorLinesNode = new FigureNode(minorGridLines)
            {
                Name = "Minor grid lines",
                PoseLocal = Pose.Identity,
                StrokeThickness = 0.5f,
                StrokeColor = new Vector3F(0.1f),
                StrokeAlpha = 1f,
                //DashInWorldSpace = true,
                //StrokeDashPattern = new Vector4F(10, 4, 0, 0) / 200,
            };
            var gridNode = new SceneNode
            {
                Name = "Grid",
                Children = new SceneNodeCollection(),
                PoseLocal = new Pose(new Vector3F(0, -0.5f, 0)),
            };
            gridNode.Children.Add(majorLinesNode);
            gridNode.Children.Add(minorLinesNode);
            Scene.Children.Add(gridNode);
        } 

        public override void Update(GameTime gameTime)
        {
            Scene.Update(gameTime.ElapsedGameTime);

            DebugRenderer.Clear();

            base.Update(gameTime);
        }

        private void Render(RenderContext context)
        {
            context.CameraNode = _cameraObject.CameraNode;

            var graphicsDevice = context.GraphicsService.GraphicsDevice;
            graphicsDevice.Clear(Color.White);

            // Find all objects within camera frustum.
            var query = Scene.Query<CameraFrustumQuery>(_cameraObject.CameraNode, context);

            // Draw figure nodes.
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            FigureRenderer.Render(query.SceneNodes, context, RenderOrder.BackToFront);

            // Draw debug information.
            DebugRenderer.Render(context);

            context.CameraNode = null;
        }
    }
}
