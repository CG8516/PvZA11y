using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y.Widgets
{
    class Placeholder : Widget
    {
        public Placeholder(MemoryIO memIO, string pointerChain = "") : base(memIO, pointerChain)
        {
        }

        public override void Interact(InputIntent intent)
        {
            Console.WriteLine("No active widget.");
        }
    }
}
