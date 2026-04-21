using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PooledAfterImage : MonoBehaviour
{
    [Header("References")]
    public SkinnedMeshRenderer handRenderer; // Leave empty for non-hand objects
    public Material tracerMaterial;

    [Header("Velocity Settings")]
    public float minVelocity = 0.7f;
    public float maxVelocity = 5.0f;
    public float sliceSpacing = 0.005f; 
    public float trailLifetime = 0.22f;

    [Header("Drop / Impact Sound")]
    [Tooltip("Click the search icon to select the specific FMOD Event for this object's drop sound.")]
    public EventReference dropSoundEvent;
    
    [Tooltip("How hard does it need to hit to make a sound? (Prevents sliding/rolling spam)")]
    public float minImpactForce = 0.5f;

    [Tooltip("Optional: If your FMOD event has a parameter for impact intensity, type its name here.")]
    public string impactParameterName = "";

    [Tooltip("Prevents the sound from playing 50 times a second if vibrating on the floor.")]
    public float soundCooldown = 0.1f;
    private float lastSoundTime = 0f;

    [Header("Audio Deadzone")]
    public float audioDeadzone = 1.2f; 
    
    [Header("Visual Refinement")]
    public Color trailTint = new Color(0f, 0.4f, 1f, 1f);
    [Range(0f, 0.05f)] public float maxAlpha = 0.022f;
    public float startScaleShrink = 0.05f; // Objects usually need less shrink than hands
    public float endScaleShrink = 0.35f;

    [Header("Psychedelic Options")]
    public bool enableHueShift = true;
    [Range(0f, 0.3f)] public float hueShiftIntensity = 0.1f; 
    
    [Header("Audio Smoothing")]
    public float audioLerpSpeed = 15f; // Snappier for objects

    private EventInstance tracerInstance;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float smoothedIntensity;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        Invoke("InitializeAudio", 0.1f);
    }

    void InitializeAudio()
    {
        // Try/catch isn't strictly necessary, but good practice if AudioManager might not exist in some scenes
        if (AudioManager.instance && FMODEvents.instance)
        {
            tracerInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.AfterImage);
            RuntimeManager.AttachInstanceToGameObject(tracerInstance, transform);
            tracerInstance.start(); 
        }
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);
        float velocity = dist / Time.deltaTime;

        UpdateAudio(velocity);

        if (velocity > minVelocity)
        {
            int slices = Mathf.Min(Mathf.FloorToInt(dist / sliceSpacing), 20); 
            for (int i = 1; i <= slices; i++)
            {
                float t = (float)i / slices;
                SpawnSlice(Vector3.Lerp(lastPosition, transform.position, t), 
                           Quaternion.Slerp(lastRotation, transform.rotation, t), 
                           Mathf.InverseLerp(minVelocity, maxVelocity, velocity) * maxAlpha);
            }
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void UpdateAudio(float velocity)
    {
        if (!tracerInstance.isValid()) return;

        float target = velocity > audioDeadzone ? Mathf.InverseLerp(audioDeadzone, maxVelocity, velocity) : 0f;
        smoothedIntensity = Mathf.Lerp(smoothedIntensity, target, Time.deltaTime * audioLerpSpeed);

        tracerInstance.setParameterByName("TracerIntensity", smoothedIntensity);

        PLAYBACK_STATE state;
        tracerInstance.getPlaybackState(out state);

        if (smoothedIntensity > 0.01f && state != PLAYBACK_STATE.PLAYING)
            tracerInstance.start();
        else if (smoothedIntensity <= 0.01f && state == PLAYBACK_STATE.PLAYING)
            tracerInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    // --- NEW PHYSICS IMPACT LOGIC ---
    void OnCollisionEnter(Collision collision)
    {
        // Don't do anything if no sound is assigned or we are still on cooldown
        if (dropSoundEvent.IsNull || Time.time < lastSoundTime + soundCooldown) return;

        // Calculate how hard the object hit the surface
        float impactForce = collision.relativeVelocity.magnitude;

        // If it hit hard enough...
        if (impactForce >= minImpactForce)
        {
            // Reset the cooldown timer
            lastSoundTime = Time.time;

            if (string.IsNullOrEmpty(impactParameterName))
            {
                // Simple Mode: Just play the sound at the object's position
                RuntimeManager.PlayOneShot(dropSoundEvent, transform.position);
            }
            else
            {
                // Advanced Mode: Pass the impact force to FMOD to control volume/intensity
                EventInstance impactInstance = RuntimeManager.CreateInstance(dropSoundEvent);
                RuntimeManager.AttachInstanceToGameObject(impactInstance, transform);
                
                // You can tweak this math later. Currently sending the raw velocity magnitude (e.g., 1.0 to 15.0)
                impactInstance.setParameterByName(impactParameterName, impactForce);
                
                impactInstance.start();
                impactInstance.release(); // Tells FMOD to automatically destroy the instance when it finishes playing
            }
        }
    }

    void SpawnSlice(Vector3 pos, Quaternion rot, float alpha)
    {
        GameObject slice = new GameObject("Tracer_Slice");
        slice.transform.SetPositionAndRotation(pos, rot);
        slice.transform.localScale = transform.lossyScale * (1f - startScaleShrink);

        Mesh mesh;
        if (handRenderer != null) {
            mesh = new Mesh();
            handRenderer.BakeMesh(mesh);
        } else {
            // Optimization: Use the shared mesh for static objects
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        
        slice.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer mr = slice.AddComponent<MeshRenderer>();
        
        Material mat = new Material(tracerMaterial);
        mat.color = new Color(trailTint.r, trailTint.g, trailTint.b, alpha);
        mr.material = mat;

        StartCoroutine(FadeAndDestroy(slice, handRenderer != null, mesh, mat, alpha, slice.transform.localScale));
    }

    System.Collections.IEnumerator FadeAndDestroy(GameObject obj, bool isSkinned, Mesh m, Material mat, float startAlpha, Vector3 baseScale)
    {
        float elapsed = 0;
        float h = 0, s = 0, v = 0;
        if (enableHueShift) Color.RGBToHSV(trailTint, out h, out s, out v);

        while (elapsed < trailLifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / trailLifetime;
            if (mat == null) break;

            float fade = 1.0f - (t * t);
            if (enableHueShift) {
                Color shifted = Color.HSVToRGB((h + (t * hueShiftIntensity)) % 1.0f, s, v);
                mat.color = new Color(shifted.r, shifted.g, shifted.b, startAlpha * fade);
            } else {
                mat.color = new Color(trailTint.r, trailTint.g, trailTint.b, startAlpha * fade);
            }

            obj.transform.localScale = Vector3.Lerp(baseScale, baseScale * (1f - endScaleShrink), t);
            yield return null;
        }

        Destroy(mat);
        if (isSkinned) Destroy(m); // Only destroy baked meshes, not sharedMesh
        Destroy(obj);
    }

    private void OnDestroy()
    {
        if (tracerInstance.isValid()) {
            tracerInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            tracerInstance.release();
        }
    }
}