using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio; // Required for managing the looping footstep instance

public class ShadowClone : MonoBehaviour
{
    [Header("Blood Settings")]
    public GameObject bloodPrefab; 
    public Transform bloodSpawnPoint; 
    public float bloodDuration = 0.5f; 

    [Header("Audio Settings")]
    private static float lastStabTime; // Shared across all clones
    private const float STAB_COOLDOWN = 0.15f; 

    private ParticleSystem activeBloodStream;
    private GameObject fatherEntity; 
    private NavMeshAgent agent;
    private Animator anim;
    private Transform target;
    private bool hasReachedTarget = false;

    // FMOD Instance for the looping footsteps
    private EventInstance footstepInstance;

    void Awake()
    {
        fatherEntity = GameObject.FindGameObjectWithTag("Father");
        if (fatherEntity == null)
            Debug.LogError("ShadowClone: Could not find object tagged 'Father'!");
            
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // 1. Create the looping footstep instance via your AudioManager
        // This ensures it is added to the manager's internal cleanup list
        footstepInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.KidFootsteps);
        
        // 2. Attach the sound to this specific clone transform for 3D spatialization
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(footstepInstance, transform);
        
        // 3. Start the footsteps immediately as the clone spawns and begins chasing
        footstepInstance.start();
    }

    public void SetTarget(Transform father)
    {
        target = father;
        if (agent != null)
        {
            agent.speed = 5.0f; 
            agent.stoppingDistance = 1f; 
            agent.SetDestination(target.position);
        }
        
        if (anim != null) anim.SetFloat("Speed", 1.0f);
    }
    
    // Triggered by Animation Event on the "Hit" frame
    public void CreateBloodEffect()
    {
        if (bloodSpawnPoint == null) return;

        // Instantiate the blood effect if it doesn't exist
        if (activeBloodStream == null && bloodPrefab != null)
        {
            GameObject bloodObj = Instantiate(bloodPrefab, bloodSpawnPoint.position, bloodSpawnPoint.rotation);
            bloodObj.transform.SetParent(bloodSpawnPoint); 
            activeBloodStream = bloodObj.GetComponent<ParticleSystem>();
        }

        if (activeBloodStream != null)
        {
            // --- AUDIO: Play Stab SFX with Cooldown ---
            // This prevents 3 clones from playing the sound at the exact same millisecond
            if (Time.time >= lastStabTime + STAB_COOLDOWN)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.Stab, transform.position);
                lastStabTime = Time.time;
            }

            // --- AUDIO: Stop Footsteps while actively stabbing ---
            footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            // Visuals
            Vector3 sprayDir = (bloodSpawnPoint.position - transform.position).normalized;
            sprayDir.y = -0.5f; 
            activeBloodStream.transform.forward = sprayDir;

            activeBloodStream.Play();
            Invoke("StopBloodEffect", bloodDuration);
            
            // Trigger the floor pool to grow
            BloodPoolGrow pool = FindObjectOfType<BloodPoolGrow>();
            if (pool != null) pool.StartPool();
        }
    }

    private void StopBloodEffect()
    {
        if (activeBloodStream != null) activeBloodStream.Stop();
    }

    void Update()
    {
        // Update FMOD 3D attributes so the sound follows the clone's movement
        if (footstepInstance.isValid())
        {
            footstepInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        }

        if (target == null) return;

        // Pathfinding Logic
        if (!hasReachedTarget)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= agent.stoppingDistance)
            {
                hasReachedTarget = true;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                anim.SetTrigger("isAttacking");
                anim.SetFloat("Speed", 0f);
            }
        }
        // Staring Logic: Keep looking at Father while attacking, stop looking when fleeing
        else if (agent.enabled && agent.isStopped) 
        {
            if (fatherEntity != null)
            {
                Vector3 lookDir = (fatherEntity.transform.position - transform.position).normalized;
                lookDir.y = 0; 
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
    
    public void Flee()
    {
        if (anim == null || agent == null || fatherEntity == null) return;

        // 1. Reset state
        hasReachedTarget = true; 
        StopBloodEffect();

        // 2. AUDIO: Restart Footsteps for the escape
        PLAYBACK_STATE pbState;
        footstepInstance.getPlaybackState(out pbState);
        if (pbState != PLAYBACK_STATE.PLAYING) footstepInstance.start();

        // 3. Collision & NavMesh Prep
        // Turn into a trigger so they don't get stuck on the Father's collider while turning
        Collider cloneCol = GetComponent<Collider>();
        if (cloneCol != null) cloneCol.isTrigger = true; 

        agent.isStopped = false;
        agent.updateRotation = true; // Let the agent turn the body toward destination
        agent.speed = 7.0f;
        agent.acceleration = 30f; 

        // 4. Animation
        anim.SetFloat("Speed", 1.0f);
        anim.ResetTrigger("isAttacking"); 

        // 5. Smart Flee Direction
        Vector3 awayFromFather = (transform.position - fatherEntity.transform.position).normalized;
        Vector3 targetPos = transform.position + (awayFromFather * 15f);

        // Ensure target is valid on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(targetPos);
        }

        // 6. Cleanup
        Destroy(gameObject, 3f);
    }

    private void OnDestroy()
    {
        // Critical: Stop and release the FMOD instance to prevent memory leaks
        footstepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        footstepInstance.release();
    }
}