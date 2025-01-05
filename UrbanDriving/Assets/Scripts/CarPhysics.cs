using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// A wheel can either be used to propel the car forward (motor), to steer the car (steerable), neither or both.
// Used to configure all-wheel drive, front-wheel drive and back-wheel drive vehicles
[Serializable]
struct Tire
{
    public Transform transform;
    public bool isSteerable;
    public bool isMotor;
}

public enum AutomaticGear {
    Neutral,
    Parking,
    Driving,
    Reverse,
    None
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarController))]
public class CarPhysics : MonoBehaviour
{
    private Rigidbody carRigidbody;
    private CarController carController;

    [Header("General Car Parameters")]
    [SerializeField]
    private Tire[] carTires;
    [SerializeField]
    private float wheelDiameter = 1.0f;
    [SerializeField]
    private float[] gearRatios;
    // The critical point of motor RPM before an automatic gear switch
    [SerializeField]
    private float minRPM = 2.0f;
    [SerializeField]
    private float maxRPM = 4.5f;
    [SerializeField]
    private float accelerationForce = 10f;
    [SerializeField]
    private float brakeForce = 1.0f;
    [SerializeField]
    private float engineBaseForce = 1000f;
    
    // Suspension force is the vertical force of the wheel bouncing of the ground
    [Header("Suspension Force Parameters")]
    [SerializeField]
    bool enableSuspensionForce = true;
    [SerializeField]
    private float springStrength;
    [SerializeField]
    private float damping;
    // The distance at which the spring will be in its resting position
    // (calculated in-game as the distance between the bottom of the car's hull and bottom of the wheel)
    [SerializeField]
    private float restDistance = 0.0f;

    // Lateral force of the wheels sliding against the ground
    [Header("Steering Force Parameters")]
    [SerializeField]
    bool enableSteeringForce = true;
    [SerializeField]
    private AnimationCurve tireGripCurve;
    [SerializeField]
    private float tireRotationAngle;
    [SerializeField]
    private float steeringStrengthCoefficient = 1.0f;

    // Axial force of the wheels gripping the ground as they rotate
    [Header("Axial Force Parameters")]
    [SerializeField]
    bool enableAxialForce = true;
    [SerializeField]
    private float axialFriction = 0.5f;

    private AutomaticGear autoGearMode = AutomaticGear.Parking;
    private int currentGear = 0;
    private float currentRpm = 0.0f;
    private float targetRpm = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
    }

    // FixedUpdate is called at fixed intervals
    void FixedUpdate()
    {
        carRigidbody.WakeUp(); 
        targetRpm =  GetCarSpeed(false) / (wheelDiameter * (float)Math.PI * gearRatios[currentGear]);
        targetRpm = Mathf.Clamp(targetRpm, 0.0f, 10.0f);
        currentRpm = Mathf.Lerp(currentRpm, targetRpm, 0.3f);

        // Calculates the forces to be applied at next physics calculation
        foreach(Tire tire in carTires)
        {
            carRigidbody.AddForceAtPosition(CalculateForces(tire, carController.GetGasValue(), carController.GetBrakeValue()), tire.transform.position);

            // If the wheel is a steerable wheel, rotate it at an angle proportional to the steering wheel's rotation
            if (tire.isSteerable)
            {
                tire.transform.localEulerAngles = new Vector3(0f, tireRotationAngle * carController.GetWheelRelativeTurn(), 0f);
            }
        }
        if (autoGearMode == AutomaticGear.Reverse)
        {
            currentGear = 1;
        }
        else if (currentRpm > maxRPM && currentGear < gearRatios.Length - 1)
        {
            currentGear++;
        }
        else if (currentRpm < minRPM && currentGear > 0)
        {
            currentGear--;
        }
    }

    private Vector3 CalculateForces(Tire tire, float gasValue, float brakeValue)
    {
        float suspensionForce = 0.0f;
        float steeringForce = 0.0f;
        float axialForce = 0.0f;
        RaycastHit tireHit;
        if (Physics.Raycast(tire.transform.position, -tire.transform.up, out tireHit, restDistance))
        {
            Vector3 tireWorldVelocity = carRigidbody.GetPointVelocity(tire.transform.position);
            Vector3 tireRelativeVelocity = new Vector3(
                Vector3.Dot(tireWorldVelocity, tire.transform.right),
                Vector3.Dot(tireWorldVelocity, tire.transform.up),
                Vector3.Dot(tireWorldVelocity, tire.transform.forward));

            float offset = restDistance - tireHit.distance;
            suspensionForce = (offset * springStrength) - (tireRelativeVelocity.y * damping);
            steeringForce = -tireRelativeVelocity.x * GetTireGrip(tireRelativeVelocity) * steeringStrengthCoefficient;
            
            axialForce = -tireRelativeVelocity.z * axialFriction;
            if (autoGearMode == AutomaticGear.Parking)
            {
                axialForce -= tireRelativeVelocity.z * brakeForce;
            }
            else
            {
                axialForce -= tireRelativeVelocity.z * brakeForce * brakeValue;
            }
            if (tire.isMotor && autoGearMode != AutomaticGear.Parking && autoGearMode != AutomaticGear.Neutral)
            {
                if (autoGearMode == AutomaticGear.Reverse)
                {
                    axialForce -= accelerationForce * gasValue * gearRatios[currentGear];
                }
                else
                {
                    axialForce += accelerationForce * gasValue * gearRatios[currentGear];
                }
            }
        }

        Vector3 totalForce = Vector3.zero;
        if (enableSuspensionForce) totalForce += suspensionForce * tire.transform.up;
        if (enableSteeringForce) totalForce += steeringForce * tire.transform.right;
        if (enableAxialForce) totalForce += axialForce * tire.transform.forward;

        return totalForce;
    }

    private float GetTireGrip(Vector3 relativeVelocity)
    {
        Vector3 horizontalVelocity = new Vector3(relativeVelocity.x, 0f, relativeVelocity.z);
        if (horizontalVelocity.magnitude < 0.0001f) return 0f;

        float relativeDrift = Math.Abs(relativeVelocity.x) / horizontalVelocity.magnitude;
        return Math.Clamp(tireGripCurve.Evaluate(relativeDrift), 0.0f, 1.0f);
    }

    public float GetCarSpeed(bool inKilometers)
    {
        if (inKilometers)
        {
            return carRigidbody.velocity.magnitude * 3.6f;
        }
        else
        {
            return carRigidbody.velocity.magnitude;
        }
    }

    public float GetWheelSpeed()
    {
        return 0.0f;
    }

    public AutomaticGear GetAutomaticGear()
    {
        return autoGearMode;
    }

    public void SetAutomaticGear(AutomaticGear gear)
    {
        autoGearMode = gear;
    }

    public float GetRPM()
    {
        return currentRpm;
    }
}
