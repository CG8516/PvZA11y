using Memory;
using NAudio.Wave.SampleProviders;
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

        public override string? SayTitle(bool shouldSay)
        {
            string? content = listItems[listIndex].text;
            if (content != null)
            {
                Console.WriteLine(content);
                Program.Say(content);
            }
            return content;
            
        }

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
                listItem.extraData = i;
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
            return (Config.current.SayAvailableInputs ? inputDesc : "") + listItems[listIndex].text + GetCompletionString();
        }

        string GetCompletionString(bool say = false)
        {
            if (listIndex == listItems.Length - 1)
                return "";
            bool isComplete = memIO.GetChallengeScore(listItems[listIndex].extraData + 1) > 0;
            string completionString = isComplete ? "Complete" : "";
            if(say)
            {
                if (!isComplete)
                    completionString = "Incomplete";
                Console.WriteLine(completionString);
                Program.Say(completionString);
            }
            if (!say && isComplete)
                completionString = ", " + completionString;
            return completionString;
        }

        public override void DenyInteraction()
        {
            Program.Click(listItems[listItems.Length - 1].relativePos);
        }

        void SayTrophyCount()
        {
            int pageType = 0;   //0: minigames, 1: puzzle, 2: survival
            if (listItems[0].extraData +1 == (int)GameMode.VaseBreaker1)
                pageType = 1;
            if (listItems[0].extraData +1 == (int)GameMode.SurvivalDay)
                pageType = 2;
            int maxTrophies = 20;
            if (pageType == 1)
                maxTrophies = 18;
            if (pageType == 2)
                maxTrophies = 10;

            int completions = 0;
            for(int i =0; i < listItems.Length-1; i++)
            {
                if (memIO.GetChallengeScore(listItems[i].extraData + 1) > 0)
                    completions++;
            }

            string trophyStr = completions + " of " + maxTrophies + " trophies";
            Console.WriteLine(trophyStr);
            Program.Say(trophyStr);
        }

        public override void Interact(InputIntent intent)
        {
            listItems = GetGameButtons(memIO);  //Update button list, because sometimes the screen loads before the buttons have been initialized :(

            int lastIndex = listIndex;
            switch (intent)
            {
                case InputIntent.Up:
                    listIndex--;
                    break;
                case InputIntent.Down:
                    listIndex++;
                    break;
                case InputIntent.Confirm:
                    ConfirmInteraction();
                    break;
                case InputIntent.Deny:
                    DenyInteraction();
                    break;
                case InputIntent.Info1:
                    SayTitle(true);
                    return;
                case InputIntent.Info2:
                    GetCompletionString(true);
                    return;
                case InputIntent.Info3:
                    SayTrophyCount();
                    return;

            }

            if (listIndex != lastIndex)
            {
                if (listIndex < 0 || listIndex >= listItems.Length)
                    Program.Vibrate(0.1f, 0.1f, 50);
                ConfineInteractionIndex();
                Vector2 mousePos = GetItemPos();
                Program.MoveMouse(mousePos.X, mousePos.Y);

                float freq = 1250.0f - ((((float)listIndex / (float)listItems.Length) * 5000.0f) / 5.0f);
                bool isComplete = memIO.GetChallengeScore(listItems[listIndex].extraData + 1) > 0;
                if(isComplete)
                    Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Square);
                else
                    Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Sin);
                string itemName = listItems[listIndex].text + GetCompletionString(false);
                Console.WriteLine(itemName);
                Program.Say(itemName);
            }
            else if (intent is (InputIntent.Up or InputIntent.Down))
                Program.PlayBoundaryTone();
        }
    }
}
