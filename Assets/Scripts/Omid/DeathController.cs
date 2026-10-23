using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement; 

public class DeathController : MonoBehaviour
{
    public BloodPoolGrow floorPool;     
    public Animator fatherAnim;
    
    [Header("Timing Settings")]
    public float soundDelay = 1.2f; 
    public float sceneLoadDelay = 5.0f;

    private string transitionSceneName = "Level4_Brief"; 
    private bool sequenceStarted = false;

    void Update()
    {
        if (floorPool == null) return;

        // Trigger sequence when the blood pool safely reaches its max size
        if (!sequenceStarted && floorPool.transform.localScale.x >= floorPool.maxXZSize - 0.1f)
        {
            StartDeathSequence();
        }
    }

    void StartDeathSequence()
    {
        sequenceStarted = true;

        if (fatherAnim != null)
        {
            fatherAnim.SetTrigger("isDead");
        }

        StartCoroutine(PlayDeathSoundWithDelay());
        StartCoroutine(WaitAndLoadScene());

        if (GetComponent<CapsuleCollider>()) GetComponent<CapsuleCollider>().enabled = false;
        if (GetComponent<NavMeshAgent>()) GetComponent<NavMeshAgent>().enabled = false;

        DismissClones();
    }

    IEnumerator PlayDeathSoundWithDelay()
    {
        yield return new WaitForSeconds(soundDelay);
        if (FMODEvents.instance != null)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Die, transform.position);
        }
    }

    IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(transitionSceneName);
    }

    void DismissClones()
    {
        ShadowClone[] clones = FindObjectsOfType<ShadowClone>();
        foreach (ShadowClone clone in clones)
        {
            if (clone != null) clone.Flee();
        }
    }
}