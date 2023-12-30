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

        public static string[] minigameNames;

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
                public string WrapCursorInMenus;
                public string WrapCursorOnGrids;
                public string WrapPlantSelection;
                public string KeyRepetition;
                public string DoubleTapDelay;
                public string ControllerVibration;
                public string RebindInputs;
                public string RequireShovelConfirmation;
                public string AutoCollectItems;
                public string GameplayTutorial;
                public string ZombieSonarInterval;
                public string ZombieSonarOnRowChange;
                public string ZombieTripwireRow;
                public string ZombieCycleMode;
                public string MoveOnZombieCycle;
                public string BeghouledMatchAssist;
                public string Beghouled2MatchAssist;
                public string AutoWakeStinky;
                public string SamplePlantOnSwitch;
                public string SayTilePosOnMove;
                public string SayPlantOnTileMove;
                public string SayZombieOnTileMove;
                public string SayWhenTripwireCrossed;
                public string SaySunCountOnCollect;
                public string SayCoinValueOnCollect;
                public string SayAvailableInputs;
                public string ScreenReaderEngine;
                public string MenuPositionCueVolume;
                public string GridPositionCueVolume;
                public string HitBoundaryVolume;
                public string PlantSlotChangeVolume;
                public string AutomaticZombieSonarVolume;
                public string ManualZombieSonarVolume;
                public string PlantReadyCueVolume;
                public string FoundObjectCueVolume;
                public string FastZombieCueVolume;
                public string DeadZombieCueVolume;
                public string ZombieOnTileVolume;
                public string ZombieEntryVolume;
                public string ZombieTripwireVolume;
                public string BeghouledAssistVolume;
                public string MiscAlertCueVolume;
                public string AudioCueMasterVolume;
                public string RestartOnCrash;
                public string MoveMouseCursor;
                public string AutoLaunchGame;
                public string Language;
            }

            public class Description
            {
                public string WrapCursorInMenus;
                public string WrapCursorOnGrids;
                public string WrapPlantSelection;
                public string KeyRepetition;
                public string DoubleTapDelay;
                public string ControllerVibration;
                public string RebindInputs;
                public string RequireShovelConfirmation;
                public string AutoCollectItems;
                public string GameplayTutorial;
                public string ZombieSonarInterval;
                public string ZombieSonarOnRowChange;
                public string ZombieTripwireRow;
                public string ZombieCycleMode;
                public string MoveOnZombieCycle;
                public string BeghouledMatchAssist;
                public string Beghouled2MatchAssist;
                public string AutoWakeStinky;
                public string SamplePlantOnSwitch;
                public string SayTilePosOnMove;
                public string SayPlantOnTileMove;
                public string SayZombieOnTileMove;
                public string SayWhenTripwireCrossed;
                public string SaySunCountOnCollect;
                public string SayCoinValueOnCollect;
                public string SayAvailableInputs;
                public string ScreenReaderEngine;
                public string MenuPositionCueVolume;
                public string GridPositionCueVolume;
                public string HitBoundaryVolume;
                public string PlantSlotChangeVolume;
                public string AutomaticZombieSonarVolume;
                public string ManualZombieSonarVolume;
                public string PlantReadyCueVolume;
                public string FoundObjectCueVolume;
                public string FastZombieCueVolume;
                public string DeadZombieCueVolume;
                public string ZombieOnTileVolume;
                public string ZombieEntryVolume;
                public string ZombieTripwireVolume;
                public string BeghouledAssistVolume;
                public string MiscAlertCueVolume;
                public string AudioCueMasterVolume;
                public string RestartOnCrash;
                public string MoveMouseCursor;
                public string AutoLaunchGame;
                public string Language;
            }

            public class Value
            {
                public string On;
                public string Off;
                public string Sonar1;
                public string Sonar2;
                public string Sonar3;
                public string Beghouled0;
                public string Beghouled1;
                public string Beghouled2;
                public string Seconds;
                public string ZombieCycle1;
                public string ZombieCycle2;
                public string ScreenreaderWarning;
                public string AutomaticScreenReader;
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
                Accessibility newAccessibility = deserializer.Deserialize<Accessibility>(File.ReadAllText(langDir + "\\" + langName + "\\AccessibilityMenu.yaml"));
                string[] newMinigames = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\Minigames.yaml"));

                plantNames = newPlantNames;
                plantTooltips = newPlantTooltips;
                plantAlmanacDescriptions = newPlantAlmanacDescriptions;
                zombieAlmanacDescriptions = newZombieAlmanacDescriptions;
                achievementNames = newAchievementNames;
                achievementDescriptions = newAchivementDescriptions;
                TreeDialogue = newTreeDialoge;
                tutorial = newTutorials;
                accessibility = newAccessibility;
                zombieNames = newZombieNames;
                minigameNames = newMinigames;
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
            //File.WriteAllText("Language\\English\\AccessibilityMenu.yaml", serializer.Serialize(accessibility), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Minigames.yaml", serializer.Serialize(minigameNames), Encoding.Unicode);

            //var deserializer = new YamlDotNet.Serialization.Deserializer();
            //var treedial = deserializer.Deserialize<Dictionary<int, string>>(File.ReadAllText("Language\\English\\TreeDialogue.yaml"));

            //foreach(var entry in TreeDialogue)
            //foreach(var entry in treedial)
            //  Console.WriteLine("{0}: {1}", entry.Key, entry.Value);

        }

    }
}
