using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    [Header("Suspension Force Parameters")]
    [SerializeField]
    bool enableSuspensionForce = true;
    [SerializeField]
    private float springStrength;
    [SerializeField]
    private float damping;
    [SerializeField]
    private float restDistance = 0.0f;

    [Header("Steering Force Parameters")]
    [SerializeField]
    bool enableSteeringForce = true;
    [SerializeField]
    private AnimationCurve tireGripCurve;
    [SerializeField]
    private float tireRotationAngle;
    [SerializeField]
    private float steeringStrengthCoefficient = 1.0f;

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
        foreach(Tire tire in carTires)
        {
            carRigidbody.AddForceAtPosition(CalculateForces(tire.transform), tire.transform.position);
            
            // Temporary placeholder code
            if (tire.isMotor) {
                carRigidbody.AddForceAtPosition(accelerationForce * carController.GetGasValue() * tire.transform.forward, tire.transform.position);
            }
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
