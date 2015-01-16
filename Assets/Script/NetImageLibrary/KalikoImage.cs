#region License and copyright notice
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

namespace Kaliko.ImageLibrary {
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using Scaling;

    /// <summary>
    /// 
    /// </summary>
    public class KalikoImage : IDisposable {
        #region Private variables

        private Graphics _g;
        private Font _font;

        #endregion


        #region Constructors and destructors

        /// <summary>Create a KalikoImage from a System.Drawing.Image.</summary>
        /// <param name="image"></param>
        public KalikoImage(Image image) {
            TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            Image = image;
            _g = Graphics.FromImage(Image);
        }

        /// <summary>Create a KalikoImage with a defined width and height.</summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Creating a new transparent image
        /// var image = new KalikoImage(640, 480);</code>
        /// </example>
        public KalikoImage(int width, int height) {
            TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            CreateImage(width, height);
        }

        /// <summary>Create a KalikoImage with a defined width and height.</summary>
        /// <param name="size"></param>
        public KalikoImage(Size size) : this(size.Width, size.Height) {
        }

        /// <summary>Create a KalikoImage with a defined width, height and background color.</summary>
        /// <param name="size"></param>
        /// <param name="backgroundColor"></param>
        public KalikoImage(Size size, Color backgroundColor) : this(size.Width, size.Height, backgroundColor) {
        }

        /// <summary>Create a KalikoImage with a defined width, height and background color.</summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="backgroundColor"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Creating a white image
        /// var image = new KalikoImage(640, 480, Color.White);</code>
        /// </example>
        public KalikoImage(int width, int height, Color backgroundColor) : this(width, height) {
            BackgroundColor = backgroundColor;
            Clear(backgroundColor);
        }

        /// <summary>Create a KalikoImage by loading an image from either disk or web URL.</summary>
        /// <param name="filepath"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Open a local image: 
        /// KalikoImage image = new KalikoImage("c:\\images\\test.jpg"); 
        ///  
        /// // Load an image from the web: 
        /// KalikoImage image = new KalikoImage("http://yourdomain.com/test.jpg");</code>
        /// </example>
        public KalikoImage(string filepath) {
            TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            string prefix = filepath.Length > 8 ? filepath.Substring(0, 8).ToLower() : "";

            if (prefix.StartsWith("http://") || prefix.StartsWith("https://")) {
                // Load from URL
                LoadImageFromUrl(filepath);
            }
            else {
                // Load from local disk
                LoadImage(filepath);
            }
        }

        /// <summary>Create a KalikoImage from a stream.</summary>
        /// <param name="stream"></param>
        public KalikoImage(Stream stream) {
            TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            LoadImage(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Destroy() {
            if (_g != null) _g.Dispose();
            if (Image != null) Image.Dispose();
        }


        #endregion

        #region Internal properties

        internal Image Image { get; set; }

        internal Graphics Graphics {
            get { return _g; }
        }

        internal Font Font {
            get { return _font; }
        }

        #endregion

        #region Public properties

        /// <summary>Color used for background.</summary>
        public Color BackgroundColor { get; set; }
        /// <summary>Color used for graphical operations such as writing text on image.</summary>
        public Color Color { get; set; }
        /// <summary>Rendering mode for text operations.</summary>
        public TextRenderingHint TextRenderingHint { get; set; }

        /// <summary>Image width.</summary>
        public int Width {
            get {
                return Image.Width;
            }
        }

        /// <summary>Image height.</summary>
        public int Height {
            get {
                return Image.Height;
            }
        }

        /// <summary>Size of the image</summary>
        public Size Size {
            get {
                return Image.Size;
            }
        }

        /// <summary>
        /// Check if the current image has an indexed palette.
        /// </summary>
        public bool IndexedPalette {
            get {
                switch (Image.PixelFormat) {
                    case PixelFormat.Undefined:
                    case PixelFormat.Format1bppIndexed:
                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Format16bppGrayScale:
                    case PixelFormat.Format16bppArgb1555:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>Returns true if image has portrait ratio (higher than wide).</summary>
        public bool IsPortrait {
            get {
                return Width < Height;
            }
        }

        /// <summary>Returns true if image has landscape ratio (wider than high).</summary>
        public bool IsLandscape {
            get {
                return Width > Height;
            }
        }

        /// <summary>Returns true if image has a 1:1 ratio (same width and height).</summary>
        public bool IsSquare {
            get {
                return Width == Height;
            }
        }


        /// <summary>Width/height ratio of image.</summary>
        public double ImageRatio {
            get {
                return (double)Width / Height;
            }
        }

        #endregion


        #region Common image functions

        /// <summary>
        /// Create a new image as a clone.
        /// </summary>
        /// <returns></returns>
        public KalikoImage Clone() {
            var newImage = new KalikoImage(Image) {
                Color = Color, 
                _font = _font, 
                BackgroundColor = BackgroundColor, 
                TextRenderingHint = TextRenderingHint
            };

            return newImage;
        }

        private void CreateImage(int width, int height) {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Image = bitmap;
            _g = Graphics.FromImage(Image);
        }

        #endregion


        #region Functions for text

        /// <summary>Set the font that will be used for text operations.</summary>
        /// <param name="fileName"></param>
        /// <param name="size"></param>
        /// <param name="fontStyle"></param>
        public void SetFont(string fileName, float size, FontStyle fontStyle) {
            var pf = new PrivateFontCollection();
            pf.AddFontFile(fileName);
            _font = new Font(pf.Families[0], size, fontStyle);
        }


        /// <summary>Write text to image using the font assigned using <see cref="SetFont">SetFont Method</see>.</summary>
        /// <param name="text">Text that will be written on image</param>
        /// <param name="x">Left-most position of the text</param>
        /// <param name="y">Top-most position of the text</param>
        /// <seealso cref="M:Kaliko.ImageLibrary.KalikoImage.SetFont(System.String,System.Single,System.Drawing.FontStyle)"></seealso>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Write Hello World on image with semi transparent white
        /// var image = new KalikoImage(@"C:\Img\MyImage.jpg");
        ///  
        /// // Load the font, relative path from the application path
        /// image.SetFont("84_rock.ttf", 120, FontStyle.Regular);
        /// image.Color = Color.FromArgb(64, Color.White);
        /// image.WriteText("Hello World!", 0, 0);</code>
        /// </example>
        [Obsolete("This method is deprecated. Use DrawText(TextField textField)")]
        public void WriteText(string text, int x, int y)
        {
            _g.TextRenderingHint = TextRenderingHint;
            _g.DrawString(text, _font, new SolidBrush(Color), new Point(x, y));
        }


        /// <summary>Write text to image using the font assigned using <see cref="SetFont">SetFont Method</see> rotated in the defined angle.</summary>
        /// <param name="text">Text that will be written on image</param>
        /// <param name="x">Left-most position of the text</param>
        /// <param name="y">Top-most position of the text</param>
        /// <param name="angle">Angle to rotate the text to</param>
        /// <seealso cref="M:Kaliko.ImageLibrary.KalikoImage.SetFont(System.String,System.Single,System.Drawing.FontStyle)"></seealso>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Write Hello World on image with semi transparent white
        /// var image = new KalikoImage(@"C:\Img\MyImage.jpg");
        ///  
        /// // Load the font, relative path from the application path
        /// image.SetFont("84_rock.ttf", 120, FontStyle.Regular);
        /// image.Color = Color.FromArgb(64, Color.White);
        /// image.WriteText("Hello World!", 100, 100, 45);</code>
        /// </example>
        [Obsolete("This method is deprecated. Use DrawText(TextField textField)")]
        public void WriteText(string text, int x, int y, float angle) {
            _g.TextRenderingHint = TextRenderingHint;
            _g.TranslateTransform(x, y);
            _g.RotateTransform(angle);
            _g.DrawString(text, _font, new SolidBrush(Color), new Point(0, 0));
            _g.ResetTransform();
        }

        public void DrawText(TextField textField) {
            textField.Draw(this);
        }

        #endregion


        #region Functions for loading images (from file, stream or web)

        /// <summary>
        /// Load an image from local disk
        /// </summary>
        /// <param name="fileName">File path</param>
        public void LoadImage(string fileName) {
            Image = Image.FromFile(fileName);

            MakeImageNonIndexed();

            _g = Graphics.FromImage(Image);
        }

        /// <summary>
        /// Load an image from a stream object (MemoryStream, Stream etc)
        /// </summary>
        /// <param name="stream">Pointer to stream</param>
        public void LoadImage(Stream stream) {
            Image = Image.FromStream(stream);

            MakeImageNonIndexed();

            _g = Graphics.FromImage(Image);
        }

        /// <summary>
        /// Load an image from an URL
        /// </summary>
        /// <param name="url"></param>
        public void LoadImageFromUrl(string url) {
            var request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;

            var source = request.GetResponse().GetResponseStream();
            var ms = new MemoryStream();

            var data = new byte[256];
            int c = source.Read(data, 0, data.Length);

            while (c > 0) {
                ms.Write(data, 0, c);
                c = source.Read(data, 0, data.Length);
            }

            source.Close();
            ms.Position = 0;

            LoadImage(ms);

            ms.Close();
        }

        /// <summary>
        /// Check if image has an indexed palette and if so convert to truecolor
        /// </summary>
        private void MakeImageNonIndexed() {
            if (IndexedPalette) {
                Image = new Bitmap(new Bitmap(Image));
            }
        }

        #endregion


        #region Primitive drawing functions like clear, fill etc

        /// <summary>Clear the image and set background image to the specified color.</summary>
        /// <param name="color"></param>
        public void Clear(Color color) {
            _g.Clear(color);
        }


        /// <summary>Makes a gradient fill top to bottom from one color to another.</summary>
        /// <param name="colorFrom"></param>
        /// <param name="colorTo"></param>
        public void GradientFill(Color colorFrom, Color colorTo) {
            GradientFill(new Point(0, 0), new Point(0, Image.Height), colorFrom, colorTo);
        }


        /// <summary>Makes a gradient fill from point 1 to point 2 from one color to another.</summary>
        /// <param name="pointFrom"></param>
        /// <param name="pointTo"></param>
        /// <param name="colorFrom"></param>
        /// <param name="colorTo"></param>
        public void GradientFill(Point pointFrom, Point pointTo, Color colorFrom, Color colorTo) {
            Brush brush = new LinearGradientBrush(pointFrom, pointTo, colorFrom, colorTo);
            _g.FillRectangle(brush, 0, 0, Image.Width, Image.Height);
        }

        #endregion


        #region Functions for thumbnail creation

        /// <summary>Scale the image using a defined scaling engine which can be <see cref="Scaling.CropScaling">CropScaling Class</see> will crop the image so that the final result always
        /// has the given dimension, <see cref="Scaling.FitScaling">FitScaling Class</see> will ensure that the complete image is visible inside the given
        /// dimension or <see cref="Scaling.PadScaling">PadScaling Class</see> that will pad the image so that it cover the given dimension.</summary>
        /// <param name="scaleEngine"></param>
        /// <returns></returns>
        /// <seealso cref="Scaling.CropScaling"></seealso>
        /// <seealso cref="Scaling.FitScaling"></seealso>
        /// <seealso cref="Scaling.PadScaling"></seealso>
        public KalikoImage Scale(ScalingBase scaleEngine) {
            var resizedImage = scaleEngine.Scale(this);
            return resizedImage;
        }



        internal static void DrawScaledImage(
            KalikoImage destinationImage, KalikoImage sourceImage, int x, int y, int width, int height) {
            DrawScaledImage(destinationImage.Image, sourceImage.Image, x, y, width, height);
        }

        private static void DrawScaledImage(Image destImage, Image sourceImage, int x, int y, int width, int height) {
            using (Graphics g = Graphics.FromImage(destImage)) {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(
                        sourceImage,
                        new Rectangle(x, y, width, height),
                        0,
                        0,
                        sourceImage.Width,
                        sourceImage.Height,
                        GraphicsUnit.Pixel,
                        wrapMode);
                }
            }
        }



        #endregion


        #region Functions for blitting

        /// <summary>Will load an image and place it on the destination image at top left corner.</summary>
        /// <param name="fileName"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Load image and place on top, left of our image
        /// image.BlitImage(@"C:\Img\Stamp.png");</code>
        /// </example>
        public void BlitImage(string fileName) {
            BlitImage(fileName, 0, 0);
        }


        /// <summary>Will load an image and place it on the destination image at the defined coordinates.</summary>
        /// <param name="fileName"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Load image and place 10 pixels from the left and 20 pixels from the top
        /// image.BlitImage(@"C:\Img\Stamp.png", 10, 20);</code>
        /// </example>
        public void BlitImage(string fileName, int x, int y) {
            Image mark = Image.FromFile(fileName);
            BlitImage(mark, x, y);
            mark.Dispose();
        }


        /// <summary>Will take the source image and place it on the destination image at top left corner.</summary>
        /// <param name="image"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Place the source image on top, left of our image
        /// var sourceImage = new KalikoImage(@"C:\Img\Stamp.png");
        /// image.BlitImage(sourceImage);</code>
        /// </example>
        public void BlitImage(KalikoImage image) {
            BlitImage(image.Image, 0, 0);
        }


        /// <summary>Will take the source image and place it on the destination image at the defined coordinates.</summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Place the source image 10 pixels from the left and 20 pixels from the top
        /// var sourceImage = new KalikoImage(@"C:\Img\Stamp.png");
        /// image.BlitImage(sourceImage, 10, 20);</code>
        /// </example>
        public void BlitImage(KalikoImage image, int x, int y) {
            BlitImage(image.Image, x, y);
        }


        /// <summary>Will take the source image and place it on the destination image at top left corner.</summary>
        /// <param name="image"></param>
        public void BlitImage(Image image) {
            BlitImage(image, 0, 0);
        }


        /// <summary>Will take the source image and place it on the destination image at the defined coordinates.</summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void BlitImage(Image image, int x, int y) {
            _g.DrawImageUnscaled(image, x, y);
        }



        /// <summary>Loads the defined image and use it as a pattern to fill the image (will be tiled if the destination image is larger than the source image).</summary>
        /// <param name="fileName"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Repeat the above, but in a just one additional call
        /// var image = new KalikoImage(640, 480);
        /// image.BlitFill(@"C:\Img\Checkered.png");</code>
        /// </example>
        public void BlitFill(string fileName) {
            Image mark = Image.FromFile(fileName);
            BlitFill(mark);
            mark.Dispose();
        }


        /// <summary>Uses the defined image as a pattern to fill the image (will be tiled if the destination image is larger than the source image)..</summary>
        /// <param name="image"></param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Create a new image and fill the source image all over
        /// var image = new KalikoImage(640, 480);
        /// var patternImage = new KalikoImage(@"C:\Img\Checkered.png");
        /// image.BlitFill(patternImage);</code>
        /// </example>
        public void BlitFill(KalikoImage image) {
            BlitFill(image.Image);
        }


        /// <summary>Uses the defined image as a pattern to fill the image (will be tiled if the destination image is larger than the source image)..</summary>
        /// <param name="image"></param>
        public void BlitFill(Image image) {
            int width = image.Width;
            int height = image.Height;
            var columns = (int)Math.Ceiling((float)Image.Width / width);
            var rows = (int)Math.Ceiling((float)Image.Width / width);

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    _g.DrawImageUnscaled(image, x * width, y * height);
                }
            }
        }


        /// <summary>Crop the image into the given dimensions.</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Crop(int x, int y, int width, int height) {
            var image = new KalikoImage(width, height);
            image.BlitImage(Image, -x, -y);

            Image = image.Image;
        }


        /// <summary>Resizes the image without any <span lang="en" id="result_box" class="short_text" xml:lang="en"><span class="hps alt-edited">consideration of the current
        /// ratio. If you wish to make a ratio locked resize use <see cref="Scale">Scale Method</see> instead.</span></span></summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <seealso cref="M:Kaliko.ImageLibrary.KalikoImage.Scale(Kaliko.ImageLibrary.Scaling.ScalingBase)"></seealso>
        public void Resize(int width, int height) {
            int newWidth = width;
            int newHeight = height;

            if (newWidth == 0 && newHeight == 0) {
                return;
            }

            if (newWidth == 0) {
                newWidth = Image.Width * newHeight / Image.Height;
            }
            else if (newHeight == 0) {
                newHeight = Image.Height * newWidth / Image.Width;
            }

            var image = new KalikoImage(newWidth, newHeight);
            DrawScaledImage(image.Image, Image, 0, 0, newWidth, newHeight);

            Image = image.Image;
        }

		
        #endregion

     /*
        #region Functions for image saving and streaming


       

        /// <summary>Save image to the response stream in JPG-format. Ideal for sending realtime generated images to the web client requesting it.</summary>
        /// <param name="quality"></param>
        /// <param name="fileName"></param>
        /// <remarks>This method will set the proper HTTP-headers such as filename and mime-type.</remarks>
        public void StreamJpg(long quality, string fileName) {
            var imageStream = ImageOutput.PrepareImageStream(fileName, "image/jpeg");
            SaveJpg(imageStream, quality);
        }


        /// <summary>
        /// Save image to the response stream in PNG-format. Ideal for sending realtime generated images to the web client requesting it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <remarks>This method will set the proper HTTP-headers such as filename and mime-type.</remarks>
        public void StreamPng(string fileName) {
            var imageStream = ImageOutput.PrepareImageStream(fileName, "image/png");
            SavePng(imageStream);
        }

        /// <summary>Save image to the response stream in GIF-format. Ideal for sending realtime generated images to the web client requesting it.</summary>
        /// <param name="fileName"></param>
        /// <remarks>This method will set the proper HTTP-headers such as filename and mime-type.</remarks>
        public void StreamGif(string fileName) {
            var imageStream = ImageOutput.PrepareImageStream(fileName, "image/gif");
            SaveGif(imageStream);
        }


        /// <summary></summary>
        /// <param name="stream">Stream to save the image to</param>
        /// <param name="quality">Compression quality setting (0-100)</param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Save image to stream in jpg format with quality setting 90
        /// MemoryStream memoryStream = new MemoryStream();
        /// image.SaveJpg(memoryStream, 90);</code>
        /// </example>
        public void SaveJpg(Stream stream, long quality) {
            ImageOutput.SaveStream(Image, stream, quality, "image/jpeg");
        }


        /// <summary>Save image to file in JPG-format.</summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="quality">Compression quality setting (0-100)</param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Save image to file system in jpg format with quality setting 90
        /// image.SaveJpg(@"C:\MyImages\Output.jpg", 90);</code>
        /// </example>
        public void SaveJpg(string fileName, long quality) {
            ImageOutput.SaveFile(Image, fileName, quality, "image/jpeg");
        }


        /// <summary></summary>
        /// <exclude/>
        /// <excludetoc/>
        /// <param name="stream"></param>
        /// <param name="quality"></param>
        [Obsolete("SavePng(Stream stream, long quality) is deprecated, use SavePng(Stream stream) instead.")]
        public void SavePng(Stream stream, long quality) {
            ImageOutput.SaveStream(Image, stream, quality, "image/png");
        }


        /// <summary></summary>
        /// <exclude/>
        /// <excludetoc/>
        /// <param name="fileName"></param>
        /// <param name="quality"></param>
        [Obsolete("SavePng(string fileName, long quality) is deprecated, use SavePng(string fileName) instead.")]
        public void SavePng(string fileName, long quality) {
            ImageOutput.SaveFile(Image, fileName, quality, "image/png");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void SavePng(Stream stream) {
            Image.Save(stream, ImageFormat.Png);
        }

        /// <summary>Save image to file in PNG-format.</summary>
        /// <param name="fileName"></param>
        public void SavePng(string fileName) {
            Image.Save(fileName, ImageFormat.Png);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void SaveGif(Stream stream) {
            Image.Save(stream, ImageFormat.Gif);
        }


        /// <summary>Save image to file in GIF-format.</summary>
        /// <param name="fileName"></param>
        public void SaveGif(string fileName) {
            Image.Save(fileName, ImageFormat.Gif);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void SaveBmp(Stream stream) {
            Image.Save(stream, ImageFormat.Bmp);
        }


        /// <summary>Save image to file in BMP-format.</summary>
        /// <param name="fileName"></param>
        public void SaveBmp(string fileName) {
            Image.Save(fileName, ImageFormat.Bmp);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        public void SaveBmp(Stream stream, ImageFormat format) {
            Image.Save(stream, format);
        }


        /// <summary>Generic method that will save the image in the specified ImageFormat.</summary>
        /// <param name="fileName"></param>
        /// <param name="format">Image format to save the file in, for example ImageFormat.Tiff</param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// // Save image to file system in the selected format
        /// image.SaveImage(@"C:\MyImages\Output.tif", ImageFormat.Tiff);</code>
        /// </example>
        public void SaveImage(string fileName, ImageFormat format) {
            Image.Save(fileName, format);
        }


        #endregion
        */

        #region Functions for filters and bitmap manipulation

        private byte[] _byteArray;
        private bool _disposed;


        /// <summary>Byte array matching PixelFormat.Format32bppArgb (bgrA in real life).</summary>
        public byte[] ByteArray {
            get {
                if (_byteArray == null) {
                    BitmapData data = ((Bitmap)Image).LockBits(
                        new Rectangle(0, 0, Image.Width, Image.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);
                    int length = Image.Width * Image.Height * 4;
                    _byteArray = new byte[length];

                    if (data.Stride == Image.Width * 4) {
                        Marshal.Copy(data.Scan0, _byteArray, 0, length);
                    }
                    else {
                        for (int i = 0, l = Image.Height; i < l; i++) {
                            var p = new IntPtr(data.Scan0.ToInt32() + data.Stride * i);
                            Marshal.Copy(p, _byteArray, i * Image.Width * 4, Image.Width * 4);
                        }
                    }

                    ((Bitmap)Image).UnlockBits(data);
                }
                return _byteArray;
            }
            set {
                Image = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                BitmapData data = ((Bitmap)Image).LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                
                if (data.Stride == Image.Width * 4) {
                    Marshal.Copy(value, 0, data.Scan0, value.Length);
                }
                else {
                    for (int i = 0, l = Image.Height; i < l; i++) {
                        var p = new IntPtr(data.Scan0.ToInt32() + data.Stride * i);
                        Marshal.Copy(value, i * Image.Width * 4, p, Image.Width * 4);
                    }
                }

                ((Bitmap)Image).UnlockBits(data);
            }
        }



        /// <summary>Apply a filter to the current Image.</summary>
        /// <param name="filter">Any filter that supports the <see cref="Filters.IFilter">IFilter</see> interface</param>
        /// <seealso cref="Kaliko.ImageLibrary.Filters"></seealso>
        public void ApplyFilter(Filters.IFilter filter) {
            filter.Run(this);
        }

        #endregion

        /// <summary>Int array matching PixelFormat.Format32bppArgb (bgrA in real life)</summary>
        public int[] IntArray {
            get {
                var intArray = new int[ByteArray.Length / 4];

                for (int i = 0; i < ByteArray.Length; i += 4) {
                    intArray[i/4] = BitConverter.ToInt32(ByteArray, i);
                }

                return intArray;
            }
            set {
                var byteArray = new byte[value.Length * 4];

                for (int i = 0; i < value.Length; i++) {
                    Array.Copy(BitConverter.GetBytes(value[i]), 0, byteArray, i * 4, 4);
                }
                ByteArray = byteArray;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    if (_font != null) {
                        _font.Dispose();
                    }
                    if (_g != null) {
                        _g.Dispose();
                    }
                    if (Image != null) {
                        Image.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        ~KalikoImage() {
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #region Deprecated methods only kept for legacy

        /// <exclude/>
        /// <excludetoc/>
        [Obsolete("GetThumbnailImage method is deprecated, use Scale(ScalingBase scaleEngine) instead.")]
        public KalikoImage GetThumbnailImage(int width, int height) {
            return GetThumbnailImage(width, height, ThumbnailMethod.Crop);
        }

        /// <exclude/>
        /// <excludetoc/>
        [Obsolete("GetThumbnailImage method is deprecated, use Scale(ScalingBase scaleEngine) instead.")]
        public KalikoImage GetThumbnailImage(int width, int height, ThumbnailMethod method) {
            ScalingBase scaleEngine;

            switch (method) {
                case ThumbnailMethod.Fit:
                    scaleEngine = new FitScaling(width, height);
                    break;
                case ThumbnailMethod.Pad:
                    scaleEngine = new PadScaling(width, height);
                    break;
                default:
                    scaleEngine = new CropScaling(width, height);
                    break;
            }

            return Scale(scaleEngine);
        }

        #endregion
    }
}
