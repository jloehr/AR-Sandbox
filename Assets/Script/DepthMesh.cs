using UnityEngine;
using System.Collections;
using AForge.Imaging.Filters;
using AForge.Imaging;

public class DepthMesh : MonoBehaviour
{
    public DepthWrapper KinectDepth;
    int KinectWidth = 320;
    int KinectHeight = 240;
    [HideInInspector]
    public Vector3[] newVertices;
    [HideInInspector]
    public Vector3[] newNormals;
    [HideInInspector]
    public Color32[] newColors;
    [HideInInspector]
    public Vector2[] newUV;
    [HideInInspector]
    public int[] newTriangles;
    Mesh MyMesh;

    public int Width = 240;
    public int Height = 240;
    public int OffsetX;
    public int OffsetY;
    public int MinDepthValue = 0;
    public int MaxDepthValue = short.MaxValue;
    public float MeshHeigth;


    short MinDepthValueBuffer;
    short MaxDepthValueBuffer;
    short[] DepthImage;
    short[] FilterdAndCroppedDepthImage;
    float[] FloatValues;

    int WidthBuffer;
    int HeightBuffer;

    System.Drawing.Bitmap Bitmap;
    GaussianBlur Blur = new GaussianBlur(4, 11);

    // Use this for initialization
    void Start()
    {
        WidthBuffer = Width;
        HeightBuffer = Height;

        MyMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = MyMesh;

        SetupArrays();
    }

    // Update is called once per frame
    void Update()
    {
        if (KinectDepth.pollDepth())
        {
            DepthImage = KinectDepth.depthImg;
            CheckArrays();
            CropImage();
            FilerImage();
            CalculateFloatValues();
            UpdateMesh();
        }
    }

    void CheckArrays()
    {
        if ((Width != WidthBuffer) || (Height != HeightBuffer))
        {
            SetupArrays();
            WidthBuffer = Width;
            HeightBuffer = Height;
        }
    }

    void SetupArrays()
    {
        FilterdAndCroppedDepthImage = new short[Width * Height];
        FloatValues = new float[Width * Height];
        newVertices = new Vector3[Width * Height];
        newNormals = new Vector3[Width * Height];
        newColors = new Color32[Width * Height];
        newUV = new Vector2[Width * Height];
        newTriangles = new int[(Width - 1) * (Height - 1) * 6];
        Bitmap = new System.Drawing.Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);

        Debug.Log(Width * Height);
        Debug.Log(newTriangles.Length);

        for (int H = 0; H < Height; H++)
        {
            for (int W = 0; W < Width; W++)
            {
                int Index = GetArrayIndex(W, H);
                newVertices[Index] = new Vector3(W, H, 0f);
                newNormals[Index] = new Vector3(0, 0, 1);
                newColors[Index] = new Color32(0, 0, 0, 255);
                newUV[Index] = new Vector2(W / (float)Width, H / (float)Height);

                if ((W != (Width - 1)) && (H != (Height - 1)))
                {
                    int TopLeft = Index;
                    int TopRight = Index + 1;
                    int BotLeft = Index + Width;
                    int BotRight = Index + 1 + Width;

                    int TrinagleIndex = W + H * (Width - 1);
                    newTriangles[TrinagleIndex * 6 + 0] = TopLeft;
                    newTriangles[TrinagleIndex * 6 + 1] = BotLeft;
                    newTriangles[TrinagleIndex * 6 + 2] = TopRight;
                    newTriangles[TrinagleIndex * 6 + 3] = BotLeft;
                    newTriangles[TrinagleIndex * 6 + 4] = BotRight;
                    newTriangles[TrinagleIndex * 6 + 5] = TopRight;
                }
            }
        }

        MyMesh.Clear();
        MyMesh.vertices = newVertices;
        MyMesh.normals = newNormals;
        MyMesh.colors32 = newColors;
        MyMesh.uv = newUV;
        MyMesh.triangles = newTriangles;
    }

    void CropImage()
    {
        for (int H = 0; H < Height; H++)
        {
            for (int W = 0; W < Width; W++)
            {
                int Index = GetArrayIndex(W, H);
                short Value = (short)GetImageValue(W, H);
                FilterdAndCroppedDepthImage[Index] = Value;
            }
        }
    }

    void FilerImage()
    {
        // Copy Image to Bitmap
        System.Drawing.Rectangle ImageBounds = new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
        System.Drawing.Imaging.ImageLockMode Mode = System.Drawing.Imaging.ImageLockMode.ReadWrite;
        System.Drawing.Imaging.PixelFormat Format = Bitmap.PixelFormat;
        System.Drawing.Imaging.BitmapData BitmapData = Bitmap.LockBits(ImageBounds, Mode, Format);

        System.IntPtr ptr = BitmapData.Scan0;

        System.Runtime.InteropServices.Marshal.Copy(FilterdAndCroppedDepthImage, 0, ptr, FilterdAndCroppedDepthImage.Length);

        Bitmap.UnlockBits(BitmapData);

        //Apply Filter
        Blur.ApplyInPlace(Bitmap);

        //Copy Bitmap back to Image
        BitmapData = Bitmap.LockBits(ImageBounds, Mode, Format);

        ptr = BitmapData.Scan0;
        System.Runtime.InteropServices.Marshal.Copy(ptr, FilterdAndCroppedDepthImage, 0, FilterdAndCroppedDepthImage.Length);

        Bitmap.UnlockBits(BitmapData);
    }

    void CalculateFloatValues()
    {
        for (int H = 0; H < Height; H++)
        {
            for (int W = 0; W < Width; W++)
            {
                int Index = GetArrayIndex(W, H);
                int ImageValue = FilterdAndCroppedDepthImage[Index];

                //Clamp Value

                if (ImageValue > MaxDepthValueBuffer)
                {
                    MaxDepthValueBuffer = (short)Mathf.Clamp(ImageValue, ImageValue, short.MaxValue);
                }

                if (ImageValue < MinDepthValueBuffer)
                {
                    MinDepthValueBuffer = (short)Mathf.Clamp(ImageValue, short.MinValue, ImageValue);
                }

                if (ImageValue > MaxDepthValue)
                {
                    ImageValue = MaxDepthValue;
                }

                if (ImageValue < MinDepthValue)
                {
                    ImageValue = MinDepthValue;
                }

                //Calculate
                float FloatValue = (ImageValue - MinDepthValue) / (float)(MaxDepthValue - MinDepthValue);
                FloatValues[Index] = FloatValue;
            }
        }

    }

    void UpdateMesh()
    {
        MinDepthValueBuffer = short.MaxValue;
        MaxDepthValueBuffer = short.MinValue;

        for (int H = 0; H < Height; H++)
        {
            for (int W = 0; W < Width; W++)
            {
                ProcessPixel(W, H);
            }
        }

        MyMesh.vertices = newVertices;
        MyMesh.colors32 = newColors;
        MyMesh.RecalculateNormals();
    }

    void ProcessPixel(int W, int H)
    {
        int Index = GetArrayIndex(W, H);
        float FloatValue = FloatValues[Index];

        //Calc Normal
        //newNormals[Index] = CalculateNormal(W, H, FloatValue);

        //Calc Position
        newVertices[Index].z = FloatValue * MeshHeigth;

        //Calc Color
        float FloatValueClamped = Mathf.Clamp01(FloatValue);
        byte ByteValue = (byte)Mathf.RoundToInt(FloatValue * byte.MaxValue);

        //0-127 = 0 :: 127- 255 = 0 - 255
        byte R = (byte)(Mathf.Clamp((ByteValue - 127) * 2, 0, 255));
        //0 = 0; 127 = 255; 255 = 0
        byte G = (byte)(127 + (Mathf.Sign(127 - ByteValue) * ByteValue / 2));
        byte B = (byte)(255 - Mathf.Clamp(ByteValue * 2, 0, 255));
        newColors[Index] = new Color32(R, G, B, 255);
    }

    int GetImageIndex(int W, int H)
    {
        int ImageW = OffsetX + W;
        int ImageH = OffsetY + H;

        if ((ImageW < 0) || (ImageW > KinectWidth) || (ImageH < 0) || (ImageH > KinectHeight))
        {
            return -1;
        }

        return ImageW + ImageH * KinectWidth;
    }

    int GetImageValue(int W, int H)
    {
        int Index = GetImageIndex(W, H);
        if (Index < 0)
        {
            return (int)short.MaxValue;
        }

        int Value = DepthImage[Index];

        if (Value == 0)
        {
            return (int)short.MaxValue;
        }
        else
        {
            return Value;
        }    
    }

    /* Not needed since Unity provides a Recalculate Normals Funktion
    Vector3 CalculateNormal(int W, int H, float VertexFloat)
    {
        int TopIndex = GetArrayIndex(W, H + 1);
        int RightIndex = GetArrayIndex(W + 1, H);
        int BottomIndex = GetArrayIndex(W, H - 1);
        int LeftIndex = GetArrayIndex(W - 1, H);

        Vector3 Normal = Vector3.zero;

        //Get TopLeft
        Normal += CalculateTriangleNormal(LeftIndex, -1, TopIndex, 1, false, VertexFloat);
        //Get TopRight
        Normal += CalculateTriangleNormal(RightIndex, 1, TopIndex, 1, true, VertexFloat);
        //Get BottomLeft
        Normal += CalculateTriangleNormal(LeftIndex, -1, BottomIndex, -1, false, VertexFloat);
        //Get BottomRight
        Normal += CalculateTriangleNormal(RightIndex, 1, BottomIndex, -1, true, VertexFloat);

        return Normal.normalized;
    }

    Vector3 CalculateTriangleNormal(int XIndex, int XOffset, int YIndex, int YOffset, bool Swapped, float VertexFloat)
    {
        if((XIndex < 0) || (YIndex < 0))
        {
            return Vector3.zero;
        }

        if((XIndex >= FloatValues.Length) || (YIndex >= FloatValues.Length))
        {
            Debug.Log("OutofRange: " + FloatValues.Length + " - " + XIndex + " : " + YIndex);
            Debug.Break();
        }

        Vector3 XVector = new Vector3(XOffset, 0, FloatValues[XIndex] - VertexFloat);
        Vector3 YVector = new Vector3(0, YOffset, FloatValues[YIndex] - VertexFloat);

        if(Swapped)
        {
            return Vector3.Cross(XVector, YVector).normalized;
        }
        else
        {
            return Vector3.Cross(YVector, XVector).normalized;
        }
    }
    */

    int GetArrayIndex(int W, int H)
    {
        if ((W < 0) || (W >= Width) || (H < 0) || (H >= Height))
        {
            return -1;
        }

        return W + H * Width;
    }

    int[] ShortToRGBA(short[] DepthImage)
    {
        int[] ImageData = new int[DepthImage.Length];

        for (int i = 0; i < DepthImage.Length; i++)
        {
            ImageData[i] = (int)((((int)DepthImage[i]) << 8) | 0x000000FF);
        }

        return ImageData;
    }

    int RGBAToShort(int Value)
    {
        return (Value >> 8);
    }
}
