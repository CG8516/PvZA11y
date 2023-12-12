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
        int pageID; //0: main, 1: plants, 2: zombies
        bool plantsSelected;    //whether plants is the current selection on the index page (toggle state with any directional input on index) 

        public Almanac(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain)
        {
            pageID = memIO.GetAlmanacPage(pointerChain);
            PlantPage = new GridInput(8, 6);    //Imitater is in secret cell above peashooter, only if unlocked
            ZombiePage = new GridInput(5, 6);   //5 + 1 for zomboss

        }

        /*
        void UpdatePageInteractions()
        {
            pageID = memIO.GetAlmanacPage(pointerChain);

            //We reuse the seedpicker for plant page (AlmanacPage==1), as they're in the same layout and unlock state.
            if (pageID == 0)
            {
                
                widget.interactables = new WidgetInteractable[]
                {
                        new WidgetInteractable(){text = "Plants", relativePos = new Vector2(0.26f,0.6f)},
                        new WidgetInteractable(){text = "Zombies", relativePos = new Vector2(0.74f,0.6f)},
                        new WidgetInteractable(){text = "Close", relativePos = new Vector2(0.9f,0.95f)}
                };
            }
            else if (AlmanacPage == 2)
            {
                //Use a sequential list for zombies, as dr.zomboss is in an inaccessible location (hard to find without vision)
                //Could use a grid-based navigation, and jump all movement in the bottom row to dr.zomboss. But variable-width grids generally aren't nice for blind players.


                //TODO: Add full zombie name, stats, description
                //TODO MAYBE: Play identifiable sfx for each zombie (eg metal hit for bucket-head, pole-vault sfx, bungee sound)

                //First levels you see each zombie type
                int[] zombieLevels = new int[] { 1, 3, 6, 8, 11, 13, 16, 18, 18, 21, 23, 26, 26, 28, 31, 33, 36, 38, 40, 41, 43, 46, 48, 48, 50, 99, 99, 99, 99, 99, 99, 48, 99, 99, 99, 99 };

                int playerLevel = memIO.GetPlayerLevel();
                int adventureCompletions = mem.ReadInt(lawnAppPtr + ",94c,54");

                List<WidgetInteractable> interactables = new List<WidgetInteractable>();
                for (int i = 0; i <= (int)ZombieType.DrZomBoss; i++)
                {
                    string zombieDesc = ".\r\n" + zombieFullDescriptions[i];
                    if (i == (int)ZombieType.Yeti)
                    {
                        if (adventureCompletions > 1 || (adventureCompletions > 0 && playerLevel >= zombieLevels[i]))
                            interactables.Add(new WidgetInteractable() { text = ((ZombieType)i).ToString() + zombieDesc });
                        else
                            interactables.Add(new WidgetInteractable() { text = "Mystery Zombie. Not encountered yet." });
                    }
                    else if (adventureCompletions > 0 || playerLevel >= zombieLevels[i])
                        interactables.Add(new WidgetInteractable() { text = ((ZombieType)i).ToString() + zombieDesc });
                }

                widget.interactables = interactables.ToArray();
            }
        }
        */

        protected override string? GetContentUpdate()
        {
            switch(pageID)
            {
                case 0:
                    return "Almanac Index.";
                case 1:
                    return "Plants.";
                case 2:
                    return "Zombies.";
            }
            return null;
        }


        public override void Interact(InputIntent intent)
        {
            int newPageID = memIO.GetAlmanacPage(pointerChain);
            Console.WriteLine("Page id: " + newPageID);
            if(newPageID != pageID)
            {
                pageID = newPageID;
                hasUpdatedContents = true;
            }

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
                    Console.WriteLine("FullChain: {0}", fullChain);
                    Console.WriteLine("x/y: {0},{1}", clickX, clickY);

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


            if (pageID == 1 && PlantPage.cursorX == 0 && PlantPage.cursorY == 0 && intent == InputIntent.Up)
            {
                //Shhhh! It's a secret! (Tree of Life tells you about it)
                //If imitater unlocked, move to super secret cell.
                if(memIO.GetPlayerPurchase(StoreItem.GameUpgradeImitater) == 1)
                    PlantPage.cursorY = -1;
            }
            

            switch(pageID)
            {
                case 0:
                    if(intent is InputIntent.Up or InputIntent.Down or InputIntent.Left or InputIntent.Right)
                    {
                        plantsSelected = !plantsSelected;
                        string text = plantsSelected ? "Plants" : "Zombies";
                        Console.WriteLine(text);
                        Program.Say(text, true);

                        float rightVol = plantsSelected ? 0.2f : 0.8f;
                        float leftVol = 1.0f - rightVol;
                        float freq = 400 + (plantsSelected ? 100 : 0);

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
                    }
                    break;
                case 1:
                    {
                        int prevX = PlantPage.cursorX;
                        int prevY = PlantPage.cursorY;
                        PlantPage.Interact(intent);

                        if(PlantPage.cursorX != prevX || PlantPage.cursorY != prevY)
                        {
                            int pickerIndex = (PlantPage.cursorY * 8) + PlantPage.cursorX;
                            int sunCost = Consts.plantCosts[pickerIndex];

                            string plantInfo;// = Program.plantNames[pickerIndex] + ": " + sunCost + ": " + Program.plantDescriptions[pickerIndex];

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
                                //Click on plant in almanac, for visual update
                                Program.MoveMouse(clickX, clickY);
                                Program.Click(clickX, clickY);

                                int rechargeTime = Consts.plantCooldowns[pickerIndex];

                                string rechargeText = rechargeTime == 750 ? "Fast.\r\n" : rechargeTime == 3000 ? "Slow.\r\n" : "Very Slow.\r\n";

                                plantInfo = Consts.plantNames[pickerIndex] + ": " + sunCost + " sun.\r\nRecharge time: " + rechargeText + Consts.plantFullDescriptions[pickerIndex];
                            }

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

                        int zombieIndex = ZombiePage.cursorY * 5 + ZombiePage.cursorX;
                        if (ZombiePage.cursorY == 5 && ZombiePage.cursorX == 2)
                            zombieIndex = 25;
                        string zombieString = Consts.zombieNames[zombieIndex] + ".\r\n" + Consts.zombieFullDescriptions[zombieIndex];

                        int[] zombieLevels = new int[] { 1, 3, 6, 8, 11, 13, 16, 18, 18, 21, 23, 26, 26, 28, 31, 33, 36, 38, 40, 41, 43, 46, 48, 48, 50, 99, 99, 99, 99, 99, 99, 48, 99, 99, 99, 99 };

                        int playerLevel = memIO.GetPlayerLevel();
                        int adventureCompletions = memIO.GetAdventureCompletions();
                        if(zombieIndex == (int)ZombieType.Yeti)
                        {
                            if (adventureCompletions == 0 || playerLevel < zombieLevels[zombieIndex])
                                zombieString = "Mystery Zombie. Not encountered yet.";
                        }
                        if (adventureCompletions == 0 && playerLevel < zombieLevels[zombieIndex])
                            zombieString = "Mystery Zombie. Not encountered yet.";


                        Console.WriteLine(zombieString);
                        Program.Say(zombieString, true);

                    }
                    break;
            }
        }
    }
}
