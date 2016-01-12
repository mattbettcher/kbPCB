using DigitalRune.Graphics.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kbPCB
{
    public enum EditNodeStates
    {
        // TODO(matt) - decide which of these are or are not needed and if any others are needed.
        Default,
        MouseOver,
        Dragging,
        Focused,
        Disabled,
        Pressed,
        Active,
    }

    // goal is to create a selectable, multistate node
    public class EditableNode : SceneNode
    {

    }
}
