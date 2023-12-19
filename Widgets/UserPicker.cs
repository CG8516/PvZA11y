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
            {
                listItems[i] = new ListItem() { text = userNames[i], relativePos = new Vector2(0.2f, 0.3f + (0.05f * i)) };
                if(Config.current.SayAvailableInputs)
                    listItems[i].text += "\r\nInputs: Confirm to select, Deny to close, Info1 to rename, Info2 to delete, Directions to scroll.";
            }

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
            //if (listItems[listIndex].text.Equals("(Create a New User)"))
            if (listIndex == userCount)
            {
                clickPos = relativePos + new Vector2(0.2f, 0.27f + (0.036f * listIndex));
            }
            else
            {
                memIO.mem.WriteMemory(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",fc", "int", listIndex.ToString()); //Set index of current selection
                clickPos = relativePos + new Vector2(0.1f, 0.77f);  //position of 'ok' button in user picker
            }
            
            Program.Click(clickPos);
        }

        protected override string? GetContent()
        {
            string? baseContent = base.GetContent();
            if (baseContent != null && Config.current.SayAvailableInputs)
                baseContent += "Inputs: Confirm to select, Deny to close, Info1 to rename, Info2 to delete, Directions to scroll.";
            return baseContent;
        }
        public override void Interact(InputIntent intent)
        {
            base.Interact(intent);
            //memIO.mem.WriteMemory(pointerChain + ",18c,fc", "int", listIndex.ToString()); //Set index of current selection

            int userCount = memIO.GetUserCountFromProfileMgr();
            //Don't try to rename/delete the 'Create a new user' entry. smh
            //if (!listItems[listIndex].text.Equals("(Create a New User)"))
            if(listIndex != userCount)
            {
                if (intent == InputIntent.Info1)
                {
                    memIO.mem.WriteMemory(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",fc", "int", listIndex.ToString()); //Set index of current selection
                    //Click rename button
                    Vector2 clickPos = relativePos + new Vector2(0.1f, 0.7f);
                    Program.Click(clickPos);
                }
                else if (intent == InputIntent.Info2)
                {
                    memIO.mem.WriteMemory(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",fc", "int", listIndex.ToString()); //Set index of current selection
                    //Click delete button (don't worry, there's a confirmation dialogue)
                    Vector2 clickPos = relativePos + new Vector2(0.5f, 0.7f);
                    Program.Click(clickPos);
                }
                else if (intent == InputIntent.Deny)
                {
                    //Click cancel button
                    Vector2 clickPos = relativePos + new Vector2(0.4f, 0.78f);
                    Program.Click(clickPos);
                }

            }

        }
    }
}
