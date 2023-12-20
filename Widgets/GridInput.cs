using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    public class GridInput
    {
        public int width;
        public int height;

        public int cursorX;
        public int cursorY;

        public bool playSounds;

        public GridInput(int width, int height, bool playSounds = true)
        {
            this.width = width;
            this.height = height;
            this.playSounds = playSounds;
        }

        //Returns whether or not the cursor moved
        public virtual bool Interact(InputIntent intent)
        {
            int prevX = cursorX;
            int prevY = cursorY;
            switch(intent)
            {
                case (InputIntent.Up):
                    cursorY--;
                    break;
                case (InputIntent.Down):
                    cursorY++;
                    break;
                case (InputIntent.Left):
                    cursorX--;
                    break;
                case (InputIntent.Right):
                    cursorX++;
                    break;
            }

            if(prevX != cursorX || prevY != cursorY)
            {
                ConfineCursorToGrid();
                //Console.WriteLine("{0},{1}", cursorX, cursorY);

                if (playSounds)
                {
                    float rightVol = (float)cursorX / (float)width;
                    float freq = 1000.0f - ((cursorY * 500.0f) / (float)height);
                    float leftVol = 1.0f - rightVol;
                    rightVol *= Config.current.GridPositionCueVolume;
                    leftVol *= Config.current.GridPositionCueVolume;
                    if (prevX != cursorX || prevY != cursorY)
                        Program.PlayTone(leftVol, rightVol, freq, freq, 100, SignalGeneratorType.Sin);
                    else
                        Program.PlayBoundaryTone();
                }

                return true;
            }

            return false;
        }

        private void ConfineCursorToGrid()
        {
            if (Config.current.WrapCursorOnGrids)
            {
                cursorX = cursorX < 0 ? width - 1 : cursorX;
                cursorX = cursorX >= width ? 0 : cursorX;

                cursorY = cursorY < 0 ? height - 1 : cursorY;
                cursorY = cursorY >= height ? 0 : cursorY;
            }
            else
            {
                cursorX = cursorX < 0 ? 0 : cursorX;
                cursorX = cursorX >= width ? width - 1 : cursorX;

                cursorY = cursorY < 0 ? 0 : cursorY;
                cursorY = cursorY >= height ? height - 1 : cursorY;
            }
        }
    }
}
