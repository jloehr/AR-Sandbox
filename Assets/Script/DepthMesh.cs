using UnityEngine;
using System.Collections;

public class DepthMesh : MonoBehaviour {

    public DepthWrapper KinectDepth;
    uint KinectWidth = 320;
    uint KinectHeight = 240;
    uint Width;
    uint Height;

    public Vector3[] newVertices;
    public Vector3[] newNormals;
    public Color32[] newColors;
    public Vector2[] newUV;
    public int[] newTriangles;
    Mesh MyMesh;

    Kaliko.ImageLibrary.KalikoImage Image;

	// Use this for initialization
    void Start()
    {
        Image = new Kaliko.ImageLibrary.KalikoImage(320, 240);

        float Aspect = (float)KinectWidth / (float)KinectHeight;
        //ushort.MaxValue

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
        if(KinectDepth.pollDepth())
        {
            short[] Img = KinectDepth.depthImg;
            for (uint H = 0; H < Height; H++)
            {
                for (uint W = 0; W < Width; W++)
                {
                    uint ImgIndex = W + H * 320;
                    uint VertexIndex = W + H * Width;
                    newVertices[VertexIndex].z = -1f * (Img[ImgIndex] / 32f);
                    

                    byte R = (byte)((Mathf.Clamp(Img[ImgIndex] / 32, 127, 255) - 127) * 2);
                    byte G = (byte)(Img[ImgIndex] / 32);
                    byte B = (byte)(255 - (Mathf.Clamp(Img[ImgIndex]/ 32, 0, 127) * -2));
                    newColors[VertexIndex] = new Color32(R, G, B, 255);
                }
            }
            
            MyMesh.vertices = newVertices;
            MyMesh.colors32 = newColors;
        }
	}
}
