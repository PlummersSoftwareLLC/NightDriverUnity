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
//    FastLED Interop.  Provides some of the basics of the Arduino
//    project known as FastLED by bringing it into C# so that the
//    Unity code can more easily interact with FastLED colors streams.
//
// History:     Oct-10-2021  Davepl    Created from NightDriverServer
//---------------------------------------------------------------------------


using System;
using uint16_t = System.UInt16;
using int16_t  = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint8_t  = System.Byte;
using System.Drawing;
using UnityEngine;
using System.Threading;
using NightDriver;

namespace NightDriver
{
    // Utilities
    // 
    // Helpers and extension methods of general use throughout

    public static class Utilities
    {
        public static float constrain(this float value, float inclusiveMinimum, float inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }

        public static byte RandomByte()
        {
            return (byte)UnityEngine.Random.Range(0, 256);
        }
    }

    // CRGB
    //
    // A class that represents 24 bits of color, no alpha.  This class is similar to the CRGB class in FastLED

    public class CRGB
    {
        public byte r;
        public byte g;
        public byte b;

        public CRGB()
        {
            r = 0;
            g = 0;
            b = 0;
        }

        public CRGB(byte red, byte green, byte blue)
        {
            r = red;
            g = green;
            b = blue;
        }

        public CRGB(UInt32 input)
        {
            r = (byte)((input >> 16) & 0xFF);
            g = (byte)((input >> 8) & 0xFF);
            b = (byte)((input) & 0xFF);
        }
           
        public CRGB(CRGB other)
        {
            r = other.r;
            g = other.g;
            b = other.b;
        }

        public static CRGB RandomSaturatedColor
        {
            get
            {
                CRGB c = new CRGB();
                c.HSV2RGB((byte)UnityEngine.Random.Range(0, 256), 255, 255);
                return c;
            }

        }

        public CRGB Blend(CRGB with, float ratio = 0.5f)
        {
            Color32 c1 = new Color32(this.r, this.g, this.b, 255);
            Color32 c2 = new Color32(with.r, with.g, with.b, 255);
            Color32 c3 = Color32.Lerp(c1, c2, ratio);
            return new CRGB(c3.r, c3.g, c3.b);
        }

        public static CRGB Black        { get { return new CRGB(0,0,0);       } } 
        public static CRGB White        { get { return new CRGB(255,255,255); } }
        public static CRGB Grey         { get { return new CRGB(160,160,160); } }

        public static CRGB Red          { get { return new CRGB(255, 0, 0);   } }
        public static CRGB Maroon       { get { return new CRGB(255, 0, 128); } }
        public static CRGB Blue         { get { return new CRGB(0, 0, 255);   } }
        public static CRGB Cyan         { get { return new CRGB(0, 255, 255); } } 
        public static CRGB Green        { get { return new CRGB(0, 255, 0);   } }
        public static CRGB Yellow       { get { return new CRGB(255, 255, 0); } }
        public static CRGB Purple       { get { return new CRGB(255, 0, 255); } }
        public static CRGB Pink         { get { return new CRGB(255, 0, 128); } }
        public static CRGB Orange       { get { return new CRGB(255, 128, 0); } }

        public UInt32 ColorValueAsInt()
        {
            return (uint)(r << 16) + (uint)(g << 8) + (uint)b;
        }

        public CRGB setRGB(byte red, byte green, byte blue)
        {
            r = red;
            g = green;
            b = blue;
            return this;
        }

        public CRGB HSV2RGB(double h, double s = 1.0, double v = 1.0)
        {
            h %= 360;

            double hh, p, q, t, ff;
            long i;
            CRGB outval = new CRGB();

            if (s <= 0.0)                       // No saturaturation, return a greyscale
            {       
                outval.r = (byte)(v * 255);
                outval.g = (byte)(v * 255);
                outval.b = (byte)(v * 255);
                return outval;
            }

            hh = h;
            if (hh >= 360.0) 
                hh = 0.0;
            hh /= 60.0;
            i = (long)hh;
            ff = hh - i;
            p = v * (1.0 - s);
            q = v * (1.0 - (s * ff));
            t = v * (1.0 - (s * (1.0 - ff)));

            switch (i)
            {
                case 0:
                    outval.r = (byte)(255 * v);
                    outval.g = (byte)(255 * t);
                    outval.b = (byte)(255 * p);
                    break;
                
                case 1:
                    outval.r = (byte)(255 * q);
                    outval.g = (byte)(255 * v);
                    outval.b = (byte)(255 * p);
                    break;

                case 2:
                    outval.r = (byte)(255 * p);
                    outval.g = (byte)(255 * v);
                    outval.b = (byte)(255 * t);
                    break;

                case 3:
                    outval.r = (byte)(255 * p);
                    outval.g = (byte)(255 * q);
                    outval.b = (byte)(255 * v);
                    break;

                case 4:
                    outval.r = (byte)(255 * t);
                    outval.g = (byte)(255 * p);
                    outval.b = (byte)(255 * v); 
                    break;

                case 5:
                default:
                    outval.r = (byte)(255 * v);
                    outval.g = (byte)(255 * p);
                    outval.b = (byte)(255 * q);
                break;
            }

            return outval;
        }
    
        public CRGB HSV2RGB(byte hue, byte sat = 255, byte val = 255)
        {
            #pragma warning disable CS0162 // Unreachable code detected
            const byte K255 = 255;
            const byte K171 = 171;
            const byte K170 = 170;
            const byte K85 = 85;

            // Yellow has a higher inherent brightness than
            // any other color; 'pure' yellow is perceived to
            // be 93% as bright as white.  In order to make
            // yellow appear the correct relative brightness,
            // it has to be rendered brighter than all other
            // colors.
            // Level Y1 is a moderate boost, the default.
            // Level Y2 is a strong boost.

            const byte Y1 = 1;
            const byte Y2 = 0;

            // G2: Whether to divide all greens by two.
            // Depends GREATLY on your particular LEDs
            const byte G2 = 0;

            // Gscale: what to scale green down by.
            // Depends GREATLY on your particular LEDs
            const byte Gscale = 0;

            byte offset = (byte)(hue & 0x1F); // 0..31

            // offset8 = offset * 8
            byte offset8 = offset;
            offset8 <<= 3;

            byte third = LEDInterop.scale8(offset8, (256 / 3)); // max = 85

            if (0 == (hue & 0x80))
            {
                // 0XX
                if (0 == (hue & 0x40))
                {
                    // 00X
                    //section 0-1
                    if (0 == (hue & 0x20))
                    {
                        // 000
                        //case 0: // R -> O
                        r = (byte)(K255 - third);
                        g = third;
                        b = 0;
                    }
                    else
                    {
                        // 001
                        //case 1: // O -> Y
                        if (0 != Y1)
                        {
                            r = K171;
                            g = (byte)(K85 + third);
                            b = 0;
                        }
                        if (0 != Y2)
                        {
                            r = (byte)(K170 + third);
                            //byte twothirds = (third << 1);
                            byte twothirds = LEDInterop.scale8(offset8, ((256 * 2) / 3)); // max=170
                            g = (byte)(K85 + twothirds);
                            b = 0;
                        }
                    }
                }
                else
                {
                    //01X
                    // section 2-3
                    if (0 == (hue & 0x20))
                    {
                        // 010
                        //case 2: // Y -> G
                        if (0 != Y1)
                        {
                            //byte twothirds = (third << 1);
                            byte twothirds = LEDInterop.scale8(offset8, ((256 * 2) / 3)); // max=170
                            r = (byte)(K171 - twothirds);
                            g = (byte)(K170 + third);
                            b = 0;
                        }
                        if (0 != Y2)
                        {
                            r = (byte)(K255 - offset8);
                            g = K255;
                            b = 0;
                        }
                    }
                    else
                    {
                        // 011
                        // case 3: // G -> A
                        r = 0;
                        g = (byte)(K255 - third);
                        b = third;
                    }
                }
            }
            else
            {
                // section 4-7
                // 1XX
                if (0 == (hue & 0x40))
                {
                    // 10X
                    if (0 == (hue & 0x20))
                    {
                        // 100
                        //case 4: // A -> B
                        r = 0;
                        //byte twothirds = (third << 1);
                        byte twothirds = LEDInterop.scale8(offset8, ((256 * 2) / 3)); // max=170
                        g = (byte)(K171 - twothirds); //K170?
                        b = (byte)(K85 + twothirds);
                    }
                    else
                    {
                        // 101
                        //case 5: // B -> P
                        r = third;
                        g = 0;
                        b = (byte)(K255 - third);
                    }
                }
                else
                {
                    if (0 == (hue & 0x20))
                    {
                        // 110
                        //case 6: // P -- K
                        r = (byte)(K85 + third);
                        g = 0;
                        b = (byte)(K171 - third);
                    }
                    else
                    {
                        // 111
                        //case 7: // K -> R
                        r = (byte)(K170 + third);
                        g = 0;
                        b = (byte)(K85 - third);
                    }
                }
            }

            // Debug.Log("Midpoint -> r=" + r + ", g=" + g + ", b=" + b);

            // This is one of the good places to scale the green down,
            // although the client can scale green down as well.
            if (0 != G2)
                g = (byte)(g >> 1);
            if (0 != Gscale)
                g = LEDInterop.scale8_video(g, Gscale);

            // Scale down colors if we're desaturated at all
            // and add the brightness_floor to r, g, and b.
            if (sat != 255)
            {
                if (sat == 0)
                {
                    r = 255; b = 255; g = 255;
                }
                else
                {
                    //nscale8x3_video( r, g, b, sat);
                    if (0 != r)
                        r = (byte)(LEDInterop.scale8(r, sat) + 1);
                    if (0 != g)
                        g = (byte)(LEDInterop.scale8(g, sat) + 1);
                    if (0 != b)
                        b = (byte)(LEDInterop.scale8(b, sat) + 1);

                    byte desat = (byte)(255 - sat);
                    desat = LEDInterop.scale8(desat, desat);

                    byte brightness_floor = desat;
                    r += brightness_floor;
                    g += brightness_floor;
                    b += brightness_floor;
                }
            }

            // Now scale everything down if we're at value < 255.
            if (val != 255)
            {
                val = LEDInterop.scale8_video(val, val);
                if (val == 0)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    // nscale8x3_video( r, g, b, val);
                    if (0 != r)
                        r = (byte)(LEDInterop.scale8(r, val) + 1);
                    if (0 != g)
                        g = (byte)(LEDInterop.scale8(g, val) + 1);
                    if (0 != b)
                        b = (byte)(LEDInterop.scale8(b, val) + 1);
                }
            }
            #pragma warning disable CS0162 // Unreachable code detected
            return this;
        }

        private CRGB scaleColorsDownTo(float amount)
        { 
            r = (byte)(r * amount);
            g = (byte)(g * amount);
            b = (byte)(b * amount);
            return this;
        }

        public CRGB fadeToBlackBy(float amt) 
        {
            CRGB copy = new CRGB(this);
            float amountToFade = Utilities.constrain(amt, 0.0f, 1.0f);
            copy.scaleColorsDownTo(1.0f - amountToFade);
            return copy;
        }

        public CRGB blendWith(CRGB other, float amount)
        {
            r = (byte)(r * amount + other.r * (1.0f - amount));
            g = (byte)(g * amount + other.g * (1.0f - amount));
            b = (byte)(b * amount + other.b * (1.0f - amount));
            return this;
        }

        public static CRGB GetBlackbodyHeatColor(float temp)
        {
            temp = Math.Min(1.0f, temp);
            byte temperature = (byte)(255 * temp);
            byte t192 = (byte) Math.Round((temperature/255.0f) * 191);

            byte heatramp = (byte)(t192 & 0x3F);
            heatramp <<=2;

            if (t192 > 0x80)
                return new CRGB(255, 255, heatramp);
            else if (t192 > 0x40)
                return new CRGB(255, heatramp, 0);
            else 
                return new CRGB(heatramp, 0, 0);
        }

    }

    public interface ILEDGraphics
    {
        void DrawCircleHelper(uint x0, uint y0, uint r, uint cornername, CRGB color);
        void DrawCircle(uint x0, uint y0, uint r, bool color);
        void DrawCircle(uint x0, uint y0, uint r, CRGB color);
        void DrawRect(uint x, uint y, uint w, uint h, bool color);
        void DrawRect(uint x, uint y, uint w, uint h, CRGB color);
        void DrawFastVLine(uint x, uint y, uint h, CRGB color);
        void DrawFastHLine(uint x, uint y, uint w, CRGB color);
        void DrawRoundRect(uint x, uint y, uint w, uint h, uint r, CRGB color);
        void DrawLine(uint x0, uint y0, uint x1, uint y1, bool color);
        void DrawLine(uint x0, uint y0, uint x1, uint y1, CRGB color);
        void FillSolid(CRGB color);
        void FillRainbow(float startHue = 0.0f, float deltaHue = 5.0f);
        
        /* These are here for integration with the DotMatrix component 
        void DrawText(string            text, 
                      TextCommand.Fonts font,
                      CRGB              color,
                      float             startX, 
                      float             startY, 
                      bool              bBold = false,
                      TextAlignment     alignment = TextAlignment.Left, 
                      bool              bClear = false);
        void DrawText(string            text, 
                      TextCommand.Fonts font,
                      CRGB              color,
                      int               startX, 
                      int               startY, 
                      bool              bBold = false,
                      TextAlignment     alignment = TextAlignment.Left, 
                      bool              bClear = false);
        void DrawTextShadow(string text,
                      TextCommand.Fonts font,
                      CRGB              color,
                      uint              x,
                      uint              y,
                      bool              bBold = false,
                      TextAlignment     alignment = TextAlignment.Left);
        */

        void DrawPixel(uint x, uint y, CRGB color);
        void DrawPixels(float fPos, uint count, CRGB color);
        void DrawPixel(uint x, CRGB color);
        void BlendPixel(uint x, CRGB color);
        uint DotCount { get; }
        uint Width { get;  }
        uint Height { get;  }
    };



    abstract public class GraphicsBase : MonoBehaviour, ILEDGraphics
    {
        public virtual void Awake()
        {
        }

        private static bool IsSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public virtual uint DotCount
        {
            get { return Width * Height ;}
        }

        protected int16_t abs(int v)
        {
            return (int16_t)System.Math.Abs(v);
        }

        protected int16_t abs(int16_t v)
        {
            return System.Math.Abs(v);
        }

        protected void Swap(ref uint v1, ref uint v2)
        {
            uint v = v1;
            v1 = v2;
            v2 = v;
        }

        public void DrawCircleHelper(uint x0, uint y0, uint r, uint cornername, CRGB color)
        {
            int16_t f = (int16_t)(1 - r);
            int16_t ddF_x = 1;
            int16_t ddF_y = (int16_t)(-2 * r);
            int16_t x = 0;
            int16_t y = (int16_t) r;

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;
                if (IsSet((byte)cornername, 0x4))
                {

                    DrawPixel((uint)(x0 + x), (uint)(y0 + y), color);
                    DrawPixel((uint)(x0 + y), (uint)(y0 + x), color);
                }
                if (IsSet((byte)cornername, 0x2))
                {
                    DrawPixel((uint)(x0 + x), (uint)(y0 - y), color);
                    DrawPixel((uint)(x0 + y), (uint)(y0 - x), color);
                }
                if (IsSet((byte)cornername, 0x8))
                {
                    DrawPixel((uint)(x0 - y), (uint)(y0 + x), color);
                    DrawPixel((uint)(x0 - x), (uint)(y0 + y), color);
                }
                if (IsSet((byte)cornername, 0x1))
                {
                    DrawPixel((uint)(x0 - y), (uint)(y0 - x), color);
                    DrawPixel((uint)(x0 - x), (uint)(y0 - y), color);
                }
            }
        }
            
        public void DrawCircle(uint x0, uint y0, uint r, bool b)
        {
            DrawCircle(x0, y0, r, b ? CRGB.Green : CRGB.Black);
        }

        // Draw a circle outline
        public void DrawCircle(uint x0, uint y0, uint r, CRGB color)
        {
            int16_t f = (int16_t)(1 - r);
            int16_t ddF_x = 1;
            int16_t ddF_y = (int16_t)(-2 * r);
            int16_t x = 0;
            int16_t y = (int16_t) r;

            DrawPixel(x0, (y0 + r), color);
            DrawPixel(x0, (y0 - r), color);
            DrawPixel((x0 + r), y0, color);
            DrawPixel((x0 - r), y0, color);

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                DrawPixel((uint)(x0 + x), (uint)(y0 + y), color);
                DrawPixel((uint)(x0 - x), (uint)(y0 + y), color);
                DrawPixel((uint)(x0 + x), (uint)(y0 - y), color);
                DrawPixel((uint)(x0 - x), (uint)(y0 - y), color);
                DrawPixel((uint)(x0 + y), (uint)(y0 + x), color);
                DrawPixel((uint)(x0 - y), (uint)(y0 + x), color);
                DrawPixel((uint)(x0 + y), (uint)(y0 - x), color);
                DrawPixel((uint)(x0 - y), (uint)(y0 - x), color);
            }
        }

        public void DrawRect(uint x, uint y, uint w, uint h, CRGB color)
        {
            DrawFastHLine(x, y, w, color);
            DrawFastHLine(x, (y + h - 1), w, color);
            DrawFastVLine(x, y, h, color);
            DrawFastVLine((x + w - 1), y, h, color);
        }

        public void DrawRect(uint x, uint y, uint w, uint h, bool b)
        {
            DrawRect(x, y, w, h, b ? CRGB.Green : CRGB.Black);
        }

        public void DrawFastVLine(uint x, uint y, uint h, CRGB color)
        {
            DrawLine(x, y, x, (y + h - 1), color);
        }

        public void DrawFastHLine(uint x, uint y, uint w, CRGB color)
        {
            // Update in subclasses if desired!
            DrawLine(x, y, (x + w - 1), y, color);
        }
        public void DrawRoundRect(uint x, uint y, uint w, uint h, uint r, CRGB color)
        {
            // smarter version
            DrawFastHLine(x + r, y, w - 2 * r, color); // Top
            DrawFastHLine(x + r, y + h - 1, w - 2 * r, color); // Bottom
            DrawFastVLine(x, y + r, h - 2 * r, color); // Left
            DrawFastVLine(x + w - 1, y + r, h - 2 * r, color); // Right
                                                               // draw four corners
            DrawCircleHelper(x + r, y + r, r, 1, color);
            DrawCircleHelper(x + w - r - 1, y + r, r, 2, color);
            DrawCircleHelper(x + w - r - 1, y + h - r - 1, r, 4, color);
            DrawCircleHelper(x + r, y + h - r - 1, r, 8, color);
        }
       
        public void DrawLine(uint x0, uint y0, uint x1, uint y1, bool color)
        {
            this.DrawLine(x0, y0, x1, y1, (CRGB)(color ? new CRGB(0xFFFFFF) : new CRGB(0x000000)));
        }

        // Bresenham's algorithm - thx wikpedia
        public void DrawLine(uint x0, uint y0, uint x1, uint y1, CRGB color)
        {
            bool steep = abs((int)y1 - (int)y0) > abs((int)x1 - (int)x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int16_t dx, dy;
            dx = (int16_t)(x1 - x0);
            dy = (int16_t)(abs((int)y1 - (int)y0));

            int16_t err = (int16_t)(dx / 2);
            int16_t ystep;

            if (y0 < y1)
            {
                ystep = 1;
            }
            else
            {
                ystep = -1;
            }

            for (; x0 <= x1; x0++)
            {
                if (steep)
                {
                    DrawPixel(y0, x0, color);
                }
                else
                {
                    DrawPixel(x0, y0, color);
                }
                err -= dy;
                if (err < 0)
                {
                    y0 += (uint) ystep;
                    err += dx;
                }
            }
        }

        public void FillSolid(CRGB color)
        {
            for (uint x = 0; x < Width; x++)
                for (uint y = 0; y < Height; y++)
                    DrawPixel(x, y, color);
        }
            
        public void FillRainbow(float startHue = 0.0f, float deltaHue = 5.0f)
        {
            float hue = startHue;
            for (uint y = 0; y < Height; y++)
            {
                for (uint x = 0; x < Width; x++)
                {
                    CRGB color = new CRGB();
                    color.HSV2RGB((byte)hue);
                    DrawPixel(x, y, color);
                    hue += deltaHue;
                }
            }
        }

        /*
        public void DrawText(string            text, 
                             TextCommand.Fonts font,
                             CRGB              color,
                             float             startX, 
                             float             startY, 
                             bool              bBold = false,
                             TextAlignment     alignment = TextAlignment.Left, 
                             bool              bClear = false)
        {
            DrawText(text, font, color, (startX * Width), (startY * Height), bBold, alignment, bClear);
        }

        public void DrawText(string            text, 
                             TextCommand.Fonts font,
                             CRGB              color,
                             int               startX, 
                             int               startY, 
                             bool              bBold = false,
                             TextAlignment     alignment = TextAlignment.Left, 
                             bool              bClear = false)
        {
            int[,] textContent = TextToCont
        ent.getContent(text, font, true, bBold, 1);
            int height = textContent.GetLength(0);
            int width = textContent.GetLength(1);

            if (alignment == TextAlignment.Left)
                startX += 0;
            else if (alignment == TextAlignment.Center)
                startX -= width / 2;
            else if (alignment == TextAlignment.Right)
                startX -= width;


            for (uint y = 0; y < Height; y++)
            {
                for (uint x = 0; x < Width; x++)
                {
                    if (x >= startX && x < startX + width && y >= startY && y < startY + height)
                    {
                        var content = textContent[y - startY, x - startX];
                        if (content != 0)
                            DrawPixel(x, y, color);
                        else if (bClear)
                            DrawPixel(x, y, CRGB.Black);
                    }
                }
            }
        }

        public void DrawTextShadow(string text,
                     TextCommand.Fonts font,
                     CRGB color,
                     uint  x,
                     uint  y,
                     bool bBold = false,
                     TextAlignment alignment = TextAlignment.Left)
        {
            DrawText(text, font, color, x - 1, y - 1, bBold, TextAlignment.Center);
            DrawText(text, font, color, x + 1, y + 1, bBold, TextAlignment.Center);
            DrawText(text, font, color, x - 1, y + 1, bBold, TextAlignment.Center);
            DrawText(text, font, color, x + 1, y - 1, bBold, TextAlignment.Center);
        }
        */

        // Your implementation class must provide these, the actual writing of pixels

        public abstract void DrawPixel(uint x, uint y, CRGB color);
        public abstract void DrawPixels(float fPos, uint count, CRGB color);
        public abstract void DrawPixel(uint x, CRGB color);
        public abstract void BlendPixel(uint x, CRGB color);

        public abstract uint Width
        {
            get;
        }

        public abstract uint Height
        {
            get;
        }

        public abstract uint LEDCount
        {
            get;
        }

    }
}