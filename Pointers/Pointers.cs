using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

//  I'm not super stoked on this whole PointerInfo/MemoryIO implementation.
//  Ideal solution would probably use integers, rather than strings.
//  But the memory.dll library is very string-based, so that's what I catered it for.
//  Comments indicate exactly which part of the pointerchain each string should be.
//      Make sure commas are included, if text within [] has any.
//
//PointerInfo should probably be an abstract class, requiring overrides for the values.


namespace PvZA11y
{
    class PointerInfo
    {

        //Defaults for 1.2.0.1073
        //Vtable pointer for different widget types, used to hackily detect the type of widget in some cases
        //I'd really like to move away from this completely, if possible
        public class WidgetType
        {
            public uint MainMenu = 7236624;
            public uint Board = 7228728;
            public uint SeedPicker = 7278824;
            public uint SimpleDialogue = 7248640; //'Game Paused' dialogue, 'Restart Level?' prompt, 'Leave Game?' prompt
            public uint UserName = 7251024; //Prompt to set/rename user
            public uint CrazyDave = 7280200;    //Can be any number. Happens to be the right one for 1073, but we overwrite it anyway, so it doesn't matter. Just needs to be different to the other WidgetType id's
        }

        public WidgetType widgetType = new WidgetType();

        public string appName;                          //popcapgame1.exe or PlantsVsZombies.exe

        public string lawnAppPtr;                       //[lawnapp]
        public string dirtyBoardPtr;                    //yucky TODO: remove this

        public string boardPtrOffset;                   //lawnapp[,board]
        public string boardPausedOffset;                //lawnapp,board[,paused]
        public string playerInfoOffset;                 //lawnApp[,playerInfo]
        public string playerLevelOffset;                //lawnApp,playerInfo[,level]
        public string playerCoinsOffset;                //lawnApp,playerInfo[,coins]
        public string playerAdventureCompletionsOffset; //lawnApp,playerInfo[,AdventureCompletions]
        //public string playerChallengeScoresOffset = ",0x54";  //lawnApp,playerInfo[,challengeScores]
        public int playerChallengeScoreOffsetInt = 0x54;
        //public string playerPurchasesOffset;            //lawnApp,playerInfo[,purchases]
        public int playerPurchaseOffsetInt = 0x1e8;
        public string playerMinigamesUnlockedOffset;    //lawnApp,playerInfo[,minigamesUnlocked]
        public string playerPuzzleUnlockedOffset;       //lawnApp,playerInfo[,puzzleUnlocked]
        public string playerSurvivalUnlockedOffset;     //lawnApp,playerInfo[,survivalUnlocked]
        public string gameSceneOffset;                  //lawnapp[,gameScene]
        public string gameModeOffset;                   //lawnapp[,gameMode]
        public string awardScreenOffset;                //lawnapp[,awardScreen]
        public string awardTypeOffset;                  //lawnapp,awardScreen[,awardType]

        public string dialogIDOffset = ",154";          //widgetPtr[,dialogID]

        //Don't seem to change between versions, but I haven't checked every version yet.
        public string creditsScreenOffset = ",87c";              //lawnApp[,creditsScene]
        public string creditsStateOffset = ",ac";               //lawnApp,creditsScreen[,state]

        public string zenGardenOffset = ",93c";         //lawnapp[,zenGarden]
        public string zenGardenPageOffset = ",8";       //lawnapp,zenGarden[,ZenPage]

        public string widgetPosXOffset = ",40";         //widget[,posX]
        public string widgetPosYOffset = ",44";         //widget[,posY]
        public string widgetWidthOffset = ",48";        //widget[,width]
        public string widgetHeightOffset = ",4c";       //widget[,height]
        public string widgetDialogStringOffset = ",a8"; //buttonWidget[,string]
        public string sliderPercentageOffset = ",a8";   //Same as above, but is a double representing how far a slider is (0 is left, 1 = right)
        public string dialogueWidgetButton1Offset = ",178"; //dialogueWidget[,button1]
        public string dialogueWidgetButton2Offset = ",17c"; //dialogueWidget[,button2]
        public bool buttonTextAlwaysPtr = false;    //Whether the strings for button text is always a pointer, rather than sometimes being inline
        public string dialogTitleLenOffset = ",cc";
        public string dialogTitleStrOffset = ",bc";
        public string dialogBodyLenOffset = ",104";
        public string dialogBodyStrOffset = ",f4";

        public string userPickerRenameOffset = ",190";
        public string userPickerDeleteOffset = ",194";

        public string optionsMenuContinueOffset = ",190"; //optionsMenu[,returnButton]
        public string optionsMenuRestartOffset = ",18c"; //optionsMenu[,RestartLevelButton]
        public string optionsMenuReturnToMainOffset = ",188"; //optionsMenu[,MainMenuButton]
        public string optionsMenuAlmanacOffset = ",184"; //optionsMenu[,ViewAlmanacButton]
        public string optionsMenu3DAccelOffset = ",180"; //optionsMenu[,3dAccelerationCheckbox]
        public string optionsMenuFullscreenOffset = ",17c"; //optionsMenu[,EnableFullscreenCheckbox]
        public string optionsMenuSfxSliderOffset = ",178"; //optionsMenu[,SfxSlider]
        public string optionsMenuMusicSliderOffset = ",174"; //optionsMenu[,MusicSlider]

        public string inlineButtonPosXOffset = ",08";
        public string inlineButtonPosYOffset = ",0c";
        public string inlineButtonWidthOffset = ",10";
        public string inlineButtonHeightOffset = ",14";

        public string awardContinueButton = ",a8";

        public string zenPlantCountOffset = ",378";
        public uint zenPlantStartOffset = 0x37c;

        public string buttonDisabledOffet = ",1a";

        public string usernamePickerCountOffset = ",198";   //usernamePickerWidget[,userCountOffset]
        public string usernamePickerNamesOffset = ",18c";   //usernamePickerWidget[,nameListOffset]

        //public string optionsMenuReturnStrOffset = ",190,a8";   //optionsWidget[,returnButton,str]

        public string daveMessageIDOffset = ",970";     //lawnapp[,daveMessageID]
        public string daveMessageLenOffset = ",988";    //lawnApp[,daveMessageID]
        public string daveMessageTextOffset = ",978,0"; //lawnApp[,daveMessageText]

        public string minigameSelectorOffset = ",880,";  //lawnApp[,minigameSelector,]       (Note the two commas)
        public string minigameIsVisibleOffset = ",64";  //lawnApp,minigameSelector,minigameX[,isVisible]
        public string minigameIsLockedOffset = ",66"; //lawnApp,minigameSelector,minigameX[,isLocked]

        public string almanacPageOffset = ",198";       //widgetPtr[,almanacPage]

        public string almanacCloseButtonOffset = ",188";
        public string almanacIndexButtonOffset = ",18c";

        public string lastStandButtonVisible = ",164,fd";


        //Memory.dll throws an exception if an aobscan is missing a space between any bytes in the search string
        //Whyyyyyyy 


        //overwrite instruction at '0045AC88' (cmp dword ptr [esi+00000154],03)
        //This checks if dialogue is almanac. If not, allows a couple of keys to be used to interact with dialogue
        //We don't want any default key input behaviour, so to disable completely, we make the cmp always true
        //83 ?? ???????? 03 74 ?? 83 ?? 20 74 ?? 83 ?? 0d
        //cmp ?? ???????? 03    (check if widget is almanac)
        //je ??                 (If almanac, skip keyboard checks)
        //cmp ?? 20             (check if spacebar is pressed)
        //je ??                 (if space is pressed, jump to space-processing code)
        //cmp ?? 0d             (check if enter is pressed)
        public string keyboardInputDisable1 = "83 ?? ?? ?? ?? ?? 03 74 ?? 83 ?? 20 74 ?? 83 ?? 0D";
        public string keyboardInputDisable1Patched = "83 ?? ?? ?? ?? ?? 03 eb ?? 83 ?? 20 74 ?? 83 ?? 0D";


        //Instruction in options menu, which reads space/enter/escape
        //je 07 (jump past the call, if not on board)
        //mov (ecx,edi)
        //e8 (CheatTypingCheckAddr)
        //cmp ?? 20 (check if space is pressed)
        //je ?? (Jump to confirm operation if so)
        //cmp ?? 0d (check if enter is pressed)
        //je ?? (jump to confirm operation if so)
        //cmp ?? 1b (check if escape is pressed)
        //jne ?? (jump past the confirm operation if so)
        //
        //We want to replace the je with nops
        //And the jne with jmp
        //
        //Replace last two "74 ??" with "90 90" (je offset > nop nop)
        //Replace "75" near end with "EB" (jne > jmp)
        public string keyboardInputDisable2 = "74 07 8b ?? e8 ?? ?? ?? ?? 83 ?? 20 74 ?? 83 ?? 0d 74 ?? 83 ?? 1b 75 ??";
        public string keyboardInputDisable2Patched = "74 07 8b ?? e8 ?? ?? ?? ?? 83 ?? 20 90 90 83 ?? 0d 90 90 83 ?? 1b eb ??";



        //Disable space/enter key when on board
        //This one is probably the most likely to go wrong
        //Replaces a cmp instruction after the cheatTypingCheck with a return (pop edi, pop eax, ret 0004).
        public string keyboardInputDisable3 = "83 ?? ?? ?? 00 00 02 75 ?? 8b ?? ?? ?? 00 00 83 ?? 2b";
        public string keyboardInputDisable3Patched = "5f 5e c2 04 00 90 90 75 ?? 8b ?? ?? ?? 00 00 83 ?? 2b";


        //Disable space/enter key when on award/help screen
        public string keyboardInputDisable4 = "8A 44 24 04 3c 20 74 08 3c 0d 74 04 3c 1b 75 09 89 4c 24 04 e9 ?? ?? ?? ?? c2 04 00";
        public string keyboardInputDisable4Patched = "c2 04 00 04 3c 20 74 08 3c 0d 74 04 3c 1b 75 09 89 4c 24 04 e9 ?? ?? ?? ?? c2 04 00";

        //Patch to allow music track synchronisation while in freeze mode
        public string musicPausePatch = "75 ?? e8 ?? ?? ?? ?? 8b ?? ?? 83 ?? ?? 74 ?? 8b ?? ?? 83 ?? ?? 74 ?? 50";
        public string musicPausePatched = "90 90 e8 ?? ?? ?? ?? 8b ?? ?? 83 ?? ?? 74 ?? 8b ?? ?? 83 ?? ?? 74 ?? 50";

        //These are probably the only things that should be exposed externally
        public string boardChain;                       //[lawnApp,board]
        public string boardPausedChain;                 //[lawnApp,board,paused]
        public string playerInfoChain;                  //[lawnApp,playerInfo]
        public string playerLevelChain;                 //[lawnApp,playerInfo,level]
        public string playerCoinsChain;                 //[lawnApp,playerInfo,coins]
        public string playerAdventureCompletionsChain;  //[lawnApp,playerInfo,adventureCompletions]
        //public string playerPurchasesChain;             //[lawnApp,playerInfo,purchases]
        public string playerMinigamesUnlockedChain;     //[lawnApp,playerInfo,minigamesUnlocked]
        public string playerPuzzleUnlockedChain;     //[lawnApp,playerInfo,puzzleUnlocked]
        public string playerSurvivalUnlockedChain;     //[lawnApp,playerInfo,survivalUnlocked]
        public string gameSceneChain;                   //[lawnApp,gameScene]
        public string gameModeChain;                    //[lawnApp,gameMode]
        public string awardTypeChain;                   //[lawnApp,awardScreen,awardType]

        public string creditsStageChain;                //[lawnApp,credits,stage]
        public string zenGardenPageChain;               //[lawnApp,zenGarden,zenPage]

        public string daveMessageIDChain;               //[lawnApp,daveMessageID]
        public string daveMessageLenChain;              //[lawnApp,daveMessageLen]
        public string daveMessageTextChain;             //[lawnApp,daveMessageText]



        public PointerInfo(
            string appName,
            string lawnAppPtrOffset,
            string dirtyBoardPtr,
            string boardPtrOffset,
            string boardPausedOffset,
            string playerInfoOffset,
            string playerLevelOffset,
            string playerCoinsOffset,
            string playerAdventureCompletionsOffset,
            //string playerPurchasesOffset,
            int playerPurchaseOffset,
            string playerMinigamesUnlockedOffset,
            string playerPuzzleUnlockedOffset,
            string playerSurvivalUnlockedOffset,
            string gameSceneOffset,
            string gameModeOffset,
            string awardScreenOffset,
            string awardTypeOffset
            )
        {
            this.appName = appName;
            this.lawnAppPtr = appName + lawnAppPtrOffset;
            this.dirtyBoardPtr = dirtyBoardPtr;
            this.boardPtrOffset = boardPtrOffset;
            this.boardPausedOffset = boardPausedOffset;
            this.playerInfoOffset = playerInfoOffset;
            this.playerLevelOffset = playerLevelOffset;
            this.gameModeOffset = gameModeOffset;
            this.playerCoinsOffset = playerCoinsOffset;
            this.playerAdventureCompletionsOffset = playerAdventureCompletionsOffset;
            //this.playerPurchasesOffset = playerPurchasesOffset;
            this.playerPurchaseOffsetInt = playerPurchaseOffset;
            this.playerMinigamesUnlockedOffset = playerMinigamesUnlockedOffset;
            this.playerPuzzleUnlockedOffset = playerPuzzleUnlockedOffset;
            this.playerSurvivalUnlockedOffset = playerSurvivalUnlockedOffset;
            this.awardScreenOffset = awardScreenOffset;

            boardChain = lawnAppPtr + boardPtrOffset;
            boardPausedChain = boardChain + boardPausedOffset;
            playerInfoChain = lawnAppPtr + playerInfoOffset;
            playerLevelChain = playerInfoChain + playerLevelOffset;
            playerCoinsChain = playerInfoChain + playerCoinsOffset;
            playerAdventureCompletionsChain = playerInfoChain + playerAdventureCompletionsOffset;
            //playerPurchasesChain = playerInfoChain + playerPurchasesOffset;
            playerMinigamesUnlockedChain = playerInfoChain + playerMinigamesUnlockedOffset;
            playerPuzzleUnlockedChain = playerInfoChain + playerPuzzleUnlockedOffset;
            playerSurvivalUnlockedChain = playerInfoChain + playerSurvivalUnlockedOffset;
            gameSceneChain = lawnAppPtr + gameSceneOffset;
            gameModeChain = lawnAppPtr + gameModeOffset;
            awardTypeChain = lawnAppPtr + awardScreenOffset + awardTypeOffset;

            creditsStageChain = lawnAppPtr + creditsScreenOffset + creditsStateOffset;
            zenGardenPageChain = lawnAppPtr + zenGardenOffset + zenGardenPageOffset;

            daveMessageIDChain = lawnAppPtr + daveMessageIDOffset;
            daveMessageLenChain = lawnAppPtr + daveMessageLenOffset;
            daveMessageTextChain = lawnAppPtr + daveMessageTextOffset;
        }

    }
}
