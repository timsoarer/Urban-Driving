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
    private InputActionReference gasPedal;
    [SerializeField]
    private InputActionReference wheelTurn;
    
    private bool isInVR = false;
    private float wheelAngle;

    void Awake()
    {
        isInVR = UnityEngine.XR.XRSettings.enabled;
        Debug.Log(isInVR);
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
            wheelKnob.value = (wheelTurn.action.ReadValue<float>() + 1.0f) / 2;
        }
    }

    public float GetWheelRelativeTurn()
    {
        return (wheelKnob.value - 0.5f) * 2;
    }

    public float GetGasValue()
    {
        return gasPedal.action.ReadValue<float>();
    }
}
