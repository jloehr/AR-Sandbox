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
    public class ConvolveFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public enum EdgeMode {
            /// <summary>
            /// Treat pixels off the edge as zero.
            /// </summary>
            Zero = 0,

            /// <summary>
            /// Clamp pixels off the edge to the nearest edge.
            /// </summary>
            Clamp = 1,

            /// <summary>
            /// Wrap pixels off the edge to the opposite edge.
            /// </summary>
            Wrap = 2
        }


        /// <summary>
        /// Construct a filter with a null kernel. This is only useful if you're going to change the kernel later on.
        /// </summary>
        public ConvolveFilter() : this(new float[9]) {}

        /// <summary>
        /// Construct a filter with the given 3x3 kernel.
        /// </summary>
        /// <param name="matrix"></param>
        public ConvolveFilter(float[] matrix) : this(new Kernel(3, 3, matrix)) {}

        /// <summary>
        /// Construct a filter with the given kernel.
        /// </summary>
        /// <param name="rows">The number of rows in the kernel.</param>
        /// <param name="cols">The number of columns in the kernel.</param>
        /// <param name="matrix">An array of rows*cols floats containing the kernel.</param>
        public ConvolveFilter(int rows, int cols, float[] matrix) : this(new Kernel(cols, rows, matrix)) {}

        /// <summary>
        /// Construct a filter with the given 3x3 kernel.
        /// </summary>
        /// <param name="kernel"></param>
        public ConvolveFilter(Kernel kernel) {
            UseAlpha = true;
            PremultiplyAlpha = true;
            Kernel = kernel;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseAlpha { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Kernel Kernel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool PremultiplyAlpha { get; set; }


        /// <summary>
        /// Convolve a block of pixels.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="inPixels"></param>
        /// <param name="outPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="edgeAction"></param>
        public static void Convolve(Kernel kernel, int[] inPixels, int[] outPixels, int width, int height, EdgeMode edgeAction) {
            Convolve(kernel, inPixels, outPixels, width, height, true, edgeAction);
        }


        /// <summary>
        /// Convolve a block of pixels
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="inPixels"></param>
        /// <param name="outPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="alpha"></param>
        /// <param name="edgeAction"></param>
        public static void Convolve(Kernel kernel, int[] inPixels, int[] outPixels, int width, int height, bool alpha, EdgeMode edgeAction) {
            if (kernel.Height == 1)
                ConvolveH(kernel, inPixels, outPixels, width, height, alpha, edgeAction);
            else if (kernel.Width == 1)
                ConvolveV(kernel, inPixels, outPixels, width, height, alpha, edgeAction);
            else
                ConvolveHV(kernel, inPixels, outPixels, width, height, alpha, edgeAction);
        }


        /// <summary>
        /// Convolve with a 2D kernel.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="inPixels"></param>
        /// <param name="outPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="alpha"></param>
        /// <param name="edgeAction"></param>
        public static void ConvolveHV(Kernel kernel, int[] inPixels, int[] outPixels, int width, int height, bool alpha, EdgeMode edgeAction) {
            int index = 0;
            float[] matrix = kernel.GetKernel();
            int rows = kernel.Height;
            int cols = kernel.Width;
            int rows2 = rows/2;
            int cols2 = cols/2;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float r = 0, g = 0, b = 0, a = 0;

                    for (int row = -rows2; row <= rows2; row++) {
                        int iy = y + row;
                        int ioffset;
                        if (0 <= iy && iy < height)
                            ioffset = iy*width;
                        else if (edgeAction == EdgeMode.Clamp)
                            ioffset = y*width;
                        else if (edgeAction == EdgeMode.Wrap)
                            ioffset = ((iy + height)%height)*width;
                        else
                            continue;
                        int moffset = cols*(row + rows2) + cols2;
                        for (int col = -cols2; col <= cols2; col++) {
                            float f = matrix[moffset + col];

                            if (f != 0) {
                                int ix = x + col;
                                if (!(0 <= ix && ix < width)) {
                                    if (edgeAction == EdgeMode.Clamp)
                                        ix = x;
                                    else if (edgeAction == EdgeMode.Wrap)
                                        ix = (x + width)%width;
                                    else
                                        continue;
                                }
                                int rgb = inPixels[ioffset + ix];
                                a += f*((rgb >> 24) & 0xff);
                                r += f*((rgb >> 16) & 0xff);
                                g += f*((rgb >> 8) & 0xff);
                                b += f*(rgb & 0xff);
                            }
                        }
                    }
                    int ia = alpha ? PixelUtils.Clamp((int)(a + 0.5)) : 0xff;
                    int ir = PixelUtils.Clamp((int)(r + 0.5));
                    int ig = PixelUtils.Clamp((int)(g + 0.5));
                    int ib = PixelUtils.Clamp((int)(b + 0.5));
                    outPixels[index++] = (ia << 24) | (ir << 16) | (ig << 8) | ib;
                }
            }
        }


        /// <summary>
        /// Convolve with a kernel consisting of one row.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="inPixels"></param>
        /// <param name="outPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="alpha"></param>
        /// <param name="edgeAction"></param>
        public static void ConvolveH(Kernel kernel, int[] inPixels, int[] outPixels, int width, int height, bool alpha, EdgeMode edgeAction) {
            int index = 0;
            float[] matrix = kernel.GetKernel();
            int cols = kernel.Width;
            int cols2 = cols/2;

            for (int y = 0; y < height; y++) {
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
                            a += f*((rgb >> 24) & 0xff);
                            r += f*((rgb >> 16) & 0xff);
                            g += f*((rgb >> 8) & 0xff);
                            b += f*(rgb & 0xff);
                        }
                    }
                    int ia = alpha ? PixelUtils.Clamp((int)(a + 0.5)) : 0xff;
                    int ir = PixelUtils.Clamp((int)(r + 0.5));
                    int ig = PixelUtils.Clamp((int)(g + 0.5));
                    int ib = PixelUtils.Clamp((int)(b + 0.5));
                    outPixels[index++] = (ia << 24) | (ir << 16) | (ig << 8) | ib;
                }
            }
        }


        /// <summary>
        /// Convolve with a kernel consisting of one column.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="inPixels"></param>
        /// <param name="outPixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="alpha"></param>
        /// <param name="edgeAction"></param>
        public static void ConvolveV(Kernel kernel, int[] inPixels, int[] outPixels, int width, int height, bool alpha, EdgeMode edgeAction) {
            int index = 0;
            float[] matrix = kernel.GetKernel();
            int rows = kernel.Height;
            int rows2 = rows/2;

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float r = 0, g = 0, b = 0, a = 0;

                    for (int row = -rows2; row <= rows2; row++) {
                        int iy = y + row;
                        int ioffset;
                        if (iy < 0) {
                            if (edgeAction == EdgeMode.Clamp)
                                ioffset = 0;
                            else if (edgeAction == EdgeMode.Wrap)
                                ioffset = ((y + height)%height)*width;
                            else
                                ioffset = iy*width;
                        }
                        else if (iy >= height) {
                            if (edgeAction == EdgeMode.Clamp)
                                ioffset = (height - 1)*width;
                            else if (edgeAction == EdgeMode.Wrap)
                                ioffset = ((y + height)%height)*width;
                            else
                                ioffset = iy*width;
                        }
                        else
                            ioffset = iy*width;

                        float f = matrix[row + rows2];

                        if (f != 0) {
                            int rgb = inPixels[ioffset + x];
                            a += f*((rgb >> 24) & 0xff);
                            r += f*((rgb >> 16) & 0xff);
                            g += f*((rgb >> 8) & 0xff);
                            b += f*(rgb & 0xff);
                        }
                    }
                    int ia = alpha ? PixelUtils.Clamp((int)(a + 0.5)) : 0xff;
                    int ir = PixelUtils.Clamp((int)(r + 0.5));
                    int ig = PixelUtils.Clamp((int)(g + 0.5));
                    int ib = PixelUtils.Clamp((int)(b + 0.5));
                    outPixels[index++] = (ia << 24) | (ir << 16) | (ig << 8) | ib;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Run(KalikoImage image) {
            throw new NotImplementedException();
        }
    }

    public class PixelUtils {
        //public const int REPLACE = 0;
        //public const int NORMAL = 1;
        //public const int MIN = 2;
        //public const int MAX = 3;
        //public const int ADD = 4;
        //public const int SUBTRACT = 5;
        //public const int DIFFERENCE = 6;
        //public const int MULTIPLY = 7;
        //public const int HUE = 8;
        //public const int SATURATION = 9;
        //public const int VALUE = 10;
        //public const int COLOR = 11;
        //public const int SCREEN = 12;
        //public const int AVERAGE = 13;
        //public const int OVERLAY = 14;
        //public const int CLEAR = 15;
        //public const int EXCHANGE = 16;
        //public const int DISSOLVE = 17;
        //public const int DST_IN = 18;
        //public const int ALPHA = 19;
        //public const int ALPHA_TO_GRAY = 20;


        /// <summary>
        /// Clamp a value to the range 0..255
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Clamp(int value) {
            if (value < 0) {
                return 0;
            }
            if (value > 255) {
                return 255;
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int Interpolate(int v1, int v2, float f) {
            return Clamp((int)(v1 + f*(v2 - v1)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static int Brightness(int rgb) {
            int r = (rgb >> 16) & 0xff;
            int g = (rgb >> 8) & 0xff;
            int b = rgb & 0xff;
            return (r + g + b)/3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb1"></param>
        /// <param name="rgb2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool NearColors(int rgb1, int rgb2, int tolerance) {
            int r1 = (rgb1 >> 16) & 0xff;
            int g1 = (rgb1 >> 8) & 0xff;
            int b1 = rgb1 & 0xff;
            int r2 = (rgb2 >> 16) & 0xff;
            int g2 = (rgb2 >> 8) & 0xff;
            int b2 = rgb2 & 0xff;
            return Math.Abs(r1 - r2) <= tolerance && Math.Abs(g1 - g2) <= tolerance && Math.Abs(b1 - b2) <= tolerance;
        }
    }
}
