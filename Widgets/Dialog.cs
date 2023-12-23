using Memory;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{

    public struct ListItem
    {
        public Vector2 relativePos; //position relative to parent/modal widget
        public string text;
        public int extraData;
    }

    class Dialog : Widget
    {
        public ListItem[] listItems;
        public int listIndex;


        public Dialog(MemoryIO memIO, string pointerChain, ListItem[] listItems) : base(memIO, pointerChain)
        {
            this.listItems = listItems;
        }

        public Vector2 GetItemPos()
        {
            UpdateWidgetPosition();
            Vector2 elementScreenPos = listItems[listIndex].relativePos;
            if (pointerChain != null && pointerChain.Length > 0)
            {
                elementScreenPos *= size;
                elementScreenPos.X /= 800.0f;
                elementScreenPos.Y /= 600.0f;
            }
            return relativePos + elementScreenPos;
        }

        //Action when confirm intent is processed
        public virtual void ConfirmInteraction()
        {
            Vector2 clickPos = GetItemPos();
            //Console.WriteLine("relativePos: {0},{1}", clickPos.X, clickPos.Y);
            //Console.WriteLine("listItem Name: " + listItems[listIndex].text);
            Program.Click(clickPos.X, clickPos.Y, false, false, 50, true);
        }

        public virtual void DenyInteraction()
        {
            return;
            //Default behaviour is to select bottom item in list
            //listIndex = listItems.Length - 1;
            //ConfirmInteraction();
        }

        public virtual string? SayTitle(bool shouldSay)
        {
            //Read title/contents
            int titleLen = memIO.mem.ReadInt(pointerChain + memIO.ptr.dialogTitleLenOffset);
            int bodyLen = memIO.mem.ReadInt(pointerChain + memIO.ptr.dialogBodyLenOffset);
            string titleString = "";
            if (titleLen <= 15)
                titleString = memIO.mem.ReadString(pointerChain + memIO.ptr.dialogTitleStrOffset,"", titleLen);
            else
                titleString = memIO.mem.ReadString(pointerChain + memIO.ptr.dialogTitleStrOffset + ",0", "", titleLen);
            
            string bodyString = "";
            if (bodyLen > 0)
                bodyString = memIO.mem.ReadString(pointerChain + memIO.ptr.dialogBodyStrOffset + ",0", "", bodyLen);
            bodyString = bodyString.ReplaceLineEndings(" ");

            string completeString =  titleString + "\r\n" + bodyString;

            if(shouldSay)
            {
                Console.WriteLine(completeString);
                Program.Say(completeString);
            }

            return completeString;
        }

        public override void Interact(InputIntent intent)
        {
            int lastIndex = listIndex;
            switch (intent)
            {
                case (InputIntent.Up):
                    listIndex--;
                    break;
                case (InputIntent.Down):
                    listIndex++;
                    break;
                case (InputIntent.Confirm):
                    ConfirmInteraction();
                    break;
                case (InputIntent.Deny):
                    DenyInteraction();
                    break;
                case (InputIntent.Info1):
                    SayTitle(true);
                    return;

            }

            if (listIndex != lastIndex)
            {
                if (listIndex < 0 || listIndex >= listItems.Length)
                    Program.Vibrate(0.1f, 0.1f, 50);
                ConfineInteractionIndex();
                Vector2 mousePos = GetItemPos();
                Program.MoveMouse(mousePos.X, mousePos.Y);

                float freq = 1250.0f - ((((float)listIndex / (float)listItems.Length) * 5000.0f) / 5.0f);
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Sin);

                Console.WriteLine(listItems[listIndex].text);
                Program.Say(listItems[listIndex].text, true);
            }
            else if (intent is (InputIntent.Up or InputIntent.Down))
                Program.PlayBoundaryTone();
        }

        public void ConfineInteractionIndex()
        {
            if (Config.current.WrapCursorInMenus)
            {
                listIndex = listIndex < 0 ? listItems.Length - 1 : listIndex;
                listIndex = listIndex >= listItems.Length ? 0 : listIndex;
            }
            else
            {
                listIndex = listIndex < 0 ? 0 : listIndex;
                listIndex = listIndex >= listItems.Length ? listItems.Length - 1 : listIndex;
            }
        }

        protected override string? GetContent()
        {
            return SayTitle(false);
        }

    }
}
