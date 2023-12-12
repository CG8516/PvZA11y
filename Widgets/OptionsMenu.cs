using Memory;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PvZA11y.Widgets
{
    class OptionsMenu : Dialog
    {
        public Widget? previousWidget;
        AccessibilitySettings? accessibilitySettings = null;
        
        static ListItem[] InitListItems(MemoryIO memIO, string ptrChain, Widget? prevWidget = null)
        {
            //Stupid hack to wait for menu to finish loading before reading
            string continueString = "";
            Console.WriteLine("Entering pause menu wait loop");
            while (continueString == null || continueString.Length < 2)
            {
                Task.Delay(10).Wait();
                continueString = memIO.mem.ReadString(ptrChain + memIO.ptr.optionsMenuContinueOffset + memIO.ptr.widgetDialogStringOffset);  //"OK" at main menu, "Return To Game" in game
            }
            Console.WriteLine("Got pause menu contents");

            Vector2 baseSize = memIO.GetWidgetSize(ptrChain);

            //Read positions and visibility of menu options

            //float musicSliderX = memIO.mem.ReadInt(ptrChain + ",174,40") / baseSize.X + 0.002f;
            Vector2 posOffset = new Vector2(0.002f, 0.002f);

            Vector2 musicSliderPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuMusicSliderOffset) / baseSize;
            musicSliderPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuMusicSliderOffset) / baseSize / 2;

            Vector2 sfxSliderPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuSfxSliderOffset) / baseSize;
            sfxSliderPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuSfxSliderOffset) / baseSize / 2;

            Vector2 fullscreenButtonPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuFullscreenOffset) / baseSize;
            fullscreenButtonPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuFullscreenOffset) / baseSize / 1.5f;

            Vector2 accelButtonPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenu3DAccelOffset) / baseSize;
            accelButtonPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenu3DAccelOffset) / baseSize / 1.5f;

            Vector2 almanacPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuAlmanacOffset) / baseSize;
            almanacPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuAlmanacOffset) / baseSize / 2;
            bool almanacVisible = memIO.mem.ReadByte(ptrChain + memIO.ptr.optionsMenuAlmanacOffset + ",64") > 0;

            Vector2 mainMenuPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuReturnToMainOffset) / baseSize;
            mainMenuPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuReturnToMainOffset) / baseSize / 2;
            bool mainMenuVisible = memIO.mem.ReadByte(ptrChain + memIO.ptr.optionsMenuReturnToMainOffset + ",64") > 0;

            Vector2 restartPos = memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuRestartOffset) / baseSize;
            restartPos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuRestartOffset) / baseSize / 2;
            bool restartVisible = memIO.mem.ReadByte(ptrChain + memIO.ptr.optionsMenuRestartOffset + ",64") > 0;

            Vector2 continuePos = (memIO.GetWidgetPos(ptrChain + memIO.ptr.optionsMenuContinueOffset) / baseSize);
            continuePos += memIO.GetWidgetSize(ptrChain + memIO.ptr.optionsMenuContinueOffset) / baseSize / 2;

            int itemCount = 5 + (almanacVisible ? 1 : 0) + (mainMenuVisible ? 1 : 0) + (restartVisible ? 1 : 0) +1; //+1 for accessibility options

            ListItem[] listItems = new ListItem[itemCount];

            listItems[0] = new ListItem() { text = "Music Volume Slider", relativePos = musicSliderPos };
            listItems[1] = new ListItem() { text = "SFX Volume Slider", relativePos = sfxSliderPos };
            listItems[2] = new ListItem() { text = "3D Acceleration Checkbox", relativePos = accelButtonPos };
            listItems[3] = new ListItem() { text = "Fullscreen Checkbox", relativePos = fullscreenButtonPos };
            int itemIndex = 4;
            if (almanacVisible)
                listItems[itemIndex++] = new ListItem() { text = "View Almanac", relativePos = almanacPos };
            if (restartVisible)
                listItems[itemIndex++] = new ListItem() { text = "Restart Level", relativePos = restartPos };
            if (mainMenuVisible)
            {
                if(prevWidget != null && prevWidget is Widgets.MainMenu)
                    listItems[itemIndex++] = new ListItem() { text = "Credits", relativePos = mainMenuPos };
                else
                    listItems[itemIndex++] = new ListItem() { text = "Main Menu", relativePos = mainMenuPos };
            }

            listItems[itemIndex++] = new ListItem() { text = "Accessibility Settings", relativePos = Vector2.Zero };

            listItems[itemIndex] = new ListItem() { text = continueString, relativePos = continuePos };

            return listItems;

        }

        public OptionsMenu(MemoryIO memIO, string pointerChain, Widget? prevWidget = null) : base(memIO, pointerChain, InitListItems(memIO, pointerChain, prevWidget))
        {
            previousWidget = prevWidget;
        }

        public override void ConfirmInteraction()
        {
            //Don't process enter/confirm input on sliders
            if (listIndex <= 1)
                return;

            base.ConfirmInteraction();

            if (listIndex == 2 || listIndex == 3)
                hasUpdatedContents = true;

            //Accessibility settings hack
            if (listIndex == listItems.Length - 2)
                accessibilitySettings = new Widgets.AccessibilitySettings(memIO);
        }

        public override void Interact(InputIntent intent)
        {
            //Update position, in case dialogue was moved
            relativePos = memIO.GetWidgetPos(pointerChain);
            relativePos /= new Vector2(800.0f, 600.0f);
            
            if(accessibilitySettings != null)
            {
                accessibilitySettings.Interact(intent);
                if (accessibilitySettings.menuClosed)
                {
                    accessibilitySettings = null;
                    hasUpdatedContents = true;
                }

                return;
            }
            
            //Close if back/pause is pressed
            if(intent == InputIntent.Start || intent == InputIntent.Deny)
            {
                listIndex = listItems.Length - 1;
                ConfirmInteraction();
                //Program.Click(relativePos + listItems[listItems.Length-1].relativePos);
                //return;
            }
            
            base.Interact(intent);  //Up/down movement

            //Process left/right input on volume sliders
            if(listIndex <= 1)
            {
                if (intent == InputIntent.Left || intent == InputIntent.Right)
                {
                    //TODO: Move to memoryIO/Pointers
                    //TODO: Verify pointers work for multiple game versions
                    string sliderPtr = pointerChain + (listIndex == 0 ? memIO.ptr.optionsMenuMusicSliderOffset : memIO.ptr.optionsMenuSfxSliderOffset);
                    Vector2 sliderPos = memIO.GetWidgetPos(sliderPtr);
                    Vector2 sliderSize = memIO.GetWidgetSize(sliderPtr);

                    //sliderPos.Y += 0.03f;

                    float sliderPercent = memIO.GetSliderPercentage(sliderPtr);
                    sliderPercent = MathF.Max(sliderPercent, 0.0f);
                    sliderPercent = MathF.Min(sliderPercent, 1.0f);

                    float handleStartX = (sliderPos.X + sliderSize.X * sliderPercent);

                    if (sliderPercent > 0.9f)
                        handleStartX -= 0.01f;
                    if (sliderPercent < 0.1f)
                    handleStartX += 0.01f;

                    if (intent == InputIntent.Left)
                        sliderPercent -= 0.05f;
                    if (intent == InputIntent.Right)
                        sliderPercent += 0.05f;




                    sliderPercent = MathF.Max(sliderPercent, 0.0f);
                    sliderPercent = MathF.Min(sliderPercent, 1.0f);

                    float handleTargetX = (sliderPos.X + sliderSize.X * sliderPercent);

                    Vector2 downPos = relativePos + new Vector2((handleStartX) / 800.0f, (sliderPos.Y) / 600.0f);
                    Vector2 upPos = relativePos + new Vector2((handleTargetX) / 800.0f, (sliderPos.Y) / 600.0f);

                    float frequency = 400 + (200.0f * sliderPercent);

                    //Play sound when music slider moves, to indicate current volume (sfx slider already has one built in)
                    if (listIndex == 0)
                        Program.PlayTone(sliderPercent, sliderPercent, frequency, frequency, 50, SignalGeneratorType.Sin);

                    Program.Click(downPos.X, downPos.Y, upPos.X, upPos.Y);  //Drag slider
                }
            }

            //Ensure checkbox text is accurate
            bool accelChecked = memIO.mem.ReadByte(pointerChain + memIO.ptr.optionsMenu3DAccelOffset + ",a8") == 1;
            bool fullscreenChecked = memIO.mem.ReadByte(pointerChain + memIO.ptr.optionsMenuFullscreenOffset + ",a8") == 1;

            string accelText = (accelChecked ? "Checked: " : "Unchecked: ") + "3D Acceleration";
            string fullscreenText = (fullscreenChecked ? "Checked: " : "Unchecked: ") + "Full Screen";

            listItems[2].text = accelText;
            listItems[3].text = fullscreenText;
        }

        protected override string? GetContent()
        {
            //Read title
            int titleLen = memIO.mem.ReadInt(pointerChain + memIO.ptr.dialogTitleLenOffset);
            string titleString = "";
            if (titleLen <= 15)
                titleString = memIO.mem.ReadString(pointerChain + memIO.ptr.dialogTitleStrOffset);
            else
                titleString = memIO.mem.ReadString(pointerChain + memIO.ptr.dialogTitleStrOffset + ",0");

            return titleString;
        }

        protected override string? GetContentUpdate()
        {
            return listItems[listIndex].text;
        }
    }
}
