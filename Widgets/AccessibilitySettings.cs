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
        uint selectedLanguageID = Config.current.LanguageID;

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
                    return Text.accessibility.value.Sonar1;
                case 2:
                    return Text.accessibility.value.Sonar2;
                case 3:
                    return Text.accessibility.value.Sonar3;
                default:
                    return Text.accessibility.value.Off;
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
                    return Text.accessibility.value.Off;
            }
        }

        string GetBoolOptionValue(bool value)
        {
            return value ? Text.accessibility.value.On : Text.accessibility.value.Off;
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
                return Text.accessibility.value.Beghouled1;
            if (value == 2)
                return Text.accessibility.value.Beghouled2;
            return Text.accessibility.value.Beghouled0;
        }

        string GetZombieSweepInterval(int value)
        {
            switch(value)
            {
                case 1:
                    return "0.5" + Text.accessibility.value.Seconds;
                case 2:
                    return "1" + Text.accessibility.value.Seconds;
                case 3:
                    return "2" + Text.accessibility.value.Seconds;
                case 4:
                    return "3" + Text.accessibility.value.Seconds;
                default:
                    return Text.accessibility.value.Off;
            }
        }

        string GetZombieCycleMode(int value)
        {
            if (value == 0)
                return Text.accessibility.value.ZombieCycle1;
            return Text.accessibility.value.ZombieCycle2;
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

        void ScrollLanguages(InputIntent intent)
        {
            string? selectedLanguage = GetCurrentLanguageSelection();
            if (selectedLanguage == null)
                return;
            string[]? languages = GetAvailiableLanguages();

            if (languages == null || languages.Length < 1)
                return;

            Array.Sort(languages);

            int langIndex = 0;
            for(int i =0; i < languages.Length; i++)
            {
                if(selectedLanguage == languages[i])
                {
                    langIndex = i;
                    break;
                }
            }


            if (intent is InputIntent.Left)
                langIndex--;
            if (intent is InputIntent.Right)
                langIndex++;

            if (Config.current.WrapCursorInMenus)
            {
                langIndex = langIndex < 0 ? languages.Length - 1 : langIndex;
                langIndex = langIndex >= languages.Length ? 0 : langIndex;
            }
            else
            {
                langIndex = langIndex < 0 ? 0 : langIndex;
                langIndex = langIndex >= languages.Length ? languages.Length - 1 : langIndex;
            }

            selectedLanguageID = K4os.Hash.xxHash.XXH32.DigestOf(Encoding.Unicode.GetBytes(languages[langIndex]));
        }

        void ConfirmScreenReader()
        {
            if (Config.current.ScreenReader is not null)
                Config.current.ScreenReader.StopSpeaking();
            if (selectedScreenreader == availableScreenreaders.Count - 1)
            {
                Console.WriteLine(Text.accessibility.value.ScreenreaderWarning);
                Program.Say(Text.accessibility.value.ScreenreaderWarning, true);

                needsScreenReaderConfirmation = true;
            }
            else
            {
                Config.current.ScreenReader = availableScreenreaders[selectedScreenreader].output;
                Config.SaveConfig(availableScreenreaders[selectedScreenreader].selection);
            }
        }

        void ConfirmLanguage()
        {
            Config.current.LanguageID = selectedLanguageID;
            Text.FindLanguages();
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

        string[]? GetAvailiableLanguages()
        {
            bool langExists = Directory.Exists(Text.langDir);

            if (!langExists)
            {
                string errorMsg = "Error! Language directory not found!";
                Console.WriteLine(errorMsg);
                Program.Say(errorMsg);
                return null;
            }

            var languageDirs = Directory.GetDirectories(Text.langDir);

            if (languageDirs == null || languageDirs.Length == 0)
            {
                string errorMsg = "Error! No language files found!";
                Console.WriteLine(errorMsg);
                Program.Say(errorMsg);
                return null;
            }

            for (int i = 0; i < languageDirs.Length; i++)
                languageDirs[i] = languageDirs[i].Substring(Text.langDir.Length + 1);

            return languageDirs;
        }

        string? GetCurrentLanguageSelection()
        {
            string[]? languageNames = GetAvailiableLanguages();
            if (languageNames == null || languageNames.Length < 0)
                return null;

            string? currentName = null;
            foreach(var name in languageNames)
            {
                if(K4os.Hash.xxHash.XXH32.DigestOf(Encoding.Unicode.GetBytes(name)) == selectedLanguageID)
                {
                    currentName = name;
                    break;
                }
            }

            return currentName;
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

            availableScreenreaders.Add(new ScreenReader() { name = Text.accessibility.value.AutomaticScreenReader, output = auto, selection = Config.ScreenReaderSelection.Auto });
            if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Auto)
                selectedScreenreader = 0;

            if (jaws is not null && jaws.IsAvailable())
                availableScreenreaders.Add(new ScreenReader() { name = "JAWS.", output = jaws, selection = Config.ScreenReaderSelection.Jaws });
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

            availableScreenreaders.Add(new ScreenReader() { name = Text.accessibility.value.Off, output = null, selection = Config.ScreenReaderSelection.Disabled });
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
            return delayValue.ToString("0.0") + Text.accessibility.value.Seconds;
        }

        void MainAccessibilityMenu()
        {
            options.Clear();
            optionIndex = 0;
            hasReadContent = false;
            
            //TODO: This could probably be done a lot nicer
            //Input
            options.Add(new Option() { name = Text.accessibility.name.WrapCursorInMenus, description = Text.accessibility.description.WrapCursorInMenus, confirmAction = () => ToggleBool(ref Config.current.WrapCursorInMenus), valueGrabber = () => GetBoolOptionValue(Config.current.WrapCursorInMenus), category = OptionCategory.Input });
            options.Add(new Option() { name = Text.accessibility.name.WrapCursorOnGrids, description = Text.accessibility.description.WrapCursorOnGrids, confirmAction = () => ToggleBool(ref Config.current.WrapCursorOnGrids), valueGrabber = () => GetBoolOptionValue(Config.current.WrapCursorOnGrids), category = OptionCategory.Input });
            options.Add(new Option() { name = Text.accessibility.name.WrapPlantSelection, description = Text.accessibility.description.WrapPlantSelection, confirmAction = () => ToggleBool(ref Config.current.WrapPlantSelection), valueGrabber = () => GetBoolOptionValue(Config.current.WrapPlantSelection), category = OptionCategory.Input });
            options.Add(new Option() { name = Text.accessibility.name.KeyRepetition, description = Text.accessibility.description.KeyRepetition, confirmAction = () => ToggleBool(ref Config.current.KeyRepetition), valueGrabber = () => GetBoolOptionValue(Config.current.KeyRepetition), category = OptionCategory.Input });
            options.Add(new Option() { name = Text.accessibility.name.DoubleTapDelay, description = Text.accessibility.description.DoubleTapDelay, confirmAction = () => DummyLeftRightAction(InputIntent.None), leftRightAction = SetDoubletapDelay, valueGrabber = GetDoubleTapDelay, category = OptionCategory.Input });
            options.Add(new Option() { name = Text.accessibility.name.ControllerVibration, description = Text.accessibility.description.ControllerVibration, confirmAction = () => ToggleBool(ref Config.current.ControllerVibration), valueGrabber = () => GetBoolOptionValue(Config.current.ControllerVibration), category = OptionCategory.Input });
            options.Add(new Option() { name = Text.accessibility.name.RebindInputs, description = Text.accessibility.description.RebindInputs, confirmAction = InputRebindMenu, leftRightAction = DummyLeftRightAction });

            //Gameplay
            options.Add(new Option() { name = Text.accessibility.name.RequireShovelConfirmation, description = Text.accessibility.description.RequireShovelConfirmation, confirmAction = () => ToggleBool(ref Config.current.RequireShovelConfirmation), valueGrabber = () => GetBoolOptionValue(Config.current.RequireShovelConfirmation), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.AutoCollectItems, description = Text.accessibility.description.AutoCollectItems, confirmAction = () => ToggleBool(ref Config.current.AutoCollectItems), valueGrabber = () => GetBoolOptionValue(Config.current.AutoCollectItems), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.GameplayTutorial, description = Text.accessibility.description.GameplayTutorial, confirmAction = () => ToggleBool(ref Config.current.GameplayTutorial), valueGrabber = () => GetBoolOptionValue(Config.current.GameplayTutorial), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.ZombieSonarInterval, description = Text.accessibility.description.ZombieSonarInterval, confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieSonarInterval, 5), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieSonarInterval, 5), valueGrabber = () => GetZombieSweepInterval(Config.current.ZombieSonarInterval), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.ZombieSonarOnRowChange, description = Text.accessibility.description.ZombieSonarOnRowChange, leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieSonarOnRowChange, 4), confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieSonarOnRowChange, 4), valueGrabber = () => GetZombieSonarValue(Config.current.ZombieSonarOnRowChange), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.ZombieTripwireRow, description = Text.accessibility.description.ZombieTripwireRow, leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieTripwireRow, 10), confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieTripwireRow, 10), valueGrabber = () => GetZombieTripwireValue(Config.current.ZombieTripwireRow), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.ZombieCycleMode, description = Text.accessibility.description.ZombieCycleMode, confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.ZombieCycleMode, 2), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.ZombieCycleMode, 2), valueGrabber = () => GetZombieCycleMode(Config.current.ZombieCycleMode), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.MoveOnZombieCycle, description = Text.accessibility.description.MoveOnZombieCycle, confirmAction = () => ToggleBool(ref Config.current.MoveOnZombieCycle), valueGrabber = () => GetBoolOptionValue(Config.current.MoveOnZombieCycle), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.BeghouledMatchAssist, description = Text.accessibility.description.BeghouledMatchAssist, confirmAction = () => SetIntValue(InputIntent.Right, ref Config.current.BeghouledMatchAssist, 3), leftRightAction = (intent) => SetIntValue(intent, ref Config.current.BeghouledMatchAssist, 3), valueGrabber = () => GetBeghouledMode(Config.current.BeghouledMatchAssist), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.Beghouled2MatchAssist, description = Text.accessibility.description.Beghouled2MatchAssist, confirmAction = () => ToggleBool(ref Config.current.Beghouled2MatchAssist), valueGrabber = () => GetBoolOptionValue(Config.current.Beghouled2MatchAssist), category = OptionCategory.Gameplay });
            options.Add(new Option() { name = Text.accessibility.name.AutoWakeStinky, description = Text.accessibility.description.AutoWakeStinky, confirmAction = () => ToggleBool(ref Config.current.AutoWakeStinky), valueGrabber = () => GetBoolOptionValue(Config.current.AutoWakeStinky), category = OptionCategory.Gameplay });    //hehe
            options.Add(new Option() { name = Text.accessibility.name.SamplePlantOnSwitch, description = Text.accessibility.description.SamplePlantOnSwitch, confirmAction = () => ToggleBool(ref Config.current.SamplePlantOnSwitch), valueGrabber = () => GetBoolOptionValue(Config.current.SamplePlantOnSwitch), category = OptionCategory.Gameplay });    

            //Narration
            options.Add(new Option() { name = Text.accessibility.name.SayTilePosOnMove, description = Text.accessibility.description.SayTilePosOnMove, confirmAction = () => ToggleBool(ref Config.current.SayTilePosOnMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayTilePosOnMove), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.SayPlantOnTileMove, description = Text.accessibility.description.SayPlantOnTileMove, confirmAction = () => ToggleBool(ref Config.current.SayPlantOnTileMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayPlantOnTileMove), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.SayZombieOnTileMove, description = Text.accessibility.description.SayZombieOnTileMove, confirmAction = () => ToggleBool(ref Config.current.SayZombieOnTileMove), valueGrabber = () => GetBoolOptionValue(Config.current.SayZombieOnTileMove), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.SayWhenTripwireCrossed, description = Text.accessibility.description.SayWhenTripwireCrossed, confirmAction = () => ToggleBool(ref Config.current.SayWhenTripwireCrossed), valueGrabber = () => GetBoolOptionValue(Config.current.SayWhenTripwireCrossed), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.SaySunCountOnCollect, description = Text.accessibility.description.SaySunCountOnCollect, confirmAction = () => ToggleBool(ref Config.current.SaySunCountOnCollect), valueGrabber = () => GetBoolOptionValue(Config.current.SaySunCountOnCollect), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.SayCoinValueOnCollect, description = Text.accessibility.description.SayCoinValueOnCollect, confirmAction = () => ToggleBool(ref Config.current.SayCoinValueOnCollect), valueGrabber = () => GetBoolOptionValue(Config.current.SayCoinValueOnCollect), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.SayAvailableInputs, description = Text.accessibility.description.SayAvailableInputs, confirmAction = () => ToggleBool(ref Config.current.SayAvailableInputs), valueGrabber = () => GetBoolOptionValue(Config.current.SayAvailableInputs), category = OptionCategory.Narration });
            options.Add(new Option() { name = Text.accessibility.name.ScreenReaderEngine, description = Text.accessibility.description.ScreenReaderEngine, leftRightAction = ScrollScreenReaders, valueGrabber = GetCurrentScreenreaderSelection, confirmAction = ConfirmScreenReader, category = OptionCategory.Narration });

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


            List<ToneProperties> backgroundPlantReady =
            [
                new ToneProperties() { leftVolume = 0.6f, rightVolume = 0.4f, startFrequency = 400.0f, endFrequency = 400.0f, duration = 190, signalType = SignalGeneratorType.Sin, startDelay = 40 },
                new ToneProperties() { leftVolume = 0.6f, rightVolume = 0.4f, startFrequency = 500.0f, endFrequency = 500.0f, duration = 170, signalType = SignalGeneratorType.Sin, startDelay = 20 },
                new ToneProperties() { leftVolume = 0.6f, rightVolume = 0.4f, startFrequency = 600.0f, endFrequency = 600.0f, duration = 150, signalType = SignalGeneratorType.Sin, startDelay = 0 },
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
            options.Add(new Option() { name = Text.accessibility.name.MenuPositionCueVolume, description = Text.accessibility.description.MenuPositionCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.MenuPositionCueVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.MenuPositionCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.GridPositionCueVolume, description = Text.accessibility.description.GridPositionCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.GridPositionCueVolume, gridPos), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.GridPositionCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.HitBoundaryVolume, description = Text.accessibility.description.HitBoundaryVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.HitBoundaryVolume, boundaryTone), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.HitBoundaryVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.PlantSlotChangeVolume, description = Text.accessibility.description.PlantSlotChangeVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.PlantSlotChangeVolume, slotTone), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.PlantSlotChangeVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.AutomaticZombieSonarVolume, description = Text.accessibility.description.AutomaticZombieSonarVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.AutomaticZombieSonarVolume, autoSonar), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.AutomaticZombieSonarVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.ManualZombieSonarVolume, description = Text.accessibility.description.ManualZombieSonarVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.ManualZombieSonarVolume, manualSonar), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ManualZombieSonarVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.PlantReadyCueVolume, description = Text.accessibility.description.PlantReadyCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.PlantReadyCueVolume, plantReady), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.PlantReadyCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.BackgroundPlantReadyCueVolume, description = Text.accessibility.description.BackgroundPlantReadyCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.BackgroundPlantReadyCueVolume, backgroundPlantReady), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.BackgroundPlantReadyCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.FoundObjectCueVolume, description = Text.accessibility.description.FoundObjectCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.FoundObjectCueVolume, plantFinder), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.FoundObjectCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.FastZombieCueVolume, description = Text.accessibility.description.FastZombieCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.FastZombieCueVolume, fastAlert), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.FastZombieCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.DeadZombieCueVolume, description = Text.accessibility.description.DeadZombieCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.DeadZombieCueVolume, zombieDead), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.DeadZombieCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.ZombieOnTileVolume, description = Text.accessibility.description.ZombieOnTileVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.ZombieOnTileVolume, zombieOnTile), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ZombieOnTileVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.ZombieEntryVolume, description = Text.accessibility.description.ZombieEntryVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.ZombieEntryVolume, zombieTones), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ZombieEntryVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.ZombieTripwireVolume, description = Text.accessibility.description.ZombieTripwireVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.ZombieTripwireVolume, tripwire), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.ZombieTripwireVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.BeghouledAssistVolume, description = Text.accessibility.description.BeghouledAssistVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.BeghouledAssistVolume, beghouled), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.BeghouledAssistVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.MiscAlertCueVolume, description = Text.accessibility.description.MiscAlertCueVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.MiscAlertCueVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.MiscAlertCueVolume), category = OptionCategory.Volume });
            options.Add(new Option() { name = Text.accessibility.name.AudioCueMasterVolume, description = Text.accessibility.description.AudioCueMasterVolume, leftRightAction = (intent) => SetFloat(intent, ref Config.current.AudioCueMasterVolume), valueGrabber = () => GetFloatOptionAsPercentage(Config.current.AudioCueMasterVolume), category = OptionCategory.Volume });

            //Other
            options.Add(new Option() { name = Text.accessibility.name.RestartOnCrash, description = Text.accessibility.description.RestartOnCrash, confirmAction = () => ToggleBool(ref Config.current.RestartOnCrash), valueGrabber = () => GetBoolOptionValue(Config.current.RestartOnCrash), category = OptionCategory.Other });
            options.Add(new Option() { name = Text.accessibility.name.MoveMouseCursor, description = Text.accessibility.description.MoveMouseCursor, confirmAction = () => ToggleBool(ref Config.current.MoveMouseCursor), valueGrabber = () => GetBoolOptionValue(Config.current.MoveMouseCursor), category = OptionCategory.Other });
            options.Add(new Option() { name = Text.accessibility.name.AutoLaunchGame, description = Text.accessibility.description.AutoLaunchGame, confirmAction = () => ToggleBool(ref Config.current.AutoLaunchGame), valueGrabber = () => GetBoolOptionValue(Config.current.AutoLaunchGame), category = OptionCategory.Other });
            options.Add(new Option() { name = Text.accessibility.name.Language, description = Text.accessibility.description.Language, leftRightAction = ScrollLanguages, valueGrabber = GetCurrentLanguageSelection, confirmAction = ConfirmLanguage, category = OptionCategory.Other });
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
                string categoryName = "";
                switch(options[optionIndex].category)
                {
                    case OptionCategory.Input:
                        categoryName = Text.accessibility.category.input;
                        break;
                    case OptionCategory.Gameplay:
                        categoryName = Text.accessibility.category.gameplay;
                        break;
                    case OptionCategory.Narration:
                        categoryName = Text.accessibility.category.narration;
                        break;
                    case OptionCategory.Volume:
                        categoryName = Text.accessibility.category.volume;
                        break;
                    case OptionCategory.Other:
                        categoryName = Text.accessibility.category.other;
                        break;
                }
                optionText = categoryName + ".\r\n" + optionText;
            }

            optionText = prepend + optionText;


            Console.WriteLine(optionText);
            Program.Say(optionText, true);
        }

        protected override string? GetContent()
        {
            string inputStr = Text.inputs.accessibility + "\r\n";
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
