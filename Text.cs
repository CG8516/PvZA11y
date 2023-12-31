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
            public string mainMenu = "Main Menu";
            public string welcomeBack = "Welcome back, ";
            public string changeUser = "Change User";
            public string adventureLevel = "Adventure. Level ";
            public string minigames = "Mini-games";
            public string puzzle = "Puzzle";
            public string survival = "Survival";
            public string achievements = "Achievements";
            public string zenGarden = "Zen Garden";
            public string almanac = "Almanac";
            public string store = "Store";
            public string options = "Options";
            public string help = "Help";
            public string quit = "Quit";
            public string locked = " (Locked)";
            public string silverTrophy = "Silver trophy";
            public string goldTrophy = "Gold trophy";

            public string replayCredits = "Replay credits";

            public string next = "Next";
            public string repeat = "Repeat";

            public string yes = "Yes";
            public string no = "No";

            public string continueGame = "Continue Game?\r\nDo you want to continue your current game, or restart the level?";
            public string Continue = "Continue";
            public string restartLevel = "Restart Level";
            public string cancel = "Cancel";

            public string achievementComplete = "Completed: ";

            public string minigameComplete = "Complete";
            public string minigameNotComplete = "Incomplete";

            public string trophyCount = "[0] of [1] trophies";

            public string gameUpgrades = "Game Upgrades.";
            public string plantUpgrades = "Plant Upgrades.";

            public string plantUnavailable = "Unavailable.";
            public string purchasablePlantUnavailable = " Buy it from the store.";
            public string plantLocked = "Locked. Keep playing adventure mode to unlock more plants.";

            public string plantPicked = "Picked. ";
            public string imitation = "Imitation ";
            public string plantNotAllowed = "That plant is not allowed on this level.";
            public string emptySlot = "Empty Slot";

            public string pickMorePlants = "Please select [0] more plants to begin";
            public string pickLastPlant = "Please select 1 more plant to begin";

            public string choosePlants = "Choose your plants!";
            public string notRecommended = "Not recommended. ";
            public string aquatic = "Aquatic. ";
            public string nocturnal = "Nocturnal. ";
            public string notAllowed = "Not allowed. ";

            public string steamCloudMessage = "Steam Cloud Saving Active.\r\nWe have detected that you have a steam cloud save for this game, as well as a save stored on this machine.\r\nWhich save would you like to use?\r\nChoosing cancel will disable Steam Cloud for this session.";
            public string localSave = "Local Save";
            public string steamSave = "Steam Save";

            public string boxChecked = "Checked: ";
            public string boxUnchecked = "Unchecked: ";

            public string musicSlider = "Music Volume Slider";
            public string sfxSlider = "SFX Volume Slider";
            public string accelCheckbox = "3D Acceleration Checkbox";
            public string fullscreenCheckbox = "Fullscreen Checkbox";
            public string viewAlmanac = "View Almanac";
            public string credits = "Credits";
            public string accessibilitySettings = "Accessibility Settings";

            public string createUser = "Create user. Type a username, then press enter to confirm, or press escape to cancel.";
            public string renameUser = "Rename user. Type a username, then press enter to confirm, or press escape to cancel.";
        }

        public static ZenGarden zenGarden = new ZenGarden();
        public class ZenGarden
        {
            public string emptyTile = "Empty tile";
            public string noPlant = "No plant";
            public string needyPlants = "[0] plants need attention";
            public string needyPlant = "1 plant needs attention";
            public string treeHeight = "[0] Feet Tall.";
            public string mainGarden = "Main garden";
            public string mushroomGarden = "Mushroom garden";
            public string aquarium = "Aquarium";
            public string treeOfWisdom = "Tree Of Wisdom.";

            public string happy = "Happy";
            public string nocturnal = "Nocturnal. Needs to be moved to mushroom garden";
            public string aquatic = "Aquatic. Needs to be moved to aquarium garden";
            public string waterNeeded = "Water needed";
            public string fertilizerNeeded = "Fertilizer needed";
            public string bugSprayNeeded = "Bug Spray needed";
            public string phonographNeeded = "Phonograph needed";

            public string wheelBarrow = "WheelBarrow";
            public string wheelBarrowHolding = "WheelBarrow with [0]";
            public string nextGarden = "Next Garden";
            public string chocolate = "Chocolate: ";
            public string glove = "Glove";
            public string sell = "Sell";

            public string phonograph = "Phonograph";
            public string bugSpray = "Bug Spray: ";
            public string fertilizer = "Fertilizer: ";
            public string wateringCan = "Watering Can";
            public string goldenCan = "Golden Watering Can";
            public string treeFood = "Tree Food: ";
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

        public static Inputs inputs = new Inputs();
        public class Inputs
        {
            public string treeHeight = "Press Info1 to say tree height.";
            public string zenGarden = "Inputs: Directions to move around garden, Confirm to use tool, Deny or Start to leave, Info1 to say plant on current tile, Info2 to say plant need on current tile, Info3 to say number of needy plants, CycleLeft/CycleRight to change tools, Option to visit the store.";
            public string userPicker = "Inputs: Up and Down to scroll, Confirm to select, Deny to close, Info1 to rename, Info2 to delete.";
            public string store = "Inputs: Confirm to buy, Deny to close, Info1 to say coin balance, Horizontal directions and Cycle buttons to switch categories, Vertical directions to scroll items.";
            public string seedPicker = "Inputs: Directional input to Navigate grid, Confirm to select/deselect plant, Deny to pause, Start to start level, Info1 to list zombies in level, Info2 to say level type, CycleLeft/CycleRight to list selected plants, Info3 to add or remove imitater clone of current plant.";
            public string optionsMenu = "Inputs: Up and Down to scroll list, Confirm to select, Left and Right to adjust sliders, Deny to close.";
            public string mainMenu = "Inputs: Up and down to scroll, Confirm button to select";
            public string buttonPicker = "Inputs: Confirm to select, Deny to reject, Info1 to repeat, Up and Down to scroll options.";
            public string minigameSelector = "Inputs: Up and down to scroll, Confirm to select, Deny to close.";
            public string almanacIndex = "Inputs: Directions to change option, Confirm to select, Deny to close.";
            public string almanacGrid = "Inputs: Directions to navigate grid, Deny to return to index.";
            public string achievements = "Inputs: Up and Down to scroll, Deny to close.";
            public string accessibility = "Inputs: Up and Down to scroll list, CycleLeft and CycleRight to jump to categories, Confirm to toggle, Left and Right to toggle or change values, Deny to go back, Info1 to repeat description.";
        }

        public static Awards awards = new Awards();
        public class Awards
        {
            public string newPlant = "You got a new plant!";
            public string bossTitle = "You have defeated the Boss Zombie!";
            public string bossMessage = "Congratulations!  You have most triumphantly fended off the zombie attack!  Your lawn is safe... for now!";

            public string noteTitle = "You found a note!";
            public string note1 = "Hello, we are about to launch an all-out attack on your houze. Sincerely, the Zombies";
            public string note2 = "Hello, We wood like to visit for a midnight znack. How does icecream and brains zound? Sincerely, the Zombies";
            public string note3 = "Hello, We herd you were having a pool party. We think that iz fun. Well be rite over. Sincerely, the Zombies";
            public string note4 = "Hello, This iz your muther. Please come over to my house for 'meatloaf'. Leave your front door open and your lawn unguarded. Sincerely, mom (not the Zombies)";
            public string note5 = "Homeowner, you have failed to submit to our rightful claim. Be advised that unless you comply, we will be forced to take extreme action. Please remit your home and brains to us forthwith. Sincerely, Dr. Edgar Zomboss";
            public string note6 = "Ok, you win. No more eatin brains for us. We just want to make music video with you now. Sincerely, the Zombies";

            public string shovelTitle = "You got the shovel!";
            public string shovelMessage = "Lets you dig up a plant to make room for another plant";

            public string almanacTitle = "You found a suburban almanac!";
            public string almanacMessage = "Keeps track of all plants and zombies you encounter";

            public string shopTitle = "You found Crazy Dave's car key!";
            public string shopMessage = "Now you can visit Crazy Dave's shop!";

            public string tacoTitle = "You found a taco!";
            public string tacoMessage = "What are you going to do with a taco?";

            public string zenGardenTitle = "You found a watering can!";
            public string zenGardenMessage = "Now you can play Zen Garden Mode!";

            public string trophyTitle = "You got a trophy!";
            public string vaseBreaker = "You've unlocked a new Vasebreaker level!";
            public string iZombie = "You've unlocked a new 'I, Zombie' level!";
            public string minigame = "You've unlocked a new mini-game!";
            public string survival = "You've unlocked a new survival level!";

            public string badHelpTitle = "Help for Plants and Zombies Game";
            public string badHelpMessage = "When the Zombies show up. just sit there and don't do anything. You win the game when the Zombies get to your houze. -this help section brought to you by the Zombies";

        }

        public static Game game = new Game();
        public class Game
        {
            public string completion = "[0]% complete";
            public string waveStatus = "Wave [0] of [1]";
            public string finalWave = "Final Wave";
            public string dirt = "dirt";
            public string grass = "grass";
            public string water = "water";
            public string roof = "roof";
            public string emptyTileString = "Empty [0] tile";
            public string crater = "Crater";
            public string graveStone = "Gravestone";
            public string plantInVase = "[0] in vase";
            public string zombieInVase = "[0] zombie in vase";
            public string vase = "Vase";
            public string plantVase = "Plant vase";
            public string zombieVase = "Zombie vase";

            public string ice = "Ice";
            public string starGuide = "Starfruit guide";
            public string roundPortal = "Round portal";
            public string squarePortal = "Square portal";

            public string squished = "Squished ";
            public string sleeping = "Sleeping ";
            public string buried = "Buried ";
            public string armed = "Armed ";
            public string chewing = "Chewing ";
            public string small = "Small ";
            public string magnetFilled = "Filled ";
            public string laddered = "Laddered ";
            public string cobCharging = "Charging ";
            public string cobReady = "Ready ";
            public string nutDamaged = "Damaged ";
            public string nutChipped = "Chipped ";
            public string tallnutCrying = "Crying ";
            public string garlicSad = "Sad ";
            public string garlicNibbled = "Nibbled ";
            public string pumpkinShield = " with [0]pumpkin shield";

            public string magBucket = " holding bucket";
            public string magHelmet = " holding football helmet";
            public string magDoor = " holding screen door";
            public string magPogo = " holding pogo stick";
            public string magJack = " holding Jack-in-the-box";
            public string magLadder = " holding ladder";
            public string magPickaxe = " holding pickaxe";

            public string hasRoundPortal = "[0] and Round portal";
            public string hasSquarePortal = "[0] and Square portal";

            public string bossHead = "Zomboss Head";
            public string offBoard = "Off-Board";

            public string hypnotized = "Hypnotized ";
            public string dinted = "Dinted ";
            public string damaged = "Damaged ";
            public string exposed = "Exposed ";
            public string ripped = "Ripped ";
            public string shredded = "Shredded ";
            public string angry = "Angry ";

            public string armless = "Armless ";
            public string headless = "Headless ";
            public string underground = "Underground ";
            public string grounded = "Grounded ";
            public string tired = "Tired ";
            public string falling = "Falling ";
            public string icy = "Icy ";
            public string buttered = "Buttered ";
            public string hungry = "Hungry ";

            public string iceBall = "Ice Ball.";
            public string fireBall = "Fire Ball.";

            public string notEnoughSun = "Not enough sun! [0] out of [1]";

            public string imitation = "Imitation [0]";
            public string plantReady = "Ready";
            public string plantRefreshing = "Refreshing";
            public string plantSun = "[0] of [1] sun";

            public string frozen = "Frozen!";
            public string unfrozen = "UnFrozen!";
            public string noLawnmower = "Unprotected!";
            public string lawnMower = "Lawn Mower";
            public string poolCleaner = "Pool Cleaner";
            public string roofSweeper = "Roof Sweeper";
            public string hasBrain = "Brain remaining!";
            public string noBrain = "No Brain!";

            public string noZombies = "No Zombies";

            public string buySnorkel = "Buy snorkel zombie.";
            public string buyTrophy = "Buy trophy.";
            public string buyBrain = "Brain food.";
            public string purchased = "Purchased. ";

            public string upgradePeashooters = " Upgrade peashooters to repeaters.";
            public string upgradeShrooms = " Upgrade puff-shrooms to fume-shrooms.";
            public string upgradeNuts = " Upgrade wall-nuts to tall-nuts.";
            public string shufflePlants = " Shuffle plants.";
            public string repairCrater = " Repair crater.";
            public string collectSun = "Collect sun to unlock options";

            public string refreshPercent = "[0]% refreshed";
            public string alreadyPurchased = "Already purchased.";
            public string waitingForPlants = "Waiting for plants to arrive";

            public string brainStatus = "[0] of 5 brains eaten";
            public string slotStatus = "[0] of 2,000 sun";
            public string beghouledStatus = "[0] of 75 matches.";
            public string starStatus = "[0] of 14 required stars";
            public string vasesRemaining = "[0] vases remaining";
            public string vaseRemaining = "1 vase remaining";
            public string noVases = "no vases remaining";
            public string survivalEndlessStage = "[0] stages completed";
            public string survivalStage = "[0] of 5 stages completed!";
            public string zombiquariumGoal = "Save up 1000 sun to purchase the level trophy";

            public string coinCount = "[0] coins!";
            public string sunCount = "[0] sun!";
        }

        public static Almanac almanac;
        public class Almanac
        {
            public string plants = "Plants.";
            public string zombies = "Zombies.";
            public string index = "Almanac Index.";

            public string fast = "Fast.";
            public string slow = "Slow.";
            public string verySlow = "Very Slow.";

            public string rechargeTime = "Recharge time: ";
            public string sun = " sun.";

            public string mysteryZombie = "Mystery Zombie. Not encountered yet.";
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

            //var deserializer = new YamlDotNet.Serialization.Deserializer();
            //var treedial = deserializer.Deserialize<Dictionary<int, string>>(File.ReadAllText("Language\\English\\TreeDialogue.yaml"));

            //foreach(var entry in TreeDialogue)
            //foreach(var entry in treedial)
            //  Console.WriteLine("{0}: {1}", entry.Key, entry.Value);

        }

    }
}
