//+--------------------------------------------------------------------------
//
// NightDriver.Net - (c) 2019 Dave Plummer.  All Rights Reserved.
//
// File:        LEDSignController.cs
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
//   Represents a specific channel on a particular strip and exposes the
//   GraphicsBase class for drawing directly on it.  
//
//   Each instance has a worker thread that manages keeping the socket 
//   connected and sending it the data that has been queued up for it.
//
// History:     Jun-15-2019        Davepl      Created
//
//---------------------------------------------------------------------------

using System;
using System.Threading;
using NightDriver;
using UnityEngine;

public class LEDSignController : LEDControllerChannel
{
    // BUGBUG(davepl) Once upon a time I protected this with a lock, and it might not be a good or bad idea,
    //                but I can't prove it either way.  

    private byte[] GetPixelData(CRGB[] MainLEDs)
    {
        return LEDInterop.GetColorBytes(MainLEDs);
    }

    public const uint FramesPerBuffer = 40;              // How many buffer frames the chips have
    public const double PercentBufferUse = 0.80;            // How much of the buffer we should use up

    public double TimeOffset
    {
        get
        {
            if (0 == FramesPerSecond)                  // No speed indication yet, can't guess at offset, assume 1 second for now
                return 1.0;

            if (!Supports64BitClock)                            // Old V001 flash is locked at 22 fps
                return 1.0;
            else
                return (double)FramesPerBuffer / FramesPerSecond * PercentBufferUse;
        }
    }

    const UInt16 WIFI_COMMAND_PIXELDATA = 0;
    const UInt16 WIFI_COMMAND_VU = 1;
    const UInt16 WIFI_COMMAND_CLOCK = 2;
    const UInt16 WIFI_COMMAND_PIXELDATA64 = 3;

    ulong foo = 0;

    protected override byte[] GetDataFrame(CRGB [] MainLEDs, DateTime timeStart)
    {
            // The old original code truncated 64 bit values down to 32, and we need to fix that, so it's a in a packet called PIXELDATA64
            // and is only sent to newer flashes taht support it.  Otherwise we send the old original foramt.

            if (false == Supports64BitClock)
            {
                // The timeOffset is how far in the future frames are generated for.  If the chips have a 2 second buffer, you could
                // go up to 2 seconds, but I shoot for the middle of the buffer depth.  Right now it's calculated as using 


                double epoch = (timeStart.Ticks - 621355968000000000 + (0.5 * TimeSpan.TicksPerSecond)) / (double)TimeSpan.TicksPerSecond;
                double fraction = epoch - (Int64)epoch;

                ulong seconds = (ulong)epoch;                                       // Whole part of time number (left of the decimal point)
                ulong uSeconds = (ulong)(fraction * 1000000);           // Fractional part of time (right of the decimal point)

                var data = GetPixelData(MainLEDs);

//                Debug.Log("Old Seconds: " + seconds + "uSec: " + uSeconds);

                return LEDInterop.CombineByteArrays(LEDInterop.WORDToBytes(WIFI_COMMAND_PIXELDATA),        // Offset, always zero for us
                                                    LEDInterop.WORDToBytes((UInt16)Channel),               // LED channel on ESP32
                                                    LEDInterop.DWORDToBytes((UInt32)data.Length / 3),      // Number of LEDs
                                                    LEDInterop.DWORDToBytes((UInt32)seconds),              // Timestamp seconds (32 bit truncation)
                                                    LEDInterop.DWORDToBytes((UInt32)uSeconds),             // Timestmap microseconds (32 bit truncation)
                                                    data);                                                 // Color Data
            }
            else
            {
            // The timeOffset is how far in the future frames are generated for.  If the chips have a 2 second buffer, you could
            // go up to 2 seconds, but I shoot for the middle of the buffer depth.  Right now it's calculated as using 

            double epoch = (timeStart.Ticks - 621355968000000000 + (1.0 * TimeSpan.TicksPerSecond)) / (double)TimeSpan.TicksPerSecond;
            double fraction = epoch - (Int64)epoch;

            ulong seconds = (ulong)epoch;                                       // Whole part of time number (left of the decimal point)
            ulong uSeconds = (ulong)(fraction * 1000000);           // Fractional part of time (right of the decimal point)

            // Debug.Log("New Seconds: " + seconds + "uSec: " + uSeconds);

                var data = GetPixelData(MainLEDs);
                return LEDInterop.CombineByteArrays(LEDInterop.WORDToBytes(WIFI_COMMAND_PIXELDATA64),      // Offset, always zero for us
                                                    LEDInterop.WORDToBytes((UInt16)Channel),               // LED channel on ESP32
                                                    LEDInterop.DWORDToBytes((UInt32)data.Length / 3),      // Number of LEDs
                                                    LEDInterop.ULONGToBytes(seconds),                      // Timestamp seconds (64 bit)
                                                    LEDInterop.ULONGToBytes(uSeconds),                     // Timestmap microseconds (64 bit)
                                                    data);                                                 // Color Data

            }
        }

};


