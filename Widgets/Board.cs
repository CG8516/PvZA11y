﻿using Memory;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace PvZA11y.Widgets
{
    class Board : Widget
    {
        public GridInput gridInput;
        public int seedbankSlot;
        int heldPlantID;    //For VaseBreaker/ItsRainingSeeds
        bool shovelPressedLast; //Whether the shovel was the last input (required for shovel confirmation mode)
        int animatingSunAmount;
        int currentZombieID;

        int lastConveyorCount;
        bool doneBowlingTutorial = false;

        List<FloatingPacket> floatingPackets = new List<FloatingPacket>();
        int prevFloatingPacketCount;

        InputIntent lastIntent;
        long inputRepeatTimer;
        int inputRepeatCount;
        int inputRepeatCooldown = 900;  //How many ms can pass before an input is no longer considered a double/triple tap

        public struct Zombie
        {
            public int zombieType;
            public int phase;
            public float posX;
            public float posY;
            public int row;    //From GameObject
            public int health;
            public int maxHealth;
            public bool hypnotized;
            public bool armless;
            public bool headless;
            public bool holdingSomething;   //pole-vaulting, pogo, flag, digger
            public int helmetState;    //cone/bucket/football/miner/wallnutHead/tallnutHead
            public int shieldState;    //screen-door/ladder/newspaper
            public int uniqueID;
            public bool frozen;
            public bool buttered;
            public int age;
        }

        public struct LawnMower
        {
            public MowerType mowerType;
            public int row;
        }

        struct Fireball
        {
            public float x;
            public float y;
            public int row;
            public bool isIce;
        }

        struct FloatingPacket
        {
            public int packetType;
            public float posX;
            public float posY;
            public int disappearTime;
            public int arrayIndex;
        }

        struct plantInBoardBank
        {
            public int refreshCounter;
            public int refreshTime;
            public int index;   //Of what?
            public int offsetX;
            public int packetType;
            public int imitaterType;
            public bool isRefreshing;
            public float absX;
            public bool active;
        }

        public Board(MemoryIO memIO, string pointerChain = "", bool tempOnly = false) : base(memIO, pointerChain)
        {
            //Get width/height for current level

            int width = 9;
            int height = 0;

            LevelType levelType = memIO.GetLevelType();
            switch(levelType)
            {
                case LevelType.Normal:
                case LevelType.Night:
                case LevelType.Roof:
                case LevelType.Boss:
                    height = 5;
                    break;
                case LevelType.Pool:
                case LevelType.PoolNight:
                    height = 6;
                    break;
            }

            if (memIO.GetGameMode() == (int)GameMode.Zombiquarium)
                height = 5;

            if (memIO.GetAdventureCompletions() < 1)
            {
                if (memIO.GetPlayerLevel() == 0)
                    height = 1;
            }
            

            gridInput = new GridInput(width, height, false);

            if(Config.current.GameplayTutorial && !tempOnly)
                DoTutorials();
        }

        int GetZombossHealth()
        {
            var zombies = GetZombies();
            foreach(var zombie in zombies)
            {
                if(zombie.zombieType == (int)ZombieType.DrZomBoss)
                {
                    return zombie.health;
                }
            }

            return 0;
        }

        public List<LawnMower> GetLawnMowers(bool thisRowOnly = false)
        {
            List<LawnMower> lawnMowers = new List<LawnMower>();

            int maxIndex = memIO.mem.ReadInt(memIO.ptr.boardChain + ",11c");
            int currentCount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",128");
            //118
            for(int i =0; i < maxIndex; i++)
            {
                int index = i * 0x48;

                int state = memIO.mem.ReadInt(memIO.ptr.boardChain + ",118," + (index + 0x2c).ToString("X2"));
                byte isDead = (byte)memIO.mem.ReadByte(memIO.ptr.boardChain + ",118," + (index + 0x30).ToString("X2"));
                byte isVisible = (byte)memIO.mem.ReadByte(memIO.ptr.boardChain + ",118," + (index + 0x31).ToString("X2"));
                if (state != 1 || isDead == 1 || isVisible == 0)
                    continue;

                int row = memIO.mem.ReadInt(memIO.ptr.boardChain + ",118," + (index + 0x14).ToString("X2"));
                int type = memIO.mem.ReadInt(memIO.ptr.boardChain + ",118," + (index + 0x34).ToString("X2"));

                if (thisRowOnly && row != gridInput.cursorY)
                    continue;
                lawnMowers.Add(new LawnMower() { row = row, mowerType = (MowerType)type });
            }

            return lawnMowers;
        }

        public List<Program.ToneProperties> FindDeadZombies()
        {
            List<Program.ToneProperties> tones = new List<Program.ToneProperties>();

            int maxIndex = memIO.mem.ReadInt(memIO.ptr.boardChain + ",ac");
            List<Zombie> zombies = new List<Zombie>();

            for (int i = 0; i < maxIndex; i++)
            {
                int index = i * 360;
                int health = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xc8).ToString("X2"));

                //Set dead zombie health to -99999 when scanned with deadScanner for first time, to avoid replaying the scanner beep.
                if (health <= 0 && health != -99999)
                {
                    memIO.mem.WriteMemory(memIO.ptr.boardChain + ",a8," + (index + 0xc8).ToString("X2"), "int", "-99999");

                    int row = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x1c).ToString("X2"));
                    float posX = memIO.mem.ReadFloat(memIO.ptr.boardChain + ",a8," + (index + 0x2c).ToString("X2"));

                    float rVolume = posX / 900.0f;
                    float lVolume = 1.0f - rVolume;
                    rVolume *= Config.current.DeadZombieCueVolume;
                    lVolume *= Config.current.DeadZombieCueVolume;
                    float freq = 1000.0f - ((row * 500.0f) / (float)gridInput.height);
                    float freq2 = freq + 50;
                    tones.Add(new Program.ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = freq, endFrequency = freq, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 0 });
                    tones.Add(new Program.ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = freq2, endFrequency = freq2, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 100 });
                }
            }

            return tones;
        }

        Zombie? CycleZombie(InputIntent intent)
        {
            var zombies = GetZombies();
            
            if (zombies.Count == 0)
                return null;

            if(Config.current.ZombieCycleMode == 0)
                zombies.Sort((a, b) => a.uniqueID.CompareTo(b.uniqueID)); //Sort by id
            else
                zombies.Sort((a, b) => a.posX.CompareTo(b.posX)); //Sort by distance

            int currentIndex = zombies.FindIndex((a) => a.uniqueID == currentZombieID);

            if (currentIndex <= -1)
                currentIndex = 0;
            else if (intent is InputIntent.ZombieMinus)
                currentIndex--;
            else
                currentIndex++;

            if (currentIndex < 0)
                currentIndex = zombies.Count - 1;
            if (currentIndex >= zombies.Count)
                currentIndex = 0;

            currentZombieID = zombies[currentIndex].uniqueID;
            return zombies[currentIndex];
        }

        public List<Zombie> GetZombies(bool seedPicker = false, bool entryScanner = false)
        {
            int maxIndex = memIO.mem.ReadInt(memIO.ptr.boardChain + ",ac");
            int currentCount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",b8");

            List<Zombie> zombies = new List<Zombie>();
            int addedZombies = 0;
            int deadZombies = 0;

            for (int i = 0; i < maxIndex; i++)
            {
                int index = i * 360;
                bool isDead = memIO.mem.ReadByte(memIO.ptr.boardChain + ",a8," + (index + 0xec).ToString("X2")) > 0;
                int health = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xc8).ToString("X2"));
                if (isDead || health <= 0)
                {
                    deadZombies++;
                    continue;
                }
                Zombie zombie = new Zombie();
                zombie.row = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x1c).ToString("X2"));
                zombie.zombieType = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x24).ToString("X2"));
                zombie.age = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x60).ToString("X2"));

                zombie.health = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xc8).ToString("X2"));
                zombie.maxHealth = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xcc).ToString("X2"));

                zombie.phase = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x28).ToString("X2"));
                zombie.posX = memIO.mem.ReadFloat(memIO.ptr.boardChain + ",a8," + (index + 0x2c).ToString("X2"));

                zombie.hypnotized = memIO.mem.ReadByte(memIO.ptr.boardChain + ",a8," + (index + 0xb8).ToString("X2")) == 1;
                zombie.headless = memIO.mem.ReadByte(memIO.ptr.boardChain + ",a8," + (index + 0xba).ToString("X2")) == 0;
                zombie.armless = memIO.mem.ReadByte(memIO.ptr.boardChain + ",a8," + (index + 0xbb).ToString("X2")) == 0;
                zombie.holdingSomething = memIO.mem.ReadByte(memIO.ptr.boardChain + ",a8," + (index + 0xbc).ToString("X2")) == 1;
                zombie.frozen = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xac).ToString("X2")) > 0;
                zombie.buttered = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xb0).ToString("X2")) > 0;

                int helmetHealth = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xd0).ToString("X2"));
                int helmetMax = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xd4).ToString("X2"));

                zombie.uniqueID = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x164).ToString("X2"));
                zombie.helmetState = 0;
                if (helmetHealth > 0 && helmetMax > 0)
                {
                    if (helmetHealth < helmetMax / 3)
                        zombie.helmetState = 2;
                    else if (helmetHealth < helmetMax / 1.5f)
                        zombie.helmetState = 1;
                }
                else if (helmetMax > 0 && helmetHealth <= 0)
                    zombie.helmetState = 3;

                int shieldHealth = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xdc).ToString("X2"));
                int shieldMax = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xe0).ToString("X2"));

                zombie.shieldState = 0;
                if (shieldHealth > 0 && shieldMax > 0)
                {
                    if (shieldHealth < shieldMax / 3)
                        zombie.shieldState = 2;
                    else if (shieldHealth < shieldMax / 1.5f)
                        zombie.shieldState = 1;
                }
                else if (shieldMax > 0 && shieldHealth <= 0)
                    zombie.shieldState = 3;

                //Zomboss should be detected on the right
                if (zombie.zombieType == (int)ZombieType.DrZomBoss)
                    zombie.posX = 799;

                //Skip off-screen zombies
                if (zombie.posX > 800 && !seedPicker && !entryScanner)
                    continue;

                zombie.posY = memIO.mem.ReadFloat(memIO.ptr.boardChain + ",a8," + (index + 0x30).ToString("X2"));

                int zombieAge = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x60).ToString("X2"));
                if(entryScanner && zombieAge < 5)
                {
                    memIO.mem.WriteMemory(memIO.ptr.boardChain + ",a8," + (index + 0x60).ToString("X2"), "int", "5");
                    zombies.Add(zombie);
                    addedZombies++;
                }
                else if(!entryScanner)
                {
                    zombies.Add(zombie);
                    addedZombies++;
                }

                

                //Zomboss is in all rows
                if (zombie.zombieType == (int)ZombieType.DrZomBoss && !entryScanner)
                {
                    zombie.row = 1;
                    zombies.Add(zombie);
                    zombie.row = 2;
                    zombies.Add(zombie);
                    zombie.row = 3;
                    zombies.Add(zombie);
                    zombie.row = 4;
                    zombies.Add(zombie);
                    addedZombies += 4;
                }



            }
            return zombies;
        }

        //Zomboss fireballs/iceballs seem to be their own entity type, so require a different method of obtaining coordinates.
        //They don't have row/column data stored, so we have to guess which row it's in, based on the current X/Y values (with roof slope taken into consideration)
        //Returns closest-matching row, which 'should' always be correct (testing needed for verification)
        int GetClosestZombossBallRow(float x, float y)
        {
            float[] expectedY = new float[5];
            for (int i = 0; i < 5; i++)
                expectedY[i] = -20 + (i * 85);

            if (x < 370)
            {
                for (int i = 0; i < 5; i++)
                    expectedY[i] += ((x - 285.0f) * -0.25f); // maybe?
            }

            int closestRow = 0;
            for (int i = 1; i < 5; i++)
            {
                if (MathF.Abs(expectedY[i] - y) < MathF.Abs(expectedY[closestRow] - y))
                    closestRow = i;
            }

            return closestRow;
        }

        Fireball? GetZombossFireballInfo()
        {
            int reanimCount = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",940,8,4");

            byte[] reanimBytes = memIO.mem.ReadBytes(memIO.ptr.lawnAppPtr + ",940,8,0,0", reanimCount * 0xa0);

            for (int i = 0; i < reanimCount; i++)
            {
                int index = i * 0xa0;
                int reanimID = BitConverter.ToInt32(reanimBytes, index);
                bool isDead = reanimBytes[index + 0x14] != 0;

                if (isDead)
                    continue;




                //If iceball or fireball is found
                if (reanimID == 81 || reanimID == 82)
                {
                    float x = BitConverter.ToSingle(reanimBytes, index + 0x2c);
                    float y = BitConverter.ToSingle(reanimBytes, index + 0x38);
                    Console.WriteLine("{0},{1}", x, y);

                    int closestRow = GetClosestZombossBallRow(x, y);
                    Console.WriteLine("Row: " + closestRow);

                    Fireball fireball = new Fireball();
                    fireball.x = x;
                    fireball.y = y;
                    fireball.row = closestRow;
                    fireball.isIce = reanimID == 81;

                    return fireball;
                }
            }

            return null;
        }

        bool VaseBreakerCheck()
        {
            int level = memIO.GetPlayerLevel();
            int gameMode = memIO.GetGameMode();

            if (gameMode == 0 && level == 35)
                return true;

            if (gameMode >= (int)GameMode.VaseBreaker1 && gameMode <= (int)GameMode.VaseBreakerEndless)
                return true;

            return false;
        }

        Vector2 GetBoardCellPosition(int plantX = -1, int plantY = -1)
        {
            if (plantX == -1)
                plantX = gridInput.cursorX;
            if (plantY == -1)
                plantY = gridInput.cursorY;
            LevelType levelType = memIO.GetLevelType();

            float yOffset = 0.16f;


            if (levelType == LevelType.Pool || levelType == LevelType.PoolNight)
                yOffset = 0.145f;
            //yOffset = 0.13f;

            if (levelType == LevelType.Roof || levelType == LevelType.Boss)
                yOffset = 0.145f;

            float cellX = 0.09f + (plantX * 0.1f);
            float cellY = 0.22f + (plantY * yOffset);


            if (levelType == LevelType.Roof || levelType == LevelType.Boss)
                cellY += 0.04f * (4.0f - MathF.Min(plantX, 4.0f));

            return new Vector2(cellX, cellY);

        }

        uint ConveyorBeltCounter()
        {
            return memIO.mem.ReadUInt(memIO.ptr.boardChain + ",15c,34c"); //TODO: Move to pointers/memIO
        }


        //TODO: Rework this. We can grab seedbank size within function.
        //We don't need this offset stuff, just a switch/case with some hardcoded x values will be fine
        //I just got lazy and copied the plant placement code, then increased the offset
        void ShovelPlant(int seedbankSize)
        {
            bool wasPaused = memIO.GetBoardPaused();

            //Ensure we aren't paused while placing a plant
            memIO.SetBoardPaused(false);


            float dirtyOffset = 1.0f;
            if (seedbankSize == 4)
                dirtyOffset = 1.4f;
            else if (seedbankSize <= 6)
                dirtyOffset = 1.2f;
            else if (seedbankSize <= 7)
                dirtyOffset = 1.076f;
            else if (seedbankSize == 8)
                dirtyOffset = 1.025f;
            else if (seedbankSize == 9)
                dirtyOffset = 1.0127f;

            float shovelY = 0.065f;
            float shovelX = (0.14f + (seedbankSize * 0.063f * dirtyOffset)) + 0.1f;
            //Program.MoveMouse(shovelX, shovelY);
            Program.Click(shovelX, shovelY);

            Task.Delay(50).Wait();

            Vector2 cellPos = GetBoardCellPosition();

            Program.Click(cellPos.X, cellPos.Y);

            //Restore pause state
            memIO.SetBoardPaused(wasPaused);
        }

        List<plantInBoardBank> GetPlantsInBoardBank()
        {
            List<plantInBoardBank> newPlants = new List<plantInBoardBank>(10);
            while (newPlants.Count < 10)
                newPlants.Add(new plantInBoardBank() { packetType = -1 });

            byte[] plantBytes = memIO.mem.ReadBytes(memIO.ptr.lawnAppPtr + ",868,15c,28", 800);    //yucky
            if (plantBytes == null)
                return newPlants;

            //On conveyor levels, for each plant at offsetX == 0, the max offsetX should decrease by idk something
            int maxX = 450;
            int stoppedPlants = 0;

            for (int i = 0; i < 10; i++)
            {
                int byteIndex = i * 80;
                //plants[i] = new plantInBoardBank();

                plantInBoardBank plant = new plantInBoardBank();

                plant.refreshCounter = BitConverter.ToInt32(plantBytes, byteIndex + 0x24);
                plant.refreshTime = BitConverter.ToInt32(plantBytes, byteIndex + 0x28);
                plant.index = BitConverter.ToInt32(plantBytes, byteIndex + 0x2c);
                plant.offsetX = BitConverter.ToInt32(plantBytes, byteIndex + 0x30);
                plant.absX = BitConverter.ToInt32(plantBytes, byteIndex + 0x08);
                plant.absX += plant.offsetX;
                plant.absX /= 800.0f;
                plant.packetType = BitConverter.ToInt32(plantBytes, byteIndex + 0x34);
                plant.imitaterType = BitConverter.ToInt32(plantBytes, byteIndex + 0x38);
                plant.isRefreshing = BitConverter.ToBoolean(plantBytes, byteIndex + 0x49);
                plant.active = BitConverter.ToBoolean(plantBytes, byteIndex + 0x48);
                //Console.WriteLine(plant.offsetX);
                //Console.WriteLine(plant.packetType);

                if (plant.offsetX <= 0)
                    stoppedPlants++;

                //Console.WriteLine("Plant absX: " + plant.absX);

                if (plant.absX < 0.72f)
                    //if (plant.offsetX < maxX - (stoppedPlants*50))
                    newPlants[i] = plant;

                //Console.WriteLine("i: {0}, Index: {1}, Type: {2}", i, plants[i].index, plants[i].packetType);
            }

            return newPlants;
        }

        void MoveMouseToTile(Vector2? cellPos = null)
        {
            if(cellPos is null)
                cellPos = GetBoardCellPosition();
            Program.MoveMouse(cellPos.Value.X, cellPos.Value.Y);
        }

        string GetWaveInfo()
        {
            int numWaves = memIO.mem.ReadInt(memIO.ptr.boardChain + ",557c");
            int currentWave = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5594");

            int wavesPerFlag = numWaves < 10 ? numWaves : 10;
            int numFlags = numWaves / wavesPerFlag;

            int completedFlags = currentWave / wavesPerFlag;

            string percentageStr = (((float)currentWave / (float)numWaves) * 100.0f).ToString("0") + "% complete";

            string waveInfo = "";

            if (numFlags >= 1)
                waveInfo = "Wave " + completedFlags + " of " + numFlags;

            if (currentWave == numWaves)
                waveInfo = "Final Wave";

            string info = "";

            if (currentWave == numWaves)
                info = waveInfo;
            else if (inputRepeatCount % 2 == 0)
                info = percentageStr + ", " + waveInfo;
            else
                info = waveInfo + ", " + percentageStr;

            return info;

        }

        void DragPlant(InputIntent intent)
        {
            bool mPaused = memIO.GetBoardPaused();
            memIO.SetBoardPaused(false);
            Vector2 cellPos = GetBoardCellPosition();
            Vector2 dragPos = cellPos;
            if (intent is InputIntent.Up)
                dragPos.Y -= 0.1f;
            if (intent is InputIntent.Down)
                dragPos.Y += 0.1f;
            if (intent is InputIntent.Left)
                dragPos.X -= 0.1f;
            if (intent is InputIntent.Right)
                dragPos.X += 0.1f;
            Program.Click(cellPos.X, cellPos.Y, dragPos.X, dragPos.Y);
            memIO.SetBoardPaused(mPaused);
        }

        //TODO: Split code into method which obtains cell position, one that obtains seedbank/conveyor pos, one that clicks, one that moves the mouse.
        //Can't we just grab the packet position from memory? I wrote this near the start of the project, so don't remember if I tried, or just started with a hacky solution.
        void PlacePlant(int seedbankIndex, int maxPlants, float offsetX = 0, bool clickPlantFirst = true, bool visualOnly = false, bool visualSeedLoc = false, bool releasePlant = true, bool clickPlantOnly = false)
        {

            bool mPaused = memIO.GetBoardPaused();

            //Ensure we aren't paused while placing a plant
            if (!visualOnly)
                memIO.SetBoardPaused(false);


            //Check if can plant
            //Click selected plant, then selected grid cell.

            //Console.WriteLine(maxPlants);

            float dirtyOffset = 1.0f;
            if (maxPlants == 4)
                dirtyOffset = 1.1f;
            else if (maxPlants <= 6)
                dirtyOffset = 1.2f;
            else if (maxPlants <= 7)
                dirtyOffset = 1.076f;
            else if (maxPlants == 8)
                dirtyOffset = 1.025f;
            else if (maxPlants == 9)
                dirtyOffset = 1.0127f;

            float cardX = 0.14f + (seedbankIndex * (0.063f * dirtyOffset));
            cardX += offsetX / 800.0f;

            //Console.WriteLine("CardX: " + cardX*800.0f);

            float cardY = 0.065f;

            if (clickPlantFirst && !visualOnly)
            {
                Program.Click(cardX, cardY);

                Task.Delay(50).Wait();
            }


            Vector2 cellPos = GetBoardCellPosition();

            if (visualOnly)
            {
                if (visualSeedLoc)
                    Program.MoveMouse(cardX, cardY);
                else
                    Program.MoveMouse(cellPos.X, cellPos.Y);
                return;
            }
            
            if(!clickPlantOnly)
                Program.Click(cellPos.X, cellPos.Y);

            Task.Delay(50).Wait();

            if(releasePlant)
                Program.Click(cellPos.X, cellPos.Y, true);  //Right-click to release plant

            //Restore pause state
            memIO.SetBoardPaused(mPaused);
        }

        void DoTutorials()
        {
            GameMode gameMode = (GameMode)memIO.GetGameMode();

            if (gameMode is GameMode.Adventure)
            {
                int level = memIO.GetPlayerLevel();

                if (memIO.GetAdventureCompletions() > 0)
                {
                    if (level == 1)
                    {
                        Program.GameplayTutorial(new string[] { "Welcome to new game plus!", "You'll be able to replay adventure mode, but this time with added difficulty.", "Additionally, Dave will select three plants for every level, which you can not change.", "Good luck!" });
                    }
                    return;
                }


                if (level == 1)
                {
                    Program.GameplayTutorial(new string[] { "Use your directional keys to navigate tutorial text.", "To close a tutorial, press the confirm button." });
                    Program.GameplayTutorial(new string[] { "You are presented with a grid-like lawn, 9 tiles long, and one tile high.", "Your house is on the left, and zombies will soon appear from the right.", "You'll need to use plants to stop the zombies from breaking into your house, and eating your brain." });
                    Program.GameplayTutorial(new string[] { "A deck at the top of the screen contains all the plants you can place on the lawn.", "Placing plants will cost sun, and will trigger a cooldown before you can place the same plant again.", "Sun falls from the sky, and will be automatically collected as you play.", "Press the Info3 button to read your sun count." });
                    Program.GameplayTutorial(new string[] { "After the tutorial, place a plant with the confirm button.", "Most shooting plants will shoot from their placed position, towards the right of the screen.", "You will need to place two plants before the game will start.", "Soon, zombies will start appearing. You can detect them with your zombie-sonar, which is the Info1 button." });
                    Program.GameplayTutorial(new string[] { "And finally, you may occasionally hear advice telling you to click on things.", "This is part of the game's built-in tutorial, and can be ignored." });
                }
                if (level == 2)
                {
                    Program.GameplayTutorial(new string[] { "Two more rows of lawn have been unrolled, making it a 9 by 3 grid.", "You've also unlocked a new plant, which you can select with the cycleLeft and cycleRight buttons" });
                    Program.GameplayTutorial(new string[] { "Sunflowers are extremely important, as they produce sun, which is a vital resource for building your defenses.", "You'll want to place at least one sunflower for each row.", "When navigating the board, you can check which plant has been placed in a tile, by pressing the Info2 button." });
                    Program.GameplayTutorial(new string[] { "Keep in mind that the zombie sonar is for the current row only.", "You'll need to move up and down on the board, to detect zombies in each row." });
                }
                if (level == 3)
                {
                    Program.GameplayTutorial(new string[] { "You can use the option button to freeze the game.", "This is slightly different to pausing, as it will not open the pause menu.", "You will still be able to interact with the board, but all gameplay will be frozen.", "Don't forget to unfreeze with the option button again." });
                }
                if (level == 4)
                {
                    Program.GameplayTutorial(new string[] { "The final two rows of the lawn have been unrolled, making it a 9 by 5 grid.", "Each plant has unique stats, including how long it takes to refresh." });
                }
                if (level == 5)
                {
                    Program.GameplayTutorial(new string[] { "Pressing the deny button will dig up the plant at your current board position.", "Move around the board, and use the Info2 button to find the three peashooters.", "Shovel all three peashooters to continue." });
                }
                if (level == 6)
                {
                    Program.GameplayTutorial(new string[] { "Each row has a lawn mower on the left side.", "If a zombie reaches a lawnmower, it will be activated and shred all zombies in that row.", "Once a lawnmower has been used, it won't come back until the level restarts, or a new one begins." });
                    Program.GameplayTutorial(new string[] { "Also, watch out for pole-vaulting zombies! They're fast, quiet, and jump over the first plant they run into!" });
                }
                if (level == 10)
                {
                    Program.GameplayTutorial(new string[] { "This is another conveyor belt level, similar to the bowling minigame.", "Plants will stop arriving when the belt gets full.", "Placing a plant will make room on the belt, and it will start moving again." });
                }
                if (level == 11)
                {
                    Program.GameplayTutorial(new string[] { "The sun has set; leaving you with new challenges to face in this moonlit night.", "You'll find gravstones scattered around your front yard, which can not be planted on.", "And on the final wave of each level, additional zombies will emerge from the graves." });
                }
                if (level == 15)
                {
                    Program.GameplayTutorial(new string[] { "This is Whack-A-Zombie.", "Zombies will quickly rise from graves around the lawn.", "You need to quickly find each zombie, then press the deny button to whack them with your mallet.", "Some zombies take more than one hit, and some will drop sun for you to use." });
                }
                if (level == 21)
                {
                    Program.GameplayTutorial(new string[] { "Your backyard has six rows of tiles, with an in-ground pool taking up the two middle rows.", "Only aquatic plants can be placed in the pool, however, you can place non-aquatic plants on top of lillypads." });
                }
                if (level == 31)
                {
                    Program.GameplayTutorial(new string[] { "As the moon takes the suns place in the sky, it brings a thick rolling fog with it." });
                }
                if (level == 35)
                {
                    Program.GameplayTutorial(new string[] { "This is VaseBreaker, which takes place at night in your front yard.", "There are columns of vases on the right side of your lawn.", "Press confirm on a vase to break it open.", "Breaking a vase can spawn a zombie, or put a plant in your hand.", "Your goal is to break all the vases, and defeat all the zombies." });
                }
                if (level == 36)
                {
                    Program.GameplayTutorial(new string[] { "Digger zombies will flank your plants by tunneling under your lawn, and emerging on the left.", "They walk from where they emerge, to the right, eating any plants that get in their way." });
                }
                if (level == 41)
                {
                    Program.GameplayTutorial(new string[] { "The roof slopes up from left to right, with a flat section on the right four columns.", "Most shooting plants will be useless on the left four columns, as their projectiles will hit the slope.", "Catapulting plants will lob projectiles up and to the right, making them effective from any column." });
                    Program.GameplayTutorial(new string[] { "Additionally, you can not plant on roof tiles.", "You will need to plant in flowerpots, similarly to lillypads in the pool.", "It is also day now, so mushrooms will be useless." });
                }
                if (level == 50)
                {
                    Program.GameplayTutorial(new string[] { "This is it, the final level.", "Dr.Zomboss is in the cockpit of a giant robot.", "He will be invincible, until he lowers his head onto the screen.", "Your zombie sonar will inform you of when he is vulnerable.", "When he lowers his head, he can be attacked from plants in any row." });
                    Program.GameplayTutorial(new string[] { "Dr.Zomboss will open his mouth, to release a large ball of fire, or ice.", "Fireballs and Iceballs will slowly roll from right to left, squashing any plants in their path.", "Fireballs can be extinguished from any row, but iceballs will need to be melted from the same row." });
                    Program.GameplayTutorial(new string[] { "Dr.Zomboss can also be frozen with ice-shrooms, extending your attack time.", "Good luck!" });
                }
            }
            if (gameMode is GameMode.ZomBotany)
                Program.GameplayTutorial(new string[] { "This is ZomBotany.", "In this mode, the zombies have swapped their heads with plants!", "Zombies with a plant head will behave very similarly to the plant they've become.", "This takes place in your front yard, during the day." });
            else if (gameMode is GameMode.SlotMachine)
                Program.GameplayTutorial(new string[] { "In this game, your deck has been replaced with an old casino slot machine.", "Use the Info3 button to spend 25 sun and spin the slots.", "If you get two or three of a kind, you'll win plants, or some sun.", "Use the cycleLeft and cycleRight buttons to find out what plant you've won.", "Save up 2000 sun while defending your front yard, to win the game." });
            else if (gameMode is GameMode.ItsRainingSeeds)
                Program.GameplayTutorial(new string[] { "In this game, you have no deck.", "Plant seed packets will fall from the sky as you play, similarly to a conveyor belt, but they will disappear if you don't use them quickly enough.", "The game takes place at night in your foggy backyard pool." });
            else if (gameMode is GameMode.Beghouled)
            {
                Program.GameplayTutorial(new string[] { "In this mode, you'll need to match triplets of plants.", "To do that, select a plant with the deny button, then press a directional button.", "This will drag the plant in the direction you choose, but only if a match can be made." });
                Program.GameplayTutorial(new string[] { "Once a match has been made, the matched plants will disappear, and the plants above will fall down to fill their place.", "You'll also get some sun for each match, which you can use to upgrade your plants.", "Upgrading plants will help, because you also need to defend your house at the same time!" });
                Program.GameplayTutorial(new string[] { "If a zombie eats one of your plants, it will create a crater, which will obstruct your matches.", "You can fill a random crater by purchasing the crater fill item for 200 sun.", "To win the game, you'll need to make 75 matches." });
                Program.GameplayTutorial(new string[] { "You may want to adjust your accessibility options, to help focus on just the plants, rather than the zombies.", "There are also some be-ghouled accessibility options, which can assist with finding matches." });
            }
            else if (gameMode is GameMode.Invisighoul)
                Program.GameplayTutorial(new string[] { "All zombies are completely invisible in this mode, which offers quite the... oh, wait... this changes absolutely nothing.", "Maybe a future mod update will add some new challenge to this mode.", "It's a conveyor belt level in your backyard at night btw." });   //Can't wait for first comment about this, haha
            else if (gameMode is GameMode.SeeingStars)
                Program.GameplayTutorial(new string[] { "In this mode, you'll need to plant starfruits on your front yard, to make a large star shape.", "You'll find some placeholder tiles, which can only have startfruits planted there.", "Fill all of the starfruit tiles to win the game.", });
            else if (gameMode is GameMode.Zombiquarium)
                Program.GameplayTutorial(new string[] { "You have a large aquarium tank, which currently contains two friendly snorkel zombies.", "Snorkel zombies will swim freely around the tank and produce sun for you, but if they don't eat enough brains, they will die.", "You can spend sun to feed them brains, or buy additional snorkel zombies.", "Once you reach 1000 sun, you can buy a trophy to complete the level." });
            else if (gameMode is GameMode.BeghouledTwist)
            {
                Program.GameplayTutorial(new string[] { "Be-ghouled 2 is very similar to the original be-ghouled minigame", "You still need to make matches of 3, however, the controls have changed."});
                Program.GameplayTutorial(new string[] { "Pressing the deny button will perform a clockwise rotation, if a match can be made.", "The rotation includes the current plant, the plant below it, the plant diagonally down-left one, and the plant directly to the left.", "Because the action is performed on plants to the left and below, if you perform this action in the first column, or bottom row, it will perform the action in the second column or second-last row instead."});
                Program.GameplayTutorial(new string[] { "As with the previous game, there is an optional match assistance in the accessibility settings, which will play a tone if rotating the current tile will create a match.", "It's also recommended that you reduce or disable zombie sonars while playing this minigame, as your only actionable defense is to purchase plant upgrades." });
            }
            else if (gameMode is GameMode.BigTroubleLittleZombie)
                Program.GameplayTutorial(new string[] { "This is the same minigame you experience in level 3,5, but much harder.", "You'll have to defeat huge swarms of tiny zombies, which have less health than full zombies, but they move much faster and deal the same damage.", "This is a conveyor belt level, and takes place in your back yard during the day." });
            else if (gameMode is GameMode.PortalCombat)
                Program.GameplayTutorial(new string[] { "This minigame features four portals. Two of them are square, and the other two are round.", "Any zombie, projectile, or lawnmower that passes through a portal, will teleport to the other one of the same shape.", "While playing, a random portal may be picked and moved somewhere else on the lawn. You will be notified when this happens.", "This level features a conveyor belt, and takes place in your front yard at night." });
            else if (gameMode is GameMode.ColumnLikeYouSeeEm)
                Program.GameplayTutorial(new string[] { "This minigame takes place on your roof.", "Placing a plant in any row will fill the entire column with that plant.", "You'll receive plants on a conveyor." });
            else if (gameMode is GameMode.BobsledBonanza)
                Program.GameplayTutorial(new string[] { "This minigame takes place in your backyard during the day.", "The level starts with long ice trails in the four grass rows.", "Bobsled zombies will quickly slide in on these trails in groups of four.", "Your only goal is to survive.", "There are four waves of zombies." });
            else if (gameMode is GameMode.ZombieNimbleZombieQuick)
                Program.GameplayTutorial(new string[] { "This game runs at two times the normal speed, and features many pole-vaulting and dolphin-rider zombies.", "Defend your daytime backyard through four waves of zombies, to win the game." });
            else if (gameMode is GameMode.WhackAZombie)
                Program.GameplayTutorial(new string[] { "This is the standalone Whack-A-Zombie minigames.", "This is a more difficult version of the same minigame on level 2,5." });   //cop-out?
            else if (gameMode is GameMode.LastStand)
                Program.GameplayTutorial(new string[] { "In this game, which takes place in your backyard during the day,", "no sun will fall from the sky, and you'll be unable to choose any sun-producing plants.", "You start the game with 5,000 sun, and are given as much time as needed to prepare your lawn.", "When you're ready to begin, press the option/freeze input, and the wave will begin.", "At the end of a wave, you'll be given some additional sun, and time to prepare once again.", "Win the game by surviving through five waves." });
            else if (gameMode is GameMode.ZomBotany2)
                Program.GameplayTutorial(new string[] { "This minigame is the same as ZomBotany, but with many new types of plant-headed zombies.", "It takes place in your back yard, during the day." });
            else if (gameMode is GameMode.WallnutBowling2)
                Program.GameplayTutorial(new string[] { "This is wall-nut bowling 2!", "It's very similar to wall-nut bowling 1, but you will encounter more zombies.", "You'll also receive giant wall-nuts, which will always roll directly over anything that gets in their way, without ricocheting.", "This is a daytime front yard level." });
            else if (gameMode is GameMode.PogoParty)
                Program.GameplayTutorial(new string[] { "In this minigame, you must defend your roof from swarms of pogo zombies." });
            else if (gameMode is GameMode.DrZombossRevenge)
                Program.GameplayTutorial(new string[] { "This minigame is a harder version of the final boss of adventure mode.", "Dr. ZomBoss will have twice as much health, and will send zombies more frequently." });
        }

        int getIzombieBrainCount(bool thisRowOnly = false)
        {
            int count = 0;

            var gridItems = Program.GetGridItems();
            foreach (var item in gridItems)
            {
                if ((GridItemType)item.type == GridItemType.IzombieBrain)
                {
                    if (!thisRowOnly || item.y == gridInput.cursorY)
                        count++;
                }
            }
            return count;
        }

        bool IsSurvival()
        {
            GameMode gameMode = (GameMode)memIO.GetGameMode();
            if (gameMode >= GameMode.SurvivalDay && gameMode <= GameMode.SurvivalEndless5)
                return true;
            return false;
        }

        //Returns true if current plant packet is fully refreshed, and there's enough sun to place it
        public bool PlantPacketReady()
        {
            GameMode gameMode = (GameMode)memIO.GetGameMode();
            bool inIZombie = gameMode >= GameMode.IZombie1 && gameMode <= GameMode.IZombieEndless;
            
            bool inSlotMachine = gameMode is GameMode.SlotMachine;
            bool inZombiquarium = gameMode is GameMode.Zombiquarium;
            bool inBeghouled = gameMode is GameMode.Beghouled;
            bool inBeghouled2 = gameMode is GameMode.BeghouledTwist;
            bool conveyorLevel = ConveyorBeltCounter() > 0;
            bool inVaseBreaker = VaseBreakerCheck();
            bool vaseBreakerEndless = gameMode is GameMode.VaseBreakerEndless;
            bool inRainingSeeds = gameMode is GameMode.ItsRainingSeeds;

            int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");
            sunAmount += animatingSunAmount;

            var plants = GetPlantsInBoardBank();

            if (inVaseBreaker || inRainingSeeds)
            {
                int floatingCount = floatingPackets.Count;
                if(vaseBreakerEndless && seedbankSlot == 0)
                {
                    if (plants[seedbankSlot].isRefreshing || sunAmount < 150)
                        return false;
                    return true;
                }
                if(floatingCount > prevFloatingPacketCount)
                {
                    prevFloatingPacketCount = floatingCount;
                    return true;
                }
                prevFloatingPacketCount = floatingCount;
                return false;
            }
            

            if(conveyorLevel)
            {
                var bankPlants = GetPlantsInBoardBank();
                int conveyorCount = 0;
                foreach(var plant in bankPlants)
                {
                    if (plant.packetType != -1)
                        conveyorCount++;
                }
                int prevCount = lastConveyorCount;
                lastConveyorCount = conveyorCount;
                return prevCount < conveyorCount;
            }

            if (inSlotMachine)
            {
                bool slotReady = memIO.mem.ReadInt(memIO.ptr.boardChain + ",178,54") == 0;
                return slotReady;
            }

            if(inZombiquarium)
            {
                if (seedbankSlot == 0 && sunAmount >= 100)
                    return true;
                if (seedbankSlot == 1 && sunAmount >= 1000)
                    return true;

                var gridItems = Program.GetGridItems();
                int brainCount = 0;
                foreach(var item in gridItems)
                {
                    if (item.type == (int)GridItemType.Brain)
                        brainCount++;
                }

                if (seedbankSlot == 2 && sunAmount >= 5 && brainCount < 3)
                    return true;

                return false;
            }
            if(inBeghouled || inBeghouled2)
            {
                if (seedbankSlot == 0 && sunAmount >= 1000)
                    return true;
                if (seedbankSlot == 1 && sunAmount >= 500)
                    return true;
                if (seedbankSlot == 2 && sunAmount >= 250)
                    return true;
                if (seedbankSlot == 3 && sunAmount >= 100)
                    return true;
                if (seedbankSlot == 4 && sunAmount >= 200)
                    return true;
                return false;
            }

            if (plants[seedbankSlot].packetType < 0)
                return false;

            
            int sunCost = inIZombie ? Consts.iZombieSunCosts[plants[seedbankSlot].packetType - 60] : Consts.plantCosts[plants[seedbankSlot].packetType];

            bool notEnoughSun = sunAmount < sunCost;
            bool refreshing = plants[seedbankSlot].isRefreshing;

            if (notEnoughSun || refreshing)
                return false;

            return true;
        }

        bool CheckIceAtTile()
        {
            int distanceIndex = 0x624 + (4 * gridInput.cursorY);
            int timerIndex = 0x63c + (4 * gridInput.cursorY);
            int iceTimerThisRow = memIO.mem.ReadInt(memIO.ptr.boardChain + "," + timerIndex.ToString("X2"));
            int iceDistanceThisRow = memIO.mem.ReadInt(memIO.ptr.boardChain + "," + distanceIndex.ToString("X2"));
            int[] iceLimits = new int[] { 107, 187, 267, 347, 427, 507, 587, 667, 750 };

            if (iceTimerThisRow < 1)
                return false;

            if (iceDistanceThisRow <= iceLimits[gridInput.cursorX])
                return true;

            return false;
        }

        bool CheckVaseAtTile()
        {
            var gridItems = Program.GetGridItems();
            foreach(var item in gridItems)
            {
                if (item.x == gridInput.cursorX && item.y == gridInput.cursorY && item.type == (int)GridItemType.Vase)
                    return true;
            }
            return false;
        }

        string? GetCurrentTileObject(bool informEmptyTiles = true, bool beepOnFound = true, bool beepOnEmpty = true)
        {
            var gridItems = Program.GetGridItems();

            int vaseType = -1;
            bool hasCrater = false;
            bool hasGravestone = false;
            bool hasIce = CheckIceAtTile();
            bool hasSquarePortal = false;
            bool hasCirclePortal = false;
            for (int i = 0; i < gridItems.Count; i++)
            {
                if (gridItems[i].x == gridInput.cursorX && gridItems[i].y == gridInput.cursorY)
                {
                    if (gridItems[i].type == (int)GridItemType.Gravestone)
                        hasGravestone = true;
                    else if (gridItems[i].type == (int)GridItemType.Vase)
                        vaseType = gridItems[i].state;
                    else if (gridItems[i].type == (int)GridItemType.Crater)
                        hasCrater = true;
                    else if (gridItems[i].type == (int)GridItemType.PortalCircle)
                        hasCirclePortal = true;
                    else if (gridItems[i].type == (int)GridItemType.PortalSquare)
                        hasSquarePortal = true;
                }
                //Sometimes portals in 'portal combat' minigame spawn in column 9, but display in column 8
                if(gridInput.cursorX == 8 && gridItems[i].x == 9 && gridItems[i].y == gridInput.cursorY)
                {
                    if (gridItems[i].type == (int)GridItemType.PortalCircle)
                        hasCirclePortal = true;
                    else if (gridItems[i].type == (int)GridItemType.PortalSquare)
                        hasSquarePortal = true;
                }
            }

            bool starfruitTemplate = memIO.GetGameMode() == (int)GameMode.SeeingStars && Consts.SeeingStars[gridInput.cursorY * 9 + gridInput.cursorX];
            if (starfruitTemplate && Program.GetPlantAtCell(gridInput.cursorX, gridInput.cursorY).plantType == (int)SeedType.SEED_STARFRUIT)
                starfruitTemplate = false;

            float rightVol = (float)gridInput.cursorX / (float)gridInput.width;
            float leftVol = 1.0f - rightVol;
            rightVol *= Config.current.FoundObjectCueVolume;
            leftVol *= Config.current.FoundObjectCueVolume;
            float freq = 1000.0f - ((gridInput.cursorY * 500.0f) / (float)gridInput.height);

            var plant = Program.GetPlantAtCell(gridInput.cursorX, gridInput.cursorY);

            string plantInfoString = "";

            if (plant.plantType == -1)
            {
                //Get row type
                int rowType = memIO.mem.ReadInt(memIO.ptr.boardChain + "," + (0x5f0 + (gridInput.cursorY * 4)).ToString("X2"));
                LevelType levelType = memIO.GetLevelType();

                string typeString = "";

                typeString = rowType == 0 ? "dirt" : rowType == 1 ? "grass" : rowType == 2 ? "water" : "";
                if (levelType == LevelType.Roof || levelType == LevelType.Boss)
                    typeString = "roof";

                bool inBeghouled = memIO.GetGameMode() == (int)GameMode.Beghouled;
                if (inBeghouled && gridInput.cursorX < 8)
                    hasCrater = true;

                if (hasCrater)
                    plantInfoString = "Crater";
                else if (hasGravestone)
                    plantInfoString = "Gravestone";
                else if (vaseType != -1)
                    plantInfoString = vaseType == 3 ? "Vase" : vaseType == 4 ? "Plant vase" : "Zombie vase";
                else if (hasIce)
                    plantInfoString = "Ice";
                else if (starfruitTemplate)
                    plantInfoString = "Starfruit guide";
                else if (hasCirclePortal)
                    plantInfoString = "Round portal";
                else if (hasSquarePortal)
                    plantInfoString = "Square portal";
                else
                {
                    if (informEmptyTiles)
                        plantInfoString = "Empty " + typeString + " tile";
                    else
                        plantInfoString = null;
                }

                if ((hasCrater || hasGravestone || vaseType != -1 || hasIce || starfruitTemplate || hasCirclePortal || hasSquarePortal) && beepOnFound)
                {
                    Program.PlayTone(leftVol, rightVol, freq, freq, 100, SignalGeneratorType.SawTooth);
                    Program.Vibrate(0.3f, 0.3f, 50);
                }
                else if (beepOnEmpty)
                    Program.PlayTone(leftVol, rightVol, freq, freq, 50, SignalGeneratorType.Square);
            }
            else
            {
                if (plant.squished)
                    plantInfoString = "Squished ";
                if (plant.sleeping)
                    plantInfoString += "Sleeping ";
                if (plant.plantType == (int)SeedType.SEED_POTATOMINE)
                {
                    if (plant.state == 0)
                        plantInfoString = "Buried ";
                    else
                        plantInfoString = "Armed ";
                }
                if(plant.plantType == (int)SeedType.SEED_CHOMPER)
                {
                    if (plant.state == 13)
                        plantInfoString = "Chewing ";
                }
                if (plant.plantType == (int)SeedType.SEED_SCAREDYSHROOM)
                {
                    if (plant.state == 20 || plant.state == 21)
                        plantInfoString = "Buried ";
                }
                if (plant.plantType == (int)SeedType.SEED_SUNSHROOM)
                {
                    if (plant.state == 23)
                        plantInfoString = "Small ";
                }
                if(plant.plantType == (int)SeedType.SEED_MAGNETSHROOM)
                {
                    if (plant.state == 27)
                        plantInfoString = "Filled ";
                }
                if(plant.hasLadder)
                {
                    plantInfoString += "Laddered ";
                }
                if(plant.plantType == (int)SeedType.SEED_COBCANNON)
                {
                    if (plant.state != 37)
                        plantInfoString = "Charging ";
                    else
                        plantInfoString = "Ready ";
                }
                if(plant.plantType == (int)SeedType.SEED_WALLNUT || plant.plantType == (int)SeedType.SEED_PUMPKINSHELL)
                {
                    string healthState = "Damaged ";
                    if (plant.health > 1333)
                        healthState = "Chipped ";
                    if (plant.health > 2666)
                        healthState = "";
                    plantInfoString += healthState;
                }
                if(plant.plantType == (int)SeedType.SEED_TALLNUT)
                {
                    string healthState = "Crying ";
                    if (plant.health > 2666)
                        healthState = "Chipped ";
                    if (plant.health > 5333)
                        healthState = "";
                    plantInfoString += healthState;
                }
                if(plant.plantType == (int)SeedType.SEED_GARLIC)
                {
                    string healthState = "Sad ";
                    if (plant.health > 133)
                        healthState = "Nibbled ";
                    if (plant.health > 266)
                        healthState = "";
                    plantInfoString += healthState;
                }
                plantInfoString += Consts.plantNames[plant.plantType];
                if (plant.plantType != (int)SeedType.SEED_PUMPKINSHELL && plant.hasPumpkin)
                {
                    string pumpkinState = " damaged ";
                    if (plant.pumpkinHealth > 1333)
                        pumpkinState = " chipped ";
                    if (plant.pumpkinHealth > 2666)
                        pumpkinState = " ";
                    plantInfoString += " with" + pumpkinState + "pumpkin shield";
                }

                if (plant.plantType == (int)SeedType.SEED_MAGNETSHROOM && plant.magItem != 0)
                {
                    switch(plant.magItem)
                    {
                        case 1:
                        case 2:
                        case 3:
                            plantInfoString += " holding bucket";
                            break;
                        case 4:
                        case 5:
                        case 6:
                            plantInfoString += " holding football helmet";
                            break;
                        case 7:
                        case 8:
                        case 9:
                            plantInfoString += " holding screen door";
                            break;
                        case 10:
                        case 11:
                        case 12:
                            plantInfoString += " holding pogo stick";
                            break;
                        case 13:
                            plantInfoString += " holding Jack-in-the-box";
                            break;
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                            plantInfoString += " holding ladder";
                            break;
                        case 21:
                            plantInfoString += " holding pickaxe";
                            break;
                    }
                }

                if (hasCirclePortal)
                    plantInfoString += " and Round portal";
                else if (hasSquarePortal)
                    plantInfoString += " and Square portal";

                if (beepOnFound)
                {
                    Program.PlayTone(leftVol, rightVol, freq, freq, 100, SignalGeneratorType.SawTooth);
                    Program.Vibrate(0.3f, 0.3f, 50);
                }
            }

            return plantInfoString;
        }

        public int GetZombieColumn(float posX)
        {
            int[] tileLimitsFront = new int[] { 70, 130, 217, 297, 367, 485, 535, 627, 720 };
            int[] tileLimitsPool = new int[] { 70, 145, 217, 305, 385, 470, 540, 627, 720 };
            int[] tileLimitsRoof = new int[] { 35, 115, 195, 275, 355, 435, 515, 595, 720 };
            LevelType lvlType = memIO.GetLevelType();
            int[] tileLimits = lvlType is LevelType.Normal or LevelType.Night ? tileLimitsFront : lvlType is LevelType.Roof or LevelType.Boss ? tileLimitsRoof : tileLimitsPool;
            int zombieColumn = 9;
            for (int i = 0; i < tileLimits.Length; i++)
            {
                if (posX <= tileLimits[i])
                {
                    zombieColumn = i;
                    break;
                }
            }

            return zombieColumn;
        }

        string FormatSingleZombieInfo(Zombie zombie, bool includeTileName, ref int prevColumn)
        {
            bool zombossVulnerable = zombie.phase >= 87 && zombie.phase <= 89;
            string zombieName = Consts.zombieNames[zombie.zombieType];
            int zombieNameLen = zombieName.Length;
            string infoPrepend = "";

            int zombieColumn = GetZombieColumn(zombie.posX);

            if (zombie.zombieType == (int)ZombieType.DrZomBoss && zombossVulnerable)
                zombieName = "Zomboss Head";

            if (zombieColumn > prevColumn)
            {
                prevColumn = zombieColumn;
                if (includeTileName)
                {
                    string tileName = ((char)('A' + zombieColumn)).ToString();
                    if (zombieColumn > 8)
                        tileName = "Off-Board";
                    infoPrepend += " " + tileName + ": ";
                }
            }

            if (zombie.hypnotized)
                infoPrepend += "Hypnotized ";

            string addonDescriptor = "";

            if (zombie.helmetState == 1 || zombie.shieldState == 1)
                addonDescriptor = "Dinted ";
            else if (zombie.helmetState == 2 || zombie.shieldState == 2)
                addonDescriptor = "Damaged ";
            else if (zombie.helmetState == 3 || zombie.shieldState == 3)
                addonDescriptor = "Exposed ";

            if (zombie.zombieType == (int)ZombieType.Newspaper)
            {
                if (zombie.shieldState == 1)
                    addonDescriptor = "Ripped ";
                else if (zombie.shieldState == 2)
                    addonDescriptor = "Shredded ";
                else if (zombie.shieldState == 3)
                    addonDescriptor = "Angry ";
            }

            if (zombie.zombieType == (int)ZombieType.WallnutHead || zombie.zombieType == (int)ZombieType.TallnutHead)
            {
                if (zombie.helmetState == 1)
                    addonDescriptor = "Chipped ";
                else if (zombie.helmetState == 2 || zombie.helmetState == 3)
                    addonDescriptor = "Damaged ";
            }

            if (zombie.armless)
                addonDescriptor = "Armless ";
            if (zombie.headless)
                addonDescriptor = "Headless ";

            if (zombie.zombieType == (int)ZombieType.Digger && zombie.holdingSomething)
                infoPrepend += "Underground  ";
            else if (zombie.zombieType == (int)ZombieType.Pogo && !zombie.holdingSomething)
                infoPrepend += "Grounded ";
            else if (zombie.zombieType == (int)ZombieType.PoleVaulting && !zombie.holdingSomething)
                infoPrepend += "Tired ";
            else if (zombie.zombieType == (int)ZombieType.Balloon && zombie.phase == 74)
                infoPrepend += "Falling ";
            else if (zombie.zombieType == (int)ZombieType.Balloon && zombie.phase == 75)
                infoPrepend += "Grounded ";

            if (zombie.frozen)
                infoPrepend += "Icy ";
            if (zombie.buttered)
                infoPrepend += "Buttered ";

            bool zombiquarium = memIO.GetGameMode() == (int)GameMode.Zombiquarium;
            if (zombiquarium && zombie.health <= 150 && zombie.zombieType == (int)ZombieType.Snorkel)
                infoPrepend += "Hungry ";


            return infoPrepend + addonDescriptor + zombieName;
        }

        //TODO: Clean this up. Passing 5 bools to a function is a pretty clear sign that the function needs to be split into different parts.
        public string? GetZombieInfo(bool currentTileOnly = false, bool beepOnFound = true, bool beepOnNone = true, bool includeTileName = true, bool countOnly = false)
        {
            bool zombiquarium = memIO.GetGameMode() == (int)GameMode.Zombiquarium;
            
            List<Zombie> zombies = GetZombies();
            int y = gridInput.cursorY;
            float dirtyOffset = gridInput.cursorX * 5f;

            List<Zombie> zombiesThisRow = new List<Zombie>();

            for (int i = 0; i < zombies.Count; i++)
            {
                if ((!zombiquarium && zombies[i].row == y) || (zombiquarium && (int)zombies[i].posY /100 == y))
                {
                    if(!currentTileOnly)
                        Console.Write("{0} ", zombies[i].posX);
                    if(currentTileOnly && GetZombieColumn(zombies[i].posX) == gridInput.cursorX)
                        zombiesThisRow.Add(zombies[i]);
                    else if(!currentTileOnly)
                        zombiesThisRow.Add(zombies[i]);
                }
            }
            if(!currentTileOnly)
                Console.WriteLine("");

            string verboseZombieInfo = "";

            Fireball? fireball = GetZombossFireballInfo();
            bool needToAddFireball = false;
            if (fireball.HasValue && fireball.Value.row == gridInput.cursorY)
            {
                verboseZombieInfo = (1 + zombiesThisRow.Count).ToString() + ". ";
                needToAddFireball = true;
            }
            else
                verboseZombieInfo = zombiesThisRow.Count.ToString() + ". ";

            if (countOnly)
                return verboseZombieInfo;

            zombiesThisRow.Sort((x, y) => (int)x.posX - (int)y.posX); //Sort by distance (so we can print/inform player in the correct order when speaking)

            int prevColumn = -1;

            List<Program.ToneProperties> tones = new List<Program.ToneProperties>();

            for (int i = 0; i < zombiesThisRow.Count; i++)
            {
                if (needToAddFireball && zombiesThisRow[i].posX > fireball.Value.x)
                {
                    string name = fireball.Value.isIce ? "Ice Ball." : "Fire Ball.";

                    int ballColumn = (int)((fireball.Value.x + 100.0f) / 100.0f);
                    ballColumn = ballColumn < 0 ? 0 : ballColumn;
                    ballColumn = ballColumn > 10 ? 10 : ballColumn;

                    verboseZombieInfo += " ";
                    if (ballColumn > prevColumn)
                    {
                        prevColumn = ballColumn;
                        if (includeTileName)
                            verboseZombieInfo += (char)('A' + ballColumn) + ": ";
                    }
                    verboseZombieInfo += name;

                    needToAddFireball = false;
                }


                //For each zombie, play a sound, with a start delay relative to their distance from left to right
                float rVolume = zombiesThisRow[i].posX / 900.0f;
                float lVolume = 1.0f - rVolume;
                rVolume *= Config.current.ManualZombieSonarVolume;
                lVolume *= Config.current.ManualZombieSonarVolume;
                int startDelay = (int)(zombiesThisRow[i].posX / 2.0f);
                if (startDelay > 1000)
                    continue;
                if (startDelay < 0)
                    startDelay = 0;

                bool zombossVulnerable = zombiesThisRow[i].phase >= 87 && zombiesThisRow[i].phase <= 89;
                if (zombiesThisRow[i].zombieType == (int)ZombieType.DrZomBoss)
                {
                    if(zombossVulnerable)
                        tones.Add(new Program.ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = 300 + (i * 10), endFrequency = 300 + (i * 10), duration = 100, signalType = SignalGeneratorType.Sin, startDelay = startDelay });
                }
                else
                   tones.Add(new Program.ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = 300 + (i*10), endFrequency = 300 + (i*10), duration = 100, signalType = SignalGeneratorType.Sin, startDelay = startDelay });

                if (zombiesThisRow[i].zombieType >= (int)ZombieType.CachedZombieTypes)
                    continue;


                string thisZombieInfo = FormatSingleZombieInfo(zombiesThisRow[i], includeTileName, ref prevColumn);

                verboseZombieInfo += thisZombieInfo + ", ";
            }

            if (beepOnFound)
                Program.PlayTones(tones);

            if (zombiesThisRow.Count == 0)
            {
                if (beepOnNone)
                {
                    Program.PlayTone(Config.current.ManualZombieSonarVolume, Config.current.ManualZombieSonarVolume, 200, 200, 50, SignalGeneratorType.SawTooth);
                    Program.PlayTone(Config.current.ManualZombieSonarVolume, Config.current.ManualZombieSonarVolume, 250, 250, 50, SignalGeneratorType.SawTooth, 55);
                }
                return null;
            }
            else
                return verboseZombieInfo;
        }

        //Sets the amount of sun that has been collected, and is flying to the sun bank
        public void SetAnimatingSunAmount(int sun)
        {
            this.animatingSunAmount = sun;
        }

        public int GetFastZombieCount(ref int lastRow)
        {
            var zombies = GetZombies();
            int poleVaultingCount = 0;
            int footballCount = 0;
            int bobsledCount = 0;
            int youngest = 100000;
            foreach(var zombie in zombies)
            {
                if ((ZombieType)zombie.zombieType is ZombieType.PoleVaulting)
                    poleVaultingCount++;

                if ((ZombieType)zombie.zombieType is ZombieType.Football)
                    footballCount++;

                if ((ZombieType)zombie.zombieType is ZombieType.Bobsled)
                    bobsledCount++;

                if ((ZombieType)zombie.zombieType is ZombieType.Bobsled or ZombieType.PoleVaulting or ZombieType.Football)
                {
                    if(zombie.age < youngest)
                    {
                        youngest = zombie.age;
                        lastRow = zombie.row;
                    }
                }
            }

            return poleVaultingCount + footballCount + bobsledCount;
        }

        void SunWarning(int sunAmount, int sunCost)
        {
            string warning = "Not enough sun! " + sunAmount + " out of " + sunCost;
            Console.WriteLine(warning);
            Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 300, 300, 50, SignalGeneratorType.Square);
            Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 275, 275, 50, SignalGeneratorType.Square, 50);
            Program.Say(warning, true);
        }

        //Don't look at this, please.
        //If you aren't blind already, you may want to gouge your eyeballs out after seeing this.
        bool BeghouledMatchablePlant()
        {
            var allPlants = Program.GetPlantsOnBoard();
            Program.PlantOnBoard thisPlant = new Program.PlantOnBoard();
            thisPlant.plantType = -1;
            foreach(var plant in allPlants)
            {
                if(plant.row == gridInput.cursorY && plant.column == gridInput.cursorX)
                {
                    thisPlant = plant;
                    break;
                }
            }
            if (thisPlant.plantType == -1)
                return false;

            //Sort plants to a 2d array for faster lookups
            Program.PlantOnBoard[,] gridPlants = new Program.PlantOnBoard[5, 8];
            for(int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 8; x++)
                    gridPlants[y, x] = new Program.PlantOnBoard() { plantType = -1 };
            }
            foreach(var plant in allPlants)
                gridPlants[plant.row, plant.column] = plant;

            //Is there a nicer way to do this?
            //I bet there's some super-clean, smart way to do this.
            //But I'm not smart. I'm an ape. It will take me longer to think about an ideal solution than it would take for me to bruteforce this.

            bool dragThisToMatch = false;   //Whether this plant is the one that needs to be moved for a match
            bool partOfMatch = false;   //Whether this plant is part of a match when moving another plant

            //Drag-up matches
            if(thisPlant.row > 0)
            {
                //Drag up to be middle plant in horizontal match
                if (thisPlant.column > 0 && thisPlant.column < 7 && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag up to be left plant in horizontal match
                if (thisPlant.column < 6 && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column + 2].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag up to be right plant in horizontal match
                if (thisPlant.column > 1 && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column - 2].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag up to be bottom of vertical match
                if (thisPlant.row > 2 && gridPlants[thisPlant.row - 2, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 3, thisPlant.column].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
            }
            //Drag-down matches
            if (!dragThisToMatch && thisPlant.row < 4)
            {
                //Drag down to be middle plant in horizontal match
                if (thisPlant.column > 0 && thisPlant.column < 7 && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag down to be left plant in horizontal match
                if (thisPlant.column < 6 && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 2].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag down to be right plant in horizontal match
                if (thisPlant.column > 1 && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column - 2].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag down to be top of vertical match
                if (thisPlant.row < 2 && gridPlants[thisPlant.row + 2, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 3, thisPlant.column].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
            }

            //Drag-left matches
            if (!dragThisToMatch && thisPlant.column > 0)
            {
                //Drag left to be middle plant in vertical match
                if (thisPlant.row > 0 && thisPlant.row < 4 && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag left to be top plant in vertical match
                if (thisPlant.row < 3 && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column - 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag left to be bottom plant in vertical match
                if (thisPlant.row > 1 && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column - 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag left to be right of horizontal match
                if (thisPlant.column > 2 && gridPlants[thisPlant.row, thisPlant.column-2].plantType == thisPlant.plantType && gridPlants[thisPlant.row, thisPlant.column-3].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
            }
            //Drag-right matches
            if (!dragThisToMatch && thisPlant.column < 7)
            {
                //Drag left to be middle plant in vertical match
                if (thisPlant.row > 0 && thisPlant.row < 4 && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag left to be top plant in vertical match
                if (thisPlant.row < 3 && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column + 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag left to be bottom plant in vertical match
                if (thisPlant.row > 1 && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column + 1].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
                //Drag right to be left of horizontal match
                if (thisPlant.column < 5 && gridPlants[thisPlant.row, thisPlant.column + 2].plantType == thisPlant.plantType && gridPlants[thisPlant.row, thisPlant.column + 3].plantType == thisPlant.plantType)
                    dragThisToMatch = true;
            }

            if (dragThisToMatch)
                return true;

            if (Config.current.BeghouledMatchAssist == 1)
                return dragThisToMatch;

            //My god this is bad


            //Check if can be part of horizontal match

            //Check if plant can be right-most in a horizontal three, matching with plant diagonal down-left and left two (diagonal down-left drags up to match) (triangle shape)
            if (thisPlant.column > 1 && thisPlant.row < 4 && gridPlants[thisPlant.row, thisPlant.column - 2].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Same as previous, but diagonal up-left instead of down-left
            if (thisPlant.column > 1 && thisPlant.row > 0 && gridPlants[thisPlant.row, thisPlant.column - 2].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be left-most in a horizontal three, matching with plant diagonal down-right and right two (diagonal down-right drags up to match)
            if (thisPlant.column < 6 && thisPlant.row < 4 && gridPlants[thisPlant.row, thisPlant.column + 2].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Same as previous, but diagonal up-right instead of down-right
            if (thisPlant.column < 6 && thisPlant.row > 0 && gridPlants[thisPlant.row, thisPlant.column + 2].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;

            if (partOfMatch)
                return true;

            //Check if can be leftmost in horizontal three, matching with plant directly to right, and plant x+2 y+1 (L shape)
            if (thisPlant.column < 6 && thisPlant.row < 4 && gridPlants[thisPlant.row, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 2].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if can be leftmost in horizontal three, matching with plant directly to right, and plant x+2 y-1 (L shape)
            if (thisPlant.column < 6 && thisPlant.row > 0 && gridPlants[thisPlant.row, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column + 2].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if can be rightmost in horizontal three, matching with plant directly to left, and plant x-2 y+1 (L shape)
            if (thisPlant.column > 1 && thisPlant.row < 4 && gridPlants[thisPlant.row, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column - 2].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if can be rightmost in horizontal three, matching with plant directly to left, and plant x-2 y-1 (L shape)
            if (thisPlant.column > 1 && thisPlant.row > 0 && gridPlants[thisPlant.row, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column - 2].plantType == thisPlant.plantType)
                partOfMatch = true;

            if (partOfMatch)
                return true;

            //Check if plant can be middle of horizontal three, with same plant directly to left, and another diagonal down-right one.
            if (thisPlant.column > 0 && thisPlant.column < 7 && thisPlant.row < 4 && gridPlants[thisPlant.row, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be middle of horizontal three, with same plant directly to left, and another diagonal up-right one.
            if (thisPlant.column > 0 && thisPlant.column < 7 && thisPlant.row > 0 && gridPlants[thisPlant.row, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be middle of horizontal three, with same plant directly to right, and another diagonal down-left one.
            if (thisPlant.column > 0 && thisPlant.column < 7 && thisPlant.row < 4 && gridPlants[thisPlant.row, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be middle of horizontal three, with same plant directly to right, and another diagonal up-left one.
            if (thisPlant.column > 0 && thisPlant.column < 7 && thisPlant.row > 0 && gridPlants[thisPlant.row, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;

            if (partOfMatch)
                return true;

            //Check if plant can be left of horizontal three, matching with plant directly to the right, and the plant at x+3 (right-most plant slides left)
            if (thisPlant.column < 5 && gridPlants[thisPlant.row, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row, thisPlant.column + 3].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be right of horizontal three, matching with plant directly to the left, and the plant at x-3 (left-most plant slides right)
            if (thisPlant.column > 2 && gridPlants[thisPlant.row, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row, thisPlant.column - 3].plantType == thisPlant.plantType)
                partOfMatch = true;
            //check if plant can be middle of horizontal three, matching with plant directly to left, and plant at x+2 (rightmost plant slides left)
            if (thisPlant.column > 0 && thisPlant.column < 6 && gridPlants[thisPlant.row, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row, thisPlant.column + 2].plantType == thisPlant.plantType)
                partOfMatch = true;
            //check if plant can be middle of horizontal three, matching with plant directly to right, and plant at x-2 (leftmost plant slides right)
            if (thisPlant.column > 1 && thisPlant.column < 7 && gridPlants[thisPlant.row, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row, thisPlant.column - 2].plantType == thisPlant.plantType)
                partOfMatch = true;


            if (partOfMatch)
                return true;

            //check if can be part of vertical match


            //Check if plant can be bottom of vertical three, with same plant two above, and same plant diagonal up-left one (< shape)
            if (thisPlant.column > 0 && thisPlant.row > 1 && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be bottom of vertical three, with same plant two above, and same plant diagonal up-right one (> shape)
            if (thisPlant.column < 7 && thisPlant.row > 1 && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be top of vertical three, with same plant two below, and same plant diagonal down-left one (< shape)
            if (thisPlant.column > 0 && thisPlant.row < 3 && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be top of vertical three, with same plant two below, and same plant diagonal down-right one (> shape)
            if (thisPlant.column < 7 && thisPlant.row < 3 && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;

            if (partOfMatch)
                return true;

            //Pls send help

            //Check if plant can be bottom of vertical three, with same plant directly above, and another at x+1,y+2 (backwards 7 shape)
            if (thisPlant.column < 7 && thisPlant.row > 1 && gridPlants[thisPlant.row - 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be bottom of vertical three, with same plant directly above, and another at x-1,y+2 (7 shape)
            if (thisPlant.column > 0 && thisPlant.row > 1 && gridPlants[thisPlant.row - 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be top of vertical three, with same plant directly below, and another at x+1,y-2 (L shape)
            if (thisPlant.column < 7 && thisPlant.row < 3 && gridPlants[thisPlant.row + 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be top of vertical three, with same plant directly below, and another at x-1,y-2 (backwards L shape)
            if (thisPlant.column > 0 && thisPlant.row < 3 && gridPlants[thisPlant.row + 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;

            if (partOfMatch)
                return true;

            //Check if plant be middle of vertical three, with one directly below, and another diagonal up-left (y-1,x-1)
            if (thisPlant.column > 0 && thisPlant.row > 0 && thisPlant.row < 4 && gridPlants[thisPlant.row + 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant be middle of vertical three, with one directly below, and another diagonal up-right (y-1,x+1)
            if (thisPlant.column < 7 && thisPlant.row > 0 && thisPlant.row < 4 && gridPlants[thisPlant.row + 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant be middle of vertical three, with one directly above, and another diagonal down-left (y+1,x-1)
            if (thisPlant.column > 0 && thisPlant.row > 0 && thisPlant.row < 4 && gridPlants[thisPlant.row - 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column - 1].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant be middle of vertical three, with one directly above, and another diagonal down-right (y+1,x+1)
            if (thisPlant.column < 7 && thisPlant.row > 0 && thisPlant.row < 4 && gridPlants[thisPlant.row - 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 1, thisPlant.column + 1].plantType == thisPlant.plantType)
                partOfMatch = true;

            if (partOfMatch)
                return true;

            //Check if plant can be top of vertical three, with one directly below, and another three below (lowest plant slides up)
            if (thisPlant.row < 2 && gridPlants[thisPlant.row + 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 3, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be bottom of vertical three, with one directly above, and another three above (highest plant slides down)
            if (thisPlant.row > 2 && gridPlants[thisPlant.row - 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 3, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be middle of vertical three, with one directly above, and another two below (lowest plant slides up)
            if (thisPlant.row > 0 && thisPlant.row < 3 && gridPlants[thisPlant.row - 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row + 2, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;
            //Check if plant can be middle of vertical three, with one directly below, and another two above (highest plant slides down)
            if (thisPlant.row < 4 && thisPlant.row > 1 && gridPlants[thisPlant.row + 1, thisPlant.column].plantType == thisPlant.plantType && gridPlants[thisPlant.row - 2, thisPlant.column].plantType == thisPlant.plantType)
                partOfMatch = true;

            //is... is it over?

            return partOfMatch;
        }

        bool Beghouled2MatchablePlant()
        {
            var allPlants = Program.GetPlantsOnBoard();
            Program.PlantOnBoard thisPlant = new Program.PlantOnBoard();
            thisPlant.plantType = -1;
            foreach (var plant in allPlants)
            {
                if (plant.row == gridInput.cursorY && plant.column == gridInput.cursorX)
                {
                    thisPlant = plant;
                    break;
                }
            }
            if (thisPlant.plantType == -1)
                return false;

            //Sort plants to a 2d array for faster lookups
            Program.PlantOnBoard[,] gridPlants = new Program.PlantOnBoard[5, 8];
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 8; x++)
                    gridPlants[y, x] = new Program.PlantOnBoard() { plantType = -1 };
            }
            foreach (var plant in allPlants)
                gridPlants[plant.row, plant.column] = plant;

            //Plants spin clockwise. CLicking bottom-left corner of top-right plant, will spin plant and the one below, down-left, and left of it.
            //Column A plants will need to target column B instead
            //Bottom row will need to target row above it

            //This way we can avoid array bound checking, as we'll always be within bounds (right?)
            int plantX = gridInput.cursorX;
            int plantY = gridInput.cursorY;
            if (plantX == 0)
                plantX = 1;
            if (plantY == 4)
                plantY = 3;

            //Make sure none of the tiles are craters
            if (gridPlants[plantY, plantX].plantType == -1 || gridPlants[plantY, plantX - 1].plantType == -1 || gridPlants[plantY + 1, plantX].plantType == -1 || gridPlants[plantY + 1, plantX - 1].plantType == -1)
                return false;

            //perform rotation operation
            var tempPlant = gridPlants[plantY, plantX];
            gridPlants[plantY, plantX] = gridPlants[plantY, plantX-1];
            gridPlants[plantY, plantX-1] = gridPlants[plantY + 1, plantX - 1];
            gridPlants[plantY + 1, plantX - 1] = gridPlants[plantY+1, plantX];
            gridPlants[plantY + 1, plantX] = tempPlant;

            //Check if the board has any matches
            //Check horizontal matches
            for(int y =0; y < 5; y++)
            {
                for(int x = 0; x < 6; x++)
                {
                    if (gridPlants[y, x].plantType == gridPlants[y, x + 1].plantType && gridPlants[y, x+1].plantType == gridPlants[y, x + 2].plantType && gridPlants[y, x].plantType != -1)
                        return true;
                }
            }

            //Check vertical matches
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (gridPlants[y, x].plantType == gridPlants[y+1,x].plantType && gridPlants[y+1, x].plantType == gridPlants[y+2,x].plantType && gridPlants[y, x].plantType != -1)
                        return true;
                }
            }

            return false;
        }

        public void UpdateFloatingSeedPackets()
        {
            //Temporarily pause game, to avoid issued caused by memory shuffling while processing
            bool wasPaused = memIO.GetBoardPaused();
            memIO.SetBoardPaused(true);

            bool vaseBreakerEndless = memIO.GetGameMode() == (int)GameMode.VaseBreakerEndless;

            //Grab all coins, sunflowers, awards
            int maxCount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",100");

            byte[] coinBytes = memIO.mem.ReadBytes(memIO.ptr.boardChain + ",fc,0", maxCount * 216);           

            floatingPackets = new List<FloatingPacket>();

            for (int i = 0; i < maxCount; i++)
            {
                int index = i * 216;

                int coinTypeNumber = BitConverter.ToInt32(coinBytes, index + 0x58);
                int disappearTime = BitConverter.ToInt32(coinBytes, index + 0x54);

                //Skip inactive clickables
                if (coinBytes[index+0x38] == 1)
                    continue;

                CoinType coinType = (CoinType)coinTypeNumber;
                if (coinType is CoinType.UsableSeedPacket)
                {
                    int newY = 550;
                    string newYStr = newY.ToString();
                    memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x2c).ToString("X2"), "float", "0"); //Xvelocity 0
                    memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x30).ToString("X2"), "float", "0"); //yVelocity 0

                    int packetType = memIO.mem.ReadInt(memIO.ptr.boardChain + ",fc," + (index + 0x68).ToString("X2"));
                    floatingPackets.Add(new FloatingPacket() { packetType = packetType, arrayIndex = i, disappearTime = disappearTime });
                }
            }


            //Sort by disappearTime, to ensure newest packets are to the right
            floatingPackets.Sort((a, b) => b.disappearTime.CompareTo(a.disappearTime));

            int posX = 0;
            int overflowIndex = 9;
            if (vaseBreakerEndless)
            {
                posX = 145;
                overflowIndex = 6;
            }
            for(int i =0; i < floatingPackets.Count; i++)
            {
                if (i == overflowIndex)
                    posX = 0;
                floatingPackets[i] = floatingPackets[i] with { posX = posX, posY = i < overflowIndex ? 8 : 550 };
                string newXStr = posX.ToString();
                string newYStr = floatingPackets[i].posY.ToString();
                int index = floatingPackets[i].arrayIndex * 216;
                memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x24).ToString("X2"), "float", newXStr);   //xPos
                memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x40).ToString("X2"), "float", newXStr);   //collectionXpos

                memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x44).ToString("X2"), "float", newYStr);   //collectionYpos
                memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x48).ToString("X2"), "int", newYStr);     //groundPos
                memIO.mem.WriteMemory(memIO.ptr.boardChain + ",fc," + (index + 0x28).ToString("X2"), "float", newYStr);   //yPos 
                posX += 50;
            }

            memIO.SetBoardPaused(wasPaused);
        }

        int GetVaseCount()
        {
            int vaseCount = 0;
            var items = Program.GetGridItems();
            foreach (var item in items)
                vaseCount += item.type == (int)GridItemType.Vase ? 1 : 0;
            return vaseCount;
        }

        void SayPlantSlotInfo(InputIntent intent, List<plantInBoardBank> plants)
        {
            string plantInfo = "";
            string plantName = Consts.plantNames[plants[seedbankSlot].packetType];
            string plantState = "";
            bool isConveyor = ConveyorBeltCounter() > 0;
            bool ready = PlantPacketReady();
            bool canAfford = true;
            bool isImitater = plants[seedbankSlot].packetType == (int)SeedType.SEED_IMITATER;
            if (isImitater)
                plantName = "Imitation " + Consts.plantNames[plants[seedbankSlot].imitaterType];
            if (ready && !isConveyor)
                plantState = "Ready";
            else if (!isConveyor)
            {
                bool refreshing = plants[seedbankSlot].isRefreshing;
                int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");
                sunAmount += animatingSunAmount;
                if (refreshing)
                    plantState = "Refreshing";
                else
                {
                    plantState = sunAmount + " of " + Consts.plantCosts[plants[seedbankSlot].packetType] + " sun";
                    canAfford = false;
                }
            }
            string plantSun = "";
            if (!isConveyor)
                plantSun = Consts.plantCosts[plants[seedbankSlot].packetType] + " sun";

            if (inputRepeatCount == 1 && intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10)
                plantInfo = plantState + ", " + plantName + (canAfford ? ", " + plantSun : "");
            else if (inputRepeatCount == 2 && intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10)
                plantInfo = plantSun + ", " + plantName + ", " + plantState;
            else
                plantInfo = plantName + ", " + plantState + (canAfford ? ", " + plantSun : "");

            if (isConveyor)
                plantInfo = plantInfo.Replace(", ", "");

            Console.WriteLine(plantInfo);
            Program.Say(plantInfo);
        }

        public override void Interact(InputIntent intent)
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= inputRepeatTimer)
            {
                if (lastIntent != intent)
                    inputRepeatCount = 0;
                else
                    inputRepeatCount++;

                inputRepeatCount %= 3;

                lastIntent = intent;
            }
            else
                inputRepeatCount = 0;

            inputRepeatTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + inputRepeatCooldown;

            UpdateFloatingSeedPackets();

            if(intent == InputIntent.Option)
            {
                if(memIO.GetGameMode() == (int)GameMode.LastStand)
                {
                    //start/continue onslaught button
                    bool buttonVisible = memIO.mem.ReadByte(memIO.ptr.boardChain + memIO.ptr.lastStandButtonVisible) != 1;
                    if(buttonVisible)
                    {
                        memIO.SetBoardPaused(false);
                        Program.Click(0.5f, 0.98f);
                        return;
                    }
                }
                
                bool mPaused = !memIO.GetBoardPaused();
                memIO.SetBoardPaused(mPaused);

                if (mPaused)
                {
                    Console.WriteLine("Frozen");
                    Program.Say("Frozen", true);
                }
                else
                {
                    Console.WriteLine("UnFrozen");
                    Program.Say("UnFrozen", true);
                }

            }

            //Click Menu button
            if(intent == InputIntent.Start)
                Program.Click(0.95f, 0.05f);

            GameMode gameMode = (GameMode)memIO.GetGameMode();
            bool inBeghouled = gameMode == GameMode.Beghouled;
            bool inBeghouled2 = gameMode == GameMode.BeghouledTwist;
            bool inVaseBreaker = VaseBreakerCheck();
            bool inRainingSeeds = gameMode == GameMode.ItsRainingSeeds;
            bool inWhackAZombie = gameMode == GameMode.WhackAZombie || memIO.GetPlayerLevel() == 15;
            bool inIZombie = gameMode >= GameMode.IZombie1 && gameMode <= GameMode.IZombieEndless;
            bool inSlotMachine = gameMode == GameMode.SlotMachine;
            bool vaseBreakerEndless = gameMode is GameMode.VaseBreakerEndless;

            //TODO: move to memIO/Pointers
            int seedbankSize = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",868,15c,24") - 1;  //10 seeds have max index of 9
            int maxConveryorBeltIndex = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",868,15c,34c") - 1;
            if (inVaseBreaker || inRainingSeeds || inSlotMachine)
                seedbankSize = floatingPackets.Count - 1;
            if (vaseBreakerEndless)
                seedbankSize++;

            int prevX = gridInput.cursorX;
            int prevY = gridInput.cursorY;
            gridInput.Interact(intent);

            int minY = 0;
            int maxY = 0;
            Program.GetMinMaxY(ref minY, ref maxY);


            //Zombie info hotkey
            if (intent == InputIntent.Info1)
            {
                string? zombiesThisRow = GetZombieInfo();
                if(zombiesThisRow == null)
                    zombiesThisRow = "No Zombies";

                Console.WriteLine(zombiesThisRow);
                Program.Say(zombiesThisRow);
            }

            var plants = GetPlantsInBoardBank();

            if (prevX != gridInput.cursorX || prevY != gridInput.cursorY)
            {
                int minX = 0;
                int maxX = 8;

                //If in wallnut bowling
                bool inBowling = gameMode == GameMode.WallnutBowling || gameMode == GameMode.WallnutBowling2;
                inBowling |= memIO.GetPlayerLevel() == 5 && ConveyorBeltCounter() > 0; //converyorBeltCounter >0, means we're passed the peashooter-shovelling part.
                if (inBowling)
                {
                    if (!doneBowlingTutorial && Config.current.GameplayTutorial && gameMode != GameMode.WallnutBowling2)
                    {
                        Program.GameplayTutorial(new string[] { "This is wall-nut bowling.", "Your deck at the top has been replaced with a conveyor belt.", "As you play, plants will arrive and build up on the conveyor belt.", "When you place a plant, it will be removed from the belt." });
                        Program.GameplayTutorial(new string[] { "You will be confined to the left three columns, and you will start receiving wall-nuts on the conveyor belt.", "When you place a wall-nut, it will roll from where you place it, to the right of the screen.", "Zombies will arrive as normal, and you must bowl them over to defend your house." });
                        Program.GameplayTutorial(new string[] { "When a wallnut rolls into a zombie, it will ricochet and begin moving diagonally up-right or down-right.", "You will also occasionally receive explode-o-nuts, which will also roll, but will explode like a cherry bomb when they hit a zombie." });
                        doneBowlingTutorial = true;
                    }
                    maxX = 2;
                }

                //bool inIZombie = gameMode >= (int)GameMode.PUZZLE_I_ZOMBIE_1 && gameMode <= (int)GameMode.PUZZLE_I_ZOMBIE_ENDLESS;
                //if (inIZombie)
                //  minX = 4;

                bool inZomboss = gameMode == GameMode.DrZombossRevenge || (gameMode == GameMode.Adventure && memIO.GetPlayerLevel() == 50);
                if (inZomboss)
                    maxX = 7;

                //We don't shrink the board, because we want audio cues to remain the same for the same level types
                gridInput.cursorX = gridInput.cursorX < minX ? minX : gridInput.cursorX;
                gridInput.cursorX = gridInput.cursorX > maxX ? maxX : gridInput.cursorX;
                gridInput.cursorY = gridInput.cursorY < minY ? minY : gridInput.cursorY;
                gridInput.cursorY = gridInput.cursorY > maxY ? maxY : gridInput.cursorY;

                string totalTileInfoStr = "";

                if (prevX == gridInput.cursorX && prevY == gridInput.cursorY)
                    Program.PlayBoundaryTone();
                else
                {
                    float rightVol = (float)gridInput.cursorX / (float)gridInput.width;
                    float leftVol = 1.0f - rightVol;
                    float freq = 1000.0f - ((gridInput.cursorY * 500.0f) / (float)gridInput.height);

                    bool plantFound = false;
                    if ((Config.current.SayPlantOnTileMove || Config.current.FoundObjectCueVolume > 0) && !inBowling)
                    {
                        //Say plant at current tile
                        string? tileObjectInfo = GetCurrentTileObject(false, Config.current.FoundObjectCueVolume > 0, false);
                        plantFound = tileObjectInfo != null;
                        if (tileObjectInfo is not null && Config.current.SayPlantOnTileMove)
                            totalTileInfoStr += tileObjectInfo;
                    }

                    if (inBeghouled && Config.current.BeghouledMatchAssist != 0)
                    {
                        //OOOOHHH boy, this will be crazy
                        bool matchable = BeghouledMatchablePlant();
                        if (matchable)
                            Program.PlayBeghouledAssistTone();
                    }

                    if(inBeghouled2 && Config.current.Beghouled2MatchAssist)
                    {
                        bool matchable = Beghouled2MatchablePlant();
                        if (matchable)
                            Program.PlayBeghouledAssistTone();
                    }

                    bool zombieFound = false;
                    if (Config.current.SayZombieOnTileMove || Config.current.ZombieOnTileVolume > 0)
                    {
                        string? zombiesThisTile = GetZombieInfo(true, false, false, false);
                        if (zombiesThisTile != null)
                        {
                            zombieFound = true;

                            if (Config.current.SayZombieOnTileMove)
                                totalTileInfoStr += " " + zombiesThisTile;

                            if (Config.current.ZombieOnTileVolume > 0)
                            {
                                //Try getting the count from the string
                                int count = zombiesThisTile[0] - '0';
                                float zombieFreq = freq - 30;
                                if (count >= 0 && count < 10)
                                {
                                    float lZombieVol = leftVol * Config.current.ZombieOnTileVolume;
                                    float rZombieVol = rightVol * Config.current.ZombieOnTileVolume;
                                    List<Program.ToneProperties> tones = new List<Program.ToneProperties>();
                                    for (int i = 0; i < count; i++)
                                        tones.Add(new Program.ToneProperties() { leftVolume = lZombieVol, rightVolume = rZombieVol, startFrequency = zombieFreq - (50 * i), endFrequency = zombieFreq - (50 * (i + 1)), duration = 80, signalType = SignalGeneratorType.Square, startDelay = 100 * i });
                                    Program.PlayTones(tones);
                                }
                            }
                        }
                    }

                    if (Config.current.ZombieSonarOnRowChange > 0 && prevY != gridInput.cursorY)
                    {
                        string? zombiesThisRow = GetZombieInfo(false, (Config.current.ZombieSonarOnRowChange == 1 || Config.current.ZombieSonarOnRowChange == 2), Config.current.ZombieSonarOnRowChange == 1 || Config.current.ZombieSonarOnRowChange == 2, Config.current.ZombieSonarOnRowChange == 1, Config.current.ZombieSonarOnRowChange == 3);
                        if (zombiesThisRow == null)
                            zombiesThisRow = "No Zombies";

                        if (Config.current.ZombieSonarOnRowChange == 1 || Config.current.ZombieSonarOnRowChange == 3)
                            totalTileInfoStr += " " + zombiesThisRow;
                    }



                    if ((!plantFound || Config.current.FoundObjectCueVolume == 0) && !zombieFound)
                    {
                        float lGridVol = leftVol * Config.current.GridPositionCueVolume;
                        float rGridVol = rightVol * Config.current.GridPositionCueVolume;
                        Program.PlayTone(lGridVol, rGridVol, freq, freq, 100, SignalGeneratorType.Sin);
                    }
                }

                if (Config.current.SayTilePosOnMove)
                {
                    string tilePos = string.Format("{0}-{1}", (char)(gridInput.cursorX + 'A'), gridInput.cursorY + 1);
                    totalTileInfoStr += " " + tilePos;
                }

                if (totalTileInfoStr.Length > 0)
                {
                    Console.WriteLine(totalTileInfoStr);
                    Program.Say(totalTileInfoStr);
                }

                //Move mouse cursor to aid sighted players in knowing where their cursor is located visually
                MoveMouseToTile();
            }
            else if (intent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                Program.PlayBoundaryTone();

            bool inZombiquarium = gameMode == GameMode.Zombiquarium;
            //Dirty hack to allow scrolling to empty seedbank slot in zombiequarium (so placing brains can be a seedbank option)
            if (inZombiquarium)
            {
                seedbankSize = 3;
                plants[2] = plants[2] with { packetType = 0 };
            }

            int lastPlantSlot = seedbankSlot;
            if (intent == InputIntent.CycleLeft)
                seedbankSlot--;
            if (intent == InputIntent.CycleRight)
                seedbankSlot++;

            if (intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10)
                seedbankSlot = intent - InputIntent.Slot1;

            if (Config.current.WrapPlantSelection)
            {
                seedbankSlot = seedbankSlot < 0 ? seedbankSize : seedbankSlot;
                seedbankSlot = seedbankSlot > seedbankSize ? 0 : seedbankSlot;
            }
            else
            {
                seedbankSlot = seedbankSlot < 0 ? 0 : seedbankSlot;
                seedbankSlot = seedbankSlot > seedbankSize ? seedbankSize : seedbankSlot;
            }

            bool shiftedBack = false;
            //Make sure we can't select invalid slots on conveyor levels
            for (int i = 0; i < seedbankSize; i++)
            {
                if (inVaseBreaker | inSlotMachine | inRainingSeeds)
                    break;
                if (seedbankSlot < 0)
                    break;
                if (plants[seedbankSlot].packetType == -1)
                {
                    seedbankSlot--;
                    shiftedBack = true;
                }
            }
            if (shiftedBack && intent is InputIntent.CycleRight && Config.current.WrapPlantSelection)
                seedbankSlot = 0;

            bool hasSeeds = seedbankSlot >= 0;
            seedbankSlot = seedbankSlot < 0 ? 0 : seedbankSlot; //cap index min to 0 again

            if (intent is InputIntent.ZombieMinus or InputIntent.ZombiePlus)
            {
                Zombie? nullableZombie = CycleZombie(intent);
                if (nullableZombie is null)
                    Program.PlayBoundaryTone();
                else
                {
                    Zombie cycleZombie = nullableZombie.Value;

                    float rVolume = cycleZombie.posX / 900.0f;
                    float lVolume = 1.0f - rVolume;
                    rVolume *= Config.current.GridPositionCueVolume;
                    lVolume *= Config.current.GridPositionCueVolume;
                    int startDelay = (int)(cycleZombie.posX / 2.0f);
                    float freq = 1000.0f - ((cycleZombie.row * 500.0f) / (float)gridInput.height);

                    if (cycleZombie.zombieType == (int)ZombieType.DrZomBoss)
                    {
                        bool zombossVulnerable = cycleZombie.phase >= 87 && cycleZombie.phase <= 89;
                        if (zombossVulnerable)
                            Program.PlayTone(lVolume, rVolume, freq, freq, 100, SignalGeneratorType.Sin, 0);
                    }
                    else
                        Program.PlayTone(lVolume, rVolume, freq, freq, 100, SignalGeneratorType.Sin, 0);

                    int zombieColumn = GetZombieColumn(cycleZombie.posX);
                    string tileName = ((char)('A' + zombieColumn)).ToString();
                    if (zombieColumn > 8)
                        tileName = "Off-Board";

                    tileName += " " + (cycleZombie.row + 1) + ", ";

                    int prevColumn = -1;
                    string zombieInfoStr = tileName + FormatSingleZombieInfo(cycleZombie, false, ref prevColumn);
                    Console.WriteLine(zombieInfoStr);
                    Program.Say(zombieInfoStr);

                    if (Config.current.MoveOnZombieCycle)
                    {
                        gridInput.cursorX = zombieColumn;
                        gridInput.cursorY = cycleZombie.row;
                        MoveMouseToTile();
                    }
                }
            }

            

            bool cycleInputIntent = intent == InputIntent.CycleLeft || intent == InputIntent.CycleRight || (intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10);

            //If user tries to switch seeds while holding one in vasebreaker or rainingSeeds, inform them of already held plant.
            //Otherwise, inform them of plant in newly switched slot
            if (cycleInputIntent && (inVaseBreaker || inRainingSeeds || inSlotMachine))
            {
                if(vaseBreakerEndless && seedbankSlot == 0)
                {
                    Program.PlaySlotTone(seedbankSlot, seedbankSize);
                    PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, true, true); //Move mouse cursor to aid sighted players in knowing which seed packet is selected

                    SayPlantSlotInfo(intent, plants);
                }
                else if (floatingPackets.Count > 0)
                {
                    Program.PlaySlotTone(seedbankSlot,seedbankSize);

                    Program.MoveMouse((floatingPackets[seedbankSlot- (vaseBreakerEndless ? 1 : 0)].posX + 25) / 800.0f, (floatingPackets[seedbankSlot - (vaseBreakerEndless ? 1 : 0)].posY + 50) / 600.0f);
                    int heldPlantID = floatingPackets[seedbankSlot - (vaseBreakerEndless ? 1 : 0)].packetType;
                    string plantStr = Consts.plantNames[heldPlantID];
                    Console.WriteLine(plantStr);
                    Program.Say(plantStr, true);
                }
                else
                    Program.PlayBoundaryTone();
            }
            else if(cycleInputIntent)
            {
                PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, true, true); //Move mouse cursor to aid sighted players in knowing which seed packet is selected
                Program.PlaySlotTone(seedbankSlot, seedbankSize);

                if (inZombiquarium)
                {
                    string functionString = "";
                    switch (seedbankSlot)
                    {
                        case 0:
                            functionString = "Buy snorkel zombie.";
                            break;
                        case 1:
                            functionString = "Buy trophy.";
                            break;
                        case 2:
                            functionString = "Brain food.";
                            break;
                    }

                    Console.WriteLine(functionString);
                    Program.Say(functionString);
                }
                else if(inBeghouled || inBeghouled2)
                {
                    string functionString = "";
                    if (!plants[seedbankSlot].active)
                        functionString = "Purchased. ";
                    switch (seedbankSlot)
                    {
                        
                        case 0:
                            functionString += " Upgrade peashooters to repeaters.";
                            break;
                        case 1:
                            functionString = " Upgrade puff-shrooms to fume-shrooms.";
                            break;
                        case 2:
                            functionString = " Upgrade wall-nuts to tall-nuts.";
                            break;
                        case 3:
                            functionString = " Shuffle plants.";
                            break;
                        case 4:
                            functionString = " Repair crater.";
                            break;
                    }
                    if (seedbankSlot == 0 && plants[0].packetType == -1)
                        functionString = "Collect sun to unlock options";

                    Console.WriteLine(functionString);
                    Program.Say(functionString);
                }
                else
                {
                    if (plants.Count > 0)
                    {
                        int packetType = plants[seedbankSlot].packetType;
                        if (packetType >= 0 && packetType < (int)SeedType.NUM_SEED_TYPES)
                        {
                            SayPlantSlotInfo(intent, plants);
                        }
                        else if (packetType >= 60 && packetType <= 74)
                        {
                            //iZombie levels
                            string zombieInfo = Consts.zombieNames[Consts.iZombieNameIndex[packetType - 60]] + " : " + Consts.iZombieSunCosts[packetType - 60] + " sun";
                            Console.WriteLine(zombieInfo);
                            Program.Say(zombieInfo, true);
                        }
                    }
                }

            }

            //Place plant
            if(intent == InputIntent.Confirm && (inRainingSeeds || inVaseBreaker || inSlotMachine || plants[seedbankSlot].packetType >= 0))
            {
                bool isCobCannon = Program.GetPlantAtCell(gridInput.cursorX, gridInput.cursorY).plantType == (int)SeedType.SEED_COBCANNON;
                isCobCannon |= Program.GetCursorType() == 8;

                int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");
                sunAmount += animatingSunAmount;

                //Click where plant needs to go. Not where plant is located (we already grab plant when auto-collecting everything on screen)
                if (inVaseBreaker || inRainingSeeds || isCobCannon || inSlotMachine)
                {
                    bool isFrozen = memIO.GetBoardPaused();
                    bool hasVase = CheckVaseAtTile();
                    if (hasVase)
                    {
                        memIO.SetBoardPaused(false);
                        PlacePlant(0, 0, 0, false, false, false, false);
                        Task.Delay(200).Wait();  //Wait for vase to break
                    }
                    else if (vaseBreakerEndless && seedbankSlot == 0)
                    {
                        //Check if there's enough sun, and plant isn't on cooldown
                        int sunCost = Consts.plantCosts[plants[seedbankSlot].packetType];

                        bool notEnoughSun = sunAmount < sunCost;
                        bool refreshing = plants[seedbankSlot].isRefreshing;

                        if (notEnoughSun)
                            SunWarning(sunAmount, sunCost);
                        else if (refreshing)
                        {
                            string warning = (((float)plants[seedbankSlot].refreshCounter / (float)plants[seedbankSlot].refreshTime) * 99.9f).ToString("0.") + "% refreshed";
                            Console.WriteLine(warning);
                            Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 250, 250, 50, SignalGeneratorType.Square);
                            Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 275, 275, 50, SignalGeneratorType.Square, 50);
                            Program.Say(warning, true);
                        }
                        else
                            PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, true, false, false);
                    }
                    else if (floatingPackets.Count > 0)
                    {
                        memIO.SetBoardPaused(false);
                        Program.Click((floatingPackets[seedbankSlot - (vaseBreakerEndless ? 1 : 0)].posX + 25) / 800.0f, (floatingPackets[seedbankSlot - (vaseBreakerEndless ? 1 : 0)].posY + 25) / 600.0f, false, false, 50, true);
                        PlacePlant(seedbankSlot, seedbankSize, 0, false, false, false, false);
                    }
                    memIO.SetBoardPaused(isFrozen);
                    UpdateFloatingSeedPackets();
                }
                else if(inZombiquarium)
                {
                    int sunCost = seedbankSlot == 0 ? 100 : seedbankSlot == 1 ? 1000 : 5;

                    if(sunAmount < sunCost)
                        SunWarning(sunAmount, sunCost);
                    else if (seedbankSlot == 2)
                        PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, false, false, false);
                    else
                        PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, true, false, false, false, true);


                }
                else if (inBeghouled || inBeghouled2)
                {
                    int sunCost = seedbankSlot == 0 ? 1000 : seedbankSlot == 1 ? 500 : seedbankSlot == 2 ? 250 : seedbankSlot == 3 ? 100 : 200;
                    bool purchased = !plants[seedbankSlot].active;
                    if (purchased)
                    {
                        Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 200, 200, 50, SignalGeneratorType.Square);
                        Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 200, 200, 50, SignalGeneratorType.Square, 100);
                        string info = "Already purchased.";
                        Console.WriteLine(info);
                        Program.Say(info);
                    }
                    else if (sunAmount < sunCost)
                        SunWarning(sunAmount, sunCost);
                    else
                        PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, true, false, false, false, true);
                }
                else if (plants[seedbankSlot].absX < 0.72f)
                {
                    //Check if there's enough sun, and plant isn't on cooldown
                    int sunCost = inIZombie? Consts.iZombieSunCosts[plants[seedbankSlot].packetType - 60] : Consts.plantCosts[plants[seedbankSlot].packetType];

                    bool notEnoughSun = sunAmount < sunCost;
                    bool refreshing = plants[seedbankSlot].isRefreshing;
                    bool isConveyorLevel = ConveyorBeltCounter() > 0;


                    if (notEnoughSun && !isConveyorLevel && !inIZombie)
                        SunWarning(sunAmount, sunCost);
                    else if (refreshing)
                    {
                        string warning = (((float)plants[seedbankSlot].refreshCounter / (float)plants[seedbankSlot].refreshTime) * 99.9f).ToString("0.") + "% refreshed";
                        Console.WriteLine(warning);
                        Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 250, 250, 50, SignalGeneratorType.Square);
                        Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 275, 275, 50, SignalGeneratorType.Square, 50);
                        Program.Say(warning, true);
                    }
                    else if (isConveyorLevel && !hasSeeds)
                    {
                        string warning = "Waiting for plants to arrive";
                        Console.WriteLine(warning);
                        Program.Say(warning, true);
                    }
                    else
                        PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, true, false, false);
                }
            }

            if (intent == InputIntent.Deny)
            {
                if (inBeghouled)
                {
                    var newIntent = Program.input.GetCurrentIntent();
                    while (newIntent is InputIntent.None)
                        newIntent = Program.input.GetCurrentIntent();
                    if (newIntent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                        DragPlant(newIntent);
                }
                else if (inBeghouled2)
                {
                    int plantX = gridInput.cursorX;
                    int plantY = gridInput.cursorY;
                    if (plantX == 0)
                        plantX = 1;
                    if (plantY == 4)
                        plantY = 3;
                    Vector2 cellPos = GetBoardCellPosition(plantX,plantY);
                    cellPos.X -= 0.035f;
                    cellPos.Y += 0.03f;
                    bool wasPaused = memIO.GetBoardPaused();
                    memIO.SetBoardPaused(false);
                    Program.Click(cellPos.X,cellPos.Y,false,false,50,true);
                    memIO.SetBoardPaused(wasPaused);
                }
                else if (inWhackAZombie)
                    PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, false, false);
                else if (inRainingSeeds || inVaseBreaker || inSlotMachine)
                    ShovelPlant(5);
                else if (Config.current.RequireShovelConfirmation)
                {
                    if (shovelPressedLast)
                    {
                        ShovelPlant(seedbankSize);
                        shovelPressedLast = false;  //Set to false, so pressing shovel three times won't trigger two shovel events (accidentally shoveling lillypad/potplant).
                    }
                    else
                        shovelPressedLast = true;
                }
                else
                    ShovelPlant(seedbankSize);

            }
            else
                shovelPressedLast = false;

            if(intent == InputIntent.Info4)
            {
                //For debugging purposes, info4 will instantly finish the level

                //memIO.SetPlayerCoinCount(1000);
                //Program.Debug_FinishLevel();
                //return;

                string info4String = "";

                var lawnMowers = GetLawnMowers(true);

                float freq = 1000.0f - ((gridInput.cursorY * 500.0f) / (float)gridInput.height);

                if (lawnMowers.Count > 0)
                {
                    if(Config.current.SayLawnmowerType)
                        info4String += lawnMowers[0].mowerType.ToString();
                    Program.PlayTone(Config.current.MiscAlertCueVolume, 0, freq, freq, 100, SignalGeneratorType.Square, 0);
                }

                if(getIzombieBrainCount(true) > 0)
                    Program.PlayTone(Config.current.MiscAlertCueVolume, 0, freq, freq, 100, SignalGeneratorType.Square, 0);

                //GetZombossHealth
                bool zomBossMinigame = memIO.GetGameMode() == (int)GameMode.DrZombossRevenge;

                if ((memIO.GetPlayerLevel() == 50 && memIO.GetGameMode() == (int)GameMode.Adventure) || zomBossMinigame)
                {
                    int zombossHealth = GetZombossHealth();
                    float percentage = (float)zombossHealth / (zomBossMinigame ? 60000.0f : 40000.0f);
                    percentage = 1.0f - percentage;
                    percentage *= 100.0f;
                    string percentStr = percentage.ToString("0") + "% complete.";
                    info4String += " " + percentStr;
                }
                else if(inIZombie)
                {
                    info4String += 5-getIzombieBrainCount() + " of 5 brains eaten";
                }
                else if(inSlotMachine)
                {
                    int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");
                    sunAmount += animatingSunAmount;
                    info4String += " " + Program.FormatNumber(sunAmount) + " of 2,000 sun";
                }
                else if (inBeghouled || inBeghouled2)
                {
                    int matches = memIO.mem.ReadInt(memIO.ptr.boardChain + ",178,60");
                    info4String += " " + matches + " of 75 matches.";
                }
                else if (gameMode == GameMode.SeeingStars)
                {
                    var boardPlants = Program.GetPlantsOnBoard();
                    bool[] placedStars = Consts.SeeingStars.ToArray();
                    foreach(var boardPlant in boardPlants)
                    {
                        if(boardPlant.plantType == (int)SeedType.SEED_STARFRUIT)
                            placedStars[boardPlant.row * 9 + boardPlant.column] = false;
                    }
                    int remainingStars = 0;
                    foreach(bool b in placedStars)
                    {
                        if (b)
                            remainingStars++;
                    }

                    info4String = 14 - remainingStars + " of 14 required stars";

                }
                else if (inVaseBreaker)
                {
                    int vaseCount = GetVaseCount();
                    info4String = (vaseCount > 0 ? vaseCount : "no") + " vase" + (vaseCount != 1 ? "s" : "") + " remaining";
                }
                else if(gameMode == GameMode.LastStand || IsSurvival())
                {
                    int stageCount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",178,6c");
                    int maxStages = 0;
                    if (gameMode == GameMode.LastStand)
                        maxStages = 5;
                    else if (gameMode >= GameMode.SurvivalDay && gameMode <= GameMode.SurvivalRoof)
                        maxStages = 5;
                    else if (gameMode >= GameMode.SurvivalHardDay && gameMode <= GameMode.SurvivalHardRoof)
                        maxStages = 10;

                    string roundCountString = "";
                    if (maxStages == 0)
                        roundCountString = stageCount + " stages completed";
                    else
                        roundCountString = stageCount + " of " + maxStages + " stages completed";
                    info4String = GetWaveInfo() + ", " + roundCountString;
                }
                else if (gameMode == GameMode.Zombiquarium)
                    info4String = "Save up 1000 sun to purchase the level trophy";
                else
                {
                    string waveInfo = GetWaveInfo();
                    info4String += " " + waveInfo;
                }

                Console.WriteLine(info4String);
                Program.Say(info4String, true);
            }

            //Get plant info for this cell
            //Eg; Grass, Water, RoofTile, Lilypad, Flowerpot, Peashooter, Sunflower with pumpkin,
            if (intent == InputIntent.Info2)
            {


                //string totalString = plantInfoString;
                string totalString = GetCurrentTileObject();
                Console.WriteLine(totalString);
                Program.Say(totalString, true);
            }

            if(intent == InputIntent.Info3)
            {
                if(inSlotMachine)
                {
                    //bool slotReady = memIO.mem.ReadInt(memIO.ptr.boardChain + ",178,54") == 0;
                    //if (slotReady)
                    Program.Click(0.62f, 0.1f);
                    return;
                }
                int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");
                sunAmount += animatingSunAmount;
                string sunString = Program.FormatNumber(sunAmount) + " sun.";
                Console.WriteLine(sunString);
                Program.Say(sunString, true);
            }
        }
    }
}
