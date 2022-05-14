using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulationCore.Interfaces
{
    /// <summary>
    /// Display interface, which the Cpu can use to display things to the user
    /// </summary>
    public interface IDisplay
    {
        // Clear everything on the display
        public void Clear();
    }
}
