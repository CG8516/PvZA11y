using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class SteamSaveChoice : Dialog
    {
        static ListItem[] _listItems = new ListItem[]
        {
            new ListItem(){relativePos = new Vector2(0.15f,0.75f), text = Text.menus.localSave},
            new ListItem(){relativePos = new Vector2(0.5f,0.75f), text = Text.menus.steamSave},
            new ListItem(){relativePos = new Vector2(0.85f,0.75f), text = Text.menus.cancel},
        };

        public SteamSaveChoice(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, _listItems)
        {
        }

        protected override string? GetContent()
        {
            return Text.menus.steamCloudMessage;
        }

        
    }
}
