using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    class UserPicker : Dialog
    {
        static ListItem[] InitListItems(MemoryIO memIO, string pointerChain)
        {
            string[] userNames = memIO.GetUserNamesFromPicker(pointerChain);

            ListItem[] listItems = new ListItem[userNames.Length];

            for (int i = 0; i < userNames.Length; i++)
                listItems[i] = new ListItem() { text = userNames[i], relativePos = new Vector2(0.2f, 0.3f + (0.05f * i)) };

            return listItems;
        }

        public UserPicker(MemoryIO memIO, string pointerChain) : base(memIO, pointerChain, InitListItems(memIO,pointerChain))
        {
        }

        public override void ConfirmInteraction()
        {

            Vector2 clickPos;

            int userCount = memIO.GetUserCountFromProfileMgr();

            //Don't click ok if creating a new user
            if (listIndex == userCount)
            {
                clickPos = relativePos + new Vector2(0.2f, 0.27f + (0.036f * listIndex));
            }
            else
            {
                memIO.mem.WriteMemory(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",fc", "int", listIndex.ToString()); //Set index of current selection

                //Ok button position
                UpdateWidgetPosition();
                Vector2 buttonSize = memIO.GetWidgetSize(pointerChain + memIO.ptr.dialogueWidgetButton1Offset) / new Vector2(2, 2);
                clickPos = relativePos + ((memIO.GetWidgetButton1Pos(pointerChain) + buttonSize) / new Vector2(800, 600));
            }
            
            Program.Click(clickPos);
        }

        protected override string? GetContent()
        {
            string? baseContent = base.GetContent();
            if (baseContent != null)
                baseContent += (Config.current.SayAvailableInputs ? Text.inputs.userPicker : "");
            return baseContent;
        }

        public override string? SayTitle(bool shouldSay)
        {
            if(!shouldSay)
                return base.SayTitle(shouldSay);
            return null;
        }

        public override void Interact(InputIntent intent)
        {
            base.Interact(intent);

            int userCount = memIO.GetUserCountFromProfileMgr();
            //Don't try to rename/delete the 'Create a new user' entry. smh
            if(listIndex != userCount)
            {
                if (intent == InputIntent.Info1)
                {
                    memIO.mem.WriteMemory(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",fc", "int", listIndex.ToString()); //Set index of current selection
                    //Click rename button

                    UpdateWidgetPosition();
                    string buttonOffset = pointerChain + memIO.ptr.userPickerRenameOffset;
                    int buttonX = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetPosXOffset);
                    int buttonY = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetPosYOffset);
                    int buttonWidth = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetWidthOffset) / 2;
                    int buttonHeight = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetHeightOffset) / 2;
                    Vector2 clickPos = relativePos + ((new Vector2((buttonX + buttonWidth), (buttonY + buttonHeight)) / new Vector2(800, 600)));
                    Program.Click(clickPos);
                }
                else if (intent == InputIntent.Info2)
                {
                    memIO.mem.WriteMemory(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",fc", "int", listIndex.ToString()); //Set index of current selection
                    //Click delete button (don't worry, there's a confirmation dialogue)

                    UpdateWidgetPosition();
                    string buttonOffset = pointerChain + memIO.ptr.userPickerDeleteOffset;
                    int buttonX = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetPosXOffset);
                    int buttonY = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetPosYOffset);
                    int buttonWidth = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetWidthOffset) / 2;
                    int buttonHeight = memIO.mem.ReadInt(buttonOffset + memIO.ptr.widgetHeightOffset) / 2;
                    Vector2 clickPos = relativePos + ((new Vector2((buttonX+buttonWidth), (buttonY+buttonHeight))/ new Vector2(800,600)));
                    Program.Click(clickPos);
                }
            }
            if (intent == InputIntent.Deny)
            {
                //Click cancel button

                UpdateWidgetPosition();
                Vector2 buttonSize = memIO.GetWidgetSize(pointerChain + memIO.ptr.dialogueWidgetButton2Offset) / new Vector2(2, 2);
                Vector2 clickPos = relativePos + ((memIO.GetWidgetButton2Pos(pointerChain)+buttonSize) / new Vector2(800,600));
                Program.Click(clickPos);
            }

        }
    }
}
