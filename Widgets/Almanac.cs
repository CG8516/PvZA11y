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
                        return Text.almanac.index;
                    case 1:
                        return Text.almanac.plants;
                    case 2:
                        return Text.almanac.zombies;
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
                    return contents + (Config.current.SayAvailableInputs ? "\r\n" + Text.inputs.almanacGrid + "\r\n" : "") + GetPlantInfo();
                else if(pageID == 2)
                    return contents + (Config.current.SayAvailableInputs ? "\r\n" + Text.inputs.almanacGrid + "\r\n":"") + GetZombieInfo();
                return contents + (Config.current.SayAvailableInputs ? "\r\n" + Text.inputs.almanacIndex : "");
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
                    plantInfo = Text.menus.plantUnavailable + (storeUnlocked ? Text.menus.purchasablePlantUnavailable : "");
                else
                    plantInfo = Text.menus.plantLocked;

                Program.MoveMouse(clickX, clickY);  //Move cursor for sighted players
            }
            else
            {
                //Shhhh! It's a secret! (Tree of Life tells you about it)
                //If imitater unlocked, move to super secret cell.

                Program.MoveMouse(clickX, clickY);
                Program.Click(clickX, clickY);

                int rechargeTime = Consts.plantCooldowns[pickerIndex];

                string rechargeText = rechargeTime == 750 ? Text.almanac.fast + "\r\n" : rechargeTime == 3000 ? Text.almanac.slow + "\r\n" : Text.almanac.verySlow + "\r\n";

                plantInfo = Text.plantNames[pickerIndex] + ": " + sunCost + Text.almanac.sun + "\r\n" + Text.almanac.rechargeTime + rechargeText + Text.plantAlmanacDescriptions[pickerIndex];
            }

            return plantInfo;
        }

        string GetZombieInfo()
        {
            int zombieIndex = ZombiePage.cursorY * 5 + ZombiePage.cursorX;
            if (ZombiePage.cursorY == 5 && ZombiePage.cursorX == 2)
                zombieIndex = 25;

            string zombieString = Text.zombieNames[zombieIndex] + ".\r\n" + Text.zombieAlmanacDescriptions[zombieIndex];

            int[] zombieLevels = new int[] { 1, 1, 3, 6, 8, 11, 13, 16, 18, 19, 21, 23, 26, 27, 28, 31, 33, 36, 38, 40, 41, 43, 46, 48, 49, 50, 99, 99, 99, 99, 99, 48, 99, 99, 99, 99 };

            int playerLevel = memIO.GetPlayerLevel();
            int adventureCompletions = memIO.GetAdventureCompletions();
            if (zombieIndex == (int)ZombieType.Yeti)
            {
                if (adventureCompletions == 0 || playerLevel < zombieLevels[zombieIndex])
                    zombieString = Text.almanac.mysteryZombie;
            }
            if (adventureCompletions == 0 && playerLevel < zombieLevels[zombieIndex])
                zombieString = Text.almanac.mysteryZombie;

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
                        string text = plantsSelected ? Text.almanac.plants : Text.almanac.zombies;
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
                            string plantInfo = Text.plantNames[(int)SeedType.SEED_IMITATER] + ": " + Text.plantAlmanacDescriptions[(int)SeedType.SEED_IMITATER];
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
