using UnityEngine;
using System.Collections;
using FMOD.Studio;

public class MusicFaderCarrier : MonoBehaviour
{
    private EventInstance drum;
    private EventInstance navajo;
    private EventInstance dialogue;
    private float fadeDuration;

    public void SetupCarrier(EventInstance d, EventInstance n, EventInstance dial, float duration)
    {
        drum = d;
        navajo = n;
        dialogue = dial;
        fadeDuration = duration;

        // FIX: Strip the 3D attributes so the audio engine plays it flat in 2D stereo.
        // This prevents the track from dropping out when the old camera/listener location unloads!
        if (drum.isValid()) drum.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(Vector3.zero));
        if (navajo.isValid()) navajo.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(Vector3.zero));

        StartCoroutine(ExecuteCrossSceneFade());
    }

    private IEnumerator ExecuteCrossSceneFade()
    {
        // Give the new scene a brief moment (e.g., 1.5 seconds) to load in 
        // while keeping the music playing completely at full volume (1.0)
        float holdingTime = 1.5f; 
        yield return new WaitForSeconds(holdingTime);

        // Now run the smooth fade-out calculation in the new scene
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float vol = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            
            if (drum.isValid()) drum.setVolume(vol);
            if (navajo.isValid()) navajo.setVolume(vol);
            if (dialogue.isValid()) dialogue.setVolume(vol);
            
            yield return null;
        }

        // Complete final release cleanup
        if (drum.isValid()) { drum.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); drum.release(); }
        if (navajo.isValid()) { navajo.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); navajo.release(); }
        if (dialogue.isValid()) { dialogue.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); dialogue.release(); }

        Destroy(gameObject);
    }
}