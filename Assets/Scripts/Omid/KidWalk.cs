using UnityEngine;
using UnityEngine.AI;

public class KidWalk : MonoBehaviour
{
    public Transform father;
    public Transform escapePoint; 
    public GameObject painting; 
    
    [Header("Dialogue Timing")]
    public float timeUntilFatherYells = 4.0f; // Length of kid's audio
    public float timeBeforeRunning = 2.5f;   // Length of father's audio

    private NavMeshAgent agent;
    private Animator anim;
    private bool hasBeenYelledAt = false;
    private bool isShowing = false;
    private bool isEscaping = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.stoppingDistance = 1.2f;
    }

    void Update()
    {
        // 1. ESCAPE PHASE
        if (hasBeenYelledAt) 
        {
            // Only play run animation once Flee starts
            if (isEscaping) anim.SetFloat("Speed", 1.0f);
            
            if (isEscaping && !agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                Destroy(gameObject); 
            }
            return; 
        }

        float distance = Vector3.Distance(transform.position, father.position);

        if (distance > agent.stoppingDistance)
        {
            isShowing = false;
            agent.isStopped = false;
            agent.SetDestination(father.position);
            anim.SetFloat("Speed", 0.5f);
        }
        else
        {
            if (!isShowing)
            {
                isShowing = true;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                anim.SetFloat("Speed", 0f); 

                if (FMODEvents.instance != null)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(FMODEvents.instance.kidLine, transform.position);
                }

                // Schedule the father to yell
                Invoke("ExecuteYell", timeUntilFatherYells); 
            }

            // Look at father
            Vector3 lookPos = father.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5f);
        }
    }

    public void ExecuteYell()
    {
        if (hasBeenYelledAt) return;
        hasBeenYelledAt = true; // Stop Update logic

        if (FMODEvents.instance != null)
        {
            FMODUnity.RuntimeManager.PlayOneShot(FMODEvents.instance.yell, transform.position);
        }

        anim.SetTrigger("isYelledAt");

        // Physics Fix for the Painting
        if(painting != null) {
            painting.transform.SetParent(null);
            
            MeshCollider mc = painting.GetComponent<MeshCollider>();
            if (mc != null) mc.convex = true;

            // Get or Add Rigidbody safely
            Rigidbody rb = painting.GetComponent<Rigidbody>();
            if (rb == null) rb = painting.AddComponent<Rigidbody>();
        
            if (rb != null)
            {
                rb.isKinematic = false; 
                rb.AddForce(transform.forward * -0.8f, ForceMode.Impulse); 
            }
        }

        // --- THE DELAY: Wait for dad to finish yelling, then run ---
        Invoke("Flee", timeBeforeRunning); 
    }

    void Flee()
    {
        if (escapePoint == null) return;

        isEscaping = true; 
        agent.isStopped = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        // Turn away
        Vector3 fleeDir = (escapePoint.position - transform.position).normalized;
        fleeDir.y = 0; 
        transform.rotation = Quaternion.LookRotation(fleeDir);

        agent.speed = 6.0f; 
        agent.acceleration = 35.0f; 
        agent.SetDestination(escapePoint.position);
    
        anim.SetFloat("Speed", 1.0f); 
    }
}