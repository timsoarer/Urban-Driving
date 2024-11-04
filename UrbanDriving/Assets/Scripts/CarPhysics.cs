using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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
    // Array of tires that will steer left/right
    [SerializeField]
    // Array of tires that will provide acceleration force for the car
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
    private float placeholderVariable1;

    [Header("Acceleration Force Parameters")]
    [SerializeField]
    private float placeholderVariable2;

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
            Vector3 totalForce = Vector3.zero;
            totalForce += CalculateSuspensionForce(tire.transform);
            carRigidbody.AddForceAtPosition(totalForce, tire.transform.position);
        }
    }

    private Vector3 CalculateSuspensionForce(Transform tire)
    {
        float force = 0.0f;
        RaycastHit tireHit;
        if (Physics.Raycast(tire.position, -tire.up, out tireHit, restDistance))
        {
            float tireVelocity = Vector3.Dot(carRigidbody.GetPointVelocity(tire.position), tire.up);
            float offset = restDistance - tireHit.distance;
            force = (offset * springStrength) - (tireVelocity * damping);
        }
        return force * tire.up;
    }
}
