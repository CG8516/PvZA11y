using Memory;
using Microsoft.VisualBasic.Devices;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    class Almanac : Widget
    {
        GridInput PlantPage;
        GridInput ZombiePage;
        int pageID = -1; //0: main, 1: plants, 2: zombies
        bool plantsSelected;    //whether plants is the current selection on the index page (toggle state with any directional input on index) 

        public Almanac(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain)
        {
            PlantPage = new GridInput(8, 6);    //Imitater is in secret cell above peashooter, only if unlocked
            ZombiePage = new GridInput(5, 6);   //5 + 1 for zomboss
        }

        protected override string? GetContentUpdate()
        {
            hasReadContent = false;
            hasUpdatedContents = true;
            int newPageID = memIO.GetAlmanacPage(pointerChain);
            if (newPageID != pageID)
            {
                pageID = newPageID;
                switch (pageID)
                {
                    case 0:
                        return "Almanac Index.";
                    case 1:
                        return "Plants.";
                    case 2:
                        return "Zombies.";
                }
            }
            return null;
        }

        protected override string? GetContent()
        {
            string? contents = GetContentUpdate();
            if(contents != null)
            {
                if(pageID == 1)
                    return contents + "\r\nInputs: Directions to navigate grid, Deny to return to index.\r\n" + GetPlantInfo();
                else if(pageID == 2)
                    return contents + "\r\nInputs: Directions to navigate grid, Deny to return to index.\r\n" + GetZombieInfo();
                return contents + "\r\nInputs: Directions to change option, Confirm to select, Deny to close.";
            }
            return null;
        }

        string GetPlantInfo()
        {
            int pickerIndex = (PlantPage.cursorY * 8) + PlantPage.cursorX;
            int sunCost = Consts.plantCosts[pickerIndex];

            string plantInfo = "";

            bool plantUnlocked = Program.CheckOwnedPlant(pickerIndex);

            float clickX = 0.06f + (0.065f * PlantPage.cursorX);
            float clickY = 0.2f + (0.13f * PlantPage.cursorY);

            if (!plantUnlocked)
            {
                int finishedAdventure = memIO.GetAdventureCompletions();
                bool storeUnlocked = finishedAdventure > 0 || memIO.GetPlayerLevel() > 24;

                if (PlantPage.cursorY == 5)
                    plantInfo = "Unavailable." + (storeUnlocked ? " Buy it from the store." : "");
                else
                    plantInfo = "Locked. Keep playing adventure mode to unlock more plants.";

                Program.MoveMouse(clickX, clickY);  //Move cursor for sighted players
            }
            else
            {
                //Shhhh! It's a secret! (Tree of Life tells you about it)
                //If imitater unlocked, move to super secret cell.

                Program.MoveMouse(clickX, clickY);
                Program.Click(clickX, clickY);

                int rechargeTime = Consts.plantCooldowns[pickerIndex];

                string rechargeText = rechargeTime == 750 ? "Fast.\r\n" : rechargeTime == 3000 ? "Slow.\r\n" : "Very Slow.\r\n";

                plantInfo = Consts.plantNames[pickerIndex] + ": " + sunCost + " sun.\r\nRecharge time: " + rechargeText + Consts.plantFullDescriptions[pickerIndex];
            }

            return plantInfo;
        }

        string GetZombieInfo()
        {
            int zombieIndex = ZombiePage.cursorY * 5 + ZombiePage.cursorX;
            if (ZombiePage.cursorY == 5 && ZombiePage.cursorX == 2)
                zombieIndex = 25;

            string zombieString = Consts.zombieNames[zombieIndex] + ".\r\n" + Consts.zombieFullDescriptions[zombieIndex];

            int[] zombieLevels = new int[] { 1, 3, 6, 8, 11, 13, 16, 18, 18, 21, 23, 26, 26, 28, 31, 33, 36, 38, 40, 41, 43, 46, 48, 48, 50, 99, 99, 99, 99, 99, 99, 48, 99, 99, 99, 99 };

            int playerLevel = memIO.GetPlayerLevel();
            int adventureCompletions = memIO.GetAdventureCompletions();
            if (zombieIndex == (int)ZombieType.Yeti)
            {
                if (adventureCompletions == 0 || playerLevel < zombieLevels[zombieIndex])
                    zombieString = "Mystery Zombie. Not encountered yet.";
            }
            if (adventureCompletions == 0 && playerLevel < zombieLevels[zombieIndex])
                zombieString = "Mystery Zombie. Not encountered yet.";

            return zombieString;
        }

        public override void Interact(InputIntent intent)
        {
            hasUpdatedContents = true;

            if (intent is InputIntent.Deny or InputIntent.Start)
            {
                if (pageID > 0)
                {
                    string indexChain = pointerChain + memIO.ptr.almanacIndexButtonOffset;
                    int indexPosX = memIO.mem.ReadInt(indexChain + memIO.ptr.inlineButtonPosXOffset); //32
                    int indexPosY = memIO.mem.ReadInt(indexChain + memIO.ptr.inlineButtonPosYOffset); //567
                    int indexWidth = memIO.mem.ReadInt(indexChain + memIO.ptr.inlineButtonWidthOffset);
                    int indexHeight = memIO.mem.ReadInt(indexChain + memIO.ptr.inlineButtonHeightOffset);
                    float clickX = (indexPosX + indexWidth / 2) / 800.0f;
                    float clickY = (indexPosY + indexHeight / 2) / 600.0f;

                    string fullChain = indexChain + memIO.ptr.inlineButtonPosXOffset;

                    Program.Click(clickX, clickY, false, false,100,true);

                    int delayCount = 30;   //~30ms timeout to avoid hanging while waiting for almanac to update
                    while (pageID > 0 && delayCount-- > 0)
                    {
                        pageID = memIO.GetAlmanacPage(pointerChain);
                        Task.Delay(1).Wait();
                    }
                }
                else
                {
                    string closeChain = pointerChain + memIO.ptr.almanacCloseButtonOffset;
                    int closePosX = memIO.mem.ReadInt(closeChain + memIO.ptr.inlineButtonPosXOffset); //676
                    int closePosY = memIO.mem.ReadInt(closeChain + memIO.ptr.inlineButtonPosYOffset); //567
                    int closeWidth = memIO.mem.ReadInt(closeChain + memIO.ptr.inlineButtonWidthOffset);
                    int closeHeight = memIO.mem.ReadInt(closeChain + memIO.ptr.inlineButtonHeightOffset);
                    float clickX = (closePosX + closeWidth / 2) / 800.0f;
                    float clickY = (closePosY + closeHeight / 2) / 600.0f;

                    Program.Click(clickX, clickY, false, false, 100, true);
                }

                hasUpdatedContents = true;
                return;
            }
            

            switch(pageID)
            {
                case 0:
                    if(intent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                    {
                        plantsSelected = !plantsSelected;
                        string inputText = "";
                        string text = plantsSelected ? "Plants" : "Zombies";
                        text += inputText;
                        Console.WriteLine(text);
                        Program.Say(text, true);

                        float rightVol = plantsSelected ? 0.2f : 0.8f;
                        float leftVol = 1.0f - rightVol;
                        float freq = 400 + (plantsSelected ? 100 : 0);
                        leftVol *= Config.current.MenuPositionCueVolume;
                        rightVol *= Config.current.MenuPositionCueVolume;
                        Program.PlayTone(leftVol, rightVol, freq, freq, 100, SignalGeneratorType.Sin);


                        //Move cursor for sighted players
                        if (plantsSelected)
                            Program.MoveMouse(0.25f, 0.62f);
                        else
                            Program.MoveMouse(0.75f, 0.62f);

                    }
                    if(intent == InputIntent.Confirm)
                    {
                        if (plantsSelected)
                            Program.Click(0.25f, 0.62f);
                        else
                            Program.Click(0.75f, 0.62f);
                        hasUpdatedContents = true;
                    }
                    break;
                case 1:
                    {
                        int prevX = PlantPage.cursorX;
                        int prevY = PlantPage.cursorY;
                        bool onImitater = false;
                        if (prevY == 0 && prevX == 0 && intent is InputIntent.Up && Program.CheckOwnedPlant((int)SeedType.SEED_IMITATER))
                            onImitater = true;
                        else
                            PlantPage.Interact(intent);

                        if(PlantPage.cursorX != prevX || PlantPage.cursorY != prevY)
                        {
                            string plantInfo = GetPlantInfo();
                            Console.WriteLine(plantInfo);
                            Program.Say(plantInfo, true);
                        }

                        if(onImitater)
                        {
                            float clickX = 0.06f;
                            float clickY = 0.1f;
                            Program.MoveMouse(clickX, clickY);
                            Program.Click(clickX, clickY);
                            string plantInfo = Consts.plantNames[(int)SeedType.SEED_IMITATER] + ": " + Consts.plantFullDescriptions[(int)SeedType.SEED_IMITATER];
                            Console.WriteLine(plantInfo);
                            Program.Say(plantInfo, true);
                        }

                        break;
                    }
                case 2:
                    ZombiePage.Interact(intent);
                    {
                        //If on bottom row of zombies page, ensure cursorX is on the Zomboss cell
                        if (ZombiePage.cursorY == 5)
                            ZombiePage.cursorX = 2;

                        //Click on zombie tile
                        float clickX = 0.07f + (0.105f * ZombiePage.cursorX);
                        float clickY = 0.22f + (0.14f * ZombiePage.cursorY);
                        Program.MoveMouse(clickX, clickY);
                        Program.Click(clickX, clickY);

                        string zombieString = GetZombieInfo();


                        Console.WriteLine(zombieString);
                        Program.Say(zombieString, true);

                    }
                    break;
            }
        }
    }
}
