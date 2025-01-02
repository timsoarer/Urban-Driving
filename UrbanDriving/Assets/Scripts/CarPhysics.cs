using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarController))]
public class CarPhysics : MonoBehaviour
{
    private Rigidbody carRigidbody;
    private CarController carController;
    [SerializeField]
    private Tire[] carTires;
    
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
    [Header("Acceleration Force Parameters")]
    [SerializeField]
    bool enableAccelerationForce = true;
    [SerializeField]
    private float axialFriction = 0.5f;
    [SerializeField]
    private float accelerationForce = 10f;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
    }

    // FixedUpdate is called at fixed intervals
    void FixedUpdate()
    {
        // Calculates the forces to be applied at next physics calculation
        foreach(Tire tire in carTires)
        {
            carRigidbody.AddForceAtPosition(CalculateForces(tire.transform), tire.transform.position);
            
            // If the wheel is a motor wheel, add an acceleration force equal to the strength of the gas pedal
            if (tire.isMotor) {
                carRigidbody.AddForceAtPosition(accelerationForce * carController.GetGasValue() * tire.transform.forward, tire.transform.position);
            }
            // If the wheel is a steerable wheel, rotate it at an angle proportional to the steering wheel's rotation
            if (tire.isSteerable)
            {
                tire.transform.localEulerAngles = new Vector3(0f, tireRotationAngle * carController.GetWheelRelativeTurn(), 0f);
            }
        }
    }

    private Vector3 CalculateForces(Transform tire)
    {
        float suspensionForce = 0.0f;
        float steeringForce = 0.0f;
        float frictionForce = 0.0f;
        RaycastHit tireHit;
        if (Physics.Raycast(tire.position, -tire.up, out tireHit, restDistance))
        {
            Vector3 tireWorldVelocity = carRigidbody.GetPointVelocity(tire.position);
            Vector3 tireRelativeVelocity = new Vector3(
                Vector3.Dot(tireWorldVelocity, tire.right),
                Vector3.Dot(tireWorldVelocity, tire.up),
                Vector3.Dot(tireWorldVelocity, tire.forward));

            float offset = restDistance - tireHit.distance;
            suspensionForce = (offset * springStrength) - (tireRelativeVelocity.y * damping);
            steeringForce = -tireRelativeVelocity.x * GetTireGrip(tireRelativeVelocity) * steeringStrengthCoefficient;
            frictionForce = -tireRelativeVelocity.z * axialFriction;
        }

        Vector3 totalForce = Vector3.zero;
        if (enableSuspensionForce) totalForce += suspensionForce * tire.up;
        if (enableSteeringForce) totalForce += steeringForce * tire.right;
        if (enableAccelerationForce) totalForce += frictionForce * tire.forward;

        return totalForce;
    }

    private float GetTireGrip(Vector3 relativeVelocity)
    {
        Vector3 horizontalVelocity = new Vector3(relativeVelocity.x, 0f, relativeVelocity.z);
        if (horizontalVelocity.magnitude < 0.0001f) return 0f;

        float relativeDrift = Math.Abs(relativeVelocity.x) / horizontalVelocity.magnitude;
        return Math.Clamp(tireGripCurve.Evaluate(relativeDrift), 0.0f, 1.0f);
    }
}
