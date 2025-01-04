using System.Collections;
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // An XRKnob is a script that allows an object to be rotated in VR using the controller grips. The rotation is measured via the "value" parameter.
    [SerializeField]
    private XRKnob wheelKnob;
    [SerializeField]
    private InputActionReference gasPedal;
    [SerializeField]
    private InputActionReference wheelTurn;
    
    private bool isInVR = false;

    void Awake()
    {
        isInVR = UnityEngine.XR.XRSettings.enabled;
        Debug.Log(isInVR);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Set the wheel rotation via buttons only when not in VR.
        if (!isInVR)
        {
            wheelKnob.value = (wheelTurn.action.ReadValue<float>() + 1.0f) / 2;
        }
    }

    // Gets the wheel's rotation and converts it from a value between [0; 1] to a value between [-1; 1] 
    public float GetWheelRelativeTurn()
    {
        return (wheelKnob.value - 0.5f) * 2;
    }

    // Gets the strength of the gas pedal press
    public float GetGasValue()
    {
        return gasPedal.action.ReadValue<float>();
    }
}
