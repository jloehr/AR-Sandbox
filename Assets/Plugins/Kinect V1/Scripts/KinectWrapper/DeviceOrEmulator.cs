using UnityEngine;
using System.Collections;

public class DeviceOrEmulator : MonoBehaviour {
	
	public KinectSensor device;
	public KinectEmulator emulator;
	
	public bool useEmulator = false;
	
	// Use this for initialization
	void Start () {
		if(useEmulator){
			emulator.enabled = true;
		}
		else {
			device.enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public Kinect.KinectInterface getKinect() {
		if(useEmulator){
			return emulator;
		}
		return device;
	}
}
