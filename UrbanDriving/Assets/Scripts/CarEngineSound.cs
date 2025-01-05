using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private float rpm;

    // Start is called before the first frame update
    void Start()
    {
        soundSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        rpm = carPhysics.GetRPM();
        if (rpm < minRPM)
        {
            SetAndPlay(engineStallSound);
            soundSource.pitch = 1.0f;
        }
        else if (rpm > minRPM && rpm < maxRPM)
        {
            SetAndPlay(engineNormalSound);
            soundSource.pitch = minPitch + ((rpm - minRPM) / (maxRPM - minRPM)) * (maxPitch - minPitch);
        }
        else
        {
            soundSource.pitch = maxPitch;
        }
    }

    private void SetAndPlay(AudioClip clip)
    {
        if (soundSource.clip != clip)
        {
            soundSource.clip = clip;
            soundSource.Play();
        }
    }
}
