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
            {
                Program.GameplayTutorial(new string[] { "Now that you have more than six plants, you'll have to start each level by choosing which ones you want to use.", "Navigate the plant picker similarly to the board.", "Press confirm to select or deselect a plant.", "Once you've picked enough plants, you can begin the game by pressing the start button." });
            }
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
                index += 16;
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

                switch(issue)
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

                plantInfo += (isPicked ? "Picked. " : "") + Consts.plantNames[pickerIndex] + ": " + sunCost + ": " + Consts.plantDescriptions[pickerIndex];

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
                Program.PlayTone(1, 1, 70, 70, 50, SignalGeneratorType.Square);


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

                float frequency = 100.0f + (100.0f * pickedPlantIndex);
                float rVolume = (float)pickedPlantIndex / (float)seedBankSize;
                Program.PlayTone(1.0f - rVolume, rVolume, frequency, frequency, 100, SignalGeneratorType.Square);

                int friendlySlotNumber = pickedPlantIndex + 1;

                string plantName = friendlySlotNumber + ": Empty Slot";

                if (pickedPlantIndex >= 0 && pickedPlantIndex < pickedPlants.Length)
                    plantName = friendlySlotNumber + ": " + Consts.plantNames[(int)pickedPlants[pickedPlantIndex].seedType];

                Console.WriteLine(plantName);
                Program.Say(plantName, true);
            }


            //Click on current plant, adding to picked plants row
            if (intent == InputIntent.Confirm)
            {
                RefreshPlantPickerState();

                int startPickCount = GetSelectedPlants().Length;
                int endPickCount = startPickCount;

                PlantIssue issue = FindPlantIssues((SeedType)pickerIndex);
                if(issue is PlantIssue.NotAllowed)
                {
                    Program.PlayTone(1, 1, 300, 300, 100, SignalGeneratorType.Triangle);
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
                    if (seedState == ChosenSeedState.InChooser && startPickCount == seedBankSize)
                        Program.PlayTone(1, 1, 300, 300, 100, SignalGeneratorType.Triangle);
                    else
                        Program.Click(clickX, clickY);
                }

                hasUpdatedContents = true;
            }

            if (intent == InputIntent.Start)
            {
                //If player has picked enough plants, click ready. Otherwise inform them that they need to pick more plants.

                int pickedCount = GetSelectedPlants().Length;
                if (pickedCount == seedBankSize)
                {
                    Program.Click(0.29f, 0.925f);
                    return;
                }

                Program.PlayTone(1, 1, 300, 300, 100, SignalGeneratorType.Triangle);
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
                    totalZombieTypeString += Consts.zombieNames[zombieType] + ", ";

                Console.WriteLine(totalZombieTypeString);
                Program.Say(totalZombieTypeString);

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
            string plantInfo = Consts.plantNames[0] + ": " + sunCost + ": " + Consts.plantDescriptions[0];

            info += "\r\n" + plantInfo;
            return info;
        }
    }
}
