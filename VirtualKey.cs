namespace PvZA11y
{
    class VIRTUALKEY
    {

        /*
    * Virtual Keys, Standard Set */
        public const uint VK_LBUTTON = 0x01;
        public const uint VK_RBUTTON = 0x02;
        public const uint VK_CANCEL = 0x03;
        public const uint VK_MBUTTON = 0x04;    /* NOT contiguous with L & RBUTTON */

        //#if(_WIN32_WINNT >= 0x0500)
        public const uint VK_XBUTTON1 = 0x05;    /* NOT contiguous with L & RBUTTON */
        public const uint VK_XBUTTON2 = 0x06;    /* NOT contiguous with L & RBUTTON */
        //#endif /* _WIN32_WINNT >= 0x0500 */

        /*
    * 0x07 : unassigned */
        public const uint VK_BACK = 0x08;
        public const uint VK_TAB = 0x09;

        /*
    * 0x0A - 0x0B : reserved */
        public const uint VK_CLEAR = 0x0C;
        public const uint VK_RETURN = 0x0D;

        public const uint VK_SHIFT = 0x10;
        public const uint VK_CONTROL = 0x11;
        public const uint VK_MENU = 0x12;
        public const uint VK_PAUSE = 0x13;
        public const uint VK_CAPITAL = 0x14;

        public const uint VK_KANA = 0x15;
        public const uint VK_HANGEUL = 0x15;  /* old name - should be here for compatibility */
        public const uint VK_HANGUL = 0x15;
        public const uint VK_JUNJA = 0x17;
        public const uint VK_FINAL = 0x18;
        public const uint VK_HANJA = 0x19;
        public const uint VK_KANJI = 0x19;

        public const uint VK_ESCAPE = 0x1B;

        public const uint VK_CONVERT = 0x1C;
        public const uint VK_NONCONVERT = 0x1D;
        public const uint VK_ACCEPT = 0x1E;
        public const uint VK_MODECHANGE = 0x1F;

        public const uint VK_SPACE = 0x20;
        public const uint VK_PRIOR = 0x21;
        public const uint VK_NEXT = 0x22;
        public const uint VK_END = 0x23;
        public const uint VK_HOME = 0x24;
        public const uint VK_LEFT = 0x25;
        public const uint VK_UP = 0x26;
        public const uint VK_RIGHT = 0x27;
        public const uint VK_DOWN = 0x28;
        public const uint VK_SELECT = 0x29;
        public const uint VK_PRINT = 0x2A;
        public const uint VK_EXECUTE = 0x2B;
        public const uint VK_SNAPSHOT = 0x2C;
        public const uint VK_INSERT = 0x2D;
        public const uint VK_DELETE = 0x2E;
        public const uint VK_HELP = 0x2F;

        /*
        public const uint VK_LWIN = 0x5B;CII '0' - '9' (0x30 - 0x39)
    * 0x40 : unassigned * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A) */
        public const uint VK_LWIN = 0x5B;
        public const uint VK_RWIN = 0x5C;
        public const uint VK_APPS = 0x5D;

        /*
    * 0x5E : reserved */
        public const uint VK_SLEEP = 0x5F;

        public const uint VK_NUMPAD0 = 0x60;
        public const uint VK_NUMPAD1 = 0x61;
        public const uint VK_NUMPAD2 = 0x62;
        public const uint VK_NUMPAD3 = 0x63;
        public const uint VK_NUMPAD4 = 0x64;
        public const uint VK_NUMPAD5 = 0x65;
        public const uint VK_NUMPAD6 = 0x66;
        public const uint VK_NUMPAD7 = 0x67;
        public const uint VK_NUMPAD8 = 0x68;
        public const uint VK_NUMPAD9 = 0x69;
        public const uint VK_MULTIPLY = 0x6A;
        public const uint VK_ADD = 0x6B;
        public const uint VK_SEPARATOR = 0x6C;
        public const uint VK_SUBTRACT = 0x6D;
        public const uint VK_DECIMAL = 0x6E;
        public const uint VK_DIVIDE = 0x6F;
        public const uint VK_F1 = 0x70;
        public const uint VK_F2 = 0x71;
        public const uint VK_F3 = 0x72;
        public const uint VK_F4 = 0x73;
        public const uint VK_F5 = 0x74;
        public const uint VK_F6 = 0x75;
        public const uint VK_F7 = 0x76;
        public const uint VK_F8 = 0x77;
        public const uint VK_F9 = 0x78;
        public const uint VK_F10 = 0x79;
        public const uint VK_F11 = 0x7A;
        public const uint VK_F12 = 0x7B;
        public const uint VK_F13 = 0x7C;
        public const uint VK_F14 = 0x7D;
        public const uint VK_F15 = 0x7E;
        public const uint VK_F16 = 0x7F;
        public const uint VK_F17 = 0x80;
        public const uint VK_F18 = 0x81;
        public const uint VK_F19 = 0x82;
        public const uint VK_F20 = 0x83;
        public const uint VK_F21 = 0x84;
        public const uint VK_F22 = 0x85;
        public const uint VK_F23 = 0x86;
        public const uint VK_F24 = 0x87;

        /*
    * 0x88 - 0x8F : unassigned */
        public const uint VK_NUMLOCK = 0x90;
        public const uint VK_SCROLL = 0x91;

        /*
    * NEC PC-9800 kbd definitions */
        public const uint VK_OEM_NEC_EQUAL = 0x92;   // '=' key on numpad

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
        public const uint VK_LSHIFT = 0xA0;
        public const uint VK_RSHIFT = 0xA1;
        public const uint VK_LCONTROL = 0xA2;
        public const uint VK_RCONTROL = 0xA3;
        public const uint VK_LMENU = 0xA4;
        public const uint VK_RMENU = 0xA5;

        //#if(_WIN32_WINNT >= 0x0500)
        public const uint VK_BROWSER_BACK = 0xA6;
        public const uint VK_BROWSER_FORWARD = 0xA7;
        public const uint VK_BROWSER_REFRESH = 0xA8;
        public const uint VK_BROWSER_STOP = 0xA9;
        public const uint VK_BROWSER_SEARCH = 0xAA;
        public const uint VK_BROWSER_FAVORITES = 0xAB;
        public const uint VK_BROWSER_HOME = 0xAC;

        public const uint VK_VOLUME_MUTE = 0xAD;
        public const uint VK_VOLUME_DOWN = 0xAE;
        public const uint VK_VOLUME_UP = 0xAF;
        public const uint VK_MEDIA_NEXT_TRACK = 0xB0;
        public const uint VK_MEDIA_PREV_TRACK = 0xB1;
        public const uint VK_MEDIA_STOP = 0xB2;
        public const uint VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const uint VK_LAUNCH_MAIL = 0xB4;
        public const uint VK_LAUNCH_MEDIA_SELECT = 0xB5;
        public const uint VK_LAUNCH_APP1 = 0xB6;
        public const uint VK_LAUNCH_APP2 = 0xB7;

        //#endif /* _WIN32_WINNT >= 0x0500 */

        /*
    * 0xB8 - 0xB9 : reserved */
        public const uint VK_OEM_1 = 0xBA;   // ';:' for US
        public const uint VK_OEM_PLUS = 0xBB;   // '+' any country
        public const uint VK_OEM_COMMA = 0xBC;   // ',' any country
        public const uint VK_OEM_MINUS = 0xBD;   // '-' any country
        public const uint VK_OEM_PERIOD = 0xBE;   // '.' any country
        public const uint VK_OEM_2 = 0xBF;   // '/?' for US
        public const uint VK_OEM_3 = 0xC0;   // '`~' for US

        /*
    * 0xC1 - 0xD7 : reserved */
        /*
    * 0xD8 - 0xDA : unassigned */
        public const uint VK_OEM_4 = 0xDB;  //  '[{' for US
        public const uint VK_OEM_5 = 0xDC;  //  '\|' for US
        public const uint VK_OEM_6 = 0xDD;  //  ']}' for US
        public const uint VK_OEM_7 = 0xDE;  //  ''"' for US
        public const uint VK_OEM_8 = 0xDF;

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
        public static int VK_0 = 0x30;
        public static int VK_1 = 0x31;
        public static int VK_2 = 0x32;
        public static int VK_3 = 0x33;
        public static int VK_4 = 0x34;
        public static int VK_5 = 0x35;
        public static int VK_6 = 0x36;
        public static int VK_7 = 0x37;
        public static int VK_8 = 0x38;
        public static int VK_9 = 0x39;
        /* 0x40 : unassigned*/
        public static int VK_A = 0x41;
        public static int VK_B = 0x42;
        public static int VK_C = 0x43;
        public static int VK_D = 0x44;
        public static int VK_E = 0x45;
        public static int VK_F = 0x46;
        public static int VK_G = 0x47;
        public static int VK_H = 0x48;
        public static int VK_I = 0x49;
        public static int VK_J = 0x4A;
        public static int VK_K = 0x4B;
        public static int VK_L = 0x4C;
        public static int VK_M = 0x4D;
        public static int VK_N = 0x4E;
        public static int VK_O = 0x4F;
        public static int VK_P = 0x50;
        public static int VK_Q = 0x51;
        public static int VK_R = 0x52;
        public static int VK_S = 0x53;
        public static int VK_T = 0x54;
        public static int VK_U = 0x55;
        public static int VK_V = 0x56;
        public static int VK_W = 0x57;
        public static int VK_X = 0x58;
        public static int VK_Y = 0x59;
        public static int VK_Z = 0x5A;
    }
}
