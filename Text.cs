using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;

namespace PvZA11y
{
    public static class Text
    {

        public static string langDir = "Language";

        public static string[] plantNames;

        public static string[] plantTooltips;

        public static string[] plantAlmanacDescriptions;

        public static Dictionary<int, string> TreeDialogue;

        public static string[] zombieNames;

        public static string[] zombieAlmanacDescriptions;

        public static string[] achievementNames;

        public static string[] achievementDescriptions;

        public static Tutorial tutorial = new Tutorial();
        public class Tutorial
        {
            public string[] NewGamePlus;

            public string[] Level1;

            public string[] Level2;

            public string[] Level3;

            public string[] Level4;

            public string[] Level5;

            public string[] Level6;

            public string[] Level8;

            public string[] Level10;

            public string[] Level11;

            public string[] Level15;

            public string[] Level21;

            public string[] Level31;

            public string[] Level35;

            public string[] Level36;

            public string[] Level41;

            public string[] Level50;

            public string[] ZomBotany;

            public string[] SlotMachine;

            public string[] ItsRainingSeeds;

            public string[] Beghouled;

            public string[] Invisighoul;

            public string[] SeeingStars;

            public string[] Zombiquarium;

            public string[] BeghouledTwist;

            public string[] BigTroubleLittleZombie;

            public string[] PortalCombat;

            public string[] ColumnLikeYouSeeEm;

            public string[] BobsledBonanza;

            public string[] ZombieNimbleZombieQuick;

            public string[] WhackAZombie;

            public string[] LastStand;

            public string[] ZomBotany2;

            public string[] WallnutBowling2;

            public string[] PogoParty;

            public string[] DrZombossRevenge;


        }

        public static Accessibility accessibility = new Accessibility();
        public class Accessibility
        {
            public Name name = new Name();
            public Description description = new Description();
            public Value value = new Value();

            public class Name
            {
                public string WrapCursorInMenus = "Menu cursor wrapping";
                public string WrapCursorOnGrids = "Grid cursor wrapping";
                public string WrapPlantSelection = "Plant selection wrapping";
                public string KeyRepetition = "Key repetition";
                public string DoubleTapDelay = "Double tap delay";
                public string ControllerVibration = "Controller vibration";
                public string RebindInputs = "Rebind inputs";
                public string RequireShovelConfirmation = "Shovel Confirmation";
                public string AutoCollectItems = "Automatic sun collection";
                public string GameplayTutorial = "Gameplay Tutorials";
                public string ZombieSonarInterval = "Automatic sonar interval";
                public string ZombieSonarOnRowChange = "Zombie Sonar on row change";
                public string ZombieTripwireRow = "Zombie tripwire column";
                public string ZombieCycleMode = "Zombie cycle mode";
                public string MoveOnZombieCycle = "Move when cycling zombies.";
                public string BeghouledMatchAssist = "Be-ghouled match assistance";
                public string Beghouled2MatchAssist = "Be-ghouled Twist match assistance";
                public string AutoWakeStinky = "Automatic Snail Bullying";
                public string SamplePlantOnSwitch = "Click plant when cycled";
                public string SayTilePosOnMove = "Say board position";
                public string SayPlantOnTileMove = "Say Plant/Object on cursor movement";
                public string SayZombieOnTileMove = "Say zombies on cursor tile";
                public string SayWhenTripwireCrossed = "Say when tripwire has been crossed";
                public string SaySunCountOnCollect = "Say sun count when collected";
                public string SayCoinValueOnCollect = "Say coin value when collected";
                public string SayAvailableInputs = "Say available inputs";
                public string ScreenReaderEngine = "Screen Reader Engine.";
                public string MenuPositionCueVolume = "Menu position volume";
                public string GridPositionCueVolume = "Grid position volume";
                public string HitBoundaryVolume = "Boundary hit volume";
                public string PlantSlotChangeVolume = "Selected slot volume";
                public string AutomaticZombieSonarVolume = "Automatic sonar volume";
                public string ManualZombieSonarVolume = "Manual sonar volume";
                public string PlantReadyCueVolume = "Plant ready volume";
                public string FoundObjectCueVolume = "Plant/Object finder volume";
                public string FastZombieCueVolume = "Fast zombie alert volume";
                public string DeadZombieCueVolume = "Zombie death indicator volume";
                public string ZombieOnTileVolume = "Zombie on tile alert volume";
                public string ZombieEntryVolume = "Zombie entry alert volume";
                public string ZombieTripwireVolume = "Zombie tripwire volume";
                public string BeghouledAssistVolume = "Be-ghouled assistance volume";
                public string MiscAlertCueVolume = "Miscellaneous alert volume";
                public string AudioCueMasterVolume = "Master Audio Cue Volume";
                public string RestartOnCrash = "Restart on crash";
                public string MoveMouseCursor = "Move mouse cursor";
                public string AutoLaunchGame = "Automatically launch game";
                public string Language = "Language";
            }

            public class Description
            {
                public string WrapCursorInMenus = "Jump to the opposite end of menus when passing the first or last item";
                public string WrapCursorOnGrids = "Jump to the opposite side of grids when passing the bounds";
                public string WrapPlantSelection = "Loop selection when cycling plants and Zen Garden tools";
                public string KeyRepetition = "Repeat directional inputs when held";
                public string DoubleTapDelay = "Higher values will allow more time to perform a double-tap. Lower values will require faster tapping, but may offer a more responsive experience";
                public string ControllerVibration = "Whether controller vibration will be used or not";
                public string RebindInputs = "Allows you to rebind all controls.";
                public string RequireShovelConfirmation = "Require the shovel button to be pressed twice, to avoid accidental shoveling";
                public string AutoCollectItems = "Highly recommended until sun collection has been made accessible. Automatically clicks sun, coins, and end-level rewards.";
                public string GameplayTutorial = "Highly recommended on first playthroughs. Provides helpful gameplay advice.";
                public string ZombieSonarInterval = "How frequently to perform whole-board zombie sonar sweeps.";
                public string ZombieSonarOnRowChange = "Which zombie sonar mode to automatically use when changing rows.";
                public string ZombieTripwireRow = "When any zombie is on, or to the left of this column, an alarm will play.";
                public string ZombieCycleMode = "Which mode to use when cycling through zombies on the board.";
                public string MoveOnZombieCycle = "Move your cursor to the zombie's position when cycling zombies";
                public string BeghouledMatchAssist = "Match assistance mode for the be-ghouled minigame.";
                public string Beghouled2MatchAssist = "Match assistance for the be-ghouled Twist minigame";
                public string AutoWakeStinky = "Automatically wake Stinky the Snail, whenever he falls asleep in zen garden mode.";
                public string SamplePlantOnSwitch = "When cycling plants, automatically pickup and drop the current plant to trigger an in-game sound effect. Causes lag when rapidly cycling through plants";
                public string SayTilePosOnMove = "Say the current board position when the cursor is moved";
                public string SayPlantOnTileMove = "Say which plant, gravestone, crater or vase is at the current position, whenever you move around the board.";
                public string SayZombieOnTileMove = "Says which zombies are on the current tile, when the cursor moves.";
                public string SayWhenTripwireCrossed = "If zombie tripwire is enabled, say when a zombie has crossed the tripwire.";
                public string SaySunCountOnCollect = "Say your current sun amount when any sun is collected.";
                public string SayCoinValueOnCollect = "Say the value of each coin/diamond when it's collected.";
                public string SayAvailableInputs = "Reads the available inputs when a new dialogue has opened.";
                public string ScreenReaderEngine = "Which screen reader engine to use, Use left and right to select, and press confirm to apply";
                public string MenuPositionCueVolume = "Indicates where the cursor is located on a menu or list";
                public string GridPositionCueVolume = "Indicates where the cursor is located on a grid";
                public string HitBoundaryVolume = "Indicates when the cursor passes the bounds of an unwrapped grid or list";
                public string PlantSlotChangeVolume = "Indicates which slot is currently selected";
                public string AutomaticZombieSonarVolume = "Used for automatic zombie sonar";
                public string ManualZombieSonarVolume = "Used when zombie sonar for current row is pressed manually";
                public string PlantReadyCueVolume = "Plays when the current plant is refreshed, and you have enough sun to place it.";
                public string FoundObjectCueVolume = "When navigating the board, plays a different tone if a plant, gravestone, crater or vase is on the current tile.";
                public string FastZombieCueVolume = "Plays when a pole-vaulting or football zombie enters the board.";
                public string DeadZombieCueVolume = "Plays when a zombie dies.";
                public string ZombieOnTileVolume = "Plays descending tones to indicate the number of zombies on the current tile, when the cursor moves to it.";
                public string ZombieEntryVolume = "Plays pitched tones to indicate when and where any zombies have entered the lawn.";
                public string ZombieTripwireVolume = "Background alarm that plays when any zombie is on the left side of the tripwire.";
                public string BeghouledAssistVolume = "Plays in Be-ghouled minigames, when a match can be found";
                public string MiscAlertCueVolume = "Used for various alerts throughout the game.";
                public string AudioCueMasterVolume = "Adjusts the volume of all non-speech audio cues.";
                public string RestartOnCrash = "Automatically attempt to restart the mod if it crashes";
                public string MoveMouseCursor = "Move the mouse cursor to visually indicate where clicks will be performed";
                public string AutoLaunchGame = "Attempt to load the game process, if it's not already running.";
                public string Language = "Which language to use";
            }

            public class Value
            {
                public string On = "On.";
                public string Off = "Off.";

                public string Sonar1 = "Full Sonar.";
                public string Sonar2 = "Beeps only.";
                public string Sonar3 = "Count only.";

                public string Beghouled0 = "Hardest, None.";
                public string Beghouled1 = "Easiest, When current plant can be dragged to make a match.";
                public string Beghouled2 = "Medium, When current plant can be part of a match, but might not be the one which needs to be dragged.";

                public string Seconds = " seconds.";

                public string ZombieCycle1 = "By ID . Ensures all zombies are cycled through in the same order.";
                public string ZombieCycle2 = "By distance. Cycle order will change based on zombie distance.";

                public string ScreenreaderWarning = "Warning. This will disable the screenreader. If you are blind or visually impaired, this setting may be difficult to find again.\r\nTo Re-enable the screenreader, press the deny button five times while at the main menu.\r\nIf you want to disable the screenreader, press the ok button again. Otherwise, press the deny button now.";

                public string AutomaticScreenReader = "Automatic.";

                /*
                
                 */
            }
        }

        public static Inputs inputs;
        public class Inputs
        {

        }

        static bool LoadLanguage(string langName)
        {
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            try
            {
                string[] newPlantNames = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\PlantNames.yaml"));
                string[] newPlantTooltips = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\PlantTooltips.yaml"));
                string[] newPlantAlmanacDescriptions = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\PlantAlmanacDescriptions.yaml"));
                string[] newZombieAlmanacDescriptions = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\ZombieAlmanacDescriptions.yaml"));
                string[] newZombieNames = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\ZombieNames.yaml"));
                string[] newAchievementNames = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\AchievementNames.yaml"));
                string[] newAchivementDescriptions = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\AchievementDescriptions.yaml"));
                var newTreeDialoge = deserializer.Deserialize<Dictionary<int, string>>(File.ReadAllText(langDir + "\\" + langName + "\\TreeDialogue.yaml"));
                Tutorial newTutorials = deserializer.Deserialize<Tutorial>(File.ReadAllText(langDir + "\\" + langName + "\\Tutorials.yaml"));

                plantNames = newPlantNames;
                plantTooltips = newPlantTooltips;
                plantAlmanacDescriptions = newPlantAlmanacDescriptions;
                zombieAlmanacDescriptions = newZombieAlmanacDescriptions;
                achievementNames = newAchievementNames;
                achievementDescriptions = newAchivementDescriptions;
                TreeDialogue = newTreeDialoge;
                tutorial = newTutorials;
                zombieNames = newZombieNames;
                Console.WriteLine("Language '{0}' loaded successfully!", langName);
                Config.current.LanguageID = K4os.Hash.xxHash.XXH32.DigestOf(Encoding.Unicode.GetBytes(langName));
                Config.SaveConfig();
                return true;
            }
            catch
            {
                return false;
            }


            return false;
        }

        public static void FindLanguages()
        {
            
            bool langExists = Directory.Exists(langDir);

            if(!langExists)
            {
                string errorMsg = "Error! Language directory not found!";
                Console.WriteLine(errorMsg);
                Program.Say(errorMsg);
                return;
            }

            var languageDirs = Directory.GetDirectories(langDir);

            if(languageDirs == null || languageDirs.Length == 0 )
            {
                string errorMsg = "Error! No language files found!";
                Console.WriteLine(errorMsg);
                Program.Say(errorMsg);
                return;
            }

            bool langLoaded = false;
            foreach (var dir in languageDirs)
            {
                string langName = dir.Substring(langDir.Length + 1);
                uint langHash = K4os.Hash.xxHash.XXH32.DigestOf(Encoding.Unicode.GetBytes(langName));
                if (langHash == Config.current.LanguageID)
                {
                    langLoaded = LoadLanguage(langName);
                    break;
                }
            }

            if(!langLoaded)
            {
                foreach(var dir in languageDirs)
                {
                    langLoaded = LoadLanguage(dir.Substring(langDir.Length + 1));
                    if (langLoaded)
                        break;
                }
            }

            if(!langLoaded)
            {
                string errorMsg = "Failed to load any language files!";
                Console.WriteLine(errorMsg);
                Program.Say(errorMsg);
                Console.WriteLine("Press enter to quit");
                Console.ReadLine();
                Environment.Exit(1);
            }

            YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();

            //Config.current.Language = "2";

            //Directory.CreateDirectory("Language\\English\\");

            //File.WriteAllText("Language\\English\\Tutorials.yaml", serializer.Serialize(tutorial), Encoding.Unicode);

            //File.WriteAllText("Language\\English\\PlantNames.yaml", serializer.Serialize(plantNames), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\PlantTooltips.yaml", serializer.Serialize(plantTooltips), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\PlantAlmanacDescriptions.yaml", serializer.Serialize(plantAlmanacDescriptions), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\ZombieNames.yaml", serializer.Serialize(zombieNames), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\ZombieAlmanacDescriptions.yaml", serializer.Serialize(zombieAlmanacDescriptions), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\AchievementNames.yaml", serializer.Serialize(achievementNames), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\AchievementDescriptions.yaml", serializer.Serialize(achievementDescriptions), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\TreeDialogue.yaml", serializer.Serialize(TreeDialogue), Encoding.Unicode);
            File.WriteAllText("Language\\English\\AccessibilityMenu.yaml", serializer.Serialize(accessibility), Encoding.Unicode);

            //var deserializer = new YamlDotNet.Serialization.Deserializer();
            //var treedial = deserializer.Deserialize<Dictionary<int, string>>(File.ReadAllText("Language\\English\\TreeDialogue.yaml"));

            //foreach(var entry in TreeDialogue)
            //foreach(var entry in treedial)
            //  Console.WriteLine("{0}: {1}", entry.Key, entry.Value);

        }

    }
}
