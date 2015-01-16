namespace Kaliko.ImageLibrary.Filters {
    using System;

    public class Kernel
    {
        public float[] Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Kernel(int width, int height, float[] data) {
            if (data.Length < width*height) {
                throw new ArgumentException("Array should not be smaller than width*height");
            }

            Data = data;
            Width = width;
            Height = height;
        }

        public float[] GetKernel() {
            var data = new float[Data.Length];
            Array.Copy(Data, 0, data, 0, Data.Length);
            return data;
        }
    }
}
