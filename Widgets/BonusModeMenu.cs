using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    internal class BonusModeMenu : Dialog
    {

        private static string[] MinigameNames = new string[]
        {
            "Survival: Day",
            "Survival: Night",
            "Survival: Pool",
            "Survival: Fog",
            "Survival: Roof",
            "Survival: Day (Hard)",
            "Survival: Night (Hard)",
            "Survival: Pool (Hard)",
            "Survival: Fog (Hard)",
            "Survival: Roof (Hard)",
            "Survival: Day (Endless)",
            "Survival: Night (Endless)",
            "Survival: Pool (Endless)",
            "Survival: Fog (Endless)",
            "Survival: Roof (Endless)",
            "ZomBotany",
            "Wall-nut Bowling",
            "Slot Machine",
            "It's Raining Seeds",
            "Be-ghouled",
            "Invisi-ghoul",
            "Seeing Stars",
            "Zombiquarium",
            "Be-ghouled Twist",
            "Big Trouble Little Zombie",
            "Portal Combat",
            "Column Like You See 'Em",
            "Bobsled Bonanza",
            "Zombie Nimble Zombie Quick",
            "Whack a Zombie",
            "Last Stand",
            "ZomBotany 2",
            "Wall-nut Bowling 2",
            "Pogo Party",
            "Dr. Zomboss's Revenge",
            "Art Challenge Wall-nut",
            "Sunny Day",
            "Unsodded",
            "Big Time",
            "Art Challenge Sunflower",
            "Air Raid",
            "Ice Level",
            "Zen Garden",
            "High Gravity",
            "Grave Danger",
            "CHALLENGE_SHOVEL",
            "Dark Stormy Night",
            "Bungee Blitz",
            "Squirrels",
            "Tree of Wisdom",
            "Vasebreaker",
            "To the Left",
            "Third Vase",
            "Chain Reaction",
            "M is for Metal",
            "Scary Potter",
            "Hokey Pokey",
            "Another Chain Reaction",
            "Ace of Vase",
            "Vasebreaker Endless",
            "I, Zombie",
            "I, Zombie 2",
            "Can You Dig It?",
            "Totally Nuts",
            "Dead Zeppelin",
            "Me Smash!",
            "ZomBoogie",
            "Three Hit Wonder",
            "All your brainz r belong to us",
            "I, Zombie Endless,"
        };

        private static ListItem[] GetGameButtons(MemoryIO memIO)
        {
            List<ListItem> listItems = new List<ListItem>();
            for (int i = 0; i < 72; i++)
            {
                ListItem? nullableItem = memIO.TryGetMinigameButton(i);
                if (nullableItem == null)
                    continue;

                ListItem listItem = nullableItem.Value;
                listItem.text = MinigameNames[i];
                //Console.WriteLine("buttonPos: {0},{1}", listItem.relativePos.X, listItem.relativePos.Y);
                listItems.Add(listItem);
            }

            listItems.Add(new ListItem() { text = "Back To Menu", relativePos = new Vector2(0.1f, 0.95f) });

            return listItems.ToArray();
        }

        public BonusModeMenu(MemoryIO memIO) : base(memIO, "", GetGameButtons(memIO))
        {
        }

        protected override string? GetContent()
        {
            string inputDesc = "Inputs: Up and down to scroll, Confirm to select, Deny to close\r\n";
            return inputDesc + listItems[listIndex].text;
        }

        public override void Interact(InputIntent intent)
        {
            listItems = GetGameButtons(memIO);  //Update button list, because sometimes the screen loads before the buttons have been initialized :(
            
            base.Interact(intent);

            //Click quit button if user presses back
            if (intent == InputIntent.Deny)
                Program.Click(listItems[listItems.Length -1].relativePos);
        }
    }
}
