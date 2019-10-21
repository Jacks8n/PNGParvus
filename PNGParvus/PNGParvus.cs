using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PNGParuvs
{
    public static class PNGParvus
    {
        private static readonly uint[] CRC32_TABLE = { 0x00000000, 0x1DB71064, 0x3B6E20C8, 0x26D930AC, 0x76DC4190, 0x6B6B51F4, 0x4DB26158, 0x5005713C,
                                                       0xEDB88320, 0xF00F9344, 0xD6D6A3E8, 0xCB61B38C, 0x9B64C2B0, 0x86D3D2D4, 0xa00AE278, 0xBDBDF21C };

        private static uint _crc32, _adlerA, _adlerB;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<TPNG, TColor>(ReadOnlySpan<char> path, TPNG png) where TPNG : IPNG<TColor> where TColor : unmanaged
        {
            using FileStream stream = File.Create(path.ToString());
            Write<TPNG, TColor, FileStream>(stream, png);
        }
        public static unsafe void Write<TPNG, TColor, TStream>(TStream stream, TPNG png) where TPNG : IPNG<TColor> where TStream : Stream where TColor : unmanaged
        {
            if (((sizeof(TColor) - 3) & (~0 - 1)) > 0)
                throw new Exception("Size of TColor may be 3 or 4");

            uint height = png.Height, width = png.Width, bytesPerRow = width * (uint)sizeof(TColor) + 1, codedBytesPerRow = ~bytesPerRow;
            codedBytesPerRow = (bytesPerRow << 24) | (bytesPerRow & 0xFF00) << 8 | ((codedBytesPerRow & 0xFF) << 8) | ((codedBytesPerRow & 0xFF00) >> 8);
            _adlerA = 1; _adlerB = 0;
            WriteValue(stream, 0x89504E470D0A1A0A);                                //Magic number
            WriteChunkBegin(stream, "IHDR", 13);                                   //IHDR BEGIN
            WriteValueCRC(stream, width); WriteValueCRC(stream, height);           //  Width & Height
            WriteByteCRC(stream, 8);                                               //  8 bit depth
            WriteValueCRC(stream, (sizeof(TColor) == 4 ? 6 : 2) << 24);            //  True color with/without alpha, Deflate compression, No filter, No interlace
            WriteChunkEnd(stream);                                                 //IHDR END
            WriteChunkBegin(stream, "IDAT", 2 + height * (5 + bytesPerRow) + 4);   //IDAT BEGIN
            WriteValueCRC(stream, (ushort)0x7801);                                 //   No compression
            for (int y = 0; y < height; y++)
            {
                WriteByteCRC(stream, (byte)(y == height - 1 ? 1 : 0));             //   Marker bit for last block
                WriteValueCRC(stream, codedBytesPerRow);                           //   Coded size of block
                WriteByteAdler(stream, 0);                                         //   No filter
                for (int x = 0; x < width; x++)
                    WriteValueAdler(stream, png.GetColor(x, y));                   //   Write pixel
            }
            WriteValueCRC(stream, (_adlerB << 16) | _adlerA);                      //   Deflate END
            WriteChunkEnd(stream);                                                 //IDAT END
            WriteChunkBegin(stream, "IEND", 0); WriteChunkEnd(stream);             //IEND BEGIN & END
            stream.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteValue<TStream, TValue>(TStream stream, TValue value) where TStream : Stream where TValue : unmanaged
        {
            for (int i = sizeof(TValue) - 1; i > -1; stream.WriteByte(((byte*)&value)[i--])) ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteByteCRC<TStream>(TStream stream, byte value) where TStream : Stream
        {
            static void crc() => _crc32 = (_crc32 >> 4) ^ CRC32_TABLE[_crc32 & 0xF];
            stream.WriteByte(value);
            _crc32 ^= value; crc(); crc();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteByteAdler<TStream>(TStream stream, byte value) where TStream : Stream
        {
            WriteByteCRC(stream, value);
            _adlerA = _adlerA + value > 65521 ? _adlerA + value - 65521 : _adlerA + value;
            _adlerB = _adlerA + _adlerB > 65521 ? _adlerA + _adlerB - 65521 : _adlerA + _adlerB;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteValueCRC<TStream, TValue>(TStream stream, TValue value) where TStream : Stream where TValue : unmanaged
        {
            for (int i = sizeof(TValue) - 1; i > -1; WriteByteCRC(stream, ((byte*)&value)[i--])) ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteValueAdler<TStream, TValue>(TStream stream, TValue value) where TStream : Stream where TValue : unmanaged
        {
            for (int i = 0; i < sizeof(TValue); WriteByteAdler(stream, ((byte*)&value)[i++])) ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteChunkBegin<TStream>(TStream stream, ReadOnlySpan<char> type, uint length) where TStream : Stream
        {
            _crc32 = ~0u;
            WriteValue(stream, length);
            for (int i = 0; i < type.Length; WriteByteCRC(stream, (byte)type[i++])) ;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteChunkEnd<TStream>(TStream stream) where TStream : Stream => WriteValue(stream, ~_crc32);
    }
}
