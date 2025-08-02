using System.Collections.Generic;
using UnityEngine;

public class LightReflector : MonoBehaviour
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

    public bool Active { get; private set; }

    private LineRenderer lightBeam;
    private Transform currentlyHitObject;
    private int lightLevel;
    [SerializeField] private List<int> lightsGoingIntoThis;

    private void Start()
    {
        lightsGoingIntoThis = new();
        if (castOnStart) Activate(1);
    }

    [ContextMenu("Activate")]
    public void Activate(int levelGoingIn)
    {
        if (lightBeam != null) Destroy(lightBeam.gameObject);
        if (currentlyHitObject != null) DeactivateCurrentlyHitReflector();

        Active = true;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();
    }

    [ContextMenu("Deactivate")]
    public void Deactivate(int levelGoingOut)
    {
        if (currentlyHitObject != null) DeactivateCurrentlyHitReflector();

        lightsGoingIntoThis.Remove(levelGoingOut);
        lightLevel = CalculateLightLevel();

        if (lightLevel <= 0) Active = false;
    }

    private void CastLight()
    {
        bool hitSomething = Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPoint.forward, out RaycastHit castHit, lightLength, lightLayer);

        // Spawn light
        if (lightBeam == null)
        {
            lightBeam = Instantiate(linePrefab, lightSpawnPoint);
            lightBeam.startWidth = lightBeam.endWidth = lightRadius;
        }

        // Color light
        Color lightColor = lightLevel switch
        {
            0 => Color.white,
            1 => Color.white,
            2 => Color.yellow,
            3 => new Color(1, 0.5f, 0),
            4 => Color.red,
            5 => new Color(0.5f, 0, 1),
            _ => throw new System.NotImplementedException(),
        };
        lightBeam.startColor = lightBeam.endColor = lightColor;

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

            DeactivateCurrentlyHitReflector();
            return;
        }

        if (castHit.transform == currentlyHitObject) return;


        if (currentlyHitObject != null) DeactivateCurrentlyHitReflector();
        currentlyHitObject = castHit.transform;
        if (!castHit.transform.TryGetComponent<LightReflector>(out LightReflector hitReflector) || lightLevel <= 0) return;
        hitReflector.Activate(lightLevel);
    }

    private void Update()
    {
        if (!Active) return;

        CastLight();
    }

    private void LateUpdate()
    {
        if (!Active && !remainCastingAfterActive) if (lightBeam != null) Destroy(lightBeam.gameObject);
    }

    private void DeactivateCurrentlyHitReflector()
    {
        if (currentlyHitObject.transform.TryGetComponent<LightReflector>(out LightReflector currentlyHitReflector)) currentlyHitReflector.Deactivate(lightLevel);
        currentlyHitObject = null;
    }

    private int CalculateLightLevel()
    {
        lightsGoingIntoThis.Sort((x, y) => y.CompareTo(x));

        //lightsGoingIntoThis.Sort();
        //lightsGoingIntoThis.Reverse();

        if (lightsGoingIntoThis.Count == 1) return lightsGoingIntoThis[0];
        else if (lightsGoingIntoThis.Count >= 2)
        {
            if (lightsGoingIntoThis[0] == lightsGoingIntoThis[1]) return lightsGoingIntoThis[0] + 1;
            else return lightsGoingIntoThis[0];
        }

        return 0;
    }
}
