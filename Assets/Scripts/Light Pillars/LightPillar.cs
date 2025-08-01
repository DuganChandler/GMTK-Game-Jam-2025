using UnityEngine;

public class LightPillar : MonoBehaviour
{
    [Header("Pillar Settings")]
    [SerializeField] private bool remainCastingAfterActive = false;
    [SerializeField] private bool castOnStart = false;
    [SerializeField] private LayerMask lightLayer;
    [SerializeField] private float lightRadius = 0.25f;
    [SerializeField] private float lightLength = 15f;

    [Header("Objects")]
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private Transform lightSpawnPoint;

    public bool Active /*{ get; private set; }*/;

    private LineRenderer lightBeam;
    private Transform currentlyHitObject;

    private void Start()
    {
        if (castOnStart) Activate();
    }

    [ContextMenu("Activate")]
    public void Activate()
    {
        Active = true;
    }

    [ContextMenu("Deactivate")]
    public void Deactivate()
    {
        Active = false;

        if (currentlyHitObject != null) DeactivateCurrentlyHitBeacon();
    }

    private void CastLight()
    {
        bool hitSomething = Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPoint.forward, out RaycastHit castHit, lightLength, lightLayer);

        // Spawn Light
        if (lightBeam == null)
        {
            lightBeam = Instantiate(linePrefab, lightSpawnPoint);
            lightBeam.startWidth = lightBeam.endWidth = lightRadius;
        }

        // Find end point
        Vector3 lightBeamEndPos;
        if (hitSomething)
        {
            lightBeamEndPos = lightSpawnPoint.InverseTransformPoint(castHit.point);
            lightBeamEndPos.x = 0;
        }
        else lightBeamEndPos = new Vector3(0, 0, lightLength);
        

        // Set points on line
        lightBeam.positionCount = 2;
        lightBeam.SetPosition(0, Vector3.zero);
        lightBeam.SetPosition(1, lightBeamEndPos);

        // Check if light hit something
        if (!hitSomething)
        {
            if (currentlyHitObject == null) return;

            DeactivateCurrentlyHitBeacon();
            return;
        }

        if (castHit.transform == currentlyHitObject) return;


        if (currentlyHitObject != null) DeactivateCurrentlyHitBeacon();
        currentlyHitObject = castHit.transform;
        if (!castHit.transform.TryGetComponent<LightPillar>(out LightPillar hitBeacon)) return;
        hitBeacon.Activate();
    }

    private void Update()
    {
        if (!Active) return;

        CastLight();
    }

    private void LateUpdate()
    {
        if (!Active && !remainCastingAfterActive) Destroy(lightBeam);
    }

    private void DeactivateCurrentlyHitBeacon()
    {
        if (currentlyHitObject.transform.TryGetComponent<LightPillar>(out LightPillar currentlyHitBeacon)) currentlyHitBeacon.Deactivate();
        currentlyHitObject = null;
    }
}
