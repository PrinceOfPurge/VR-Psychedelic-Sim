using UnityEngine;
using System.Collections;

public class HoganSceneInitializer : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private float fadeDuration = 2.0f;
    
    [Header("Therapist Entry")]
    [SerializeField] private GameObject therapistNPC;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float entryDelay = 3.0f;
    [SerializeField] private float walkDuration = 5.0f;

    private void Start()
    {
        // 1. Trigger the Fade In
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.PerformFade(isInReverse:true, fadeDurationOverride: fadeDuration);
        }
    
        // 2. Start the sequence
        if (therapistNPC != null && startPoint != null && endPoint != null)
        {
            StartCoroutine(TherapistEntrySequence());
        }
    }

    private IEnumerator TherapistEntrySequence()
    {
        // Set therapist to starting position (outside or behind a wall)
        therapistNPC.transform.position = startPoint.position;
        
        yield return new WaitForSeconds(entryDelay);

        float elapsed = 0;
        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / walkDuration);
            
            therapistNPC.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            
            // Tell the motion script to update its "base" so it doesn't snap back when bobbing
            therapistNPC.GetComponent<NPCBiologicalMotion>()?.ResetBasePosition();
            
            yield return null;
        }

        // Final snap to end point
        therapistNPC.transform.position = endPoint.position;
        therapistNPC.GetComponent<NPCBiologicalMotion>()?.ResetBasePosition();
    }
}