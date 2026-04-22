using UnityEngine;
using UnityEngine.AI;

public class ShadowClone : MonoBehaviour
{
    [Header("Blood Settings")]
    public GameObject bloodPrefab; 
    public Transform bloodSpawnPoint; 
    public float bloodDuration = 0.5f; // How long the spray lasts per stab
    
    private ParticleSystem activeBloodStream;
    private GameObject fatherEntity; 
    private NavMeshAgent agent;
    private Animator anim;
    private Transform target;
    private bool hasReachedTarget = false;

    void Awake()
    {
        fatherEntity = GameObject.FindGameObjectWithTag("Father");
        if (fatherEntity == null)
            Debug.LogError("ShadowClone: Could not find object tagged 'Father'!");
    }

    public void SetTarget(Transform father)
    {
        target = father;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

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

        // 1. Instantiate the effect only once if it doesn't exist
        if (activeBloodStream == null && bloodPrefab != null)
        {
            GameObject bloodObj = Instantiate(bloodPrefab, bloodSpawnPoint.position, bloodSpawnPoint.rotation);
            bloodObj.transform.SetParent(bloodSpawnPoint); 
            activeBloodStream = bloodObj.GetComponent<ParticleSystem>();
        }

        if (activeBloodStream != null)
        {
            // 2. Orient the spray
            Vector3 sprayDir = (bloodSpawnPoint.position - transform.position).normalized;
            sprayDir.y = -0.5f; 
            activeBloodStream.transform.forward = sprayDir;

            // 3. Play the burst and then stop it after a delay
            activeBloodStream.Play();
            Invoke("StopBloodEffect", bloodDuration);
            
            // Trigger pool growth
            BloodPoolGrow pool = FindObjectOfType<BloodPoolGrow>();
            if (pool != null) pool.StartPool();
        }
    }

    private void StopBloodEffect()
    {
        if (activeBloodStream != null)
        {
            activeBloodStream.Stop();
        }
    }

    void Update()
    {
        if (target == null) return;

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
        else 
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
}