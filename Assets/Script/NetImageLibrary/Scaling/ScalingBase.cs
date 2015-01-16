#region License information
/*
 * Kaliko Image Library
 * 
 * Copyright (c) 2014 Fredrik Schultz and Contributors
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

namespace Kaliko.ImageLibrary.Scaling {
    using System.Drawing;

    /// <summary>Abstract class used by scaling engines.</summary>
    /// <seealso cref="CropScaling"></seealso>
    /// <seealso cref="FitScaling"></seealso>
    /// <seealso cref="PadScaling"></seealso>
    public abstract class ScalingBase {
        internal abstract Size CalculateNewImageSize(Size originalSize);

        internal abstract KalikoImage DrawResizedImage(KalikoImage sourceImage, Size calculatedSize, Size originalSize);

        internal double GetRatio(Size size) {
            return GetRatio(size.Width, size.Height);
        }

        internal static double GetRatio(int width, int height) {
            return (double)width / height;
        }

        /// <summary>Core function that applies the scaling to the image.</summary>
        /// <param name="sourceImage">Image to be scaled</param>
        /// <returns>Scaled image</returns>
        public KalikoImage Scale(KalikoImage sourceImage) {
            var originalSize = new Size(sourceImage.Width, sourceImage.Height);

            var calculatedSize = CalculateNewImageSize(originalSize);

            return DrawResizedImage(sourceImage, calculatedSize, originalSize);
        }
    }
}
