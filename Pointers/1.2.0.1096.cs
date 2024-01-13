using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y
{
    partial class Pointers
    {
        //Latest steam release

        public static PointerInfo _1_2_0_1096(string appName)
        {
            PointerInfo ret = new PointerInfo(
                appName: appName,
                lawnAppPtrOffset: "+00331C50",
                dirtyBoardPtr: ",320,18,0,8",
                boardPtrOffset: ",868",
                boardPausedOffset: ",17c",
                playerInfoOffset: ",94c",
                playerLevelOffset: ",50",
                playerCoinsOffset: ",54",
                playerAdventureCompletionsOffset: ",58",
                playerPurchaseOffset: 0x1ec,
                playerMinigamesUnlockedOffset: ",34c",
                playerPuzzleUnlockedOffset: ",350",
                playerSurvivalUnlockedOffset: ",364",
                gameSceneOffset: ",91c",
                gameModeOffset: ",918",
                awardScreenOffset: ",878",
                awardTypeOffset: ",b8"
                );

            ret.widgetType.MainMenu = 7269512;
            ret.widgetType.Board = 7261280;
            ret.widgetType.SeedPicker = 7311744;
            ret.widgetType.SimpleDialogue = 7281976;
            ret.widgetType.UserName = 7283984;
            //ret.widgetType.Almanac = 7255288;

            ret.dialogIDOffset = ",158";

            ret.dialogueWidgetButton1Offset = ",17c";
            ret.dialogueWidgetButton2Offset = ",180";

            ret.dialogTitleLenOffset = ",d0";
            ret.dialogTitleStrOffset = ",c0";

            //Turns out this wasn't needed.
            //Only happened because game failed to load a file, which was causing strings to get corrupt with the steam version (oops)
            //ret.buttonTextAlwaysPtr = true;

            ret.optionsMenuContinueOffset = ",194";
            ret.optionsMenuRestartOffset = ",190";
            ret.optionsMenuReturnToMainOffset = ",18c";
            ret.optionsMenuAlmanacOffset = ",188";
            ret.optionsMenu3DAccelOffset = ",184";
            ret.optionsMenuFullscreenOffset = ",180";
            ret.optionsMenuSfxSliderOffset = ",17c";
            ret.optionsMenuMusicSliderOffset = ",178";

            ret.almanacPageOffset = ",1a8";

            ret.zenPlantCountOffset = ",37c";
            ret.zenPlantStartOffset = 0x384;

            ret.dialogBodyStrOffset = ",f8";
            ret.dialogBodyLenOffset = ",108";

            ret.usernamePickerCountOffset = ",1a0";
            ret.usernamePickerNamesOffset = ",194";

            ret.inlineButtonPosXOffset = ",10";
            ret.inlineButtonPosYOffset = ",14";
            ret.inlineButtonWidthOffset = ",18";
            ret.inlineButtonHeightOffset = ",1c";

            ret.awardContinueButton = ",a8";

            ret.almanacCloseButtonOffset = ",194";
            ret.almanacIndexButtonOffset = ",198";

            ret.lastStandButtonVisible = ",164,105";

            ret.playerChallengeScoreOffsetInt = 0x58;

            ret.buttonDisabledOffet = ",22";

            ret.userPickerRenameOffset = ",198";
            ret.userPickerDeleteOffset = ",19c";

            return ret;
        }
    }
}
