using Chip8EmulationCore.IO;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace BlazorChip8Emulator
{
    internal class BlazorKeyPad : IKeyPad
    {
        // Future: Make this user configurable
        // For now the following mapping is used
        // Original Keypad     Mapped Keys
        // +-+-+-+-+          +-+-+-+-+
        // |1|2|3|C|          |1|2|3|4|
        // +-+-+-+-+    ===>  +-+-+-+-+
        // |4|5|6|D|          |Q|W|E|R|
        // +-+-+-+-+          +-+-+-+-+
        // |7|8|9|E|          |A|S|D|F|
        // +-+-+-+-+          +-+-+-+-+
        // |A|0|B|F|          |Z|X|C|V|
        // +-+-+-+-+          +-+-+-+-+
        private readonly ReadOnlyDictionary<string, byte> _keyMapping = new(new Dictionary<string, byte>()
        {
            {"1", 0x1 },
            {"2", 0x2 },
            {"3", 0x3 },
            {"4", 0xc },
            {"Q", 0x4 },
            {"W", 0x5 },
            {"E", 0x6 },
            {"R", 0xD },
            {"A", 0x7 },
            {"S", 0x8 },
            {"D", 0x9 },
            {"F", 0xE },
            {"Z", 0xA },
            {"X", 0x0 },
            {"C", 0xB },
            {"V", 0xF },
        });
        private readonly ReadOnlyDictionary<byte, string> _reverseKeyMapping;
        private readonly ConcurrentDictionary<string, bool> _currentlyPressed;
        private TaskCompletionSource<byte> _nextKeyTask = new();
        public BlazorKeyPad()
        {
            _currentlyPressed = new(_keyMapping.ToDictionary(x => x.Key, x => false));
            _reverseKeyMapping = new(_keyMapping.ToDictionary(x => x.Value, x => x.Key));
        }

        public Task<byte> AwaitNextKey()
        {
            _nextKeyTask = new TaskCompletionSource<byte>();
            return _nextKeyTask.Task;
        }

        public bool IsKeyPressed(byte key)
        {
            if (!_reverseKeyMapping.ContainsKey(key))
                throw new ArgumentOutOfRangeException($"Key should be between 0x0 and 0xF. Received key 0x{key:X2}");
            return _currentlyPressed[_reverseKeyMapping[key]];
        }

        internal void RegisterKeyDown(KeyboardEventArgs e)
        {
            string key = e.Key.ToUpperInvariant();
            if (!_keyMapping.ContainsKey(key))
                return;
            _currentlyPressed[key] = true;
            _nextKeyTask.TrySetResult(_keyMapping[key]);
        }
        internal void RegisterKeyUp(KeyboardEventArgs e)
        {
            string key = e.Key.ToUpperInvariant();
            if (_keyMapping.ContainsKey(key))
                _currentlyPressed[key] = false;
        }
    }
}
