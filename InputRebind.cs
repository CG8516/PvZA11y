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
            new RequiredInput(){intent = InputIntent.None, description = Text.inputRebind.resetKeyboard},
            new RequiredInput(){intent = InputIntent.None, description =  Text.inputRebind.resetController},
            new RequiredInput(){intent = InputIntent.None, description =  Text.inputRebind.setKeyboard},
            new RequiredInput(){intent = InputIntent.None, description =  Text.inputRebind.setController},

            new RequiredInput(){intent = InputIntent.Up, description =  Text.inputRebind.inputDescriptions.up},
            new RequiredInput(){intent = InputIntent.Down, description = Text.inputRebind.inputDescriptions.down},
            new RequiredInput(){intent = InputIntent.Left, description = Text.inputRebind.inputDescriptions.left},
            new RequiredInput(){intent = InputIntent.Right, description = Text.inputRebind.inputDescriptions.right},
            new RequiredInput(){intent = InputIntent.Confirm, description = Text.inputRebind.inputDescriptions.confirm},
            new RequiredInput(){intent = InputIntent.Deny, description = Text.inputRebind.inputDescriptions.deny},
            new RequiredInput(){intent = InputIntent.Start, description = Text.inputRebind.inputDescriptions.start},
            new RequiredInput(){intent = InputIntent.Option, description = Text.inputRebind.inputDescriptions.option},
            new RequiredInput(){intent = InputIntent.CycleLeft, description = Text.inputRebind.inputDescriptions.cycleLeft},
            new RequiredInput(){intent = InputIntent.CycleRight, description = Text.inputRebind.inputDescriptions.cycleRight},
            new RequiredInput(){intent = InputIntent.ZombieMinus, description = Text.inputRebind.inputDescriptions.zombieMinus},
            new RequiredInput(){intent = InputIntent.ZombiePlus, description = Text.inputRebind.inputDescriptions.zombiePlus},
            new RequiredInput(){intent = InputIntent.Info1, description = Text.inputRebind.inputDescriptions.info1},
            new RequiredInput(){intent = InputIntent.Info2, description = Text.inputRebind.inputDescriptions.info2},
            new RequiredInput(){intent = InputIntent.Info3, description = Text.inputRebind.inputDescriptions.info3},
            new RequiredInput(){intent = InputIntent.Info4, description = Text.inputRebind.inputDescriptions.info4},
            new RequiredInput(){intent = InputIntent.Slot1, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot2, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot3, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot4, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot5, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot6, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot7, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot8, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot9, description = Text.inputRebind.inputDescriptions.slot},
            new RequiredInput(){intent = InputIntent.Slot10, description = Text.inputRebind.inputDescriptions.slot},
        };

        public InputRebind()
        {
            SayCurrentOption(Config.current.SayAvailableInputs ? "\r\n" + Text.inputs.rebindMenu : "");
        }

        string GetBoundInputStr()
        {            
            string controllerStr = "";
            string controllerButton = "";
            string controllerButton2 = "";

            if (Config.current.controllerBinds.ContainsValue(requiredInputs[inputIndex].intent))
                controllerButton = Config.current.controllerBinds.FirstOrDefault((x) => x.Value == requiredInputs[inputIndex].intent).Key.ToString();

            

            //TODO: Make these changable
            if (requiredInputs[inputIndex].intent >= InputIntent.Slot1 && requiredInputs[inputIndex].intent <= InputIntent.Slot10)
                controllerButton2 = Text.inputRebind.rightStick;

            if (requiredInputs[inputIndex].intent >= InputIntent.Up && requiredInputs[inputIndex].intent <= InputIntent.Right)
                controllerButton2 = Text.inputRebind.leftStick;

            if (requiredInputs[inputIndex].intent is InputIntent.ZombieMinus)
                controllerButton2 = Text.inputRebind.leftTrigger;

            if (requiredInputs[inputIndex].intent is InputIntent.ZombiePlus)
                controllerButton2 = Text.inputRebind.rightTrigger;

            if (controllerButton != "" && controllerButton2 != "")
                controllerStr = Text.inputRebind.controllerExtraBind.Replace("[0]", controllerButton).Replace("[1]", controllerButton2);
            else if (controllerButton != "")
                controllerStr = Text.inputRebind.controllerBind.Replace("[0]", controllerButton);
            else if (controllerButton2 != "")
                controllerStr = Text.inputRebind.controllerBind.Replace("[0]", controllerButton2);

            if (Config.current.keyBinds.ContainsValue(requiredInputs[inputIndex].intent))
            {
                uint keyCode = 0;
                try
                {
                    keyCode = Config.current.keyBinds.FirstOrDefault((x) => x.Value == requiredInputs[inputIndex].intent).Key;
                    string keyName = ((System.Windows.Forms.Keys)keyCode).ToString();
                    keyName = keyName.Replace("Oem", "");
                    if (keyName[0] == 'D' && keyName[1] >= '0' && keyName[1] <= '9')
                        keyName = keyName[1].ToString();
                    controllerStr += ", " + Text.inputRebind.keyboardBind.Replace("[0]",keyName);
                }
                catch
                {
                    controllerStr += ", " + Text.inputRebind.keyboardBind.Replace("[0]",Text.inputRebind.unknownKey) + " " + keyCode.ToString();
                }
            }

            return controllerStr;
        }

        void SayCurrentOption(string? prependStr = null)
        {               
            if (prependStr == null)
                prependStr = "";
            string description = requiredInputs[inputIndex].description.Replace("[0]", (inputIndex - 16).ToString());
            string thisInputStr = prependStr + InputName(requiredInputs[inputIndex].intent) + ", " + description;

            thisInputStr += " " + GetBoundInputStr();

            if (requiredInputs[inputIndex].intent == InputIntent.None)
                thisInputStr = prependStr + description;

            Console.WriteLine(thisInputStr);
            Program.Say(thisInputStr);

            float frequency = 400 + (((requiredInputs.Length - inputIndex) + 1) * 100);
            Program.PlayTone(1.0f, 1.0f, frequency, frequency, 100, SignalGeneratorType.Sin, 0);
        }

        public string InputName(int index)
        {
            switch(index)
            {
                case 0:
                    return Text.inputRebind.inputNames.up;
                case 1:
                    return Text.inputRebind.inputNames.down;
                case 2:
                    return Text.inputRebind.inputNames.left;
                case 3:
                    return Text.inputRebind.inputNames.right;
                case 4:
                    return Text.inputRebind.inputNames.confirm;
                case 5:
                    return Text.inputRebind.inputNames.deny;
                case 6:
                    return Text.inputRebind.inputNames.start;
                case 7:
                    return Text.inputRebind.inputNames.option;
                case 8:
                    return Text.inputRebind.inputNames.cycleLeft;
                case 9:
                    return Text.inputRebind.inputNames.cycleRight;
                case 10:
                    return Text.inputRebind.inputNames.zombieMinus;
                case 11:
                    return Text.inputRebind.inputNames.zombiePlus;
                case 12:
                    return Text.inputRebind.inputNames.info1;
                case 13:
                    return Text.inputRebind.inputNames.info2;
                case 14:
                    return Text.inputRebind.inputNames.info3;
                case 15:
                    return Text.inputRebind.inputNames.info4;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    return Text.inputRebind.inputNames.slot.Replace("[0]",(index-16).ToString());
            }
            return "";
        }

        public string InputName(InputIntent intent)
        {
            return InputName(((int)intent) - 1);
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
            if(intent is InputIntent.Info2 && inputIndex > 3)
            {
                string boundInputs = GetBoundInputStr();
                Console.WriteLine(boundInputs);
                Program.Say(boundInputs);
            }

            if (intent is InputIntent.Confirm)
            {
                if (inputIndex == 0)
                {
                    Program.input.StopThread();
                    Config.current.keyBinds = new Dictionary<uint, InputIntent>();
                    Program.input = new Input();
                    SayCurrentOption(Text.inputRebind.keyboardReset + "\r\n");
                }
                else if (inputIndex == 1)
                {
                    Program.input.StopThread();
                    Config.current.controllerBinds = new Dictionary<GamepadButtons, InputIntent>();
                    Program.input = new Input();
                    SayCurrentOption(Text.inputRebind.controllerReset + "\r\n");
                }
                else if (inputIndex == 2)
                    BindAllInputs(false);
                else if(inputIndex == 3)
                    BindAllInputs(true);
                else
                {
                    string? result = BindInput();
                    SayCurrentOption(result);
                }
                Program.input.ClearIntents();
                Program.input.WaitForNoInput();
            }


            return true;
        }

        void BindAllInputs(bool controller = false)
        {
            for(int i =0; i < requiredInputs.Length; i++)
            {
                InputIntent intent = requiredInputs[i].intent;

                if (intent is InputIntent.None)
                    continue;

                if (controller)
                {
                    if (intent is InputIntent.ZombieMinus or InputIntent.ZombiePlus)
                        continue;
                    if (intent >= InputIntent.Slot1 && intent <= InputIntent.Slot10)
                        continue;
                    
                    string? result = BindInput(i, false, true);
                    if (result == null)
                        return;
                }
                else
                {
                    string? result = BindInput(i, true, false);
                    if (result == null)
                        return;
                }
            }

            SayCurrentOption(Text.inputRebind.rebindComplete + "\r\n");
        }

        string? BindInput(int index = -1, bool allowKeyboard = true, bool allowController = true)
        {
            if (index == -1)
                index = inputIndex;
            //Try grabbing keyboard or controller input
            RequiredInput reqInput = requiredInputs[index];
            string instruction = InputName(index-4);

            if (allowKeyboard && allowController)
                instruction += ". " + Text.inputRebind.pressKeyboardOrController;
            else if(allowKeyboard)
                instruction += ". " + Text.inputRebind.pressKeyboard;
            else if(allowController)
                instruction += ". " + Text.inputRebind.pressController;

            Console.WriteLine(instruction);
            Program.Say(instruction);

            GamepadButtons pressedButton = GamepadButtons.None;
            uint pressedKey = 0;

            Program.input.GetKeyOrButton(ref pressedKey, ref pressedButton, allowKeyboard, allowController);

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
                    resultString = Text.inputRebind.buttonAlreadyBound.Replace("[0]", InputName(Config.current.controllerBinds[pressedButton]));
                    resultString += "\r\n" + Text.inputRebind.buttonUnbound.Replace("[0]", InputName(Config.current.controllerBinds[pressedButton]));
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
                    resultString = Text.inputRebind.keyAlreadyBound.Replace("[0]", InputName(Config.current.keyBinds[pressedKey]));
                    resultString += "\r\n" + Text.inputRebind.keyUnbound.Replace("[0]", InputName(Config.current.keyBinds[pressedKey]));
                    Config.current.keyBinds.Remove(pressedKey);
                }
                Config.current.keyBinds.Add(pressedKey, reqInput.intent);
                Program.input.UpdateKeyboardBinds(Config.current.keyBinds);
            }
            resultString += (resultString.Length > 0 ? "\r\n" : "") + Text.inputRebind.inputBound;
            return resultString + "\r\n";
        }

    }
}
