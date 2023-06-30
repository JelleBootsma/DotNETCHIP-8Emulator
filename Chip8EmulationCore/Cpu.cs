using Chip8EmulationCore.IOInterfaces;
using Chip8EmulationCore.Utilities;

namespace Chip8EmulationCore
{
    /// <summary>
    /// Description of the CHIP-8 CPU.
    /// 
    /// see https://en.wikipedia.org/wiki/CHIP-8#Virtual_machine_description
    /// Most descriptions and comments for the operations were sourced from here
    /// </summary>
    public class Cpu
    {
        private const double CLOCK_FREQUENCY = 500; // Frequency of the CPU in Hz

        private const int CYCLE_TIME = (int)(1 / CLOCK_FREQUENCY * 1000); // Time per instruction cycle in ms

        private readonly byte[] _memory = new byte[4096]; // CHIP-8 has 4k of memory
        private readonly byte[] _v = new byte[16]; // 16 8-bit data registers, named V0 to Vf
        private ushort _i = 0; // 16-bit address register

        private ushort _pc = 0; // Program counter
        private byte _sp = 0; // Stack pointer


        private readonly byte[] _stack = new byte[64];
        private readonly Dictionary<byte, ushort> _fontAddresses; // Location of default font sprites in memory


        private readonly IDisplay _display;
        private readonly IKeyPad _keyPad;
        private readonly DelayTimer _delayTimer;
        private readonly SoundTimer _soundTimer;
        private readonly PrecisionClock _cpuClock;
        private readonly Dictionary<ushort, Func<Opcode, ValueTask>> _operations;

        public Cpu(IDisplay display, ISoundHandler sound, IKeyPad keyPad)
        {
            _keyPad = keyPad ?? throw new ArgumentNullException(nameof(keyPad));
            _display = display ?? throw new ArgumentNullException(nameof(display));
            _delayTimer = new DelayTimer();
            _soundTimer = new SoundTimer(sound ?? throw new ArgumentNullException(nameof(sound)));
            _cpuClock = new PrecisionClock();

            _fontAddresses = Font.LoadFontIntoMemory(_memory, ref _i);
            _operations = new Dictionary<ushort, Func<Opcode, ValueTask>>() {
                { 0x0   , (op) => WrapInstruction(CMCR, op)},
                { 0x00E0, (op) => WrapInstruction(Clear, op)},
                { 0x00EE, (op) => WrapInstruction(Return, op)},
                { 0x1   , (op) => WrapInstruction(Goto, op)},
                { 0x2   , (op) => WrapInstruction(Call, op)},
                { 0x3   , (op) => WrapInstruction(CondSkip, op)},
                { 0x4   , (op) => WrapInstruction(CondSkip, op)},
                { 0x50  , (op) => WrapInstruction(CondSkip, op)},
                { 0x6   , (op) => WrapInstruction(Assign, op)},
                { 0x7   , (op) => WrapInstruction(AddWithoutCarry, op)},
                { 0x80  , (op) => WrapInstruction(Assign, op)},
                { 0x81  , (op) => WrapInstruction(BitOr, op)},
                { 0x82  , (op) => WrapInstruction(BitAnd, op)},
                { 0x83  , (op) => WrapInstruction(BitXor, op)},
                { 0x84  , (op) => WrapInstruction(AddWithCarry, op)},
                { 0x85  , (op) => WrapInstruction(SubtractVyFromVxWithBorrow, op)},
                { 0x86  , (op) => WrapInstruction(ShiftRight, op)},
                { 0x87  , (op) => WrapInstruction(SubtractVxFromVyWithBorrow, op)},
                { 0x8E  , (op) => WrapInstruction(ShiftLeft, op)},
                { 0x90  , (op) => WrapInstruction(CondSkip, op)},
                { 0xA   , (op) => WrapInstruction(SetI, op)},
                { 0xB   , (op) => WrapInstruction(GoToWithV0, op)},
                { 0xC   , (op) => WrapInstruction(RandomizeVx, op)},
                { 0xD   , (op) => WrapInstruction(Display, op)},
                { 0xE9E , (op) => WrapInstruction(SkipInstructionIfKeyVxPressed, op)},
                { 0xEA1 , (op) => WrapInstruction(SkipInstructionIfKeyVxNotPressed, op)},
                { 0xF07 , (op) => WrapInstruction(ReadDelayTimerToVx, op)},
                { 0xF0A , WaitUntilNextKeyPress },
                { 0xF15 , (op) => WrapInstruction(SetDelayTimerFromVx, op)},
                { 0xF18 , (op) => WrapInstruction(SetSoundTimerFromVx, op)},
                { 0xF1E , (op) => WrapInstruction(AddToI, op)},
                { 0xF29 , (op) => WrapInstruction(LoadFontMemoryAddress, op)},
                { 0xF33 , (op) => WrapInstruction(SetBCD, op)},
                { 0xF55 , (op) => WrapInstruction(RegisterDump, op)},
                { 0xF65 , (op) => WrapInstruction(RegisterLoad, op)}
            };

        }

        public void LoadRom(ReadOnlySpan<byte> romData, ushort startLocation = 0x200)
        {
            _pc = startLocation;
            var romLength = romData.Length;
            Span<byte> targetBytes = ((Span<byte>)_memory).Slice(_pc, romLength);
            romData.CopyTo(targetBytes);
        }
        
        public async Task StartEmulator(CancellationToken cancellationToken = default)
        {
            try
            {

                while (!cancellationToken.IsCancellationRequested)
                    await ExecuteNextInstruction();
            }
            catch (Exception e)
            {

            }
        }

        private async ValueTask ExecuteNextInstruction()
        {
            _cpuClock.StartElapsedTimer();

            var op = OpParser.Parse(ReadNextInstruction());

            var function = _operations[op.OpId];

            // All operation implementations are valuetasks
            // Only blocking operations wrap an actual task. 
            // All other operations complete synchronously
            await function(op);
            _cpuClock.BlockUntilElapsed(CYCLE_TIME);
        }

        #region ProgramLogic
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

        private ushort ReadNextInstruction()
        {
            var op = (ushort)(_memory[_pc] << 8 | (_memory[_pc + 1] & 0xFF));
            _pc += 2;
            return op;
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
        /// 0x3 => pc++ iff Vx == NN
        /// 0x4 => pc++ iff Vx != NN
        /// 0x50 => pc++ iff Vx == Vy
        /// 0x90 => pc++ iff Vx != Vy
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void CondSkip(Opcode op)
        {
            switch (op.OpId)
            {
                case 0x3:
                    if (_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] ==
                        (op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode")))
                        _pc += 2;

                    return;
                case 0x4:
                    if (_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] !=
                        (op.NN ?? throw new InvalidOperationException("Missing constant (NN) from opcode")))
                        _pc += 2;

                    return;
                case 0x50:
                    if (_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] ==
                        _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")])
                        _pc += 2;

                    return;
                case 0x90:
                    if (_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] !=
                        _v[op.Y ?? throw new InvalidOperationException("Missing Y from opcode")])
                        _pc += 2;

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

        #region OpType BCD


        /// <summary>
        /// tores the binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2. 
        /// (In other words, take the decimal representation of VX, place the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.);
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SetBCD(Opcode op)
        {
            var vx = _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")];
            _memory[_i] = (byte)(vx / 100);
            _memory[_i + 1] = (byte)((vx % 100) / 10);
            _memory[_i + 2] = (byte)(vx % 10);
        }

        #endregion OpType BCD

        #region OpType MEM
        /// <summary>
        /// Sets I to the address NNN
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SetI(Opcode op) =>
            _i = op.NNN ?? throw new InvalidOperationException("Missing address data (NNN) from opcode");

        /// <summary>
        /// Add Vx to I. Vf is not affected
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void AddToI(Opcode op) =>
            _i += _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")];

        /// <summary>
        /// Stores from V0 to VX (including VX) in memory, starting at address I. 
        /// The offset from I is increased by 1 for each value written, but I itself is left unmodified
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void RegisterDump(Opcode op)
        {
            var targetAddress = _i;
            for (int i = 0; i <= (op.X ?? throw new InvalidOperationException("Missing X from opcode")); i++)
            {
                _memory[targetAddress++] = _v[i];
            }
        }

        /// <summary>
        /// Fills from V0 to VX (including VX) with values from memory, starting at address I. 
        /// The offset from I is increased by 1 for each value written, but I itself is left unmodified
        /// </summary>
        /// <param name="op"></param>
        private void RegisterLoad(Opcode op)
        {
            var targetAddress = _i;
            for (int i = 0; i <= (op.X ?? throw new InvalidOperationException("Missing X from opcode")); i++)
            {
                _v[i] = _memory[targetAddress++];
            }
        }

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

        #region OpType KeyOp

        /// <summary>
        /// Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block);
        /// 
        /// VX should not be larger than 0xF
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SkipInstructionIfKeyVxPressed(Opcode op)
        {
            if (_keyPad.IsKeyPressed(_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")]))
                _pc += 2;
        }
        /// <summary>
        /// Skips the next instruction if the key stored in VX is not pressed. (Usually the next instruction is a jump to skip a code block);
        /// 
        /// VX should not be larger than 0xF
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SkipInstructionIfKeyVxNotPressed(Opcode op)
        {
            if (!_keyPad.IsKeyPressed(_v[op.X ?? throw new InvalidOperationException("Missing X from opcode")]))
                _pc += 2;
        }

        /// <summary>
        /// Await the next keypress, then store the key in VX. Will not continue execution until a key is pressed
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async ValueTask WaitUntilNextKeyPress(Opcode op)
        {
            byte keyPressed = await _keyPad.AwaitNextKey();
            _v[op.X ?? throw new InvalidOperationException("Missing X from opcode")] = keyPressed;
        }

        #endregion OpType KeyOp

        #region OpType FontOp

        /// <summary>
        /// Sets I to the location of the sprite for the character in VX. 
        /// Characters 0-F (in hexadecimal) are represented by a 4x5 font.
        /// </summary>
        /// <param name="op"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void LoadFontMemoryAddress(Opcode op)
        {
            var locSet = _fontAddresses.TryGetValue(
                op.X ?? throw new InvalidOperationException("Missing X from opcode"),
                out ushort newI);
            if (locSet)
                _i = newI;
            else
                throw new ArgumentException($"Requested font character {op.X:X2} is not loaded");
        }
        #endregion OpType FontOp

        #endregion OpActions

        #region Helpers
        ValueTask WrapInstruction(Action<Opcode> instruction, Opcode op)
        {
            instruction(op);
            return ValueTask.CompletedTask;
        }
        #endregion Helpers
    }
}