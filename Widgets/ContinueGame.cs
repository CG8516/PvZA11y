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
            new ListItem(){relativePos = new Vector2(0.15f,0.75f), text = Text.menus.Continue},
            new ListItem(){relativePos = new Vector2(0.6f,0.75f), text = Text.menus.restartLevel},
            new ListItem(){relativePos = new Vector2(0.5f,0.85f), text = Text.menus.cancel},
        };

        public ContinueGame(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, _listItems)
        {
        }

        public override void DenyInteraction()
        {
            //Select cancel
            listIndex = listItems.Length - 1;
            ConfirmInteraction();
        }

        protected override string? GetContent()
        {
            return Text.menus.continueGame;
        }

        public override void Interact(InputIntent intent)
        {
            base.Interact(intent);
            if (intent is InputIntent.Info1)
                hasReadContent = false;
        }
    }
}
