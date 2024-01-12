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

        public static string[] levelTypes;

        public static Menus menus = new Menus();
        public class Menus
        {
            //Main
            public string mainMenu;
            public string welcomeBack;
            public string changeUser;
            public string adventureLevel;
            public string minigames;
            public string puzzle;
            public string survival;
            public string achievements;
            public string zenGarden;
            public string almanac;
            public string store;
            public string options;
            public string help;
            public string quit;
            public string locked;
            public string silverTrophy;
            public string goldTrophy;
            public string replayCredits;
            public string next;
            public string repeat;
            public string yes;
            public string no;
            public string continueGame;
            public string Continue;
            public string restartLevel;
            public string cancel;
            public string achievementComplete;
            public string minigameComplete;
            public string minigameNotComplete;
            public string trophyCount;
            public string gameUpgrades;
            public string plantUpgrades;
            public string plantUnavailable;
            public string purchasablePlantUnavailable;
            public string plantLocked;
            public string plantPicked;
            public string imitation;
            public string plantNotAllowed;
            public string emptySlot;
            public string pickMorePlants;
            public string pickLastPlant;
            public string choosePlants;
            public string notRecommended;
            public string aquatic;
            public string nocturnal;
            public string notAllowed;
            public string steamCloudMessage;
            public string localSave;
            public string steamSave;
            public string boxChecked;
            public string boxUnchecked;
            public string musicSlider;
            public string sfxSlider;
            public string accelCheckbox;
            public string fullscreenCheckbox;
            public string viewAlmanac;
            public string credits;
            public string accessibilitySettings;
            public string createUser;
            public string renameUser;
            public string pressStart;
        }

        public static ZenGarden zenGarden = new ZenGarden();

        public class ZenGarden
        {
            public string emptyTile;
            public string noPlant;
            public string needyPlants;
            public string needyPlant;
            public string treeHeight;
            public string mainGarden;
            public string mushroomGarden;
            public string aquarium;
            public string treeOfWisdom;
            public string happy;
            public string nocturnal;
            public string aquatic;
            public string waterNeeded;
            public string fertilizerNeeded;
            public string bugSprayNeeded;
            public string phonographNeeded;
            public string wheelBarrow;
            public string wheelBarrowHolding;
            public string nextGarden;
            public string chocolate;
            public string glove;
            public string sell;
            public string phonograph;
            public string bugSpray;
            public string fertilizer;
            public string wateringCan;
            public string goldenCan;
            public string treeFood;
        }



        public static Store store = new Store();
        public class Store
        {
            public string NoStock;
            public string NoStockWarning;
            public string NotEnoughCoins;
            public string PurchaseConfirmation;
            public string CoinCount;
            public string ZenGardenLocked;
            public string UnlockMoreUpgrades;
            public string AllUpgradesObtained;
            public string PurchaseComplete;
            public string PurchaseCancelled;
            public string CurrencyName;


            public ItemNames itemNames = new ItemNames();
            public ItemDescriptions itemDescriptions = new ItemDescriptions();
            public class ItemNames
            {
                public string seedSlot;
                public string poolCleaners;
                public string gardenRake;
                public string roofCleaners;
                public string wallnutAid;
                public string goldenCan;
                public string fertilizer;
                public string bugSpray;
                public string phonograph;
                public string gardeningGlove;
                public string mushroomGarden;
                public string aquariumGarden;
                public string wheelbarrow;
                public string stinky;
                public string treeOfWisdom;
                public string treeFood;
            }

            public class ItemDescriptions
            {
                public string seedSlot;
                public string poolCleaners;
                public string gardenRake;
                public string roofCleaners;
                public string wallnutAid;
                public string imitater;
                public string gatlingPea;
                public string twinSunflower;
                public string gloomShroom;
                public string catTail;
                public string spikeRock;
                public string goldMagnet;
                public string winterMelon;
                public string cobCannon;
                public string goldenCan;
                public string fertilizer;
                public string bugSpray;
                public string phonograph;
                public string gardeningGlove;
                public string mushroomGarden;
                public string aquariumGarden;
                public string wheelbarrow;
                public string stinky;
                public string treeOfWisdom;
                public string treeFood;
                public string marigold;
            }
        }

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

            public string[] WallnutBowling;
        }

        public static Accessibility accessibility = new Accessibility();
        public class Accessibility
        {
            public Name name = new Name();
            public Description description = new Description();
            public Value value = new Value();
            public Category category = new Category();
            public class Category
            {
                public string input;
                public string gameplay;
                public string narration;
                public string volume;
                public string other;
            }

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
                public string BackgroundPlantReadyCueVolume;
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
                public string BackgroundPlantReadyCueVolume;
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

        public static InputRebind inputRebind = new InputRebind();

        public class InputRebind
        {
            public InputNames inputNames = new InputNames();
            public InputDescriptions inputDescriptions = new InputDescriptions();
            public class InputNames
            {
                public string up;
                public string down;
                public string left;
                public string right;
                public string confirm;
                public string deny;
                public string start;
                public string option;
                public string cycleLeft;
                public string cycleRight;
                public string zombieMinus;
                public string zombiePlus;
                public string info1;
                public string info2;
                public string info3;
                public string info4;
                public string slot;
            }

            public class InputDescriptions
            {
                public string up;
                public string down;
                public string left;
                public string right;
                public string confirm;
                public string deny;
                public string start;
                public string option;
                public string cycleLeft;
                public string cycleRight;
                public string zombieMinus;
                public string zombiePlus;
                public string info1;
                public string info2;
                public string info3;
                public string info4;
                public string slot;
            }

            public string resetKeyboard;
            public string resetController;
            public string setKeyboard;
            public string setController;
            public string keyboardReset;
            public string controllerReset;
            public string rebindComplete;
            public string pressKeyboard;
            public string pressController;
            public string pressKeyboardOrController;
            public string keyAlreadyBound;
            public string buttonAlreadyBound;
            public string keyUnbound;
            public string buttonUnbound;
            public string inputBound;
            public string keyboardBind;
            public string controllerBind;
            public string controllerExtraBind;
            public string leftStick;
            public string rightStick;
            public string leftTrigger;
            public string rightTrigger;
            public string unknownKey;
        }
        public static Inputs inputs = new Inputs();
        public class Inputs
        {
            public string treeHeight;
            public string zenGarden;
            public string userPicker;
            public string store;
            public string seedPicker;
            public string optionsMenu;
            public string mainMenu;
            public string buttonPicker;
            public string minigameSelector;
            public string almanacIndex;
            public string almanacGrid;
            public string achievements;
            public string accessibility;
            public string rebindMenu;
        }

        public static Awards awards = new Awards();
        public class Awards
        {
            public string newPlant;
            public string bossTitle;
            public string bossMessage;
            public string noteTitle;
            public string note1;
            public string note2;
            public string note3;
            public string note4;
            public string note5;
            public string note6;
            public string shovelTitle;
            public string shovelMessage;
            public string almanacTitle;
            public string almanacMessage;
            public string shopTitle;
            public string shopMessage;
            public string tacoTitle;
            public string tacoMessage;
            public string zenGardenTitle;
            public string zenGardenMessage;
            public string trophyTitle;
            public string vaseBreaker;
            public string iZombie;
            public string minigame;
            public string survival;
            public string badHelpTitle;
            public string badHelpMessage;
        }

        public static Game game = new Game();
        public class Game
        {
            public string completion;
            public string waveStatus;
            public string finalWave;
            public string dirt;
            public string grass;
            public string water;
            public string roof;
            public string emptyTileString;
            public string crater;
            public string graveStone;
            public string plantInVase;
            public string zombieInVase;
            public string vase;
            public string plantVase;
            public string zombieVase;
            public string ice;
            public string starGuide;
            public string roundPortal;
            public string squarePortal;
            public string squished;
            public string sleeping;
            public string buried;
            public string armed;
            public string chewing;
            public string small;
            public string magnetFilled;
            public string laddered;
            public string cobCharging;
            public string cobReady;
            public string nutDamaged;
            public string nutChipped;
            public string tallnutCrying;
            public string garlicSad;
            public string garlicNibbled;
            public string pumpkinShield;
            public string magBucket;
            public string magHelmet;
            public string magDoor;
            public string magPogo;
            public string magJack;
            public string magLadder;
            public string magPickaxe;
            public string hasRoundPortal;
            public string hasSquarePortal;
            public string bossHead;
            public string offBoard;
            public string hypnotized;
            public string dinted;
            public string damaged;
            public string exposed;
            public string ripped;
            public string shredded;
            public string angry;
            public string armless;
            public string headless;
            public string underground;
            public string grounded;
            public string tired;
            public string falling;
            public string scratched;
            public string wounded;
            public string icy;
            public string buttered;
            public string hungry;
            public string iceBall;
            public string fireBall;
            public string notEnoughSun;
            public string imitation;
            public string plantReady;
            public string plantRefreshing;
            public string plantSun;
            public string frozen;
            public string unfrozen;
            public string noLawnmower;
            public string lawnMower;
            public string poolCleaner;
            public string roofSweeper;
            public string hasBrain;
            public string noBrain;
            public string noZombies;
            public string buySnorkel;
            public string buyTrophy;
            public string buyBrain;
            public string purchased;
            public string upgradePeashooters;
            public string upgradeShrooms;
            public string upgradeNuts;
            public string shufflePlants;
            public string repairCrater;
            public string collectSun;
            public string refreshPercent;
            public string alreadyPurchased;
            public string waitingForPlants;
            public string brainStatus;
            public string slotStatus;
            public string beghouledStatus;
            public string starStatus;
            public string vasesRemaining;
            public string vaseRemaining;
            public string noVases;
            public string survivalEndlessStage;
            public string survivalStage;
            public string zombiquariumGoal;
            public string coinCount;
            public string sunCount;
            public string tripwire1;
            public string tripwire2;
        }

        public static Almanac almanac = new Almanac();
        public class Almanac
        {
            public string plants;
            public string zombies;
            public string index;
            public string fast;
            public string slow;
            public string verySlow;
            public string rechargeTime;
            public string sun;
            public string mysteryZombie;
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
                string[] newLevelTypes = deserializer.Deserialize<string[]>(File.ReadAllText(langDir + "\\" + langName + "\\LevelTypes.yaml"));
                Store newStore = deserializer.Deserialize<Store>(File.ReadAllText(langDir + "\\" + langName + "\\Store.yaml"));
                Game newGame = deserializer.Deserialize<Game>(File.ReadAllText(langDir + "\\" + langName + "\\Game.yaml"));
                ZenGarden newZenGarden = deserializer.Deserialize<ZenGarden>(File.ReadAllText(langDir + "\\" + langName + "\\ZenGarden.yaml"));
                Inputs newInputs = deserializer.Deserialize<Inputs>(File.ReadAllText(langDir + "\\" + langName + "\\Inputs.yaml"));
                Awards newAwards = deserializer.Deserialize<Awards>(File.ReadAllText(langDir + "\\" + langName + "\\Awards.yaml"));
                Menus newMenus = deserializer.Deserialize<Menus>(File.ReadAllText(langDir + "\\" + langName + "\\Menus.yaml"));
                Almanac newAlmanac = deserializer.Deserialize<Almanac>(File.ReadAllText(langDir + "\\" + langName + "\\Almanac.yaml"));
                InputRebind newRebind = deserializer.Deserialize<InputRebind>(File.ReadAllText(langDir + "\\" + langName + "\\RebindMenu.yaml"));

                //int codepage = 437; //EN-US
                Encoding newEncoding = Encoding.UTF8;
                try
                {
                    string codePageStr = File.ReadAllText(langDir + "\\" + langName + "\\codepage.txt");
                    Console.WriteLine("Read codepage string");
                    int codepage = int.Parse(codePageStr);
                    Console.WriteLine("Read codepage int: {0}", codepage);
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    newEncoding = Encoding.GetEncoding(codepage);
                    Console.WriteLine("Using codepage {0}", codepage);
                }
                catch
                {
                    Console.WriteLine("Using default encoding...");
                }

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
                levelTypes = newLevelTypes;
                store = newStore;
                game = newGame;
                zenGarden = newZenGarden;
                inputs = newInputs;
                awards = newAwards;
                menus = newMenus;
                almanac = newAlmanac;
                inputRebind = newRebind;
                Program.encoding = newEncoding;
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
            //File.WriteAllText("Language\\English\\LevelTypes.yaml", serializer.Serialize(levelTypes), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Store.yaml", serializer.Serialize(store), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Game.yaml", serializer.Serialize(game), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\ZenGarden.yaml", serializer.Serialize(zenGarden), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Inputs.yaml", serializer.Serialize(inputs), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\RebindMenu.yaml", serializer.Serialize(inputRebind), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Awards.yaml", serializer.Serialize(awards), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Menus.yaml", serializer.Serialize(menus), Encoding.Unicode);
            //File.WriteAllText("Language\\English\\Almanac.yaml", serializer.Serialize(almanac), Encoding.Unicode);

            //var deserializer = new YamlDotNet.Serialization.Deserializer();
            //var treedial = deserializer.Deserialize<Dictionary<int, string>>(File.ReadAllText("Language\\English\\TreeDialogue.yaml"));

            //foreach(var entry in TreeDialogue)
            //foreach(var entry in treedial)
            //  Console.WriteLine("{0}: {1}", entry.Key, entry.Value);

        }

    }
}
