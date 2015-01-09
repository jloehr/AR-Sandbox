using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Kinect;

public class KinectRecorder : MonoBehaviour {
	
	public DeviceOrEmulator devOrEmu;
	private KinectInterface kinect;
	
	public string outputFile = "Assets/Kinect/Recordings/playback";
	
	
	private bool isRecording = false;
	private ArrayList currentData = new ArrayList();
	
	
	//add by lxjk
	private int fileCount = 0;
	//end lxjk
	
	
	// Use this for initialization
	void Start () {
		kinect = devOrEmu.getKinect();
	}
	
	// Update is called once per frame
	void Update () {
		if(!isRecording){
			if(Input.GetKeyDown(KeyCode.F10)){
				StartRecord();
			}
		} else {
			if(Input.GetKeyDown(KeyCode.F10)){
				StopRecord();
			}
			if (kinect.pollSkeleton()){
				currentData.Add(kinect.getSkeleton());
			}
		}
	}
	
	void StartRecord() {
		isRecording = true;
		Debug.Log("start recording");
	}
	
	void StopRecord() {
		isRecording = false;
		//edit by lxjk
		string filePath = outputFile+fileCount.ToString();
		FileStream output = new FileStream(@filePath,FileMode.Create);
		//end lxjk
		BinaryFormatter bf = new BinaryFormatter();
		
		SerialSkeletonFrame[] data = new SerialSkeletonFrame[currentData.Count];
		for(int ii = 0; ii < currentData.Count; ii++){
			data[ii] = new SerialSkeletonFrame((NuiSkeletonFrame)currentData[ii]);
		}
		bf.Serialize(output, data);
		output.Close();
		fileCount++;
		Debug.Log("stop recording");
	}
}
