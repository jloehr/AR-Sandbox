using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Level of indirection for the depth image,
/// provides:
/// -a frames of depth image (no player information),
/// -an array representing which players are detected,
/// -a segmentation image for each player,
/// -bounds for the segmentation of each player.
/// </summary>
public class DepthWrapper: MonoBehaviour {
	
	public DeviceOrEmulator devOrEmu;
	private Kinect.KinectInterface kinect;
	
	private struct frameData
	{
		public short[] depthImg;
		public bool[] players;
		public bool[,] segmentation;
		public int[,] bounds;
	}
	
	public int storedFrames = 1;
	
	private bool updatedSeqmentation = false;
	private bool newSeqmentation = false;
	
	private Queue frameQueue;
	
	/// <summary>
	/// Depth image for the latest frame
	/// </summary>
	[HideInInspector]
	public short[] depthImg;
	/// <summary>
	/// players[i] true iff i has been detected in the frame
	/// </summary>
	[HideInInspector]
	public bool[] players;
	/// <summary>
	/// Array of segmentation images [player, pixel]
	/// </summary>
	[HideInInspector]
	public bool[,] segmentations;
	/// <summary>
	/// Array of bounding boxes for each player (left, right, top, bottom)
	/// </summary>
	[HideInInspector]
	//right,left,up,down : but the image is fliped horizontally.
	public int[,] bounds;
	
	// Use this for initialization
	void Start () {
		kinect = devOrEmu.getKinect();
		//allocate space to store the data of storedFrames frames.
		frameQueue = new Queue(storedFrames);
		for(int ii = 0; ii < storedFrames; ii++){	
			frameData frame = new frameData();
			frame.depthImg = new short[320 * 240];
			frame.players = new bool[Kinect.Constants.NuiSkeletonCount];
			frame.segmentation = new bool[Kinect.Constants.NuiSkeletonCount,320*240];
			frame.bounds = new int[Kinect.Constants.NuiSkeletonCount,4];
			frameQueue.Enqueue(frame);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void LateUpdate()
	{
		updatedSeqmentation = false;
		newSeqmentation = false;
	}
	/// <summary>
	/// First call per frame checks if there is a new depth image and updates,
	/// returns true if there is new data
	/// Subsequent calls do nothing have the same return as the first call.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	public bool pollDepth()
	{
		//Debug.Log("" + updatedSeqmentation + " " + newSeqmentation);
		if (!updatedSeqmentation)
		{
			updatedSeqmentation = true;
			if (kinect.pollDepth())
			{
				newSeqmentation = true;
				frameData frame = (frameData)frameQueue.Dequeue();
				depthImg = frame.depthImg;
				players = frame.players;
				segmentations = frame.segmentation;
				bounds = frame.bounds;
				frameQueue.Enqueue(frame);
				processDepth();
			}
		}
		return newSeqmentation;
	}
	
	private void processDepth()
	{
		for(int player = 0; player < Kinect.Constants.NuiSkeletonCount; player++)
		{
			//clear players
			players[player] = false;
			//clear old segmentation images
			for(int ii = 0; ii < 320 * 240; ii++)
			{
				segmentations[player,ii] = false;
			}
			//clear old bounds
			for(int ii = 0; ii < 4; ii++)
			{
				bounds[player,ii] = 0;
			}
		}
		for(int ii = 0; ii < 320 * 240; ii++)
		{
			//get x and y coords
			int xx = ii % 320;
			int yy = ii / 320;
			//extract the depth and player
			depthImg[ii] = (short)(kinect.getDepth()[ii] >> 3);
			int player = (kinect.getDepth()[ii] & 0x07) - 1;
			if (player > 0)
			{
				if (!players[player])
				{
					players[player] = true;
					segmentations[player,ii] = true;
					bounds[player,0] = xx;
					bounds[player,1] = xx;
					bounds[player,2] = yy;
					bounds[player,3] = yy;
				}
				else
				{
					segmentations[player,ii] = true;
					bounds[player,0] = Mathf.Min(bounds[player,0],xx);
					bounds[player,1] = Mathf.Max(bounds[player,1],xx);
					bounds[player,2] = Mathf.Min(bounds[player,2],yy);
					bounds[player,3] = Mathf.Max(bounds[player,3],yy);
				}
			}
		}
	}
}
