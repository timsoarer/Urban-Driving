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
public class CarPhysics : MonoBehaviour
{
    private Rigidbody carRigidbody;
    [SerializeField]
    private Tire[] carTires;
    
    [Header("Suspension Force Parameters")]
    [SerializeField]
    private float springStrength;
    [SerializeField]
    private float damping;
    [SerializeField]
    private float restDistance = 0.0f;

    [Header("Steering Force Parameters")]
    [SerializeField]
    private AnimationCurve tireGripCurve;
    [SerializeField]
    private float tireRotationAngle;
    [SerializeField]
    private float steeringStrengthCoefficient = 1.0f;

    [Header("Acceleration Force Parameters")]
    [SerializeField]
    private float axialFriction = 0.5f;
    [SerializeField]
    private float accelerationForce = 10f;

    // Start is called before the first frame update
    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    // FixedUpdate is called at fixed intervals
    void FixedUpdate()
    {
        foreach(Tire tire in carTires)
        {
            carRigidbody.AddForceAtPosition(CalculateForces(tire.transform), tire.transform.position);
            
            // Temporary placeholder code
            if (tire.isMotor) {
                if (Input.GetKey(KeyCode.W))
                {
                    carRigidbody.AddForceAtPosition(accelerationForce * tire.transform.forward, tire.transform.position);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    carRigidbody.AddForceAtPosition(-accelerationForce * tire.transform.forward, tire.transform.position);
                }
            }
            if (tire.isSteerable)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    tire.transform.localEulerAngles = new Vector3(0f, -tireRotationAngle, 0f);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    tire.transform.localEulerAngles = new Vector3(0f, tireRotationAngle, 0f);
                }
                else
                {
                    tire.transform.localEulerAngles = Vector3.zero;
                }
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
        return suspensionForce * tire.up + steeringForce * tire.right + frictionForce * tire.forward;
    }

    private float GetTireGrip(Vector3 relativeVelocity)
    {
        Vector3 horizontalVelocity = new Vector3(relativeVelocity.x, 0f, relativeVelocity.z);
        if (horizontalVelocity.magnitude < 0.0001f) return 0f;

        float relativeDrift = Math.Abs(relativeVelocity.x) / horizontalVelocity.magnitude;
        return Math.Clamp(tireGripCurve.Evaluate(relativeDrift), 0.0f, 1.0f);
    }
}
