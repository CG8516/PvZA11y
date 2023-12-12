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
using System.Drawing;
using SimWinInput;

/*
[PVZ-A11y Beta 1.4]

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
    Make lawnMowers/poolCleaners/roofSweepers/iZombieBrains accessible (maybe play a left-panned squareWave when using zombie sonar, if one is present in that lane.)
    Imitater support in plant picker
    Ensure feature-parity across game versions
    Level progress for all minigame/puzzle modes
    Make all minigames blind-accessible. Some will be easy (Seeing Stars), but some will take a lot of work to before they can be not just played, but enjoyed, by blind gamers.
    Fix bug that causes tutorial messages to be played more than once (also happens when resuming an in-progress game on a level with a tutorial)
    Make dropped seed packets more acccessible, for modes like vasebreaker (don't instantly grab them, maybe move them to top of screen, and cycle similarly to normal plant deck)
    Make zomboni ice trails accessible
    Allow plants to be placed in whack-a-zombie (cherry bomb, gravebuster, ice shroom)
    Calculate scaled plant-upgrade prices in 'Last Stand' minigame (plant upgrade prices increase as you use them)
        Inform player that sun plants aren't allowed on 'Last Stand' minigame
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
        Could change grid tone type (sine/square), to indicate if tile is empty or occupied.
        Could have a whole-grid zombie-sonar, rather than per-row, to get a quick/rough idea of where each zombie is.
        Could have a plant-column checker, to indicate how many empty/plantable tiles are in the current column (useful for detecting if a plant has been eaten, or if you missed a spot)
        Could add a screen reader cue when a plant is eaten (eg; "E-4 Peashooter Eaten").
    

The memory.dll library could use some enhancements, but it's workable for now.


*/

namespace PvZA11y
{
    internal class Program
    {



        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);


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
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);


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

        static string[] plantNameLookup = new string[]
        {
            "[PEASHOOTER]",
            "[SUNFLOWER]",
            "[CHERRY_BOMB]",
            "[WALL_NUT]",
            "[POTATO_MINE]",
            "[SNOW_PEA]",
            "[CHOMPER]",
            "[REPEATER]",
            "[PUFF_SHROOM]",
            "[SUN_SHROOM]",
            "[FUME_SHROOM]",
            "[GRAVE_BUSTER]",
            "[HYPNO_SHROOM]",
            "[SCAREDY_SHROOM]",
            "[ICE_SHROOM]",
            "[DOOM_SHROOM]",
            "[LILY_PAD]",
            "[SQUASH]",
            "[THREEPEATER]",
            "[TANGLE_KELP]",
            "[JALAPENO]",
            "[SPIKEWEED]",
            "[TORCHWOOD]",
            "[TALL_NUT]",
            "[SEA_SHROOM]",
            "[PLANTERN]",
            "[CACTUS]",
            "[BLOVER]",
            "[SPLIT_PEA]",
            "[STARFRUIT]",
            "[PUMPKIN]",
            "[MAGNET_SHROOM]",
            "[CABBAGE_PULT]",
            "[FLOWER_POT]",
            "[KERNEL_PULT]",
            "[COFFEE_BEAN]",
            "[GARLIC]",
            "[UMBRELLA_LEAF]",
            "[MARIGOLD]",
            "[MELON_PULT]",
            "[GATLING_PEA]",
            "[TWIN_SUNFLOWER]",
            "[GLOOM_SHROOM]",
            "[CATTAIL]",
            "[WINTER_MELON]",
            "[GOLD_MAGNET]",
            "[SPIKEROCK]",
            "[COB_CANNON]",
            "[IMITATER]",
            "[EXPLODE_O_NUT]",
            "[GIANT_WALLNUT]"
        };

        static string[] plantDescriptionLookup = new string[]
        {
            "[PEASHOOTER_TOOLTIP]",
            "[SUNFLOWER_TOOLTIP]",
            "[CHERRY_BOMB_TOOLTIP]",
            "[WALL_NUT_TOOLTIP]",
            "[POTATO_MINE_TOOLTIP]",
            "[SNOW_PEA_TOOLTIP]",
            "[CHOMPER_TOOLTIP]",
            "[REPEATER_TOOLTIP]",
            "[PUFF_SHROOM_TOOLTIP]",
            "[SUN_SHROOM_TOOLTIP]",
            "[FUME_SHROOM_TOOLTIP]",
            "[GRAVE_BUSTER_TOOLTIP]",
            "[HYPNO_SHROOM_TOOLTIP]",
            "[SCAREDY_SHROOM_TOOLTIP]",
            "[ICE_SHROOM_TOOLTIP]",
            "[DOOM_SHROOM_TOOLTIP]",
            "[LILY_PAD_TOOLTIP]",
            "[SQUASH_TOOLTIP]",
            "[THREEPEATER_TOOLTIP]",
            "[TANGLE_KELP_TOOLTIP]",
            "[JALAPENO_TOOLTIP]",
            "[SPIKEWEED_TOOLTIP]",
            "[TORCHWOOD_TOOLTIP]",
            "[TALL_NUT_TOOLTIP]",
            "[SEA_SHROOM_TOOLTIP]",
            "[PLANTERN_TOOLTIP]",
            "[CACTUS_TOOLTIP]",
            "[BLOVER_TOOLTIP]",
            "[SPLIT_PEA_TOOLTIP]",
            "[STARFRUIT_TOOLTIP]",
            "[PUMPKIN_TOOLTIP]",
            "[MAGNET_SHROOM_TOOLTIP]",
            "[CABBAGE_PULT_TOOLTIP]",
            "[FLOWER_POT_TOOLTIP]",
            "[KERNEL_PULT_TOOLTIP]",
            "[COFFEE_BEAN_TOOLTIP]",
            "[GARLIC_TOOLTIP]",
            "[UMBRELLA_LEAF_TOOLTIP]",
            "[MARIGOLD_TOOLTIP]",
            "[MELON_PULT_TOOLTIP]",
            "[GATLING_PEA_TOOLTIP]",
            "[TWIN_SUNFLOWER_TOOLTIP]",
            "[GLOOM_SHROOM_TOOLTIP]",
            "[CATTAIL_TOOLTIP]",
            "[WINTER_MELON_TOOLTIP]",
            "[GOLD_MAGNET_TOOLTIP]",
            "[SPIKEROCK_TOOLTIP]",
            "[COB_CANNON_TOOLTIP]",
            "[IMITATER_TOOLTIP]",
            "[EXPLODE_O_NUT_TOOLTIP]"
        };

        

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

        static int windowWidth;
        static int windowHeight;
        static int drawWidth;
        static int drawHeight;

        static int drawStartX;


        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);


        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x, y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public int type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        const int INPUT_MOUSE = 0;
        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;

        static void SendLeftMouseClick(POINT targetPoint)
        {
            INPUT[] inputs = new INPUT[2];

            // Move the mouse to the target point
            inputs[0].type = INPUT_MOUSE;
            inputs[0].U.mi.dx = targetPoint.x;
            inputs[0].U.mi.dy = targetPoint.y;
            inputs[0].U.mi.mouseData = 0;
            inputs[0].U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;

            // Release the left mouse button
            inputs[1].type = INPUT_MOUSE;
            inputs[1].U.mi.dx = targetPoint.x;
            inputs[1].U.mi.dy = targetPoint.y;
            inputs[1].U.mi.mouseData = 0;
            inputs[1].U.mi.dwFlags = MOUSEEVENTF_LEFTUP;

            // Send the input
            if (SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
            {
                Console.WriteLine("Failed to send input. Error code: " + Marshal.GetLastWin32Error());
            }
        }

        public static void Click(float downX, float downY, float upX, float upY)
        {
            Task.Run(() => ClickTask(downX, downY, upX, upY));
        }

        public static void MoveMouse(float x, float y)
        {
            if (!Config.current.MoveMouseCursor)
                return;

            int posX = (int)((x * drawWidth) + drawStartX);
            int posY = (int)(y * drawHeight);

            //RECT rect = new RECT();
            GetWindowRect(gameWHnd, out RECT rect);
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
            int clickX = (int)((downX * drawWidth) + drawStartX);
            int clickY = (int)(downY * drawHeight);

            int clickUpX = (int)((upX * drawWidth) + drawStartX);
            int clickUpY = (int)(upY * drawHeight);

            //if(Config.current.FocusOnInteract)
                //SetForegroundWindow(gameWHnd.ToInt32()); //Bring window to front

            PostMessage(gameWHnd, WM_LBUTTONDOWN, 1, MakeLParam(clickX, clickY));
            Task.Delay(50).Wait();
            PostMessage(gameWHnd, WM_LBUTTONUP, 0, MakeLParam(clickUpX, clickUpY));
        }

        static void ClickTask(float x, float y, bool rightClick = false, int delayTime = 50, bool moveMouse = false)
        {
            int clickX = (int)((x * drawWidth) + drawStartX);
            int clickY = (int)(y*drawHeight);

            //Console.WriteLine("ClickX: {0} ClickY: {1}", clickX, clickY);

            //mem.WriteMemory("PlantsVsZombies.exe+00329670,D9C", "byte", "1");   //Set windowFocus variable to true

            uint clickDown = rightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint clickUp = rightClick ? WM_RBUTTONUP : WM_LBUTTONUP;

            if (Config.current.FocusOnInteract)
                SetForegroundWindow(gameWHnd.ToInt32());    //Bring window to front

            //Overwrite mouse position in widgetManager
            //mem.WriteMemory(lawnAppPtr + ",320,108", "int", clickX.ToString());
            //mem.WriteMemory(lawnAppPtr + ",320,10c", "int", clickY.ToString());

            //if (moveMouse && Config.current.MoveMouseCursor)
            //{
                //RECT rect = new RECT();
                GetWindowRect(gameWHnd, out RECT rect);
                //Console.WriteLine("Window Pos: {0},{1}", rect.Left, rect.Top);
                int cursorX = rect.Left + clickX;
                int cursorY = rect.Top + clickY;
            //Cursor.Position = new System.Drawing.Point(cursorX, cursorY);

            //Move mouse before processing click
            //PostMessage(gameWHnd, 0x0200, 1, MakeLParam(clickX, clickY));

            //  Task.Delay(delayTime).Wait();
            //}

            //SimMouse.Click(MouseButtons.Left, clickX, clickY);
            //SimMouse.Act(SimMouse.Action.MoveOnly, cursorX, cursorY);
            //Console.WriteLine("New click");

            POINT targetPoint = new POINT() { x = clickX, y = clickY };
            ClientToScreen(gameWHnd, ref targetPoint);

            if (GetForegroundWindow() == gameWHnd)
            {
                Cursor.Position = new(cursorX, cursorY);
                SendLeftMouseClick(targetPoint);
            }

            //SimMouse.Click(MouseButtons.Left, cursorX, cursorY);

            //PostMessage(gameWHnd, clickDown, 1, MakeLParam(clickX, clickY));

            //Task.Delay(delayTime).Wait();

            //PostMessage(gameWHnd, clickUp, 0, MakeLParam(clickX, clickY));
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
            }

            string procDir = foundProcs[0].MainModule.FileName;
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

            //See if we can find popcapgame first
            Process[] procs2 = Process.GetProcessesByName("popcapgame1");
            if (procs2 != null && procs2.Length > 0)
            {
                gameProc = procs2[0];
                appName = appNamePopcap;
            }
            else
            {
                if (reqpopcapgame1)
                {
                    while (procs2 == null || procs2.Length < 1)
                    {
                        procs2 = Process.GetProcessesByName("popcapgame1");

                        //If plantsVsZombies.exe starts more threads, it's not a launcher. So stop searching for popcapagame1
                        foundProcs = Process.GetProcessesByName("PlantsVsZombies");
                        if (foundProcs[0].Threads.Count >= 10)
                        {
                            reqpopcapgame1 = false;
                            break;
                        }
                        Task.Delay(100).Wait();
                    }

                    if (reqpopcapgame1)
                    {
                        gameProc = procs2[0];
                        appName = appNamePopcap;
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

            string versionStr = gameProc.MainModule.FileVersionInfo.ProductVersion;

            //Steam version creates temporary/locked popcapgames1.exe, which will fail to grab version info. If that's the case, grab the verison info from PlantsVsZombies.exe instead.
            if (versionStr is null && appName == appNamePopcap && foundProcs != null && foundProcs.Length > 0)
                versionStr = foundProcs[0].MainModule.FileVersionInfo.ProductVersion;

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

            //Only gathered when GetPlantAtCell is called
            public bool hasPumpkin;
            public bool hasPot;
            public bool hasLillypad;
            public bool squished;
            public bool sleeping;
        }

        //TODO: Move to board class
        public struct GridItem
        {
            public int type;
            public int state;
            public int x;
            public int y;
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
            memIO.SetBoardPaused(true);
            Program.input.ClearIntents();

            int currentLine = 0;
            int lineCount = tutorial.Length;
            string completeTutorial = tutorial[0];
            for (int i = 1; i < lineCount; i++)
                completeTutorial += "\r\n" + tutorial[i];

            Console.WriteLine(completeTutorial);
            Program.Say(completeTutorial);

            InputIntent intent = Program.input.GetCurrentIntent();
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
                    Console.WriteLine(tutorial[currentLine]);
                    Program.Say(tutorial[currentLine]);
                }
                intent = Program.input.GetCurrentIntent();
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
            for(int i =0; i < plants.Count; i++)
            {
                if (plants[i].column != x)
                    continue;
                if (plants[i].row != y)
                    continue;

                if (plants[i].plantType == (int)SeedType.SEED_PUMPKINSHELL)
                    hasPumpkin = true;
                else if (plants[i].plantType == (int)SeedType.SEED_LILYPAD)
                    hasLillypad = true;
                else if (plants[i].plantType == (int)SeedType.SEED_FLOWERPOT)
                    hasPot = true;
                else
                {
                    plantID = plants[i].plantType;
                    state = plants[i].state;
                }

                squished |= plants[i].squished;
                sleeping |= plants[i].sleeping;
            }

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

            if (plant.hasLillypad && plant.plantType == -1)
                plant.plantType = (int)SeedType.SEED_LILYPAD;
            if (plant.hasPot && plant.plantType == -1)
                plant.plantType = (int)SeedType.SEED_FLOWERPOT;
            if (plant.hasPumpkin && plant.plantType == -1)
                plant.plantType = (int)SeedType.SEED_PUMPKINSHELL;

            return plant;
        }

        //TODO: Move to board class
        static List<PlantOnBoard> GetPlantsOnBoard()
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

                    plants.Add(p);
                }

            }

            return plants;
        }

        static void CollectBoardStuff(Widget currentWidget)
        {
            if (currentWidget is not (Board or ZenGarden))
                return;
            
            //Don't try to collect anything while paused
            if (memIO.GetBoardPaused() && currentWidget is Board)
                return;

            //Make sure we're actually on the board
            bool hasBoardPtr = mem.ReadUInt(memIO.ptr.boardChain) != 0;
            if (!hasBoardPtr)
                return;

            //And we aren't holding anything with our cursor (plant/shovel)
            //Don't care if it's whack-a-zombie, it's fine.
            int cursorType = GetCursorType();
            if (cursorType > 0 && cursorType != 7)
                return;

            //Grab all coins, sunflowers, awards
            int maxCount = mem.ReadInt(memIO.ptr.boardChain + ",100");
            //List<Vector2> clickables = new List<Vector2>();
            for(int i =0; i < maxCount; i++)
            {
                int index = i * 216;

                //Skip inactive clickables
                if (mem.ReadByte(memIO.ptr.boardChain + ",fc," + (index + 0x38).ToString("X2")) == 1)
                    continue;

                //Skip collectables we've already clicked on
                if (mem.ReadByte(memIO.ptr.boardChain + ",fc," + (index + 0x50).ToString("X2")) == 1)
                    continue;

                //Get pos, add a couple of pixels to account for rounding errors
                Vector2 pos = new Vector2();
                pos.X = (mem.ReadFloat(memIO.ptr.boardChain + ",fc," + (index + 0x24).ToString("X2"))+30.0f) / 800.0f;
                pos.Y = (mem.ReadFloat(memIO.ptr.boardChain + ",fc," + (index + 0x28).ToString("X2"))+30.0f) / 600.0f;

                //If at/above the seed picker/bank, don't click.
                if (pos.Y < 0.15f)
                    continue;

                //Wait until click goes through
                Click(pos);

                //If we're now holding a plant after clicking, don't click anything else.
                cursorType = GetCursorType();

                if(cursorType == 2)
                {
                    int plantHeldID = GetCursorPlantID();
                    if (plantHeldID == -1)
                        return;

                    string plantHoldStr = Consts.plantNames[plantHeldID] + " in hand.";
                    Console.WriteLine(plantHoldStr);
                    Say(plantHoldStr, true);
                    return;
                }

                if (cursorType > 0 && cursorType != 7)
                    return;
            }
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

            //Console.WriteLine("Read dialogID: " + dialogID);

            if(currentWidget is null)
                currentWidget = new Placeholder(memIO);


            //If in zen garden, and pause menu is not open. Use garden.
            int gameMode = memIO.GetGameMode();
            int gameScene = (int)memIO.GetGameScene();

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
            bool wallnutBowling = (memIO.GetPlayerLevel() == 5) && memIO.GetGameScene() == (int)GameScene.SeedPicker;

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

            //Zen garden intro
            if (daveMessageIndex >= 2100 && daveMessageIndex <= 2104)
                return true;

            //if(daveMessageIndex != -1)
              //  Console.WriteLine("NO DAVE OVERRIDE: " + daveMessageIndex);
            return false;
        }

        //TODO: Implement screenreader message history
        //TODO: Move all Console.WriteLine calls to here instead. Will cleanup the code significantly, and we don't want to print anything we aren't saying, anyway. (except when debugging of course)
        public static void Say(string? text, bool interrupt = true)
        {
            try
            {
                if (Config.current.screenReaderSelection is Config.ScreenReaderSelection.Auto)
                    Config.current.ScreenReader = Config.AutoScreenReader();
                if (Config.current.ScreenReader != null && text != null)
                    Config.current.ScreenReader.Speak(text, interrupt);
            }
            catch
            {
                Config.current.ScreenReader = Config.AutoScreenReader();
                Config.SaveConfig(Config.ScreenReaderSelection.Auto);

            }
        }

        public static void PlayTone(float leftVolume, float rightVolume, float startFrequency, float endFrequency, float duration, SignalGeneratorType signalType = SignalGeneratorType.Sweep, int startDelay = 0)
        {            
            leftVolume *= Config.current.AudioCueVolume;
            rightVolume *= Config.current.AudioCueVolume;
            
            var sine20Seconds = new SignalGenerator(44100, 1)
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

            var waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init(sinepause.FollowedBy(sine20Seconds.ToStereo(leftVolume, rightVolume)));
            waveOutEvent.PlaybackStopped += NewWaveOut_PlaybackStopped;
            waveOutEvent.Play();
            return;
        }

        private static void NewWaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if(sender != null && sender is WaveOutEvent waveOut)
                waveOut.Dispose();
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
            int lvl = ((playerLevel - 1) % 10);
            int unlockedSeeds = (zone - 1) * 8 + lvl;
            if (lvl+1 >= 10)
                unlockedSeeds -= 2;
            //else if (lvl == 10)
                //unlockedSeeds -= 3;
            else if (lvl >= 5)
                unlockedSeeds -= 1;
            if (unlockedSeeds > 39)
                unlockedSeeds = 39;

            return int.Min(49,unlockedSeeds);
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

        static string CleanupAlmanacDescription(string input)
        {            
            char[] outChars = new char[input.Length+10];
            bool skipChar = false;
            int addedChars = 0;
            string statStr = "{STAT}";
            string nocturnalStr = "{NOCTURNAL}";
            string aquaticStr = "{AQUATIC}";
            string keymetalStr = "{KEYMETAL}";  //Wtf is this? key metal? yucky.
            string keywordStr = "{KEYWORD}";
            bool endLineWithPeriod = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{')
                    skipChar = true;
                if (input[i] == '}')
                {
                    skipChar = false;
                    continue;
                }
                if (input[i] == '[')
                {
                    //If start of next entry, go back to before the last \r\n and stop.
                    addedChars -= 2;
                    break;
                }
                
                if (!skipChar)
                {
                    //If trailing space, replace that with '.', otherwise replace the return symbol '\r', not used by terminal anyway.
                    if (endLineWithPeriod && (input[i] == '\r' || (input[i] == '\n' && input[i-1] != '\r')))
                    {
                        endLineWithPeriod = false;
                        if (outChars[addedChars - 1] == ' ')
                        {
                            outChars[addedChars - 1] = '.';
                            outChars[addedChars] = '\r';
                            outChars[addedChars+1] = '\n';
                            addedChars++;
                        }
                        else
                        {
                            outChars[addedChars] = '.';
                            outChars[addedChars+1] = '\r';
                            outChars[addedChars+2] = '\n';
                            addedChars+=2;
                        }

                        while (input[i] == '\r' || input[i] == '\n')
                            i++;
                        i--;
                    }
                    else
                        outChars[addedChars] = input[i];

                    addedChars++;
                }
                else
                {
                    if(!endLineWithPeriod)
                    {
                        //Loops? We ain't got no loops. We don't need no loops. I DON'T HAVE TO SHOW YOU ANY STINKIN' LOOPS!
                        //This is called for pretty much every '{}' keyword in a plant/zombie almanac description.
                        //In total, across all plants/zombies, it's called maybe three hundred times? idk. You think I didn't just pull that number out of my ass?
                        //It's done at startup, and the results are cached. So performance really isn't an issue.
                        //If you want to improve startup times on a 30+ year old cpu, feel free to optimise it.
                        if ((input.Length - i >= statStr.Length && input.Substring(i, statStr.Length) == statStr)
                            || (input.Length - i >= nocturnalStr.Length && input.Substring(i, nocturnalStr.Length) == nocturnalStr)
                            || (input.Length - i >= aquaticStr.Length && input.Substring(i, aquaticStr.Length) == aquaticStr)
                            || (input.Length - i >= keymetalStr.Length && input.Substring(i, keymetalStr.Length) == keymetalStr)
                            || (input.Length - i >= keywordStr.Length && input.Substring(i, keywordStr.Length) == keywordStr)
                            )
                            endLineWithPeriod = true;
                    }
                }
            }

            return new string(outChars, 0, addedChars);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("I'm out of here");
            //nvda.StopSpeaking();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");


            //This is pretty gross, but kind of required if the app crashes while your mid-game
            try
            {
                SafeMain(args);
            }
            catch (Exception ex)
            {
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
        }

        public static Input input = null;


        static void SafeMain(string[] args)
        {
            Config.LoadConfig();
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

            try
            {
                gameWHnd = Process.GetProcessById(gameProc.Id).MainWindowHandle; //Ensure we have the main handle (changes when entering/exiting fullscreen)
            }
            catch
            {
                gameProc = HookProcess();
                gameWHnd = gameProc.MainWindowHandle;
            }



            while (prevScene == GameScene.Loading)
            {
                bool loadingComplete = mem.ReadByte(memIO.ptr.lawnAppPtr + ",86c,b9") > 0;  //lawnapp,titleScreen,loadingComplete
                if (loadingComplete)
                {
                    Click(0.5f, 0.5f);
                }
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
            while (true)
            {
                //Ensure window/draw specs, and hwnd are accurate
                windowWidth = memIO.GetWindowWidth();
                windowHeight = memIO.GetWindowHeight();
                drawWidth = memIO.GetDrawWidth();
                drawHeight = memIO.GetDrawHeight();
                drawStartX = (windowWidth - drawWidth) / 2; //After first black bar, if there are any.



                try
                {
                    gameWHnd = Process.GetProcessById(gameProc.Id).MainWindowHandle; //Ensure we have the main handle (changes when entering/exiting fullscreen)
                }
                catch
                {
                    gameProc = HookProcess();
                    gameWHnd = gameProc.MainWindowHandle;
                }


                GameScene gameScene = (GameScene)memIO.GetGameScene();
                /*
                if(gameScene != prevScene)
                    Console.WriteLine("Scene: " + gameScene);
                */
                prevScene = gameScene;

                Type oldType = currentWidget.GetType();
                currentWidget = GetActiveWidget(currentWidget);
                
                /*
                if (oldType != currentWidget.GetType())
                    Console.WriteLine("New widget type: " + currentWidget.GetType());
                */

                bool inVaseBreaker = VaseBreakerCheck(); //GetPlayerLevel() == 35 || (gameMode >= (int)GameMode.SCARY_POTTER_1 && gameMode <= (int)GameMode.SCARY_POTTER_ENDLESS);

                bool onBoard = mem.ReadUInt(memIO.ptr.boardChain) != 0;

                if (onBoard && inVaseBreaker)
                {
                    int heldPlantID = mem.ReadInt(memIO.ptr.boardChain + ",150,28");
                    if (heldPlantID != -1 && VasebreakerHeldPlantID == -1)
                    {
                        string plantStr = Consts.plantNames[heldPlantID] + " in hand.";
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
                        string messageStr = mem.ReadString(memIO.ptr.boardChain + ",158,4","",128);
                        Console.WriteLine(messageStr);
                        Say(messageStr, true);
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
                        if (Consts.TreeDialogue.ContainsKey(newTreeDialogue))
                        {
                            string dialogue = Consts.TreeDialogue[newTreeDialogue];
                            Console.WriteLine(dialogue);
                            Say(dialogue, false);
                        }
                    }
                    CurrentTreeDialogue = newTreeDialogue;
                }
                else
                    CurrentTreeDialogue = -1;
                
                if(Config.current.AutoCollectItems)
                    CollectBoardStuff(currentWidget);


                DoWidgetInteractions(currentWidget, input);

                string? currentWidgetText = currentWidget.GetCurrentWidgetText();
                if(currentWidgetText is not null)
                {
                    Console.WriteLine(currentWidgetText);
                    Say(currentWidgetText);
                    //screenreader output
                }
                //ReadCurrentWidgetText(ref currentWidget);
                //WidgetInteraction(ref currentWidget);

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
                            Console.WriteLine("Press Start To begin");
                            Say("Press Start to begin", true);

                        }
                        playedPlantFullIndicator = true;
                    }
                    else
                        playedPlantFullIndicator = false;
                }


                //Debug setting. Won't be accurate in fullscreen, or if cursor is outside of window.
                //Just used to grab positions of clickable items during development
                //Kept here, in case somebody wants to add support for other clickable items.
                /*
                bool qDown = NativeKeyboard.IsKeyDown(VIRTUALKEY.VK_Q);
                if (qDown && !qHeld)
                    printCursorPos = !printCursorPos;
                qHeld = qDown;
                if(printCursorPos)
                {
                    float mouseX = (float)mem.ReadInt(dirtyBoardPtr + ",A4,3A0,D20");
                    float mouseY = (float)mem.ReadInt(dirtyBoardPtr + ",A4,3A0,D24");
                    //float mouseX = (float)mem.ReadInt(boardPtr + ",A4,3A0,D20");
                    //float mouseY = (float)mem.ReadInt(boardPtr + ",A4,3A0,D24");
                    Console.WriteLine("{0},{1}", mouseX / 800.0f, mouseY / 600.0f);
                }
                */
                
                Task.Delay(10).Wait();
                
                
            }
        }
    }
}