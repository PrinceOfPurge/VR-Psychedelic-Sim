using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio;

public class KidWalk : MonoBehaviour
{
    public Transform father;
    public Transform escapePoint; 
    public GameObject painting; 
    
    [Header("Dialogue Timing")]
    public float timeUntilFatherYells = 4.0f; 
    public float timeBeforeRunning = 2.5f;   

    private NavMeshAgent agent;
    private Animator anim;
    private bool hasBeenYelledAt = false;
    private bool isShowing = false;
    private bool isEscaping = false;

    private EventInstance footstepInstance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.stoppingDistance = 1.2f;

        footstepInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.KidFootsteps);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(footstepInstance, transform);
        footstepInstance.start();
    }

    void Update()
    {
        if (footstepInstance.isValid())
        {
            footstepInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        }

        if (hasBeenYelledAt) 
        {
            if (isEscaping) anim.SetFloat("Speed", 1.0f);
            
            if (isEscaping && !agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                NightmareManager nm = FindObjectOfType<NightmareManager>();
                if(nm != null) nm.StartNightmareSequence();
                
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
            CheckFootsteps(true);
        }
        else
        {
            if (!isShowing)
            {
                isShowing = true;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                anim.SetFloat("Speed", 0f); 
                CheckFootsteps(false);

                if (FMODEvents.instance != null)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(FMODEvents.instance.kidLine, transform.position);
                }

                Invoke("ExecuteYell", timeUntilFatherYells); 
            }

            Vector3 lookPos = father.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5f);
        }
    }

    private void CheckFootsteps(bool shouldPlay)
    {
        PLAYBACK_STATE pbState;
        footstepInstance.getPlaybackState(out pbState);

        if (shouldPlay && pbState != PLAYBACK_STATE.PLAYING)
            footstepInstance.start();
        else if (!shouldPlay && pbState == PLAYBACK_STATE.PLAYING)
            footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void ExecuteYell()
    {
        if (hasBeenYelledAt) return;
        hasBeenYelledAt = true; 

        // 1. Father Yells
        if (FMODEvents.instance != null)
        {
            FMODUnity.RuntimeManager.PlayOneShot(FMODEvents.instance.yell, transform.position);
        }

        anim.SetTrigger("YelledAt");
        
        // We play this right after the yell to show the kid's reaction
        if (FMODEvents.instance != null)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.sadKid, transform.position); 
        }

        if(painting != null) {
            painting.transform.SetParent(null);
            MeshCollider mc = painting.GetComponent<MeshCollider>();
            if (mc != null) mc.convex = true;

            Rigidbody rb = painting.GetComponent<Rigidbody>();
            if (rb == null) rb = painting.AddComponent<Rigidbody>();
        
            if (rb != null)
            {
                rb.isKinematic = false; 
                rb.AddForce(transform.forward * -0.8f, ForceMode.Impulse); 
            }
        }

        Invoke("Flee", timeBeforeRunning); 
    }

    void Flee()
    {
        if (escapePoint == null) return;

        isEscaping = true; 
        agent.isStopped = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        CheckFootsteps(true);

        Vector3 fleeDir = (escapePoint.position - transform.position).normalized;
        fleeDir.y = 0; 
        transform.rotation = Quaternion.LookRotation(fleeDir);

        agent.speed = 3.0f; 
        agent.acceleration = 7.0f; 
        agent.SetDestination(escapePoint.position);
    
        anim.SetFloat("Speed", 1.0f); 
    }

    private void OnDestroy()
    {
        footstepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        footstepInstance.release();
    }
}