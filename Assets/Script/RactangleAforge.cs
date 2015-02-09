using UnityEngine;
using System.Collections;
using AForge.Imaging;
using System.Collections.Generic;
using AForge;
using System.Drawing;
using AForge.Math.Geometry;
using AForge.Video;
using AForge.Video.Kinect;

public class RactangleAforge : MonoBehaviour {

	// Use this for initialization
    DepthMesh MeshScript;



	void Start () {
        MeshScript = GetComponent<DepthMesh>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
          /*  Debug.Log("Calcutalting!");
            int[] DepthImage = MeshScript.FilterdAndCroppedDepthImage;
            Bitmap bitmap = new Bitmap(MeshScript.Width, MeshScript.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte ImageValue = (byte)(DepthImage[x * bitmap.Height + y] / 255);
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(ImageValue, ImageValue, ImageValue));
                }
            }
                        // create instance of blob counter
                        BlobCounter blobCounter = new BlobCounter();
                        // process input image
                        blobCounter.ProcessImage(bitmap);
                        // get information about detected objects
                        Blob[] blobs = blobCounter.GetObjectsInformation();

                        //System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
                        //Pen bluePen = new Pen(System.Drawing.Color.Blue, 2);
                        //// check each object and draw circle around objects, which
                        //// are recognized as circles
                        //for (int i = 0, n = blobs.Length; i < n; i++)
                        //{
                        //    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                        //    List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);

                        //    g.DrawPolygon(bluePen, ToPointsArray(corners));
                        //}

                        //bluePen.Dispose();
                        //g.Dispose();

                        SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
                        Pen redPen = new Pen(System.Drawing.Color.Blue, 2);

                        for (int i = 0, n = blobs.Length; i < n; i++)
                        {
                            List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                            List<IntPoint> corners;

                            if (shapeChecker.IsQuadrilateral(edgePoints, out corners))
                            {
                                foreach(IntPoint Point in corners)
                                {
                                    Debug.Log(Point.X + " - " + Point.Y);
                                }
                                //g.DrawPolygon(redPen, ToPointsArray(corners));
                            }
                        }
        */
           }
 
	}
    private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
    {
        System.Drawing.Point[] array = new System.Drawing.Point[points.Count];

        for (int i = 0, n = points.Count; i < n; i++)
        {
            array[i] = new System.Drawing.Point(points[i].X, points[i].Y);
        }

        return array;
    }

}
