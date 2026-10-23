using UnityEngine;
using System.Collections;
using FMOD.Studio;

public class CrossSceneMusicReceiver : MonoBehaviour
{
    [Header("FMOD Event Reference")]
    [SerializeField] private FMODUnity.EventReference starweaverMusicPart2;

    [Header("Timing Settings")]
    [Tooltip("How long to let the music play at full volume in the new scene before fading.")]
    [SerializeField] private float delayBeforeFade = 1.5f;

    private EventInstance musicInstance;

    void Start()
    {
        if (!starweaverMusicPart2.IsNull)
        {
            // Create and start the pickup track immediately on scene load
            musicInstance = FMODUnity.RuntimeManager.CreateInstance(starweaverMusicPart2);
            
            // Force it to 2D stereo so it doesn't quiet down based on player location
            musicInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(Vector3.zero));
            
            musicInstance.start();

            // Kick off the automated fade sequence
            StartCoroutine(WaitAndFadeOut());
        }
    }

    private IEnumerator WaitAndFadeOut()
    {
        // Let it play out where it left off for a moment
        yield return new WaitForSeconds(delayBeforeFade);

        if (musicInstance.isValid())
        {
            // This triggers your FMOD AHDSR Release envelope to fade out flawlessly!
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }

    private void OnDestroy()
    {
        // Safety cleanup layer if the player skips or leaves the scene early
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}