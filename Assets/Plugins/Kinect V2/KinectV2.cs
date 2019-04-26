using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class KinectV2 : DepthSensorBase
{
    private Windows.Kinect.KinectSensor sensor;
    private DepthFrameReader reader;
    private ushort[] data;

    public override int Width
    {
        get
        {
            if (sensor != null)
                return sensor.DepthFrameSource.FrameDescription.Width;

            return 0;
        }
    }

    public override int Height
    {
        get
        {
            if (sensor != null)
                return sensor.DepthFrameSource.FrameDescription.Height;

            return 0;
        }
    }

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
        }
    }
}
