using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineSound : MonoBehaviour
{
    private AudioSource soundSource;

    [SerializeField]
    private CarPhysics carPhysics;

    [Header("Sound clips")]
    [SerializeField]
    private AudioClip engineStallSound;
    [SerializeField]
    private AudioClip engineNormalSound;

    [Header("RPM caps")]
    [SerializeField]
    private float minRPM;
    [SerializeField]
    private float maxRPM;

    [Header("Pitch caps")]
    [SerializeField]
    private float minPitch;
    [SerializeField]
    private float maxPitch;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (carPhysics.GetRPM() < minRPM)
        {
            soundSource.clip = engineStallSound;
            
        }
    }
}
