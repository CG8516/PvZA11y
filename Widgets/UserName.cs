using Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class UserName : Widget
    {
        public UserName(MemoryIO memIO, string pointerChain, bool newUser) : base(memIO, pointerChain)
        {

            //Currently not in use, as text cursor isn't changed, leading to any text input happening 'before' the prefilled username :/
            //If we figure out how to move the text cursor, we can re-enable this, and move the cursor to the last character
            /*
            if(newUser)
            {
                //Pre-fill with current windows username ;)
                string userName = Environment.UserName;// System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                string result = string.Concat(userName.Where(char.IsLetterOrDigit));    //using linq feels like biting the forbidden fruit. So nice, but so evil.
                Console.WriteLine(result);

                if (result.Length > 12)
                    result = result.Substring(0, 12);

                memIO.mem.WriteMemory(pointerChain + ",18c,a8", "string", result);
                memIO.mem.WriteMemory(pointerChain + ",18c,b8", "int", result.Length.ToString());
            }
            */

            //Wait for enter or escape
            uint keyInput = 0;
            string prevName = memIO.mem.ReadString(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",a8", "", 12, true, Program.encoding);

            string menuStr = newUser ? Text.menus.createUser : Text.menus.renameUser;
            Console.WriteLine(menuStr);
            Program.Say(menuStr);

            
            while (keyInput != Key.Return && keyInput != Key.Escape)
            {
                keyInput = Program.input.GetKey(false);

                if(keyInput == Key.Up || keyInput == Key.Down || keyInput == Key.F1)
                {
                    Console.WriteLine(menuStr);
                    Program.Say(menuStr);
                }

                string nameText = memIO.mem.ReadString(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",a8", "", 12, true, Program.encoding);
                if (nameText != prevName)
                {
                    Console.WriteLine(nameText);
                    Program.Say(nameText);
                    prevName = nameText;
                }

                //If we're no longer in the username dialogue, break out of this (prevent hanging if we don't catch the dialogue close)
                if (memIO.mem.ReadUInt(pointerChain + ",0") != memIO.ptr.widgetType.UserName)
                {
                    Program.input.ClearIntents();
                    return;
                }
            }

            if (keyInput == Key.Escape)
            {
                Vector2 cancelButtonPos = memIO.GetWidgetButton2Pos(pointerChain);
                cancelButtonPos.X += 0.1f;
                cancelButtonPos.Y += 0.1f;
                cancelButtonPos /= new Vector2(800, 600);

                Vector2 parentPos = memIO.GetWidgetPos(pointerChain) / new Vector2(800, 600);

                cancelButtonPos += parentPos;
                Program.Click(cancelButtonPos);
            }

            Program.input.WaitForNoInput(); //Wait until the user isn't pressing any keys/buttons, and clear any potential input intents which were added
        }

        public override void Interact(InputIntent intent)
        {
            //Ignore intents, we want text input
        }

        protected override string? GetContent()
        {
            string titleString = memIO.mem.ReadString(pointerChain + memIO.ptr.dialogTitleStrOffset, "", 32, true, Program.encoding);    //New User / Rename User
            string bodyString = memIO.mem.ReadString(pointerChain + ",f4,0", "", 128, true, Program.encoding);  //Please Enter your name:
            string currentName = memIO.mem.ReadString(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",a8", "", 16, true, Program.encoding); //Current name text
            return titleString + " ... " + bodyString + " ... " + currentName;
        }

        protected override string? GetContentUpdate()
        {
            return memIO.mem.ReadString(pointerChain + memIO.ptr.usernamePickerNamesOffset + ",a8", "", 16, true, Program.encoding);  //Current name text
        }
    }
}
