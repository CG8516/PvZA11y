using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class ZombatarLicenseAgreement : Widget
    {
        public ZombatarLicenseAgreement(MemoryIO memIO, string pointerChain = "") : base(memIO, pointerChain)
        {
            Console.WriteLine("Zombatar license agreement!");
            Program.Say("Zombatar license agreement!");
        }

        public override void Interact(InputIntent intent)
        {
            return;
        }
        protected override string? GetContent()
        {
            base.UpdateWidgetPosition();
            Vector2 backButtonPos = relativePos;
            backButtonPos.X += 0.1f;
            backButtonPos.Y += 0.61f;
            Task.Delay(100).Wait();
            Program.Click(backButtonPos.X,backButtonPos.Y,false,false,100);
            hasReadContent = false;

            return null;
        }
    }
}
