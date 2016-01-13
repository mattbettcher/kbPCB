using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kbPCB
{
    public class GridSettingsWindow : Window
    {
        public GridSettingsWindow()
        {
            Width = 250;
            Height = 200;
        }

        protected override void OnLoad()
        {
            // size controls
            {
                var widthL = new TextBlock { Text = "Width:", VerticalAlignment = VerticalAlignment.Center };
                var widthTB = new TextBox { Text = "", VerticalAlignment = VerticalAlignment.Center };
                var widthSP = new StackPanel { Orientation = Orientation.Horizontal,
                    Margin = new Vector4F(4),
                    Padding = new Vector4F(4),
                };
                widthSP.Children.Add(widthL);
                widthSP.Children.Add(widthTB);

                var sizeStack = new StackPanel() { Orientation = Orientation.Vertical,
                    Margin = new Vector4F(4),
                    Padding = new Vector4F(4),
                };
                sizeStack.Children.Add(widthSP);
                
                var sizeGroup = new GroupBox()
                {
                    Title = "Size",
                    Content = sizeStack,
                    Margin = new Vector4F(4),
                    Padding = new Vector4F(4),
                };

                this.Content = sizeGroup;
            }

            base.OnLoad();
        }
    }
}
