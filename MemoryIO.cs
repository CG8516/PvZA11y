using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Memory;
using PvZA11y.Widgets;

namespace PvZA11y
{
    internal class MemoryIO
    {
        public Mem mem;
        public PointerInfo ptr;

        //Apply a few patches to ignore the minimal keyboard support built into the game
        //For example, some dialogues will close when escape is pressed.
        //Or the game will pause when space or escape is pressed.
        //If we want the user to be able to map their own keys, we need to skip the original keyboard input code.
        private void ApplyKeyboardPatches()
        {
            //Overwrite instruction which handles keyboard input for some dialogues (y/n for yes/no dialogues, escape, space)
            long codeAddr = mem.AoBScan(ptr.keyboardInputDisable1).Result.FirstOrDefault();
            if (codeAddr == 0)
            {
                if (mem.AoBScan(ptr.keyboardInputDisable1Patched).Result.FirstOrDefault() != 0)
                    Console.WriteLine("'keyboardInputDisable1' already patched!");
                else
                    Console.WriteLine("Failed to find 'keyboardInputDisable1' code!");
            }
            codeAddr += 7;
            mem.WriteMemory(codeAddr.ToString("X2"), "byte", "EB"); //Replace je with jmp, so widget keyboard handler always ignores keyboard

            //Overwrite instructions which read keyboard input in options menu (space/escape/enter to close dialog)
            long codeAddr2 = mem.AoBScan(ptr.keyboardInputDisable2).Result.FirstOrDefault();
            if (codeAddr2 == 0)
            {
                if (mem.AoBScan(ptr.keyboardInputDisable2Patched).Result.FirstOrDefault() != 0)
                    Console.WriteLine("'keyboardInputDisable2' already patched!");
                else
                    Console.WriteLine("Failed to find 'keyboardInputDisable2' code!");
            }
            //"74 07 8b ?? e8 ???????? 83 ?? 20 74 ?? 83 ?? 0d 74 ?? 83 ?? 1b 75 ??";
            //replace je with two nops
            codeAddr2 += 12;
            mem.WriteMemory(codeAddr2.ToString("X2"), "bytes", "90 90");

            //replace second je with two nops
            codeAddr2 += 5;
            mem.WriteMemory(codeAddr2.ToString("X2"), "bytes", "90 90");

            //replace jne with jmp
            codeAddr2 += 5;
            mem.WriteMemory(codeAddr2.ToString("X2"), "byte", "eb");

            //Overwrite instructions which read keyboard input on board (space/escape for pause/options)
            long codeAddr3 = mem.AoBScan(ptr.keyboardInputDisable3).Result.FirstOrDefault();
            if (codeAddr3 == 0)
            {
                if (mem.AoBScan(ptr.keyboardInputDisable3Patched).Result.FirstOrDefault() != 0)
                    Console.WriteLine("'keyboardInputDisable3' already patched!");
                else
                    Console.WriteLine("Failed to find 'keyboardInputDisable3' code!");
            }
            //Replace (cmp dword ptr [ecx+0000091C],02) (check if gameScene is Into/cutscene/seedpicker), with "pop edi; pop esi; ret 0004;" (return)
            mem.WriteMemory(codeAddr3.ToString("X2"), "bytes", "5f 5e c2 04 00 90 90");

            long codeAddr4 = mem.AoBScan(ptr.musicPausePatch).Result.FirstOrDefault();
            if(codeAddr4 == 0)
            {
                if (mem.AoBScan(ptr.musicPausePatched).Result.FirstOrDefault() != 0)
                    Console.WriteLine("Music sync patch already applied!");
                else
                    Console.WriteLine("Failed to find music sync code!");
            }
            mem.WriteMemory(codeAddr4.ToString("X2"), "bytes", "90 90");


            long codeAddr5 = mem.AoBScan(ptr.keyboardInputDisable4).Result.FirstOrDefault();
            if(codeAddr5 == 0)
            {
                if(mem.AoBScan(ptr.keyboardInputDisable4Patched).Result.FirstOrDefault() != 0)
                    Console.WriteLine("'keyboardInputDisable4' already patched!");
                else
                    Console.WriteLine("Failed to find 'keyboardInputDisable4' code!");
            }
            mem.WriteMemory(codeAddr5.ToString("X2"), "bytes", "c2 04 00");

            Console.WriteLine("Finished patching");
        }

        public MemoryIO(string appName, ulong gameVersion, Mem mem)
        {
            this.mem = mem;

            switch(gameVersion)
            {
                case 1201073:
                    ptr = Pointers._1_2_0_1073(appName);
                    break;
                case 1201096:
                    ptr = Pointers._1_2_0_1096(appName);
                    break;
                default:
                    Console.WriteLine("Unsupported game version!");
                    Console.WriteLine("Press enter to quit!");
                    Console.ReadLine();
                    Environment.Exit(1);
                    break;
            }

            ApplyKeyboardPatches();

        }

        public int GetPlayerLevel()
        {
            return mem.ReadInt(ptr.playerLevelChain);   
        }

        public int GetGameScene()
        {
            return mem.ReadInt(ptr.gameSceneChain);
        }

        public int GetGameMode()
        {
            return mem.ReadInt(ptr.gameModeChain);
        }

        public int GetAwardType()
        {
            return mem.ReadInt(ptr.awardTypeChain);
        }

        public int GetAdventureCompletions()
        {
            return mem.ReadInt(ptr.playerAdventureCompletionsChain);
        }

        public int GetPlayerCoinCount()
        {
            return mem.ReadInt(ptr.playerCoinsChain);
        }

        public void SetPlayerCoinCount(int value)
        {
            mem.WriteMemory(ptr.playerCoinsChain, "int", value.ToString());
        }

        public bool GetMinigamesUnlocked()
        {
            return mem.ReadInt(ptr.playerMinigamesUnlockedChain) > 0;
        }

        public bool GetPuzzleUnlocked()
        {
            return mem.ReadInt(ptr.playerPuzzleUnlockedChain) > 0;
        }

        public bool GetSurvivalUnlocked()
        {
            return mem.ReadInt(ptr.playerSurvivalUnlockedChain) > 0;
        }

        public bool GetBoardPaused()
        {
            return mem.ReadInt(ptr.boardPausedChain) > 0;
        }

        public void SetBoardPaused(bool paused)
        {
            mem.WriteMemory(ptr.boardPausedChain, "int", paused ? "1" : "0");
        }

        public Span<int> GetPlayerPurchases()
        {
            //playerPurchasesChain
            byte[] purchaseBytes = mem.ReadBytes(ptr.playerInfoChain + "," + ptr.playerPurchaseOffsetInt.ToString("X2"), 320);
            //int[] purchases = new int[80];

            return MemoryMarshal.Cast<byte,int>(purchaseBytes);
        }


        public int GetPlayerPurchase(StoreItem item)
        {
            return GetPlayerPurchase((int)item);
        }

        public int GetPlayerPlantCount()
        {
            return mem.ReadInt(ptr.playerInfoChain + ptr.zenPlantCountOffset);
        }

        public int GetPlayerPurchase(int itemID)
        {
            int purchaseOffset = itemID * 4;
            return mem.ReadInt(ptr.playerInfoChain + "," + (ptr.playerPurchaseOffsetInt + purchaseOffset).ToString("X2"));
        }

        public int GetChallengeScore(int challengeID)
        {
            int challengeOffset = challengeID * 4;
            
            return mem.ReadInt(ptr.playerInfoChain + "," + (ptr.playerChallengeScoreOffsetInt + challengeOffset).ToString("X2"));
        }

        public void SetPlayerPurchase(int itemID, int value)
        {
            int purchaseOffset = itemID * 4;
            mem.WriteMemory(ptr.playerInfoChain + "," + (ptr.playerPurchaseOffsetInt + purchaseOffset).ToString("X2"), "int",value.ToString());
        }

        public Vector2 GetWidgetPos(string ptrChain)
        {
            float posX = mem.ReadInt(ptrChain + ptr.widgetPosXOffset);
            float posY = mem.ReadInt(ptrChain + ptr.widgetPosYOffset);
            return new Vector2(posX, posY);
        }

        public Vector2 GetWidgetSize(string ptrChain)
        {
            float width = mem.ReadInt(ptrChain + ptr.widgetWidthOffset);
            float height = mem.ReadInt(ptrChain + ptr.widgetHeightOffset);
            return new Vector2(width, height);
        }

        public int GetCreditsState()
        {
            return mem.ReadInt(ptr.creditsStageChain);
        }

        public int GetZenGardenPage()
        {
            int gameMode = GetGameMode();
            if (gameMode == (int)GameMode.TreeOfWisdom)
                return 4;
            return mem.ReadInt(ptr.zenGardenPageChain);
        }

        public int GetDaveMessageID()
        {
            return mem.ReadInt(ptr.daveMessageIDChain);
        }

        public int GetDaveMessageLength()
        {
            return mem.ReadInt(ptr.daveMessageLenChain);
        }

        public string? GetDaveMessageText()
        {
            int length = GetDaveMessageLength();
            if (length == 0)
                return null;
            return mem.ReadString(ptr.daveMessageTextChain, "", length, true, Program.encoding);
        }

        public bool WidgetHasButton2(string ptrChain)
        {
            return mem.ReadUInt(ptrChain + ptr.dialogueWidgetButton2Offset) != 0;
        }

        public Vector2 GetWidgetButton1Pos(string ptrChain)
        {
            float posX = mem.ReadInt(ptrChain + ptr.dialogueWidgetButton1Offset + ptr.widgetPosXOffset);
            float posY = mem.ReadInt(ptrChain + ptr.dialogueWidgetButton1Offset + ptr.widgetPosYOffset);
            return new Vector2(posX, posY);
        }

        public string GetWidgetButton1String(string ptrChain)
        {
            if(ptr.buttonTextAlwaysPtr)
              return mem.ReadString(ptrChain + ptr.dialogueWidgetButton1Offset + ptr.widgetDialogStringOffset + ",0", "", 64, true, Program.encoding);
            
            return mem.ReadString(ptrChain + ptr.dialogueWidgetButton1Offset + ptr.widgetDialogStringOffset, "", 64, true, Program.encoding);
        }

        public Vector2 GetWidgetButton2Pos(string ptrChain)
        {
            float posX = mem.ReadInt(ptrChain + ptr.dialogueWidgetButton2Offset + ptr.widgetPosXOffset);
            float posY = mem.ReadInt(ptrChain + ptr.dialogueWidgetButton2Offset + ptr.widgetPosYOffset);
            return new Vector2(posX, posY);
        }

        public string GetWidgetButton2String(string ptrChain)
        {
            if (ptr.buttonTextAlwaysPtr)
                return mem.ReadString(ptrChain + ptr.dialogueWidgetButton2Offset + ptr.widgetDialogStringOffset + ",0", "", 64, true, Program.encoding);

            return mem.ReadString(ptrChain + ptr.dialogueWidgetButton2Offset + ptr.widgetDialogStringOffset, "", 64, true, Program.encoding);
        }

        public int GetUserCountFromProfileMgr()
        {
            return mem.ReadInt(ptr.lawnAppPtr + ",948,20");
        }

        public int GetUserCountFromPicker(string ptrChainToWidget)
        {
            return mem.ReadInt(ptrChainToWidget + ptr.usernamePickerCountOffset);
        }

        //TODO: Verify this works on every game version. Move pointers to Pointers class if not.
        //Should probably move to the pointers class regardless
        public string[] GetUserNamesFromPicker(string ptrChainToWidget)
        {
            int userCount = GetUserCountFromPicker(ptrChainToWidget);

            //Make sure we read "(Create a New User)" entry
            if (userCount <= 7)
                userCount++;

            int userEntrySize = 28;
            string[] userNames = new string[userCount];
            for (int i = 0; i < userCount; i++)
            {                
                int nameLength = mem.ReadInt(ptrChainToWidget + ptr.usernamePickerNamesOffset + ",c0," + ((i * userEntrySize) + 20).ToString("X2"));

                //If name is over 15 characters, name will be a pointer to the name. Otherwise name will be embedded directly in struct.
                if(nameLength > 15)
                    userNames[i] = mem.ReadString(ptrChainToWidget + ptr.usernamePickerNamesOffset + ",c0," + ((i * userEntrySize) + 4).ToString("X2") + ",0", "", nameLength, true, Program.encoding);
                else
                    userNames[i] = mem.ReadString(ptrChainToWidget + ptr.usernamePickerNamesOffset + ",c0," + ((i * userEntrySize) + 4).ToString("X2"), "", nameLength, true, Program.encoding);
            }

            return userNames;
        }


        public ListItem? TryGetMinigameButton(int gameID)
        {
            int index = 0xb8 + (gameID * 4);
            string buttonChain = ptr.lawnAppPtr + ptr.minigameSelectorOffset + index.ToString("X2");    //[lawnapp,minigameSelector,minigames(gameID)]

            bool isVisible = mem.ReadByte(buttonChain + ptr.minigameIsVisibleOffset) == 1;
            bool isLocked = mem.ReadByte(buttonChain + ptr.minigameIsLockedOffset) == 1;

            if (!isVisible || isLocked)
                return null;

            float posX = mem.ReadInt(buttonChain + ptr.widgetPosXOffset);
            float posY = mem.ReadInt(buttonChain + ptr.widgetPosYOffset);
            Vector2 relPos = new Vector2(posX / 800.0f, posY / 600.0f);

            return new ListItem() { text = "", relativePos = relPos };
        }

        public int GetAlmanacPage(string widgetPtrChain)
        {
            return mem.ReadInt(widgetPtrChain + ptr.almanacPageOffset);
        }

        public float GetSliderPercentage(string widgetPtrChain)
        {
            return (float)mem.ReadDouble(widgetPtrChain + ptr.sliderPercentageOffset);
        }


        public LevelType GetLevelType()
        {
            return (LevelType)mem.ReadInt(ptr.boardChain + ",5564"); //TODO: Move pointer offset to pointers.cs
        }

        public int GetWindowWidth()
        {
            return mem.ReadInt(ptr.lawnAppPtr + ",3a0,a0"); ;
        }
        public int GetWindowHeight()
        {
            return mem.ReadInt(ptr.lawnAppPtr + ",3a0,a4"); ;
        }

        public int GetDrawWidth()
        {
            return mem.ReadInt(ptr.lawnAppPtr + ",3a0,b8"); ;
        }
        public int GetDrawHeight()
        {
            return mem.ReadInt(ptr.lawnAppPtr + ",3a0,bc"); ;
        }

        public bool GetWindowed()
        {
            return mem.ReadByte(ptr.lawnAppPtr + ",3a0,ce4") == 1;
        }
    }
}
