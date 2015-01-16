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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="CropScaling"></seealso>
    /// <seealso cref="FitScaling"></seealso>
    public class PadScaling : ScalingBase {
        private Color _backgroundColor;
        private Size _targetSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        public PadScaling(int targetWidth, int targetHeight) {
            _targetSize = new Size(targetWidth, targetHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <param name="backgroundColor"></param>
        public PadScaling(int targetWidth, int targetHeight, Color backgroundColor) {
            _backgroundColor = backgroundColor;
            _targetSize = new Size(targetWidth, targetHeight);
        }

        internal override Size CalculateNewImageSize(Size originalSize) {
            // Thanks to Cosmin for the following fix!
            var verticalRatio = originalSize.Height / (float)_targetSize.Height;
            var horizontalRatio = originalSize.Width / (float)_targetSize.Width;
            var newRatio = verticalRatio > horizontalRatio ? verticalRatio : horizontalRatio;
            var imageHeight = (int)(originalSize.Height / newRatio);
            var imageWidth = (int)(originalSize.Width / newRatio);

            return new Size(imageWidth, imageHeight);
        }

        internal override KalikoImage DrawResizedImage(KalikoImage sourceImage, Size calculatedSize, Size originalSize) {
            if(_backgroundColor.IsEmpty) {
                _backgroundColor = sourceImage.BackgroundColor;
            }

            var resizedImage = new KalikoImage(_targetSize, _backgroundColor);

            KalikoImage.DrawScaledImage(resizedImage, sourceImage, (_targetSize.Width - calculatedSize.Width) / 2, (_targetSize.Height - calculatedSize.Height) / 2, calculatedSize.Width, calculatedSize.Height);

            return resizedImage;
        }
    }
}