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
        Achievements? achievements = null;

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

            int trophyState = 0;

            if (memIO.GetAdventureCompletions() > 0)
                trophyState = 1;

            if (CheckTrophies(memIO) >= 48)
                trophyState = 2;


            ListItem[] listItems = new ListItem[]
            {
                new ListItem(){text = Text.menus.changeUser, relativePos = new Vector2(0.21f,0.25f)},
                new ListItem(){text = Text.menus.adventureLevel + worldNum.ToString() + ", " + levelNum.ToString(), relativePos = new Vector2(0.7f,0.2f)},
                new ListItem(){text = Text.menus.minigames + (minigamesUnlocked? "" : Text.menus.locked), relativePos = new Vector2(0.7f,0.4f)},
                new ListItem(){text = Text.menus.puzzle + (puzzlesUnlocked? "" : Text.menus.locked), relativePos = new Vector2(0.7f,0.5f)},
                new ListItem(){text = Text.menus.survival + (survivalUnlocked? "" : Text.menus.locked), relativePos = new Vector2(0.7f,0.65f)},
                new ListItem(){text = Text.menus.achievements + (trophyState == 1 ? ", " + Text.menus.silverTrophy : trophyState == 2 ? ", " + Text.menus.goldTrophy : ""), relativePos = new Vector2(0.1f,0.6f)},
                new ListItem(){text = Text.menus.zenGarden + (zenGardenUnlocked? "" : Text.menus.locked), relativePos = new Vector2(0.3f,0.8f)},
                new ListItem(){text = Text.menus.almanac + (almanacUnlocked? "" : Text.menus.locked), relativePos = new Vector2(0.46f,0.78f)},
                new ListItem(){text = Text.menus.store + (storeUnlocked? "" : Text.menus.locked), relativePos = new Vector2(0.58f,0.85f)},                
                new ListItem(){text = Text.menus.options, relativePos = new Vector2(0.75f,0.85f)},
                new ListItem(){text = Text.menus.help, relativePos = new Vector2(0.83f,0.88f)},
                new ListItem(){text = Text.menus.quit, relativePos = new Vector2(0.92f,0.87f)}
            };


            return listItems;
        }

        static int CheckTrophies(MemoryIO memIO)
        {
            int trophyCount = 0;

            //Puzzle modes
            for (int i = (int)GameMode.VaseBreaker1; i < (int)GameMode.VaseBreakerEndless; i++)
            {
                if (memIO.GetChallengeScore(i) > 0)
                    trophyCount++;
            }
            for (int i = (int)GameMode.IZombie1; i < (int)GameMode.IZombieEndless; i++)
            {
                if (memIO.GetChallengeScore(i) > 0)
                    trophyCount++;
            }

            //Survival modes
            for (int i = (int)GameMode.SurvivalDay; i < (int)GameMode.SurvivalEndless1; i++)
            {
                if (memIO.GetChallengeScore(i) > 0)
                    trophyCount++;
            }

            //Minigames
            for (int i = (int)GameMode.ZomBotany; i <= (int)GameMode.DrZombossRevenge; i++)
            {
                if (memIO.GetChallengeScore(i) > 0)
                    trophyCount++;
            }

            return trophyCount;
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
            string titleString = Text.menus.mainMenu;
            string bodyString = Text.menus.welcomeBack + memIO.mem.ReadString(memIO.ptr.lawnAppPtr + ",94c,04");  //TODO: Move this to memIO. //lawnapp,playerInfo,name
            string inputStr = Text.inputs.mainMenu ;
            return titleString + "\r\n" + bodyString + (Config.current.SayAvailableInputs ? "\r\n" + inputStr : "");
        }

        public override void Interact(InputIntent intent)
        {
            if (achievements != null)
            {
                achievements.Interact(intent);
                if (achievements.menuClosed)
                {
                    achievements = null;
                    hasUpdatedContents = true;
                    hasReadContent = false;
                }

                return;
            }

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
            if (listIndex == 5)
            {
                achievements = new Achievements(memIO);
                string? title = achievements.GetCurrentWidgetText();
                if(title != null)
                {
                    Console.WriteLine(title);
                    Program.Say(title);
                }
            }
            else
                Program.Click(listItems[listIndex].relativePos);
        }
    }
}
