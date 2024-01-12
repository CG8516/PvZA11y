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
            public int DoubleTapDelay = 600;
            public Dictionary<uint, InputIntent> keyBinds = new Dictionary<uint, InputIntent>();
            public Dictionary<GamepadButtons, InputIntent> controllerBinds = new Dictionary<GamepadButtons, InputIntent>();
            public bool ControllerVibration = true;

            //Gameplay
            public bool RequireShovelConfirmation = false;
            public bool AutoCollectItems = true;
            public bool GameplayTutorial = true;
            public int ZombieSonarInterval = 3;
            public int ZombieSonarOnRowChange = 0;
            public int ZombieTripwireRow = 3;
            public int ZombieCycleMode = 1;
            public bool MoveOnZombieCycle = true;
            public int BeghouledMatchAssist = 1;
            public bool Beghouled2MatchAssist = true;
            public bool AutoWakeStinky = false;
            public bool SamplePlantOnSwitch = false;

            //Narration
            public bool SayTilePosOnMove = false;
            public bool SayPlantOnTileMove = true;
            public bool SayZombieOnTileMove = true;
            public bool SayWhenTripwireCrossed = true;
            public bool SayAvailableInputs = true;
            public bool SaySunCountOnCollect = false;
            public bool SayCoinValueOnCollect = false;
            public ScreenReaderSelection screenReaderSelection = ScreenReaderSelection.Auto;

            //Core functionality
            public bool RestartOnCrash = true;
            public bool MoveMouseCursor = true;
            public bool AutoLaunchGame = false;
            public uint LanguageID = K4os.Hash.xxHash.XXH32.DigestOf(Encoding.Unicode.GetBytes("English"));
            public string GameStartPath = "";

            //Volumes
            public float MenuPositionCueVolume = 0.3f;
            public float HitBoundaryVolume = 0.6f;
            public float GridPositionCueVolume = 0.6f;
            public float PlantSlotChangeVolume = 0.5f;
            public float AutomaticZombieSonarVolume = 0.3f;
            public float ManualZombieSonarVolume = 1.0f;
            public float PlantReadyCueVolume = 0.3f;
            public float BackgroundPlantReadyCueVolume = 0.0f;
            public float FoundObjectCueVolume = 0.6f;
            public float FastZombieCueVolume = 1.0f;
            public float DeadZombieCueVolume = 0.1f;
            public float ZombieOnTileVolume = 1.0f;
            public float ZombieEntryVolume = 1.0f;
            public float ZombieTripwireVolume = 0.2f;
            public float BeghouledAssistVolume = 1.0f;
            public float MiscAlertCueVolume = 1.0f; //Not enough sun, refreshing, press start to begin, lawnmowers, etc..
            public float AudioCueMasterVolume = 1.0f;


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
