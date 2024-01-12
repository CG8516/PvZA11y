using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y
{
    partial class Pointers
    {
        //GOTY dvd release
        public static PointerInfo _1_2_0_1073 (string appName)
        {            
            return new PointerInfo(
                appName:                            appName,
                lawnAppPtrOffset:                   "+00329670",
                //lawnAppPtrOffset:                   "+0035799C",  //Pointer for cn version of 1.2.0.1073 (why didn't they change the version number)
                dirtyBoardPtr:                      ",320,18,0,8",
                boardPtrOffset:                     ",868",
                boardPausedOffset:                  ",17c",
                playerInfoOffset:                   ",94c",
                playerLevelOffset:                  ",4c",
                playerCoinsOffset:                  ",50",
                playerAdventureCompletionsOffset:   ",54",
                playerPurchaseOffset:              0x1e8,
                playerMinigamesUnlockedOffset:      ",348",
                playerPuzzleUnlockedOffset:         ",34c",
                playerSurvivalUnlockedOffset:       ",360",
                gameSceneOffset:                    ",91c",
                gameModeOffset:                     ",918",
                awardScreenOffset:                  ",878",
                awardTypeOffset:                    ",b8"
                );
        }
    }
}
