using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Vortice.XInput;
using static System.Windows.Forms.Design.AxImporter;

namespace PvZA11y
{
    public class InputRebind
    {
        int inputIndex = 0;

        struct RequiredInput
        {
            public InputIntent intent;
            public string description;
        }

        RequiredInput[] requiredInputs = new RequiredInput[]
        {
            new RequiredInput(){intent = InputIntent.Up, description = "Scroll menu up, directional input for grid navigation."},
            new RequiredInput(){intent = InputIntent.Down, description = "Scroll menu down, directional input for board navigation."},
            new RequiredInput(){intent = InputIntent.Left, description = "Adjust menu value, directional input for board navigation."},
            new RequiredInput(){intent = InputIntent.Right, description = "Adjust menu value, directional input for board navigation."},
            new RequiredInput(){intent = InputIntent.Confirm, description = "Plant placement and confirming menu choices."},
            new RequiredInput(){intent = InputIntent.Deny, description = "Shovel plants, deny menu choices."},
            new RequiredInput(){intent = InputIntent.Start, description = "Open the pause menu, start game from plant selector."},
            new RequiredInput(){intent = InputIntent.Option, description = "Toggle freeze mode, open the store from Zen Garden, start wave in Last Stand"},
            new RequiredInput(){intent = InputIntent.CycleLeft, description = "Cycle left between plants, zen garden tools, and store pages."},
            new RequiredInput(){intent = InputIntent.CycleRight, description = "Cycle right between plants, zen garden tools, and store pages."},
            new RequiredInput(){intent = InputIntent.ZombieMinus, description = "Cycle backwards through zombies on the lawn."},
            new RequiredInput(){intent = InputIntent.ZombiePlus, description = "Cycle forwards through zombies on the lawn."},
            new RequiredInput(){intent = InputIntent.Info1, description = "Zombie sonar, repeat dialogue messages, say upcoming zombies while on plant selector, rename user when in user picker dialogue, say current coin balance in store, say zen garden plant name"},
            new RequiredInput(){intent = InputIntent.Info2, description = "Plant and object information for current tile, say level type while on plant selector, delete user when in user picker dialogue, say zen garden plant status"},
            new RequiredInput(){intent = InputIntent.Info3, description = "Say current sun count, say number of needy plants in zen garden, spin slots in SlotMachine minigames"},
            new RequiredInput(){intent = InputIntent.Info4, description = "Say level progress, play audio cue if the current row has a lawnmower or a brain in iZombie levels."},
            new RequiredInput(){intent = InputIntent.Slot1, description = "Instantly select plant/tool slot 1."},
            new RequiredInput(){intent = InputIntent.Slot2, description = "Instantly select plant/tool slot 2."},
            new RequiredInput(){intent = InputIntent.Slot3, description = "Instantly select plant/tool slot 3."},
            new RequiredInput(){intent = InputIntent.Slot4, description = "Instantly select plant/tool slot 4."},
            new RequiredInput(){intent = InputIntent.Slot5, description = "Instantly select plant/tool slot 5."},
            new RequiredInput(){intent = InputIntent.Slot6, description = "Instantly select plant/tool slot 6."},
            new RequiredInput(){intent = InputIntent.Slot7, description = "Instantly select plant/tool slot 7."},
            new RequiredInput(){intent = InputIntent.Slot8, description = "Instantly select plant/tool slot 8."},
            new RequiredInput(){intent = InputIntent.Slot9, description = "Instantly select plant/tool slot 9."},
            new RequiredInput(){intent = InputIntent.Slot10, description = "Instantly select plant/tool slot 10."},
        };

        public InputRebind()
        {
            SayCurrentOption(Config.current.SayAvailableInputs ? "\r\nInputs: Confirm to rebind, Deny to close, Info1 to repeat, Info2 to say bound keys/buttons, Directional up and down to scroll list." : "");
        }

        string GetBoundInputStr()
        {
            string controllerStr = "";

            if (Config.current.controllerBinds.ContainsValue(requiredInputs[inputIndex].intent))
                controllerStr += "Controller: " + Config.current.controllerBinds.FirstOrDefault((x) => x.Value == requiredInputs[inputIndex].intent).Key.ToString();

            //TODO: Make these changable
            if (requiredInputs[inputIndex].intent >= InputIntent.Slot1 && requiredInputs[inputIndex].intent <= InputIntent.Slot10)
                controllerStr += (controllerStr.Length > 0 ? " and " : "Controller: ") + "Right Stick";

            if (requiredInputs[inputIndex].intent >= InputIntent.Up && requiredInputs[inputIndex].intent <= InputIntent.Right)
                controllerStr += (controllerStr.Length > 0 ? " and " : "Controller: ") + "Left Stick";

            if (requiredInputs[inputIndex].intent is InputIntent.ZombieMinus)
                controllerStr += (controllerStr.Length > 0 ? " and " : "Controller: ") + "Left Trigger";

            if (requiredInputs[inputIndex].intent is InputIntent.ZombiePlus)
                controllerStr += (controllerStr.Length > 0 ? " and " : "Controller: ") + "Right Trigger";

            if (Config.current.keyBinds.ContainsValue(requiredInputs[inputIndex].intent))
            {
                string keyName = ((System.Windows.Forms.Keys)Config.current.keyBinds.FirstOrDefault((x) => x.Value == requiredInputs[inputIndex].intent).Key).ToString();
                keyName = keyName.Replace("Oem", "");
                if (keyName[0] == 'D' && keyName[1] >= '0' && keyName[1] <= '9')
                    keyName = keyName[1].ToString();
                controllerStr += ", Keyboard: " + keyName;
            }

            return controllerStr;
        }

        void SayCurrentOption(string? prependStr = null)
        {
            if (prependStr == null)
                prependStr = "";

            string thisInputStr = prependStr + requiredInputs[inputIndex].intent.ToString() + ", " + requiredInputs[inputIndex].description;

            thisInputStr += " " + GetBoundInputStr();

            Console.WriteLine(thisInputStr);
            Program.Say(thisInputStr);

            float frequency = 400 + (((requiredInputs.Length - inputIndex) + 1) * 100);
            Program.PlayTone(1.0f, 1.0f, frequency, frequency, 100, SignalGeneratorType.Sin, 0);
        }

        public bool HandleInput()
        {
            InputIntent intent = Program.input.GetCurrentIntent();
            if (intent is InputIntent.Deny)
                return false;

            if (intent is InputIntent.Up or InputIntent.Down)
            {
                if (intent is InputIntent.Up)
                    inputIndex--;
                else
                    inputIndex++;

                if (Config.current.WrapCursorInMenus)
                {
                    if (inputIndex < 0)
                        inputIndex = requiredInputs.Length - 1;
                    if (inputIndex >= requiredInputs.Length)
                        inputIndex = 0;
                }
                else
                {
                    if (inputIndex < 0)
                        inputIndex = 0;
                    if (inputIndex >= requiredInputs.Length)
                        inputIndex = requiredInputs.Length - 1;
                }

            }

            if (intent is InputIntent.Up or InputIntent.Down or InputIntent.Info1)
                SayCurrentOption();
            if(intent is InputIntent.Info2)
            {
                string boundInputs = GetBoundInputStr();
                Console.WriteLine(boundInputs);
                Program.Say(boundInputs);
            }

            if (intent is InputIntent.Confirm)
            {
                string? result = BindInput();
                Program.input.ClearIntents();

                SayCurrentOption(result);
                Program.input.WaitForNoInput();
            }


            return true;
        }

        string? BindInput()
        {
            //Try grabbing keyboard or controller input
            RequiredInput reqInput = requiredInputs[inputIndex];
            string instruction = reqInput.intent.ToString();
            instruction += ", Press a keyboard or controller button to map to this input, or hold escape to cancel.";
            Console.WriteLine(instruction);
            Program.Say(instruction);

            GamepadButtons pressedButton = GamepadButtons.None;
            uint pressedKey = 0;

            Program.input.GetKeyOrButton(ref pressedKey, ref pressedButton);

            //If user pressed escape while grabbing controller button, cancel this
            if (pressedButton == GamepadButtons.None && pressedKey == 0)
            {
                Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                return null;
            }

            bool wasControllerBind = pressedButton != GamepadButtons.None;

            Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
            Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);

            string resultString = "";

            if (wasControllerBind)
            {
                if (Config.current.controllerBinds.ContainsValue(reqInput.intent))
                {
                    GamepadButtons origButton = Config.current.controllerBinds.FirstOrDefault(x => x.Value == reqInput.intent).Key;
                    Config.current.controllerBinds.Remove(origButton);
                    //resultString += "Input already added. Original button: " + origButton.ToString();
                }
                if (Config.current.controllerBinds.ContainsKey(pressedButton))
                {
                    resultString = "Specified button was already bound to: " + Config.current.controllerBinds[pressedButton].ToString();
                    resultString += "\r\nController input for " + Config.current.controllerBinds[pressedButton].ToString() + " action is now unbound!";
                    Config.current.controllerBinds.Remove(pressedButton);
                }
                Config.current.controllerBinds.Add(pressedButton, reqInput.intent);
                Program.input.UpdateControllerBinds(Config.current.controllerBinds);
            }
            else
            {
                if (Config.current.keyBinds.ContainsValue(reqInput.intent))
                {
                    uint origKey = Config.current.keyBinds.FirstOrDefault(x => x.Value == reqInput.intent).Key;
                    Config.current.keyBinds.Remove(origKey);
                    //resultString += "Input already added. Original key: " + origKey.ToString();
                }
                if (Config.current.keyBinds.ContainsKey(pressedKey))
                {
                    resultString = "Specified key was already bound to: " +  Config.current.keyBinds[pressedKey].ToString();
                    resultString += "\r\nKeyboard input for " + Config.current.keyBinds[pressedKey].ToString() + " action is now unbound!";
                    Config.current.keyBinds.Remove(pressedKey);
                }
                Config.current.keyBinds.Add(pressedKey, reqInput.intent);
                Program.input.UpdateKeyboardBinds(Config.current.keyBinds);
            }
            resultString += (resultString.Length > 0 ? "\r\n" : "") + "Input bound!";
            return resultString + "\r\n";
        }

    }
}
