using UnityEngine;
using System.Collections.Generic;

public class ParticleFleeBehavior : MonoBehaviour
{
    [Header("Flee Dynamics")]
    public float scatterForce = 8.0f;

    private ParticleSystem partSystem;
    private List<ParticleSystem.Particle> insideParticles = new List<ParticleSystem.Particle>();

    void Awake()
    {
        partSystem = GetComponent<ParticleSystem>();
        
        // Safety check: verify the module is active
        if (partSystem != null && !partSystem.trigger.enabled)
        {
            Debug.LogError($"[Flee Debug] The Trigger module on {gameObject.name}'s Particle System is NOT turned on! Please check the box in the inspector.");
        }
    }

    void OnParticleTrigger()
    {
        if (partSystem == null) return;

        int numInside = partSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, insideParticles);
        
        // DEBUG PRINT: Let's see if the engine even notices a collision
        if (numInside > 0)
        {
            Debug.Log($"[Flee Debug] Success! {numInside} particles are currently inside the pointer raycast bubble.");
        }

        for (int i = 0; i < numInside; i++)
        {
            ParticleSystem.Particle p = insideParticles[i];
            Component triggerComponent = partSystem.trigger.GetCollider(0);
            
            if (triggerComponent != null)
            {
                Vector3 triggerPosition = triggerComponent.transform.position;

                Vector3 particleWorldPos = (partSystem.main.simulationSpace == ParticleSystemSimulationSpace.World) 
                    ? p.position 
                    : transform.TransformPoint(p.position);

                Vector3 fleeDirection = (particleWorldPos - triggerPosition).normalized;
                p.velocity += fleeDirection * scatterForce;
                
                insideParticles[i] = p;
            }
            else
            {
                Debug.LogWarning("[Flee Debug] The system detected particles inside, but your Colliders list slot 0 in the Particle Trigger module is completely empty!");
            }
        }

        partSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Inside, insideParticles);
    }
}