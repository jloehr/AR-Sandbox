using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using Kinect;

public class KinectSensor : MonoBehaviour, KinectInterface {
	//make KinectSensor a singleton (sort of)
	private static KinectInterface instance;
    public static KinectInterface Instance
    {
        get
        {
            if (instance == null)
                throw new Exception("There needs to be an active instance of the KinectSensor component.");
            return instance;
        }
        private set
        { instance = value; }
    }
	
	/// <summary>
	/// how high (in meters) off the ground is the sensor
	/// </summary>
	public float sensorHeight;
	/// <summary>
	/// where (relative to the ground directly under the sensor) should the kinect register as 0,0,0
	/// </summary>
	public Vector3 kinectCenter;
	/// <summary>
	/// what point (relative to kinectCenter) should the sensor look at
	/// </summary>
	public Vector4 lookAt;
	
	/// <summary>
	/// Variables used to pass to smoothing function. Values are set to default based on Action in Motion's Research
	/// </summary>
	public float smoothing =0.5f;	
	public float correction=0.5f;
	public float prediction=0.5f;
	public float jitterRadius=0.05f;
	public float maxDeviationRadius=0.04f;
	public bool enableNearMode = false;
	
	public NuiSkeletonFlags skeltonTrackingMode;
	
	
	/// <summary>
	///variables used for updating and accessing depth data 
	/// </summary>
	private bool updatedSkeleton = false;
	private bool newSkeleton = false;
	[HideInInspector]
	private NuiSkeletonFrame skeletonFrame = new NuiSkeletonFrame() { SkeletonData = new NuiSkeletonData[6] };
	/// <summary>
	///variables used for updating and accessing depth data 
	/// </summary>
	private bool updatedColor = false;
	private bool newColor = false;
	[HideInInspector]
	private Color32[] colorImage;
	/// <summary>
	///variables used for updating and accessing depth data 
	/// </summary>
	private bool updatedDepth = false;
	private bool newDepth = false;
	[HideInInspector]
	private short[] depthPlayerData;
	
	//image stream handles for the kinect
	private IntPtr colorStreamHandle;
	private IntPtr depthStreamHandle;
	[HideInInspector]
	private NuiTransformSmoothParameters smoothParameters = new NuiTransformSmoothParameters();
	
	float KinectInterface.getSensorHeight() {
		return sensorHeight;
	}
	Vector3 KinectInterface.getKinectCenter() {
		return kinectCenter;
	}
	Vector4 KinectInterface.getLookAt() {
		return lookAt;
	}
	
	
	void Awake()
	{
		if (KinectSensor.instance != null)
		{
			Debug.Log("There should be only one active instance of the KinectSensor component at at time.");
            throw new Exception("There should be only one active instance of the KinectSensor component at a time.");
		}
		try
		{
			// The MSR Kinect DLL (native code) is going to load into the Unity process and stay resident even between debug runs of the game.  
            // So our component must be resilient to starting up on a second run when the Kinect DLL is already loaded and
            // perhaps even left in a running state.  Kinect does not appear to like having NuiInitialize called when it is already initialized as
            // it messes up the internal state and stops functioning.  It is resilient to having Shutdown called right before initializing even if it
            // hasn't been initialized yet.  So calling this first puts us in a good state on a first or second run.
			// However, calling NuiShutdown before starting prevents the image streams from being read, so if you want to use image data
			// (either depth or RGB), comment this line out.
			//NuiShutdown();
			
			int hr = NativeMethods.NuiInitialize(NuiInitializeFlags.UsesDepthAndPlayerIndex | NuiInitializeFlags.UsesSkeleton | NuiInitializeFlags.UsesColor);
            if (hr != 0)
			{
            	throw new Exception("NuiInitialize Failed.");
			}
			
			hr = NativeMethods.NuiSkeletonTrackingEnable(IntPtr.Zero, skeltonTrackingMode);
			if (hr != 0)
			{
				throw new Exception("Cannot initialize Skeleton Data.");
			}
			
			depthStreamHandle = IntPtr.Zero;
			hr = NativeMethods.NuiImageStreamOpen(NuiImageType.DepthAndPlayerIndex, NuiImageResolution.resolution320x240, 0, 2, IntPtr.Zero, ref depthStreamHandle);
			//Debug.Log(depthStreamHandle);
			if (hr != 0)
			{
				throw new Exception("Cannot open depth stream.");
			}
			
			colorStreamHandle = IntPtr.Zero;
			hr = NativeMethods.NuiImageStreamOpen(NuiImageType.Color, NuiImageResolution.resolution640x480, 0, 2, IntPtr.Zero, ref colorStreamHandle);
			//Debug.Log(colorStreamHandle);
			if (hr != 0)
			{
				throw new Exception("Cannot open color stream.");
			}
			colorImage = new Color32[640*480];
			
			double theta = Mathf.Atan((lookAt.y+kinectCenter.y-sensorHeight) / (lookAt.z + kinectCenter.z));
			long kinectAngle = (long)(theta * (180 / Mathf.PI));
			NativeMethods.NuiCameraSetAngle(kinectAngle);
			
			DontDestroyOnLoad(gameObject);
			KinectSensor.Instance = this;
			NativeMethods.NuiSetDeviceStatusCallback(new NuiStatusProc(), IntPtr.Zero);
		}
		
		catch (Exception e)
		{
			Debug.Log(e.Message);
		}
	}
	
	void LateUpdate()
	{
		updatedSkeleton = false;
		newSkeleton = false;
		updatedColor = false;
		newColor = false;
		updatedDepth = false;
		newDepth = false;
	}
	/// <summary>
	///The first time in each frame that it is called, poll the kinect for updated skeleton data and return
	///true if there is new data. Subsequent calls do nothing and return the same value.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/> : is there new data this frame
	/// </returns>
	bool KinectInterface.pollSkeleton()
	{
		if (!updatedSkeleton)
		{
			updatedSkeleton = true;
			int hr = NativeMethods.NuiSkeletonGetNextFrame(100,ref skeletonFrame);
			if(hr == 0)
			{
				newSkeleton = true;
			}
			smoothParameters.fSmoothing = smoothing;
			smoothParameters.fCorrection = correction;
			smoothParameters.fJitterRadius = jitterRadius;
			smoothParameters.fMaxDeviationRadius = maxDeviationRadius;
			smoothParameters.fPrediction = prediction;
			hr = NativeMethods.NuiTransformSmooth(ref skeletonFrame,ref smoothParameters);
		}
		return newSkeleton;
	}
	
	NuiSkeletonFrame KinectInterface.getSkeleton(){
		return skeletonFrame;
	}
	
	/// <summary>
	/// Get all bones orientation based on the skeleton passed in
	/// </summary>
	/// <returns>
	/// Bone Orientation in struct of NuiSkeletonBoneOrientation, quarternion and matrix
	/// </returns>
	NuiSkeletonBoneOrientation[] KinectInterface.getBoneOrientations(Kinect.NuiSkeletonData skeletonData){
		NuiSkeletonBoneOrientation[] boneOrientations = new NuiSkeletonBoneOrientation[(int)(NuiSkeletonPositionIndex.Count)];
		NativeMethods.NuiSkeletonCalculateBoneOrientations(ref skeletonData, boneOrientations);
		return boneOrientations;
	}
	
	/// <summary>
	///The first time in each frame that it is called, poll the kinect for updated color data and return
	///true if there is new data. Subsequent calls do nothing and return the same value.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/> : is there new data this frame
	/// </returns>
	bool KinectInterface.pollColor()
	{
		if (!updatedColor)
		{
			updatedColor = true;
			IntPtr imageFramePtr = IntPtr.Zero;
		
			int hr = NativeMethods.NuiImageStreamGetNextFrame(colorStreamHandle, 100, ref imageFramePtr);
			if (hr == 0){
				newColor = true;
				NuiImageFrame imageFrame = (NuiImageFrame)Marshal.PtrToStructure(imageFramePtr, typeof(NuiImageFrame));
				
				INuiFrameTexture frameTexture = (INuiFrameTexture)Marshal.GetObjectForIUnknown(imageFrame.pFrameTexture);
				
				NuiLockedRect lockedRectPtr = new NuiLockedRect();
				IntPtr r = IntPtr.Zero;
				
				frameTexture.LockRect(0,ref lockedRectPtr,r,0);
				colorImage = extractColorImage(lockedRectPtr);
				
				hr = NativeMethods.NuiImageStreamReleaseFrame(colorStreamHandle, imageFramePtr);
			}
		}
		return newColor;
	}
	
	Color32[] KinectInterface.getColor(){
		return colorImage;
	}
	
	/// <summary>
	///The first time in each frame that it is called, poll the kinect for updated depth (and player) data and return
	///true if there is new data. Subsequent calls do nothing and return the same value.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/> : is there new data this frame
	/// </returns>
	bool KinectInterface.pollDepth()
	{
		if (!updatedDepth)
		{
			updatedDepth = true;
			IntPtr imageFramePtr = IntPtr.Zero;
			
			/* KK Addition*/
			/// <summary>
			/// Sets near mode - move this into the Awake () function
			/// if you do not need to constantly change between near and far mode
			/// current organization is this way to allow for rapid changes IF they're required
			/// and to allow for experimentation with the 2 modes
			/// </summary>
			if (enableNearMode)
			{
				NativeMethods.NuiImageStreamSetImageFrameFlags 
										(depthStreamHandle, NuiImageStreamFlags.EnableNearMode);
				//test = NativeMethods.NuiImageStreamSetImageFrameFlags 
				//						(depthStreamHandle, NuiImageStreamFlags.TooFarIsNonZero);
			}
			
			else
			{
				NativeMethods.NuiImageStreamSetImageFrameFlags
										 (depthStreamHandle, NuiImageStreamFlags.None);
			}
			
			int hr = NativeMethods.NuiImageStreamGetNextFrame(depthStreamHandle, 100, ref imageFramePtr);
			if (hr == 0){
				newDepth = true;
				NuiImageFrame imageFrame;
				imageFrame = (NuiImageFrame)Marshal.PtrToStructure(imageFramePtr, typeof(NuiImageFrame));
				
				INuiFrameTexture frameTexture = (INuiFrameTexture)Marshal.GetObjectForIUnknown(imageFrame.pFrameTexture);
				
				NuiLockedRect lockedRectPtr = new NuiLockedRect();
				IntPtr r = IntPtr.Zero;
				
				frameTexture.LockRect(0,ref lockedRectPtr,r,0);
				depthPlayerData = extractDepthImage(lockedRectPtr);
				
				frameTexture.UnlockRect(0);
				hr = NativeMethods.NuiImageStreamReleaseFrame(depthStreamHandle, imageFramePtr);
			}
		}
		return newDepth;
	}
	
	short[] KinectInterface.getDepth(){
		return depthPlayerData;
	}
	
	private Color32[] extractColorImage(NuiLockedRect buf)
	{
		int totalPixels = 640*480;
		Color32[] colorBuf = colorImage;
		ColorBuffer cb = (ColorBuffer)Marshal.PtrToStructure(buf.pBits,typeof(ColorBuffer));
		
		for (int pix = 0; pix < totalPixels; pix++)
		{
			colorBuf[pix].r = cb.pixels[pix].r;
			colorBuf[pix].g = cb.pixels[pix].g;
			colorBuf[pix].b = cb.pixels[pix].b;

		}
		return colorBuf;
	}
	
	private short[] extractDepthImage(NuiLockedRect lockedRect)
	{
		DepthBuffer db = (DepthBuffer)Marshal.PtrToStructure(lockedRect.pBits,typeof(DepthBuffer));
		
		return db.pixels;
	}
	
	void OnApplicationQuit()
	{
		NativeMethods.NuiShutdown();
	}
	
}
