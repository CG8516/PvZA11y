using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class ContinueGame : Dialog
    {
        static ListItem[] _listItems = new ListItem[]
        {
            new ListItem(){relativePos = new Vector2(0.15f,0.75f), text = "Continue"},
            new ListItem(){relativePos = new Vector2(0.6f,0.75f), text = "Restart Level"},
            new ListItem(){relativePos = new Vector2(0.5f,0.85f), text = "Cancel"},
        };

        public ContinueGame(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, _listItems)
        {
        }

        protected override string? GetContent()
        {
            return "Continue Game?\r\n" + "Do you want to continue your current game, or restart the level?";
        }
    }
}
