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

        string GetZombieSonarValue(int value)
        {
            switch(value)
            {
                case 1:
                    return "Full Sonar";
                case 2:
                    return "Beeps only";
                case 3:
                    return "Count only";
                default:
                    return "Off";
            }
        }

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

        void SetIntValue(InputIntent intent, ref int value, int options)
        {
            if (intent is InputIntent.Left)
                value--;
            if (intent is InputIntent.Right)
                value++;

            if (value < 0)
                value = options-1;
            if (value >= options)
                value = 0;

            Config.SaveConfig();
        }

        string GetBeghouledMode(int value)
        {
            if (value == 1)
                return "Easiest, When current plant can be dragged to make a match.";
            if (value == 2)
                return "Medium, When current plant can be part of a match, but might not be the one which needs to be dragged.";
            return "Hardest, None.";
        }

        string GetZombieSweepInterval(int value)
        {
            switch(value)
            {
                case 1:
                    return "0.5 seconds";
                case 2:
                    return "1 second";
                case 3:
                    return "2 seconds";
                case 4:
                    return "3 seconds";
                default:
                    return "Off";
            }
        }

        string GetZombieCycleMode(int value)
        {
            if (value == 0)
                return "By ID . Ensures all zombies are cycled through in the same order.";
            return "By distance. Cycle order will change based on zombie distance.";
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
                //AccessibleOutput throws an exception while trying to load JAWS on a system without it installed, as it requires COM interop with their propriatry library.
                //Would be nice if we could bundle a dll, but Freedom Scientific are a terrible company, and refuse to make their API public.
                //Instead, they want to charge us money to make 'their' software more useful for their users.
                //I hope their greed burns them one day.
            }

            AccessibleOutput.NvdaOutput nvda = new AccessibleOutput.NvdaOutput();
            AccessibleOutput.SapiOutput sapi = new AccessibleOutput.SapiOutput();
            IAccessibleOutput? auto = Config.AutoScreenReader();

            availableScreenreaders.Add(new ScreenReader() { name = "Automatic", output = auto, selection = Config.ScreenReaderSelection.Auto });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Auto)
                selectedScreenreader = 0;

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

        //Really gross that we need this
        //TODO: Remove this
        void DummyLeftRightAction(InputIntent intent){}

        void InputRebindMenu()
        {
            InputRebind rebindMenu = new InputRebind();
            while (rebindMenu.HandleInput()) ;

            return;
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
            options.Add(new Option() { name = "Rebind inputs", description = "Allows you to rebind all controls.", confirmAction = InputRebindMenu, leftRightAction = DummyLeftRightAction });
            options.Add(new Option() { name = "Say available inputs", description = "Say available key/button inputs in menus.", confirmAction = () => ToggleBool(ref Config.current.SayAvailableInputs), valueGrabber = () => GetBoolOptionValue(Config.current.SayAvailableInputs) });

            //Gameplay
            options.Add(new Option() { name = "Shovel Confirmation", description = "Require the shovel button to be pressed twice, to avoid accidental shoveling", confirmAction = () => ToggleBool(ref Config.current.RequireShovelConfirmation), valueGrabber = () => GetBoolOptionValue(Config.current.RequireShovelConfirmation) });
            options.Add(new Option() { name = "Automatic sun collection", description = "Highly recommended until sun collection has been made accessible. Automatically clicks sun, coins, and end-level rewards.", confirmAction = () => ToggleBool(ref Config.current.AutoCollectItems), valueGrabber = () => GetBoolOptionValue(Config.current.AutoCollectItems) });
            options.Add(new Option() { name = "Say board position", description = "Say the current board position when the cursor is moved", confirmAction = () => ToggleBool(ref Config.current.SayTilePosOnMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayTilePosOnMove) });
            options.Add(new Option() { name = "Gameplay Tutorial", description = "Highly recommended on first playthroughs. Provides helpful gameplay advice at specific points in adventure mode.", confirmAction = () => ToggleBool(ref Config.current.GameplayTutorial), valueGrabber = () => GetBoolOptionValue(Config.current.GameplayTutorial) });
            options.Add(new Option() { name = "Say Object on move", description = "Say which plant, gravestone, crater or vase is at the current position, whenever the cursor is moved.", confirmAction = () => ToggleBool(ref Config.current.SayPlantOnTileMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayPlantOnTileMove) });
            options.Add(new Option() { name = "Beep when object is on tile", description = "Plays a beep tone if a plant, gravestone, crater or vase is on the current tile.", confirmAction = () => ToggleBool(ref Config.current.BeepWhenPlantFound), valueGrabber = () => GetBoolOptionValue(Config.current.BeepWhenPlantFound) });
            options.Add(new Option() { name = "Beep when zombie is on tile", description = "Plays a beep tone if the cursor moves to a tile with a zombie on it.", confirmAction = () => ToggleBool(ref Config.current.BeepWhenZombieFound), valueGrabber = () => GetBoolOptionValue(Config.current.BeepWhenZombieFound) });
            options.Add(new Option() { name = "Say zombies on moved tile", description = "Says which zombies are on the current tile, when the cursor moves.", confirmAction = () => ToggleBool(ref Config.current.SayZombieOnTileMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayZombieOnTileMove) });
            options.Add(new Option() { name = "Say lawnmower type", description = "When info4 is pressed, say what type of lawnmower is in the current lane, if any.", confirmAction = () => ToggleBool(ref Config.current.SayLawnmowerType), valueGrabber = () => GetBoolOptionValue(Config.current.SayLawnmowerType) });
            options.Add(new Option() { name = "Silent Fast zombie alert", description = "Plays an audio cue when a pole-vaulting or football zombie enters the board.", confirmAction = () => ToggleBool(ref Config.current.FastZombieAlert), valueGrabber = () => GetBoolOptionValue(Config.current.FastZombieAlert) });
            options.Add(new Option() { name = "Beep when current plant is ready", description = "Plays a cue when the current plant has refreshed, and you have enough sun to place it.", confirmAction = () => ToggleBool(ref Config.current.BeepOnPacketReady), valueGrabber = () => GetBoolOptionValue(Config.current.BeepOnPacketReady) });
            options.Add(new Option() { name = "Zombie Sonar on row change", description = "Which zombie sonar mode to use when changing rows.", leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieSonarOnRowChange,4), confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieSonarOnRowChange, 4),  valueGrabber = () => GetZombieSonarValue(Config.current.ZombieSonarOnRowChange) });
            options.Add(new Option() { name = "Zombie entry alert", description = "Plays an audio cue when a zombie enters from any point on the board.", confirmAction = () => ToggleBool(ref Config.current.ZombieEnterAlert), valueGrabber = () => GetBoolOptionValue(Config.current.ZombieEnterAlert) });
            options.Add(new Option() { name = "Be-ghouled match assistance", description = "Match assistance mode for the be-ghouled minigame.", confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.BeghouledMatchAssist, 3), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.BeghouledMatchAssist, 3),  valueGrabber = () => GetBeghouledMode(Config.current.BeghouledMatchAssist) });
            options.Add(new Option() { name = "Zombie sonar sweep", description = "How frequently to perform whole-board zombie sonar sweeps.", confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieSonarInterval, 5), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieSonarInterval, 5),  valueGrabber = () => GetZombieSweepInterval(Config.current.ZombieSonarInterval) });
            options.Add(new Option() { name = "Zombie death indicator", description = "Plays a sound cue when a zombie is dead.", confirmAction = () => ToggleBool(ref Config.current.ZombieKilledIndicator), valueGrabber = () => GetBoolOptionValue(Config.current.ZombieKilledIndicator) });
            options.Add(new Option() { name = "Zombie cycle mode", description = "Which mode to use when cycling through zombies on the board.", confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieCycleMode, 2), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieCycleMode, 2), valueGrabber = () => GetZombieCycleMode(Config.current.ZombieCycleMode) });
            options.Add(new Option() { name = "Move when cycling zombies.", description = "Move your cursor to the zombie's position when cycling zombies", confirmAction = () => ToggleBool(ref Config.current.MoveOnZombieCycle), valueGrabber = () => GetBoolOptionValue(Config.current.MoveOnZombieCycle) });

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

            if(Config.current.SayAvailableInputs)
            {
                optionText += "\r\nInputs: Confirm to toggle/apply value, Deny to close, Info1 for more information, Left and Right to change value, Up and Down to scroll list.";
            }

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
