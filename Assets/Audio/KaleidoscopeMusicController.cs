using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class KaleidoscopeMusicController : MonoBehaviour
{
    private EventInstance musicInstance;

    void Start()
    {
        musicInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.KaleidoscopeMusic);
        musicInstance.start();
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        
        // Release it from memory
        musicInstance.release();
    }
}