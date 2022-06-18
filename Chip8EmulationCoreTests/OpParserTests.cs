using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chip8EmulationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8EmulationCore.Tests
{
    [TestClass()]
    public class OpParserTests
    {
        [DataTestMethod]
        //          RAW OP              Op Id           NNN             NN          N           X           Y
        [DataRow(   (ushort)0x0123,     (ushort)0x0    , (ushort)0x123 , null      , null      , null      , null      )] // 0NNN
        [DataRow(   (ushort)0x00E0,     (ushort)0x00E0 , null          , null      , null      , null      , null      )] // 00E0
        [DataRow(   (ushort)0x00EE,     (ushort)0x00EE , null          , null      , null      , null      , null      )] // 00EE
        [DataRow(   (ushort)0x1123,     (ushort)0x1    , (ushort)0x123 , null      , null      , null      , null      )] // 1NNN
        [DataRow(   (ushort)0x2123,     (ushort)0x2    , (ushort)0x123 , null      , null      , null      , null      )] // 2NNN
        [DataRow(   (ushort)0x3A12,     (ushort)0x3    , null          , (byte)0x12, null      , (byte)0xA , null      )] // 3XNN
        [DataRow(   (ushort)0x4A12,     (ushort)0x4    , null          , (byte)0x12, null      , (byte)0xA , null      )] // 4XNN
        [DataRow(   (ushort)0x5120,     (ushort)0x50   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 5XY0
        [DataRow(   (ushort)0x6A12,     (ushort)0x6    , null          , (byte)0x12, null      , (byte)0xA , null      )] // 6XNN
        [DataRow(   (ushort)0x7A12,     (ushort)0x7    , null          , (byte)0x12, null      , (byte)0xA , null      )] // 7XNN
        [DataRow(   (ushort)0x8120,     (ushort)0x80   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY0
        [DataRow(   (ushort)0x8121,     (ushort)0x81   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY1
        [DataRow(   (ushort)0x8122,     (ushort)0x82   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY2
        [DataRow(   (ushort)0x8123,     (ushort)0x83   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY3
        [DataRow(   (ushort)0x8124,     (ushort)0x84   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY4
        [DataRow(   (ushort)0x8125,     (ushort)0x85   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY5
        [DataRow(   (ushort)0x8126,     (ushort)0x86   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY6
        [DataRow(   (ushort)0x8127,     (ushort)0x87   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XY7
        [DataRow(   (ushort)0x812E,     (ushort)0x8E   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 8XYE
        [DataRow(   (ushort)0x9120,     (ushort)0x90   , null          , null      , null      , (byte)0x1 , (byte)0x2 )] // 9XY0
        [DataRow(   (ushort)0xA123,     (ushort)0xA    , (ushort)0x123 , null      , null      , null      , null      )] // ANNN
        [DataRow(   (ushort)0xB123,     (ushort)0xB    , (ushort)0x123 , null      , null      , null      , null      )] // BNNN
        [DataRow(   (ushort)0xCA12,     (ushort)0xC    , null          , (byte)0x12, null      , (byte)0xA , null      )] // CXNN
        [DataRow(   (ushort)0xD123,     (ushort)0xD    , null          , null      , (byte)0x3 , (byte)0x1 , (byte)0x2 )] // DXYN
        [DataRow(   (ushort)0xE19E,     (ushort)0xE9E  , null          , null      , null      , (byte)0x1 , null      )] // EX9E
        [DataRow(   (ushort)0xE1A1,     (ushort)0xEA1  , null          , null      , null      , (byte)0x1 , null      )] // EXA1
        [DataRow(   (ushort)0xF107,     (ushort)0xF07  , null          , null      , null      , (byte)0x1 , null      )] // FX07
        [DataRow(   (ushort)0xF10A,     (ushort)0xF0A  , null          , null      , null      , (byte)0x1 , null      )] // FX0A
        [DataRow(   (ushort)0xF115,     (ushort)0xF15  , null          , null      , null      , (byte)0x1 , null      )] // FX15
        [DataRow(   (ushort)0xF118,     (ushort)0xF18  , null          , null      , null      , (byte)0x1 , null      )] // FX18
        [DataRow(   (ushort)0xF11E,     (ushort)0xF1E  , null          , null      , null      , (byte)0x1 , null      )] // FX1E
        [DataRow(   (ushort)0xF129,     (ushort)0xF29  , null          , null      , null      , (byte)0x1 , null      )] // FX29
        [DataRow(   (ushort)0xF133,     (ushort)0xF33  , null          , null      , null      , (byte)0x1 , null      )] // FX33
        [DataRow(   (ushort)0xF155,     (ushort)0xF55  , null          , null      , null      , (byte)0x1 , null      )] // FX55
        [DataRow(   (ushort)0xF165,     (ushort)0xF65  , null          , null      , null      , (byte)0x1 , null      )] // FX65
        public void ParseTest(ushort rawOp, ushort opId, ushort? nnn = null, byte? nn = null, byte? n = null, byte? x = null, byte? y = null)
        {
            var result = OpParser.Parse(rawOp);
            Assert.AreEqual(opId, result.OpId);
            Assert.AreEqual(nnn, result.NNN);
            Assert.AreEqual(nn, result.NN);
            Assert.AreEqual(n, result.N);
            Assert.AreEqual(x, result.X);
            Assert.AreEqual(y, result.Y);
        }
    }
}