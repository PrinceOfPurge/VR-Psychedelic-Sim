using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Required for Coroutines

public class DeathController : MonoBehaviour
{
    public BloodPoolGrow floorPool;     
    public Animator fatherAnim;
    
    [Header("Timing Settings")]
    [Tooltip("Seconds to wait after animation starts before playing the 'Die' thud sound")]
    public float soundDelay = 1.2f; 
    
    private bool sequenceStarted = false;

    void Update()
    {
        if (floorPool == null) return;

        if (!sequenceStarted && floorPool.transform.localScale.x >= floorPool.maxSize - 0.1f)
        {
            StartDeathSequence();
        }
    }

    void StartDeathSequence()
    {
        sequenceStarted = true;

        // 1. Trigger Animation immediately
        if (fatherAnim != null)
        {
            fatherAnim.SetTrigger("isDead");
        }

        // 2. Start the timed audio delay
        StartCoroutine(PlayDeathSoundWithDelay());

        // 3. Physical cleanup
        if (GetComponent<CapsuleCollider>()) GetComponent<CapsuleCollider>().enabled = false;
        if (GetComponent<NavMeshAgent>()) GetComponent<NavMeshAgent>().enabled = false;

        DismissClones();
    }

    IEnumerator PlayDeathSoundWithDelay()
    {
        // Wait for the visual fall to reach the ground
        yield return new WaitForSeconds(soundDelay);

        // Play the sound at the current position
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Die, transform.position);
        
        Debug.Log("Death sound triggered after delay.");
    }

    void DismissClones()
    {
        ShadowClone[] clones = FindObjectsOfType<ShadowClone>();
        foreach (ShadowClone clone in clones)
        {
            clone.Flee();
        }
    }
}