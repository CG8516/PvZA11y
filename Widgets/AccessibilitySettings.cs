using AccessibleOutput;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.XInput;
using static PvZA11y.Program;

//TODO: Controller remapping is pretty ugly right now

namespace PvZA11y.Widgets
{
    class AccessibilitySettings : Widget
    {
        public bool menuClosed = false; //Hacky solution to close accessibility menu

        List<Option> options = new List<Option>();
        bool needsScreenReaderConfirmation = false;
        OptionCategory currentCategory = OptionCategory.Other;

        struct ScreenReader
        {
            public string name;
            public AccessibleOutput.IAccessibleOutput? output;
            public Config.ScreenReaderSelection selection;
        }

        List<ScreenReader> availableScreenreaders;
        int selectedScreenreader;

        enum OptionCategory
        {
            Input,
            Gameplay,
            Narration,
            Volume,
            Other,
        }

        struct Option
        {
            public string name;
            public string description;
            public Action confirmAction;
            public Action<InputIntent> leftRightAction;
            public Func<string>? valueGrabber;
            public OptionCategory category;
        }

        int optionIndex;

        string GetZombieSonarValue(int value)
        {
            switch(value)
            {
                case 1:
                    return "Full Sonar.";
                case 2:
                    return "Beeps only.";
                case 3:
                    return "Count only.";
                default:
                    return "Off.";
            }
        }

        string GetZombieTripwireValue(int value)
        {
            switch (value)
            {
                case 1:
                    return "A.";
                case 2:
                    return "B.";
                case 3:
                    return "C.";
                case 4:
                    return "D.";
                case 5:
                    return "E.";
                case 6:
                    return "F.";
                case 7:
                    return "G.";
                case 8:
                    return "H.";
                case 9:
                    return "I.";
                default:
                    return "Off.";
            }
        }

        string GetBoolOptionValue(bool value)
        {
            return value ? "On." : "Off.";
        }

        void ToggleBool(ref bool value)
        {
            value = !value;
            Config.SaveConfig();
        }

        void SetFloat(InputIntent intent, ref float value, List<Program.ToneProperties>? tones = null)
        {
            if (intent is InputIntent.Left)
                value -= 0.1f;
            if (intent is InputIntent.Right)
                value += 0.1f;

            value = MathF.Min(1.0f, value);
            value = MathF.Max(0.0f, value);

            if (tones == null || tones.Count == 0)
                tones = new List<Program.ToneProperties>() { new Program.ToneProperties() { leftVolume = value, rightVolume = value, duration = 50, startFrequency = 400, endFrequency = 400, signalType = SignalGeneratorType.Sin, startDelay = 0 } };
            var newTones = tones.ToList();  //Make a copy
            for (int i = 0; i < tones.Count; i++)
                newTones[i] = tones[i] with {leftVolume = tones[i].leftVolume * value, rightVolume = tones[i].rightVolume * value};

            Program.PlayTones(newTones);

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
                    return "0.5 seconds.";
                case 2:
                    return "1 second.";
                case 3:
                    return "2 seconds.";
                case 4:
                    return "3 seconds.";
                default:
                    return "Off.";
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
                bigWarning += "\r\nTo Re-enable the screenreader, press the deny button five times while at the main menu.";
                bigWarning += "\r\nIf you want to disable the screenreader, press the ok button again. Otherwise, press the deny button now.";
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
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, 400, 400, 100, SignalGeneratorType.Sin, 0);
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
            optionText += ". " + options[optionIndex].description;

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

            availableScreenreaders.Add(new ScreenReader() { name = "Automatic.", output = auto, selection = Config.ScreenReaderSelection.Auto });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Auto)
                selectedScreenreader = 0;

            if (jaws is not null && jaws.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "JAWS (NOT RECOMMENDED).", output = jaws, selection = Config.ScreenReaderSelection.Jaws });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Jaws)
                selectedScreenreader = availableScreenreaders.Count - 1;

            if (nvda.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "NVDA.", output = nvda, selection = Config.ScreenReaderSelection.Nvda });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Nvda)
                selectedScreenreader = availableScreenreaders.Count - 1;

            if (sapi.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "SAPI.", output = sapi, selection = Config.ScreenReaderSelection.Sapi });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Sapi)
                selectedScreenreader = availableScreenreaders.Count - 1;

            availableScreenreaders.Add(new ScreenReader() { name = "Deactivate.", output = null, selection = Config.ScreenReaderSelection.Disabled });
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

        void SetDoubletapDelay(InputIntent intent)
        {
            if (intent is InputIntent.Left)
                Config.current.DoubleTapDelay -= 100;
            else if (intent is InputIntent.Right)
                Config.current.DoubleTapDelay += 100;

            if (Config.current.DoubleTapDelay > 4000)
                Config.current.DoubleTapDelay = 4000;

            if (Config.current.DoubleTapDelay < 100)
                Config.current.DoubleTapDelay = 100;

            Config.SaveConfig();
        }

        string GetDoubleTapDelay()
        {
            float delayValue = (float)Config.current.DoubleTapDelay / 1000.0f;
            return delayValue.ToString("0.0") + " seconds.";
        }

        void MainAccessibilityMenu()
        {
            options.Clear();
            optionIndex = 0;
            hasReadContent = false;
            
            //TODO: This could probably be done a lot nicer
            //Input
            options.Add(new Option() { name = "Menu cursor wrapping", description = "Jump to the opposite end of menus when passing the first or last item", confirmAction = () => ToggleBool(ref Config.current.WrapCursorInMenus), valueGrabber = () => GetBoolOptionValue(Config.current.WrapCursorInMenus), category = OptionCategory.Input });
            options.Add(new Option() { name = "Grid cursor wrapping", description = "Jump to the opposite side of grids when passing the bounds", confirmAction = () => ToggleBool(ref Config.current.WrapCursorOnGrids), valueGrabber = () => GetBoolOptionValue(Config.current.WrapCursorOnGrids), category = OptionCategory.Input });
            options.Add(new Option() { name = "Plant selection wrapping", description = "Loop selection when cycling plants and Zen Garden tools", confirmAction = () => ToggleBool(ref Config.current.WrapPlantSelection), valueGrabber = () => GetBoolOptionValue(Config.current.WrapPlantSelection), category = OptionCategory.Input });
            options.Add(new Option() { name = "Key repetition", description = "Repeat directional inputs when held", confirmAction = () => ToggleBool(ref Config.current.KeyRepetition), valueGrabber = () => GetBoolOptionValue(Config.current.KeyRepetition), category = OptionCategory.Input });
            options.Add(new Option() { name = "Double tap delay", description = "Higher values will allow more time to perform a double-tap. Lower values will require faster tapping, but may offer a more responsive experience", confirmAction = () => DummyLeftRightAction(InputIntent.None), leftRightAction = SetDoubletapDelay, valueGrabber = GetDoubleTapDelay, category = OptionCategory.Input });
            options.Add(new Option() { name = "Controller vibration", description = "Whether controller vibration will be used or not", confirmAction = () => ToggleBool(ref Config.current.ControllerVibration), valueGrabber = () => GetBoolOptionValue(Config.current.ControllerVibration), category = OptionCategory.Input });
            options.Add(new Option() { name = "Rebind inputs", description = "Allows you to rebind all controls.", confirmAction = InputRebindMenu, leftRightAction = DummyLeftRightAction });

            //Gameplay
            options.Add(new Option() { name = "Shovel Confirmation", description = "Require the shovel button to be pressed twice, to avoid accidental shoveling", confirmAction = () => ToggleBool(ref Config.current.RequireShovelConfirmation), valueGrabber = () => GetBoolOptionValue(Config.current.RequireShovelConfirmation), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Automatic sun collection", description = "Highly recommended until sun collection has been made accessible. Automatically clicks sun, coins, and end-level rewards.", confirmAction = () => ToggleBool(ref Config.current.AutoCollectItems), valueGrabber = () => GetBoolOptionValue(Config.current.AutoCollectItems), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Gameplay Tutorials", description = "Highly recommended on first playthroughs. Provides helpful gameplay advice.", confirmAction = () => ToggleBool(ref Config.current.GameplayTutorial), valueGrabber = () => GetBoolOptionValue(Config.current.GameplayTutorial), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Automatic sonar interval", description = "How frequently to perform whole-board zombie sonar sweeps.", confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieSonarInterval, 5), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieSonarInterval, 5), valueGrabber = () => GetZombieSweepInterval(Config.current.ZombieSonarInterval), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Zombie Sonar on row change", description = "Which zombie sonar mode to automatically use when changing rows.", leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieSonarOnRowChange, 4), confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieSonarOnRowChange, 4), valueGrabber = () => GetZombieSonarValue(Config.current.ZombieSonarOnRowChange), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Zombie tripwire row", description = "When any zombie is on, or to the left of this row, an alarm will play.", leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieTripwireRow, 10), confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieTripwireRow, 10), valueGrabber = () => GetZombieTripwireValue(Config.current.ZombieTripwireRow), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Zombie cycle mode", description = "Which mode to use when cycling through zombies on the board.", confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieCycleMode, 2), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieCycleMode, 2), valueGrabber = () => GetZombieCycleMode(Config.current.ZombieCycleMode), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Move when cycling zombies.", description = "Move your cursor to the zombie's position when cycling zombies", confirmAction = () => ToggleBool(ref Config.current.MoveOnZombieCycle), valueGrabber = () => GetBoolOptionValue(Config.current.MoveOnZombieCycle), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Be-ghouled match assistance", description = "Match assistance mode for the be-ghouled minigame.", confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.BeghouledMatchAssist, 3), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.BeghouledMatchAssist, 3), valueGrabber = () => GetBeghouledMode(Config.current.BeghouledMatchAssist), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Be-ghouled Twist match assistance", description = "Match assistance for the be-ghouled Twist minigame", confirmAction = () => ToggleBool(ref Config.current.Beghouled2MatchAssist), valueGrabber = () => GetBoolOptionValue(Config.current.Beghouled2MatchAssist), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = "Automatic Snail Bullying", description = "Automatically wake Stinky the Snail, whenever he falls asleep in zen garden mode.", confirmAction = () => ToggleBool(ref Config.current.AutoWakeStinky), valueGrabber = () => GetBoolOptionValue(Config.current.AutoWakeStinky), category = OptionCategory.Gameplay });    //hehe

            //Narration
            options.Add(new Option() { name = "Say board position", description = "Say the current board position when the cursor is moved", confirmAction = () => ToggleBool(ref Config.current.SayTilePosOnMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayTilePosOnMove), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Say Plant/Object on cursor movement", description = "Say which plant, gravestone, crater or vase is at the current position, whenever you move around the board.", confirmAction = () => ToggleBool(ref Config.current.SayPlantOnTileMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayPlantOnTileMove), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Say zombies on cursor tile", description = "Says which zombies are on the current tile, when the cursor moves.", confirmAction = () => ToggleBool(ref Config.current.SayZombieOnTileMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayZombieOnTileMove), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Say when tripwire has been crossed", description = "If zombie tripwire is enabled, say when a zombie has crossed the tripwire.", confirmAction = () => ToggleBool(ref Config.current.SayWhenTripwireCrossed), valueGrabber = () => GetBoolOptionValue(Config.current.SayWhenTripwireCrossed), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Say sun count when collected", description = "Say your current sun amount when any sun is collected.", confirmAction = () => ToggleBool(ref Config.current.SaySunCountOnCollect), valueGrabber = () => GetBoolOptionValue(Config.current.SaySunCountOnCollect), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Say coin value when collected", description = "Say the value of each coin/diamond when it's collected.", confirmAction = () => ToggleBool(ref Config.current.SayCoinValueOnCollect), valueGrabber = () => GetBoolOptionValue(Config.current.SayCoinValueOnCollect), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Say available inputs", description = "Reads the available inputs when a new dialogue has opened.", confirmAction = () => ToggleBool(ref Config.current.SayAvailableInputs), valueGrabber = () => GetBoolOptionValue(Config.current.SayAvailableInputs), category = OptionCategory.Narration });
            options.Add(new Option() { name = "Screen Reader Engine.", description = "Which screen reader engine to use, Use left and right to select, and press confirm to apply", leftRightAction = ScrollScreenReaders, valueGrabber = GetCurrentScreenreaderSelection, confirmAction = ConfirmScreenReader, category = OptionCategory.Narration });

            List<ToneProperties> boundaryTone = new List<ToneProperties>() { new ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 70, endFrequency = 70, duration = 50, signalType = SignalGeneratorType.Square, startDelay = 0 } };
            List<ToneProperties> fastAlert =
            [
                new ToneProperties() { leftVolume = 0, rightVolume = Config.current.FastZombieCueVolume, startFrequency = 800, endFrequency = 800, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 0 },
                new ToneProperties() { leftVolume = 0, rightVolume = Config.current.FastZombieCueVolume, startFrequency = 800, endFrequency = 800, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 100 },
                new ToneProperties() { leftVolume = 0, rightVolume = Config.current.FastZombieCueVolume, startFrequency = 800, endFrequency = 800, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 200 }
            ];

            List<ToneProperties> slotTone = new List<ToneProperties>() { new ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 300, endFrequency = 300, duration = 50, signalType = SignalGeneratorType.Square, startDelay = 0 } };

            List<ToneProperties> autoSonar =
            [
                new ToneProperties() {leftVolume = 0.777f, rightVolume = 0.222f, startFrequency = 1000, endFrequency = 1000, duration = 100, signalType = SignalGeneratorType.Triangle, startDelay = 100 },
                new ToneProperties() {leftVolume = 0.222f, rightVolume = 0.777f, startFrequency = 700, endFrequency = 700, duration = 100, signalType = SignalGeneratorType.Triangle, startDelay = 350 },
            ];


            List<ToneProperties> manualSonar =
            [
                new ToneProperties() {leftVolume = 0.777f, rightVolume = 0.222f, startFrequency = 300, endFrequency = 300, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 100 },
                new ToneProperties() {leftVolume = 0.222f, rightVolume = 0.777f, startFrequency = 310, endFrequency = 310, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 350 },
            ];

            List<ToneProperties> plantReady =
            [
                new ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 698.46f, endFrequency = 698.46f, duration = 190, signalType = SignalGeneratorType.Sin, startDelay = 0 },
                new ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 880, endFrequency = 880, duration = 170, signalType = SignalGeneratorType.Sin, startDelay = 20 },
                new ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 1046.5f, endFrequency = 1046.5f, duration = 150, signalType = SignalGeneratorType.Sin, startDelay = 40 },
            ];


            List<ToneProperties> gridPos = new List<ToneProperties>() { new ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 800, endFrequency = 800, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 0 } };
            List<ToneProperties> plantFinder = new List<ToneProperties>() { new ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 800, endFrequency = 800, duration = 100, signalType = SignalGeneratorType.SawTooth, startDelay = 0 } };

            List<ToneProperties> zombieOnTile =
            [
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 750, endFrequency = 750, duration = 80, signalType = SignalGeneratorType.Square, startDelay = 100 },
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 700, endFrequency = 700, duration = 80, signalType = SignalGeneratorType.Square, startDelay = 200 },
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 650, endFrequency = 650, duration = 80, signalType = SignalGeneratorType.Square, startDelay = 300 },
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 600, endFrequency = 600, duration = 80, signalType = SignalGeneratorType.Square, startDelay = 400 },
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 550, endFrequency = 550, duration = 80, signalType = SignalGeneratorType.Square, startDelay = 500 },
            ];

            List<ToneProperties> zombieDead =
            [
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 1000, endFrequency = 1000, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 0 },
                new Program.ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, startFrequency = 1050, endFrequency = 1050, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 100 },
            ];

            List<ToneProperties> zombieTones =
            [
                new ToneProperties() { leftVolume = 0, rightVolume = 1, startFrequency = 1000, endFrequency = 1000, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 0 },
                new ToneProperties() { leftVolume = 0, rightVolume = 1, startFrequency = 700, endFrequency = 700, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 200 },
                new ToneProperties() { leftVolume = 0, rightVolume = 1, startFrequency = 400, endFrequency = 400, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 400 },
            ];

            List<Program.ToneProperties> beghouled =
            [
                new Program.ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 700, endFrequency = 700, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 0 },
                new Program.ToneProperties() { leftVolume = 1, rightVolume = 1, startFrequency = 800, endFrequency = 800, duration = 200, signalType = SignalGeneratorType.Sin, startDelay = 100 },
            ];

            List<ToneProperties> tripwire =
            [
                new ToneProperties() { leftVolume = 1.0f, rightVolume = 1.0f, duration = 500, startFrequency = 200, endFrequency = 200, signalType = SignalGeneratorType.Triangle, startDelay = 0 },
                new ToneProperties() { leftVolume = 1.0f, rightVolume = 1.0f, duration = 500, startFrequency = 275, endFrequency = 275, signalType = SignalGeneratorType.Triangle, startDelay = 20 },
                new ToneProperties() { leftVolume = 1.0f, rightVolume = 1.0f, duration = 500, startFrequency = 350, endFrequency = 350, signalType = SignalGeneratorType.Triangle, startDelay = 20 },
                new ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, duration = 100, startFrequency = 100, endFrequency = 100, signalType = SignalGeneratorType.Square, startDelay = 0 },
                new ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, duration = 100, startFrequency = 100, endFrequency = 100, signalType = SignalGeneratorType.Square, startDelay = 200 },
                new ToneProperties() { leftVolume = 0.5f, rightVolume = 0.5f, duration = 100, startFrequency = 100, endFrequency = 100, signalType = SignalGeneratorType.Square, startDelay = 400 },
            ];


            //Volume
            options.Add(new Option() { name = "Menu position volume", description = "Indicates where the cursor is located on a menu or list", leftRightAction = (intent) => SetFloat(intent, ref Config.current.MenuPositionCueVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.MenuPositionCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Grid position volume", description = "Indicates where the cursor is located on a grid", leftRightAction = (intent) => SetFloat(intent, ref Config.current.GridPositionCueVolume, gridPos), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.GridPositionCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Boundary hit volume", description = "Indicates when the cursor passes the bounds of an unwrapped grid or list", leftRightAction = (intent) => SetFloat(intent, ref Config.current.HitBoundaryVolume, boundaryTone), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.HitBoundaryVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Selected slot volume", description = "Indicates which slot is currently selected", leftRightAction = (intent) => SetFloat(intent, ref Config.current.PlantSlotChangeVolume, slotTone), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.PlantSlotChangeVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Automatic sonar volume", description = "Used for automatic zombie sonar", leftRightAction = (intent) => SetFloat(intent, ref Config.current.AutomaticZombieSonarVolume, autoSonar), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.AutomaticZombieSonarVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Manual sonar volume", description = "Used when zombie sonar for current row is pressed manually", leftRightAction = (intent) => SetFloat(intent, ref Config.current.ManualZombieSonarVolume, manualSonar), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ManualZombieSonarVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Plant ready volume", description = "Plays when the current plant is refreshed, and you have enough sun to place it.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.PlantReadyCueVolume, plantReady), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.PlantReadyCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Plant/Object finder volume", description = "When navigating the board, plays a different tone if a plant, gravestone, crater or vase is on the current tile.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.FoundObjectCueVolume, plantFinder), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.FoundObjectCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Fast zombie alert volume", description = "Plays when a pole-vaulting or football zombie enters the board.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.FastZombieCueVolume, fastAlert), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.FastZombieCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Zombie death indicator volume", description = "Plays when a zombie dies.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.DeadZombieCueVolume, zombieDead), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.DeadZombieCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Zombie on tile alert volume", description = "Plays descending tones to indicate the number of zombies on the current tile, when the cursor moves to it.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.ZombieOnTileVolume, zombieOnTile), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ZombieOnTileVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Zombie entry alert volume", description = "Plays pitched tones to indicate when and where any zombies have entered the lawn.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.ZombieEntryVolume, zombieTones), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ZombieEntryVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Zombie tripwire volume", description = "Background alarm that plays when any zombie is on the left side of the tripwire.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.ZombieTripwireVolume, tripwire), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ZombieTripwireVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Be-ghouled assistance volume", description = "Plays in Be-ghouled minigames, when a match can be found", leftRightAction = (intent) => SetFloat(intent, ref Config.current.BeghouledAssistVolume, beghouled), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.BeghouledAssistVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Miscellaneous alert volume", description = "Used for various alerts throughout the game.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.MiscAlertCueVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.MiscAlertCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = "Master Audio Cue Volume", description = "Adjusts the volume of all non-speech audio cues.", leftRightAction = (intent) => SetFloat(intent, ref Config.current.AudioCueMasterVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.AudioCueMasterVolume), category = OptionCategory.Volume });

            //Other
            options.Add(new Option() { name = "Restart on crash", description = "Automatically attempt to restart the mod if it crashes", confirmAction = () => ToggleBool(ref Config.current.RestartOnCrash), valueGrabber = () => GetBoolOptionValue(Config.current.RestartOnCrash), category = OptionCategory.Other });
            options.Add(new Option() { name = "Move mouse cursor", description = "Move the mouse cursor to visually indicate where clicks will be performed", confirmAction = () => ToggleBool(ref Config.current.MoveMouseCursor), valueGrabber = () => GetBoolOptionValue(Config.current.MoveMouseCursor), category = OptionCategory.Other });
        }

        public AccessibilitySettings(MemoryIO memIO) : base(memIO, "")
        {
            GetAvailableScreenreaders();
            MainAccessibilityMenu();
        }

        void ReadOptionText(bool readStateFirst = false, string prepend = "")
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

            optionText += "\r\n" + options[optionIndex].description;

            OptionCategory thisCategory = options[optionIndex].category;
            if (currentCategory != thisCategory)
            {
                currentCategory = thisCategory;
                optionText = options[optionIndex].category.ToString() + ".\r\n" + optionText;
            }

            optionText = prepend + optionText;


            Console.WriteLine(optionText);
            Program.Say(optionText, true);
        }

        protected override string? GetContent()
        {
            string inputStr = "Inputs: Up and Down to scroll list, CycleLeft and CycleRight to jump to categories, Confirm to toggle, Left and Right to toggle or change values, Deny to go back, Info1 to repeat description.\r\n";
            if (!Config.current.SayAvailableInputs)
                inputStr = "";
            ReadOptionText(false, inputStr);
            return null;
        }

        void JumpCategory(InputIntent intent)
        {
            int currentCategory = (int)options[optionIndex].category;
            int nextCategory = currentCategory + (intent is InputIntent.CycleLeft ? -1 : 1);
            int categoryCount = Enum.GetValues(typeof(OptionCategory)).Length;
            nextCategory += categoryCount;
            nextCategory %= categoryCount;

            int nextIndex = optionIndex;
            for(int i = 0; i < options.Count; i++)
            {
                if (options[i].category == (OptionCategory)nextCategory)
                {
                    nextIndex = i;
                    break;
                }
            }

            optionIndex = nextIndex;
            ReadOptionText(false);

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
                OptionCategory prevCategory = currentCategory;
                ReadOptionText();
                float frequency = 400 + (((options.Count - optionIndex) + 1) * 100);
                if (prevCategory != currentCategory)
                {
                    Program.PlayTone(Config.current.MiscAlertCueVolume, Config.current.MiscAlertCueVolume, 200, 200, 100, SignalGeneratorType.Triangle, 0);
                    Program.Vibrate(0.1f, 0.1f, 50);
                }
                else
                    Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, frequency, frequency, 100, SignalGeneratorType.Sin, 0);
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
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, 400, 400, 50, SignalGeneratorType.Sin, 0);
                Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, 450, 450, 50, SignalGeneratorType.Sin, 50);
            }

            if (intent is InputIntent.Left or InputIntent.Right)
            {
                if (options[optionIndex].leftRightAction != null)
                    options[optionIndex].leftRightAction(intent);
                else if (options[optionIndex].confirmAction != null)
                    options[optionIndex].confirmAction();
                else
                    Program.PlayTone(Config.current.MenuPositionCueVolume, Config.current.MenuPositionCueVolume, 400, 400, 50, SignalGeneratorType.Sin, 0);

                ReadOptionText(true);
            }

            if (intent is InputIntent.Deny)
                menuClosed = true;

            if (intent is InputIntent.CycleLeft or InputIntent.CycleRight)
                JumpCategory(intent);

        }
    }
}
