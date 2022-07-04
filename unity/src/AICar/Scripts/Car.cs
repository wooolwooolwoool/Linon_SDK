using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    [System.NonSerialized] public float remoteMotorTorque;
    [System.NonSerialized] public float remoteSteeringAngle;

    // Start is called before the first frame update
    void Start()
    {
        remoteMotorTorque = 0;
        remoteSteeringAngle = 0;
    }

    public void FixedUpdate()
    {
        float motor = maxMotorTorque * (Input.GetAxis("Vertical") + remoteMotorTorque);
        float steering = maxSteeringAngle * (Input.GetAxis("Horizontal") + remoteSteeringAngle);

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                if (motor > 0)
                {
                    axleInfo.leftWheel.motorTorque = motor;
                    axleInfo.rightWheel.motorTorque = motor;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.brakeTorque = 0;
                }
                else
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.rightWheel.motorTorque = 0;
                    axleInfo.leftWheel.brakeTorque = motor * 5 * (-1);
                    axleInfo.rightWheel.brakeTorque = motor * 5 * (-1);
                }
            }
        }
    }
}
[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; 
    public bool steering; 
}