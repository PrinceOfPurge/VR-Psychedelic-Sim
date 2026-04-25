using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity; 

public class FMODEvents : MonoBehaviour
{
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; private set; }
    [field: SerializeField] public EventReference Desert { get; private set; }
    
    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }
    [field: SerializeField] public EventReference KaleidoscopeMusic { get; private set; }
    [field: SerializeField] public EventReference NarrativeMusic { get; private set; }
    [field: SerializeField] public EventReference LocksmithMusic { get; private set; }
    [field: SerializeField] public EventReference RadioMusic { get; private set; }
    [field: SerializeField] public EventReference HospitalMusic { get; private set; }
    [field: SerializeField] public EventReference HoganDesMusic { get; private set; }
    [field: SerializeField] public EventReference NavajoMusic { get; private set; }
    
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference AfterImage { get; private set; }
    
    [field: Header("World SFX")]
    [field: SerializeField] public EventReference grinding { get; private set; }
    [field: SerializeField] public EventReference keyDrop { get; private set; }
    [field: SerializeField] public EventReference yell { get; private set; }
    [field: SerializeField] public EventReference kidLine { get; private set; }
    [field: SerializeField] public EventReference Stab { get; private set; }
    [field: SerializeField] public EventReference Die { get; private set; }
    [field: SerializeField] public EventReference KidFootsteps { get; private set; }
    [field: SerializeField] public EventReference sadKid { get; private set; }
    [field: SerializeField] public EventReference CloneHurt { get; private set; }
    [field: SerializeField] public EventReference gasp { get; private set; }
    [field: SerializeField] public EventReference tinnitus { get; private set; }
    
    [Header("Hospital Dialogue")]
    public FMODUnity.EventReference[] hospitalLines;
    
    [Header("Narrative Dialogue")]
    public FMODUnity.EventReference[] narrativeLines;
    
    [Header("Hogan Dialogue")]
    public FMODUnity.EventReference[] hoganDialogue;
    
    public FMODUnity.EventReference[] healerDialogue; 
    

    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one AudioManager in scene.");
        }
        instance = this;
    }
    
}
