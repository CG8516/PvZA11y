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

            Console.WriteLine("Continue pos: {0},{1}", continueX, continueY);
            Console.WriteLine("Continue pos relative: {0},{1}", continueX / 800.0f, continueY / 600.0f);
            Console.WriteLine("Continue Chain: '{0}'", continueChain);

            var listItems = new ListItem[]
            {
                //new ListItem(){text = "Continue", relativePos = new Vector2(0.5f,0.87f)}
                new ListItem(){text = "Continue", relativePos = new Vector2(continueX/800.0f, continueY/600.0f)}
            };


            return listItems;
        }

        public AwardScreen(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, GetListItems(pointerChain,memIO))
        {
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
            base.Interact(intent);
            if (intent is InputIntent.Info1)
                hasReadContent = false;
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

            string awardTitle = "You got a new plant!";
            string awardBody = Consts.plantNames[plant] + ": " + Consts.plantDescriptions[plant]; //TODO: Move plantNames and plantDescriptions somewhere better

            bool adventureMode = memIO.GetGameMode() == 0;
            awardType = memIO.GetAwardType();
            Console.WriteLine("Award Type: " + awardType);
            //Level resets to 1 after beating zomboss
            if (awardType == 1 && playerLevel == 1)
            {
                playerLevel = 60;
                listItems[0].relativePos = new Vector2(0.5f, 0.95f);
            }
            if (adventureMode)
            {
                if(playerLevel == 1)
                {
                    awardTitle = "You have defeated the Boss Zombie!";
                    awardBody = "Congratulations!  You have most triumphantly fended off the zombie attack!  Your lawn is safe... for now!";
                }
                
                if (playerLevel % 10 == 0)
                {
                    awardTitle = "You found a note";
                    if (playerLevel == 10)
                        awardBody = "Hello, we are about to launch an all-out attack on your houze. Sincerely, the Zombies";
                    if (playerLevel == 20)
                        awardBody = "Hello, We wood like to visit for a midnight znack. How does icecream and brains zound? Sincerely, the Zombies";
                    if (playerLevel == 30)
                        awardBody = "Hello, We herd you were having a pool party. We think that iz fun. Well be rite over. Sincerely, the Zombies";
                    if (playerLevel == 40)
                        awardBody = "Hello, This iz your muther. Please come over to my house for 'meatloaf'. Leave your front door open and your lawn unguarded. Sincerely, mom (not the Zombies)";
                    if (playerLevel == 50)
                        awardBody = "Homeowner, you have failed to submit to our rightful claim. Be advised that unless you comply, we will be forced to take extreme action. Please remit your home and brains to us forthwith. Sincerely, Dr. Edgar Zomboss";
                    if (playerLevel > 50)
                        awardBody = "Ok, you win. No more eatin brains for us. We just want to make music video with you now. Sincerely, the Zombies";
                }

                if (playerLevel == 5)
                {
                    awardTitle = "You got the shovel!";
                    awardBody = "Lets you dig up a plant to make room for another plant";
                }
                if (playerLevel == 15)
                {
                    awardTitle = "You found a suburban almanac!";
                    awardBody = "Keeps track of all plants and zombies you encounter";
                }
                if (playerLevel == 25)
                {
                    awardTitle = "You found Crazy Dave's car key!";
                    awardBody = "Now you can visit Crazy Dave's shop!";
                }
                if (playerLevel == 35)
                {
                    awardTitle = "You found a taco!";
                    awardBody = "What are you going to do with a taco?";
                }
                if (playerLevel == 45)
                {
                    awardTitle = "You found a watering can!";
                    awardBody = "Now you can play Zen Garden Mode!";
                }
            }
            else
            {
                int gameMode = memIO.GetGameMode();
                bool isVaseBreaker = (gameMode >= (int)GameMode.VaseBreaker1 && gameMode <= (int)GameMode.VaseBreakerEndless);
                bool isIZombie = gameMode >= (int)GameMode.IZombie1 && gameMode <= (int)GameMode.IZombieEndless;
                awardTitle = "You got a trophy!";
                //TODO: Make sure player actually 'has' unlocked a new level (I think it's capped to the first three of each mode, before adventure mode is complete)
                if (isVaseBreaker)
                    awardBody = "You've unlocked a new Vasebreaker level!";
                if (isIZombie)
                    awardBody = "You've unlocked a new 'I, Zombie' level!";

                if (gameMode >= (int)GameMode.ZomBotany && gameMode <= (int)GameMode.LimboIntro)
                    awardBody = "You've unlocked a new mini-game!";

                if (gameMode >= (int)GameMode.SurvivalDay && gameMode <= (int)GameMode.SurvivalEndless5)
                    awardBody = "You've unlocked a new survival level!";

            }

            if (awardType == 2)
            {
                awardTitle = "Help for Plants and Zombies Game";
                awardBody = "When the Zombies show up. just sit there and don't do anything. You win the game when the Zombies get to your houze. -this help section brought to you by the Zombies";
            }

            string completeString = awardTitle + "\r\n" + awardBody;

            hasUpdatedContents = true;

            return completeString;
        }
    }
}
