using UnityEngine;
using System.Collections;

using System.Drawing;
using AForge.Imaging;
using System.Collections.Generic;
using AForge;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.IO;

public class Marker : MonoBehaviour {

    UnmanagedImage grayImage = null;
    int frameskip = 0;
    int glyphSize = 5;
    const int stepSize = 3;
    UnmanagedImage image = null;
    Bitmap bitmapTest = null;
    List<IntPoint> Corners = new List<IntPoint>();

    public DeviceOrEmulator devOrEmu;
    private Kinect.KinectInterface kinect;
	// Use this for initialization
	void Start () {

        bitmapTest = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
	}
	
	// Update is called once per frame
	void Update () {
        kinect = devOrEmu.getKinect();
        if (kinect.pollColor() == true)
        {
            Color32[] colorKinect =  kinect.getColor();

            for (int i = 0; i < 640; i++)
            {
                for (int j = 0; j < 480; j++)
                {
                    int index = i * 480 + j;
                    int a = colorKinect[index].a;
                    int r = colorKinect[index].r;
                    int g = colorKinect[index].g;
                    int b = colorKinect[index].b;
                    bitmapTest.SetPixel(i, j, System.Drawing.Color.FromArgb(a,r,g,b));
                }
            }

            image = UnmanagedImage.FromManagedImage(bitmapTest);
            //var bitmap = image.ToManagedImage();

            if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                grayImage = image;
            }
            else
            {
                if (grayImage != null)
                    grayImage.Dispose();
                grayImage = UnmanagedImage.Create(image.Width, image.Height,
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
               Grayscale.CommonAlgorithms.BT709.Apply(image, grayImage);
            }
            scan_code();

        }
        
    }



        public byte[,] Recognize(UnmanagedImage image, System.Drawing.Rectangle rect,
    out float confidence)
        {
            int glyphStartX = rect.Left;
            int glyphStartY = rect.Top;

            int glyphWidth = rect.Width;
            int glyphHeight = rect.Height;

            // glyph's cell size
            int cellWidth = glyphWidth / glyphSize;
            int cellHeight = glyphHeight / glyphSize;

            // allow some gap for each cell, which is not scanned
            int cellOffsetX = (int)(cellWidth * 0.2);
            int cellOffsetY = (int)(cellHeight * 0.2);

            // cell's scan size
            int cellScanX = (int)(cellWidth * 0.6);
            int cellScanY = (int)(cellHeight * 0.6);
            int cellScanArea = cellScanX * cellScanY;

            // summary intensity for each glyph's cell
            int[,] cellIntensity = new int[glyphSize, glyphSize];

            //unsafe
            //{
                int stride = image.Stride;

                //byte* srcBase = (byte*)image.ImageData.ToPointer() +
                //    (glyphStartY + cellOffsetY) * stride +
                //    glyphStartX + cellOffsetX;
                //byte* srcLine;
                //byte* src;

             int srcBase =
                    (glyphStartY + cellOffsetY) * stride +
                    glyphStartX + cellOffsetX;
            
                // for all glyph's rows
                for (int gi = 0; gi < glyphSize; gi++)
                {
                    // for all lines in the row
                    for (int y = 0; y < cellScanY; y++)
                    {
                        int IndexY = glyphStartY + gi * cellScanY + y;

                        // for all glyph columns
                        for (int gj = 0; gj < glyphSize; gj++)
                        {
                            // for all pixels in the column
                            for (int x = 0; x < cellScanX; x++)
                            {
                                int IndexX = glyphStartX + gj * cellScanX + x;
                                List<IntPoint> Bar = new List<IntPoint>();
                                Bar.Add(new IntPoint(IndexX, IndexY));
                                byte[] foo = image.Collect8bppPixelValues(Bar);



                                cellIntensity[gi, gj] += foo[0];
                            }
                        }

                       // srcLine += stride;
                    }
                }
            //}

            // calculate value of each glyph's cell and set
            // glyphs' confidence to minim value of cell's confidence
            byte[,] glyphValues = new byte[glyphSize, glyphSize];
            confidence = 1f;

            for (int gi = 0; gi < glyphSize; gi++)
            {
                for (int gj = 0; gj < glyphSize; gj++)
                {
                    float fullness = (float)
                        (cellIntensity[gi, gj] / 255) / cellScanArea;
                    float conf = (float)System.Math.Abs(fullness - 0.5) + 0.5f;

                    glyphValues[gi, gj] = (byte)((fullness > 0.5f) ? 1 : 0);

                    if (conf < confidence)
                        confidence = conf;
                }
            }

            return glyphValues;
        }


        // Calculate average brightness difference between pixels outside and
        // inside of the object bounded by specified left and right edge
        private float CalculateAverageEdgesBrightnessDifference(
            List<IntPoint> leftEdgePoints,
            List<IntPoint> rightEdgePoints,
            UnmanagedImage image)
        {
            // create list of points, which are a bit on the left/right from edges
            List<IntPoint> leftEdgePoints1 = new List<IntPoint>();
            List<IntPoint> leftEdgePoints2 = new List<IntPoint>();
            List<IntPoint> rightEdgePoints1 = new List<IntPoint>();
            List<IntPoint> rightEdgePoints2 = new List<IntPoint>();

            int tx1, tx2, ty;
            int widthM1 = image.Width - 1;

            for (int k = 0; k < leftEdgePoints.Count; k++)
            {
                tx1 = leftEdgePoints[k].X - stepSize;
                tx2 = leftEdgePoints[k].X + stepSize;
                ty = leftEdgePoints[k].Y;

                leftEdgePoints1.Add(new IntPoint(
                    (tx1 < 0) ? 0 : tx1, ty));
                leftEdgePoints2.Add(new IntPoint(
                    (tx2 > widthM1) ? widthM1 : tx2, ty));

                tx1 = rightEdgePoints[k].X - stepSize;
                tx2 = rightEdgePoints[k].X + stepSize;
                ty = rightEdgePoints[k].Y;

                rightEdgePoints1.Add(new IntPoint(
                    (tx1 < 0) ? 0 : tx1, ty));
                rightEdgePoints2.Add(new IntPoint(
                    (tx2 > widthM1) ? widthM1 : tx2, ty));
            }

            // collect pixel values from specified points
            byte[] leftValues1 = image.Collect8bppPixelValues(leftEdgePoints1);
            byte[] leftValues2 = image.Collect8bppPixelValues(leftEdgePoints2);
            byte[] rightValues1 = image.Collect8bppPixelValues(rightEdgePoints1);
            byte[] rightValues2 = image.Collect8bppPixelValues(rightEdgePoints2);

            // calculate average difference between pixel values from outside of
            // the shape and from inside
            float diff = 0;
            int pixelCount = 0;

            for (int k = 0; k < leftEdgePoints.Count; k++)
            {
                if (rightEdgePoints[k].X - leftEdgePoints[k].X > stepSize * 2)
                {
                    diff += (leftValues1[k] - leftValues2[k]);
                    diff += (rightValues2[k] - rightValues1[k]);
                    pixelCount += 2;
                }
            }

            return diff / pixelCount;
        }
        private void scan_code()
        {
            List<IntPoint> TempCorners = new List<IntPoint>();

                    // 2 - Edge detection
                    DifferenceEdgeDetector edgeDetector = new DifferenceEdgeDetector();
                    UnmanagedImage edgesImage = edgeDetector.Apply(grayImage);

                    // 3 - Threshold edges
                    Threshold thresholdFilter = new Threshold(40);
                    thresholdFilter.ApplyInPlace(edgesImage);

                    // create and configure blob counter
                    BlobCounter blobCounter = new BlobCounter();

                    blobCounter.MinHeight = 32;
                    blobCounter.MinWidth = 32;
                    blobCounter.FilterBlobs = true;
                    blobCounter.ObjectsOrder = ObjectsOrder.Size;

                    // 4 - find all stand alone blobs
                    blobCounter.ProcessImage(edgesImage);
                    Blob[] blobs = blobCounter.GetObjectsInformation();

                    // 5 - check each blob
                    for (int i = 0, n = blobs.Length; i < n; i++)
                    {
                        // ...
                        List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                        List<IntPoint> corners = null;

                        // does it look like a quadrilateral ?
                        SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
                        if (shapeChecker.IsQuadrilateral(edgePoints, out corners))
                        {
                            TempCorners.AddRange(corners);

                            // ...
                            // get edge points on the left and on the right side
                            List<IntPoint> leftEdgePoints, rightEdgePoints;
                            blobCounter.GetBlobsLeftAndRightEdges(blobs[i],
                                out leftEdgePoints, out rightEdgePoints);

                            // calculate average difference between pixel values fro+m outside of the
                            // shape and from inside
                            float diff = CalculateAverageEdgesBrightnessDifference(
                                leftEdgePoints, rightEdgePoints, grayImage);

                            // check average difference, which tells how much outside is lighter than
                            // inside on the average
                            if (diff > 20)
                            {

                                QuadrilateralTransformation quadrilateralTransformation =
                               new QuadrilateralTransformation(corners, 100, 100);
                                UnmanagedImage glyphImage = quadrilateralTransformation.Apply(grayImage);


                                //// otsu thresholding hier fehler

                                OtsuThreshold otsuThresholdFilter = new OtsuThreshold();
                                otsuThresholdFilter.ApplyInPlace(glyphImage);

                                image = glyphImage;



                                //// recognize raw glyph
                                float confidence;
                                //code geändert
                                byte[,] LeftUpMarker = new byte[5, 5];
                                LeftUpMarker[0, 0] = 0;
                                LeftUpMarker[0, 1] = 0;
                                LeftUpMarker[0, 2] = 0;
                                LeftUpMarker[0, 3] = 0;
                                LeftUpMarker[0, 4] = 0;

                                LeftUpMarker[1, 0] = 0;
                                LeftUpMarker[1, 1] = 0;
                                LeftUpMarker[1, 2] = 1;
                                LeftUpMarker[1, 3] = 0;
                                LeftUpMarker[1, 4] = 0;

                                LeftUpMarker[2, 0] = 0;
                                LeftUpMarker[2, 1] = 1;
                                LeftUpMarker[2, 2] = 0;
                                LeftUpMarker[2, 3] = 1;
                                LeftUpMarker[2, 4] = 0;

                                LeftUpMarker[3, 0] = 0;
                                LeftUpMarker[3, 1] = 0;
                                LeftUpMarker[3, 2] = 1;
                                LeftUpMarker[3, 3] = 0;
                                LeftUpMarker[3, 4] = 0;

                                LeftUpMarker[4, 0] = 0;
                                LeftUpMarker[4, 1] = 0;
                                LeftUpMarker[4, 2] = 0;
                                LeftUpMarker[4, 3] = 0;
                                LeftUpMarker[4, 4] = 0;


                                byte[,] RightUpMarker = new byte[5, 5];
                                RightUpMarker[0, 0] = 0;
                                RightUpMarker[0, 1] = 0;
                                RightUpMarker[0, 2] = 0;
                                RightUpMarker[0, 3] = 0;
                                RightUpMarker[0, 4] = 0;

                                RightUpMarker[1, 0] = 0;
                                RightUpMarker[1, 1] = 1;
                                RightUpMarker[1, 2] = 0;
                                RightUpMarker[1, 3] = 1;
                                RightUpMarker[1, 4] = 0;

                                RightUpMarker[2, 0] = 0;
                                RightUpMarker[2, 1] = 0;
                                RightUpMarker[2, 2] = 0;
                                RightUpMarker[2, 3] = 0;
                                RightUpMarker[2, 4] = 0;

                                RightUpMarker[3, 0] = 0;
                                RightUpMarker[3, 1] = 1;
                                RightUpMarker[3, 2] = 0;
                                RightUpMarker[3, 3] = 1;
                                RightUpMarker[3, 4] = 0;

                                RightUpMarker[4, 0] = 0;
                                RightUpMarker[4, 1] = 0;
                                RightUpMarker[4, 2] = 0;
                                RightUpMarker[4, 3] = 0;
                                RightUpMarker[4, 4] = 0;


                                byte[,] LeftDownMarker = new byte[5, 5];
                                LeftDownMarker[0, 0] = 0;
                                LeftDownMarker[0, 1] = 0;
                                LeftDownMarker[0, 2] = 0;
                                LeftDownMarker[0, 3] = 0;
                                LeftDownMarker[0, 4] = 0;

                                LeftDownMarker[1, 0] = 0;
                                LeftDownMarker[1, 1] = 0;
                                LeftDownMarker[1, 2] = 1;
                                LeftDownMarker[1, 3] = 0;
                                LeftDownMarker[1, 4] = 0;

                                LeftDownMarker[2, 0] = 0;
                                LeftDownMarker[2, 1] = 1;
                                LeftDownMarker[2, 2] = 1;
                                LeftDownMarker[2, 3] = 1;
                                LeftDownMarker[2, 4] = 0;

                                LeftDownMarker[3, 0] = 0;
                                LeftDownMarker[3, 1] = 0;
                                LeftDownMarker[3, 2] = 1;
                                LeftDownMarker[3, 3] = 0;
                                LeftDownMarker[3, 4] = 0;

                                LeftDownMarker[4, 0] = 0;
                                LeftDownMarker[4, 1] = 0;
                                LeftDownMarker[4, 2] = 0;
                                LeftDownMarker[4, 3] = 0;
                                LeftDownMarker[4, 4] = 0;


                                byte[,] ReightDownMarker = new byte[5, 5];
                                ReightDownMarker[0, 0] = 0;
                                ReightDownMarker[0, 1] = 0;
                                ReightDownMarker[0, 2] = 0;
                                ReightDownMarker[0, 3] = 0;
                                ReightDownMarker[0, 4] = 0;

                                ReightDownMarker[1, 0] = 0;
                                ReightDownMarker[1, 1] = 1;
                                ReightDownMarker[1, 2] = 1;
                                ReightDownMarker[1, 3] = 1;
                                ReightDownMarker[1, 4] = 0;

                                ReightDownMarker[2, 0] = 0;
                                ReightDownMarker[2, 1] = 1;
                                ReightDownMarker[2, 2] = 0;
                                ReightDownMarker[2, 3] = 1;
                                ReightDownMarker[2, 4] = 0;

                                ReightDownMarker[3, 0] = 0;
                                ReightDownMarker[3, 1] = 1;
                                ReightDownMarker[3, 2] = 1;
                                ReightDownMarker[3, 3] = 1;
                                ReightDownMarker[3, 4] = 0;

                                ReightDownMarker[4, 0] = 0;
                                ReightDownMarker[4, 1] = 0;
                                ReightDownMarker[4, 2] = 0;
                                ReightDownMarker[4, 3] = 0;
                                ReightDownMarker[4, 4] = 0;


                                

                                byte[,] glyphValues = Recognize(glyphImage,
                                    new System.Drawing.Rectangle(0, 0, glyphImage.Width, glyphImage.Height), out confidence);

                                Boolean bool_LeftUpMarkerMarker = true;

                                for (int l = 0; l < 5; l++)
                                {
                                    for (int m = 0; m < 5; m++)
                                    {
                                        if (LeftUpMarker[l, m] != glyphValues[l, m])
                                        {
                                            bool_LeftUpMarkerMarker = false;
                                            break;
                                        }
                                    }
                                }

                                if (bool_LeftUpMarkerMarker)
                                {
                                    Debug.Log("Marker erkannt");

                                }


                                Boolean bool_RightUpMarker = true;

                                for (int l = 0; l < 5; l++)
                                {
                                    for (int m = 0; m < 5; m++)
                                    {
                                        if (RightUpMarker[l, m] != glyphValues[l, m])
                                        {
                                            bool_RightUpMarker = false;
                                            break;
                                        }
                                    }
                                }

                                if (bool_RightUpMarker)
                                {

                                    Debug.Log("Marker erkannt");

                                }


                                Boolean bool_LeftDownMarker = true;

                                for (int l = 0; l < 5; l++)
                                {
                                    for (int m = 0; m < 5; m++)
                                    {
                                        if (LeftDownMarker[l, m] != glyphValues[l, m])
                                        {
                                            bool_LeftDownMarker = false;
                                            break;
                                        }
                                    }
                                }

                                if (bool_LeftDownMarker)
                                {

                                    Debug.Log("Marker erkannt");
                                }


                                Boolean bool_ReightDownMarker = true;

                                for (int l = 0; l < 5; l++)
                                {
                                    for (int m = 0; m < 5; m++)
                                    {
                                        if (ReightDownMarker[l, m] != glyphValues[l, m])
                                        {
                                            bool_ReightDownMarker = false;
                                            break;
                                        }
                                    }
                                }

                                if (bool_ReightDownMarker)
                                {

                                    Debug.Log("Marker erkannt");

                                }

                            }
                        }
                    }

            if(TempCorners.Count > 0)
            {
                Corners = TempCorners;
            }

                }


        void OnDrawGizmos()
        {
            Gizmos.color = UnityEngine.Color.red;

            for(int i = 0; i < Corners.Count - 2; i++)
            {
                if((i % 4) == 3)
                {
                    continue;
                }

                IntPoint Point1 = Corners[i];
                IntPoint Point2 = Corners[i + 1];
                Gizmos.DrawLine(new Vector3(Point1.X, Point1.Y, 0) / 2, new Vector3(Point2.X, Point2.Y, 0) / 2);
                Gizmos.DrawWireSphere(new Vector3(Point1.X, Point1.Y, 0) / 2, 1);
            }
        }
        }
	


