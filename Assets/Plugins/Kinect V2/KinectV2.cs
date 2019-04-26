using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class KinectV2 : DepthSensorBase
{
    private Windows.Kinect.KinectSensor sensor;
    private DepthFrameReader reader;
    private ushort[] data;
    private int width;
    private int heigth;

    public override int Width => width;

    public override int Height => heigth;

    public override ushort[] depthImage
    {
        get { return data; }
    }

    public override bool pollDepth()
    {
        if (reader != null)
        {
            var frame = reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(data);
                frame.Dispose();
                frame = null;

                return true;
            }
        }

        return false;
    }

    private void Start()
    {
        sensor = Windows.Kinect.KinectSensor.GetDefault();

        if (sensor != null)
        {
            reader = sensor.DepthFrameSource.OpenReader();
            data = new ushort[sensor.DepthFrameSource.FrameDescription.LengthInPixels];
            width = sensor.DepthFrameSource.FrameDescription.Width;
            heigth = sensor.DepthFrameSource.FrameDescription.Height;

            if (!sensor.IsOpen)
                sensor.Open();
        }
        else
        {
            Debug.LogErrorFormat("Failed to acquire Kinect Sensor!");
        }
    }

    private void OnDestroy()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        if (sensor != null)
        {
            if (sensor.IsOpen)
                sensor.Close();

            sensor = null;
        }
    }
}
