using UnityEngine;

public class OrbHeartSync : MonoBehaviour
{
    [Header("Orb GameObjects")]
    public Transform playerOrb;
    public Transform targetOrb;

    [Header("Heartbeat Inputs (BPM)")]
    [Range(40, 140)] public float playerBPM = 100f; // Start out of sync
    [Range(40, 140)] public float targetBPM = 72f;

    [Header("Sync Thresholds")]
    public float bpmTolerance = 5f;
    public float mergeDistance = 0.5f;

    [Header("Visual Feedback")]
    public LineRenderer connectionBeam;
    public ParticleSystem fusionParticles;

    private Vector3 originalPlayerPos;
    private Vector3 originalTargetPos;
    
    // Store original sizes to maintain proportions
    private Vector3 playerStartScale;
    private Vector3 targetStartScale;

    private float syncTimer = 0f;
    private float lastTapTime = 0f;
    
    private bool isFused = false; // Prevents the burst from looping

    void Start()
    {
        originalPlayerPos = playerOrb.position;
        originalTargetPos = targetOrb.position;
        
        // Cache the exact sizes you set in the Unity Editor
        playerStartScale = playerOrb.localScale;
        targetStartScale = targetOrb.localScale;
    }

    void Update()
    {
        if (isFused) return; // Stops the loop once merged

        HandleSpacebarInput();

        // Pass the cached scales into the pulse function
        PulseOrb(playerOrb, playerBPM, playerStartScale);
        PulseOrb(targetOrb, targetBPM, targetStartScale);

        float bpmDifference = Mathf.Abs(playerBPM - targetBPM);

        if (bpmDifference <= bpmTolerance)
        {
            syncTimer += Time.deltaTime;
            DrawBeam();
            MovePlayerToTarget(syncTimer);
        }
        else
        {
            syncTimer = Mathf.Max(0, syncTimer - Time.deltaTime);
            if (connectionBeam != null) connectionBeam.enabled = false;
            ResetPlayerPosition();
        }
    }

    void HandleSpacebarInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float timeSinceLastTap = Time.time - lastTapTime;
            lastTapTime = Time.time;

            if (timeSinceLastTap > 0.4f && timeSinceLastTap < 2.0f)
            {
                playerBPM = 60f / timeSinceLastTap;
                playerBPM = Mathf.Clamp(playerBPM, 40f, 140f);
            }
        }
    }

    void PulseOrb(Transform orb, float bpm, Vector3 baseScale)
    {
        float pulseSpeed = (bpm / 60f) * Mathf.PI * 2f;
        float pulseMultiplier = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;
        
        // Apply the pulse multiplier to the original base scale
        orb.localScale = baseScale * pulseMultiplier;
    }

    void DrawBeam()
    {
        if (connectionBeam != null)
        {
            connectionBeam.enabled = true;
            connectionBeam.SetPosition(0, playerOrb.position);
            connectionBeam.SetPosition(1, targetOrb.position);
        }
    }

    void MovePlayerToTarget(float progress)
    {
        // Move the player orb directly to the stationary target orb
        playerOrb.position = Vector3.Lerp(originalPlayerPos, originalTargetPos, progress * 0.2f);

        if (Vector3.Distance(playerOrb.position, originalTargetPos) < mergeDistance)
        {
            FuseOrbs(originalTargetPos);
        }
    }

    void ResetPlayerPosition()
    {
        // Only the player orb needs to return to its original position
        playerOrb.position = Vector3.Lerp(playerOrb.position, originalPlayerPos, Time.deltaTime);
    }

    void FuseOrbs(Vector3 position)
    {
        if (fusionParticles != null && !fusionParticles.isPlaying)
        {
            fusionParticles.transform.position = position;
            fusionParticles.Play();
        }
        
        // Turn off the connection beam
        if (connectionBeam != null)
        {
            connectionBeam.enabled = false;
        }

        playerOrb.gameObject.SetActive(false);
        targetOrb.gameObject.SetActive(false);
        
        isFused = true; // Locks the sequence
    }
}