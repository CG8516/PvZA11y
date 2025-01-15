using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y
{
    partial class Pointers
    {
        public static PointerInfo _1_0_0_1051(string appName)
        {
            PointerInfo ret = new(
                    appName: appName,
                    lawnAppPtrOffset: "+002A9EC0",
                    boardPtrOffset: ",768",
                    boardPausedOffset: ",164",
                    playerInfoOffset: ",82c",
                    playerLevelOffset: ",24",
                    playerCoinsOffset: ",28",
                    playerAdventureCompletionsOffset: ",2c",
                    playerPurchaseOffset: 0x1c0,
                    playerMinigamesUnlockedOffset: ",320",
                    playerPuzzleUnlockedOffset: ",324",
                    playerSurvivalUnlockedOffset: ",334",
                    gameSceneOffset: ",7fc",
                    gameModeOffset: ",7f8",
                    awardScreenOffset: ",778",
                    awardTypeOffset: ",98"
            );

            ret.widgetType.MainMenu = 0x665540;
            ret.widgetType.Board = 0x656CA8;
            ret.widgetType.SeedPicker = 0x66D578;
            ret.widgetType.SimpleDialogue = 0x667ED0;
            ret.widgetType.UserName = 0x668840;
            ret.widgetType.CrazyDave = 0x66DAF0;

            ret.dialogIDOffset = ",13c";

            ret.zombieObjSize = 348;
            ret.zombiesOffset = ",90,";
            ret.zombiesMaxCountOffset = ",94";
            ret.zombiesCurrentCountOffset = ",a0";
            ret.plantsOffset = ",ac,";
            ret.plantsMaxCountOffset = ",b0";
            ret.coinsOffset = ",e4,";
            ret.coinsMaxCountOffset = ",e8";
            ret.mowersOffset = ",100,";
            ret.mowersMaxCountOffset = ",104";
            ret.mowersCurrentCountOffset = ",110";
            ret.gridItemsOffset = ",11c,";
            ret.gridItemsMaxCountOffset = ",120";
            ret.cursorOffset = ",138";
            ret.messageWidget = ",140";
            ret.seedPacketCountOffset = ",144,24";
            ret.seedPacketArrayOffset = ",144,28";
            ret.conveyorBeltCounterOffset = ",144,34c";
            ret.choosingSeedOffset = ",15c,2c";
            ret.challengeOffset = ",160";
            ret.rowTypeOffset = 0x5d8;
            ret.iceMinXOffset = 0x60c;
            ret.iceTimerOffset = 0x624;
            ret.levelTypeOffset = ",554c";
            ret.sunAmountOffset = ",5560";
            ret.numWavesOffset = ",5564";
            ret.mainCounterOffset = ",5568";
            ret.effectCounterOffset = ",556c";
            ret.currentWaveOffset = ",557c";
            ret.levelCompletedOffset = ",55fc";

            ret.creditsScreenOffset = ",77c";
            ret.creditsStateOffset = ",94";

            ret.zenGardenOffset = ",81c";

            ret.reanimsOffset = ",820,8,0";
            ret.reanimsMaxCountOffset = ",820,8,4";

            ret.widgetPosXOffset = ",30";
            ret.widgetPosYOffset = ",34";
            ret.widgetWidthOffset = ",38";
            ret.widgetHeightOffset = ",3c";
            ret.widgetDialogStringOffset = ",8c";
            ret.sliderPercentageOffset = ",90";
            ret.checkBoxIsCheckedOffset = ",90";
            ret.dialogueWidgetButton1Offset = ",94";
            ret.dialogueWidgetButton2Offset = ",98";
            ret.dialogTitleLenOffset = ",a0";
            ret.dialogTitleStrOffset = ",90";
            ret.dialogBodyLenOffset = ",ec";
            ret.dialogBodyStrOffset = ",dc";

            ret.userPickerRenameOffset = ",178";
            ret.userPickerDeleteOffset = ",17c";

            ret.optionsMenuContinueOffset = ",178";
            ret.optionsMenuRestartOffset = ",17c";
            ret.optionsMenuReturnToMainOffset = ",170";
            ret.optionsMenuAlmanacOffset = ",16c";
            ret.optionsMenu3DAccelOffset = ",168";
            ret.optionsMenuFullscreenOffset = ",164";
            ret.optionsMenuSfxSliderOffset = ",160";
            ret.optionsMenuMusicSliderOffset = ",15c";

            ret.awardContinueButton = ",88";

            ret.zenPlantCountOffset = ",350";
            ret.zenPlantStartOffset = 0x358;

            ret.seedChooserScreenOffset = ",774";
            ret.letsRockButtonOffset = ",88";
            ret.chosenSeedsOffset = 0xa4;
            ret.seedsInBankCountOffset = ",d24";

            ret.userCountFromProfileMgrOffset = ",828,14";
            ret.usernamePickerCountOffset = ",180";
            ret.usernamePickerNamesOffset = ",174";
            ret.usernamePickerNamesArrOffset = ",9c,";

            ret.daveMessageIDOffset = ",850";
            ret.daveMessageLenOffset = ",864";
            ret.daveMessageTextOffset = ",854,0";

            ret.minigameSelectorOffset = ",780";
            ret.widgetIsVisibleOffset = ",54";
            ret.widgetIsDisabledOffset = ",56";

            ret.almanacPageOffset = ",180";

            ret.almanacCloseButtonOffset = ",170";
            ret.almanacIndexButtonOffset = ",174";

            ret.loadingCompletedOffset = ",76c,9d";

            ret.focusedWidgetOffset = ",88";
            ret.baseWidgetOffset = ",94";

            ret.windowHandleOffset = ",350";

            return ret;
        }
    }
}
