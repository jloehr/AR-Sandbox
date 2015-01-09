using UnityEngine;
using System.Collections;
using Kinect;

public class SkeletonWrapper : MonoBehaviour {
	
	public DeviceOrEmulator devOrEmu;
	private Kinect.KinectInterface kinect;
	
	private bool updatedSkeleton = false;
	private bool newSkeleton = false;
	
	[HideInInspector]
	public Kinect.NuiSkeletonTrackingState[] players;
	[HideInInspector]
	public int[] trackedPlayers;
	[HideInInspector]
	public Vector3[,] bonePos;
	[HideInInspector]
	public Vector3[,] rawBonePos;
	[HideInInspector]
	public Vector3[,] boneVel;
	[HideInInspector]
	public Quaternion[,] boneLocalOrientation;
	[HideInInspector]
	public Quaternion[,] boneAbsoluteOrientation;
	
	public Kinect.NuiSkeletonPositionTrackingState[,] boneState;	
	private System.Int64 ticks;
	private float deltaTime;
	
	private Matrix4x4 kinectToWorld;
	public Matrix4x4 flipMatrix;
	
	// Use this for initialization
	void Start () {
		kinect = devOrEmu.getKinect();
		players = new Kinect.NuiSkeletonTrackingState[Kinect.Constants.NuiSkeletonCount];
		trackedPlayers = new int[Kinect.Constants.NuiSkeletonMaxTracked];
		trackedPlayers[0] = -1;
		trackedPlayers[1] = -1;
		bonePos = new Vector3[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		rawBonePos = new Vector3[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		boneVel = new Vector3[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		
		boneState = new Kinect.NuiSkeletonPositionTrackingState[2,(int)Kinect.NuiSkeletonPositionIndex.Count];
		boneLocalOrientation = new Quaternion[2, (int)Kinect.NuiSkeletonPositionIndex.Count];
		boneAbsoluteOrientation = new Quaternion[2, (int)Kinect.NuiSkeletonPositionIndex.Count];
		
		//create the transform matrix that converts from kinect-space to world-space
		Matrix4x4 trans = new Matrix4x4();
		trans.SetTRS( new Vector3(-kinect.getKinectCenter().x,
		                          kinect.getSensorHeight()-kinect.getKinectCenter().y,
		                          -kinect.getKinectCenter().z),
		             Quaternion.identity, Vector3.one );
		Matrix4x4 rot = new Matrix4x4();
		Quaternion quat = new Quaternion();
		double theta = Mathf.Atan((kinect.getLookAt().y+kinect.getKinectCenter().y-kinect.getSensorHeight()) / (kinect.getLookAt().z + kinect.getKinectCenter().z));
		float kinectAngle = (float)(theta * (180 / Mathf.PI));
		quat.eulerAngles = new Vector3(-kinectAngle, 0, 0);
		rot.SetTRS( Vector3.zero, quat, Vector3.one);

		//final transform matrix offsets the rotation of the kinect, then translates to a new center
		kinectToWorld = flipMatrix*trans*rot;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void LateUpdate () {
		updatedSkeleton = false;
		newSkeleton = false;
	}
	
	/// <summary>
	/// First call per frame checks if there is a new skeleton frame and updates,
	/// returns true if there is new data
	/// Subsequent calls do nothing have the same return as the first call.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	public bool pollSkeleton () {
		if (!updatedSkeleton)
		{
			updatedSkeleton = true;
			if (kinect.pollSkeleton())
			{
				newSkeleton = true;
				System.Int64 cur = kinect.getSkeleton().liTimeStamp;
				System.Int64 diff = cur - ticks;
				ticks = cur;
				deltaTime = diff / (float)1000;
				processSkeleton();
			}
		}
		return newSkeleton;
	}
	
	private void processSkeleton () {
		int[] tracked = new int[Kinect.Constants.NuiSkeletonMaxTracked];
		tracked[0] = -1;
		tracked[1] = -1;
		int trackedCount = 0;
		//update players
		for (int ii = 0; ii < Kinect.Constants.NuiSkeletonCount; ii++)
		{
			players[ii] = kinect.getSkeleton().SkeletonData[ii].eTrackingState;
			if (players[ii] == Kinect.NuiSkeletonTrackingState.SkeletonTracked)
			{
				tracked[trackedCount] = ii;
				trackedCount++;
			}
		}
		//this should really use trackingID instead of index, but for now this is fine
		switch (trackedCount)
		{
		case 0:
			trackedPlayers[0] = -1;
			trackedPlayers[1] = -1;
			break;
		case 1:
			//last frame there were no players: assign new player to p1
			if (trackedPlayers[0] < 0 && trackedPlayers[1] < 0)
				trackedPlayers[0] = tracked[0];
			//last frame there was one player, keep that player in the same spot
			else if (trackedPlayers[0] < 0) 
				trackedPlayers[1] = tracked[0];
			else if (trackedPlayers[1] < 0)
				trackedPlayers[0] = tracked[0];
			//there were two players, keep the one with the same index (if possible)
			else
			{
				if (tracked[0] == trackedPlayers[0])
					trackedPlayers[1] = -1;
				else if (tracked[0] == trackedPlayers[1])
					trackedPlayers[0] = -1;
				else
				{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = -1;
				}
			}
			break;
		case 2:
			//last frame there were no players: assign new players to p1 and p2
			if (trackedPlayers[0] < 0 && trackedPlayers[1] < 0)
			{
				trackedPlayers[0] = tracked[0];
				trackedPlayers[1] = tracked[1];
			}
			//last frame there was one player, keep that player in the same spot
			else if (trackedPlayers[0] < 0)
			{
				if (trackedPlayers[1] == tracked[0])
					trackedPlayers[0] = tracked[1];
				else{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = tracked[1];
				}
			}
			else if (trackedPlayers[1] < 0)
			{
				if (trackedPlayers[0] == tracked[1])
					trackedPlayers[1] = tracked[0];
				else{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = tracked[1];
				}
			}
			//there were two players, keep the one with the same index (if possible)
			else
			{
				if (trackedPlayers[0] == tracked[1] || trackedPlayers[1] == tracked[0])
				{
					trackedPlayers[0] = tracked[1];
					trackedPlayers[1] = tracked[0];
				}
				else
				{
					trackedPlayers[0] = tracked[0];
					trackedPlayers[1] = tracked[1];
				}
			}
			break;
		}
		
		//update the bone positions, velocities, and tracking states)
		for (int player = 0; player < 2; player++)
		{
			//print(player + ", " +trackedPlayers[player]);
			if (trackedPlayers[player] >= 0)
			{
				for (int bone = 0; bone < (int)Kinect.NuiSkeletonPositionIndex.Count; bone++)
				{
					Vector3 oldpos = bonePos[player,bone];
					
					bonePos[player,bone] = kinectToWorld.MultiplyPoint3x4(kinect.getSkeleton().SkeletonData[trackedPlayers[player]].SkeletonPositions[bone]);
					//bonePos[player,bone] = kinectToWorld.MultiplyPoint3x4(bonePos[player, bone]);
					rawBonePos[player, bone] = kinect.getSkeleton().SkeletonData[trackedPlayers[player]].SkeletonPositions[bone];
					
					
					Kinect.NuiSkeletonBoneOrientation[] or = kinect.getBoneOrientations(kinect.getSkeleton().SkeletonData[trackedPlayers[player]]);
					boneLocalOrientation[player,bone] = or[bone].hierarchicalRotation.rotationQuaternion.GetQuaternion();
					boneAbsoluteOrientation[player,bone] = or[bone].absoluteRotation.rotationQuaternion.GetQuaternion();
					
					//print("index " + bone + ", start" + (int)or[bone].startJoint + ", end" + (int)or[bone].endJoint);
					
					boneVel[player,bone] = (bonePos[player,bone] - oldpos) / deltaTime;
					boneState[player,bone] = kinect.getSkeleton().SkeletonData[trackedPlayers[player]].eSkeletonPositionTrackingState[bone];
					//print(kinect.getSkeleton().SkeletonData[player].Position.z);
				}
			}
		}
	}
	
}
