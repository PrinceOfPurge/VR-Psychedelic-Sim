using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RaycastPointerTip : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag your RaycastRepellerField object here.")]
    public ParticleSystemForceField repellerField;

    [Header("360 Sky Collision")]
    [Tooltip("The radius matching your starfield sphere (e.g., 30).")]
    public float skySphereRadius = 30f;
    
    private XRRayInteractor rayInteractor;

    void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    void Update()
    {
        if (rayInteractor == null || repellerField == null) return;

        // Route 1: If the laser hits a physical menu button or UI canvas, snap the force field there
        if (rayInteractor.TryGetHitInfo(out Vector3 hitPosition, out Vector3 hitNormal, out int positionInLine, out bool isValidTarget))
        {
            if (isValidTarget)
            {
                repellerField.transform.position = hitPosition;
                if (!repellerField.gameObject.activeSelf) repellerField.gameObject.SetActive(true);
                return;
            }
        }

        // Route 2: If pointing into the empty black space, calculate the 360-degree sky position mathematically
        // Project a coordinate along the controller's forward pointing direction out to the star distance
        Vector3 rayDirection = transform.forward;
        Vector3 skyProjectedPosition = transform.position + (rayDirection * skySphereRadius);

        repellerField.transform.position = skyProjectedPosition;
        if (!repellerField.gameObject.activeSelf) repellerField.gameObject.SetActive(true);
    }
}