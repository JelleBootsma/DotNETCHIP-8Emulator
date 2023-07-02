using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Chip8EmulationCore.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorChip8Emulator
{
    public class CanvasDisplay : DisplayBase
    {
        private const string FILL_COLOUR = "white";
        private readonly Canvas2DContext _context;

        protected readonly BECanvasComponent _canvasReference;
        
        /// <summary>
        /// Linear scaling factor of pixels.
        /// 
        /// E.G. 640x320 display is factor 10 (64 * 10)x(32 * 10)
        /// </summary>
        private readonly int _canvasFactor;

        private bool styleSet = false;
        /// <summary>
        /// Canvas should be 2:1 and be a multiple of 64.
        /// </summary>
        /// <param name="canvasReference"></param>
        /// <exception cref="ArgumentException"></exception>
        public CanvasDisplay(BECanvasComponent canvasReference, Canvas2DContext context)
        {
            if (canvasReference.Width % 64 != 0)
                throw new ArgumentException("Width needs to be a multiple of 64");
            if (canvasReference.Height % 32 != 0)
                throw new ArgumentException("Height needs to be a multiple of 32");
            if (canvasReference.Width / canvasReference.Height != 2)
                throw new ArgumentException("Display aspect ratio needs to be 2:1");
            _canvasFactor = (int)(canvasReference.Width / 64);
            _canvasReference = canvasReference;
            _context = context;
        }



        private async Task ExecuteChanges(byte[] changedBytes)
        {
            await EnsureStyleSet();

            var changedPixels = new bool[8];
            var pixelValues = new bool[8];
            // Explicit batching of draw calls

            await _context.BeginBatchAsync();
            for (int i = 0; i < 256; i++)
            {
                byte change = changedBytes[i];
                byte valueByte = _buffer[i];
                ByteToBoolSpan(valueByte, pixelValues);
                ByteToBoolSpan(change, changedPixels);
                for (int j = 0; j < 8; j++)
                {
                    if (!changedPixels[j])
                        continue;
                    await SetPixel(pixelValues[j], (i * 8) + j);
                }
            }
            await _context.EndBatchAsync();
        }

        private async ValueTask EnsureStyleSet()
        {
            if (!styleSet)
            {
                styleSet = true;
                await _context.SetFillStyleAsync(FILL_COLOUR);
            }
        }

        /// <summary>
        /// Set a specific chip-8 pixel on or off
        /// </summary>
        /// <param name="pixelValue"></param>
        /// <param name="location">Location of the chip-8 pixel. Indexed from 0 in a Left-to-Right, then downwards fashion</param>
        /// <returns></returns>
        private async Task SetPixel(bool pixelValue, int location)
        {
            // First calculate the location of the pixel
            var chip8X = location % Width;
            var chip8Y = location / Width;

            // Convert those chip-8 coordinates to canvas coordinates
            var canvasX = chip8X * _canvasFactor;
            var canvasY = chip8Y * _canvasFactor;

            if (pixelValue)
                await _context.FillRectAsync(canvasX, canvasY, _canvasFactor, _canvasFactor);
            else
                await _context.ClearRectAsync(canvasX, canvasY, _canvasFactor, _canvasFactor);
        }

        private void ByteToBoolSpan(byte source, Span<bool> target)
        {
            for (int i = 0; i < 8; i++)
                target[i] = (source & (0x80 >> i)) != 0;
        }

        protected override void OnBufferChanged(ReadOnlySpan<byte> changedBytes) =>
            ExecuteChanges(changedBytes.ToArray()); // Don't await the update of the screen, but let the CPU continue
        
    }
}
