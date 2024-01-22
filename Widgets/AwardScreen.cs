using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class AwardScreen : Dialog
    {
        int awardType = -1;

        static ListItem[] GetListItems(string pointerChain, MemoryIO memIO)
        {
            string continueChain = memIO.ptr.lawnAppPtr + memIO.ptr.awardScreenOffset + memIO.ptr.awardContinueButton;
            int continueX = memIO.mem.ReadInt(continueChain + memIO.ptr.inlineButtonPosXOffset);
            int continueY = memIO.mem.ReadInt(continueChain + memIO.ptr.inlineButtonPosYOffset);

            int continueWidth = memIO.mem.ReadInt(continueChain + memIO.ptr.inlineButtonWidthOffset);
            int continueHeight = memIO.mem.ReadInt(continueChain + memIO.ptr.inlineButtonHeightOffset);

            continueX += continueWidth / 2;
            continueY += continueHeight / 2;

            var listItems = new ListItem[]
            {
                new ListItem(){text = Text.menus.repeat},
                new ListItem(){text = Text.menus.Continue, relativePos = new Vector2(continueX/800.0f, continueY/600.0f)}
            };


            return listItems;
        }

        public AwardScreen(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, GetListItems(pointerChain,memIO))
        {
            listIndex = 1;
        }

        public override void DenyInteraction()
        {
            listIndex = listItems.Length - 1;
            ConfirmInteraction();
        }

        public override void Interact(InputIntent intent)
        {
            //Ensure continue button position is updated
            listItems = GetListItems(pointerChain, memIO);
            if((listIndex == 0 && intent is InputIntent.Confirm) || intent is InputIntent.Info1)
                hasReadContent = false;
            else
                base.Interact(intent);
        }

        protected override string? GetContentUpdate()
        {
            //Avoid bug caused by reading award type before awardScreen has fully loaded (pressing help at main menu would sometimes read awardType as 0, resulting in the wrong message being shown)
            if (memIO.GetAwardType() != awardType)
                hasReadContent = false;

            hasUpdatedContents = true;
            return null;
        }

        protected override string? GetContent()
        {
            //Find what the player was awarded.
            //No plant on level 5 and 10 of each zone
            int playerLevel = memIO.GetPlayerLevel();
            int plant = Program.MaxOwnedSeedIndex(playerLevel);

            string awardTitle = Text.awards.newPlant;
            string awardBody = Text.plantNames[plant] + ": " + Text.plantTooltips[plant]; //TODO: Move plantNames and plantDescriptions somewhere better

            GameMode gameMode = (GameMode)memIO.GetGameMode();
            awardType = memIO.GetAwardType();
            //Console.WriteLine("Award Type: " + awardType);
            //Level resets to 1 after beating zomboss
            if (awardType == 1 && playerLevel == 1)
            {
                playerLevel = 60;
                listItems[1].relativePos = new Vector2(0.5f, 0.95f);
            }
            if (gameMode == GameMode.Adventure)
            {
                if(playerLevel == 1)
                {
                    awardTitle = Text.awards.bossTitle;
                    awardBody = Text.awards.bossMessage;
                }
                
                if (playerLevel % 10 == 0)
                {
                    awardTitle = Text.awards.noteTitle;
                    if (playerLevel == 10)
                        awardBody = Text.awards.note1;
                    if (playerLevel == 20)
                        awardBody = Text.awards.note2;
                    if (playerLevel == 30)
                        awardBody = Text.awards.note3;
                    if (playerLevel == 40)
                        awardBody = Text.awards.note4;
                    if (playerLevel == 50)
                        awardBody = Text.awards.note5;
                    if (playerLevel > 50)
                        awardBody = Text.awards.note6;
                }

                if (playerLevel == 5)
                {
                    awardTitle = Text.awards.shovelTitle;
                    awardBody = Text.awards.shovelMessage;
                }
                if (playerLevel == 15)
                {
                    awardTitle = Text.awards.almanacTitle;
                    awardBody = Text.awards.almanacMessage;
                }
                if (playerLevel == 25)
                {
                    awardTitle = Text.awards.shopTitle;
                    awardBody = Text.awards.shopMessage;
                }
                if (playerLevel == 35)
                {
                    awardTitle = Text.awards.tacoTitle;
                    awardBody = Text.awards.tacoMessage;
                }
                if (playerLevel == 45)
                {
                    awardTitle = Text.awards.zenGardenTitle;
                    awardBody = Text.awards.zenGardenMessage;
                }
            }
            else
            {
                bool isVaseBreaker = (gameMode >= GameMode.VaseBreaker1 && gameMode <= GameMode.VaseBreakerEndless);
                bool isIZombie = gameMode >= GameMode.IZombie1 && gameMode <= GameMode.IZombieEndless;
                awardTitle = Text.awards.trophyTitle;
                //TODO: Make sure player actually 'has' unlocked a new level (I think it's capped to the first three of each mode, before adventure mode is complete)
                if (isVaseBreaker)
                    awardBody = Text.awards.vaseBreaker;
                if (isIZombie)
                    awardBody = Text.awards.iZombie;

                int earnedTrophies = 0;

                if (gameMode >= GameMode.ZomBotany && gameMode <= GameMode.LimboIntro)
                {
                    awardBody = Text.awards.minigame;

                    for(int i = (int)GameMode.ZomBotany; i < (int)GameMode.DrZombossRevenge; i++)
                    {
                        if (memIO.GetChallengeScore(i) >= 1)
                            earnedTrophies++;
                    }
                    if (earnedTrophies > 17)
                        awardBody = Text.awards.getMoreTrophies;
                }





                if (gameMode >= GameMode.SurvivalDay && gameMode <= GameMode.SurvivalEndless5)
                {
                    awardBody = Text.awards.survival;
                    for (int i = (int)GameMode.SurvivalDay; i < (int)GameMode.SurvivalHardRoof; i++)
                    {
                        int reqScore = 10;
                        if (i < (int)GameMode.SurvivalHardDay)
                            reqScore = 5;

                        if (memIO.GetChallengeScore(i) >= reqScore)
                            earnedTrophies++;
                    }

                    if (earnedTrophies > 7)
                        awardBody = Text.awards.moreSurvivalTrophies;
                    if(earnedTrophies == 10)
                        awardBody = Text.awards.endlessSurvivalUnlocked;
                }

                if(MainMenu.CheckTrophies(memIO) >= 48)
                {
                    awardTitle = Text.awards.goldenSunflowerTitle;
                    awardBody = Text.awards.goldenSunflowerBody;
                }
                
                

            }

            if (awardType == 2)
            {
                awardTitle = Text.awards.badHelpTitle;
                awardBody = Text.awards.badHelpMessage;
            }

            string completeString = awardTitle + "\r\n" + awardBody;

            hasUpdatedContents = true;

            return completeString;
        }
    }
}
