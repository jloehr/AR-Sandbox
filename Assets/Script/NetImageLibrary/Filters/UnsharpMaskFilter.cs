#region License and copyright notice
/*
 * Kaliko Image Library
 * 
 * Copyright (c) 2014 Fredrik Schultz
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

namespace Kaliko.ImageLibrary.Filters {
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class UnsharpMaskFilter : IFilter {
        readonly float _radius;
        readonly float _amount;
        private readonly int _threshold;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="amount"></param>
        /// <param name="threshold"></param>
        public UnsharpMaskFilter(float radius, float amount, int threshold) {
            _radius = radius * 3.14f;
            _amount = amount;
            _threshold = threshold;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public void Run(KalikoImage image) {
            Sharpen(image, _amount, _radius, _threshold);
        }

        private static void Sharpen(KalikoImage image, float amount, float radius, int threshold) {
            var inPixels = image.IntArray;
            var workPixels = new int[inPixels.Length];
            var outPixels = new int[inPixels.Length];

            if (radius > 0) {
                var kernel = GaussianBlurFilter.CreateKernel(radius);
                GaussianBlurFilter.ConvolveAndTranspose(kernel, inPixels, workPixels, image.Width, image.Height, true, true, false, ConvolveFilter.EdgeMode.Clamp);
                GaussianBlurFilter.ConvolveAndTranspose(kernel, workPixels, outPixels, image.Height, image.Width, true, false, true, ConvolveFilter.EdgeMode.Clamp);
            }

            for (int index = 0; index < inPixels.Length; index++) {
                int rgb1 = inPixels[index];
                int r1 = (rgb1 >> 16) & 0xff;
                int g1 = (rgb1 >> 8) & 0xff;
                int b1 = rgb1 & 0xff;

                int rgb2 = outPixels[index];
                int r2 = (rgb2 >> 16) & 0xff;
                int g2 = (rgb2 >> 8) & 0xff;
                int b2 = rgb2 & 0xff;

                if (Math.Abs(r1 - r2) >= threshold) {
                    r1 = PixelUtils.Clamp((int)((amount + 1)*(r1 - r2) + r2));
                }
                if (Math.Abs(g1 - g2) >= threshold) {
                    g1 = PixelUtils.Clamp((int)((amount + 1)*(g1 - g2) + g2));
                }
                if (Math.Abs(b1 - b2) >= threshold) {
                    b1 = PixelUtils.Clamp((int)((amount + 1)*(b1 - b2) + b2));
                }

                inPixels[index] = (int)(rgb1 & 0xff000000) | (r1 << 16) | (g1 << 8) | b1;
            }

            image.IntArray = inPixels;
        }
    }
}
