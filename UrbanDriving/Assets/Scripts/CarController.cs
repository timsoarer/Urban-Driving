using System.Collections;
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField]
    private Transform steeringWheel;
    [SerializeField]
    private XRKnob wheelKnob;
    [SerializeField]
    private InputActionReference leftHandTrigger;
    [SerializeField]
    private InputActionReference rightHandTrigger;
    [SerializeField]
    private float steerTurnLimit = 150.0f;
    [SerializeField]
    private bool isInVR = false;
    private float wheelAngle;

    void Awake()
    {
        if (!isInVR)
        {
            wheelAngle = steeringWheel.localEulerAngles.x;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInVR)
        {
            if (Input.GetKey(KeyCode.A))
            {
                steeringWheel.localEulerAngles = new Vector3(wheelAngle, 0f, -steerTurnLimit);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                steeringWheel.localEulerAngles = new Vector3(wheelAngle, 0f, steerTurnLimit);
            }
            else
            {
                steeringWheel.localEulerAngles = new Vector3(wheelAngle, 0f, 0f);
            }
        }
    }

    public float GetWheelRelativeTurn()
    {
        if (!isInVR)
        {
            if (steeringWheel.localEulerAngles.z > 180f)
            {
                return (steeringWheel.localEulerAngles.z -360f) / steerTurnLimit;
            }
            return steeringWheel.localEulerAngles.z / steerTurnLimit;
        }
        else
        {
            return (wheelKnob.value - 0.5f) * 2;
        }
    }

    public float GetGasValue()
    {
        return rightHandTrigger.action.ReadValue<float>() - leftHandTrigger.action.ReadValue<float>();
    }
}
