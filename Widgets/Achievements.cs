using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class Achievements : Dialog
    {
        public bool menuClosed = false;
        static ListItem[] GetListItems(MemoryIO memIO)
        {
            ListItem[] items = new ListItem[21];
            for(int i =0; i < 21; i++)
            {
                int achieved = memIO.mem.ReadByte(memIO.ptr.playerInfoChain + "," + (0x24 + i).ToString("X2"));
                items[i].extraData = achieved;
                items[i].text = Consts.achievementNames[i] + " : " + Consts.achievementDescriptions[i];
            }
            return items;
        }

        public override string? SayTitle(bool shouldSay)
        {
            string prepend = "Achievements.\r\n";
            if (Config.current.SayAvailableInputs)
                prepend += "Inputs: Up and Down to scroll, Deny to close.\r\n";
            SayItem(prepend);
            return null;
        }

        public override void ConfirmInteraction()
        {
            return;
        }

        public override void DenyInteraction()
        {
            menuClosed = true;
        }

        public Achievements(MemoryIO memIO) : base(memIO, "", GetListItems(memIO))
        {
        }

        void SayItem(string? prepend = null)
        {
            ConfineInteractionIndex();

            float freq = 1250.0f - ((((float)listIndex / (float)listItems.Length) * 5000.0f) / 5.0f);

            if (listItems[listIndex].extraData == 1)
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Square);
            else
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Sin);

            string itemText = listItems[listIndex].text;
            if (listItems[listIndex].extraData == 1)
                itemText = "Completed: " + itemText;

            if (prepend != null)
                itemText = prepend + itemText;
            Console.WriteLine(itemText);
            Program.Say(itemText);
        }

        public override void Interact(InputIntent intent)
        {
            int lastIndex = listIndex;
            switch (intent)
            {
                case (InputIntent.Up):
                    listIndex--;
                    break;
                case (InputIntent.Down):
                    listIndex++;
                    break;
                case (InputIntent.Confirm):
                    ConfirmInteraction();
                    break;
                case (InputIntent.Deny):
                    DenyInteraction();
                    break;
                case (InputIntent.Info1):
                    SayItem();
                    return;
            }

            if (listIndex != lastIndex)
            {
                if (listIndex < 0 || listIndex >= listItems.Length)
                    Program.Vibrate(0.1f, 0.1f, 50);

                SayItem();
            }
            else if (intent is (InputIntent.Up or InputIntent.Down))
                Program.PlayBoundaryTone();
        }
    }
}
