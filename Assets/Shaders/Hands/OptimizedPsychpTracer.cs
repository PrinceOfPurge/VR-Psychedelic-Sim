using UnityEngine;
using System.Collections.Generic;

public class OptimizedPsychoTracer : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject slicePrefab; 
    public int poolSize = 30;
    private List<TracerSliceLogic> pool = new List<TracerSliceLogic>();
    private int currentPoolIndex = 0;

    [Header("Velocity Settings")]
    public float minVelocity = 1.5f;   // Won't spawn if you move slowly
    public float sliceSpacing = 0.08f; // Distance between each smoke ghost

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        InitializePool();
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

    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);
        float velocity = dist / Time.deltaTime;

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
}