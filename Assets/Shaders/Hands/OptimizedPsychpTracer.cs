using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class OptimizedPsychoTracer : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject slicePrefab; // Your Tracer_Slice_Template
    public int poolSize = 30;
    private List<TracerSliceLogic> pool = new List<TracerSliceLogic>();
    private int currentPoolIndex = 0;

    [Header("Velocity Settings")]
    public float minVelocity = 0.7f;   // Speed required to start spawning
    public float maxVelocity = 5.0f;   // Speed for max FMOD volume
    public float sliceSpacing = 0.05f; // Physical distance between slices

    [Header("FMOD Audio")]
    public EventReference afterImageEvent; 
    private EventInstance tracerInstance;
    public float audioDeadzone = 1.2f;
    public float audioLerpSpeed = 15f;
    private float smoothedIntensity;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        InitializePool();
        InitializeAudio();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(slicePrefab);
            obj.SetActive(false); 
            pool.Add(obj.GetComponent<TracerSliceLogic>());
        }
    }

    void InitializeAudio()
    {
        // Creates the FMOD instance using your event
        tracerInstance = RuntimeManager.CreateInstance(afterImageEvent);
        RuntimeManager.AttachInstanceToGameObject(tracerInstance, transform);
        tracerInstance.start(); 
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);
        float velocity = dist / Time.deltaTime;

        UpdateAudio(velocity);

        // ONLY SPAWN if moving fast enough AND moved far enough
        if (velocity > minVelocity && dist > sliceSpacing)
        {
            SpawnFromPool(transform.position, transform.rotation);
            lastPosition = transform.position;
        }
    }

    void SpawnFromPool(Vector3 pos, Quaternion rot)
    {
        if (pool.Count == 0) return;

        TracerSliceLogic slice = pool[currentPoolIndex];
        slice.transform.SetPositionAndRotation(pos, rot);
        slice.ActivateSlice(); 

        currentPoolIndex = (currentPoolIndex + 1) % poolSize;
    }

    void UpdateAudio(float velocity)
    {
        if (!tracerInstance.isValid()) return;
        
        float target = velocity > audioDeadzone ? Mathf.InverseLerp(audioDeadzone, maxVelocity, velocity) : 0f;
        smoothedIntensity = Mathf.Lerp(smoothedIntensity, target, Time.deltaTime * audioLerpSpeed);
        
        tracerInstance.setParameterByName("TracerIntensity", smoothedIntensity);
    }

    void OnDestroy()
    {
        tracerInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        tracerInstance.release();
    }
}