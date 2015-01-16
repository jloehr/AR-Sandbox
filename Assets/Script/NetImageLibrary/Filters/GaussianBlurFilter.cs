#region License and copyright notice
/*
 * Ported to .NET for use in Kaliko.ImageLibrary by Fredrik Schultz 2014
 *
 * Original License:
 * Copyright 2006 Jerry Huxtable
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
#endregion

namespace Kaliko.ImageLibrary.Filters {
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class GaussianBlurFilter : ConvolveFilter {
        private float _radius;

        /// <summary>
        /// 
        /// </summary>
        public GaussianBlurFilter() : this(2) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        public GaussianBlurFilter(float radius) {
            Radius = radius;
        }


        /// <summary>
        /// The radius of the kernel, and hence the amount of blur. The bigger the radius, the longer this filter will take.
        /// </summary>
        public float Radius {
            get {
                return _radius;
            }
            set {
                _radius = value*3.14f;
                Kernel = CreateKernel(_radius);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public override void Run(KalikoImage image) {
            var inPixels = image.IntArray;
            var outPixels = new int[inPixels.Length];

            if ( _radius > 0 ) {
                ConvolveAndTranspose(Kernel, inPixels, outPixels, image.Width, image.Height, UseAlpha, UseAlpha && PremultiplyAlpha, false, EdgeMode.Clamp);
                ConvolveAndTranspose(Kernel, outPixels, inPixels, image.Height, image.Width, UseAlpha, false, UseAlpha && PremultiplyAlpha, EdgeMode.Clamp);
            }

            image.IntArray = inPixels;
        }
        

        /// <summary>
        /// Blur and transpose a block of ARGB pixels.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="inPixels"></param>
        /// <param name="outPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="alpha"></param>
        /// <param name="premultiply"></param>
        /// <param name="unpremultiply"></param>
        /// <param name="edgeAction"></param>
        public static void ConvolveAndTranspose(Kernel kernel, int[] inPixels, int[] outPixels, int width, int height, bool alpha, bool premultiply, bool unpremultiply, EdgeMode edgeAction) {
            float[] matrix = kernel.GetKernel();
            int cols = kernel.Width;
            int cols2 = cols/2;

            for (int y = 0; y < height; y++) {
                int index = y;
                int ioffset = y*width;
                for (int x = 0; x < width; x++) {
                    float r = 0, g = 0, b = 0, a = 0;
                    int moffset = cols2;
                    for (int col = -cols2; col <= cols2; col++) {
                        float f = matrix[moffset + col];

                        if (f != 0) {
                            int ix = x + col;
                            if (ix < 0) {
                                if (edgeAction == EdgeMode.Clamp)
                                    ix = 0;
                                else if (edgeAction == EdgeMode.Wrap)
                                    ix = (x + width)%width;
                            }
                            else if (ix >= width) {
                                if (edgeAction == EdgeMode.Clamp)
                                    ix = width - 1;
                                else if (edgeAction == EdgeMode.Wrap)
                                    ix = (x + width)%width;
                            }
                            int rgb = inPixels[ioffset + ix];
                            int pa = (rgb >> 24) & 0xff;
                            int pr = (rgb >> 16) & 0xff;
                            int pg = (rgb >> 8) & 0xff;
                            int pb = rgb & 0xff;
                            if (premultiply) {
                                float a255 = pa*(1.0f/255.0f);
                                pr = (int)(pr*a255);
                                pg = (int)(pg*a255);
                                pb = (int)(pb*a255);
                            }
                            a += f*pa;
                            r += f*pr;
                            g += f*pg;
                            b += f*pb;
                        }
                    }
                    if (unpremultiply && a != 0 && a != 255) {
                        float f = 255.0f/a;
                        r *= f;
                        g *= f;
                        b *= f;
                    }
                    int ia = alpha ? PixelUtils.Clamp((int)(a + 0.5)) : 0xff;
                    int ir = PixelUtils.Clamp((int)(r + 0.5));
                    int ig = PixelUtils.Clamp((int)(g + 0.5));
                    int ib = PixelUtils.Clamp((int)(b + 0.5));
                    outPixels[index] = (ia << 24) | (ir << 16) | (ig << 8) | ib;
                    index += height;
                }
            }
        }


        /// <summary>
        /// Make a Gaussian blur kernel.
        /// </summary>
        /// <param name="radius"> the blur radius</param>
        /// <returns>the kernel</returns>
        public static Kernel CreateKernel(float radius) {
            var r = (int)Math.Ceiling(radius);
            int rows = r*2 + 1;
            var matrix = new float[rows];
            float sigma = radius/3;
            float sigma22 = 2*sigma*sigma;
            var sigmaPi2 = (float)(2*Math.PI*sigma);
            var sqrtSigmaPi2 = (float)Math.Sqrt(sigmaPi2);
            float radius2 = radius*radius;
            float total = 0;
            int index = 0;

            for (int row = -r; row <= r; row++) {
                float distance = row*row;
                if (distance > radius2)
                    matrix[index] = 0;
                else
                    matrix[index] = (float)Math.Exp(-(distance)/sigma22)/sqrtSigmaPi2;
                total += matrix[index];
                index++;
            }

            for (int i = 0; i < rows; i++) {
                matrix[i] /= total;
            }

            return new Kernel(rows, 1, matrix);
        }
    }
}
