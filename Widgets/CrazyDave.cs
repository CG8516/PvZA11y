using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class CrazyDave : Dialog
    {
        int currentMessageID;
        string? prevText = null;
        public Widget? prevWidget = null;

        //Removes scripting keywords from dave dialogue (eg; {SHOW_MONEYBAG} {SCREAM2})
        //Will have to process those keywords later (eg'{SELL_PRICE} {PLANT_TYPE} {UPGRADE_COST} {MONEY}')
        string CleanupDaveString(string input)
        {
            char[] outChars = new char[input.Length];
            bool skipChar = false;
            int addedChars = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{')
                    skipChar = true;
                if (input[i] == '}')
                {
                    skipChar = false;
                    continue;
                }
                if (input[i] == 0x0d || input[1] == 0x0a)
                    break;

                if (!skipChar)
                {
                    outChars[addedChars] = input[i];
                    addedChars++;
                }
            }

            return new string(outChars, 0, addedChars);
        }

        static ListItem[] defaultList = new ListItem[]
        {
            new ListItem(){text = "Repeat"},
            new ListItem(){relativePos = new Vector2(0.5f,0.5f), text = "Next"},
        };

        public CrazyDave(MemoryIO memIO, string ptrChain, Widget? prevWidget = null) : base(memIO, ptrChain, defaultList)
        {
            this.prevWidget = prevWidget;
            listIndex = 1;
            //currentMessageID = memIO.GetDaveMessageID();
        }

        public override void ConfirmInteraction()
        {
            if(listIndex != 0)
                base.ConfirmInteraction();
        }
        public override void Interact(InputIntent intent)
        {
            if (IsDaveReady() && intent != InputIntent.Info1)
            {
                UpdateInteractions();   //Bugfix, the yes/no buttons when buying seed slots gets messed up
                base.Interact(intent);
            }

            if (intent is InputIntent.Confirm or InputIntent.Deny && listIndex != 0)
                hasUpdatedContents = true;

            if(intent is InputIntent.Info1 || (intent is InputIntent.Confirm && listIndex == 0))
            {
                string? daveText = memIO.GetDaveMessageText();
                if (daveText is null)
                    return;

                daveText = CleanupDaveString(daveText);
                Console.WriteLine(daveText);
                Program.Say(daveText);
            }
        }

        public override void DenyInteraction()
        {
            listIndex = listItems.Length - 1;
            ConfirmInteraction();
        }

        void UpdateInteractions()
        {
            int newMessageID = memIO.GetDaveMessageID();
            int msgLen = memIO.GetDaveMessageLength();

            bool zenSellDialogue = memIO.GetGameMode() == (int)GameMode.ZenGarden && msgLen > 0;

            //Zen garden intro
            if (newMessageID >= 2100 && newMessageID <= 2104)
                zenSellDialogue = false;

            uint baseDialogueID = memIO.mem.ReadUInt(memIO.ptr.lawnAppPtr + ",320,ac" + memIO.ptr.dialogIDOffset);  //TODO: Move to pointers/memIO

            if (baseDialogueID == DialogIDs.Store)
                zenSellDialogue = false;

            if (currentMessageID == 1503 || currentMessageID == 1553 || zenSellDialogue)
            {

                listItems = new ListItem[]
                {
                    new ListItem(){text = "Repeat"},
                    new ListItem(){text = "Yes", relativePos = (memIO.GetWidgetButton1Pos(pointerChain) / memIO.GetWidgetSize(pointerChain)) + new Vector2(0.1f,0.1f)},
                    new ListItem(){text = "No", relativePos = (memIO.GetWidgetButton2Pos(pointerChain) / memIO.GetWidgetSize(pointerChain)) + new Vector2(0.1f,0.1f)}
                };
            }
            else
            {
                listItems = new ListItem[]
                {
                    new ListItem(){text = "Repeat"},
                    new ListItem(){text = "Next", relativePos = new Vector2(0.5f,0.5f)}
                };
            }
        }

        private bool IsDaveReady()
        {
            int messageLength = memIO.GetDaveMessageLength();
            if (messageLength == 0)
                return false;
            int messageID = memIO.GetDaveMessageID();
            if (messageID == 0)
                return false;

            return true;
        }

        protected override string? GetContent()
        {
            int newMessageID = memIO.GetDaveMessageID();

            int msgLen = memIO.GetDaveMessageLength();
            bool zenSellDialogue = memIO.GetGameMode() == (int)GameMode.ZenGarden && msgLen > 0;

            //Zen garden intro
            if (newMessageID >= 2100 && newMessageID <= 2104)
                zenSellDialogue = false;

            //Console.WriteLine("Zen sell dialogue: {0}", zenSellDialogue);

            uint baseDialogueID = memIO.mem.ReadUInt(memIO.ptr.lawnAppPtr + ",320,ac" + memIO.ptr.dialogIDOffset);  //TODO: Move to pointers/memIO

            if (baseDialogueID == DialogIDs.Store)
                zenSellDialogue = false;

            //If message hasn't loaded yet (sometimes message doesn't load until a ms or two after the DaveDialogue starts) 
            if ((newMessageID == -1 || newMessageID == currentMessageID) && !zenSellDialogue)
            {
                //Console.WriteLine("DAVE NOT READY");
                hasUpdatedContents = true;
                return null;
            }

            Task.Delay(100).Wait();  //Wait for message to update

            currentMessageID = newMessageID;

            string? daveText = memIO.GetDaveMessageText();
            if (daveText is null)
                return null;

            daveText = CleanupDaveString(daveText);

            if (currentMessageID == 1503 || currentMessageID == 1553 || zenSellDialogue)
            {
                
                listItems = new ListItem[]
                {
                    new ListItem(){text = "Yes", relativePos = (memIO.GetWidgetButton1Pos(pointerChain) / memIO.GetWidgetSize(pointerChain)) + new Vector2(0.1f,0.1f)},
                    new ListItem(){text = "No", relativePos = (memIO.GetWidgetButton2Pos(pointerChain) / memIO.GetWidgetSize(pointerChain)) + new Vector2(0.1f,0.1f)}
                };
            }
            else
            {
                listItems = new ListItem[]
                {
                    new ListItem(){text = "Next", relativePos = new Vector2(0.5f,0.5f)}
                };
            }

            prevText = daveText;
            return daveText;
        }

        protected override string? GetContentUpdate()
        {
            int newMessageID = memIO.GetDaveMessageID();

            if ((newMessageID == -1 || newMessageID == currentMessageID) && currentMessageID != 1503 && currentMessageID != 1553)
            {
                //Console.WriteLine("DAVE NOT READY");
                hasUpdatedContents = true;
                return null;
            }

            currentMessageID = newMessageID;

            //Wait for message to update
            string? newText = prevText;
            while (newText == prevText)
            {
                //Exit loop early if no longer in dave dialog
                if (Program.GetActiveWidget(null) is not Widgets.CrazyDave)
                    return null;

                newText = memIO.GetDaveMessageText();
                Task.Delay(10).Wait();
            }



            if (newText is null)
            {
                prevText = null;
                return null;
            }

            newText = CleanupDaveString(newText);
            prevText = newText;
            return newText;
        }
    }
}

