using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memory;
using NAudio.Wave.SampleProviders;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    class Store : Widget
    {

        string inputDescription = "Inputs: Confirm to buy, Deny to close, Info1 to say coin balance, Horizontal directions and Cycle buttons to switch categories, Vertical directions to scroll items.\r\n";
        public Store (MemoryIO memIO, string pointerChain) : base(memIO, pointerChain)
        {
            ReloadStore();
        }

        int storePage = 0;
        int storeItemIndex = 0;
        bool inBuyConfirmationDialogue = false;
        StorePage[] storePages = new StorePage[3]
        {
            new StorePage(){pageName = Text.menus.gameUpgrades},
            new StorePage(){pageName = Text.menus.plantUpgrades},
            new StorePage(){pageName = Text.menus.zenGarden + "."}
        };

        struct StoreEntry
        {
            public string name;
            public string description;
            public int price;
            public bool outOfStock;
            public StoreItem itemType;
        }

        struct StorePage
        {
            public string pageName;
            public List<StoreEntry> entries;
        }

        public void ReloadStore(bool resetPosition = false)
        {
            var playerPurchases = memIO.GetPlayerPurchases();

            inBuyConfirmationDialogue = false; //Just to make sure

            if (resetPosition)
            {
                storePage = 0;
                storeItemIndex = 0;
            }

            bool finishedAdventure = memIO.GetAdventureCompletions() > 0;
            int level = memIO.GetPlayerLevel();

            bool roofCleanerAvailable = (finishedAdventure || level >= 42) && playerPurchases[(int)StoreItem.GameUpgradeRoofCleaner] == 0;
            bool gloomShroomAvailable = (finishedAdventure || level >= 35) && playerPurchases[(int)StoreItem.PlantGloomshroom] == 0;
            bool catTailAvailable = (finishedAdventure || level >= 35) && playerPurchases[(int)StoreItem.PlantCattail] == 0;
            bool spikeRockAvailable = (finishedAdventure || level >= 41) && playerPurchases[(int)StoreItem.PlantSpikerock] == 0;
            bool goldMagnetAvailable = (finishedAdventure || level >= 41) && playerPurchases[(int)StoreItem.PlantGoldMagnet] == 0;
            //wintermelon, cobCannon, Imitater, WallnutFirstaid are only available after adventure has been completed
            //Zen garden page 1 not available until level 44 is complete
            //Page 2 not until adventure is complete
            bool zenGarden1Available = finishedAdventure || level >= 45;


            //They seem to add 1000 to items with quantities?
            //Very strange...
            //I wonder if it was to deter cheating, by making the values slightly harder to find?
            //Not very effective if so. Baffling either way.

            //Can only buy wheelbarrow if player owns aquarium or mushroom garden
            //Also only if page2 is unlocked
            bool wheelbarrowAvailable = finishedAdventure && (playerPurchases[(int)StoreItem.ZenAquariumGarden] != 0 || playerPurchases[(int)StoreItem.ZenMushroomGarden] != 0) && playerPurchases[(int)StoreItem.ZenWheelBarrow] == 0;
            bool treeFoodAvailable = finishedAdventure && (playerPurchases[(int)StoreItem.ZenTreeOfWisdom] != 0);
            int daysSince2000 = (int)(DateTime.Now - new DateTime(2000, 1, 1)).TotalDays;    //Value game uses to determine which day you purchased a marigold.

            //TODO: Add check for zen-garden plant count. Don't allow purcahse if garden is full.
            bool marigold1Available = playerPurchases[(int)StoreItem.ZenMarigold1] != daysSince2000 && finishedAdventure;
            bool marigold2Available = playerPurchases[(int)StoreItem.ZenMarigold2] != daysSince2000 && finishedAdventure;
            bool marigold3Available = playerPurchases[(int)StoreItem.ZenMarigold3] != daysSince2000 && finishedAdventure;

            //Check if there's a free space in the ZenGarden for a marigold, othewise they're all out of stock
            ZenGarden tempGarden = new ZenGarden(memIO);
            var page1Plants = tempGarden.GetZenPlantsForPage(0);
            if(page1Plants.Count >= 32)
            {
                marigold1Available = false;
                marigold2Available = false;
                marigold3Available = false;
            }

            int seedSlotValue = playerPurchases[(int)StoreItem.GameUpgradeSeedSlot];
            int seedSlotCost = seedSlotValue == 0 ? 750 : seedSlotValue == 1 ? 5000 : seedSlotValue == 2 ? 20000 : 80000;

            bool rakeAvailable = playerPurchases[(int)StoreItem.GameUpgradeRake] == 0;  //Always 'available' just not purchasable if any remain

            List<StoreEntry> gameUpgrades = new List<StoreEntry>();

            if (seedSlotValue < 4)
                gameUpgrades.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.seedSlot,
                    description = Text.store.itemDescriptions.seedSlot.Replace("[0]", (7+seedSlotValue).ToString()),
                    price = seedSlotCost,
                    itemType = StoreItem.GameUpgradeSeedSlot
                });

            if (playerPurchases[(int)StoreItem.GameUpgradePoolCleaner] == 0)
                gameUpgrades.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.poolCleaners,
                    description = Text.store.itemDescriptions.poolCleaners,
                    price = 1000,
                    itemType = StoreItem.GameUpgradePoolCleaner
                });

            gameUpgrades.Add(new StoreEntry()
            {
                name = (rakeAvailable ? "" : Text.store.NoStock) + Text.store.itemNames.gardenRake,
                description = Text.store.itemDescriptions.gardenRake,
                price = 200,
                outOfStock = !rakeAvailable,
                itemType = StoreItem.GameUpgradeRake
            });

            if (roofCleanerAvailable)
                gameUpgrades.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.roofCleaners,
                    description = Text.store.itemDescriptions.roofCleaners,
                    price = 3000,
                    itemType = StoreItem.GameUpgradeRoofCleaner
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.GameUpgradeImitater] == 0)
                gameUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_IMITATER],
                    description = Text.store.itemDescriptions.imitater,
                    price = 30000,
                    itemType = StoreItem.GameUpgradeImitater
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.GameUpgradeFirstaid] == 0)
                gameUpgrades.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.wallnutAid,
                    description = Text.store.itemDescriptions.wallnutAid,
                    price = 2000,
                    itemType = StoreItem.GameUpgradeFirstaid
                });

            List<StoreEntry> plantUpgrades = new List<StoreEntry>();

            if (playerPurchases[(int)StoreItem.PlantGatlingpea] == 0)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_GATLINGPEA],
                    description = Text.store.itemDescriptions.gatlingPea,
                    price = 5000,
                    itemType = StoreItem.PlantGatlingpea
                });

            if (playerPurchases[(int)StoreItem.PlantTwinsunflower] == 0)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_TWINSUNFLOWER],
                    description = Text.store.itemDescriptions.twinSunflower,
                    price = 5000,
                    itemType = StoreItem.PlantTwinsunflower
                });

            if (gloomShroomAvailable)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_GLOOMSHROOM],
                    description = Text.store.itemDescriptions.gloomShroom,
                    price = 7500,
                    itemType = StoreItem.PlantGloomshroom
                });

            if (catTailAvailable)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_CATTAIL],
                    description = Text.store.itemDescriptions.catTail,
                    price = 10000,
                    itemType = StoreItem.PlantCattail
                });

            if (spikeRockAvailable)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_SPIKEROCK],
                    description = Text.store.itemDescriptions.spikeRock,
                    price = 7500,
                    itemType = StoreItem.PlantSpikerock
                });

            if (goldMagnetAvailable)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_GOLD_MAGNET],
                    description = Text.store.itemDescriptions.goldMagnet,
                    price = 3000,
                    itemType = StoreItem.PlantGoldMagnet
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.PlantWintermelon] == 0)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_WINTERMELON],
                    description = Text.store.itemDescriptions.winterMelon,
                    price = 10000,
                    itemType = StoreItem.PlantWintermelon
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.PlantCobcannon] == 0)
                plantUpgrades.Add(new StoreEntry()
                {
                    name = Text.plantNames[(int)SeedType.SEED_COBCANNON],
                    description = Text.store.itemDescriptions.cobCannon,
                    price = 20000,
                    itemType = StoreItem.PlantCobcannon
                });

            List<StoreEntry> zenGarden = new List<StoreEntry>();


            if (zenGarden1Available && playerPurchases[(int)StoreItem.ZenGoldWateringcan] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.goldenCan,
                    description = Text.store.itemDescriptions.goldenCan,
                    price = 10000,
                    itemType = StoreItem.ZenGoldWateringcan
                });

            bool fertilizerInStock = playerPurchases[(int)StoreItem.ZenFertilizer] - 1000 <= 15;
            bool bugSprayInStock = playerPurchases[(int)StoreItem.ZenBugSpray] - 1000 <= 15;
            bool treeFoodInStock = playerPurchases[(int)StoreItem.ZenTreeFood] - 1000 < 10;

            if (zenGarden1Available)
                zenGarden.Add(new StoreEntry()
                {
                    name = (fertilizerInStock ? "" : Text.store.NoStock) + Text.store.itemNames.fertilizer,
                    description = Text.store.itemDescriptions.fertilizer,
                    price = 750,
                    outOfStock = !fertilizerInStock,
                    itemType = StoreItem.ZenFertilizer
                });

            if (zenGarden1Available)
                zenGarden.Add(new StoreEntry()
                {
                    name = (bugSprayInStock ? "" : Text.store.NoStock) + Text.store.itemNames.bugSpray,
                    description = Text.store.itemDescriptions.bugSpray,
                    price = 1000,
                    outOfStock = !bugSprayInStock,
                    itemType = StoreItem.ZenBugSpray
                });

            if (zenGarden1Available && playerPurchases[(int)StoreItem.ZenPhonograph] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.phonograph,
                    description = Text.store.itemDescriptions.phonograph,
                    price = 15000,
                    itemType = StoreItem.ZenPhonograph
                });

            if (zenGarden1Available && playerPurchases[(int)StoreItem.ZenGardeningGlove] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.gardeningGlove,
                    description = Text.store.itemDescriptions.gardeningGlove,
                    price = 1000,
                    itemType = StoreItem.ZenGardeningGlove
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.ZenMushroomGarden] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.mushroomGarden,
                    description = Text.store.itemDescriptions.mushroomGarden,
                    price = 30000,
                    itemType = StoreItem.ZenMushroomGarden
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.ZenAquariumGarden] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.aquariumGarden,
                    description = Text.store.itemDescriptions.aquariumGarden,
                    price = 30000,
                    itemType = StoreItem.ZenAquariumGarden
                });

            if (wheelbarrowAvailable)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.wheelbarrow,
                    description = Text.store.itemDescriptions.wheelbarrow,
                    price = 200,
                    itemType = StoreItem.ZenWheelBarrow
                });


            if (finishedAdventure && playerPurchases[(int)StoreItem.ZenStinkyTheSnail] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.stinky,
                    description = Text.store.itemDescriptions.stinky,
                    price = 3000,
                    itemType = StoreItem.ZenStinkyTheSnail
                });

            if (finishedAdventure && playerPurchases[(int)StoreItem.ZenTreeOfWisdom] == 0)
                zenGarden.Add(new StoreEntry()
                {
                    name = Text.store.itemNames.treeOfWisdom,
                    description = Text.store.itemDescriptions.treeOfWisdom,
                    price = 10000,
                    itemType = StoreItem.ZenTreeOfWisdom
                });

            if (treeFoodAvailable)
                zenGarden.Add(new StoreEntry()
                {
                    name = (treeFoodInStock ? "" : Text.store.NoStock) + Text.store.itemNames.treeFood,
                    description = Text.store.itemDescriptions.treeFood,
                    price = 2500,
                    outOfStock = !treeFoodInStock,
                    itemType = StoreItem.ZenTreeFood
                });

            //Numbers added to the end of marigolds, to make it clear that they're moving between them
            if (finishedAdventure)
                zenGarden.Add(new StoreEntry()
                {
                    name = (marigold1Available ? "" : Text.store.NoStock) + Text.plantNames[(int)SeedType.SEED_MARIGOLD] + " 1",
                    description = Text.store.itemDescriptions.marigold,
                    price = 2500,
                    outOfStock = !marigold1Available,
                    itemType = StoreItem.ZenMarigold1
                });

            if (finishedAdventure)
                zenGarden.Add(new StoreEntry()
                {
                    name = (marigold2Available ? "" : Text.store.NoStock) + Text.plantNames[(int)SeedType.SEED_MARIGOLD] + " 2",
                    description = Text.store.itemDescriptions.marigold,
                    price = 2500,
                    outOfStock = !marigold2Available,
                    itemType = StoreItem.ZenMarigold2
                });

            if (finishedAdventure)
                zenGarden.Add(new StoreEntry()
                {
                    name = (marigold3Available ? "" : Text.store.NoStock) + Text.plantNames[(int)SeedType.SEED_MARIGOLD] + " 3",
                    description = Text.store.itemDescriptions.marigold,
                    price = 2500,
                    outOfStock = !marigold3Available,
                    itemType = StoreItem.ZenMarigold3
                });

            storePages[0].entries = gameUpgrades;
            storePages[1].entries = plantUpgrades;
            storePages[2].entries = zenGarden;

        }

        public override void Interact(InputIntent intent)
        {
            string text = "";

            //We don't use the real store. It's completely redesigned for blind-accessibility
            int playerCoinCount = memIO.GetPlayerCoinCount() * 10;
            bool nvdaOverwrite = true;

            if (inBuyConfirmationDialogue)
            {
                if (intent == InputIntent.Confirm)
                {
                    //buy
                    var itemType = storePages[storePage].entries[storeItemIndex].itemType;
                    bool isMarigold = itemType >= StoreItem.ZenMarigold1 && itemType <= StoreItem.ZenMarigold3;
                    bool isSlotUpgrade = itemType == StoreItem.GameUpgradeSeedSlot;

                    bool isZenConsumable = itemType == StoreItem.ZenFertilizer || itemType == StoreItem.ZenBugSpray || itemType == StoreItem.ZenTreeFood;
                    bool isTreeFood = itemType == StoreItem.ZenTreeFood;

                    bool isStinky = itemType == StoreItem.ZenStinkyTheSnail;
                    bool isRake = itemType == StoreItem.GameUpgradeRake;

                    //Write correct data to ownedItem entry
                    if (isSlotUpgrade)
                    {
                        int slotValue = memIO.GetPlayerPurchase((int)itemType);
                        slotValue++;
                        memIO.SetPlayerPurchase((int)itemType, slotValue);
                    }
                    else if (isMarigold)
                    {
                        int daysSince2000 = (int)(DateTime.Now - new DateTime(2000, 1, 1)).TotalDays;
                        memIO.SetPlayerPurchase((int)itemType, daysSince2000);

                        //Add marigold to zengarden
                        ZenGarden tempGarden = new ZenGarden(memIO);

                        tempGarden.TryAddPlant(SeedType.SEED_MARIGOLD);

                        //Find an empty space in first zen garden page
                        //Find empty space in zen garden plant array
                        //Insert new marigold with random attributes
                        //Honestly, it would be easier to just interact with the shop 'properly'
                        //Random color between VARIATION_MARIGOLD_WHITE (2) and VARIATION_MARIGOLD_LIGHT_GREEN (12)
                        //Null all entries of plant entry in array
                        //Set seedType to marigold (38)
                        //Set drawVariation to VARIATION_NORMAL
                        //Set lastWateredTime to 0
                        //Random flip facing right/left bool
                        //Set plantage to sprout
                        //TimesFed to 0
                        //GardenIndex 0
                        //FeedingsPerGrow random between 3 and 5
                        //Start with no need
                        //LastNeedFilled time 0
                        //Last Fertilized 0
                        //LastChoc 0
                    }
                    else if (isZenConsumable)
                    {
                        int quantity = memIO.GetPlayerPurchase((int)itemType);
                        if (quantity < 1000)
                            quantity = 1000;
                        quantity += isTreeFood ? 1 : 5;
                        memIO.SetPlayerPurchase((int)itemType, quantity);
                    }
                    else if (isStinky)
                    {
                        uint timestamp = (uint)Program.CurrentEpoch();
                        memIO.SetPlayerPurchase((int)itemType, (int)timestamp); //TODO: Don't cast to int. Maybe convert to byte array and write as array. Or add uint support to Memory lib
                    }
                    else if (isRake)
                        memIO.SetPlayerPurchase((int)itemType, 3);
                    else
                        memIO.SetPlayerPurchase((int)itemType, 1);  //Item is owned

                    //Subtract cost
                    int newCoinCount = playerCoinCount - storePages[storePage].entries[storeItemIndex].price;
                    newCoinCount /= 10; //Game stores coin count as multiples of 10 (eg; 10 coins is 1, 100 coins is 10)
                    memIO.SetPlayerCoinCount(newCoinCount);

                    inBuyConfirmationDialogue = false;


                    text = Text.store.PurchaseComplete;
                    Console.WriteLine(text);
                    Program.Say(text, true);
                    Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 800, 800, 200, SignalGeneratorType.Sin);

                    ReloadStore();
                    hasUpdatedContents = true;
                    return;
                }
                else if (intent == InputIntent.Deny)
                {
                    inBuyConfirmationDialogue = false;
                    text = Text.store.PurchaseCancelled;
                    Console.WriteLine(text);
                    Program.Say(text, true);
                    Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 333, 333, 100, SignalGeneratorType.Sin);
                    hasUpdatedContents = true;
                    return;
                }
                else
                    return;
            }
            else if (intent == InputIntent.Deny)
            {
                Program.Click(0.5f, 0.9f);
                return;
            }


            ReloadStore();

            int prevStorePage = storePage;
            int prevItemIndex = storeItemIndex;

            if (intent == InputIntent.Left || intent == InputIntent.CycleLeft)
                storePage--;

            if (intent == InputIntent.Right || intent == InputIntent.CycleRight)
                storePage++;

            if (storePage < 0)
                storePage = 2;
            if (storePage > 2)
                storePage = 0;

            if (intent == InputIntent.Down)
                storeItemIndex++;
            if (intent == InputIntent.Up)
                storeItemIndex--;

            bool shouldReadItem = storeItemIndex != prevItemIndex;
            shouldReadItem |= storePage != prevStorePage;



            if (prevStorePage != storePage)
            {
                storeItemIndex = 0;
                text += storePages[storePage].pageName + "\r\n";
            }

            int lastItemIndex = storePages[storePage].entries.Count - 1;

            if(Config.current.WrapCursorInMenus)
            {
                storeItemIndex = storeItemIndex < 0 ? lastItemIndex : storeItemIndex;
                storeItemIndex = storeItemIndex > lastItemIndex ? 0 : storeItemIndex;
            }
            else
            {
                storeItemIndex = storeItemIndex < 0 ? 0 : storeItemIndex;
                storeItemIndex = storeItemIndex > lastItemIndex ? lastItemIndex : storeItemIndex;
            }
            if (lastItemIndex == -1)
                storeItemIndex = -1;

            bool finishedAdventure = memIO.GetAdventureCompletions() > 0;
            int level = memIO.GetPlayerLevel();
            bool zenGarden1Available = finishedAdventure || level >= 45;

            bool nullInteraction = intent == InputIntent.Confirm && storeItemIndex == -1;

            float freq = 1250.0f - ((((float)storeItemIndex / (float)lastItemIndex) * 5000.0f) / 5.0f);

            if (shouldReadItem && storeItemIndex == prevItemIndex && prevStorePage == storePage)
                Program.PlayBoundaryTone();
            else if (shouldReadItem)
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Sin);

            if (shouldReadItem && storeItemIndex != -1)
            {
                text += storePages[storePage].entries[storeItemIndex].name + ".\r\n";
                text += Program.FormatNumber(storePages[storePage].entries[storeItemIndex].price) + " " + Text.store.CurrencyName + ".\r\n";
                text += storePages[storePage].entries[storeItemIndex].description;
            }
            else if ((shouldReadItem || nullInteraction) && storePage == 2 && !zenGarden1Available)
            {
                text += Text.store.UnlockMoreUpgrades;
                if (intent == InputIntent.Confirm)
                    Program.PlayBoundaryTone();
            }
            else if ((shouldReadItem || nullInteraction) && !finishedAdventure)
            {
                text += Text.store.UnlockMoreUpgrades;
                if (intent == InputIntent.Confirm)
                    Program.PlayBoundaryTone();
            }
            else if ((shouldReadItem || nullInteraction) && finishedAdventure && storeItemIndex == -1)
            {
                text += Text.store.AllUpgradesObtained;
                if (intent == InputIntent.Confirm)
                    Program.PlayBoundaryTone();
            }
            else if (storeItemIndex != -1)
            {
                if (intent == InputIntent.Confirm)
                {
                    if (storePages[storePage].entries[storeItemIndex].outOfStock)
                    {
                        text = Text.store.NoStockWarning;
                        Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 333, 333, 100, SignalGeneratorType.Sin);
                    }
                    else if (playerCoinCount < storePages[storePage].entries[storeItemIndex].price)
                    {
                        text = Text.store.NotEnoughCoins.Replace("[0]", Program.FormatNumber(storePages[storePage].entries[storeItemIndex].price));
                        text = text.Replace("[1]", Program.FormatNumber(playerCoinCount));
                    }
                    else
                    {
                        text = Text.store.PurchaseConfirmation.Replace("[0]", storePages[storePage].entries[storeItemIndex].name);
                        text = text.Replace("[1]", Program.FormatNumber(storePages[storePage].entries[storeItemIndex].price));
                        inBuyConfirmationDialogue = true;
                    }
                }
            }

            if (intent == InputIntent.Info1)
            {
                text = Text.store.CoinCount.Replace("[0]", Program.FormatNumber(playerCoinCount));
            }

            if (text.Length > 0)
            {
                Console.WriteLine(text);
                Program.Say(text, nvdaOverwrite);
            }
        }

        protected override string? GetContentUpdate()
        {
            bool finishedAdventure = memIO.GetAdventureCompletions() > 0;
            int level = memIO.GetPlayerLevel();
            bool zenGarden1Available = finishedAdventure || level >= 45;
            string text = storePages[storePage].pageName + "\r\n";
            int lastItemIndex = storePages[storePage].entries.Count - 1;
            if (storeItemIndex > -1 && storeItemIndex < storePages[storePage].entries.Count)
            {
                text += storePages[storePage].entries[storeItemIndex].name + ".\r\n";
                text += storePages[storePage].entries[storeItemIndex].price + " " + Text.store.CurrencyName + ".\r\n";
                text += storePages[storePage].entries[storeItemIndex].description;
            }
            else if (storePage == 2 && !zenGarden1Available)
                text += Text.store.ZenGardenLocked;
            else if (!finishedAdventure)
                text += Text.store.UnlockMoreUpgrades;
            else if (finishedAdventure && storeItemIndex == -1)
                text += Text.store.AllUpgradesObtained;

            return text;
        }

        protected override string? GetContent()
        {
            return Text.menus.store + "\r\n" + (Config.current.SayAvailableInputs ? inputDescription : "") + GetContentUpdate();
        }

    }
}
