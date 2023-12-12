using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccessibleOutput;
using Vortice.XInput;

namespace PvZA11y
{
    //Current state of all options
    //TODO: Save/Load config

    public static class Config
    {
        public static ConfigOptions current = new ConfigOptions();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ScreenReaderSelection
        {
            Auto,
            Nvda,
            Jaws,
            Sapi,
            Disabled
        }

        public class ConfigOptions
        {
            //Input
            public bool WrapCursorInMenus = true;
            public bool WrapCursorOnGrids = false;
            public bool WrapPlantSelection = true;
            public bool KeyRepetition = true;
            public Dictionary<uint, InputIntent> keyBinds = new Dictionary<uint, InputIntent>();
            public Dictionary<GamepadButtons, InputIntent> controllerBinds = new Dictionary<GamepadButtons, InputIntent>();

            //Gameplay
            public bool RequireShovelConfirmation = false;
            public bool AutoCollectItems = true;
            public bool SayTilePosOnMove = false;
            public bool GameplayTutorial = true;
            public bool SayPlantOnTileMove = false;
            public bool BeepWhenPlantFound = true;
            public bool BeepWhenZombieFound = true;
            public bool SayZombieOnTileMove = false;
            

            //Core functionality
            public bool RestartOnCrash = true;
            public bool MoveMouseCursor = true;
            public bool FocusOnInteract = true;
            public float AudioCueVolume = 1.0f;
            public ScreenReaderSelection screenReaderSelection = ScreenReaderSelection.Auto;
            

            [JsonIgnore]
            public IAccessibleOutput? ScreenReader = null;  //TODO: This really shouldn't be here
        }

        

        static string configFilename = "PvzAccessibilityConfig.json";
        static JsonSerializerOptions options = new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true };

        public static IAccessibleOutput? AutoScreenReader()
        {
            AccessibleOutput.JawsOutput jaws = null;

            try
            {
                jaws = new JawsOutput();
            }
            catch
            {
                //Console.WriteLine("Fuck Freedom Scientific.");
            }
            
            AccessibleOutput.NvdaOutput nvda = new NvdaOutput();
            AccessibleOutput.SapiOutput sapi = new SapiOutput();

            if (jaws is not null && jaws.IsAvailable())
            {
                if (current.ScreenReader is AccessibleOutput.JawsOutput)
                    return current.ScreenReader;
                return jaws;
            }


            if (nvda.IsAvailable())
            {
                if (current.ScreenReader is AccessibleOutput.NvdaOutput)
                    return current.ScreenReader;
                return nvda;
            }

            if (sapi.IsAvailable())
            {
                if(current.ScreenReader is AccessibleOutput.SapiOutput)
                    return current.ScreenReader;
                return sapi;
            }

            return null;
        }

        public static void SaveConfig(ScreenReaderSelection? screenReader = null)
        {
            //Todo: Clean this up a bit (I feel like I can write this line almost anywhere in the project)
            //if (current.ScreenReader is NvdaOutput)
            //    current.screenReaderSelection = ScreenReaderSelection.Nvda;
            //else if (current.ScreenReader is JawsOutput)
            //    current.screenReaderSelection = ScreenReaderSelection.Jaws;
            //else if (current.ScreenReader is SapiOutput)
            //    current.screenReaderSelection = ScreenReaderSelection.Sapi;
            //else if (current.ScreenReader is AutoOutput)
            //    current.screenReaderSelection = ScreenReaderSelection.Auto;
            //else if (current.ScreenReader is null)
            //    current.screenReaderSelection = ScreenReaderSelection.Disabled;

            if(screenReader != null)
                current.screenReaderSelection = screenReader.Value;

            try
            {
                var serialized = JsonSerializer.Serialize<ConfigOptions>(current, options);
                File.WriteAllText(configFilename, serialized);
            }
            catch
            {
                Console.WriteLine("Failed to save config file!");
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(configFilename))
                {
                    string config = File.ReadAllText(configFilename);
                    var deserialized = JsonSerializer.Deserialize<ConfigOptions>(config, options);
                    if(deserialized != null)
                        current = deserialized;
                }
            }
            catch
            {
                Console.WriteLine("Failed to load config file!");
            }

            switch(current.screenReaderSelection)
            {
                case ScreenReaderSelection.Auto:
                    current.ScreenReader = new AutoOutput();
                    break;
                case ScreenReaderSelection.Nvda:
                    current.ScreenReader = new NvdaOutput();
                    break;
                case ScreenReaderSelection.Jaws:
                    current.ScreenReader = new JawsOutput();
                    break;
                case ScreenReaderSelection.Sapi:
                    current.ScreenReader = new SapiOutput();
                    break;
                case ScreenReaderSelection.Disabled:
                    current.ScreenReader = null;
                    break;
            }
        }
    }
}
