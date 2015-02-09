using UnityEngine;
using System.Collections;

public class CameraCalibration : MonoBehaviour {


    private Camera camera;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
	
        if(Input.GetKey(KeyCode.Q))
        {
            camera.orthographicSize++;
        }

        if (Input.GetKey(KeyCode.E))
        {
            camera.orthographicSize--;
        }

        if (Input.GetKey(KeyCode.W))
        {
            camera.transform.position += Vector3.up;
        }

        if (Input.GetKey(KeyCode.S))
        {
            camera.transform.position += Vector3.down;
        }

        if (Input.GetKey(KeyCode.A))
        {
            camera.transform.position += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            camera.transform.position += Vector3.right;
        }
	}
}
