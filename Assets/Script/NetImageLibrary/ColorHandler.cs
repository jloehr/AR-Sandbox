#region License and copyright notice
/*
 * Kaliko Image Library
 * 
 * Copyright (c) 2009 Fredrik Schultz
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 */
#endregion

namespace Kaliko.ImageLibrary {
    using System;
    using System.Drawing;
    using System.Globalization;

    /// <summary>
    /// Class to handle color specific code.
    /// </summary>
    /// <exclude/>
    /// <excludetoc/>
    public class ColorHandler {
        /// <summary>
        /// Parse a web color type of string (for example "#FF0000") into a System.Drawing.Color object.
        /// </summary>
        /// <param name="colorString">Color in string format (i e "#FFFFFF")</param>
        /// <returns>Color</returns>
        [Obsolete("StringToColor is deprecated, use ColorSpaceHelper.HexToColor instead.")]
        public static Color StringToColor(string colorString) {
            Color color;
            
            // Remove # if any
            colorString = colorString.TrimStart('#');

            // Parse the color string
            int c = int.Parse(colorString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            if(colorString.Length == 3) {
                // Convert from RGB-form
                color = Color.FromArgb(255, (c & 0xf00) >> 8, (c & 0x0f0) >> 4, (c & 0x00f));
            }
            else {
                // Convert from RRGGBB-form
                color = Color.FromArgb(255, (c & 0xff0000) >> 16, (c & 0x00ff00) >> 8, (c & 0x0000ff));
            }

            return color;
        }

    }
}
