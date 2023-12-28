using Memory;
using NAudio.Wave.SampleProviders;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PvZA11y.Widgets
{
    class ZenGarden : Widget
    {
        string inputDescription = "\r\nInputs: Directions to move around garden, Confirm to use tool, Deny or Start to leave, Info1 to say plant on current tile, Info2 to say plant need on current tile, Info3 to say number of needy plants, CycleLeft/CycleRight to change tools, Option to visit the store.";

        struct ZenTool
        {
            public float posX;
            public string name;
            public bool isChoc;
        }

        public struct ZenPlant
        {
            public int id;
            public int posX;
            public int posY;

            public ZenPlantNeed need;

            public long lastWateredTime;
            public long lastCaredTime;
            public long lastFertilizedTime;

            public int waterCount;
            public int waterTarget;
            public int age;

            public int index;  //Index of plant within zengarden plant array

            public bool mushroomInMain; //Whether this plant is a nocturnal plant in the main garden
            public bool aquaticInMain; //Whether this plant is a water plant in the main garden
        }

        //Plot positions for plants in zen mushroom garden
        static readonly Vector2[] ZenPosMG = new Vector2[]
        {
            new Vector2(0.18f,0.8f),
            new Vector2(0.34f,0.67f),
            new Vector2(0.41f,0.84f),
            new Vector2(0.49f,0.57f),
            new Vector2(0.52f,0.42f),
            new Vector2(0.62f,0.72f),
            new Vector2(0.65f,0.87f),
            new Vector2(0.73f,0.55f),
        };

        //Plot positions for plants in zen aquarium garden
        static readonly Vector2[] ZenPosAQ = new Vector2[]
        {
            new Vector2(0.18f,0.38f),
            new Vector2(0.42f,0.22f),
            new Vector2(0.49f,0.43f),
            new Vector2(0.7f,0.25f),
            new Vector2(0.88f,0.52f),
            new Vector2(0.2f,0.67f),
            new Vector2(0.5f,0.84f),
            new Vector2(0.67f,0.77f),
        };

        GridInput? gridInput;
        int currentPage;    //0:main, 1:mushroom, 2: wheelbarrow (for plant currently in wheelbarrow), 3:aquarium. not set for tree of wisdom.
        int toolIndex;
        List<ZenTool> ZenTools = new List<ZenTool>();
        const float ZenPageNext_X = 0.74f;

        public ZenGarden(MemoryIO memIO) : base(memIO, "")
        {
            UpdateGridBounds();
            //UpdateZenTools();
        }

        void UpdateGridBounds()
        {
            //Check page
            int page = memIO.GetZenGardenPage();

            if (page == currentPage && gridInput != null)
                return;

            switch(page)
            {
                case 0:
                    gridInput = new GridInput(8, 4, false);
                    break;
                case 1:
                case 3:
                    gridInput = new GridInput(4, 2, false);
                    break;
                default:
                    gridInput = null;
                    break;
            }

            currentPage = page;

        }

        void UpdateZenTools()
        {
            ZenTools = new List<ZenTool>();

            float xPos = -0.04f;
            float xInc = 0.09f;
            float yPos = 0.04f;

            //If on tree page
            if (currentPage == 4)
            {
                int treeFood = memIO.GetPlayerPurchase((int)StoreItem.ZenTreeFood);
                treeFood -= 1000;
                ZenTools.Add(new ZenTool() { posX = 0.08f, name = "Tree Food: " + treeFood });
                ZenTools.Add(new ZenTool() { posX = ZenPageNext_X, name = "Next Garden" });
                return;
            }

            bool goldWateringCan = memIO.GetPlayerPurchase((int)StoreItem.ZenGoldWateringcan) > 0;

            ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = goldWateringCan ? "Golden Watering Can" : "Watering Can" });

            int fertilizerCount = memIO.GetPlayerPurchase((int)StoreItem.ZenFertilizer);
            fertilizerCount -= 1000;
            if (fertilizerCount >= 0)
                ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Fertilizer: " + fertilizerCount.ToString("N0") });

            int bugSprayCount = memIO.GetPlayerPurchase((int)StoreItem.ZenBugSpray);
            bugSprayCount -= 1000;
            if (bugSprayCount >= 0)
                ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Bug Spray: " + bugSprayCount.ToString("N0") });

            bool hasPhonograph = memIO.GetPlayerPurchase((int)StoreItem.ZenPhonograph) > 0;
            if (hasPhonograph)
                ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Phonograph" });

            int chocolateCount = memIO.GetPlayerPurchase((int)StoreItem.Chocolate);
            chocolateCount -= 1000;
            chocolateCount = chocolateCount < 0 ? 0 : chocolateCount;
            if (chocolateCount >= 0)
                ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Chocolate: " + chocolateCount.ToString("N0"), isChoc = true });

            bool hasGlove = memIO.GetPlayerPurchase((int)StoreItem.ZenGardeningGlove) > 0;
            if (hasGlove)
                ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Glove" });

            bool canSell = memIO.GetAdventureCompletions() > 0;    //Finished adventure
            if (canSell)
                ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Sell" });

            bool hasWheelbarrow = memIO.GetPlayerPurchase((int)StoreItem.ZenWheelBarrow) > 0;
            if (hasWheelbarrow)
            {
                //Find plant in wheelbarrow (if any)
                int wheelbarrowPlantID = -1;

                var plants = GetZenPlantsForPage(2);
                if (plants.Count > 0)
                    wheelbarrowPlantID = plants[0].id;

                if (wheelbarrowPlantID == -1)
                    ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Wheel Barrow" });
                else
                    ZenTools.Add(new ZenTool() { posX = xPos += xInc, name = "Wheel Barrow with " + Text.plantNames[wheelbarrowPlantID] });
            }

            bool hasTree = memIO.GetPlayerPurchase((int)StoreItem.ZenTreeOfWisdom) > 0;
            bool hasMushroomGarden = memIO.GetPlayerPurchase((int)StoreItem.ZenMushroomGarden) > 0;
            bool hasAquarium = memIO.GetPlayerPurchase((int)StoreItem.ZenAquariumGarden) > 0;

            if (hasTree || hasMushroomGarden || hasAquarium)
                ZenTools.Add(new ZenTool() { posX = ZenPageNext_X, name = "Next Garden" });

        }

        public void TryAddPlant(SeedType plantType)
        {
            //Find empty space in main garden
            var plants = GetZenPlantsForPage(0);
            if (plants.Count >= 32)
                return;
            bool[,] takenSlots = new bool[4,8];

            //TODO: Should probably add a bound check? Though should only be an issue if a plant position was hacked to be beyond the bounds
            foreach(var plant in plants )
                takenSlots[plant.posY, plant.posX] = true;

            int freeY = -1;
            int freeX = -1;
            for(int y = 0; y < 4; y++)
            {
                for(int x = 0; x < 8; x++)
                {
                    if (!takenSlots[y,x])
                    {
                        freeY = y;
                        freeX = x;
                        break;
                    }
                }

                if (freeX != -1)
                    break;
            }

            //If we couldn't find an empty slot, don't add a new plant
            if (freeX == -1)
                return;

            //Game adds plant to next index after numPlants, so that's kinda nice for us. (they shift all plant entries back down to fill the gap, when a plant is sold)
            //MAX_POTTED_PLANTS == 200
            uint nextSlot = (uint)memIO.GetPlayerPlantCount();
            if (nextSlot >= 200)
                return;

            uint slotAddr = memIO.ptr.zenPlantStartOffset + (nextSlot * 88);  //TODO: Move zenPlant struct size to pointers.cs
            //Null plant struct
            memIO.mem.WriteBytes(memIO.ptr.playerInfoChain + "," + slotAddr.ToString("X2"), new byte[88]);

            //I'm seriously questioning whether this is all worth it
            //It's going to be so much easier to interact with the store using clicks. wtf am I doing right now?

            memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + "," + (slotAddr + 4).ToString("X2"), "int", ((int)plantType).ToString());  //Write Plant ID
            memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + "," + (slotAddr + 12).ToString("X2"), "int", freeX.ToString());  //Write posX
            memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + "," + (slotAddr + 16).ToString("X2"), "int", freeY.ToString());  //Write posY
            Random rand = new Random();
            int facingLeft = rand.Next(0, 2);   //0 or 1, stupid exclusive upper bound syntax makes it seem like 2 is a possible result
            memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + "," + (slotAddr + 20).ToString("X2"), "int", facingLeft.ToString());  //Write facing direction

            if (plantType is SeedType.SEED_MARIGOLD)
            {
                int variation = rand.Next(2, 13);   //Pick random marigold colour
                memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + "," + (slotAddr + 36).ToString("X2"), "int", variation.ToString());  //Write colour
            }

            int waterRequirement = rand.Next(3, 6); //Random amount between 3 and 5 (inclusive)
            memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + "," + (slotAddr + 48).ToString("X2"), "int", waterRequirement.ToString());  //Write water requirement

            memIO.mem.WriteMemory(memIO.ptr.playerInfoChain + memIO.ptr.zenPlantCountOffset, "int", (nextSlot+1).ToString());  //Update zengarden plant count
        }

        public List<ZenPlant> GetZenPlantsForPage(int page)
        {
            //TODO: Move pointer offsets to pointers.cs
            int numPlants = memIO.GetPlayerPlantCount();
            int maxPlants = 200;    //Total array size the game allocates for zen garden
            byte[] plantBytes = memIO.mem.ReadBytes(memIO.ptr.lawnAppPtr + ",94c," + memIO.ptr.zenPlantStartOffset.ToString("X2"), maxPlants * 88); //TODO: Move pointer offsets, and plant struct size to pointers.cs. mem read operation to memIO

            List<ZenPlant> plants = new List<ZenPlant>();
            for (int i = 0; i < numPlants; i++)
            {
                int index = i * 88;
                int plantIndex = index + 4;
                int gardenIndex = index + 8;
                int posXIndex = index + 12;
                int posYIndex = index + 16;
                int plantID = BitConverter.ToInt32(plantBytes, plantIndex);
                int gardenType = BitConverter.ToInt32(plantBytes, gardenIndex);
                int posX = BitConverter.ToInt32(plantBytes, posXIndex);
                int posY = BitConverter.ToInt32(plantBytes, posYIndex);
                if (gardenType == page)
                {
                    ZenPlant plant = new ZenPlant();
                    plant.posX = posX;
                    plant.posY = posY;
                    plant.id = plantID;
                    int need = BitConverter.ToInt32(plantBytes, index + 0x34);
                    plant.waterCount = BitConverter.ToInt32(plantBytes, index + 0x2c);
                    plant.waterTarget = BitConverter.ToInt32(plantBytes, index + 0x30);
                    plant.lastWateredTime = BitConverter.ToInt64(plantBytes, index + 0x1c);

                    plant.age = BitConverter.ToInt32(plantBytes, index + 0x28);
                    plant.index = i;
                    plant.lastCaredTime = BitConverter.ToInt64(plantBytes, index + 0x3c);
                    plant.lastFertilizedTime = BitConverter.ToInt64(plantBytes, index + 0x44);

                    if (need >= 0 && need <= 4)
                        plant.need = (ZenPlantNeed)need;

                    bool plantNeedsAttention = plant.age < 3 && Program.CurrentEpoch() - plant.lastFertilizedTime > 3600;    //Non-adult plants need attention every hour

                    //Adult plants need attention every day
                    DateTime plantTime = DateTimeOffset.FromUnixTimeSeconds(plant.lastCaredTime).LocalDateTime;
                    plantNeedsAttention |= plant.age >= 3 && (plantTime.DayOfYear != DateTime.Now.DayOfYear || plantTime.Year != DateTime.Now.Year);  //If plant hasn't been taken care of today, it needs attention

                    //After final fertilizer, leave satisfied for an hour
                    if (Program.CurrentEpoch() - plant.lastFertilizedTime < 3600)
                        plantNeedsAttention = false;

                    //Plants take up to 15 seconds to need attention (current time is offset by random value from 0 to 8 when plant is watered)
                    if (plantNeedsAttention && Program.CurrentEpoch() - plant.lastWateredTime > 15)
                    {
                        //Aquatic plants don't need water :P
                        bool isAquatic = plant.id == (int)SeedType.SEED_TANGLEKELP || plant.id == (int)SeedType.SEED_LILYPAD || plant.id == (int)SeedType.SEED_SEASHROOM || plant.id == (int)SeedType.SEED_CATTAIL;
                        if (plant.waterCount < plant.waterTarget && !isAquatic)
                            plant.need = ZenPlantNeed.Water;
                        else if (plant.age < 3)
                            plant.need = ZenPlantNeed.Fertilizer;
                    }
                    else
                        plant.need = ZenPlantNeed.None;

                    if(gardenType == 0 && Program.IsNocturnal((SeedType)plant.id))
                        plant.mushroomInMain = true;

                    if (gardenType == 0 && Program.IsAquatic((SeedType)plant.id))
                        plant.aquaticInMain = true;

                    plants.Add(plant);
                }
            }

            return plants;
        }

        ZenPlant? FindPlantAtZenTile(int x, int y)
        {
            List<ZenPlant> plants = GetZenPlantsForPage(currentPage);

            //NightGarden/Aquarium use just one row. We use two, for slightly nicer navigation (2*4 is quicker to navigate than 1*8)
            if(currentPage > 0)
            {
                x = y * 4 + x;
                y = 0;
            }

            foreach(ZenPlant plant in plants)
            {
                if (plant.posX == x && plant.posY == y)
                    return plant;
            }

            return null;
        }

        int GetNeedyPlantCount()
        {
            List<ZenPlant> plants = GetZenPlantsForPage(currentPage);
            int needyPlantCount = 0;
            foreach(ZenPlant plant in plants)
            {
                if (plant.need != ZenPlantNeed.None || plant.mushroomInMain || plant.aquaticInMain)
                    needyPlantCount++;
            }

            return needyPlantCount;
        }

        void RefreshState()
        {
            int prevPage = currentPage;
            UpdateGridBounds();
            UpdateZenTools();

            //If 'next garden' button was selected on tree of life (tool index 1 on ToL), keep tool on that position for main garden (last tool index, probably >1 for main garden)
            if (prevPage == 4 && currentPage != 4)
                toolIndex = ZenTools.Count - 1;
            //Set tool index to next garden button. Otherwise it won't update until we decrease the index with Lb, causing there to appear to be two 'Next Garden' entries
            if (prevPage != 4 && currentPage == 4)
                toolIndex = 1;
        }

        Vector2 GetGardenCellPosition()
        {

            if (currentPage == 0)
            {
                float cellX = 0.09f + (gridInput.cursorX * 0.107f);
                float cellY = 0.22f + (gridInput.cursorY * 0.164f);
                return new Vector2(cellX, cellY);
            }
            if(currentPage == 1)
                return ZenPosMG[gridInput.cursorY * 4 + gridInput.cursorX];
            if (currentPage == 3)
                return ZenPosAQ[gridInput.cursorY * 4 + gridInput.cursorX];
            else
                return new Vector2(0.5f, 0.5f);

        }

        string GetNeedString(ZenPlant? plant)
        {
            string needInfo = "";
            if (plant is not null)
            {
                needInfo = plant.Value.need.ToString();
                if (plant.Value.need == 0)
                    needInfo = "Happy";
                else
                    needInfo = needInfo + " needed";

                if (plant.Value.mushroomInMain)
                    needInfo = "Nocturnal. Needs to be moved to mushroom garden";

                if (plant.Value.aquaticInMain)
                    needInfo = "Aquatic. Needs to be moved to aquarium garden";
            }
            return needInfo;
        }

        void ClickStinky()
        {
            //If not holding a plant in a glove, Click tool/button
            if (Program.GetCursorType() == (int)CursorType.PlantFromGlove)
                return;

            bool hasStinky = memIO.GetPlayerPurchase(StoreItem.ZenStinkyTheSnail) > 0;
            if (!hasStinky)
                return;

            //Check if holding chocolate
            if (ZenTools[toolIndex].isChoc)
                Program.Click(ZenTools[toolIndex].posX, 0.05f, false, false, 50, true);

            var gridItems = Program.GetGridItems();
            foreach (var gridItem in gridItems)
            {
                if (gridItem.type == (int)GridItemType.Stinky)
                {
                    Program.Click(gridItem.floatX / 800.0f, gridItem.floatY / 600.0f);
                    break;
                }
            }
            
        }

        public override void Interact(InputIntent intent)
        {
            RefreshState();


            //Click store button
            if (intent == InputIntent.Option)
                Program.Click(0.95f, 0.1f);

            if (intent == InputIntent.CycleLeft)
                toolIndex--;
            if (intent == InputIntent.CycleRight)
                toolIndex++;

            if (intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10)
                toolIndex = intent - InputIntent.Slot1;

            bool shouldReadTool = (intent == InputIntent.CycleLeft || intent == InputIntent.CycleRight || (intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10));

            if (Config.current.WrapPlantSelection)
            {
                toolIndex = toolIndex < 0 ? ZenTools.Count -1 : toolIndex;
                toolIndex = toolIndex >= ZenTools.Count ? 0 : toolIndex;
            }
            else
            {
                toolIndex = toolIndex < 0 ? 0 : toolIndex;
                toolIndex = toolIndex >= ZenTools.Count ? ZenTools.Count - 1 : toolIndex;
            }


            if(shouldReadTool)
            {
                Program.PlaySlotTone(toolIndex, ZenTools.Count);

                Console.WriteLine(ZenTools[toolIndex].name);
                Program.Say(ZenTools[toolIndex].name, true);

                //Move cursor to selected tool, for sighted players
                Program.MoveMouse(ZenTools[toolIndex].posX, 0.05f);
            }

            ZenPlant? currentPlant = null;

            if (gridInput != null)
            {
                int prevX = gridInput.cursorX;
                int prevY = gridInput.cursorY;

                gridInput.Interact(intent); //grid cursor movement (up/down/left/right)

                currentPlant = FindPlantAtZenTile(gridInput.cursorX, gridInput.cursorY);

                if (prevX != gridInput.cursorX || prevY != gridInput.cursorY)
                {

                    string tileInfo = "";
                    if(currentPlant != null)
                    {
                        if (Config.current.SayPlantOnTileMove)
                        {
                            if(currentPlant?.need != ZenPlantNeed.None)
                                tileInfo = GetNeedString(currentPlant);
                            if (tileInfo.Length > 0)
                                tileInfo += ", ";
                            tileInfo += Text.plantNames[currentPlant.Value.id];
                        }
                    }

                    if(Config.current.SayTilePosOnMove)
                        tileInfo += (tileInfo.Length > 0 ? ", " : "") + string.Format("{0}-{1}", (char)(gridInput.cursorX + 'A'), gridInput.cursorY + 1);

                    if(tileInfo.Length > 0)
                    {
                        Console.WriteLine(tileInfo);
                        Program.Say(tileInfo);
                    }

                    float rVolume = gridInput.cursorX / (float)gridInput.width;
                    float lVolume = 1.0f - rVolume;
                    float freq = 1000.0f - ((gridInput.cursorY * 500.0f) / 5.0f);
                    if (currentPlant is null)
                    {
                        lVolume *= Config.current.GridPositionCueVolume;
                        rVolume *= Config.current.GridPositionCueVolume;
                        Program.PlayTone(lVolume, rVolume, freq, freq, 100, SignalGeneratorType.Sin);
                    }
                    else
                    {
                        lVolume *= Config.current.FoundObjectCueVolume;
                        rVolume *= Config.current.FoundObjectCueVolume;
                        if (currentPlant?.need == ZenPlantNeed.None)
                        {
                            Program.PlayTone(lVolume, rVolume, freq, freq, 100, SignalGeneratorType.Triangle);
                            Program.Vibrate(0.1f, 0.1f, 50);
                        }
                        else
                        {
                            Program.PlayTone(lVolume, rVolume, freq, freq, 100, SignalGeneratorType.Square);
                            Program.Vibrate(0.7f, 0.7f, 100);
                        }
                    }

                    //Move cursor to plant grid for sighted players
                    Vector2 cellPos = GetGardenCellPosition() + new Vector2(0.02f, 0.02f);
                    Program.MoveMouse(cellPos.X, cellPos.Y);

                }
                else if (intent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                    Program.PlayBoundaryTone();
            }

            if (currentPage <= 3)
            {
                if (intent == InputIntent.Info1)
                {
                    string plantInfo = "Empty tile";
                    if (currentPlant is not null)
                        plantInfo = Text.plantNames[currentPlant.Value.id];
                    Console.WriteLine(plantInfo);
                    Program.Say(plantInfo, true);
                }
                if (intent == InputIntent.Info2)
                {
                    string needInfo = "No plant";
                    if (currentPlant is not null)
                        needInfo = GetNeedString(currentPlant);
                    Console.WriteLine(needInfo);
                    Program.Say(needInfo, true);
                }
                if (intent == InputIntent.Info3)
                {
                    int needyPlantCount = GetNeedyPlantCount();
                    string needyPlantStr = needyPlantCount + (needyPlantCount == 1 ? " plant needs attention" : " plants need attention");
                    Console.WriteLine(needyPlantStr);
                    Program.Say(needyPlantStr, true);
                }
            }
            else
            {
                if (intent == InputIntent.Info1)
                {
                    int treeHeight = memIO.GetChallengeScore((int)GameMode.TreeOfWisdom);
                    string heightStr = Program.FormatNumber(treeHeight) + " Feet Tall.";
                    Console.WriteLine(heightStr);
                    Program.Say(heightStr, true);
                }
            }

            if (currentPage == 0 && intent is InputIntent.Info4)
                ClickStinky();

            //If back/pause is pressed, click MainMenu button
            if (intent == InputIntent.Deny || intent == InputIntent.Start)
                Program.Click(0.95f, 0.01f);

            if (intent == InputIntent.Confirm)
            {
                //If not holding a plant in a glove, Click tool/button
                if (Program.GetCursorType() != (int)CursorType.PlantFromGlove)
                    Program.Click(ZenTools[toolIndex].posX, 0.05f, false, false, 50, true);

                //If clicking 'next garden' button, don't click plant tile
                if (ZenTools[toolIndex].posX == ZenPageNext_X)
                {
                    hasUpdatedContents = true;
                    return;
                }

                //Click plant/tree position
                if (currentPage == 0)
                {
                    float posX = 0.13f;
                    float posY = 0.24f;

                    posX += gridInput.cursorX * 0.107f;
                    posY += gridInput.cursorY * 0.164f;
                    Program.Click(posX, posY, false, false, 50, true);

                    return;
                }
                else if (currentPage == 1)
                {
                    float posX = ZenPosMG[gridInput.cursorY * 4 + gridInput.cursorX].X;
                    float posY = ZenPosMG[gridInput.cursorY * 4 + gridInput.cursorX].Y;
                    Program.Click(posX, posY, false, false, 50, true);
                }
                else if (currentPage == 3)
                {
                    float posX = ZenPosAQ[gridInput.cursorY * 4 + gridInput.cursorX].X;
                    float posY = ZenPosAQ[gridInput.cursorY * 4 + gridInput.cursorX].Y;
                    Program.Click(posX, posY, false, false, 50, true);
                }
                else if (currentPage == 4)
                    Program.Click(0.5f, 0.5f, false, false, 50, true);

            }
        }

        protected override string? GetContentUpdate()
        {
            RefreshState();
            switch (currentPage)
            {
                case 0:
                    return "Main garden";
                case 1:
                    return "Mushroom garden";
                case 3:
                    return "Aquarium";
                case 4:
                    return "Tree Of Wisdom. Press Info1 to say tree height.";
                default:
                    return null;
            }
        }

        protected override string? GetContent()
        {
            return "Zen Garden\r\n" + GetContentUpdate() + (Config.current.SayAvailableInputs ? inputDescription : "");
        }
    }
}
