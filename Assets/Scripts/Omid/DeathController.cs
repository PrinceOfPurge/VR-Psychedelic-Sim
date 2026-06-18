using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement; // Essential for scene switching

public class DeathController : MonoBehaviour
{
    public BloodPoolGrow floorPool;     
    public Animator fatherAnim;
    
    [Header("Timing Settings")]
    [Tooltip("Seconds to wait after animation starts before playing the 'Die' thud sound")]
    public float soundDelay = 1.2f; 

    [Tooltip("How long the player watches the body before transitioning to the brief")]
    public float sceneLoadDelay = 5.0f;

    // Set to the specific name of your transition scene
    private string transitionSceneName = "Level4_Brief"; 
    
    private bool sequenceStarted = false;

    void Update()
    {
        if (floorPool == null) return;

        // Trigger sequence when the blood pool reaches max size
        if (!sequenceStarted && floorPool.transform.localScale.x >= floorPool.maxXZSize - 0.1f)
        {
            StartDeathSequence();
        }
    }

    void StartDeathSequence()
    {
        sequenceStarted = true;

        // 1. Trigger the 'isDead' animation
        if (fatherAnim != null)
        {
            fatherAnim.SetTrigger("isDead");
        }

        // 2. Play the impact sound on a delay (sync with animation)
        StartCoroutine(PlayDeathSoundWithDelay());

        // 3. Handle the delayed jump to the text brief scene
        StartCoroutine(WaitAndLoadScene());

        // 4. Disable physical presence so the model settles naturally
        if (GetComponent<CapsuleCollider>()) GetComponent<CapsuleCollider>().enabled = false;
        if (GetComponent<NavMeshAgent>()) GetComponent<NavMeshAgent>().enabled = false;

        // 5. Tell the kids to run away
        DismissClones();
    }

    IEnumerator PlayDeathSoundWithDelay()
    {
        yield return new WaitForSeconds(soundDelay);
        
        // Play 'Die' SFX via your AudioManager setup
        if (FMODEvents.instance != null)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Die, transform.position);
        }
        
        Debug.Log("Death thud played.");
    }

    IEnumerator WaitAndLoadScene()
    {
        // Give the player time to process the scene
        yield return new WaitForSeconds(sceneLoadDelay);

        Debug.Log("Transitioning to Level4_Brief...");
        
        // Final sanity check: Ensure the scene is in Build Settings
        SceneManager.LoadScene(transitionSceneName);
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