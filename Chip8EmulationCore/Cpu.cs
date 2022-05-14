using Chip8EmulationCore.Interfaces;

namespace Chip8EmulationCore
{
    /// <summary>
    /// Description of the CHIP-8 CPU.
    /// 
    /// see https://en.wikipedia.org/wiki/CHIP-8#Virtual_machine_description
    /// </summary>
    public class Cpu
    {
        private const byte SCREEN_HEIGHT = 32;
        private const byte SCREEN_WIDTH = 64;

        private readonly byte[] _memory = new byte[4096]; // CHIP-8 has 4k of memory
        private readonly byte[] _v = new byte[16]; // 16 8-bit data registers, named V0 to Vf
        private ushort _i = 0; // 16-bit address register

        private ushort _pc = 0; // Program counter
        private byte _sp = 0; // Stack pointer

        private readonly byte[] _stack = new byte[64];


        private readonly IDisplay _display;
        private readonly ISoundHandler _sound;
        private readonly DelayTimer _delayTimer;
        private readonly SoundTimer _soundTimer;

        private readonly Dictionary<ushort, Action<Opcode>> _Operations;

        public Cpu(IDisplay display, ISoundHandler sound)
        {
            _display = display ?? throw new ArgumentNullException(nameof(display));
            _sound = sound ?? throw new ArgumentNullException(nameof(sound));
            _delayTimer = new DelayTimer();
            _soundTimer = new SoundTimer(sound);

            _Operations = new Dictionary<ushort, Action<Opcode>>() {
                { 0x0   , CMCR},
                { 0x00E0, Clear },
                { 0x00EE, Return },
                { 0x1   , Goto },
                { 0x2   , Call },
                { 0x3   , CondSkip },
                { 0x4   , CondSkip },
                { 0x50  , CondSkip },
                { 0x6   , Assign },
                { 0x7   , AddWithoutCarry },
                { 0x80  , Assign },
                { 0x81  , BitOr },
                { 0x82  , BitAnd },
                { 0x83  , BitXor },
                { 0x84  , AddWithCarry },
                { 0x85  , SubtractVyFromVxWithBorrow },
                { 0x86  , ShiftRight },
                { 0x87  , SubtractVxFromVyWithBorrow },
                { 0x8E  , ShiftLeft },
                { 0x90  , CondSkip },
                { 0xA   , SetI },
                { 0xB   , GoToWithV0 },
                { 0xC   , RandomizeVx },
                { 0xD   , Display },
                { 0xE9E , null },
                { 0xEA1 , null },
                { 0xF07 , ReadDelayTimerToVx },
                { 0xF0A , null },
                { 0xF15 , SetDelayTimerFromVx },
                { 0xF18 , SetSoundTimerFromVx },
                { 0xF1E , null },
                { 0xF29 , null },
                { 0xF33 , null },
                { 0xF55 , null },
                { 0xF65 , null }
            };

        }

        #region PcLogic

        /// <summary>
        /// Pop byte from stack,
        /// 
        /// Decreases the stackpointer by 1, and then returns byte at current position
        /// </summary>
        /// <returns></returns>
        private byte PopByteFromStack()
        {
            return _stack[--_sp];
        }

        /// <summary>
        /// push byte to stack
        /// 
        /// Stores data at current stack address, and increases stackpointer by 1
        /// </summary>
        private void PushToStack(byte data)
        {
            _stack[_sp++] = data;
        }

        /// <summary>
        /// Push a ushort to the stack
        /// 
        /// Stores the data at current _sp and _sp+1, increases stackpointer by 2
        /// </summary>
        /// <param name="data"></param>
        private void PushToStack(ushort data)
        {
            _stack[_sp++] = (byte)(data >> 8);
            _stack[_sp++] = (byte)(data & 0xFF);
        }

        /// <summary>
        /// Pop a ushort from the stack
        /// 
        /// Decreases the stackpointer by two, then returns ushort of data at _sp and _sp+1
        /// </summary>
        /// <returns></returns>
        private ushort PopUshortFromStack()
        {
            _sp -= 2;
            return (ushort)(_stack[_sp] << 8 | (_stack[_sp + 1] & 0xFF));
        }

        #endregion PcLogic


        #region OpActions

        #region OpType Call
        /// <summary>
        /// Calls machine code routine (RCA 1802 for COSMAC VIP) at address NNN. 
        /// 
        /// Not necessary for most ROMs.
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CMCR(Opcode op) =>
            throw new NotImplementedException("Opcode 0x0NNN is not implemented in this emulator.");
        #endregion OpType Call

        #region OpType Display
        /// <summary>
        /// Clears the screen
        /// </summary>
        /// <param name="op"></param>
        private void Clear(Opcode op) => _display.Clear();

        /// <summary>
        /// Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
        /// 
        /// Each row of 8 pixels is read as bit-coded starting from memory location I; 
        /// I value does not change after the execution of this instruction. 
        /// VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that does not happen
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void Display(Opcode op)
        {
            ReadOnlySpan<byte> memorySpan = _memory;
            var spriteData = memorySpan.Slice(
                _i,
                (op.N ?? throw new InvalidOperationException("Missing constant (N) from opcode")));
            var flippedToUnset = _display.Draw(
                _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")],
                _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")],
                op.N.Value,
                spriteData);
            _v[0xF] = (flippedToUnset ? (byte)1 : (byte)0);
        }

        #endregion OpType Display

        #region OpType Flow
        /// <summary>
        /// Returns from subroutine
        /// </summary>
        /// <param name="op"></param>
        private void Return(Opcode op) =>
            _pc = PopUshortFromStack();

        /// <summary>
        /// Jump to address NNN
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void Goto(Opcode op) =>
            _pc = op.NNN ?? throw new InvalidOperationException("Missing address data (NNN) from opcode");

        /// <summary>
        /// Call subroutine at address NNN
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void Call(Opcode op)
        {
            PushToStack(_pc);
            _pc = op.NNN ?? throw new InvalidOperationException("Missing address data (NNN) from opcode");
        }

        /// <summary>
        /// Jumps to the address NNN plus V0.
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void GoToWithV0(Opcode op) =>
            _pc = (byte)(
                _v[0x0] +
                (op.NNN ?? throw new InvalidOperationException("Missing address data (NNN) from opcode")));
        #endregion OpType Flow

        #region OpType Cond
        /// <summary>
        /// Conditional skip
        /// 
        /// 0x3 => _pc++ if Vx == NN
        /// 0x4 => _pc++ if Vx != NN
        /// 0x50 => _pc++ if Vx == Vy
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void CondSkip(Opcode op)
        {
            switch (op.OpId) {
                case 0x3:
                    if (
                        _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] ==
                        (op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode")))
                        _pc++;

                    return;
                case 0x4:
                    if (
                        _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] !=
                        (op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode")))
                        _pc++;

                    return;
                case 0x50:
                    if (
                        _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] ==
                        _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")])
                        _pc++;

                    return;
                case 0x90:
                    if (
                        _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] !=
                        _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")])
                        _pc++;

                    return;
                default:
                    throw new ArgumentException($"Invalid opcode for operation action; Opcode: 0x{op.OpId:X2}");

            }
        }
        #endregion OpType Cond

        #region OpType Const/Assign
        /// <summary>
        /// Set a register value
        /// 
        /// 0x6 => Vx = NN
        /// 0x80 => Vx = Vy
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void Assign(Opcode op)
        {
            switch (op.OpId)
            {
                case 0x6:
                    _v[op.X ??
                        throw new InvalidOperationException("Missing X from opcode")] =
                        op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode");
                    return;
                case 0x80:
                    _v[op.X ?? 
                        throw new InvalidOperationException("Missing X from opcode")] = 
                    _v[op.Y ?? 
                        throw new InvalidOperationException("Missing Y from opcode")];
                    return;
                default:
                    throw new ArgumentException($"Invalid opcode for operation action; Opcode: 0x{op.OpId:X2}");
            }
        }

        /// <summary>
        /// Adds NN to Vx. (Carry flag is not changed)
        /// </summary>
        /// <param name="op"></param>
        private void AddWithoutCarry(Opcode op) =>
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] +=
                op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode");

        #endregion OpType Const/Assign

        #region OpType BitOp

        /// <summary>
        /// Sets Vx to Vx or Vy. (Bitwise OR operation)
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void BitOr(Opcode op) =>
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] |=
               _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")];

        /// <summary>
        /// Sets Vx to Vx and Vy. (Bitwise AND operation)
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void BitAnd(Opcode op) =>
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] &=
               _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")];

        /// <summary>
        /// Sets Vx to Vx xor Vy. (Bitwise eXclusive OR operation)
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void BitXor(Opcode op) =>
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] ^=
               _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")];

        /// <summary>
        /// Stores the least significant bit of Vx in Vf and then shifts Vx to the right by 1
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ShiftRight(Opcode op)
        {
            _v[0xF] = (byte)(_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] & 0x1);
            _v[op.X.Value] >>= 1;
        }

        /// <summary>
        /// Stores the most significant bit of Vx in Vf and then shifts Vx to the left by 1
        /// </summary>
        /// <param name="op"></param>
        private void ShiftLeft(Opcode op)
        {
            _v[0xF] = (byte)(_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] & 0x80);
            _v[op.X.Value] <<= 1;
        }

        #endregion OpType BitOp

        #region OpType Math
        /// <summary>
        /// Adds Vy to Vx. 
        /// 
        /// Vf is set to 1 when there's a carry, and to 0 when there is not.
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void AddWithCarry(Opcode op)
        {
            _v[0xF] = (
                _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] +
                _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")] > byte.MaxValue ? 
                    (byte)1 : (byte)0);
            _v[op.X.Value] += _v[op.Y.Value];
        }

        /// <summary>
        /// Vy is subtracted from Vx. 
        /// 
        /// Vf is set to 0 when there's a borrow, and 1 when there is not.
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SubtractVyFromVxWithBorrow(Opcode op)
        {
            _v[0xF] = (
                _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] -
                _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")] < byte.MinValue ? 
                    (byte)0 : (byte)1);
            _v[op.X.Value] -= _v[op.Y.Value];
        }

        /// <summary>
        /// Sets VX to VY minus VX. 
        /// 
        /// VF is set to 0 when there's a borrow, and 1 when there is not
        /// </summary>
        /// <param name="op"></param>
        private void SubtractVxFromVyWithBorrow(Opcode op)
        {
            _v[0xF] = (
                _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")] -
                _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] < byte.MinValue ?
                    (byte)0 : (byte)1);
            _v[op.X.Value] = (byte)(_v[op.Y.Value] - _v[op.X.Value]);
        }

        #endregion OpType Math

        #region OpType MEM
        /// <summary>
        /// Sets I to the address NNN
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SetI(Opcode op) =>
            _i = op.NNN ?? throw new InvalidOperationException("Missing address data (NNN) from opcode");
        #endregion OpType MEM

        #region OpType Rand
        /// <summary>
        /// Sets VX to the result of a bitwise and operation on a random byte and NN
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void RandomizeVx(Opcode op) =>
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] = (byte)(
                (byte)Random.Shared.Next() & 
                (op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode"))
            );

        #endregion OpType Rand

        #region OpType Timer

        /// <summary>
        /// Sets VX to the value of the delay timer
        /// </summary>
        /// <param name="op"></param>
        private void ReadDelayTimerToVx(Opcode op) =>
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] = _delayTimer.Value;
        

        /// <summary>
        /// Sets the delay timer to VX
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SetDelayTimerFromVx(Opcode op) =>
            _delayTimer.Value = _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")];

        /// <summary>
        /// Sets the sound timer to VX
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SetSoundTimerFromVx(Opcode op) =>
            _soundTimer.Value = _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")];
        #endregion OpType Timer

        #endregion OpActions
    }
}