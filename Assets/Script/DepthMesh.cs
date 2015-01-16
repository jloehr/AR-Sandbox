using UnityEngine;
using System.Collections;

public class DepthMesh : MonoBehaviour {

    public DepthWrapper KinectDepth;
    uint KinectWidth = 320;
    uint KinectHeight = 240;
    uint Width;
    uint Height;
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

    public int MinValue = 0;
    public int MaxValue = short.MaxValue;

    Kaliko.ImageLibrary.Filters.GaussianBlurFilter BlurFilter = new Kaliko.ImageLibrary.Filters.GaussianBlurFilter(2);

	// Use this for initialization
    void Start()
    {
        float Aspect = (float)KinectWidth / (float)KinectHeight;
        float SquarePixels = ushort.MaxValue * Aspect;
        float FloatWidth = Mathf.Sqrt(SquarePixels);
        float FloatHeight = FloatWidth / Aspect;
        Width = (uint)Mathf.FloorToInt(FloatWidth);
        Height = (uint)Mathf.FloorToInt(FloatHeight);

        Debug.Log("W: " + Width + " - H: " + Height);
            // W * H = ushort.MaxValue
            // W = Aspect * H
        //
        // H = Max / W
        // W = Aspect * max / W
        // W² = Aspect * Max
        // W = sqrt( Aspect * MAx)
        // H = W / Aspect



        newVertices = new Vector3[Width * Height];
        newNormals = new Vector3[Width * Height];
        newColors = new Color32[Width * Height];
        newUV = new Vector2[Width * Height];
        newTriangles = new int[(Width - 1) * (Height - 1) * 6];

        for (uint H = 0; H < Height; H++)
        {
            for (uint W = 0; W < Width; W++)
            {
                uint Index = W + H * Width;
                newVertices[Index] = new Vector3(W, H, 0f);
                newNormals[Index] = new Vector3(0, 0, 1);
                newColors[Index] = new Color32(0, 0, 0, 255);
                newUV[Index] = new Vector2(W / (float)Width, H / (float)Height);

                if((W != (Width - 1)) && (H != (Height - 1)))
                {
                    uint TopLeft = Index;
                    uint TopRight = Index + 1;
                    uint BotLeft = Index + Width;
                    uint BotRight = Index + 1 + Width;

                    uint TrinagleIndex = W + H * (Width - 1);
                    newTriangles[TrinagleIndex * 6 + 0] = (int)TopLeft;
                    newTriangles[TrinagleIndex * 6 + 1] = (int)BotLeft;
                    newTriangles[TrinagleIndex * 6 + 2] = (int)TopRight;
                    newTriangles[TrinagleIndex * 6 + 3] = (int)BotLeft;
                    newTriangles[TrinagleIndex * 6 + 4] = (int)BotRight;
                    newTriangles[TrinagleIndex * 6 + 5] = (int)TopRight;
                }
            }
        }


        MyMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = MyMesh;
        MyMesh.vertices = newVertices;
        MyMesh.normals = newNormals;
        MyMesh.colors32 = newColors;
        MyMesh.uv = newUV;
        MyMesh.triangles = newTriangles;
	}
	
	// Update is called once per frame
	void Update () {
        short MinValueBuffer = short.MaxValue;
        short MaxValueBuffer = short.MinValue;

        if(KinectDepth.pollDepth())
        {
            short[] Img = KinectDepth.depthImg;
            int[] ImageData = new int[Img.Length];

            for (int i = 0; i < Img.Length; i++)
            {
                ImageData[i] = (int)((((int)Img[i]) << 8) | 0x000000FF);
            }

            Kaliko.ImageLibrary.KalikoImage Image = new Kaliko.ImageLibrary.KalikoImage((int)KinectWidth, (int)KinectHeight);
            Image.IntArray = ImageData;
            //
            BlurFilter.Run(Image);
            //Image.Resize((int)Width, (int)Height);
            ImageData = Image.IntArray;
            //Debug.Log(ImageData);

            for (uint H = 0; H < Height; H++)
            {
                for (uint W = 0; W < Width; W++)
                {
                    uint ImgIndex = W + H * KinectWidth;
                    uint VertexIndex = W + H * Width;
                    uint Value = (uint)(ImageData[ImgIndex] >> 8);

                    if (Value > MaxValueBuffer)
                    {
                        MaxValueBuffer = (short)Mathf.Clamp(Value, Value, short.MaxValue);
                    }

                    if ((Value < MinValueBuffer) && (Value != 0))
                    {
                        MinValueBuffer = (short)Mathf.Clamp(Value, short.MinValue, Value);
                    }

                    if(Value == 0)
                    {
                        Value = (uint)short.MaxValue;
                    }

                    if(Value > MaxValue)
                    {
                        Value = (uint)MaxValue;
                    }

                    if(Value < MinValue)
                    {
                        Value = (uint)MinValue;
                    }

                    float FloatValue = (Value - MinValue) / (float)(MaxValue - MinValue);
                    float FloatValueClamped = Mathf.Clamp01(FloatValue);
                    byte ByteValue = (byte)Mathf.RoundToInt(FloatValue * byte.MaxValue);


                    newVertices[VertexIndex].z = FloatValue * 100;

                    //0-127 = 0 :: 127- 255 = 0 - 255
                    byte R = (byte)(Mathf.Clamp((ByteValue - 127) * 2, 0, 255));
                    //0 = 0; 127 = 255; 255 = 0
                    byte G = (byte)(127 + (Mathf.Sign(127 - ByteValue) * ByteValue / 2));
                    byte B = (byte)(255 - Mathf.Clamp(ByteValue * 2, 0, 255));
                    newColors[VertexIndex] = new Color32(R, G, B, 255);
                }
            }
            
            MyMesh.vertices = newVertices;
            MyMesh.colors32 = newColors;
        }

        Debug.Log(MinValueBuffer + " - " + MaxValueBuffer);
	}
}
