using AccessibleOutput;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.XInput;

//TODO: Controller remapping is pretty ugly right now

namespace PvZA11y.Widgets
{
    class AccessibilitySettings : Widget
    {
        public bool menuClosed = false; //Hacky solution to close accessibility menu

        List<Option> options = new List<Option>();
        bool needsScreenReaderConfirmation = false;

        struct ScreenReader
        {
            public string name;
            public AccessibleOutput.IAccessibleOutput? output;
            public Config.ScreenReaderSelection selection;
        }

        List<ScreenReader> availableScreenreaders;
        int selectedScreenreader;

        struct Option
        {
            public string name;
            public string description;
            public Action confirmAction;
            public Action<InputIntent> leftRightAction;
            public Func<string>? valueGrabber;
        }

        int optionIndex;

        string GetBoolOptionValue(bool value)
        {
            return value ? "On" : "Off";
        }

        void ToggleBool(ref bool value)
        {
            value = !value;
            Config.SaveConfig();
        }

        void SetFloat(InputIntent intent, ref float value)
        {
            if (intent is InputIntent.Left)
                value -= 0.1f;
            if (intent is InputIntent.Right)
                value += 0.1f;

            value = MathF.Min(1.0f, value);
            value = MathF.Max(0.0f, value);

            Config.SaveConfig();
        }

        string GetFloatOptionAsPercentage(float value)
        {
            return MathF.Round(value * 100.0f).ToString() + "%";
        }

        void ScrollScreenReaders(InputIntent intent)
        {
            if (intent is InputIntent.Left)
                selectedScreenreader--;
            if (intent is InputIntent.Right)
                selectedScreenreader++;

            if(Config.current.WrapCursorInMenus)
            {
                selectedScreenreader = selectedScreenreader < 0 ? availableScreenreaders.Count - 1 : selectedScreenreader;
                selectedScreenreader = selectedScreenreader >= availableScreenreaders.Count ? 0 : selectedScreenreader;
            }
            else
            {
                selectedScreenreader = selectedScreenreader < 0 ? 0 : selectedScreenreader;
                selectedScreenreader = selectedScreenreader >= availableScreenreaders.Count ? availableScreenreaders.Count - 1 : selectedScreenreader;
            }
        }

        void ConfirmScreenReader()
        {
            if (Config.current.ScreenReader is not null)
                Config.current.ScreenReader.StopSpeaking();
            if (selectedScreenreader == availableScreenreaders.Count - 1)
            {
                string bigWarning = "Warning. This will disable the screenreader. If you are blind or visually impaired, this setting may be difficult to find again.";
                bigWarning += "\r\nTo Re-enable the screenreader, press the back button five times while at the main menu.";
                bigWarning += "\r\nIf you want to disable the screenreader, press the ok button again. Otherwise, press the back button now.";
                Console.WriteLine(bigWarning);
                Program.Say(bigWarning, true);

                needsScreenReaderConfirmation = true;
            }
            else
            {
                Config.current.ScreenReader = availableScreenreaders[selectedScreenreader].output;
                Config.SaveConfig(availableScreenreaders[selectedScreenreader].selection);
            }
        }

        void ConfirmScreenReaderDisable(InputIntent intent)
        {
            if (intent is InputIntent.Confirm or InputIntent.Deny)
            {
                Program.Say("", true);  //Stop speaking
                Program.PlayTone(1, 1, 400, 400, 100, SignalGeneratorType.Sin, 0);
                needsScreenReaderConfirmation = false;
            }

            if (intent is InputIntent.Confirm)
            {
                Config.current.ScreenReader = availableScreenreaders[selectedScreenreader].output;
                Config.current.screenReaderSelection = Config.ScreenReaderSelection.Disabled;
                Config.SaveConfig();    //Need to make sure people know how to re-enable screenreader if config is saved with it off
            }
        }

        string? GetCurrentScreenreaderSelection()
        {
            if (needsScreenReaderConfirmation)
                return null;

            string optionText = availableScreenreaders[selectedScreenreader].name;
            optionText += " " + options[optionIndex].name;

            Console.WriteLine(optionText);

            //Stop screenreader speech, to avoid engines talking over eachother
            foreach(var screenReader in availableScreenreaders)
            {
                if (screenReader.output != null)
                    screenReader.output.StopSpeaking();
            }

            //Use selected screenreader to read its own name, allowing the user to preview an engine before applying it
            //If on 'disable screen reader' option, use automatic screenreader to read option
            if (availableScreenreaders[selectedScreenreader].output != null)
                availableScreenreaders[selectedScreenreader].output.Speak(optionText, true);
            else
                availableScreenreaders[0].output.Speak(optionText, true);

            return null;    //Prevent option readout from reading the menu option with the currently applied (not selected) voice
        }

        void GetAvailableScreenreaders()
        {
            availableScreenreaders = new List<ScreenReader>();

            //Get list of available screenreaders
            AccessibleOutput.JawsOutput jaws = null;
            try
            {
                jaws = new AccessibleOutput.JawsOutput();
            }
            catch
            {
                //Console.WriteLine("JAWS is not installed, and Freedom Scientific is a terrible company, so I literally can not support their software properly, without it crashing for non-JAWS users.");
                //Console.WriteLine("They won't release a public API. They want to charge develoeprs money to make Freedom Scientific's own product work better with more software. Which is fucking ridiculous.");
                //Console.WriteLine("I refuse to pay them any money, out of principle. But even if I wanted to, the cheapest version costs over $1000, as I have to purchase it through a reseller, because they refuse to sell directly outside of the US.");
                //Console.WriteLine("What a pathetic excuse of a company. I hope they go bankrupt, they honestly deserve to.");
                //Console.WriteLine("All the money that was wasted in JAWS subscriptions, could be donated to a more worthwhile project like NVDA.");
                //Console.WriteLine("The world will quite literally be a better place without them");
            }

            AccessibleOutput.NvdaOutput nvda = new AccessibleOutput.NvdaOutput();
            AccessibleOutput.SapiOutput sapi = new AccessibleOutput.SapiOutput();
            IAccessibleOutput? auto = Config.AutoScreenReader();

            availableScreenreaders.Add(new ScreenReader() { name = "Automatic", output = auto, selection = Config.ScreenReaderSelection.Auto });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Auto)
                selectedScreenreader = 0;

            //Fuck Freedom Scientific
            if (jaws is not null && jaws.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "JAWS (NOT RECOMMENDED)", output = jaws, selection = Config.ScreenReaderSelection.Jaws });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Jaws)
                selectedScreenreader = availableScreenreaders.Count - 1;

            if (nvda.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "NVDA", output = nvda, selection = Config.ScreenReaderSelection.Nvda });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Nvda)
                selectedScreenreader = availableScreenreaders.Count - 1;

            if (sapi.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "SAPI", output = sapi, selection = Config.ScreenReaderSelection.Sapi });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Sapi)
                selectedScreenreader = availableScreenreaders.Count - 1;

            availableScreenreaders.Add(new ScreenReader() { name = "Deactivate", output = null, selection = Config.ScreenReaderSelection.Disabled });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Disabled)
                selectedScreenreader = availableScreenreaders.Count - 1;
            
        }
        
        bool WaitForConfirmation()
        {
            InputIntent intent = InputIntent.None;
            while (intent is not (InputIntent.Confirm or InputIntent.Deny))
                intent = Program.input.GetCurrentIntent();
            if (intent is InputIntent.Confirm)
                return true;
            return false;
        }

        void GetInputForAction(InputIntent intent, string description, ref uint pressedKey, ref GamepadButtons pressedButton)
        {
            bool buttonBound = false;
            bool keyBound = false;
            while (true)
            {

                if (buttonBound && keyBound)
                    return;

                string instruction = "";


                if(buttonBound)
                {
                    instruction = "Controller button was bound! If you would like to remap a key, press one now. Otherwise, press the same controller button again to continue.";
                    Console.WriteLine(instruction);
                    Program.Say(instruction);
                    GamepadButtons newButton = GamepadButtons.None;
                    Program.input.GetKeyOrButton(ref pressedKey, ref newButton);
                    if(newButton == pressedButton)
                    {
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                        Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                        return;
                    }
                    else
                    {
                        instruction = "Press a key for the " + intent.ToString() + " action, or press any controller button to skip.";
                        Console.WriteLine(instruction);
                        Program.Say(instruction);
                        Program.input.GetKeyOrButton(ref pressedKey, ref newButton);
                        
                        if(newButton != GamepadButtons.None)
                        {
                            //Skip grabbing key
                            Program.PlayTone(1, 1, 70, 70, 50, SignalGeneratorType.Square);
                            return;
                        }

                        string result = "Press the same key again to confirm your selection, or press something else to change it.";
                        Console.WriteLine(result);
                        Program.Say(result);
                        uint keyToUse = pressedKey;
                        Program.input.GetKeyOrButton(ref pressedKey, ref newButton);
                        if (pressedKey == keyToUse)
                        {
                            Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                            Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                            keyBound = true;
                        }
                        else
                        {
                            Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                            Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                            continue;
                        }

                    }

                    Console.WriteLine("Code should never reach here, right?");
                    continue; //This shouldn't ever get hit, right?
                }

                if (keyBound)
                {
                    instruction = "Keyboard key was bound! If you would like to map a controller button, press one now. Otherwise, press the same keyboard key again to continue.";
                    Console.WriteLine(instruction);
                    Program.Say(instruction);
                    uint newKey = 0;
                    Program.input.GetKeyOrButton(ref newKey, ref pressedButton);
                    if (newKey == pressedKey)
                    {
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                        Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                        return;
                    }
                    else
                    {
                        instruction = "Press a button for the " + intent.ToString() + " action, or press any keyboard key to skip.";
                        Console.WriteLine(instruction);
                        Program.Say(instruction);
                        Program.input.GetKeyOrButton(ref newKey, ref pressedButton);

                        //If user pressed any key, skip grabbing gamepad button
                        if (newKey != 0)
                        {
                            Program.PlayTone(1, 1, 70, 70, 50, SignalGeneratorType.Square);
                            return;
                        }

                        string result = "Press the same button again to confirm your selection, or press something else to change it.";
                        Console.WriteLine(result);
                        Program.Say(result);
                        GamepadButtons buttonToUse = pressedButton;
                        Program.input.GetKeyOrButton(ref newKey, ref pressedButton);
                        if (pressedButton == buttonToUse)
                        {
                            Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                            Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                            buttonBound = true;
                        }
                        else
                        {
                            Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                            Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                            continue;
                        }

                    }

                    Console.WriteLine("Code should never reach here, right?");
                    continue; //This shouldn't ever get hit, right?
                }


                instruction = intent.ToString();
                instruction += "\r\n" + description;
                Console.WriteLine(instruction);
                Program.Say(instruction);
                Program.input.GetKeyOrButton(ref pressedKey, ref pressedButton);

                if (pressedButton != GamepadButtons.None)
                {
                    string result = "Press the same controller button again to confirm your selection, or press something else to change it";
                    Console.WriteLine(result);
                    Program.Say(result);
                    GamepadButtons buttonToUse = pressedButton;
                    Program.input.GetKeyOrButton(ref pressedKey, ref pressedButton);
                    if (pressedButton == buttonToUse)
                    {
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                        Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                        buttonBound = true;
                    }
                    else
                    {
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                        Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                        continue;
                    }
                }

                if(pressedKey != 0)
                {
                    string result = "Press the same keyboard key again to confirm your selection, or press something else to change it";
                    Console.WriteLine(result);
                    Program.Say(result);
                    uint keyToUse = pressedKey;
                    Program.input.GetKeyOrButton(ref pressedKey, ref pressedButton);
                    if (keyToUse == pressedKey)
                    {
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                        Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                        keyBound = true;
                    }
                    else
                    {
                        Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                        Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                        continue;
                    }
                }

            }
        }

        GamepadButtons GetControllerInputForAction(RequiredInput reqInput)
        {
            while (true)
            {
                string instruction = reqInput.intent.ToString();
                instruction += "\r\n" + reqInput.description;
                Console.WriteLine(instruction);
                Program.Say(instruction);
                GamepadButtons pressedButton = Program.input.GetButton();

                //If user pressed escape while grabbing controller button, cancel this
                if (pressedButton == GamepadButtons.None)
                    return GamepadButtons.None;

                string result = "Press the same controller button again to confirm your selection, or press something else to change it";
                Console.WriteLine(result);
                Program.Say(result);
                if (pressedButton == Program.input.GetButton())
                {
                    Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                    Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                    return pressedButton;
                }
                else
                {
                    Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                    Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                    continue;
                }
            }
        }

        void SetControllerInputForAction(RequiredInput reqInput)
        {
            while (true)
            {
                string instruction = reqInput.intent.ToString();
                instruction += "\r\n" + reqInput.description;
                Console.WriteLine(instruction);
                Program.Say(instruction);
                GamepadButtons pressedButton = Program.input.GetButton();

                //If user pressed escape while grabbing controller button, cancel this
                if (pressedButton == GamepadButtons.None)
                    return;

                string result = "Press the same controller button again to confirm your selection, or press something else to change it";
                Console.WriteLine(result);
                Program.Say(result);
                if (pressedButton == Program.input.GetButton())
                {
                    Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                    Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                    if(Config.current.controllerBinds.ContainsValue(reqInput.intent))
                    {
                        Console.WriteLine("Input already added");
                        GamepadButtons origButton = Config.current.controllerBinds.FirstOrDefault(x => x.Value == reqInput.intent).Key;
                        Console.WriteLine("Original button: " + origButton.ToString());
                        Config.current.controllerBinds.Remove(origButton);
                    }
                    if(Config.current.controllerBinds.ContainsKey(pressedButton))
                    {
                        string warningStr = "Specified input is already bound to: " + Config.current.controllerBinds[pressedButton].ToString();
                        warningStr += "\r\n" + Config.current.controllerBinds[pressedButton].ToString() + " action Is now unbound!";
                        Config.current.controllerBinds.Remove(pressedButton);
                        Console.WriteLine(warningStr);
                        Program.Say(warningStr);
                    }
                    Config.current.controllerBinds.Add(pressedButton, reqInput.intent);
                    Program.input.UpdateControllerBinds(Config.current.controllerBinds);
                    return;
                }
                else
                {
                    Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                    Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                    continue;
                }
            }
        }

        uint GetKyboardInputForAction(RequiredInput reqInput)
        {
            while (true)
            {
                string instruction = reqInput.intent.ToString();
                instruction += "\r\n" + reqInput.description;
                Console.WriteLine(instruction);
                Program.Say(instruction);
                uint pressedKey = Program.input.GetKey();

                string result = "Press the same keyboard key again to confirm your selection, or press something else to change it";
                Console.WriteLine(result);
                Program.Say(result);
                if (pressedKey == Program.input.GetKey())
                {
                    Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                    Program.PlayTone(1, 1, 350, 350, 50, SignalGeneratorType.Sin, 50);
                    return pressedKey;
                }
                else
                {
                    Program.PlayTone(1, 1, 300, 300, 50, SignalGeneratorType.Sin, 0);
                    Program.PlayTone(1, 1, 250, 250, 50, SignalGeneratorType.Sin, 50);
                    continue;
                }
            }
        }

        struct RequiredInput
        {
            public InputIntent intent;
            public string description;
        }

        RequiredInput[] requiredInputs = new RequiredInput[]
        {
            new RequiredInput(){intent = InputIntent.Up, description = "Up directional input"},
            new RequiredInput(){intent = InputIntent.Down, description = "Down directional input"},
            new RequiredInput(){intent = InputIntent.Left, description = "Left directional input"},
            new RequiredInput(){intent = InputIntent.Right, description = "Right directional input"},
            new RequiredInput(){intent = InputIntent.Confirm, description = "Plant placement and confirming menu choices."},
            new RequiredInput(){intent = InputIntent.Deny, description = "Shovelling plants, and for denying menu choices."},
            new RequiredInput(){intent = InputIntent.Start, description = "Opens the pause menu, also starts the game when on the plant selector."},
            new RequiredInput(){intent = InputIntent.Option, description = "Freeze/Unfreezes all plants/zombies. Also opens the store when in the Zen Garden."},
            new RequiredInput(){intent = InputIntent.CycleLeft, description = "Cycles left between plants, zen garden tools, and store pages."},
            new RequiredInput(){intent = InputIntent.CycleRight, description = "Cycles right between plants, zen garden tools, and store pages."},
            new RequiredInput(){intent = InputIntent.Info1, description = "1 of 4. Provides additional information."},
            new RequiredInput(){intent = InputIntent.Info2, description = "2 of 4. Provides additional information."},
            new RequiredInput(){intent = InputIntent.Info3, description = "3 of 4. Provides additional information."},
            new RequiredInput(){intent = InputIntent.Info4, description = "4 of 4. Provides additional information."}
        };

        //TODO: Create sub-menu for rebinds, so users can rebind individual keys, instead of needing to rebind everything at once
        void RebindInputs()
        {
            string warningText = "Warning. This will reset all keybinds. Press confirm if you would like to proceed, or deny to go back";
            Console.WriteLine(warningText);
            Program.Say(warningText);

            InputIntent intent = InputIntent.None;
            while (intent is not (InputIntent.Deny or InputIntent.Confirm))
                intent = Program.input.GetCurrentIntent();

            if(intent is InputIntent.Deny)
                return;

            string instructions = "You will be presented with a series of input types, and a brief description of what the input is used for.";
            instructions += "\r\nYou will need to press the key or controller button that you would like to use to perform that action.";
            instructions += "\r\nPress confirm to continue. Or Deny to go back.";
            Console.WriteLine(instructions);
            Program.Say(instructions);
            if (!WaitForConfirmation())
                return;

            uint pressedKey = 0;
            GamepadButtons pressedButton = GamepadButtons.None;

            Dictionary<GamepadButtons, InputIntent> controllerBinds = new Dictionary<GamepadButtons, InputIntent>();
            Dictionary<uint, InputIntent> keyBinds = new Dictionary<uint, InputIntent>();

            foreach(var reqInput in requiredInputs)
            {
                GetInputForAction(reqInput.intent, reqInput.description, ref pressedKey, ref pressedButton);
                if (pressedKey != 0)
                    keyBinds.Add(pressedKey, reqInput.intent);
                if (pressedButton != GamepadButtons.None)
                    controllerBinds.Add(pressedButton, reqInput.intent);
            }
           
            Program.input.UpdateInputBinds(keyBinds, controllerBinds);

        }

        bool KeybindWarning(bool isKeyboard)
        {
            string warningText = "Warning. This will reset all " + (isKeyboard ? "keyboard" : "controller") + " input binds. Press confirm if you would like to proceed, or deny to go back";
            Console.WriteLine(warningText);
            Program.Say(warningText);

            InputIntent intent = InputIntent.None;
            while (intent is not (InputIntent.Deny or InputIntent.Confirm))
                intent = Program.input.GetCurrentIntent();

            if (intent is InputIntent.Confirm)
                return true;
            return false;

        }

        bool KeybindIntro(bool isKeyboard)
        {
            string instructions = "You will be presented with a series of input types, and a brief description of what the input is used for.";
            instructions += "\r\nYou will need to press the " + (isKeyboard? "keyboard key" : "controller button") + " that you would like to use to perform that action.";
            instructions += "\r\nPress confirm to continue. Or Deny to go back";
            Console.WriteLine(instructions);
            Program.Say(instructions);
            if (!WaitForConfirmation())
                return false;
            return true;
        }

        void RebindKeyboard()
        {
            if (!KeybindWarning(true))
                return;
            if (!KeybindIntro(true))
                return;

            Dictionary<uint, InputIntent> keyBinds = new Dictionary<uint, InputIntent>();

            foreach (var reqInput in requiredInputs)
            {
                uint pressedKey = GetKyboardInputForAction(reqInput);
                keyBinds.Add(pressedKey, reqInput.intent);
            }

            Program.input.UpdateKeyboardBinds(keyBinds);

        }

        void RebindController()
        {
            if (!KeybindWarning(false))
                return;
            if (!KeybindIntro(false))
                return;

            Dictionary<GamepadButtons, InputIntent> controllerBinds = new Dictionary<GamepadButtons, InputIntent>();
            foreach (var reqInput in requiredInputs)
            {
                GamepadButtons pressedButton = GetControllerInputForAction(reqInput);
                if (pressedButton != GamepadButtons.None)
                    controllerBinds.Add(pressedButton, reqInput.intent);
                else
                    return; //If user pressed escape, cancel rebinding controller input
            }

            Program.input.UpdateControllerBinds(controllerBinds);
        }

        //Really gross that we need this
        //TODO: Remove this
        void DummyLeftRightAction(InputIntent intent){}

        void InputRebindMenu(bool isController = false)
        {
            options.Clear();
            optionIndex = 0;
            hasReadContent = false;

            foreach(var reqInput in requiredInputs)
            {
                if (isController)
                    options.Add(new Option() { name = reqInput.intent.ToString(), description = reqInput.description, confirmAction = () => SetControllerInputForAction(reqInput), leftRightAction = DummyLeftRightAction });
                else
                    options.Add(new Option() { name = reqInput.intent.ToString(), description = reqInput.description, confirmAction = () => GetKyboardInputForAction(reqInput), leftRightAction = DummyLeftRightAction });
            }

            //options.Add(new Option() { name = "Up Direction", description = "Button used to move up in menus and on grids"})
        }

        void MainAccessibilityMenu()
        {
            options.Clear();
            optionIndex = 0;
            hasReadContent = false;
            
            //TODO: This could probably be done a lot nicer
            //Input
            options.Add(new Option() { name = "Menu cursor wrapping", description = "Jump to the opposite end of menus when passing the first or last item", confirmAction = () => ToggleBool(ref Config.current.WrapCursorInMenus), valueGrabber = () => GetBoolOptionValue(Config.current.WrapCursorInMenus) });
            options.Add(new Option() { name = "Grid cursor wrapping", description = "Jump to the opposite side of grids when passing the bounds", confirmAction = () => ToggleBool(ref Config.current.WrapCursorOnGrids), valueGrabber = () => GetBoolOptionValue(Config.current.WrapCursorOnGrids) });
            options.Add(new Option() { name = "Plant selection wrapping", description = "Loop selection when cycling plants and Zen Garden tools", confirmAction = () => ToggleBool(ref Config.current.WrapPlantSelection), valueGrabber = () => GetBoolOptionValue(Config.current.WrapPlantSelection) });
            options.Add(new Option() { name = "Key repetition", description = "Repeat directional inputs when held", confirmAction = () => ToggleBool(ref Config.current.KeyRepetition), valueGrabber = () => GetBoolOptionValue(Config.current.KeyRepetition) });
            options.Add(new Option() { name = "Rebind all keyboard inputs", description = "Allows you to rebind all keyboard controls.", confirmAction = RebindKeyboard });
            options.Add(new Option() { name = "Rebind all controller buttons", description = "Allows you to rebind all controller buttons.", confirmAction = RebindController });
            //options.Add(new Option() { name = "Rebind all controller buttons", description = "Allows you to rebind all controller buttons.", confirmAction = () => InputRebindMenu(true) });

            //Gameplay
            options.Add(new Option() { name = "Shovel Confirmation", description = "Require the shovel button to be pressed twice, to avoid accidental shoveling", confirmAction = () => ToggleBool(ref Config.current.RequireShovelConfirmation), valueGrabber = () => GetBoolOptionValue(Config.current.RequireShovelConfirmation) });
            options.Add(new Option() { name = "Automatic sun collection", description = "Highly recommended until sun collection has been made accessible. Automatically clicks sun, coins, and end-level rewards.", confirmAction = () => ToggleBool(ref Config.current.AutoCollectItems), valueGrabber = () => GetBoolOptionValue(Config.current.AutoCollectItems) });
            options.Add(new Option() { name = "Say board position", description = "Automatically say the current board position when the cursor is moved", confirmAction = () => ToggleBool(ref Config.current.SayTilePosOnMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayTilePosOnMove) });
            options.Add(new Option() { name = "Gameplay Tutorial", description = "Highly recommended on first playthroughs. Provides helpful gameplay advice at specific points in adventure mode.", confirmAction = () => ToggleBool(ref Config.current.GameplayTutorial), valueGrabber = () => GetBoolOptionValue(Config.current.GameplayTutorial) });

            //Core
            options.Add(new Option() { name = "Restart on crash", description = "Automatically attempt to restart the mod if it crashes", confirmAction = () => ToggleBool(ref Config.current.RestartOnCrash), valueGrabber = () => GetBoolOptionValue(Config.current.RestartOnCrash) });
            options.Add(new Option() { name = "Move mouse cursor", description = "Move the mouse cursor to visually indicate where clicks will be performed", confirmAction = () => ToggleBool(ref Config.current.MoveMouseCursor), valueGrabber = () => GetBoolOptionValue(Config.current.MoveMouseCursor) });
            //options.Add(new Option() { name = "Focus on interact", description = "Automatically bring the game window to the front when pressing a mapped key or button", confirmAction = () => ToggleBool(ref Config.current.FocusOnInteract), valueGrabber = () => GetBoolOptionValue(Config.current.FocusOnInteract) });
            options.Add(new Option() { name = "Audio Cue Volume", description = "Adjusts the volume of all non-speech audio cues", leftRightAction = (intent) => SetFloat(intent, ref Config.current.AudioCueVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.AudioCueVolume) });
            options.Add(new Option() { name = "Screen Reader Engine. Press confirm to apply", description = "Which screen reader engine to use", leftRightAction = ScrollScreenReaders, valueGrabber = GetCurrentScreenreaderSelection, confirmAction = ConfirmScreenReader });

            //options.Add(new Option() { name = "Delete and Rebind all inputs", description = "Deletes all keyboard and controllers keybinds, then allows you to rebind them to new buttons", confirmAction = RebindInputs });

            //options.Add(new Option() { name = "Close Menu", description = "Closes the accessibility menu", confirmAction = () => menuClosed = true });
        }

        public AccessibilitySettings(MemoryIO memIO) : base(memIO, "")
        {
            GetAvailableScreenreaders();
            MainAccessibilityMenu();
        }

        void ReadOptionText(bool readStateFirst = false)
        {
            string optionText = "";
            if (options[optionIndex].valueGrabber != null)
            {
                optionText = options[optionIndex].valueGrabber();
                if (optionText is null)
                    return;
            }

            if (optionText == "")
                optionText = options[optionIndex].name;
            else if (readStateFirst)
                optionText += " : " + options[optionIndex].name;
            else
                optionText = options[optionIndex].name + " : " + optionText;

            Console.WriteLine(optionText);
            Program.Say(optionText, true);
        }

        public override void Interact(InputIntent intent)
        {
            
            if(needsScreenReaderConfirmation)
            {
                ConfirmScreenReaderDisable(intent);
                return;
            }
            
            if (intent is InputIntent.Up)
                optionIndex--;
            if (intent is InputIntent.Down)
                optionIndex++;

            if (Config.current.WrapCursorInMenus)
            {
                optionIndex = optionIndex < 0 ? options.Count -1 : optionIndex;
                optionIndex = optionIndex > options.Count - 1 ? 0 : optionIndex;
            }
            else
            {
                optionIndex = optionIndex < 0 ? 0 : optionIndex;
                optionIndex = optionIndex > options.Count - 1 ? options.Count - 1 : optionIndex;
            }

            if(intent is InputIntent.Up or InputIntent.Down)
            {
                ReadOptionText();
                float frequency = 400 + (((options.Count - optionIndex) + 1) * 100);
                Program.PlayTone(1.0f, 1.0f, frequency, frequency, 100, SignalGeneratorType.Sin, 0);
            }

            if(intent is InputIntent.Info1)
            {
                Console.WriteLine(options[optionIndex].description);
                Program.Say(options[optionIndex].description, true);
            }

            if (intent is InputIntent.Confirm)
            {
                if (options[optionIndex].confirmAction != null)
                    options[optionIndex].confirmAction();

                ReadOptionText(true);
                Program.PlayTone(1.0f, 1.0f, 400, 400, 50, SignalGeneratorType.Sin, 0);
                Program.PlayTone(1.0f, 1.0f, 450, 450, 50, SignalGeneratorType.Sin, 50);
            }

            if (intent is InputIntent.Left or InputIntent.Right)
            {
                if (options[optionIndex].leftRightAction != null)
                    options[optionIndex].leftRightAction(intent);
                else if (options[optionIndex].confirmAction != null)
                    options[optionIndex].confirmAction();

                Program.PlayTone(1.0f,1.0f, 400, 400, 50, SignalGeneratorType.Sin, 0);

                ReadOptionText(true);
            }

            if (intent is InputIntent.Deny)
                menuClosed = true;

        }
    }
}
