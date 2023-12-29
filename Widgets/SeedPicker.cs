using Memory;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    class SeedPicker : Widget
    {
        string inputDescription = "\r\nInputs: Directional input to Navigate grid, Confirm to select/deselect plant, Deny to pause, Start to start level, Info1 to list zombies in level, Info2 to say level type, CycleLeft/CycleRight to list selected plants, Info3 to add or remove imitater clone of current plant.\r\n";

        GridInput gridInput;
        int pickedPlantIndex;   //Currently selected slot of the picked plants row (at the top of the screen)
        plantInPicker[] plantPickerState = new plantInPicker[(int)SeedType.NUM_SEED_TYPES];

        public SeedPicker(MemoryIO memIO) : base(memIO, "")
        {
            gridInput = new GridInput(8, 6, false);
            if (Config.current.GameplayTutorial)
                DoTutorials();
        }

        void DoTutorials()
        {
            if (memIO.GetAdventureCompletions() > 0)
                return;
            int level = memIO.GetPlayerLevel();
            if(level == 8)
                Program.GameplayTutorial(Text.tutorial.Level8);
        }

        struct plantInPicker
        {
            public int posX;
            public int posY;
            //public int animStartFrame;
            //public int animEndFrame;
            //public int animStartX;
            //public int animStartY;
            //public int animEndX;
            //public int animEndY;
            public SeedType seedType;
            public ChosenSeedState seedState;
            public int indexInBank;
            public bool refreshing;
            public int refreshCounter;
            public SeedType imitaterType;
            public bool crazyDavePicked;
        }

        enum PlantIssue
        {
            None,       //No issues found
            Nocturnal,  //Nocturnal plant on day level
            CoffeeNight,//Coffeebean at night   (I feel like that should be an achievement. 'Coffee Night' Use a coffee bean on a night level)
            Aquatic,    //Aquatic plants on non-pool levels
            NoGraves,   //Gravebuster on level without graves
            NoFog,      //Plantern on non-foggy levels
            NoGround,   //spikeweed on roof levels
            HasGround,  //FlowerPot on non-roof levels
            NotAllowed, //Sun/free plants on last stand
        }

        PlantIssue FindPlantIssues(SeedType plant)
        {
            LevelType levelType = memIO.GetLevelType();
            bool isNocturnal = Program.IsNocturnal(plant);
            bool isAquatic = Program.IsAquatic(plant);

            if(memIO.GetGameMode() == (int)GameMode.LastStand)
            {
                if (plant is SeedType.SEED_SUNFLOWER or SeedType.SEED_TWINSUNFLOWER or PvZA11y.SeedType.SEED_SUNSHROOM or SeedType.SEED_PUFFSHROOM or SeedType.SEED_SEASHROOM)
                    return PlantIssue.NotAllowed;
            }

            if(isAquatic)
            {
                if (levelType != LevelType.Pool && levelType != LevelType.PoolNight)
                    return PlantIssue.Aquatic;
            }
            
            if(levelType is LevelType.Roof)
            {
                if (plant is SeedType.SEED_SPIKEWEED or SeedType.SEED_SPIKEROCK)
                    return PlantIssue.NoGround;
            }

            if (plant is SeedType.SEED_GRAVEBUSTER && levelType is not LevelType.Night)
                return PlantIssue.NoGraves;

            if (plant is SeedType.SEED_PLANTERN && levelType is not LevelType.PoolNight)
                return PlantIssue.NoFog;

            if (isNocturnal && levelType is not (LevelType.Night or LevelType.PoolNight))
                return PlantIssue.Nocturnal;

            if (plant is SeedType.SEED_FLOWERPOT && levelType is not LevelType.Roof)
                return PlantIssue.HasGround;

            if (plant is SeedType.SEED_INSTANT_COFFEE && levelType is (LevelType.Night or LevelType.PoolNight))
                return PlantIssue.CoffeeNight;

            return PlantIssue.None;
        }

        void SetImitater(int slot, int plantID, bool increaseCount, bool shouldRemove)
        {
            if (shouldRemove)
            {
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + ((int)SeedType.SEED_IMITATER * 0x3c) + 0x24).ToString("X2"), "int", "3");    //Clear InBank
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + ((int)SeedType.SEED_IMITATER * 0x3c) + 0x28).ToString("X2"), "int", "0");    //Clear Index
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + ((int)SeedType.SEED_IMITATER * 0x3c) + 0x34).ToString("X2"), "int", "-1");   //Clear imitaterType
                int prevCount = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",874,d3c");
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874,d3c", "int", (prevCount - 1).ToString());
                
                //Shuffle plants from the right, to fill the empty gap.
                for(int i =0; i < (int)SeedType.NUM_SEED_TYPES; i++)
                {
                    int slotIndex = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",874," + (0xbc + (i * 0x3c) + 0x28).ToString("X2"));
                    if (slotIndex > slot)
                    {
                        memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + (i * 0x3c) + 0x28).ToString("X2"), "int", (slotIndex - 1).ToString());
                        int posX = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",874," + (0xbc + (i * 0x3c)).ToString("X2"));
                        memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + (i * 0x3c)).ToString("X2"), "int", (posX-50).ToString());
                    }
                }
            }
            else
            {
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + ((int)SeedType.SEED_IMITATER * 0x3c) + 0x24).ToString("X2"), "int", "1");    //Write InBank
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + ((int)SeedType.SEED_IMITATER * 0x3c) + 0x28).ToString("X2"), "int", slot.ToString());    //Write Index
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874," + (0xbc + ((int)SeedType.SEED_IMITATER * 0x3c) + 0x34).ToString("X2"), "int", plantID.ToString()); //Write imitaterType
                if (increaseCount)
                {
                    int prevCount = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",874,d3c");
                    memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874,d3c", "int", (prevCount + 1).ToString());
                }
            }
        }

        void RefreshPlantPickerState()
        {
            byte[] plantPickerBytes = memIO.mem.ReadBytes(memIO.ptr.lawnAppPtr + ",874,bc", 3180);  //Can we do this without reallocating the byte array? Might have to fork memory.dll to allow it

            //If not in plant picker?
            if (plantPickerBytes == null)
                return;

            plantPickerState = new plantInPicker[(int)SeedType.NUM_SEED_TYPES];

            int index = 0;
            for (int i = 0; i < (int)SeedType.NUM_SEED_TYPES; i++)
            {
                plantPickerState[i].posX = BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].posY = BitConverter.ToInt32(plantPickerBytes, index);
                index += 28;    //Jump to plant id
                plantPickerState[i].seedType = (SeedType)BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].seedState = (ChosenSeedState)BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].indexInBank = BitConverter.ToInt32(plantPickerBytes, index);
                index += 12;
                plantPickerState[i].imitaterType = (SeedType)BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].crazyDavePicked = BitConverter.ToInt32(plantPickerBytes, index) > 0;
                index += 4;
            }

        }


        plantInPicker[] GetSelectedPlants(bool refreshPlants = true)
        {
            if (refreshPlants)
                RefreshPlantPickerState();


            List<plantInPicker> allSelectedPlants = new List<plantInPicker>();

            for (int i = 0; i < plantPickerState.Length; i++)
            {
                if ((int)plantPickerState[i].seedState == 1)
                    allSelectedPlants.Add(plantPickerState[i]);
            }

            //Sort plants by their position from left to right
            plantInPicker[] sortedPlants = new plantInPicker[allSelectedPlants.Count];

            for (int i = 0; i < allSelectedPlants.Count; i++)
            {
                for (int j = 0; j < allSelectedPlants.Count; j++)
                {
                    if (allSelectedPlants[j].indexInBank == i)
                        sortedPlants[i] = allSelectedPlants[j];
                }
            }

            return sortedPlants;
        }

        void TryAddImitation(int seedBankSize)
        {
            bool ownsImitater = memIO.GetPlayerPurchase(StoreItem.GameUpgradeImitater) > 0;
            if (!ownsImitater)
                return;

            RefreshPlantPickerState();
            var selectedPlants = GetSelectedPlants();
            int pickCount = selectedPlants.Length;
            int freeSlot = -1;
            bool increaseCount = true;
            bool shouldRemove = false;

            //Check if imitater is already one of the picked plants
            for (int i = 0; i < pickCount; i++)
            {
                if (selectedPlants[i].seedType == SeedType.SEED_IMITATER)
                {
                    freeSlot = i;
                    increaseCount = false;
                    break;
                }
            }
            if(freeSlot == -1 && pickCount < seedBankSize)
                freeSlot = pickCount;

            if (freeSlot == -1)
                return;

            int pickerIndex = (gridInput.cursorY * 8) + gridInput.cursorX;

            PlantIssue issue = FindPlantIssues((SeedType)pickerIndex);
            if (issue is PlantIssue.NotAllowed)
                return;

            foreach (var plant in selectedPlants)
            {
                if (plant.imitaterType == plantPickerState[pickerIndex].seedType)
                    shouldRemove = true;
            }

            SetImitater(freeSlot, pickerIndex, increaseCount, shouldRemove);


            //Set "Let's Rock" button to enabled/disabled, if enough plants have been picked
            if (GetSelectedPlants().Length == seedBankSize)
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874,A0" + memIO.ptr.buttonDisabledOffet, "byte", "0");
            else
                memIO.mem.WriteMemory(memIO.ptr.lawnAppPtr + ",874,A0" + memIO.ptr.buttonDisabledOffet, "byte", "1");
        }

        public override void Interact(InputIntent intent)
        {
            bool plantPickerActive = memIO.mem.ReadByte(memIO.ptr.lawnAppPtr + ",868,174,2c") > 0;
            if (!plantPickerActive)
                return;

            int prevX = gridInput.cursorX;
            int prevY = gridInput.cursorY;
            gridInput.Interact(intent);

            int pickerIndex = (gridInput.cursorY * 8) + gridInput.cursorX;

            //If cursor has moved, inform player about plant the cursor is on
            if (prevX != gridInput.cursorX || prevY != gridInput.cursorY)
            {
                float rightVol = gridInput.cursorX / 7.0f;
                float leftVol = 1.0f - rightVol;
                leftVol *= Config.current.GridPositionCueVolume;
                rightVol *= Config.current.GridPositionCueVolume;

                float startFrequency = 400 + (((7 - gridInput.cursorY) + 1) * 100);
                float endFrequency = startFrequency;

                bool isPicked = false;

                var pickedPlants = GetSelectedPlants();
                if (pickedPlants != null)
                {
                    for (int i = 0; i < pickedPlants.Length; i++)
                    {
                        if (pickedPlants[i].seedType == plantPickerState[pickerIndex].seedType)
                        {
                            isPicked = true;
                            break;
                        }
                    }
                }


                Program.PlayTone(leftVol, rightVol, startFrequency, endFrequency, 100, isPicked ? SignalGeneratorType.SawTooth : SignalGeneratorType.Sin);

                int sunCost = Consts.plantCosts[pickerIndex];
                PlantIssue issue = FindPlantIssues((SeedType)pickerIndex);
                string plantInfo = "";

                switch (issue)
                {
                    case PlantIssue.Aquatic:
                        plantInfo = "Aquatic. Not recommended. ";
                        break;
                    case PlantIssue.NoFog:
                    case PlantIssue.HasGround:
                    case PlantIssue.NoGround:
                    case PlantIssue.NoGraves:
                    case PlantIssue.CoffeeNight:
                        plantInfo = "Not recommended. ";
                        break;
                    case PlantIssue.Nocturnal:
                        plantInfo = "Nocturnal. ";
                        break;
                    case PlantIssue.NotAllowed:
                        plantInfo = "Not allowed. ";
                        break;
                }
                
                plantInfo += (isPicked ? "Picked. " : "") + Text.plantNames[pickerIndex] + ": " + sunCost + ": " + Text.plantTooltips[pickerIndex];

                bool plantUnlocked = Program.CheckOwnedPlant(pickerIndex);

                if (plantPickerState[pickerIndex].seedState == ChosenSeedState.Hidden || !plantUnlocked)
                {
                    int finishedAdventure = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",94c,54");
                    bool storeUnlocked = finishedAdventure > 0 || memIO.GetPlayerLevel() > 24;

                    if (gridInput.cursorY == 5)
                        plantInfo = "Unavailable." + (storeUnlocked ? " Buy it from the store." : "");
                    else
                        plantInfo = "Locked. Keep playing adventure mode to unlock more plants.";
                }

                Console.WriteLine(plantInfo);
                Program.Say(plantInfo, true);

                //Move mouse to plant position, for sighted players
                RefreshPlantPickerState();
                float clickX = (plantPickerState[pickerIndex].posX / 800.0f) + 0.03f;
                float clickY = (plantPickerState[pickerIndex].posY / 600.0f) + 0.08f;
                Program.MoveMouse(clickX, clickY);

            }
            else if (intent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                Program.PlayBoundaryTone();


            if (intent == InputIntent.CycleRight)
                pickedPlantIndex++;
            if (intent == InputIntent.CycleLeft)
                pickedPlantIndex--;

            int seedBankSize = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",868,15c,24");
            if (Config.current.WrapPlantSelection)
            {
                pickedPlantIndex = pickedPlantIndex < 0 ? seedBankSize -1 : pickedPlantIndex;
                pickedPlantIndex = pickedPlantIndex >= seedBankSize ? 0 : pickedPlantIndex;
            }
            else
            {
                pickedPlantIndex = pickedPlantIndex < 0 ? 0 : pickedPlantIndex;
                pickedPlantIndex = pickedPlantIndex >= seedBankSize ? seedBankSize - 1 : pickedPlantIndex;
            }

            if (intent == InputIntent.CycleRight || intent == InputIntent.CycleLeft)
            {
                //Go through list of all plants, to find which ones have been selected
                //Sort them based on their xPosition to indicate which slot they're in
                var pickedPlants = GetSelectedPlants();

                Program.PlaySlotTone(pickedPlantIndex, seedBankSize);

                int friendlySlotNumber = pickedPlantIndex + 1;

                string plantName = friendlySlotNumber + ": Empty Slot";
                if (pickedPlantIndex >= 0 && pickedPlantIndex < pickedPlants.Length)
                {
                    bool isImitater = pickedPlants[pickedPlantIndex].seedType == SeedType.SEED_IMITATER;
                    plantName = friendlySlotNumber + ": ";
                    if(isImitater)
                        plantName += "Imitation " + Text.plantNames[(int)pickedPlants[pickedPlantIndex].imitaterType];
                    else
                        plantName += Text.plantNames[(int)pickedPlants[pickedPlantIndex].seedType];
                }

                Console.WriteLine(plantName);
                Program.Say(plantName, true);
            }


            //Click on current plant, adding to picked plants row
            if (intent == InputIntent.Confirm)
            {
                RefreshPlantPickerState();

                int pickCount = GetSelectedPlants().Length;

                PlantIssue issue = FindPlantIssues((SeedType)pickerIndex);
                if(issue is PlantIssue.NotAllowed)
                {
                    Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 300, 300, 100, SignalGeneratorType.Triangle);
                    string alertStr = "That plant is not allowed on this level.";
                    Console.WriteLine(alertStr);
                    Program.Say(alertStr);
                    return;
                }

                ChosenSeedState seedState = plantPickerState[pickerIndex].seedState;
                float clickX = (plantPickerState[pickerIndex].posX / 800.0f) + 0.02f;
                float clickY = (plantPickerState[pickerIndex].posY / 600.0f) + 0.02f;
                if (seedState == ChosenSeedState.InBank || seedState == ChosenSeedState.InChooser)
                {
                    //If player tries to add seed to an alread-full seedbank, play a tone
                    if (seedState == ChosenSeedState.InChooser && pickCount == seedBankSize)
                        Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 300, 300, 100, SignalGeneratorType.Triangle);
                    else
                        Program.Click(clickX, clickY);
                }

                hasUpdatedContents = true;
            }

            if (intent is InputIntent.Info3)
                TryAddImitation(seedBankSize);

            if (intent == InputIntent.Start)
            {
                //If player has picked enough plants, click ready. Otherwise inform them that they need to pick more plants.

                int pickedCount = GetSelectedPlants().Length;
                if (pickedCount == seedBankSize)
                {
                    Program.Click(0.29f, 0.925f);
                    return;
                }

                Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 300, 300, 100, SignalGeneratorType.Triangle);
                string errorString = "Please select " + (seedBankSize - pickedCount) + " more plant" + (seedBankSize - pickedCount > 1 ? "s" : "") + " to begin";
                Console.WriteLine(errorString);
                Program.Say(errorString, true);
            }

            //Click menu button when back is pressed
            if (intent == InputIntent.Deny)
                Program.Click(0.95f, 0.05f);

            //Inform user which zombies will be present in this level
            if(intent is InputIntent.Info1)
            {
                Widgets.Board tempBoard = new Board(memIO, "", true);
                var zombiesThisLevel = tempBoard.GetZombies(true);
                List<int> zombieTypes = new List<int>();
                foreach(var zombie in zombiesThisLevel)
                {
                    if (!zombieTypes.Contains(zombie.zombieType))
                        zombieTypes.Add(zombie.zombieType);
                }
                zombieTypes.Sort();
                string totalZombieTypeString = "";
                foreach(int zombieType in zombieTypes)
                    totalZombieTypeString += Text.zombieNames[zombieType] + ", ";

                Console.WriteLine(totalZombieTypeString);
                Program.Say(totalZombieTypeString);

            }

            if(intent is InputIntent.Info2)
            {
                LevelType lvlType = memIO.GetLevelType();
                string typeName = "";
                switch(lvlType)
                {
                    case LevelType.Normal:
                        typeName = "Front yard, day";
                        break;
                    case LevelType.Night:
                        typeName = "Front yard, night";
                        break;
                    case LevelType.Pool:
                        typeName = "Back yard, day";
                        break;
                    case LevelType.PoolNight:
                        typeName = "Back yard, night";
                        break;
                    case LevelType.Roof:
                        typeName = "Rooftop, day";
                        break;
                    case LevelType.Boss:
                        typeName = "Rooftop, night";
                        break;
                    default:
                        typeName = "Unknown";
                        break;
                }
                    
                Console.WriteLine(typeName);
                Program.Say(typeName);
            }

        }

        protected override string? GetContent()
        {
            //wait until actually interacting with plantpicker
            bool plantPickerActive = memIO.mem.ReadByte(memIO.ptr.lawnAppPtr + ",868,174,2c") > 0;
            if (!plantPickerActive)
            {
                hasReadContent = false;
                return null;
            }

            string info = "Choose your Plants!";

            int sunCost = Consts.plantCosts[0];
            string plantInfo = Text.plantNames[0] + ": " + sunCost + ": " + Text.plantTooltips[0];

            return info += (Config.current.SayAvailableInputs ? inputDescription : "") + plantInfo;
        }
    }
}
