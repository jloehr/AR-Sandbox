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
    using System.Drawing;
    using ColorSpace;

    public class ChromaKeyFilter : IFilter {
        public float ToleranceHue { get; set; }

        public float ToleranceSaturnation { get; set; }

        public float ToleranceBrightness { get; set; }

        public Color KeyColor { get; set; }

        public ChromaKeyFilter() {
            KeyColor = Color.FromArgb(0, 255, 0);
            ToleranceHue = 10;
            ToleranceSaturnation = 0.7f;
            ToleranceBrightness = 0.5f;
        }

        public ChromaKeyFilter(Color keyColor) {
            KeyColor = keyColor;
            ToleranceHue = 10;
            ToleranceSaturnation = 0.7f;
            ToleranceBrightness = 0.5f;
        }

        public ChromaKeyFilter(Color keyColor, float toleranceHue, float toleranceSaturnation, float toleranceBrightness) {
            KeyColor = keyColor;
            ToleranceHue = toleranceHue;
            ToleranceSaturnation = toleranceSaturnation;
            ToleranceBrightness = toleranceBrightness;
        }

        public void Run(KalikoImage image) {
            ValidateParameters();

            ApplyChromaKey(image);
        }

        public void ApplyChromaKey(KalikoImage image) {
            var pixels = image.IntArray;
            var keyHsb = ColorSpaceHelper.RGBtoHSB(KeyColor);

            for (int i = 0; i < pixels.Length; i++) {
                int rgb = pixels[i];

                int red = (rgb >> 16) & 0xff;
                int green = (rgb >> 8) & 0xff;
                int blue = rgb & 0xff;
                HSB hsb = ColorSpaceHelper.RGBtoHSB(red, green, blue);

                if (Math.Abs(hsb.Hue - keyHsb.Hue) < ToleranceHue && Math.Abs(hsb.Saturation - keyHsb.Saturation) < ToleranceSaturnation && Math.Abs(hsb.Brightness - keyHsb.Brightness) < ToleranceBrightness) {
                    pixels[i] = rgb & 0xffffff;
                }
                else {
                    pixels[i] = rgb;
                }
            }

            image.IntArray = pixels;
        }

        private void ValidateParameters() {
            if (ToleranceHue < 0 || ToleranceHue > 360) {
                throw new ArgumentException("ToleranceHue out of range (0..360)");
            }
            if (ToleranceSaturnation < 0 || ToleranceSaturnation > 1) {
                throw new ArgumentException("ToleranceSaturnation out of range (0..1)");
            }
            if (ToleranceBrightness < 0 || ToleranceBrightness > 1) {
                throw new ArgumentException("ToleranceBrightness out of range (0..1)");
            }
        }
    }
}
