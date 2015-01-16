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
    using System.Drawing;
    using System.Drawing.Drawing2D;

    public class TextField {
        public StringAlignment Alignment { get; set; }
        public Font Font { get; set; }
        public float Outline { get; set; }
        public Color OutlineColor { get; set; }
        public Point Point { get; set; }
        public float Rotation { get; set; }
        public Rectangle TargetArea { get; set; }
        public string Text { get; set; }
        public Brush TextBrush { get; set; }
        public Color TextColor { get; set; }
        public TextShadow TextShadow { get; set; }
        public StringAlignment VerticalAlignment { get; set; }

        public TextField(string text) {
            Alignment = StringAlignment.Near;
            Outline = 0;
            OutlineColor = Color.Black;
            Text = text;
            TextColor = Color.White;
            VerticalAlignment = StringAlignment.Near;
        }

        public void Draw(KalikoImage image) {
            var graphics = image.Graphics;
            var graphicsPath = new GraphicsPath();
            var stringFormat = new StringFormat {
                Alignment = Alignment,
                LineAlignment = VerticalAlignment
            };

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (Font == null) {
                Font = image.Font ?? new Font("Arial", 32, FontStyle.Bold, GraphicsUnit.Pixel);
            }

            if (TargetArea == Rectangle.Empty) {
                TargetArea = new Rectangle(0, 0, image.Width, image.Height);
            }

            if (Point == Point.Empty) {
                graphicsPath.AddString(Text, Font.FontFamily, (int)Font.Style, Font.Size, TargetArea, stringFormat);
            }
            else {
                graphicsPath.AddString(Text, Font.FontFamily, (int)Font.Style, Font.Size, Point, stringFormat);
            }

            if (Rotation != 0) {
                var rotationTransform = new Matrix(1, 0, 0, 1, 0, 0);
                var bounds = graphicsPath.GetBounds();
                rotationTransform.RotateAt(Rotation, new PointF(bounds.X + (bounds.Width / 2f), bounds.Y + (bounds.Height / 2f)));
                graphicsPath.Transform(rotationTransform);
            }

            if (TextShadow != null) {
                DrawShadow(graphics, graphicsPath);
            }

            if (Outline > 0) {
                var pen = new Pen(OutlineColor, Outline) {
                    LineJoin = LineJoin.Round
                };
                graphics.DrawPath(pen, graphicsPath);
            }

            if (TextBrush == null) {
                TextBrush = new SolidBrush(TextColor);
            }

            graphics.FillPath(TextBrush, graphicsPath);
        }

        private void DrawShadow(Graphics graphics, GraphicsPath graphicsPath) {
            graphics.TranslateTransform(TextShadow.OffsetX, TextShadow.OffsetY);
            graphics.FillPath(new SolidBrush(TextShadow.Color), graphicsPath);
            graphics.ResetTransform();
        }
    }
}