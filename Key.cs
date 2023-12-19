namespace PvZA11y
{
    class Key
    {

        /*
    * Virtual Keys, Standard Set */
        public const uint LButton = 0x01;
        public const uint RButton = 0x02;
        public const uint Cancle = 0x03;
        public const uint MButton = 0x04;    /* NOT contiguous with L & RBUTTON */

        //#if(_WIN32_WINNT >= 0x0500)
        public const uint XButton1 = 0x05;    /* NOT contiguous with L & RBUTTON */
        public const uint XButton2 = 0x06;    /* NOT contiguous with L & RBUTTON */
        //#endif /* _WIN32_WINNT >= 0x0500 */

        /*
    * 0x07 : unassigned */
        public const uint Back = 0x08;
        public const uint Tab = 0x09;

        /*
    * 0x0A - 0x0B : reserved */
        public const uint Clear = 0x0C;
        public const uint Return = 0x0D;

        public const uint Shift = 0x10;
        public const uint Control = 0x11;
        public const uint Menu = 0x12;
        public const uint Pause = 0x13;
        public const uint Capital = 0x14;

        public const uint Kana = 0x15;
        public const uint Hangul = 0x15;
        public const uint Junja = 0x17;
        public const uint Final = 0x18;
        public const uint Hanja = 0x19;
        public const uint Kanji = 0x19;

        public const uint Escape = 0x1B;

        public const uint Convert = 0x1C;
        public const uint NonConvert = 0x1D;
        public const uint Accept = 0x1E;
        public const uint ModeChange = 0x1F;

        public const uint Space = 0x20;
        public const uint Prior = 0x21;
        public const uint Next = 0x22;
        public const uint End = 0x23;
        public const uint Home = 0x24;
        public const uint Left = 0x25;
        public const uint Up = 0x26;
        public const uint Right = 0x27;
        public const uint Down = 0x28;
        public const uint Select = 0x29;
        public const uint Print = 0x2A;
        public const uint Execute = 0x2B;
        public const uint Snapshot = 0x2C;
        public const uint Insert = 0x2D;
        public const uint Delete = 0x2E;
        public const uint Help = 0x2F;

        /*
        public const uint VK_LWIN = 0x5B;CII '0' - '9' (0x30 - 0x39)
    * 0x40 : unassigned * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A) */
        public const uint LWin = 0x5B;
        public const uint RWin = 0x5C;
        public const uint Apps = 0x5D;

        /*
    * 0x5E : reserved */
        public const uint Sleep = 0x5F;

        public const uint Numpad0 = 0x60;
        public const uint Numpad1 = 0x61;
        public const uint Numpad2 = 0x62;
        public const uint Numpad3 = 0x63;
        public const uint Numpad4 = 0x64;
        public const uint Numpad5 = 0x65;
        public const uint Numpad6 = 0x66;
        public const uint Numpad7 = 0x67;
        public const uint Numpad8 = 0x68;
        public const uint Numpad9 = 0x69;
        public const uint NumpadMultiply = 0x6A;
        public const uint NumpadAdd = 0x6B;
        public const uint Separator = 0x6C;
        public const uint NumpadSubtract = 0x6D;
        public const uint NumpadDecimal = 0x6E;
        public const uint NumpadDivide = 0x6F;
        public const uint F1 = 0x70;
        public const uint F2 = 0x71;
        public const uint F3 = 0x72;
        public const uint F4 = 0x73;
        public const uint F5 = 0x74;
        public const uint F6 = 0x75;
        public const uint F7 = 0x76;
        public const uint F8 = 0x77;
        public const uint F9 = 0x78;
        public const uint F10 = 0x79;
        public const uint F11 = 0x7A;
        public const uint F12 = 0x7B;
        public const uint F13 = 0x7C;
        public const uint F14 = 0x7D;
        public const uint F15 = 0x7E;
        public const uint F16 = 0x7F;
        public const uint F17 = 0x80;
        public const uint F18 = 0x81;
        public const uint F19 = 0x82;
        public const uint F20 = 0x83;
        public const uint F21 = 0x84;
        public const uint F22 = 0x85;
        public const uint F23 = 0x86;
        public const uint F24 = 0x87;

        /*
    * 0x88 - 0x8F : unassigned */
        public const uint NumLock = 0x90;
        public const uint Scroll = 0x91;

        /*
    * NEC PC-9800 kbd definitions */
        public const uint NumpadEqual = 0x92;   // '=' key on numpad

        /*
    * Fujitsu/OASYS kbd definitions */
        public const uint VK_OEM_FJ_JISHO = 0x92;   // 'Dictionary' key
        public const uint VK_OEM_FJ_MASSHOU = 0x93;   // 'Unregister word' key
        public const uint VK_OEM_FJ_TOUROKU = 0x94;   // 'Register word' key
        public const uint VK_OEM_FJ_LOYA = 0x95;   // 'Left OYAYUBI' key
        public const uint VK_OEM_FJ_ROYA = 0x96;   // 'Right OYAYUBI' key

        /*
    * 0x97 - 0x9F : unassigned */
        /*
    * VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys. * Used only as parameters to GetAsyncKeyState() and GetKeyState(). * No other API or message will distinguish left and right keys in this way. */
        public const uint LeftShift = 0xA0;
        public const uint RightShift = 0xA1;
        public const uint LeftControl = 0xA2;
        public const uint RightControl = 0xA3;
        public const uint LeftMenu = 0xA4;
        public const uint RightMneu = 0xA5;

        //#if(_WIN32_WINNT >= 0x0500)
        public const uint BrowserBack = 0xA6;
        public const uint BrowserForward = 0xA7;
        public const uint BrowserRefresh = 0xA8;
        public const uint BrowserStop = 0xA9;
        public const uint BrowserSearch = 0xAA;
        public const uint BrowserFavorites = 0xAB;
        public const uint BrowserHome = 0xAC;

        public const uint VolumeMute = 0xAD;
        public const uint VolumeDown = 0xAE;
        public const uint VolumeUp = 0xAF;
        public const uint MediaNextTrack = 0xB0;
        public const uint MediaPrevTrack = 0xB1;
        public const uint MediaStop = 0xB2;
        public const uint MediaPlayPause = 0xB3;
        public const uint LaunchMail = 0xB4;
        public const uint LaunchMediaSelect = 0xB5;
        public const uint LaunchApp1 = 0xB6;
        public const uint LaunchApp2 = 0xB7;

        //#endif /* _WIN32_WINNT >= 0x0500 */

        /*
    * 0xB8 - 0xB9 : reserved */
        public const uint Colon = 0xBA;   // ';:' for US
        public const uint Plus = 0xBB;   // '+' any country
        public const uint Comma = 0xBC;   // ',' any country
        public const uint Minus = 0xBD;   // '-' any country
        public const uint Period = 0xBE;   // '.' any country
        public const uint Question = 0xBF;   // '/?' for US
        public const uint Tilde = 0xC0;   // '`~' for US

        /*
    * 0xC1 - 0xD7 : reserved */
        /*
    * 0xD8 - 0xDA : unassigned */
        public const uint LeftSquareBracket = 0xDB;  //  '[{' for US
        public const uint BackSlash = 0xDC;  //  '\|' for US
        public const uint RightSquareBracket = 0xDD;  //  ']}' for US
        public const uint Apostrophe = 0xDE;  //  ''"' for US
        public const uint Oem8 = 0xDF;

        /*
    * 0xE0 : reserved */
        /*
    * Various extended or enhanced keyboards */
        public const uint VK_OEM_AX = 0xE1;  //  'AX' key on Japanese AX kbd
        public const uint VK_OEM_102 = 0xE2;  //  "<>" or "\|" on RT 102-key kbd.
        public const uint VK_ICO_HELP = 0xE3;  //  Help key on ICO
        public const uint VK_ICO_00 = 0xE4;  //  00 key on ICO

        //#if(WINVER >= 0x0400)
        public const uint VK_PROCESSKEY = 0xE5;
        //#endif /* WINVER >= 0x0400 */

        public const uint VK_ICO_CLEAR = 0xE6;
        //#if(_WIN32_WINNT >= 0x0500)
        public const uint VK_PACKET = 0xE7;
        //#endif /* _WIN32_WINNT >= 0x0500 */

        /*
    * 0xE8 : unassigned */
        /*
    * Nokia/Ericsson definitions */
        public const uint VK_OEM_RESET = 0xE9;
        public const uint VK_OEM_JUMP = 0xEA;
        public const uint VK_OEM_PA1 = 0xEB;
        public const uint VK_OEM_PA2 = 0xEC;
        public const uint VK_OEM_PA3 = 0xED;
        public const uint VK_OEM_WSCTRL = 0xEE;
        public const uint VK_OEM_CUSEL = 0xEF;
        public const uint VK_OEM_ATTN = 0xF0;
        public const uint VK_OEM_FINISH = 0xF1;
        public const uint VK_OEM_COPY = 0xF2;
        public const uint VK_OEM_AUTO = 0xF3;
        public const uint VK_OEM_ENLW = 0xF4;
        public const uint VK_OEM_BACKTAB = 0xF5;

        public const uint VK_ATTN = 0xF6;
        public const uint VK_CRSEL = 0xF7;
        public const uint VK_EXSEL = 0xF8;
        public const uint VK_EREOF = 0xF9;
        public const uint VK_PLAY = 0xFA;
        public const uint VK_ZOOM = 0xFB;
        public const uint VK_NONAME = 0xFC;
        public const uint VK_PA1 = 0xFD;
        public const uint VK_OEM_CLEAR = 0xFE;

        /*
    * 0xFF : reserved */
        /* missing letters and numbers for convenience*/
        public static int Zero = 0x30;
        public static int One = 0x31;
        public static int Two = 0x32;
        public static int Three = 0x33;
        public static int Four = 0x34;
        public static int Five = 0x35;
        public static int Six = 0x36;
        public static int Seven = 0x37;
        public static int Eight = 0x38;
        public static int Nine = 0x39;
        /* 0x40 : unassigned*/
        public static int A = 0x41;
        public static int B = 0x42;
        public static int C = 0x43;
        public static int D = 0x44;
        public static int E = 0x45;
        public static int F = 0x46;
        public static int G = 0x47;
        public static int H = 0x48;
        public static int I = 0x49;
        public static int J = 0x4A;
        public static int K = 0x4B;
        public static int L = 0x4C;
        public static int M = 0x4D;
        public static int N = 0x4E;
        public static int O = 0x4F;
        public static int P = 0x50;
        public static int Q = 0x51;
        public static int R = 0x52;
        public static int S = 0x53;
        public static int T = 0x54;
        public static int U = 0x55;
        public static int V = 0x56;
        public static int W = 0x57;
        public static int X = 0x58;
        public static int Y = 0x59;
        public static int Z = 0x5A;
    }
}
