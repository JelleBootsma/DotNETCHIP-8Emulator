using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulationCore
{
    /// <summary>
    /// Struct for CHIP-8 opcodes. 
    /// 
    /// CHIP-8 OpCodes are always 16 bits long -> ushort
    /// Therefore, we can number the hexpositions, ABCD => 0123. 
    /// 
    /// Example;
    /// 00E0 (display clear) has 
    /// p0 => 0
    /// p1 => 0
    /// p2 => E
    /// p3 => 0
    /// 
    /// 
    /// 
    /// </summary>
    internal readonly struct Opcode
    {
        public Opcode(ushort opId, ushort? nnn = null, byte? nn = null, byte? n = null, byte? x = null, byte? y = null)
        {
            NNN = nnn;
            NN = nn;
            N = n;
            X = x;
            Y = y;
            OpId = opId;
        }

        /// <summary>
        /// Address. 
        /// 
        /// If present, always in p1, p2 and p3
        /// </summary>
        ushort? NNN { get; }
        
        /// <summary>
        /// 8-bit constant. 
        /// 
        /// If present, always in p2 and p3
        /// </summary>
        byte? NN { get; }

        /// <summary>
        /// 4-bit constant
        /// 
        /// If present, always in p3
        /// </summary>
        byte? N { get; }

        /// <summary>
        /// 4-bit Registry identifier
        /// 
        /// If present, always in p1
        /// </summary>
        byte? X { get; }


        /// <summary>
        /// 4-bit Registry identifier
        /// 
        /// If present, always in p2
        /// </summary>
        byte? Y { get; }

        /// <summary>
        /// Opcode, without any data.
        /// 
        /// Length may vary from 4 to 16 bits, depending on the operation.
        /// 
        /// Always starts in p0, but may continue all the way to p3. (such as in op 00E0, (OpId = 00E0)
        /// Can also end in p0. (such as in 0NNN (OpId = 0)
        /// Can also skip positions, such as with 8XY4 (OpId = 84) 
        /// </summary>
        ushort OpId { get; }

    }
}
