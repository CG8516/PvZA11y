using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class ButtonPicker : Dialog
    {

        public Widget? prevWidget = null;
        string? prevContent = null;

        //Have to pass reference to memio/pointerchain, as this is called before base constructor (yucky)
        //Will happily accept pull requests for a cleaner implementation of this.
        private static ListItem[] GetButtons(MemoryIO memIO, string pointerChain)
        {
            int buttonCount = memIO.WidgetHasButton2(pointerChain) ? 2 : 1;
            ListItem[] listItems = new ListItem[buttonCount];
            
            Vector2 baseSize = memIO.GetWidgetSize(pointerChain);

            Vector2 button1Pos = memIO.GetWidgetButton1Pos(pointerChain);
            button1Pos.X /= baseSize.X;
            button1Pos.Y /= baseSize.Y;
            button1Pos.X += 0.1f;
            button1Pos.Y += 0.1f;
            string button1Str = memIO.GetWidgetButton1String(pointerChain);
            listItems[0] = new ListItem() { relativePos = button1Pos, text = button1Str };

            if(buttonCount == 2)
            {
                Vector2 button2Pos = memIO.GetWidgetButton2Pos(pointerChain);
                button2Pos.X /= baseSize.X;
                button2Pos.Y /= baseSize.Y;
                button2Pos.X += 0.1f;
                button2Pos.Y += 0.1f;
                string button2Str = memIO.GetWidgetButton2String(pointerChain);
                listItems[1] = new ListItem() { relativePos = button2Pos, text = button2Str };
            }

            return listItems;
        }



        public ButtonPicker(MemoryIO memIO, string pointerChain, Widget? prevWidget = null) : base(memIO, pointerChain, GetButtons(memIO,pointerChain))
        {
            this.prevWidget = prevWidget;
            hasUpdatedContents = true;
        }

        public override void Interact(InputIntent intent)
        {
            listItems = GetButtons(memIO, pointerChain);
            base.Interact(intent);
        }

        public override void DenyInteraction()
        {
            //Select last item (usually cancel/back/no)
            listIndex = listItems.Length - 1;
            ConfirmInteraction();
        }

        protected override string? GetContent()
        {
            string? startContent = base.GetContent();   //Widget title/body
            prevContent = startContent;

            if (startContent is null)
                return null;

            return startContent + "\r\n" + listItems[0].text + (Config.current.SayAvailableInputs ? "\r\n" + Text.inputs.buttonPicker : "");
        }

        protected override string? GetContentUpdate()
        {
            hasUpdatedContents = true;
            string? currentText = SayTitle(false);
            if (currentText != prevContent)
            {
                prevContent = currentText;
                return currentText;
            }
            return null;

        }
    }
}
