using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Memory;
using NAudio.Wave.SampleProviders;

namespace PvZA11y.Widgets
{
    struct BeepSound
    {
        float leftRightBalance; //0 = fully left, 0.5 = balanced/mono, 1 = fully right
        float frequency;
        SignalGeneratorType signalType;
        float duration;
        float startDelay;
    }
    struct InteractionResult
    {
        string? text;
        List<BeepSound> beeps;
    }
    
    abstract class Widget
    {
        public MemoryIO memIO;         //Needs a reference to the current MemIO context
        public string pointerChain;    //Complete pointerchain to this widget. I don't really like this, but I'm not sure how else to deal with widgets being in more than one memory location.

        public bool hasReadContent;
        public bool hasUpdatedContents;

        public Vector2 relativePos;
        public Vector2 size;

        public Widget(MemoryIO memIO, string pointerChain = "")
        {
            this.memIO = memIO;
            this.pointerChain = pointerChain;
            if(pointerChain != null && pointerChain.Length > 0)
                UpdateWidgetPosition();
        }

        public abstract void Interact(InputIntent intent);
        //public abstract string GetCurrentWidgetText();
        public string? GetCurrentWidgetText()
        {
            if(!hasReadContent)
            {
                hasReadContent = true;
                return GetContent();
            }
            if(hasUpdatedContents)
            {
                hasUpdatedContents = false;
                return GetContentUpdate();
            }
            return null;
        }

        public void UpdateWidgetPosition()
        {
            if (pointerChain == null || pointerChain.Length < 1)
                return;
            relativePos = memIO.GetWidgetPos(pointerChain);
            relativePos.X /= 800.0f;
            relativePos.Y /= 600.0f;

            size = memIO.GetWidgetSize(pointerChain);

            //Console.WriteLine("Widget pos: {0},{1}", relativePos.X, relativePos.Y);
            //Console.WriteLine("Widget size: {0},{1}", size.X, size.Y);
        }

        protected virtual string? GetContent()
        {
            return null;
        }

        protected virtual string? GetContentUpdate()
        {
            return null;
        }
    }
}
