using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NightmareManager : MonoBehaviour
{
    public GameObject shadowKidPrefab;
    public Transform[] spawnPoints;
    public Transform[] attackTargets; // Drag your 3 Empty GameObjects here
    
    [Header("Timing")]
    public float delayAfterKidVanish = 2.0f;
    public float spawnInterval = 0.8f;

    public void StartNightmareSequence()
    {
        StartCoroutine(SpawnShadows());
    }

    IEnumerator SpawnShadows()
    {
        yield return new WaitForSeconds(delayAfterKidVanish);

        int targetIndex = 0;

        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject clone = Instantiate(shadowKidPrefab, spawnPoint.position, spawnPoint.rotation);
            
            ShadowClone cloneScript = clone.GetComponent<ShadowClone>();
            if(cloneScript != null)
            {
                // Assign target based on the current index
                // % attackTargets.Length ensures if you have 5 kids and 3 spots, 
                // the 4th kid goes back to spot 1.
                Transform chosenTarget = attackTargets[targetIndex % attackTargets.Length];
                cloneScript.SetTarget(chosenTarget);
            }

            targetIndex++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}