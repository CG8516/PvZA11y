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
            new ListItem(){relativePos = new Vector2(0.15f,0.75f), text = "Local Save"},
            new ListItem(){relativePos = new Vector2(0.5f,0.75f), text = "Steam Save"},
            new ListItem(){relativePos = new Vector2(0.85f,0.75f), text = "Cancel"},
        };

        public SteamSaveChoice(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, _listItems)
        {
        }

        protected override string? GetContent()
        {
            return "Steam Cloud Saving Active.\r\n" + "We have detected that you have a steam cloud save for this game, as well as a save stored on this machine.\r\nWhich save would you like to use?\r\nChoosing cancel will disable Steam Cloud for this session.";
        }

        
    }
}
