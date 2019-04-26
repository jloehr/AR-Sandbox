using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KinectV1 : DepthSensorBase
{
    private DepthWrapper kinectSensor;


    public override int Width => 320;

    public override int Height => 240;

    public override ushort[] depthImage
    {
        get {
            return Array.ConvertAll(kinectSensor.depthImg, (value) => (ushort)value);
        }
    }

    public override bool pollDepth()
    {
        if (kinectSensor != null)
            return kinectSensor.pollDepth();

        return false;
    }

    private void Start()
    {
        kinectSensor = GetComponent<DepthWrapper>();

        if (kinectSensor == null)
            Debug.LogError("Kinect V1 Depth Wrapper not found!");
    }
}
