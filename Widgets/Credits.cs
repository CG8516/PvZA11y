using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    internal class Credits : Dialog
    {
        static ListItem[] listItems = new ListItem[]
        {
            new ListItem(){relativePos = new Vector2(0.1f,0.95f), text = "Replay"},
            new ListItem(){relativePos = new Vector2(0.5f,0.95f), text = "Main Menu"}
        };

        public Credits(MemoryIO memIO) : base(memIO, "", listItems)
        {
            
        }

        protected override string? GetContent()
        {
            return null;
        }

        public override void Interact(InputIntent intent)
        {
            //Don't allow interaction while credits are still playing
            int creditsState = memIO.GetCreditsState();
            if (creditsState != 3)
                return;

            base.Interact(intent);
        }
    }
}
