using UnityEngine;
using System.Collections;

public class SharkMovement : MonoBehaviour {

    public float TargetDistanceTreshold;
    public float MovementSpeed;
    public float TurnRate;

    [HideInInspector]
    public SharkController SharkController;

    private Vector3 Target;

	// Use this for initialization
	void Start () {
        Target = SharkController.GetRandomLocation();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 Direction = Target - transform.position;

        CheckForTarget(Direction.magnitude);

        //Turning
        Vector3 Forward = transform.rotation * Vector3.forward;
        transform.Rotate(Forward, TurnRate * Time.deltaTime);


        //Movement
        transform.Translate(Vector3.up * MovementSpeed * Time.deltaTime);

        //Turning
        //Vector3 Forward = transform.rotation * Vector3.up;
        //Quaternion Rotation = Quaternion.FromToRotation(Forward, Direction);
        //float Angle;
        //Vector3 RotationAxis;
        //Rotation.ToAngleAxis(out Angle, out RotationAxis);
        
//        Debug.Log(Angle);
        //Movment
        //float Speed = MovementSpeed;
        //Vector3 Movement = Forward * Speed * Time.deltaTime;

        //transform.Rotate(RotationToTarget.eulerAngles);
        //transform.Translate(Movement);
	}

    void CheckForTarget(float Distance)
    {
        if (Distance < TargetDistanceTreshold)
        {
            Target = SharkController.GetRandomLocation();
        }
    }
}
