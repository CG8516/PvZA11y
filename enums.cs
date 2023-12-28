using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y
{
    enum StoreItem
    {
        PlantGatlingpea,
        PlantTwinsunflower,
        PlantGloomshroom,
        PlantCattail,
        PlantWintermelon,
        PlantGoldMagnet,
        PlantSpikerock,
        PlantCobcannon,
        GameUpgradeImitater,
        Unused_LawnMower,
        ZenMarigold1,
        ZenMarigold2,
        ZenMarigold3,
        ZenGoldWateringcan,
        ZenFertilizer,
        ZenBugSpray,
        ZenPhonograph,
        ZenGardeningGlove,
        ZenMushroomGarden,
        ZenWheelBarrow,
        ZenStinkyTheSnail,
        GameUpgradeSeedSlot,
        GameUpgradePoolCleaner,
        GameUpgradeRoofCleaner,
        GameUpgradeRake,
        ZenAquariumGarden,
        Chocolate,
        ZenTreeOfWisdom,
        ZenTreeFood,
        GameUpgradeFirstaid,
    };

    enum SeedType
    {
        SEED_PEASHOOTER = 0,
        SEED_SUNFLOWER = 1,
        SEED_CHERRYBOMB = 2,
        SEED_WALLNUT = 3,
        SEED_POTATOMINE = 4,
        SEED_SNOWPEA = 5,
        SEED_CHOMPER = 6,
        SEED_REPEATER = 7,
        SEED_PUFFSHROOM = 8,
        SEED_SUNSHROOM = 9,
        SEED_FUMESHROOM = 10,
        SEED_GRAVEBUSTER = 11,
        SEED_HYPNOSHROOM = 12,
        SEED_SCAREDYSHROOM = 13,
        SEED_ICESHROOM = 14,
        SEED_DOOMSHROOM = 15,
        SEED_LILYPAD = 16,
        SEED_SQUASH = 17,
        SEED_THREEPEATER = 18,
        SEED_TANGLEKELP = 19,
        SEED_JALAPENO = 20,
        SEED_SPIKEWEED = 21,
        SEED_TORCHWOOD = 22,
        SEED_TALLNUT = 23,
        SEED_SEASHROOM = 24,
        SEED_PLANTERN = 25,
        SEED_CACTUS = 26,
        SEED_BLOVER = 27,
        SEED_SPLITPEA = 28,
        SEED_STARFRUIT = 29,
        SEED_PUMPKINSHELL = 30,
        SEED_MAGNETSHROOM = 31,
        SEED_CABBAGEPULT = 32,
        SEED_FLOWERPOT = 33,
        SEED_KERNELPULT = 34,
        SEED_INSTANT_COFFEE = 35,
        SEED_GARLIC = 36,
        SEED_UMBRELLA = 37,
        SEED_MARIGOLD = 38,
        SEED_MELONPULT = 39,
        SEED_GATLINGPEA = 40,
        SEED_TWINSUNFLOWER = 41,
        SEED_GLOOMSHROOM = 42,
        SEED_CATTAIL = 43,
        SEED_WINTERMELON = 44,
        SEED_GOLD_MAGNET = 45,
        SEED_SPIKEROCK = 46,
        SEED_COBCANNON = 47,
        SEED_IMITATER = 48,
        SEED_EXPLODE_O_NUT,
        SEED_GIANT_WALLNUT,
        SEED_SPROUT,
        SEED_LEFTPEATER,
        NUM_SEED_TYPES,
        SEED_BEGHOULED_BUTTON_SHUFFLE,
        SEED_BEGHOULED_BUTTON_CRATER,
        SEED_SLOT_MACHINE_SUN,
        SEED_SLOT_MACHINE_DIAMOND,
        SEED_ZOMBIQUARIUM_SNORKLE,
        SEED_ZOMBIQUARIUM_TROPHY,
        SEED_ZOMBIE_NORMAL,
        SEED_ZOMBIE_TRAFFIC_CONE,
        SEED_ZOMBIE_POLEVAULTER,
        SEED_ZOMBIE_PAIL,
        SEED_ZOMBIE_LADDER,
        SEED_ZOMBIE_DIGGER,
        SEED_ZOMBIE_BUNGEE,
        SEED_ZOMBIE_FOOTBALL,
        SEED_ZOMBIE_BALLOON,
        SEED_ZOMBIE_SCREEN_DOOR,
        SEED_ZOMBONI,
        SEED_ZOMBIE_POGO,
        SEED_ZOMBIE_DANCER,
        SEED_ZOMBIE_GARGANTUAR,
        SEED_ZOMBIE_IMP,
        MaxCount,
        NUM_SEEDS_IN_CHOOSER = 49,
        SEED_NONE = -1,
    };

    public static class Consts
    {
        public static int[] plantCosts = new int[] { 100, 50, 150, 50, 25, 175, 150, 200, 0, 25, 75, 75, 75, 25, 75, 125, 25, 50, 325, 25, 125, 100, 175, 125, 0, 25, 125, 100, 125, 125, 125, 100, 100, 25, 100, 75, 50, 100, 50, 300, 250, 150, 150, 225, 200, 50, 125, 500, 0, 0, 0, 0, 200 };
        public static int[] plantCooldowns = new int[] { 750, 750, 5000, 3000, 3000, 750, 750, 750, 750, 750, 750, 750, 3000, 750, 5000, 5000, 750, 3000, 750, 3000, 5000, 750, 750, 3000, 3000, 3000, 750, 750, 750, 750, 3000, 750, 750, 750, 750, 750, 750, 750, 3000, 750, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 750, 3000, 3000, 3000, 750 };

        

        //For iZombie gamemodes
        public static int[] iZombieNameIndex = new int[] { 0,2,3,4,21,17,20,7,16,6,12,18,8,23,24 };
        public static int[] iZombieSunCosts = new int[] { 50, 75, 75, 125, 150, 125, 125, 175, 150, 100, 175, 200, 350, 300, 50 };

        public static bool[] SeeingStars = new bool[45]
        {
            false,  false,  false,  true,   false,  false,  false,  false,  false,
            false,  false,  false,  true,   true,   false,  false,  false,  false,
            false,  true,   true,   true,   true,   true,   true,   false,  false,
            false,  false,  false,  true,   true,   true,   false,  false,  false,
            false,  false,  false,  true,   false,  false,  true,   false,  false
        };

        
    }

    enum GameMode
    {
        Adventure,
        SurvivalDay,
        SurvivalNight,
        SurvivalPool,
        SurvivalFog,
        SurvivalRoof,
        SurvivalHardDay,
        SurvivalHardNight,
        SurvivalHardPool,
        SurvivalHardFog,
        SurvivalHardRoof,
        SurvivalEndless1,
        SurvivalEndless2,
        SurvivalEndless3,
        SurvivalEndless4,
        SurvivalEndless5,
        ZomBotany,
        WallnutBowling,
        SlotMachine,
        ItsRainingSeeds,
        Beghouled,
        Invisighoul,
        SeeingStars,
        Zombiquarium,
        BeghouledTwist,
        BigTroubleLittleZombie,
        PortalCombat,
        ColumnLikeYouSeeEm,
        BobsledBonanza,
        ZombieNimbleZombieQuick,
        WhackAZombie,
        LastStand,
        ZomBotany2,
        WallnutBowling2,
        PogoParty,
        DrZombossRevenge,
        LimboArtChallengeWallnut,
        LimboSunnyDay,
        LimboUnsodded,
        LimboBigTime,
        LimboArtChallengeSunflower,
        LimboAirRaid,
        LimboIceLevel,
        ZenGarden,
        LimboHighGravity,
        LimboGraveDanger,
        LimboCanYouDigIt,
        LimboDarkStormyNight,
        LimboBungeeBlitz,
        LimboIntro,
        TreeOfWisdom,
        VaseBreaker1,
        VaseBreaker2,
        VaseBreaker3,
        VaseBreaker4,
        VaseBreaker5,
        VaseBreaker6,
        VaseBreaker7,
        VaseBreaker8,
        VaseBreaker9,
        VaseBreakerEndless,
        IZombie1,
        IZombie2,
        IZombie3,
        IZombie4,
        IZombie5,
        IZombie6,
        IZombie7,
        IZombie8,
        IZombie9,
        IZombieEndless,
        UpsellTest,
        Intro,
        NumGameModes
    };

    enum ChosenSeedState
    {
        FlyingToBank = 0,
        InBank = 1,
        FlyingToChooser = 2,
        InChooser = 3,
        Hidden = 4
    };

    enum ZombieType
    {
        Invalid = -1,
        Normal,
        Flag,
        ConeHead,
        PoleVaulting,
        BucketHead,
        Newspaper,
        ScreenDoor,
        Football,
        Dancing,
        BackupDancer,
        DuckyTube,
        Snorkel,
        Zomboni,
        Bobsled,
        DolphinRider,
        JackInTheBox,
        Balloon,
        Digger,
        Pogo,
        Yeti,
        Bungee,
        Ladder,
        Catapult,
        Gargantuar,
        Imp,
        DrZomBoss,
        PeaHead,
        WallnutHead,
        JalapenoHead,
        GatlingHead,
        SquashHead,
        TallnutHead,
        RedeyeGargantuar,
        NumZombieTypes,
        CachedPolevaulterWithPole,
        CachedZombieTypes
    };

    enum CoinType
    {
        None,
        Silver,
        Gold,
        Diamond,
        Sun,
        Smallsun,
        Largesun,
        FinalSeedPacket,
        Trophy,
        Shovel,
        Almanac,
        Carkeys,
        Vase,
        WateringCan,
        Taco,
        Note,
        UsableSeedPacket,
        PresentPlant,
        AwardMoneyBag,
        AwardPresent,
        AwardBagDiamond,
        AwardSilverSunflower,
        AwardGoldSunflower,
        Chocolate,
        AwardChocolate,
        PresentMinigames,
        PresentPuzzleMode,
        PresentSurvivalMode
    };

    enum GridItemType
    {
        None = 0,
        Gravestone = 1,
        Crater = 2,
        Ladder = 3,
        PortalCircle = 4,
        PortalSquare = 5,
        Brain = 6,
        Vase = 7,
        Squirrel = 8,
        ZenTool = 9,
        Stinky = 10,
        Rake = 11,
        IzombieBrain = 12
    }

    enum CursorType
    {
        Normal,
        PlantFromBank,
        PlantFromUsableCoin,
        PlantFromGlove,
        PlantFromDuplicator,
        PlantFromWheelBarrow,
        Shovel,
        Hammer,
        CobcannonTarget,
        WateringCan,
        Fertilizer,
        BugSpray,
        Phonograph,
        Chocolate,
        Glove,
        MoneySign,
        Wheeelbarrow,
        TreeFood
    }

    enum LevelType
    {
        Normal,
        Night,
        Pool,
        PoolNight,
        Roof,
        Boss //Probably more
    }

    enum ZenPlantNeed
    {
        None = 0,
        Water = 1,
        Fertilizer = 2,
        BugSpray = 3,
        Phonograph = 4
    }


    enum InputMode
    {
        Unknown,
        MainMenu,
        MinigameMenu,
        AwardScreen,
        SeedPicker,
        OnBoard
    }

    enum GameScene
    {
        Loading,
        MainMenu,
        SeedPicker,
        Board,
        BrainEaten,
        AwardScreen,
        Credits,
        MinigameSelector
    }

    enum MowerType
    {
        LawnMower,
        PoolCleaner,
        RoofSweeper,
        TrickedOut
    }
    
    public static class DialogIDs
    {
        public const int NewGame = 0;
        public const int Options = 1;
        public const int NewOptions = 2;
        public const int Almanac = 3;
        public const int Store = 4;
        public const int PregameNag = 5;
        public const int LoadGame = 6;
        public const int ConfirmUpdateCheck = 7;
        public const int CheckingUpdates = 8;
        public const int RegisterError = 9;
        public const int ColorDepthExp = 10;
        public const int OpenUrlWait = 11;
        public const int OpenUrlFail = 12;
        public const int Quit = 13;
        public const int HighScores = 14;
        public const int Nag = 15;
        public const int Info = 16;
        public const int GameOver = 17;
        public const int LevelComplete = 18;
        public const int Paused = 19;
        public const int NoMoreMoney = 20;
        public const int Bonus = 21;
        public const int ConfirmBackToMain = 22;
        public const int ConfirmRestart = 23;
        public const int ThanksForRegistering = 24;
        public const int NotEnoughMoney = 25;
        public const int Upgraded = 26;
        public const int NoUpgrade = 27;
        public const int ChooserWarning = 28;
        public const int UserDialog = 29;
        public const int CreateUser = 30;
        public const int ConfirmDeleteUser = 31;
        public const int RenameUser = 32;
        public const int CreateUserError = 33;
        public const int RenameUserError = 34;
        public const int Cheat = 35;
        public const int CheatError = 36;
        public const int Continue = 37;
        public const int GetReady = 38;
        public const int RestartConfirm = 39;
        public const int ConfirmPurchase = 40;
        public const int ConfirmSell = 41;
        public const int TimesUp = 42;
        public const int VirtualHelp = 43;
        public const int JumpAhead = 44;
        public const int CrazyDave = 45;
        public const int StorePurchase = 46;
        public const int ZenSell = 47;
        public const int CreditsPaused = 48;
        public const int Imitater = 49;
        public const int PurchasePacketSlot = 50;

        public const int unkDialog1 = 51; 
        public const int ZombatarLicense = 52;
        public const int unkDialog2 = 53;
        public const int unkDialog3 = 54;
        public const int unkDialog4 = 55;
        public const int steamCloudSavingActive = 56;
        public const int stamCloudLocalChoice = 57;
        //How many more are there?
    }

    

}
