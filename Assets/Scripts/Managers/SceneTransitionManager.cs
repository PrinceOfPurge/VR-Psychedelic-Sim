using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private SceneAsset clinicScene;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TransitionFromShopToClinic(); // Testing the cube so it's in here for now. The transition object will be separate later
        }
    }

    /// <summary>
    /// This method transitions from the first scene in the locksmith shop to the hospital scene with the Cancer diagnosis
    /// </summary>
    private void TransitionFromShopToClinic()
    {
        print("Changing scenes!");
        SceneManager.LoadScene(clinicScene.name);
    }
}