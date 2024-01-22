using Memory;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    internal class BonusModeMenu : Dialog
    {

        public override string? SayTitle(bool shouldSay)
        {
            string? content = listItems[listIndex].text;
            if (content != null)
            {
                Console.WriteLine(content);
                Program.Say(content);
            }
            return content;
            
        }

        private static ListItem[] GetGameButtons(MemoryIO memIO)
        {
            List<ListItem> listItems = new List<ListItem>();
            for (int i = 0; i < 72; i++)
            {
                ListItem? nullableItem = memIO.TryGetMinigameButton(i);
                if (nullableItem == null)
                    continue;

                ListItem listItem = nullableItem.Value;
                listItem.text = Text.minigameNames[i];
                listItem.extraData = i;
                //Console.WriteLine("buttonPos: {0},{1}", listItem.relativePos.X, listItem.relativePos.Y);
                listItems.Add(listItem);
            }

            listItems.Add(new ListItem() { text = Text.menus.mainMenu, relativePos = new Vector2(0.1f, 0.95f) });

            return listItems.ToArray();
        }

        public BonusModeMenu(MemoryIO memIO) : base(memIO, "", GetGameButtons(memIO))
        {
        }

        protected override string? GetContent()
        {
            string inputDesc = Text.inputs.minigameSelector + "\r\n";
            return (Config.current.SayAvailableInputs ? inputDesc : "") + listItems[listIndex].text + GetCompletionString();
        }

        string GetCompletionString(bool say = false)
        {
            if (listIndex == listItems.Length - 1)
                return "";
            bool isComplete = CheckComplete((GameMode)listItems[listIndex].extraData + 1);
            string completionString = isComplete ? Text.menus.minigameComplete : "";
            if(say)
            {
                if (!isComplete)
                    completionString = Text.menus.minigameNotComplete;
                Console.WriteLine(completionString);
                Program.Say(completionString);
            }
            if (!say && isComplete)
                completionString = ", " + completionString;
            return completionString;
        }

        public override void DenyInteraction()
        {
            Program.Click(listItems[listItems.Length - 1].relativePos);
        }

        void SayTrophyCount()
        {
            int pageType = 0;   //0: minigames, 1: puzzle, 2: survival
            if (listItems[0].extraData +1 == (int)GameMode.VaseBreaker1)
                pageType = 1;
            if (listItems[0].extraData +1 == (int)GameMode.SurvivalDay)
                pageType = 2;
            int maxTrophies = 20;
            if (pageType == 1)
                maxTrophies = 18;
            if (pageType == 2)
                maxTrophies = 10;

            int completions = 0;
            for(int i =0; i < listItems.Length-1; i++)
            {
                if (CheckComplete((GameMode)listItems[i].extraData + 1))
                    completions++;
            }

            string trophyStr = Text.menus.trophyCount;
            trophyStr = trophyStr.Replace("[0]", completions.ToString());
            trophyStr = trophyStr.Replace("[1]", maxTrophies.ToString());
            Console.WriteLine(trophyStr);
            Program.Say(trophyStr);
        }

        public override void ConfirmInteraction()
        {
            Vector2 clickPos = GetItemPos();
            clickPos.X += 0.05f;
            clickPos.Y += 0.05f;
            Program.Click(clickPos.X, clickPos.Y, false, false, 50, true);
        }

        bool CheckComplete(GameMode mode)
        {
            int reqScore = 1;
            if (mode >= GameMode.SurvivalDay && mode < GameMode.SurvivalHardDay)
                reqScore = 5;
            else if (mode >= GameMode.SurvivalHardDay && mode < GameMode.SurvivalEndless1)
                reqScore = 10;

            return memIO.GetChallengeScore((int)mode) >= reqScore;
        }

        public override void Interact(InputIntent intent)
        {
            listItems = GetGameButtons(memIO);  //Update button list, because sometimes the screen loads before the buttons have been initialized :(

            int lastIndex = listIndex;
            switch (intent)
            {
                case InputIntent.Up:
                    listIndex--;
                    break;
                case InputIntent.Down:
                    listIndex++;
                    break;
                case InputIntent.Confirm:
                    if (listIndex == listItems.Length - 1)
                        DenyInteraction();
                    else
                        ConfirmInteraction();
                    break;
                case InputIntent.Deny:
                    DenyInteraction();
                    break;
                case InputIntent.Info1:
                    SayTitle(true);
                    return;
                case InputIntent.Info2:
                    GetCompletionString(true);
                    return;
                case InputIntent.Info3:
                    SayTrophyCount();
                    return;

            }

            if (listIndex != lastIndex)
            {
                if (listIndex < 0 || listIndex >= listItems.Length)
                    Program.Vibrate(0.1f, 0.1f, 50);
                ConfineInteractionIndex();
                Vector2 mousePos = GetItemPos();
                mousePos.X += 0.05f;
                mousePos.Y += 0.05f;
                Program.MoveMouse(mousePos.X, mousePos.Y);

                float freq = 1250.0f - ((((float)listIndex / (float)listItems.Length) * 5000.0f) / 5.0f);

                if (CheckComplete((GameMode)listItems[listIndex].extraData + 1))
                    Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Square);
                else
                    Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, freq, freq, 100, SignalGeneratorType.Sin);
                string itemName = listItems[listIndex].text + GetCompletionString(false);
                Console.WriteLine(itemName);
                Program.Say(itemName);
            }
            else if (intent is (InputIntent.Up or InputIntent.Down))
                Program.PlayBoundaryTone();
        }
    }
}
