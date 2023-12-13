using Memory;
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
        GridInput gridInput;
        public int seedbankSlot;
        int heldPlantID;    //For VaseBreaker/ItsRainingSeeds
        bool shovelPressedLast; //Whether the shovel was the last input (required for shovel confirmation mode)
        int animatingSunAmount;

        public struct Zombie
        {
            public int zombieType;
            public int phase;
            public float posX;
            public float posY;
            public int row;    //From GameObject
            public int health;
            public int maxHealth;
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
        }

        public Board(MemoryIO memIO, string pointerChain = "") : base(memIO, pointerChain)
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

            if (memIO.GetAdventureCompletions() < 1)
            {
                if (memIO.GetPlayerLevel() == 0)
                    height = 1;
            }
            

            gridInput = new GridInput(width, height, false);

            if(Config.current.GameplayTutorial)
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

        public List<Zombie> GetZombies(bool seedPicker = false)
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

                zombie.health = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xc8).ToString("X2"));
                zombie.maxHealth = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0xcc).ToString("X2"));

                zombie.phase = memIO.mem.ReadInt(memIO.ptr.boardChain + ",a8," + (index + 0x28).ToString("X2"));
                zombie.posX = memIO.mem.ReadFloat(memIO.ptr.boardChain + ",a8," + (index + 0x2c).ToString("X2"));

                //Zomboss should be detected on the right
                if (zombie.zombieType == (int)ZombieType.DrZomBoss)
                    zombie.posX = 799;

                //Skip off-screen zombies
                if (zombie.posX > 800 && !seedPicker)
                    continue;

                zombie.posY = memIO.mem.ReadFloat(memIO.ptr.boardChain + ",a8," + (index + 0x30).ToString("X2"));

                zombies.Add(zombie);
                addedZombies++;

                //Zomboss is in all rows
                if (zombie.zombieType == (int)ZombieType.DrZomBoss)
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

        Vector2 GetBoardCellPosition()
        {
            LevelType levelType = memIO.GetLevelType();

            float yOffset = 0.16f;


            if (levelType == LevelType.Pool || levelType == LevelType.PoolNight)
                yOffset = 0.145f;
            //yOffset = 0.13f;

            if (levelType == LevelType.Roof || levelType == LevelType.Boss)
                yOffset = 0.145f;

            float cellX = 0.09f + (gridInput.cursorX * 0.1f);
            float cellY = 0.22f + (gridInput.cursorY * yOffset);


            if (levelType == LevelType.Roof || levelType == LevelType.Boss)
                cellY += 0.04f * (4.0f - MathF.Min(gridInput.cursorX, 4.0f));

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
            List<plantInBoardBank> newPlants = new List<plantInBoardBank>();

            byte[] plantBytes = memIO.mem.ReadBytes(memIO.ptr.lawnAppPtr + ",868,15c,28", 800);    //yucky

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

                //Console.WriteLine(plant.offsetX);
                //Console.WriteLine(plant.packetType);

                if (plant.offsetX <= 0)
                    stoppedPlants++;

                //Console.WriteLine("Plant absX: " + plant.absX);

                if (plant.absX < 0.72f)
                    //if (plant.offsetX < maxX - (stoppedPlants*50))
                    newPlants.Add(plant);

                //Console.WriteLine("i: {0}, Index: {1}, Type: {2}", i, plants[i].index, plants[i].packetType);
            }

            //Fill rest of list with empty plants
            while (newPlants.Count < 10)
                newPlants.Add(new plantInBoardBank() { packetType = -1 });

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


            string info = "";

            info = (((float)currentWave / (float)numWaves) * 100.0f).ToString("0") + "% complete";
            if (currentWave == numWaves)
                info = "Final Wave";

            /*
            if (requestedInfo.percentageThroughLevel)
                info += ((numWaves / currentWave) * 100.0f).ToString() + "%" + (requestedInfo.verboseText ? "complete" : "") + "\r\n";
            if (requestedInfo.currentWave)
                info += (requestedInfo.verboseText ? "Wave " : "") + currentWave.ToString() + "\r\n";
            if (requestedInfo.wavesRemaining)
                info += numWaves - currentWave + (requestedInfo.verboseText ? " waves remaining" : "") + "\r\n";
            */

            return info;

        }

        //TODO: Split code into method which obtains cell position, one that obtains seedbank/conveyor pos, one that clicks, one that moves the mouse.
        //Can't we just grab the packet position from memory? I wrote this near the start of the project, so don't remember if I tried, or just started with a hacky solution.
        void PlacePlant(int seedbankIndex, int maxPlants, float offsetX = 0, bool clickPlantFirst = true, bool visualOnly = false, bool visualSeedLoc = false)
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

            Program.Click(cellPos.X, cellPos.Y);

            Task.Delay(50).Wait();

            Program.Click(cellPos.X, cellPos.Y, true);  //Right-click to release plant

            //Restore pause state
            memIO.SetBoardPaused(mPaused);
        }

        void DoTutorials()
        {
            if (memIO.GetAdventureCompletions() > 0)
                return;
            int level = memIO.GetPlayerLevel();
            if(level == 1)
            {
                Program.GameplayTutorial(new string[] { "Use your directional keys to navigate tutorial text.", "To close a tutorial, press the confirm button."});
                Program.GameplayTutorial(new string[] { "You are presented with a grid-like lawn, 9 tiles long, and one tile high.", "Your house is on the left, and zombies will soon appear from the right.", "You'll need to use plants to stop the zombies from breaking into your house, and eating your brain." });
                Program.GameplayTutorial(new string[] { "A deck at the top of the screen contains all the plants you can place on the lawn.", "Placing plants will cost sun, and will trigger a cooldown before you can place the same plant again.", "Sun falls from the sky, and will be automatically collected as you play.", "Press the Info3 button to read your sun count."});
                Program.GameplayTutorial(new string[] { "After the tutorial, place a plant with the confirm button.", "Most shooting plants will shoot from their placed position, towards the right of the screen.", "You will need to place two plants before the game will start.", "Soon, zombies will start appearing. You can detect them with your zombie-sonar, which is the Info1 button."});
                Program.GameplayTutorial(new string[] { "And finally, you may occasionally hear advice telling you to click on things.", "This is part of the game's built-in tutorial, and can be ignored." });
            }
            if(level == 2)
            {
                Program.GameplayTutorial(new string[] { "Two more rows of lawn have been unrolled, making it a 9 by 3 grid.", "You've also unlocked a new plant, which you can select with the cycleLeft and cycleRight buttons" });
                Program.GameplayTutorial(new string[] { "Sunflowers are extremely important, as they produce sun, which is a vital resource for building your defenses.", "You'll want to place at least one sunflower for each row.", "When navigating the board, you can check which plant has been placed in a tile, by pressing the Info2 button." });
                Program.GameplayTutorial(new string[] { "Keep in mind that the zombie sonar is for the current row only.", "You'll need to move up and down on the board, to detect zombies in each row." });
            }
            if(level == 3)
            {
                Program.GameplayTutorial(new string[] { "You can use the option button to freeze the game.", "This is slightly different to pausing, as it will not open the pause menu.", "You will still be able to interact with the board, but all gameplay will be frozen.", "Don't forget to unfreeze with the option button again." });
            }
            if(level == 4)
            {
                Program.GameplayTutorial(new string[] { "The final two rows of the lawn have been unrolled, making it a 9 by 5 grid.", "Each plant has unique stats, including how long it takes to refresh."});
            }
            if(level == 5)
            {
                Program.GameplayTutorial(new string[] { "Pressing the deny button will dig up the plant at your current board position.", "Move around the board, and use the Info2 button to find the three peashooters.", "Shovel all three peashooters to continue." });
                Program.GameplayTutorial(new string[] { "You will soon be playing a conveyor level, where your deck is replaced with a conveyor belt.", "As you play, plants will arrive and build up on the conveyor belt.", "When you place a plant, it is removed from the conveyor belt." });
            }
            if(level == 6)
            {
                Program.GameplayTutorial(new string[] { "Each row has a lawn mower on the left side.", "If a zombie reaches a lawnmower, it will be activated and shred all zombies in that row.", "Once a lawnmower has been used, it won't come back until the level restarts, or a new one begins." });
                Program.GameplayTutorial(new string[] { "Also, watch out for pole-vaulting zombies! They're fast, quiet, and jump over the first plant they run into!" });
            }
            if(level == 10)
            {
                Program.GameplayTutorial(new string[] { "This is another conveyor belt level, similar to the bowling minigame.", "Plants will stop arriving when the belt gets full.", "Placing a plant will make room on the belt, and it will start moving again." });
            }
            if(level == 11)
            {
                Program.GameplayTutorial(new string[] { "The sun has set; leaving you with new challenges to face in this moonlit night.", "You'll find gravstones scattered around your front yard, which can not be planted on.", "And on the final wave of each level, additional zombies will emerge from the graves." });
            }
            if(level == 15)
            {
                Program.GameplayTutorial(new string[] { "This is Whack-A-Zombie.", "Zombies will quickly rise from graves around the lawn.", "You need to quickly find each zombie, then press the deny button to whack them with your mallet.", "Some zombies take more than one hit, and some will drop sun for you to use." });
            }
            if(level == 21)
            {
                Program.GameplayTutorial(new string[] { "Your backyard has six rows of tiles, with an in-ground pool taking up the two middle rows.", "Only aquatic plants can be placed in the pool, however, you can place non-aquatic plants on top of lillypads." });
            }
            if(level == 31)
            {
                Program.GameplayTutorial(new string[] { "As the moon takes the suns place in the sky, it brings a thick rolling fog with it." });
            }
            if(level == 35)
            {
                Program.GameplayTutorial(new string[] { "This is VaseBreaker, which takes place at night in your front yard.", "There are columns of vases on the right side of your lawn.", "Press confirm on a vase to break it open.", "Breaking a vase can spawn a zombie, or put a plant in your hand.", "Your goal is to break all the vases, and defeat all the zombies." });
            }
            if(level == 36)
            {
                Program.GameplayTutorial(new string[] { "Digger zombies will flank your plants by tunneling under your lawn, and emerging on the left.", "They walk from where they emerge, to the right, eating any plants that get in their way." });
            }
            if (level == 41)
            {
                Program.GameplayTutorial(new string[] { "The roof slopes up from left to right, with a flat section on the right four columns.", "Most shooting plants will be useless on the left four columns, as their projectiles will hit the slope.", "Catapulting plants will lob projectiles up and to the right, making them effective from any column." });
                Program.GameplayTutorial(new string[] { "Additionally, you can not plant on roof tiles.", "You will need to plant in flowerpots, similarly to lillypads in the pool.", "It is also day now, so mushrooms will be useless." });
            }
            if(level == 50)
            {
                Program.GameplayTutorial(new string[] { "This is it, the final level.", "Dr.Zomboss is in the cockpit of a giant robot.", "He will be invincible, until he lowers his head onto the screen.", "Your zombie sonar will inform you of when he is vulnerable.", "When he lowers his head, he can be attacked from plants in any row." });
                Program.GameplayTutorial(new string[] { "Dr.Zomboss will open his mouth, to release a large ball of fire, or ice.", "Fireballs and Iceballs will slowly roll from right to left, squashing any plants in their path.", "Fireballs can be extinguished from any row, but iceballs will need to be melted from the same row." });
                Program.GameplayTutorial(new string[] { "Dr.Zomboss can also be frozen with ice-shrooms, extending your attack time.", "Good luck!" });
            }
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

        //Returns true if current plant packet is fully refreshed, and there's enough sun to place it
        public bool PlantPacketReady()
        {
            bool inIZombie = memIO.GetGameMode() >= (int)GameMode.IZombie1 && memIO.GetGameMode() <= (int)GameMode.IZombieEndless;

            var plants = GetPlantsInBoardBank();

            if (plants[seedbankSlot].packetType < 0)
                return false;

            int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");    //Such a huge struct //TODO: Is that the same struct, or does it just happen to usually be in memory after the board? ie; will a fragmented heap cause this to go somewhere else?
            sunAmount += animatingSunAmount;
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

        string? GetCurrentTileObject(bool informEmptyTiles = true, bool beepOnFound = true, bool beepOnEmpty = true)
        {
            var gridItems = Program.GetGridItems();

            int vaseType = -1;
            bool hasCrater = false;
            bool hasGravestone = false;
            bool hasIce = CheckIceAtTile();
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

                }
            }

            float rightVol = (float)gridInput.cursorX / (float)gridInput.width;
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

                if (hasCrater)
                    plantInfoString = "Crater";
                else if (hasGravestone)
                    plantInfoString = "Gravestone";
                else if (vaseType != -1)
                    plantInfoString = vaseType == 3 ? "Mystery vase" : vaseType == 4 ? "Plant vase" : "Zombie vase";
                else if (hasIce)
                    plantInfoString = "Ice";
                else
                {
                    if (informEmptyTiles)
                        plantInfoString = "Empty " + typeString + " tile";
                    else
                        plantInfoString = null;
                }

                if ((hasCrater || hasGravestone || vaseType != -1 || hasIce) && beepOnFound)
                    Program.PlayTone(1.0f- rightVol, rightVol, freq, freq, 100, SignalGeneratorType.SawTooth);
                else if(beepOnEmpty)
                {
                    Program.PlayTone(1.0f- rightVol, rightVol, freq, freq, 50, SignalGeneratorType.Square);
                    //Program.PlayTone(1.0f- rightVol, rightVol, freq-100, freq, 50, SignalGeneratorType.Square, 55);
                }
            }
            else
            {
                Console.WriteLine("PlantID: " + plant.plantType);
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
                plantInfoString += Consts.plantNames[plant.plantType];
                if (plant.plantType != (int)SeedType.SEED_PUMPKINSHELL && plant.hasPumpkin)
                    plantInfoString += " with pumpkin shield";

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

                if (beepOnFound)
                    Program.PlayTone(1.0f- rightVol, rightVol, freq, freq, 100, SignalGeneratorType.SawTooth);
            }

            return plantInfoString;
        }
        
        string? GetZombieInfo(bool currentTileOnly = false, bool beepOnFound = true, bool beepOnNone = true, bool includeTileName = true)
        {
            List<Zombie> zombies = GetZombies();
            int y = gridInput.cursorY;
            float dirtyOffset = gridInput.cursorX * 5f;
            float targetX = (gridInput.cursorX * 100.0f) - 60.0f - dirtyOffset;

            List<Zombie> zombiesThisRow = new List<Zombie>();

            for (int i = 0; i < zombies.Count; i++)
            {
                if (zombies[i].row == y)
                {
                    Console.Write("{0} ", zombies[i].posX);
                    if(currentTileOnly && (zombies[i].posX >= targetX && zombies[i].posX <= targetX + 100.0f))
                        zombiesThisRow.Add(zombies[i]);
                    else if(!currentTileOnly)
                        zombiesThisRow.Add(zombies[i]);
                }
            }
            Console.WriteLine("");

            string verboseZombieInfo = "";

            Fireball? fireball = GetZombossFireballInfo();
            bool needToAddFireball = false;
            if (fireball.HasValue && fireball.Value.row == gridInput.cursorY)
            {
                verboseZombieInfo = (1 + zombiesThisRow.Count).ToString() + ".";
                needToAddFireball = true;
            }
            else
                verboseZombieInfo = zombiesThisRow.Count.ToString() + ".";

            zombiesThisRow.Sort((x, y) => (int)x.posX - (int)y.posX); //Sort by distance (so we can print/inform player in the correct order when speaking)

            int prevColumn = -1;

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
                int startDelay = (int)(zombiesThisRow[i].posX / 2.0f);
                if (startDelay > 1000 || startDelay < 0)
                    continue;

                if(beepOnFound)
                    Program.PlayTone(lVolume, rVolume, 300, 300, 100, SignalGeneratorType.Sin, startDelay);

                if (zombiesThisRow[i].zombieType >= (int)ZombieType.CachedZombieTypes)
                    continue;
                string zombieName = Consts.zombieNames[zombiesThisRow[i].zombieType];
                int zombieNameLen = zombieName.Length;

                int zombieColumn = (int)((zombiesThisRow[i].posX + 100.0f) / 100.0f);
                if (zombieColumn < 0)
                    zombieColumn = 0;
                if (zombieColumn > 10)
                    zombieColumn = 10;

                if (zombiesThisRow[i].zombieType == (int)ZombieType.DrZomBoss)
                {
                    if (zombiesThisRow[i].phase >= 87 && zombiesThisRow[i].phase <= 89)
                        zombieName = "Zomboss Head";
                }

                if (zombieColumn > prevColumn)
                {
                    prevColumn = zombieColumn;
                    if (includeTileName)
                        verboseZombieInfo += " " + (char)('A' + zombieColumn) + ": ";
                }
                verboseZombieInfo += zombieName + ", ";
            }

            if (zombiesThisRow.Count == 0)
            {
                if (beepOnNone)
                {
                    Program.PlayTone(1, 1, 200, 200, 50, SignalGeneratorType.SawTooth);
                    Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.SawTooth, 55);
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

        public int GetFastZombieCount()
        {
            var zombies = GetZombies();
            int poleVaultingCount = 0;
            int footballCount = 0;
            foreach(var zombie in zombies)
            {
                if ((ZombieType)zombie.zombieType is ZombieType.PoleVaulting)
                    poleVaultingCount++;

                if ((ZombieType)zombie.zombieType is ZombieType.Football)
                    footballCount++;
            }

            return poleVaultingCount + footballCount;
        }

        public override void Interact(InputIntent intent)
        {
            if(intent == InputIntent.Option)
            {
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

            //TODO: move to memIO/Pointers
            int seedbankSize = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",868,15c,24") - 1;  //10 seeds have max index of 9
            int maxConveryorBeltIndex = memIO.mem.ReadInt(memIO.ptr.lawnAppPtr + ",868,15c,34c") - 1;

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

            if(prevX != gridInput.cursorX || prevY != gridInput.cursorY)
            {
                int minX = 0;
                int maxX = 8;

                //If in wallnut bowling
                int gameMode = memIO.GetGameMode();
                bool inBowling = gameMode == (int)GameMode.WallnutBowling || gameMode == (int)GameMode.WallnutBowling2;
                inBowling |= memIO.GetPlayerLevel() == 5 && ConveyorBeltCounter() > 0; //converyorBeltCounter >0, means we're passed the peashooter-shovelling part.
                if (inBowling)
                    maxX = 2;

                //bool inIZombie = gameMode >= (int)GameMode.PUZZLE_I_ZOMBIE_1 && gameMode <= (int)GameMode.PUZZLE_I_ZOMBIE_ENDLESS;
                //if (inIZombie)
                  //  minX = 4;

                bool inZomboss = gameMode == (int)GameMode.DrZombossRevenge || (gameMode == (int)GameMode.Adventure && memIO.GetPlayerLevel() == 50);
                if (inZomboss)
                    maxX = 7;

                //We don't shrink the board, because we want audio cues to remain the same for the same level types
                gridInput.cursorX = gridInput.cursorX < minX ? minX : gridInput.cursorX;
                gridInput.cursorX = gridInput.cursorX > maxX ? maxX : gridInput.cursorX;
                gridInput.cursorY = gridInput.cursorY < minY ? minY : gridInput.cursorY;
                gridInput.cursorY = gridInput.cursorY > maxY ? maxY : gridInput.cursorY;

                string totalTileInfoStr = "";

                if(prevX == gridInput.cursorX && prevY == gridInput.cursorY)
                    Program.PlayTone(1, 1, 70, 70, 50, SignalGeneratorType.Square);
                else
                {
                    float rightVol = (float)gridInput.cursorX / (float)gridInput.width;
                    float freq = 1000.0f - ((gridInput.cursorY * 500.0f) / (float)gridInput.height);

                    bool plantFound = false;
                    if (Config.current.SayPlantOnTileMove || Config.current.BeepWhenPlantFound)
                    {
                        //Say plant at current tile
                        string? tileObjectInfo = GetCurrentTileObject(false, Config.current.BeepWhenPlantFound, false);
                        plantFound = tileObjectInfo != null;
                        if (tileObjectInfo is not null && Config.current.SayPlantOnTileMove)
                            totalTileInfoStr += tileObjectInfo;
                    }

                    bool zombieFound = false;
                    if(Config.current.SayZombieOnTileMove || Config.current.BeepWhenZombieFound)
                    {
                        string? zombiesThisTile = GetZombieInfo(true,false,false,false);
                        if (zombiesThisTile != null)
                        {
                            zombieFound = true;

                            if (Config.current.SayZombieOnTileMove)
                                totalTileInfoStr += " " + zombiesThisTile;

                            if (Config.current.BeepWhenZombieFound)
                            {
                                //Try getting the count from the string
                                int count = zombiesThisTile[0] - '0';
                                float zombieFreq = freq - 30;
                                Program.PlayTone(1.0f - rightVol, rightVol, zombieFreq, zombieFreq-50, 80, SignalGeneratorType.Square);
                                if (count > 0 && count < 10)
                                {
                                    for(int i =1; i < count; i++)
                                        Program.PlayTone(1.0f - rightVol, rightVol, zombieFreq - (50*i), zombieFreq - (50*(i+1)), 80, SignalGeneratorType.Square, 100*i);
                                }
                            }
                        }
                    }


                    
                    if (!plantFound && !zombieFound)
                        Program.PlayTone(1.0f - rightVol, rightVol, freq, freq, 100, SignalGeneratorType.Sin);
                }

                if (Config.current.SayTilePosOnMove)
                {
                    string tilePos = String.Format("{0}-{1}", (char)(gridInput.cursorX + 'A'), gridInput.cursorY + 1);
                    totalTileInfoStr += " " + tilePos;
                }

                if (totalTileInfoStr.Length > 0)
                {
                    Console.WriteLine(totalTileInfoStr);
                    Program.Say(totalTileInfoStr);
                }

                //Move mouse cursor to aid sighted players in knowing where their cursor is located visually
                MoveMouseToTile();
                //PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, true, false);

                /*
                float rightVol = gridInput.cursorX / 8.0f;
                //Only play tone when the cursor actually moves. Otherwise play 'boundary hit' sound
                if (prevX != gridInput.cursorX || prevY != gridInput.cursorY)
                {
                    float freq = 1000.0f - ((gridInput.cursorY * 500.0f) / 5.0f);
                    Program.PlayTone(1.0f - rightVol, rightVol, freq, freq, 100, SignalGeneratorType.Sin);
                }
                */

            }
            else if(intent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                Program.PlayTone(1, 1, 70, 70, 50, SignalGeneratorType.Square);


            int lastPlantSlot = seedbankSlot;
            if (intent == InputIntent.CycleLeft)
                seedbankSlot--;
            if (intent == InputIntent.CycleRight)
                seedbankSlot++;

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

            bool inVaseBreaker = VaseBreakerCheck();
            bool inRainingSeeds = memIO.GetGameMode() == (int)GameMode.ItsRainingSeeds;
            bool inWhackAZombie = memIO.GetGameMode() == (int)GameMode.WhackAZombie || memIO.GetPlayerLevel() == 15;
            bool inIZombie = memIO.GetGameMode() >= (int)GameMode.IZombie1 && memIO.GetGameMode() <= (int)GameMode.IZombieEndless;

            //If user tries to switch seeds while holding one in vasebreaker or rainingSeeds, inform them of already held plant.
            //Otherwise, inform them of plant in newly switched slot
            if ((intent == InputIntent.CycleLeft || intent == InputIntent.CycleRight) && (inVaseBreaker || inRainingSeeds))
            {
                heldPlantID = memIO.mem.ReadInt(memIO.ptr.boardChain + ",150,28");
                if (heldPlantID != -1)
                {
                    string plantStr = Consts.plantNames[heldPlantID] + " in hand.";
                    Console.WriteLine(plantStr);
                    Program.Say(plantStr, true);
                }
            }
            else if((intent is InputIntent.CycleLeft or InputIntent.CycleRight))
            {
                //Move mouse cursor to aid sighted players in knowing which seed packet is selected
                //Program.PlacePlant(seedbankSize, plants[seedbankSlot].offsetX, false, true, true);
                PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, true, true);

                float frequency = 100.0f + (100.0f * seedbankSlot);
                float rVolume = (float)seedbankSlot / (float)seedbankSize;
                Program.PlayTone(1.0f - rVolume, rVolume, frequency, frequency, 100, SignalGeneratorType.Square);

                if (plants.Count > 0)
                {
                    int packetType = plants[seedbankSlot].packetType;
                    if (packetType >= 0 && packetType < (int)SeedType.NUM_SEED_TYPES)
                    {
                        Console.WriteLine(Consts.plantNames[plants[seedbankSlot].packetType]);
                        Program.Say(Consts.plantNames[plants[seedbankSlot].packetType], true);
                    }
                    else if (packetType >= 60 && packetType <=74)
                    {
                        //iZombie levels
                        string zombieInfo = Consts.zombieNames[Consts.iZombieNameIndex[packetType - 60]] + " : " + Consts.iZombieSunCosts[packetType - 60] + " sun";
                        Console.WriteLine(zombieInfo);
                        Program.Say(zombieInfo, true);
                    }
                }
            }

            //Place plant
            if(intent == InputIntent.Confirm && plants[seedbankSlot].packetType >= 0)
            {
                //Click where plant needs to go. Not where plant is located (we already grab plant when auto-collecting everything on screen)
                if (inVaseBreaker || inRainingSeeds)
                    PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, false, false);
                //Program.PlacePlant(0, 0, false);
                else if (plants[seedbankSlot].absX < 0.72f)
                {
                    //Check if there's enough sun, and plant isn't on cooldown

                    int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");    //Such a huge struct //TODO: Is that the same struct, or does it just happen to usually be in memory after the board? ie; will a fragmented heap cause this to go somewhere else?
                    sunAmount += animatingSunAmount;
                    int sunCost = inIZombie? Consts.iZombieSunCosts[plants[seedbankSlot].packetType - 60] : Consts.plantCosts[plants[seedbankSlot].packetType];

                    bool notEnoughSun = sunAmount < sunCost;
                    bool refreshing = plants[seedbankSlot].isRefreshing;
                    bool isConveyorLevel = ConveyorBeltCounter() > 0;

                    int gameMode = memIO.GetGameMode();

                    //Todo: Other conveyor belt levels (is there a bool or something to indicate it's a conveyor level? Maybe search for converyor element?)
                    if (notEnoughSun && !isConveyorLevel && !inIZombie)
                    {
                        string warning = "Not enough sun! " + sunAmount + " out of " + sunCost;
                        Console.WriteLine(warning);
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Square);
                        Program.PlayTone(1, 1, 275, 275, 50, SignalGeneratorType.Square, 50);
                        Program.Say(warning, true);
                    }
                    else if (refreshing)
                    {
                        string warning = (((float)plants[seedbankSlot].refreshCounter / (float)plants[seedbankSlot].refreshTime) * 99.9f).ToString("0.") + "% refreshed";
                        Console.WriteLine(warning);
                        Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Square);
                        Program.PlayTone(1, 1, 275, 275, 50, SignalGeneratorType.Square, 50);
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
                    //Program.PlacePlant(seedbankSize, plants[seedbankSlot].offsetX);    //Do synschronously, to avoid altering pause state while placing plant (can still cause issues if we alt-tab while placing though)
                }
            }

            if (intent == InputIntent.Deny)
            {
                if(inWhackAZombie)
                    PlacePlant(seedbankSlot, seedbankSize, plants[seedbankSlot].offsetX, false, false, false);
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
                    Program.PlayTone(1, 0, freq, freq, 100, SignalGeneratorType.Square, 0);
                    //Program.PlayTone(1, 0, freq, freq, 100, SignalGeneratorType.Sin, 0);
                }

                if(getIzombieBrainCount(true) > 0)
                    Program.PlayTone(1, 0, freq, freq, 100, SignalGeneratorType.Square, 0);

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
                    info4String += 5-getIzombieBrainCount() + " out of 5 brains eaten";
                }
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
                int sunAmount = memIO.mem.ReadInt(memIO.ptr.boardChain + ",5578");
                sunAmount += animatingSunAmount;
                string sunString = sunAmount + " sun.";
                Console.WriteLine(sunString);
                Program.Say(sunString, true);
            }
        }
    }
}
