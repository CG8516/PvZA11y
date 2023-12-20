using Memory;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class MainMenu : Dialog
    {

        InputIntent[] lastIntents = new InputIntent[5];
        int lastIntentIndex = 0;

        static ListItem[] InitListItems(MemoryIO memIO)
        {
            bool finishedAdventure = memIO.GetAdventureCompletions() > 0;

            int level = memIO.GetPlayerLevel();

            bool zenGardenUnlocked = finishedAdventure || level >= 45;
            bool storeUnlocked = finishedAdventure || level >= 25;
            bool almanacUnlocked = finishedAdventure || level >= 15;

            int levelNum = level % 10;
            int worldNum = 1 + (level / 10);

            bool minigamesUnlocked = finishedAdventure || memIO.GetMinigamesUnlocked();
            bool puzzlesUnlocked = finishedAdventure || memIO.GetPuzzleUnlocked();
            bool survivalUnlocked = finishedAdventure || memIO.GetSurvivalUnlocked();

            //Final/zomboss level doesn't roll around to 6-0
            if (level == 50)
            {
                worldNum = 5;
                levelNum = 10;
            }


            ListItem[] listItems = new ListItem[]
            {
                new ListItem(){text = "Change User", relativePos = new Vector2(0.21f,0.25f)},
                new ListItem(){text = "Adventure. Level " + worldNum.ToString() + ", " + levelNum.ToString(), relativePos = new Vector2(0.7f,0.2f)},
                new ListItem(){text = "Mini-games" + (minigamesUnlocked? "" : " (Locked)"), relativePos = new Vector2(0.7f,0.4f)},
                new ListItem(){text = "Puzzle" + (puzzlesUnlocked? "" : " (Locked)"), relativePos = new Vector2(0.7f,0.5f)},
                new ListItem(){text = "Survival" + (survivalUnlocked? "" : " (Locked)"), relativePos = new Vector2(0.7f,0.65f)},
                new ListItem(){text = "Zen Garden" + (zenGardenUnlocked? "" : " (Locked)"), relativePos = new Vector2(0.3f,0.8f)},
                new ListItem(){text = "Almanac" + (almanacUnlocked? "" : " (Locked)"), relativePos = new Vector2(0.46f,0.78f)},
                new ListItem(){text = "Store" + (storeUnlocked? "" : " (Locked)"), relativePos = new Vector2(0.58f,0.85f)},                
                new ListItem(){text = "Options", relativePos = new Vector2(0.75f,0.85f)},
                new ListItem(){text = "Help", relativePos = new Vector2(0.83f,0.88f)},
                new ListItem(){text = "Quit", relativePos = new Vector2(0.92f,0.87f)}
            };


            return listItems;
        }

        public MainMenu(MemoryIO memIO) : base(memIO, "", InitListItems(memIO))
        {
        }

        public override string? SayTitle(bool shouldSay)
        {
            return GetContent();
        }

        protected override string? GetContent()
        {
            string titleString = "Main Menu";
            string bodyString = "\r\nWelcome back, " + memIO.mem.ReadString(memIO.ptr.lawnAppPtr + ",94c,04");  //TODO: Move this to memIO. //lawnapp,playerInfo,name
            string inputStr = "\r\nInputs: Up and down to scroll, Confirm button to select";
            return titleString + bodyString + inputStr;
        }

        public override void Interact(InputIntent intent)
        {
            base.Interact(intent);


            //Save last five input intents, so we can tell if back was pressed five times
            lastIntents[lastIntentIndex] = intent;
            lastIntentIndex++;
            lastIntentIndex %= 5;

            bool lastFiveDeny = true;
            for(int i =0; i < 5; i++)
            {
                if (lastIntents[i] != InputIntent.Deny)
                {
                    lastFiveDeny = false;
                    break;
                }
            }

            if(lastFiveDeny)
            {
                Array.Clear(lastIntents);
                Console.WriteLine("Enabling screenreader...");
                Config.current.ScreenReader = new AccessibleOutput.AutoOutput();
                Config.SaveConfig(Config.ScreenReaderSelection.Auto);
                Program.Say("Screenreader enabled", true);
                Program.PlayTone(1, 1, 400, 400, 100, SignalGeneratorType.Sin, 0);
            }

        }

        public override void ConfirmInteraction()
        {
            Program.Click(listItems[listIndex].relativePos);
        }
    }
}
