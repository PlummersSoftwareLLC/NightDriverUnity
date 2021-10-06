//+--------------------------------------------------------------------------
//
// File:        FastLEDInterop.h
//
// NightDriverUnity - (c) 2021 Plummer's Software LLC.  All Rights Reserved.  
//
// This file is part of the NightDriver software project.
//
//    NightDriver is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//   
//    NightDriver is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//   
//    You should have received a copy of the GNU General Public License
//    along with Nightdriver.  It is normally found in copying.txt
//    If not, see <https://www.gnu.org/licenses/>.
//
// Description:
//
//    Scripts to allow Unity to display vides on LED strip arrays.  This
//    file provides LEDInterop such as compression, converting bytestream
//    to and from long integral values, etc.  Utilities, basically, to make
//    it easier to manage color streams created by FastLED.
//
// History:     Oct-10-2021  Davepl    Created from NightDriverServer
//---------------------------------------------------------------------------

using System;
using System.IO;
using ZLIB;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NightDriver
{
    public static class QueueExtensions
    {
        public static IEnumerable<T> DequeueChunk<T>(this ConcurrentQueue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                T result;
                if (false == queue.TryDequeue(out result))
                    throw new Exception("Unable to Dequeue the data!");
                yield return result;
            }
        }
    }

    // LEDInterop
    //
    // Functions fdor working directly with the color data in an LED string

    public static class LEDInterop
    {
        // scale8 - Given a value i, scales it down by scale/256th 

        public static byte scale8(byte i, byte scale)
        {
            return (byte)(((ushort)i * (ushort)(scale)) >> 8);
        }

        public static byte scale8_video(byte i, byte scale)
        {
            byte j = (byte)((((int)i * (int)scale) >> 8) + ((i != 0 && scale != 0) ? 1 : 0));
            return j;
        }

        // fill_solid - fills a rnage of LEDs with a given color value

        public static void fill_solid(CRGB[] leds, CRGB color)
        {
            for (int i = 0; i < leds.Length; i++)
                leds[i] = color;
        }

        // fill_rainbow - fills a range of LEDs rotating through a hue wheel

        public static void fill_rainbow(CRGB[] leds, byte initialHue, float deltaHue)
        {
            float hue = initialHue;
            for (int i = 0; i < leds.Length; i++, hue += deltaHue)
                leds[i].HSV2RGB((byte)hue, 255, 255);
        }

        // GetColorBytes - get the color data as a packaged up array of bytes

        public static byte[] GetColorBytes(CRGB[] leds)
        {
            byte[] data = new byte[leds.Length * 3];
            for (int i = 0; i < leds.Length; i++)
            {
                data[i * 3]     = leds[i].r;
                data[i * 3 + 1] = leds[i].g;
                data[i * 3 + 2] = leds[i].b;
            }
            return data;
        }

        public static byte[] ULONGToBytes(UInt64 input)
        {
            return new byte[]
            {
                (byte)((input      ) & 0xff),
                (byte)((input >>  8) & 0xff),
                (byte)((input >> 16) & 0xff),
                (byte)((input >> 24) & 0xff),
                (byte)((input >> 32) & 0xff),
                (byte)((input >> 40) & 0xff),
                (byte)((input >> 48) & 0xff),
                (byte)((input >> 56) & 0xff),
            };
        }

        // DWORD and WORD to Bytes - Flatten 16 and 32 bit values to memory

        public static byte[] DWORDToBytes(UInt32 input)
        {
            return new byte[]
            {
                (byte)((input      ) & 0xff),
                (byte)((input >>  8) & 0xff),
                (byte)((input >> 16) & 0xff),
                (byte)((input >> 24) & 0xff),
            };
        }

        public static byte[] WORDToBytes(UInt16 input)
        {
            return new byte[]
            {
                (byte)((input      ) & 0xff),
                (byte)((input >>  8) & 0xff),
            };
        }

        // CombineByteArrays - Combine N arrays and returns them as one new big new one

        public static byte[] CombineByteArrays(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        // CompressMemory
        //
        // Compress a buffer using ZLIB, return the compressed version of it as a ZLIB stream

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new ZLIBStream(compressedStream, System.IO.Compression.CompressionLevel.Optimal))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        // DecompressMemory
        //
        // Expands a buffer using ZLib, returns the uncompressed version of it

        public static byte[] Decompress(byte[] input)
        {
            using (var inStream = new MemoryStream(input))
            using (var bigStream = new ZLIBStream(inStream, System.IO.Compression.CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream())
            {
                bigStream.CopyTo(bigStreamOut);
                return bigStreamOut.ToArray();
            }
        }
    }
}

