using Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Numerics;
using AccessibleOutput;
using System.Text;
using Vortice.XInput;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows.Forms;
using System.Collections.Immutable;
using PvZA11y.Widgets;
using NAudio.Mixer;

/*
[PVZ-A11y Beta 1.17.2]

Blind and motor accessibility mod for Plants Vs Zombies.
Allows input with rebindable keys and controller buttons, rather than requiring a mouse for input.
Includes many features to make the game playable for blind players.

The core goal is to make the game fully accessible for blind gamers, without affecting the game's learning curve.
Being blind shouldn't make a game more challenging to play, it should just be an alternate playstyle.
We're not quite there yet. I often use the freeze functionality as a crutch, because it takes a while to convey the current board state.
To convey information quicker, we could have a unique sound for each type of plant or zombie, but that will bring a much steeper learning curve.

For now, I recommend using NVDA with rate-boosted speech.


Works by using pointerchains to find values in memory.
Sends mouse movement and click events to the game process, to simulate input.

Todo:
    Move all pointers/offsets/struct-sizes into pointers.cs
    Move all memory interaction operations into memoryIO.cs
    General code cleanup (so much dead code, things where they shouldn't be, etc)

Ideas for future updates (Not plans. Other stuff comes first)
    Audio descriptions
    Zombie sound effects for each almanac entry
    Hook the input functions, so we're able to directly control where the game thinks the mouse is, without sending messages to the window.
    Manual sun/coin/pickup collection, possible with a sonar-like system
    Narration history, so you can see any missed messages.

Discussions:
    How should we handle the in-game store? This implements a custom accessible version, but comes with the issue that purchases aren't saved until you enter a game.
        Is this enough of an issue to warrant scrapping the accessible version, and instead implement a direct translation of the original menu? (page-shifting delays included)
    How can we make it easier for a player to read the game state, without adding a huge learning curve?
        We could add a unique audio cue for each plant/zombie, but that will require memorizing 50+ audio cues.
        We could offer an option for information brevity. With short nicknames for each plant/zombie (Peashooter > pea, Magnet-Shroom > Mag, Screen Door > Door)
        Could disable the delay in the zombie sonar, and just use panning.
        Could implement pitch to indicate zombie threat (health/armor, speed, digger, pole-vaulter pre-jump, etc..)
        Could have a plant-column checker, to indicate how many empty/plantable tiles are in the current column (useful for detecting if a plant has been eaten, or if you missed a spot)
        Could add a screen reader cue when a plant is eaten (eg; "E-4 Peashooter Eaten").

The memory.dll library could use some enhancements, but it's workable for now.

*/

namespace PvZA11y
{
    internal class Program
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        
        /// <summary>
        /// Provides keyboard access.
        /// </summary>
        internal static class NativeKeyboard
        {
            /// <summary>
            /// A positional bit flag indicating the part of a key state denoting
            /// key pressed.
            /// </summary>
            private const int KeyPressed = 0x8000;

            /// <summary>
            /// Returns a value indicating if a given key is pressed.
            /// </summary>
            /// <param name="key">The key to check.</param>
            /// <returns>
            /// <c>true</c> if the key is pressed, otherwise <c>false</c>.
            /// </returns>
            public static bool IsKeyDown(uint key)
            {
                return (GetKeyState((int)key) & KeyPressed) != 0;
            }

            //Fuck C#
            public static bool IsKeyDown(int key)
            {
                return IsKeyDown((uint)key);
            }

            /// <summary>
            /// Gets the key state of a key.
            /// </summary>
            /// <param name="key">Virtuak-key code for key.</param>
            /// <returns>The state of the key.</returns>
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern short GetKeyState(int key);
        }



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static int MakeLParam(int x, int y) => (y << 16) | (x & 0xFFFF);

        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const uint WM_CHAR = 0x0102;
        const int VK_TAB = 0x09;
        const int VK_ENTER = 0x0D;
        const int VK_UP = 0x26;
        const int VK_DOWN = 0x28;
        const int VK_RIGHT = 0x27;

        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;

        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;

        static bool steamLaunchAttempted = false;
        static int GetParentProcessId(Process process)
        {
            try
            {
                using (var mo = new System.Management.ManagementObject(string.Format("win32_process.handle='{0}'", process.Id)))
                {
                    mo.Get();
                    return (int)(uint)mo["ParentProcessId"];

                }
            }
            catch
            {
                Console.WriteLine("Failed to find parent process");
                return -1;
            }
        }

        struct plantInPicker
        {
            public int posX;
            public int posY;
            //public int animStartFrame;
            //public int animEndFrame;
            //public int animStartX;
            //public int animStartY;
            //public int animEndX;
            //public int animEndY;
            public SeedType seedType;
            public ChosenSeedState seedState;
            public int indexInBank;
            public bool refreshing;
            public int refreshCounter;
            public SeedType imitaterType;
            public bool crazyDavePicked;
        }


        static MemoryIO memIO;// = new MemoryIO("PLACEHOLDER", 0, mem);

        public static long CurrentEpoch()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        struct plantInBoardBank
        {
            public int refreshCounter;
            public int refreshTime;
            public int index;   //Of what?
            public int offsetX;
            public int packetType;
            public int imitaterType;
            public bool isRefreshing;
            public float absX;
        }

        public static Encoding encoding = Encoding.UTF8;

        static plantInPicker[] plantPickerState = new plantInPicker[(int)SeedType.NUM_SEED_TYPES];
        static byte[] plantPickerBytes = new byte[3180];


        static void RefreshPlantPickerState()
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            plantPickerBytes = mem.ReadBytes(memIO.ptr.lawnAppPtr + ",874,bc", 3180);  //Can we do this without reallocating the byte array? Might have to fork memory.dll to allow it
            //sw.Stop();
            //Console.WriteLine("Got bytes in {0}ms", sw.ElapsedMilliseconds);

            //If not in plant picker?
            if (plantPickerBytes == null)
                return;

            int index = 0;
            for (int i =0; i < (int)SeedType.NUM_SEED_TYPES; i++)
            {
                plantPickerState[i].posX = BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].posY = BitConverter.ToInt32(plantPickerBytes, index);
                index += 28;    //Jump to plant id
                plantPickerState[i].seedType = (SeedType)BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].seedState = (ChosenSeedState)BitConverter.ToInt32(plantPickerBytes, index);
                index += 4;
                plantPickerState[i].indexInBank = BitConverter.ToInt32(plantPickerBytes, index);
                index += 16;
                plantPickerState[i].crazyDavePicked = BitConverter.ToInt32(plantPickerBytes, index) > 0;
                index += 4;
            }
        }


        static int CurrentTreeDialogue = -1;

        static Mem mem = new Mem();

        static nint gameWHnd;

        public static int windowWidth;
        public static int windowHeight;
        public static int drawWidth;
        public static int drawHeight;

        static int drawStartX;

        static long vibrationEnd;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        static float GetScalingFactor()
        {
            Screen[] screenList = Screen.AllScreens;
            for (int i = 0; i < screenList.Length; i++)
            {
                DEVMODE dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                EnumDisplaySettings(screenList[i].DeviceName, -1, ref dm);

                var scalingFactor = Math.Round(Decimal.Divide(dm.dmPelsWidth, screenList[i].Bounds.Width), 2);
                return (float)scalingFactor;
            }

            return 1;
        }

        public static void Click(float downX, float downY, float upX, float upY, bool async = true)
        {
            if (async)
                Task.Run(() => ClickTask(downX, downY, upX, upY));
            else
                ClickTask(downX, downY, upX, upY);
        }

        public static void MoveMouse(float x, float y)
        {
            if (!Config.current.MoveMouseCursor)
                return;

            float windowScale = GetScalingFactor();

            int posX = (int)(((x * drawWidth)/ windowScale) + drawStartX/windowScale);
            int posY = (int)((y * drawHeight)/ windowScale);
            RECT rect = new RECT();
            GetWindowRect(gameWHnd, ref rect);
            //Console.WriteLine("Window Pos: {0},{1}", rect.Left, rect.Top);
            int cursorX = rect.Left + posX;
            int cursorY = rect.Top + posY;

            //Move mouse before processing click
            for (int attempts = 2; attempts > 0; attempts--)
            {
                PostMessage(gameWHnd, 0x0200, 1, MakeLParam(posX, posY));
                //SendMessage(gameWHnd, 0x0200, 1, MakeLParam(posX, posY));
                
                Task.Delay(10).Wait();
                Cursor.Position = new System.Drawing.Point(cursorX, cursorY);
                Task.Delay(10).Wait();
            }
        }

        public static void Click(float x, float y, bool rightClick = false, bool async = true, int delayTime = 50, bool moveMouse = false)
        {
            if (async)
                Task.Run(() => ClickTask(x, y, rightClick, delayTime, moveMouse));
            else
                ClickTask(x, y, rightClick, delayTime, moveMouse);
        }

        public static void Click(Vector2 clickPos, bool rightClick = false)
        {
            Click(clickPos.X, clickPos.Y, rightClick);
        }

        static void ClickTask(float downX, float downY, float upX, float upY)
        {
            float windowScale = GetScalingFactor();

            int clickX = (int)(((downX * drawWidth)/ windowScale) + drawStartX/windowScale);
            int clickY = (int)((downY * drawHeight)/windowScale);

            int clickUpX = (int)(((upX * drawWidth)/ windowScale) + drawStartX/windowScale);
            int clickUpY = (int)((upY * drawHeight)/ windowScale);

            PostMessage(gameWHnd, WM_LBUTTONDOWN, 1, MakeLParam(clickX, clickY));
            Task.Delay(50).Wait();
            PostMessage(gameWHnd, WM_LBUTTONUP, 0, MakeLParam(clickUpX, clickUpY));
        }

        static void ClickTask(float x, float y, bool rightClick = false, int delayTime = 50, bool moveMouse = false)
        {
            float windowScale = GetScalingFactor();

            int clickX = (int)(((x * drawWidth)/windowScale) + drawStartX/windowScale);
            int clickY = (int)((y*drawHeight)/windowScale);

            //Console.WriteLine("ClickX: {0} ClickY: {1}", clickX, clickY);

            //mem.WriteMemory("PlantsVsZombies.exe+00329670,D9C", "byte", "1");   //Set windowFocus variable to true

            uint clickDown = rightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint clickUp = rightClick ? WM_RBUTTONUP : WM_LBUTTONUP;

            //Overwrite mouse position in widgetManager
            //mem.WriteMemory(lawnAppPtr + ",320,108", "int", clickX.ToString());
            //mem.WriteMemory(lawnAppPtr + ",320,10c", "int", clickY.ToString());

            if (moveMouse && Config.current.MoveMouseCursor)
            {
                RECT rect = new RECT();
                GetWindowRect(gameWHnd, ref rect);
                //Console.WriteLine("Window Pos: {0},{1}", rect.Left, rect.Top);
                int cursorX = rect.Left + clickX;
                int cursorY = rect.Top + clickY;
                Cursor.Position = new System.Drawing.Point(cursorX, cursorY);

                //Move mouse before processing click
                PostMessage(gameWHnd, 0x0200, 1, MakeLParam(clickX, clickY));

                Task.Delay(delayTime).Wait();
            }

            PostMessage(gameWHnd, clickDown, 1, MakeLParam(clickX, clickY));
            
            Task.Delay(delayTime).Wait();

            PostMessage(gameWHnd, clickUp, 0, MakeLParam(clickX, clickY));
        }


        //static uint addr_tooltip_plantID = 0;


        static Process HookProcess()
        {

            //bool didOpen = mem.OpenProcess("PlantsVsZombies.exe");

            Process[] foundProcs = null;
            Console.WriteLine("Searching for game process...");
            while (foundProcs == null || foundProcs.Length < 1)
            {
                foundProcs = Process.GetProcessesByName("PlantsVsZombies");
                Task.Delay(100).Wait();
                if((foundProcs == null || foundProcs.Length < 1) && Config.current.AutoLaunchGame && !steamLaunchAttempted)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(Config.current.GameStartPath);
                    startInfo.WorkingDirectory = Directory.GetParent(Config.current.GameStartPath).FullName;
                    startInfo.UseShellExecute = true;
                    if (Config.current.GameStartPath.StartsWith("steam:"))
                        steamLaunchAttempted = true;
                    Console.WriteLine("Starting '{0}' in '{1}'", Config.current.GameStartPath, startInfo.WorkingDirectory);
                    Process.Start(startInfo);
                    return HookProcess();
                }
            }

            bool isSteam = false;
            int parentPid = GetParentProcessId(foundProcs[0]);
            if (parentPid != -1)
            {
                try
                {
                    Process parentProc = Process.GetProcessById(parentPid);
                    if (parentProc != null)
                    {
                        Console.WriteLine("Parent process: Pid: {0}, Name: {1}", parentPid, parentProc.ProcessName);
                        if (parentProc.ProcessName == "steam")
                            isSteam = true;
                    }
                }
                catch
                {
                    Console.WriteLine("Parent process exited! Assuming non-steam");
                }
            }

            Console.WriteLine("IsSteam: {0}", isSteam);

            ProcessModule? mainModule = null;
            try
            {
                mainModule = foundProcs[0].MainModule;
            }
            catch { }
            if(mainModule == null)
                return HookProcess();   //Process probably closed/crashed, or hasn't loaded yet. Try again.
            string procDir = mainModule.FileName;
            procDir = Path.GetDirectoryName(procDir);
            appPath = procDir;
            Console.WriteLine(procDir);

            bool reqpopcapgame1 = false;
            while (foundProcs[0].Threads == null || foundProcs[0].Threads.Count < 1)
            {
                foundProcs = Process.GetProcessesByName("PlantsVsZombies");
                Task.Delay(100).Wait();
            }

            Console.WriteLine("Thread count: " + foundProcs[0].Threads.Count);
            //TODO: Less hacky solution to detect launcher vs popcapgames1
            //Launcher doesn't use many threads
            if (foundProcs[0].Threads.Count < 10)
                reqpopcapgame1 = true;

            Process? gameProc = null;

            int processIndex = 0;
            
            //See if we can find popcapgame first
            Process[] procs2 = null;

            for (processIndex = 0; processIndex < appNameArray.Length; processIndex++)
            {
                procs2 = Process.GetProcessesByName(appNameArray[processIndex]);
                if (procs2 is { Length: > 0 })
                {
                    break;
                }
            }
            
            if (procs2 != null && procs2.Length > 0)
            {
                gameProc = procs2[0];
                appName = appNameArray[processIndex] + ".exe";
            }
            else
            {
                if (reqpopcapgame1)
                {
                    while (procs2 == null || procs2.Length < 1)
                    {
                        for (processIndex = 0; processIndex < appNameArray.Length; processIndex++)
                        {
                            procs2 = Process.GetProcessesByName(appNameArray[processIndex]);
                            if (procs2 is { Length: > 0 })
                            {
                                break;
                            }
                        }

                        //If plantsVsZombies.exe starts more threads, it's not a launcher. So stop searching for popcapagame1
                        foundProcs = Process.GetProcessesByName("PlantsVsZombies");
                        if (foundProcs != null && foundProcs.Length > 0 && foundProcs[0].Threads.Count >= 10)
                        {
                            reqpopcapgame1 = false;
                            break;
                        }
                        Task.Delay(100).Wait();
                    }

                    if (reqpopcapgame1)
                    {
                        gameProc = procs2[0];
                        appName = appNameArray[processIndex]+ ".exe";
                    }
                }

                if(!reqpopcapgame1)
                {
                    gameProc = foundProcs[0];
                    appName = appNamePvz;
                }
            }
            

            bool didOpen = mem.OpenProcess(gameProc.Id);

            //TODO: Detect game version

            string? versionStr = null;

            try
            {
                versionStr = gameProc.MainModule.FileVersionInfo.ProductVersion;
            }
            catch { }

            //Steam version creates temporary/locked popcapgames1.exe, which will fail to grab version info. If that's the case, grab the verison info from PlantsVsZombies.exe instead.
            if (versionStr is null && Array.IndexOf(appNameArray,Path.GetFileNameWithoutExtension(appName))!=-1 && foundProcs != null && foundProcs.Length > 0)
            {
                try
                {
                    versionStr = foundProcs[0].MainModule.FileVersionInfo.ProductVersion;
                }
                catch { }
            }

            if (versionStr == null)
                versionStr = "";

            versionStr = string.Concat(versionStr.Where(char.IsDigit));


            uint versionNum = 0;

            if (!uint.TryParse(versionStr, out versionNum))
            {
                Console.WriteLine("Failed to parse game version!");
                Program.Say("Failed to parse game version!");
                input.GetKey();
                Environment.Exit(1);
            }

            memIO = new MemoryIO(appName, versionNum, mem);


            //memIO = new MemoryIO(appName, 1201073, mem);

            gameWHnd = gameProc.MainWindowHandle;
            //bool didOpen = mem.OpenProcess(Process.GetProcessesByName("PlantsVsZombies.exe")[0].Id);
            Console.WriteLine("DidOpen: " + didOpen);

            if (!didOpen)
                Environment.Exit(1);

            if (isSteam)
                Config.current.GameStartPath = "steam://rungameid/3590";
            else if (!reqpopcapgame1)
            {
                try
                {
                    Config.current.GameStartPath = gameProc.MainModule.FileName;
                }
                catch { }
            }
            else if (foundProcs != null && foundProcs.Length > 0 && foundProcs[0].MainModule != null)
            {
                try
                {
                    Config.current.GameStartPath = foundProcs[0].MainModule.FileName;
                }
                catch { }
            }

            Config.SaveConfig();

            return gameProc;

        }

        static float GetFrequencyForValue(int key)
        {
            int keyIndex = (12 * 4) + key;
            return (float)Math.Pow(2, (keyIndex - 49) / 12.0) * 440;
        }

        static float GetFrequencyForValue1(int key)
        {
            int keyIndex = (12 * 4) + key;
            return (float)Math.Pow(2, (keyIndex - 49) / 9.0) * 440;
        }
        static float GetFrequencyForValue2(int key)
        {
            int keyIndex = (12 * 4) + key;
            return (float)Math.Pow(2, (keyIndex - 49) / 6.0) * 440;
        }

        static float GetFrequnecyForKey(int octave, int key)
        {
            int keyIndex = 12 * octave + key;
            return (float)Math.Pow(2, (keyIndex - 49) / 12.0) * 440;

            
        }
        
        static string[] appNameArray = ["popcapgame1","popcapgame2"];
        static string appNamePopcap = "popcapgame1.exe";
        static string appNamePvz = "PlantsVsZombies.exe";
        static string appName = "";

        static string appPath = ""; //Path where data.pak is stored (if popcapgame1.exe, use dir of PlantsVsZombies.exe, not popcapgame1.exe)

        //const string lawnAppPtrOffset = "+00329670"; //1.2.0.1073 (cracked goty)
        //const string lawnAppPtr = "PlantsVsZombies.exe+00329670"; //1.2.0.1073 (cracked goty)
        
        //const string lawnAppPtr = "PlantsVsZombies.exe+00331C50";   //1.2.0.1096 (latest steam)
        //const string dirtyBoardPtrOffset = ",320,18,0,8";  //Not updated properly at main menu
        //const string boardPtr = lawnAppPtr + ",868";   //Always accurate. Null if not in game (board)
        //const string boardPtrOffset = ",868";   //Always accurate. Null if not in game (board)

        //static string lawnAppPtr = appName + lawnAppPtrOffset;
        //static string boardPtr = lawnAppPtr + boardPtrOffset;
        //static string dirtyBoardPtr = lawnAppPtr + dirtyBoardPtrOffset;        
        

        struct Zombie
        {
            public int zombieType;
            public int phase;
            public float posX;
            public float posY;
            public int row;    //From GameObject
        }

        //TODO: Move to board class
        public struct PlantOnBoard
        {
            public int plantType;
            public int row;
            public int column;
            public int state;
            public int magItem;

            //Only gathered when GetPlantAtCell is called
            public bool hasPumpkin;
            public bool hasPot;
            public bool hasLillypad;
            public bool hasLadder;
            public bool squished;
            public bool sleeping;

            public int health;
            public int pumpkinHealth;
        }

        //TODO: Move to board class
        public struct GridItem
        {
            public int type;
            public int state;
            public int x;
            public int y;

            //Positions that aren't exact board tiles (stinky uses these)
            public float floatX;
            public float floatY;

            public int vasePlant;
            public int vaseZombie;
            public int transparent;
        }


        //TODO: move to memio
        public static int GetCursorType()
        {
            return mem.ReadInt(memIO.ptr.boardChain + ",150,30");
        }

        //TODO: Move to memio
        static int GetCursorPlantID()
        {
            return mem.ReadInt(memIO.ptr.boardChain + ",150,28");
        }

        public static void GameplayTutorial(string[] tutorial)
        {
            foreach (var tut in tutorial)
                GameplayTutorial(tut);
        }

        public static void GameplayTutorial(string tutorial)
        {
            memIO.SetBoardPaused(true);
            input.ClearIntents();

            int currentLine = 0;
            string[] splitLines = tutorial.Split("\r\n");
            int lineCount = splitLines.Length;

            Console.WriteLine(tutorial);
            Say(tutorial);

            InputIntent intent = input.GetCurrentIntent();
            while (intent != InputIntent.Confirm)
            {
                if (intent is InputIntent.Up)
                    currentLine--;
                if (intent is InputIntent.Down)
                    currentLine++;

                if (intent is InputIntent.Up or InputIntent.Down)
                {
                    currentLine = currentLine < 0 ? 0 : currentLine;
                    currentLine = currentLine >= lineCount ? lineCount - 1 : currentLine;
                    Console.WriteLine(splitLines[currentLine]);
                    Say(splitLines[currentLine]);
                }
                intent = input.GetCurrentIntent();

                bool gameClosed = false;
                try { gameClosed = memIO.mem.mProc.Process.HasExited; } catch { }
                if (gameClosed)
                    Environment.Exit(0);
            }
            memIO.SetBoardPaused(false);
        }


        //TODO: Move to board class
        public static List<GridItem> GetGridItems()
        {
            List<GridItem> gridItems = new List<GridItem>();

            int maxCount = mem.ReadInt(memIO.ptr.boardChain + ",138");

            for(int i = 0; i < maxCount; i++)
            {
                int index = i * 236;
                bool isActive = mem.ReadByte(memIO.ptr.boardChain + ",134," + (index + 0x20).ToString("X2")) == 0;
                if (!isActive)
                    continue;

                GridItem gridItem = new GridItem();
                gridItem.type = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x08).ToString("X2"));
                gridItem.state = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x0c).ToString("X2"));
                gridItem.x = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x10).ToString("X2"));
                gridItem.y = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x14).ToString("X2"));
                gridItem.floatX = mem.ReadFloat(memIO.ptr.boardChain + ",134," + (index + 0x24).ToString("X2"));
                gridItem.floatY = mem.ReadFloat(memIO.ptr.boardChain + ",134," + (index + 0x28).ToString("X2"));
                gridItem.vaseZombie = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x3c).ToString("X2"));
                gridItem.vasePlant = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x40).ToString("X2"));
                gridItem.transparent = mem.ReadInt(memIO.ptr.boardChain + ",134," + (index + 0x4c).ToString("X2"));
                gridItems.Add(gridItem);

                //Console.WriteLine("Added item at {0} {1}", gridItem.x, gridItem.y);

            }

            return gridItems;
        }
        
        public static void Debug_FinishLevel()
        {
            mem.WriteMemory(memIO.ptr.boardChain + ",5614", "byte", "1");
        }

        //TODO: Move to board class
        public static PlantOnBoard GetPlantAtCell(int x, int y)
        {
            var plants = GetPlantsOnBoard();

            bool hasPumpkin = false;
            int plantID = -1;   //None
            bool hasPot = false;
            bool hasLillypad = false;
            bool squished = false;
            bool sleeping = false;
            int state = 0;
            int magItem = 0;
            bool rightCobCannon = false;
            int health = 0;
            int pumpkinHealth = 0;
            int lilypadHealth = 0;
            int flowerpotHealth = 0;
            for(int i =0; i < plants.Count; i++)
            {
                if (plants[i].column == x - 1 && plants[i].row == y && plants[i].plantType == (int)SeedType.SEED_COBCANNON)
                {
                    state = plants[i].state;
                    rightCobCannon = true;
                }
                
                if (plants[i].column != x)
                    continue;
                if (plants[i].row != y)
                    continue;

                if (plants[i].plantType == (int)SeedType.SEED_PUMPKINSHELL)
                {
                    hasPumpkin = true;
                    pumpkinHealth = plants[i].health;
                }
                else if (plants[i].plantType == (int)SeedType.SEED_LILYPAD)
                {
                    hasLillypad = true;
                    lilypadHealth = plants[i].health;
                }
                else if (plants[i].plantType == (int)SeedType.SEED_FLOWERPOT)
                {
                    hasPot = true;
                    flowerpotHealth = plants[i].health;
                }
                else
                {
                    plantID = plants[i].plantType;
                    state = plants[i].state;
                    health = plants[i].health;
                }
                
                if (plants[i].magItem != 0)
                    magItem = plants[i].magItem;

                squished |= plants[i].squished;
                sleeping |= plants[i].sleeping;
            }

            bool hasLadder = false;
            var gridItems = GetGridItems();
            foreach(var gridItem in gridItems)
            {
                if (gridItem.type == (int)GridItemType.Ladder && gridItem.x == x && gridItem.y == y)
                    hasLadder = true;
            }

            if (rightCobCannon)
                plantID = (int)SeedType.SEED_COBCANNON;

            PlantOnBoard plant;
            plant.row = x;
            plant.column = y;
            plant.hasPumpkin = hasPumpkin;
            plant.plantType = plantID;
            plant.hasPot = hasPot;
            plant.hasLillypad = hasLillypad;
            plant.squished = squished;
            plant.sleeping = sleeping;
            plant.state = state;
            plant.magItem = magItem;
            plant.hasLadder = hasLadder;
            plant.pumpkinHealth = pumpkinHealth;
            plant.health = health;

            if (plant.hasLillypad && plant.plantType == -1)
            {
                plant.plantType = (int)SeedType.SEED_LILYPAD;
                plant.health = lilypadHealth;
            }
            if (plant.hasPot && plant.plantType == -1)
            {
                plant.plantType = (int)SeedType.SEED_FLOWERPOT;
                plant.health = flowerpotHealth;
            }
            if (plant.hasPumpkin && plant.plantType == -1)
            {
                plant.plantType = (int)SeedType.SEED_PUMPKINSHELL;
                plant.health = pumpkinHealth;
            }

            return plant;
        }

        //TODO: Move to board class
        public static List<PlantOnBoard> GetPlantsOnBoard()
        {
            List<PlantOnBoard> plants = new List<PlantOnBoard>();

            int maxCount = mem.ReadInt(memIO.ptr.boardChain + ",c8");
            //int currentCount = mem.ReadInt(boardPtr + ",d4");

            for(int i =0; i < maxCount; i++)
            {
                int index = i * 332;
                byte isDead = (byte)mem.ReadByte(memIO.ptr.boardChain + ",c4," + (index + 0x141).ToString("X2"));
                byte isSquished = (byte)mem.ReadByte(memIO.ptr.boardChain + ",c4," + (index + 0x142).ToString("X2"));
                byte isSleeping = (byte)mem.ReadByte(memIO.ptr.boardChain + ",c4," + (index + 0x143).ToString("X2"));
                byte onBoard = (byte)mem.ReadByte(memIO.ptr.boardChain + ",c4," + (index + 0x144).ToString("X2"));

                if (onBoard == 1 && isDead == 0)
                {
                    PlantOnBoard p = new PlantOnBoard();
                    p.squished = isSquished == 1;
                    p.sleeping = isSleeping == 1;
                    p.row = mem.ReadInt(memIO.ptr.boardChain + ",c4," + (index + 0x1c).ToString("X2"));
                    p.plantType = mem.ReadInt(memIO.ptr.boardChain + ",c4," + (index + 0x24).ToString("X2"));
                    p.column = mem.ReadInt(memIO.ptr.boardChain + ",c4," + (index + 0x28).ToString("X2"));
                    p.state = mem.ReadInt(memIO.ptr.boardChain + ",c4," + (index + 0x3c).ToString("X2"));
                    p.health = mem.ReadInt(memIO.ptr.boardChain + ",c4," + (index + 0x40).ToString("X2"));

                    //magnetItems: c8
                    for(int mag = 0; mag < 5; mag++)
                    {
                        int magIndex = index + 0xc8 + (mag * 0x14) + 0x10;
                        int magItem = mem.ReadInt(memIO.ptr.boardChain + ",c4," + (magIndex).ToString("X2"));
                        if (magItem > 0 && (magItem <= 17 || magItem == 21))
                            p.magItem = magItem;
                    }

                    plants.Add(p);
                }

            }

            return plants;
        }

        //returns amount of sun currently on its way to the sun bank
        static int CollectBoardStuff(Widget currentWidget)
        {
            if (currentWidget is not (Board or ZenGarden))
                return 0;
            
            //Make sure we're actually on the board
            bool hasBoardPtr = mem.ReadUInt(memIO.ptr.boardChain) != 0;
            if (!hasBoardPtr)
                return 0;

            //And we aren't holding anything with our cursor (plant/shovel)
            //Don't care if it's whack-a-zombie, it's fine.
            int cursorType = GetCursorType();
            if (cursorType > 0 && cursorType != 7)
                return 0;

            int sunAmount = 0;

            //Grab all coins, sunflowers, awards
            int maxCount = mem.ReadInt(memIO.ptr.boardChain + ",100");
            //List<Vector2> clickables = new List<Vector2>();
            for(int i =0; i < maxCount; i++)
            {
                int index = i * 216;

                int coinType = mem.ReadInt(memIO.ptr.boardChain + ",fc," + (index + 0x58).ToString("X2"));

                //Skip inactive clickables
                if (mem.ReadByte(memIO.ptr.boardChain + ",fc," + (index + 0x38).ToString("X2")) == 1)
                    continue;

                //Skip seed packets
                if (coinType == (int)CoinType.UsableSeedPacket)
                    continue;

                //Skip collectables we've already clicked on
                if (mem.ReadByte(memIO.ptr.boardChain + ",fc," + (index + 0x50).ToString("X2")) == 1)
                {

                    switch ((CoinType)coinType)
                    {
                        case CoinType.Sun:
                            sunAmount += 25;
                            break;
                        case CoinType.Smallsun:
                            sunAmount += 15;
                            break;
                    }

                    continue;
                }

                
                //Get pos, add a couple of pixels to account for rounding errors
                Vector2 pos = new Vector2();
                pos.X = (mem.ReadFloat(memIO.ptr.boardChain + ",fc," + (index + 0x24).ToString("X2"))+8.0f) / 800.0f;
                pos.Y = (mem.ReadFloat(memIO.ptr.boardChain + ",fc," + (index + 0x28).ToString("X2"))+8.0f) / 600.0f;

                //If at/above the seed picker/bank, don't click.
                if (pos.Y < 0.15f)
                    continue;

                //Don't try to collect anything while paused
                if (memIO.GetBoardPaused() && currentWidget is Board)
                    continue;

                //Wait until click goes through
                Click(pos);

                if (Config.current.SayCoinValueOnCollect)
                {
                    string sayStr = Text.game.coinCount;
                    int count = 0;
                    switch ((CoinType)coinType)
                    {
                        case CoinType.Silver:
                            count = 10;
                            break;
                        case CoinType.Gold:
                            count = 50;
                            break;
                        case CoinType.Diamond:
                            count = 1000;
                            break;
                        case CoinType.AwardMoneyBag:
                            count = 250;
                            break;
                        case CoinType.AwardBagDiamond:
                            count = 3000;
                            break;
                    }

                    if(count != 0)
                    { 
                        sayStr = sayStr.Replace("[0]", FormatNumber(count));
                        Console.WriteLine(sayStr);
                        Say(sayStr);
                    }
                }

                //If we're now holding a plant after clicking, don't click anything else.
                cursorType = GetCursorType();

                if(cursorType == 2)
                {
                    int plantHeldID = GetCursorPlantID();
                    if (plantHeldID == -1)
                        return sunAmount;

                    string plantHoldStr = Text.plantNames[plantHeldID] + " in hand.";
                    Console.WriteLine(plantHoldStr);
                    Say(plantHoldStr, true);
                    return sunAmount;
                }

                if (cursorType > 0 && cursorType != 7)
                    return sunAmount;
            }

            return sunAmount;
        }

        static void DoWidgetInteractions(Widget currentWidget, Input input)
        {
            //Ensure game window is focused
            nint focusedWindow = GetForegroundWindow();
            if(gameWHnd.ToInt32() != focusedWindow)
            {
                input.ClearIntents();
                return;
            }

            InputIntent intent = input.GetCurrentIntent();
            if (intent != InputIntent.None)
                currentWidget.Interact(intent);
        }

        static plantInBoardBank[] GetPlantsInBoardBank()
        {
            //plantInBoardBank[] plants = new plantInBoardBank[10];
            List<plantInBoardBank> newPlants = new List<plantInBoardBank>();

            byte[] plantBytes = mem.ReadBytes(memIO.ptr.lawnAppPtr + ",868,15c,28", 800);    //yucky

            //On conveyor levels, for each plant at offsetX == 0, the max offsetX should decrease by idk something
            int maxX = 450;
            int stoppedPlants = 0;

            for(int i =0; i < 10; i++)
            {
                int byteIndex = i * 80;
                //plants[i] = new plantInBoardBank();

                plantInBoardBank plant = new plantInBoardBank();

                plant.refreshCounter = BitConverter.ToInt32(plantBytes, byteIndex + 0x24);
                plant.refreshTime = BitConverter.ToInt32(plantBytes, byteIndex + 0x28);
                plant.index = BitConverter.ToInt32(plantBytes, byteIndex + 0x2c);
                plant.offsetX = BitConverter.ToInt32(plantBytes, byteIndex + 0x30);
                plant.absX = BitConverter.ToInt32(plantBytes, byteIndex + 0x08);
                plant.absX += plant.offsetX;
                plant.absX /= 800.0f;
                plant.packetType = BitConverter.ToInt32(plantBytes, byteIndex + 0x34);
                plant.imitaterType = BitConverter.ToInt32(plantBytes, byteIndex + 0x38);
                plant.isRefreshing = BitConverter.ToBoolean(plantBytes, byteIndex + 0x49);

                //Console.WriteLine(plant.offsetX);
                //Console.WriteLine(plant.packetType);

                if (plant.offsetX <= 0)
                    stoppedPlants++;

                //Console.WriteLine("Plant absX: " + plant.absX);

                if(plant.absX < 0.72f)
                //if (plant.offsetX < maxX - (stoppedPlants*50))
                    newPlants.Add(plant);

                //Console.WriteLine("i: {0}, Index: {1}, Type: {2}", i, plants[i].index, plants[i].packetType);
            }

            //Fill rest of list with empty plants
            while (newPlants.Count < 10)
                newPlants.Add(new plantInBoardBank() { packetType = -1 });

            return newPlants.ToArray();

            //return plants;
        }

        static plantInPicker[] GetSelectedPlants(bool refreshPlants = true)
        {
            if (refreshPlants)
                RefreshPlantPickerState();

            
            List<plantInPicker> allSelectedPlants = new List<plantInPicker>();
            //plantInPicker

            //allSelectedPlants = Array.resi

            for (int i = 0; i < plantPickerState.Length; i++)
            {
                if ((int)plantPickerState[i].seedState == 1)
                {
                    allSelectedPlants.Add(plantPickerState[i]);
                    //Console.WriteLine("Index in bank:" + plantPickerState[i].indexInBank);
                    //Console.WriteLine("State:" + (int)plantPickerState[i].seedState);
                    //Console.WriteLine("State:" + plantPickerState[i].seedT);
                }
            }

            plantInPicker[] sortedPlants = new plantInPicker[allSelectedPlants.Count];

            for (int i = 0; i < allSelectedPlants.Count; i++)
            {
                for (int j = 0; j < allSelectedPlants.Count; j++)
                {
                    if (allSelectedPlants[j].indexInBank == i)
                        sortedPlants[i] = allSelectedPlants[j];
                }
            }

            return sortedPlants;
        }

        public static Widget GetActiveWidget(Widget? currentWidget)
        {
            uint focusedWidgetVtableID = mem.ReadUInt(memIO.ptr.lawnAppPtr + ",320,a0,0");

            uint baseWidgetVtableID = mem.ReadUInt(memIO.ptr.lawnAppPtr + ",320,ac,0");
            uint baseWidgetDialogID = mem.ReadUInt(memIO.ptr.lawnAppPtr + ",320,ac" + memIO.ptr.dialogIDOffset);

            uint vtableID = baseWidgetVtableID;
            string ptrChain = memIO.ptr.lawnAppPtr + ",320,ac";

            if (baseWidgetVtableID == 0)
            {
                vtableID = focusedWidgetVtableID;
                ptrChain = memIO.ptr.lawnAppPtr + ",320,a0";
            }

            uint dialogID = mem.ReadUInt(ptrChain + memIO.ptr.dialogIDOffset);

            if(dialogID == DialogIDs.stamCloudLocalChoice)
            {
                if (currentWidget is not SteamSaveChoice)
                    return new SteamSaveChoice(memIO,ptrChain);
                return currentWidget;
            }

            //Console.WriteLine("Read dialogID: " + dialogID);

            if(currentWidget is null)
                currentWidget = new Placeholder(memIO);


            //If in zen garden, and pause menu is not open. Use garden.
            int gameMode = memIO.GetGameMode();
            int gameScene = (int)memIO.GetGameScene();

            if (dialogID == DialogIDs.CreditsPaused)
            {
                if (currentWidget is not ButtonPicker)
                    return new ButtonPicker(memIO, ptrChain);
                return currentWidget;
            }

            if (gameScene == (int)GameScene.Credits)
            {
                if(currentWidget is not Credits)
                    return new Credits(memIO);
                return currentWidget;
            }

            if(gameScene == (int)GameScene.MinigameSelector)
            {
                if (currentWidget is not BonusModeMenu)
                    return new Widgets.BonusModeMenu(memIO);
            }

            //Todo: Does this really need an almanac check?
            if ((gameMode == (int)GameMode.ZenGarden || gameMode == (int)GameMode.TreeOfWisdom) && (gameScene == 2 || gameScene == 3) && dialogID != DialogIDs.NewOptions && dialogID != DialogIDs.Almanac)
            {
                //If we're not in the store
                if (baseWidgetDialogID != DialogIDs.Store && baseWidgetDialogID != DialogIDs.ZenSell && baseWidgetDialogID != DialogIDs.StorePurchase && !DaveStoreOverride(memIO.GetDaveMessageID()))
                {
                    if (currentWidget is not ZenGarden)
                        return new ZenGarden(memIO);
                    return currentWidget;
                }
            }

            //DEBUG: Do we actually need to read crazyDave vtableID?
            //if (vtableID == memIO.ptr.widgetType.CrazyDave)
              //  vtableID = 0;

            //Sometimes crazy dave appears without setting focus widget :(
            //But we don't want that on the store (mId 4)
            int crazyDaveMsgIndex = memIO.GetDaveMessageID();
            if (DaveStoreOverride(crazyDaveMsgIndex))
                vtableID = memIO.ptr.widgetType.CrazyDave;
            else if (crazyDaveMsgIndex != -1 && dialogID != DialogIDs.Store)
                vtableID = memIO.ptr.widgetType.CrazyDave;
            else if (dialogID == DialogIDs.Store)
            {
                //vtableID = (uint)WidgetType.Store;  //Overwrite vtableID, to prevent another widget from overwriting the store
                if (currentWidget is not Store)
                    return new Widgets.Store(memIO, ptrChain);
                return currentWidget;
            }

            if (gameScene == (int)GameScene.AwardScreen && dialogID != DialogIDs.Almanac && dialogID != DialogIDs.Store && dialogID != DialogIDs.CrazyDave && dialogID != DialogIDs.steamCloudSavingActive)
            {
                if (currentWidget is not AwardScreen)
                    return new Widgets.AwardScreen(memIO, ptrChain);
            }


            //options menu has mID of 2? || widget.type == (uint)WidgetType.Options
            //Check if plantPicker
            bool wallnutBowling = (memIO.GetPlayerLevel() == 5) && memIO.GetGameScene() == (int)GameScene.SeedPicker && gameMode == (int)GameMode.Adventure;

            //Sometimes seed picker isn't the active widget, despite it being the focal point
            //Eg if you open the pause menu on the seed picker, and unpause again, the board will become the active widget :(
            bool seedPickerCheck = memIO.GetGameScene() == (int)GameScene.SeedPicker && vtableID == memIO.ptr.widgetType.Board;
            seedPickerCheck |= vtableID == memIO.ptr.widgetType.SeedPicker;

            //Console.WriteLine("Seedpicker check: {0}", seedPickerCheck);
            //Console.WriteLine("vtableID: {0}", vtableID);
            //Console.WriteLine("Esxpected ID: {0}", memIO.ptr.widgetType.SeedPicker);

            seedPickerCheck &= !wallnutBowling; //No seed picker on wallnut bowling (damn dave breaking shit)

            if (seedPickerCheck)
            {
                if (currentWidget is not SeedPicker)
                    return new SeedPicker(memIO);
                return currentWidget;
            }

            if(dialogID == DialogIDs.ZombatarLicense)
            {
                if (currentWidget is not ZombatarLicenseAgreement)
                    return new ZombatarLicenseAgreement(memIO, ptrChain);
                return currentWidget;
            }


            //Gotta handle crazy dave first, because he can appear during the other widgets :/
            if (vtableID == memIO.ptr.widgetType.CrazyDave)
            {
                if(currentWidget is not CrazyDave)
                    return new Widgets.CrazyDave(memIO,ptrChain, currentWidget);
            }
            //else if (vtableID == (uint)WidgetType.ContinueGame)
            else if (dialogID == DialogIDs.Continue)
            {
                if(currentWidget is not ContinueGame)
                    return new Widgets.ContinueGame(memIO, ptrChain);
            }
            else if(vtableID == memIO.ptr.widgetType.UserName || dialogID == DialogIDs.CreateUser)
            {
                if (currentWidget is not UserName)
                    return new UserName(memIO, ptrChain, dialogID == DialogIDs.CreateUser);
            }
            else if (vtableID == memIO.ptr.widgetType.SimpleDialogue || dialogID == DialogIDs.GameOver || dialogID == DialogIDs.steamCloudSavingActive)
            {
                //TODO: Might be an issue with consecutive buttonpickers (are there any?)
                if (currentWidget is not ButtonPicker)
                    return new Widgets.ButtonPicker(memIO, ptrChain, currentWidget);
            }
            //else if (vtableID == (uint)WidgetType.UserPicker)
            else if (dialogID == DialogIDs.UserDialog)
            {
                if(currentWidget is not UserPicker)
                    return new Widgets.UserPicker(memIO, ptrChain);
            }
            else if (vtableID == memIO.ptr.widgetType.MainMenu)
            {
                if (currentWidget is not MainMenu)
                    return new Widgets.MainMenu(memIO);
            }
            else if (dialogID == DialogIDs.NewOptions)
            {
                if(currentWidget is not OptionsMenu)
                    return new Widgets.OptionsMenu(memIO, ptrChain, currentWidget);
            }
            else if (dialogID == DialogIDs.Almanac)
            //else if (vtableID == memIO.ptr.widgetType.Almanac)
            {
                if(currentWidget is not Almanac)
                    return new Widgets.Almanac(memIO, ptrChain);
            }
            else if (vtableID == memIO.ptr.widgetType.Board || memIO.GetGameScene() == (int)GameScene.Board)
            {
                if (currentWidget is not Board)
                {
                    //Restore board state, if closing options menu
                    if (currentWidget is OptionsMenu && ((OptionsMenu)currentWidget).previousWidget != null && ((OptionsMenu)currentWidget).previousWidget is Board)
                        return ((OptionsMenu)currentWidget).previousWidget;
                    else if(currentWidget is ButtonPicker && ((ButtonPicker)currentWidget).prevWidget != null && ((ButtonPicker)currentWidget).prevWidget is Board)
                        return ((ButtonPicker)currentWidget).prevWidget;
                    else if (currentWidget is CrazyDave && ((CrazyDave)currentWidget).prevWidget != null && ((CrazyDave)currentWidget).prevWidget is Board)
                        return ((CrazyDave)currentWidget).prevWidget;
                    return new Widgets.Board(memIO);
                }
            }

            return currentWidget;


        }

        static bool VaseBreakerCheck()
        {
            int level = memIO.GetPlayerLevel();
            int gameMode = memIO.GetGameMode();

            if (gameMode == 0 && level == 35)
                return true;

            if (gameMode >= (int)GameMode.VaseBreaker1 && gameMode <= (int)GameMode.VaseBreakerEndless)
                return true;

            return false;
        }

        static bool DaveStoreOverride(int daveMessageIndex)
        {
            //message index isn't set on zen garden plant sell dialogue, but message length *is* set, so use that instead.
            int gameScene = memIO.GetGameScene();
            bool inGarden = memIO.GetGameMode() == (int)GameMode.ZenGarden && (gameScene == 2 || gameScene == 3);

            uint baseWidgetMid = mem.ReadUInt(memIO.ptr.lawnAppPtr + ",320,ac" +memIO.ptr.dialogIDOffset);

            if (baseWidgetMid == DialogIDs.Store)
                inGarden = false;

            if (inGarden)
            {
                int daveMsgLength = mem.ReadInt(memIO.ptr.lawnAppPtr + ",988");
                if (daveMsgLength > 0)
                    return true;
            }

            //Zen garden store page intro
            if (daveMessageIndex >= 2600 && daveMessageIndex <= 2602)
                return true;
            
            //Shop intro
            if (daveMessageIndex >= 301 && daveMessageIndex <= 304)
                return true;

            //Taco dialogue after 4-4
            if (daveMessageIndex >= 601 && daveMessageIndex <= 606)
                return true;

            //New plants available message after 5-1
            if (daveMessageIndex == 3100)
                return true;

            //Tree of life intro
            if (daveMessageIndex == 3200)
                return true;

            //Zen garden intro
            if (daveMessageIndex >= 2100 && daveMessageIndex <= 2104)
                return true;

            //All plants achievement dialog
            if (daveMessageIndex >= 4000 && daveMessageIndex <= 4004)
                return true;

            //if(daveMessageIndex != -1)
              //  Console.WriteLine("NO DAVE OVERRIDE: " + daveMessageIndex);
            return false;
        }

        //TODO: Implement screenreader message history
        //TODO: Move all Console.WriteLine calls to here instead. Will cleanup the code significantly, and we don't want to print anything we aren't saying, anyway. (except when debugging of course)
        public static void Say(string? text, bool interrupt = true)
        {
            if (text == null || text.Length == 0)
                return;
            try
            {
                if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Auto)
                    Config.current.ScreenReader = Config.AutoScreenReader();
                if (Config.current.ScreenReader != null)
                    Config.current.ScreenReader.Speak(text, interrupt);
            }
            catch
            {
                Config.current.ScreenReader = Config.AutoScreenReader();
                Config.SaveConfig(Config.ScreenReaderSelection.Auto);

            }
        }

        static List<WaveOutEvent> waveOutEvents = new List<WaveOutEvent>();

        public struct ToneProperties
        {
            public float leftVolume;
            public float rightVolume;
            public float startFrequency;
            public float endFrequency;
            public float duration;
            public SignalGeneratorType signalType;
            public int startDelay;
        }

        public static void PlayTones(List<ToneProperties> tones)
        {
            if (tones.Count < 1)
                return;

            List<ISampleProvider> allSamples = new List<ISampleProvider>();
            foreach(var tone in tones)
            {
                var beepSignal = new SignalGenerator(44100, 1)
                {
                    Gain = 0.2,
                    Frequency = tone.startFrequency,
                    Type = tone.signalType,
                    FrequencyEnd = tone.endFrequency,
                }.Take(TimeSpan.FromMilliseconds(tone.duration));

                int startDelay = tone.startDelay < 16 ? 16 : tone.startDelay;

                var sinepause = new SignalGenerator(44100, 2)
                {
                    Gain = 0,
                    Frequency = 1,
                    Type = SignalGeneratorType.Sin,
                }.Take(TimeSpan.FromMilliseconds(startDelay));

                allSamples.Add(sinepause.FollowedBy(beepSignal.ToStereo(tone.leftVolume* Config.current.AudioCueMasterVolume, tone.rightVolume* Config.current.AudioCueMasterVolume)));
            }

            MixingSampleProvider mixer = new MixingSampleProvider(allSamples);
            var waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init(mixer);
            waveOutEvent.Play();
            waveOutEvents.Add(waveOutEvent);
        }

        public static void PlayBeghouledAssistTone()
        {
            List<ToneProperties> tones = new List<ToneProperties>();
            tones.Add(new ToneProperties() { leftVolume = Config.current.BeghouledAssistVolume, rightVolume = Config.current.BeghouledAssistVolume, startFrequency = 700, endFrequency = 700, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = 0 });
            tones.Add(new ToneProperties() { leftVolume = Config.current.BeghouledAssistVolume, rightVolume = Config.current.BeghouledAssistVolume, startFrequency = 800, endFrequency = 800, duration = 200, signalType = SignalGeneratorType.Sin, startDelay = 100 });
            PlayTones(tones);
            Vibrate(1, 1, 150);
        }

        public static void PlayBoundaryTone()
        {
            PlayTone(Config.current.HitBoundaryVolume, Config.current.HitBoundaryVolume, 70, 70, 50, SignalGeneratorType.Square);
            Vibrate(0.1f, 0.1f, 50);
        }
        public static void PlayTone(float leftVolume, float rightVolume, float startFrequency, float endFrequency, float duration, SignalGeneratorType signalType = SignalGeneratorType.Sweep, int startDelay = 0)
        {            
            leftVolume *= Config.current.AudioCueMasterVolume;
            rightVolume *= Config.current.AudioCueMasterVolume;

            if (leftVolume == 0 && rightVolume == 0)
                return;
            
            var beepSignal = new SignalGenerator(44100, 1)
            {
                Gain = 0.2,
                Frequency = startFrequency,
                Type = signalType,
                FrequencyEnd = endFrequency,
            }.Take(TimeSpan.FromMilliseconds(duration));


            if (startDelay <= 16)
                startDelay = 16;

            var sinepause = new SignalGenerator(44100, 2)
            {
                Gain = 0,
                Frequency = 1,
                Type = SignalGeneratorType.Sin,
            }.Take(TimeSpan.FromMilliseconds(startDelay));

            var tempWaveOutEvents = waveOutEvents.ToList();

            foreach(var tempEvent in tempWaveOutEvents)
            {
                if (tempEvent.PlaybackState == PlaybackState.Stopped)
                {
                    tempEvent.Dispose();
                    waveOutEvents.Remove(tempEvent);
                }
            }
            
            var waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init(sinepause.FollowedBy(beepSignal.ToStereo(leftVolume, rightVolume)));
            waveOutEvent.Play();
            waveOutEvents.Add(waveOutEvent);
            return;
        }

        public static void Vibrate(float leftStrength, float rightStrength, int length)
        {
            if (!Config.current.ControllerVibration)
                return;
            XInput.SetVibration(0, leftStrength, rightStrength);
            vibrationEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + length;
        }

        public static bool CheckOwnedPlant(int plantID)
        {
            //All seeds <= 39 are unlocked if adventure mode has been completed
            bool unlockedPlant = memIO.GetAdventureCompletions() > 0;

            //Check if player has progressed far enough to have seed
            unlockedPlant |= plantID <= Program.MaxOwnedSeedIndex(memIO.GetPlayerLevel());

            //For purchasable upgrades, ensure player has purchased them
            if (plantID > 39)
                unlockedPlant = memIO.GetPlayerPurchase(plantID - 40) > 0;

            return unlockedPlant;
        }

        public static int MaxOwnedSeedIndex(int playerLevel)
        {
            int zone = ((playerLevel - 1) / 10) + 1;
            int lvl = ((playerLevel - 1) % 10) + 1;
            int unlockedSeeds = (zone - 1) * 8 + lvl;
            if (lvl >= 10)
                unlockedSeeds -= 2;
            //else if (lvl == 10)
                //unlockedSeeds -= 3;
            else if (lvl >= 5)
                unlockedSeeds -= 1;
            if (unlockedSeeds > 40)
                unlockedSeeds = 40;

            return unlockedSeeds -1;
        }

        public static bool IsAquatic(SeedType plant)
        {
            switch(plant)
            {
                case SeedType.SEED_LILYPAD:
                case SeedType.SEED_TANGLEKELP:
                case SeedType.SEED_SEASHROOM:
                case SeedType.SEED_CATTAIL:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNocturnal(SeedType plant)
        {
            switch(plant)
            {
                case SeedType.SEED_PUFFSHROOM:
                case SeedType.SEED_SUNSHROOM:
                case SeedType.SEED_FUMESHROOM:
                case SeedType.SEED_HYPNOSHROOM:
                case SeedType.SEED_SCAREDYSHROOM:
                case SeedType.SEED_ICESHROOM:
                case SeedType.SEED_DOOMSHROOM:
                case SeedType.SEED_SEASHROOM:
                case SeedType.SEED_MAGNETSHROOM:
                case SeedType.SEED_GLOOMSHROOM:
                    return true;
                default:
                    return false;
            }
        }

        //TODO: Rework this completely
        public static void GetMinMaxY(ref int minY, ref int maxY)
        {            
            //Get type of each row, to determine if plant can be placed there (eg; can't place on dirt rows in first few levels)
            int[] rowTypes = new int[6];
            for (int i = 0; i < 6; i++)
                rowTypes[i] = mem.ReadInt(memIO.ptr.boardChain + "," + (0x5f0 + (i*4)).ToString("X2"));

            minY = 0;
            for(int i =0; i < 6; i++)
            {
                if (rowTypes[i] == 0)
                    minY++;
                else
                    break;
            }

            maxY = 5;
            for(int i = 5; i > 0; i--)
            {
                if (rowTypes[i] == 0)
                    maxY--;
                else
                    break;
            }

            //Console.WriteLine("Min: {0} Max: {1}", minY, maxY);

        }



        //Converts number into a screenreader-friendly string (1234567 > 1,234,567)
        public static string FormatNumber(int value)
        {
            return value.ToString("N0");
        }

        static int VasebreakerHeldPlantID = -1;

        static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("I'm out of here");
            //nvda.StopSpeaking();
        }

        static void Main(string[] args)
        {
            Mutex mutex = new System.Threading.Mutex(false, "PvZ-A11y");
            if (!mutex.WaitOne(0, false))
                Environment.Exit(1);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Starting...");

            //This is pretty gross, but kind of required until more edge-cases are handled safely
            try
            {
                SafeMain(args);
            }
            catch (Exception ex)
            {
                bool gameClosed = false;
                try {gameClosed = memIO.mem.mProc.Process.HasExited;} catch { }
                if (gameClosed)
                    Environment.Exit(0);

                Console.WriteLine("PvZ A11y encountered an error.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.TargetSite);
                Console.WriteLine(ex.Data);

                string errorDump = ex.Message;
                errorDump += ex.StackTrace;
                errorDump += ex.InnerException;
                errorDump += ex.TargetSite;
                errorDump += ex.Data;
                try
                {
                    File.WriteAllText("Crashlog.txt", errorDump);
                }
                catch {
                    Console.WriteLine("Failed to save crashlog!");
                }

                Console.ReadLine();

                //Restart the mod
                if (Config.current != null && Config.current.RestartOnCrash)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        Arguments = "Restart"
                    });
                }

                Environment.Exit(1); // Close the current process

            }
            mutex.Close();
        }

        public static Input input = null;

        static long nextTripwireAlarmMs;
        static int tripwireAlarmState;
        static void PlayTripwireAlarm(bool intense)
        {
            //Fish are friends, not food
            if (memIO.GetGameMode() == (int)GameMode.Zombiquarium)
                return;

            //Zombies are frens in iZombie
            GameMode gameMode = (GameMode)memIO.GetGameMode();
            if (gameMode >= GameMode.IZombie1 && gameMode <= GameMode.IZombieEndless)
                return;

            if(tripwireAlarmState == 0 && !intense)
            {
                tripwireAlarmState = 1;
                if (Config.current.SayWhenTripwireCrossed)
                {
                    Console.WriteLine(Text.game.tripwire1);
                    Say(Text.game.tripwire1);
                }
            }
            if (tripwireAlarmState <= 1 && intense)
            {
                tripwireAlarmState = 2;
                if (Config.current.SayWhenTripwireCrossed)
                {
                    Console.WriteLine(Text.game.tripwire2);
                    Say(Text.game.tripwire2);
                }
            }


            if (nextTripwireAlarmMs <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                List<ToneProperties> musicTones = new List<ToneProperties>();
                if (intense)
                {
                    musicTones.Add(new ToneProperties() { leftVolume = Config.current.ZombieTripwireVolume, rightVolume = Config.current.ZombieTripwireVolume, duration = 500, startFrequency = 200, endFrequency = 200, signalType = SignalGeneratorType.Triangle, startDelay = 0 });
                    musicTones.Add(new ToneProperties() { leftVolume = Config.current.ZombieTripwireVolume, rightVolume = Config.current.ZombieTripwireVolume, duration = 500, startFrequency = 275, endFrequency = 275, signalType = SignalGeneratorType.Triangle, startDelay = 20 });
                    musicTones.Add(new ToneProperties() { leftVolume = Config.current.ZombieTripwireVolume, rightVolume = Config.current.ZombieTripwireVolume, duration = 500, startFrequency = 350, endFrequency = 350, signalType = SignalGeneratorType.Triangle, startDelay = 20 });
                }
                musicTones.Add(new ToneProperties() { leftVolume = Config.current.ZombieTripwireVolume * 0.5f, rightVolume = Config.current.ZombieTripwireVolume * 0.5f, duration = 100, startFrequency = 100, endFrequency = 100, signalType = SignalGeneratorType.Square, startDelay = 0 });
                musicTones.Add(new ToneProperties() { leftVolume = Config.current.ZombieTripwireVolume * 0.5f, rightVolume = Config.current.ZombieTripwireVolume * 0.5f, duration = 100, startFrequency = 100, endFrequency = 100, signalType = SignalGeneratorType.Square, startDelay = 200 });
                musicTones.Add(new ToneProperties() { leftVolume = Config.current.ZombieTripwireVolume * 0.5f, rightVolume = Config.current.ZombieTripwireVolume * 0.5f, duration = 100, startFrequency = 100, endFrequency = 100, signalType = SignalGeneratorType.Square, startDelay = 400 });
                PlayTones(musicTones);
                nextTripwireAlarmMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1500;
            }
        }

        static void UpdateWindowHandle(Process gameProc)
        {
            try
            {
                gameWHnd = Process.GetProcessById(gameProc.Id).MainWindowHandle; //Ensure we have the main handle (changes when entering/exiting fullscreen)
            }
            catch
            {
                //Game must be closed
                Environment.Exit(0);
            }
        }
        public static void PlaySlotTone(float slotIndex, float maxSlot, float refreshPercent = 1.0f)
        {
            float frequency = 698.46f;
            frequency = frequency / 2.0f;
            frequency = frequency + (frequency * refreshPercent);
            float rVolume = slotIndex / maxSlot;
            float lVolume = 1.0f - rVolume;

            rVolume *= Config.current.PlantSlotChangeVolume;
            lVolume *= Config.current.PlantSlotChangeVolume;
            PlayTone(lVolume, rVolume, frequency, frequency, 100, SignalGeneratorType.Sin);
        }

        static void SafeMain(string[] args)
        {
            Config.LoadConfig();
            Text.FindLanguages();
            input = new Input();    //Start input scanning thread

            Process gameProc = HookProcess();           

            GameScene prevScene = (GameScene)memIO.GetGameScene();

            if (args != null && args.Length > 0 && args[0] == "Restart")
            {
                Console.WriteLine("The mod encountered a fatal error, and had to restart");
                PlayTone(0.5f, 0.5f, 250, 250, 100, SignalGeneratorType.Square, 0);
                PlayTone(1, 1, 300, 300, 100, SignalGeneratorType.Square, 200);
                PlayTone(0.5f, 0.5f, 250, 250, 100, SignalGeneratorType.Square, 400);
                Say("PVZ A11y was restarted due to a fatal error. Press any bound keyboard or controller input to continue.", true);
                while (input.GetCurrentIntent() == InputIntent.None)
                    ;
            }

            Console.WriteLine("prevScene: " + prevScene);


            //Grab window/draw resolution (need both to calculate black bar width in fullscreen, for accurate click location)
            windowWidth = memIO.GetWindowWidth();
            windowHeight = memIO.GetWindowHeight();
            drawWidth = memIO.GetDrawWidth();
            drawHeight = memIO.GetDrawHeight();

            drawStartX = (windowWidth - drawWidth) / 2; //After first black bar, if there are any.

            UpdateWindowHandle(gameProc);

            Widget? tempWidget = GetActiveWidget(null);
            while (prevScene == GameScene.Loading)
            {
                bool loadingComplete = mem.ReadByte(memIO.ptr.lawnAppPtr + ",86c,b9") > 0;  //lawnapp,titleScreen,loadingComplete

                tempWidget = GetActiveWidget(tempWidget);
                if(tempWidget is SteamSaveChoice)
                {
                    string? text = tempWidget.GetCurrentWidgetText();
                    if(text != null)
                    {
                        Console.WriteLine(text);
                        Say(text);
                    }
                    InputIntent tempIntent = input.GetCurrentIntent();
                    if (tempIntent != InputIntent.None)
                        tempWidget.Interact(tempIntent);
                }
                else if (loadingComplete)
                    Click(0.5f, 0.5f);
                prevScene = (GameScene)memIO.GetGameScene();
                Task.Delay(100).Wait();
            }

            //DEBUG_DUMPREANIMS();

            prevScene = 0;

            //Widget currentWidget = GetActiveWidget(null);
            Widget currentWidget = new Widgets.Placeholder(memIO);

            bool printCursorPos = false;
            bool qHeld = false;

            bool playedPlantFullIndicator = false;
            int oldMsgDuration = 0;

            int prevFastZombieCount = 0;
            bool packetWasReady = false;
            int prevSeedbankSlot = -1;

            long lastSweep = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long nextFloatingPacketUpdate = 0;  //When to scan for floating seed packets (need to delay, to avoid slowing the game)
            int prevSunAmount = 0;
            List<int> prevReadyPlants = new List<int>();
            while (true)
            {
                //Ensure window/draw specs, and hwnd are accurate
                windowWidth = memIO.GetWindowWidth();
                windowHeight = memIO.GetWindowHeight();
                drawWidth = memIO.GetDrawWidth();
                drawHeight = memIO.GetDrawHeight();
                drawStartX = (windowWidth - drawWidth) / 2; //After first black bar, if there are any.

                UpdateWindowHandle(gameProc);

                if (vibrationEnd <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    XInput.SetVibration(0, 0, 0);

                GameScene gameScene = (GameScene)memIO.GetGameScene();
                prevScene = gameScene;

                currentWidget = GetActiveWidget(currentWidget);

                bool inVaseBreaker = VaseBreakerCheck();

                bool onBoard = mem.ReadUInt(memIO.ptr.boardChain) != 0;

                if (onBoard && inVaseBreaker)
                {
                    int heldPlantID = mem.ReadInt(memIO.ptr.boardChain + ",150,28");
                    if (heldPlantID != -1 && VasebreakerHeldPlantID == -1)
                    {
                        string plantStr = Text.plantNames[heldPlantID] + " in hand.";
                        Console.WriteLine(plantStr);
                        Say(plantStr, true);
                    }
                    VasebreakerHeldPlantID = heldPlantID;
                }
                else
                    VasebreakerHeldPlantID = -1;

                if (onBoard)
                {
                    int messageDuration = mem.ReadInt(memIO.ptr.boardChain + ",158,88");
                    if (oldMsgDuration == 0 && messageDuration != 0)
                    {
                        string messageStr = mem.ReadString(memIO.ptr.boardChain + ",158,4","",128, true, Program.encoding);
                        if (messageStr.StartsWith("Click-and-drag"))
                            messageStr = "Press the deny button, then a direction, to swap plants and make matches of three.";
                        if (messageStr == "No possible moves!")
                            messageStr = "Reshuffling to make possible moves.";
                        if (!messageStr.Contains("Click on the shovel"))
                        {
                            Console.WriteLine(messageStr);
                            Say(messageStr, true);
                        }
                    }
                    oldMsgDuration = messageDuration;
                }
                else
                    oldMsgDuration = 0;

                int gameMode = memIO.GetGameMode();
                bool inTree = gameMode == (int)GameMode.TreeOfWisdom && ((int)gameScene == 2 || (int)gameScene == 3);
                if (inTree)
                {
                    int newTreeDialogue = mem.ReadInt(memIO.ptr.boardChain + ",178,b8");
                    if (newTreeDialogue != CurrentTreeDialogue)
                    {
                        if (Text.TreeDialogue.ContainsKey(newTreeDialogue))
                        {
                            string dialogue = Text.TreeDialogue[newTreeDialogue];
                            Console.WriteLine(dialogue);
                            Say(dialogue, false);
                        }
                    }
                    CurrentTreeDialogue = newTreeDialogue;
                }
                else
                    CurrentTreeDialogue = -1;

                int animatingSun = 0;

                if(Config.current.AutoCollectItems)
                    animatingSun = CollectBoardStuff(currentWidget);

                int thisFastZombieCount = 0;
                bool plantPacketReady = false;
                int fastZombieRow = 0;
                float gridHeight = 1;
                int newSunAmount = 0;
                List<int>? newPlantsReady = new List<int>();
                if (currentWidget is Board)
                {
                    if(nextFloatingPacketUpdate <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    {
                        ((Board)currentWidget).UpdateFloatingSeedPackets();
                        nextFloatingPacketUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 200;   //Update once every 200ms, to avoid slowing the game too much (floating packet updates pause the board, to avoid memory relocations causing issues)
                    }

                    gridHeight = ((Board)currentWidget).gridInput.height;

                    ((Board)currentWidget).SetAnimatingSunAmount(animatingSun);

                    newSunAmount = ((Board)currentWidget).GetTotalSun();
                    if (newSunAmount > prevSunAmount && Config.current.SaySunCountOnCollect)
                    {
                        string sunString = Text.game.sunCount.Replace("[0]", FormatNumber(newSunAmount));
                        Console.WriteLine(sunString);
                        Say(sunString);
                    }

                    thisFastZombieCount = ((Board)currentWidget).GetFastZombieCount(ref fastZombieRow);
                    plantPacketReady = ((Board)currentWidget).PlantPacketReady();
                    if (((Board)currentWidget).seedbankSlot != prevSeedbankSlot)
                        packetWasReady = false;

                    newPlantsReady = ((Board)currentWidget).GetAllPlantsReady();
                    prevSeedbankSlot = ((Board)currentWidget).seedbankSlot;
                    if(Config.current.DeadZombieCueVolume > 0)
                    {
                        List<ToneProperties> tones = ((Board)currentWidget).FindDeadZombies();
                        PlayTones(tones);
                    }
                    if (Config.current.ZombieEntryVolume > 0)
                    {
                        var enteredZombies = ((Board)currentWidget).GetZombies(false, true);
                        List<ToneProperties> zombieTones = new List<ToneProperties>();
                        float boardHeight = ((Board)currentWidget).gridInput.height;
                        for(int z = 0; z < boardHeight; z++)
                        {
                            if (enteredZombies.Any(zom => zom.row == z))
                            {
                                float freq = 1000.0f - ((z * 500.0f) / boardHeight);
                                zombieTones.Add(new ToneProperties() { leftVolume = 0, rightVolume = Config.current.ZombieEntryVolume, startFrequency = freq, endFrequency = freq, duration = 100, signalType = SignalGeneratorType.Sin, startDelay = z * 200 });
                            }
                        }
                        PlayTones(zombieTones);
                    }
                    if(Config.current.ZombieSonarInterval > 0)
                    {
                        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        long timeGap = now - lastSweep;
                        bool doSweep = false;
                        if (Config.current.ZombieSonarInterval == 4 && timeGap >= 3000)
                            doSweep = true;
                        if (Config.current.ZombieSonarInterval == 3 && timeGap >= 2000)
                            doSweep = true;
                        if (Config.current.ZombieSonarInterval == 2 && timeGap >= 1000)
                            doSweep = true;
                        if (Config.current.ZombieSonarInterval == 1 && timeGap >= 500)
                            doSweep = true;

                        if (doSweep)
                        {
                            lastSweep = now;
                            var zombies = ((Board)currentWidget).GetZombies();
                            List<ToneProperties> tones = new List<ToneProperties>();
                            foreach (var zombie in zombies)
                            {
                                float rVolume = zombie.posX / 900.0f;
                                float lVolume = 1.0f - rVolume;
                                rVolume *= Config.current.AutomaticZombieSonarVolume;
                                lVolume *= Config.current.AutomaticZombieSonarVolume;
                                int startDelay = (int)(zombie.posX / 2.0f);
                                float freq = 1000.0f - ((zombie.row * 500.0f) / (float)((Board)currentWidget).gridInput.height);
                                if (startDelay > 1000 || startDelay < 0)
                                    continue;

                                if (zombie.zombieType == (int)ZombieType.DrZomBoss && (zombie.phase < 87 || zombie.phase > 89))
                                    continue;
                                tones.Add(new ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = freq, endFrequency = freq, duration = 100, signalType = SignalGeneratorType.Triangle, startDelay = startDelay });
                            }
                            PlayTones(tones);
                        }
                    }
                    if(Config.current.ZombieTripwireRow != 0 && (Config.current.ZombieTripwireVolume > 0 || Config.current.SayWhenTripwireCrossed))
                    {
                        var board = ((Board)currentWidget);
                        var zombies = board.GetZombies();
                        int alarmCount = 0;
                        foreach(var zombie in zombies)
                        {
                            if(board.GetZombieColumn(zombie.posX) < Config.current.ZombieTripwireRow)
                                alarmCount++;

                        }
                        if (alarmCount > 0)
                            PlayTripwireAlarm(alarmCount > 1);
                        else
                            tripwireAlarmState = 0;
                    }
                }
                else
                    prevSeedbankSlot = -1;

                prevSunAmount = newSunAmount;

                if(currentWidget is ZenGarden && Config.current.AutoWakeStinky)
                {
                    int timestamp = (int)CurrentEpoch();
                    int stinkyValue = memIO.GetPlayerPurchase(StoreItem.ZenStinkyTheSnail);
                    if (stinkyValue != 0 && timestamp - stinkyValue >= 180)
                    {
                        //Check if he's asleep
                        var gridItems = GetGridItems();
                        foreach(var gridItem in gridItems)
                        {
                            if(gridItem.type == (int)GridItemType.Stinky)
                            {
                                int cursorType = GetCursorType();
                                if (gridItem.state == 23 && cursorType == 0)
                                    Click((gridItem.floatX+32.0f)/800.0f, (gridItem.floatY+32.0f)/600.0f);
                                break;
                            }
                        }
                    }
                }

                if(thisFastZombieCount > prevFastZombieCount && Config.current.FastZombieCueVolume > 0)
                {
                    float freq1 = 1000.0f - ((fastZombieRow * 500.0f) / gridHeight);
                    List<ToneProperties> tones = new List<ToneProperties>();
                    tones.Add(new ToneProperties() { leftVolume = 0, rightVolume = Config.current.FastZombieCueVolume, startFrequency = freq1, endFrequency = freq1, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 0 });
                    tones.Add(new ToneProperties() { leftVolume = 0, rightVolume = Config.current.FastZombieCueVolume, startFrequency = freq1, endFrequency = freq1, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 100 });
                    tones.Add(new ToneProperties() { leftVolume = 0, rightVolume = Config.current.FastZombieCueVolume, startFrequency = freq1, endFrequency = freq1, duration = 50, signalType = SignalGeneratorType.SawTooth, startDelay = 200 });
                    PlayTones(tones);
                }

                prevFastZombieCount = thisFastZombieCount;

                if (plantPacketReady && !packetWasReady && Config.current.PlantReadyCueVolume > 0)
                {
                    List<ToneProperties> tones = new List<ToneProperties>();
                    tones.Add(new ToneProperties() { leftVolume = Config.current.PlantReadyCueVolume, rightVolume = Config.current.PlantReadyCueVolume, startFrequency = 698.46f, endFrequency = 698.46f, duration = 190, signalType = SignalGeneratorType.Sin, startDelay = 0 });
                    tones.Add(new ToneProperties() { leftVolume = Config.current.PlantReadyCueVolume, rightVolume = Config.current.PlantReadyCueVolume, startFrequency = 880, endFrequency = 880, duration = 170, signalType = SignalGeneratorType.Sin, startDelay = 20 });
                    tones.Add(new ToneProperties() { leftVolume = Config.current.PlantReadyCueVolume, rightVolume = Config.current.PlantReadyCueVolume, startFrequency = 1046.5f, endFrequency = 1046.5f, duration = 150, signalType = SignalGeneratorType.Sin, startDelay = 40 });
                    PlayTones(tones);
                }

                if (newPlantsReady.Count > 0 && Config.current.BackgroundPlantReadyCueVolume > 0)
                {
                    List<ToneProperties> tones = new List<ToneProperties>();
                    int extraDelay = 0;
                    for (int i = 0; i < newPlantsReady.Count; i++)
                    {
                        if (prevReadyPlants.Contains(newPlantsReady[i]))
                            continue;

                        //Don't play tone for current plant
                        if (((Board)currentWidget).seedbankSlot == newPlantsReady[i])
                            continue;

                        float rVolume = newPlantsReady[i] / 10.0f;
                        float lVolume = 1.0f - rVolume;
                        rVolume *= Config.current.BackgroundPlantReadyCueVolume;
                        lVolume *= Config.current.BackgroundPlantReadyCueVolume;

                        float freq1 = 200.0f + (50.0f * newPlantsReady[i]);
                        float freq2 = freq1 * 1.25f;
                        float freq3 = freq1 * 1.5f;

                        tones.Add(new ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = freq1, endFrequency = freq1, duration = 190, signalType = SignalGeneratorType.Sin, startDelay = extraDelay + 40 });
                        tones.Add(new ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = freq2, endFrequency = freq2, duration = 170, signalType = SignalGeneratorType.Sin, startDelay = extraDelay + 20 });
                        tones.Add(new ToneProperties() { leftVolume = lVolume, rightVolume = rVolume, startFrequency = freq3, endFrequency = freq3, duration = 150, signalType = SignalGeneratorType.Sin, startDelay = extraDelay });
                        extraDelay += 100;
                    }
                    PlayTones(tones);
                    prevReadyPlants = newPlantsReady;
                }
                packetWasReady = plantPacketReady;

                DoWidgetInteractions(currentWidget, input);

                string? currentWidgetText = currentWidget.GetCurrentWidgetText();
                if(currentWidgetText is not null)
                {
                    Console.WriteLine(currentWidgetText);
                    Say(currentWidgetText);
                }

                if (currentWidget is SeedPicker)
                {
                    int plantPickCount = GetSelectedPlants().Length;
                    int seedBankSize = mem.ReadInt(memIO.ptr.lawnAppPtr + ",868,15c,24");
                    if (plantPickCount == seedBankSize)
                    {
                        //Play sound indicating seedbank is full
                        //Enable play button navigation
                        //Play sound when player tries to add more seeds
                        if (!playedPlantFullIndicator)
                        {
                            PlayTone(1, 1, 400, 400, 100, SignalGeneratorType.Triangle);
                            PlayTone(1, 1, 600, 600, 100, SignalGeneratorType.Triangle,50);
                            Console.WriteLine(Text.menus.pressStart);
                            Say(Text.menus.pressStart, true);

                        }
                        playedPlantFullIndicator = true;
                    }
                    else
                        playedPlantFullIndicator = false;
                }
                
                Task.Delay(10).Wait();  //Avoid excess cpu usage
            }
        }
    }
}