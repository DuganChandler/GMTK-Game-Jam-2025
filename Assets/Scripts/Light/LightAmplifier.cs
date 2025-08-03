using System.Collections.Generic;
using UnityEngine;

public class LightAmplifier : MonoBehaviour
{
    [Header("Pillar Settings")]
    [SerializeField] private bool remainCastingAfterActive = false;
    [SerializeField] private bool castOnStart = false;
    [SerializeField] private bool isOmnidirectional = false;
    [SerializeField] private int initialLightLevel = 1;
    [SerializeField] private LayerMask lightLayer;
    [SerializeField] private float lightRadius = 0.25f;
    [SerializeField] private float lightLength = 15f;

    [Header("Objects")]
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private Transform lightSpawnPoint;

    [Header("Colors")]
    [SerializeField] private int numberOfLightUpMaterial = -1;
    [SerializeField] private int numberOfLightUpMaterialTwo = -1;
    [SerializeField] private float lightGlowAmount = 4;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color level1Color;
    [SerializeField] private Color level2Color;
    [SerializeField] private Color level3Color;
    [SerializeField] private Color level4Color;
    [SerializeField] private Color level5Color;

    public bool Active { get; private set; }

    private Material lightUpMaterial;
    private Material lightUpMaterialTwo;
    private LineRenderer lightBeam;
    private Renderer rend;
    private Transform currentlyHitObject;
    private int lightLevel;
    private List<int> lightsGoingIntoThis;

    private Vector3 directionOfSourceLight;

    private void Awake()
    {
        TryGetComponent(out rend);
    }

    private void Start()
    {
        lightsGoingIntoThis = new();

        if (rend != null && numberOfLightUpMaterial > -1 && numberOfLightUpMaterial < rend.materials.Length)
        {
            lightUpMaterial = rend.materials[numberOfLightUpMaterial];
        }

        if (rend != null && numberOfLightUpMaterialTwo > -1 && numberOfLightUpMaterialTwo < rend.materials.Length)
        {
            lightUpMaterialTwo = rend.materials[numberOfLightUpMaterialTwo];
        }

        if (castOnStart) Activate(initialLightLevel, transform.forward);
    }

    public void ActivateStart(int levelGoingIn, Vector3 direction)
    {
        if (lightBeam != null) Destroy(lightBeam.gameObject);
        DeactivateCurrentlyHitReflector();

        Active = true;

        directionOfSourceLight = direction;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();
    }

    public virtual bool Activate(int levelGoingIn, Vector3 direction)
    {
        if (castOnStart || remainCastingAfterActive) return false;

        if (lightBeam != null) Destroy(lightBeam.gameObject);
        DeactivateCurrentlyHitReflector();

        Active = true;

        directionOfSourceLight = direction;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();

        return true;
    }

    public void Deactivate(int levelGoingOut)
    {
        DeactivateCurrentlyHitReflector();

        lightsGoingIntoThis.Remove(levelGoingOut);
        lightLevel = CalculateLightLevel();

        if (lightLevel <= 0) Active = false;
    }

    private void CastLight()
    {
        if (!isOmnidirectional && (lightSpawnPoint.forward != directionOfSourceLight && lightSpawnPoint.forward != -directionOfSourceLight))
        {
            if (lightBeam != null) Destroy(lightBeam);
            return;
        }

        bool hitSomething = Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPoint.TransformDirection(lightSpawnPoint.InverseTransformDirection(directionOfSourceLight)), out RaycastHit castHit, lightLength, lightLayer);

        // Spawn light
        if (lightBeam == null)
        {
            lightBeam = Instantiate(linePrefab, lightSpawnPoint);
            lightBeam.startWidth = lightBeam.endWidth = lightRadius;
        }

        // Color light
        lightBeam.startColor = lightBeam.endColor = GetColorForLightLevel();

        // Color any additional materials
        if (lightUpMaterial != null) lightUpMaterial.SetColor("_BaseColor", GetColorForLightLevel() * lightGlowAmount);
        if (lightUpMaterialTwo != null) lightUpMaterialTwo.SetColor("_BaseColor", GetColorForLightLevel() * lightGlowAmount);

        // Find end point
        Vector3 lightBeamEndPos;
        if (hitSomething)
        {
            lightBeamEndPos = lightSpawnPoint.InverseTransformPoint(castHit.point);
            lightBeamEndPos.y = 0;
        }
        else lightBeamEndPos = lightSpawnPoint.InverseTransformDirection(directionOfSourceLight) * lightLength;

        // Set points on line
        lightBeam.positionCount = 2;
        lightBeam.SetPosition(0, Vector3.zero);
        lightBeam.SetPosition(1, lightBeamEndPos);

        // Check if light hit something
        if (!hitSomething)
        {
            DeactivateCurrentlyHitReflector();
            return;
        }

        if (castHit.collider.transform == currentlyHitObject || castHit.collider.transform == transform) return;


        DeactivateCurrentlyHitReflector();
        currentlyHitObject = castHit.collider.transform;

        if (lightLevel <= 0) return;

        if (currentlyHitObject.TryGetComponent<LightReflector>(out LightReflector hitReflector))
        {
            if (!hitReflector.Activate(lightLevel)) currentlyHitObject = null;
        }
        else if (currentlyHitObject.TryGetComponent<LightAmplifier>(out LightAmplifier amplifier))
        {
            if (!amplifier.Activate(lightLevel, lightSpawnPoint.forward)) currentlyHitObject = null;
        }
        else if (currentlyHitObject.TryGetComponent<LightReciever>(out LightReciever reciever))
        {
            if (!reciever.Activate(lightLevel)) currentlyHitObject = null;
        }
    }

    private void Update()
    {
        if (!Active) return;

        CastLight();
    }

    private void LateUpdate()
    {
        if (!Active && !remainCastingAfterActive)
            if (lightBeam != null)
            {
                Destroy(lightBeam.gameObject);
                if (lightUpMaterial != null) lightUpMaterial.SetColor("_BaseColor", GetColorForLightLevel());
                if (lightUpMaterialTwo) lightUpMaterialTwo.SetColor("_BaseColor", GetColorForLightLevel());
            }
    }

    protected virtual Color GetColorForLightLevel() =>
        lightLevel switch
        {
            0 => normalColor,
            1 => level1Color,
            2 => level2Color,
            3 => level3Color,
            4 => level4Color,
            5 => level5Color,
            _ => throw new System.NotImplementedException(),
        };

    private void DeactivateCurrentlyHitReflector()
    {
        Transform temp = currentlyHitObject;
        currentlyHitObject = null;
        if (temp != null)
        {
            if (temp.transform.TryGetComponent<LightReflector>(out LightReflector currentlyHitReflector)) currentlyHitReflector.Deactivate(lightLevel);
            else if (temp.transform.TryGetComponent<LightAmplifier>(out LightAmplifier currentlyHitAmplifier)) currentlyHitAmplifier.Deactivate(lightLevel);
        }
    }

    private int CalculateLightLevel()
    {
        if (lightsGoingIntoThis.Count == 0) return 0;

        lightsGoingIntoThis.Sort((x, y) => y.CompareTo(x));

        int newLightLevel = 0;

        if (lightsGoingIntoThis.Count == 1) newLightLevel = lightsGoingIntoThis[0];
        else if (lightsGoingIntoThis.Count >= 2)
        {
            if (lightsGoingIntoThis[0] == lightsGoingIntoThis[1]) newLightLevel = lightsGoingIntoThis[0] + 1;
            else return lightsGoingIntoThis[0];
        }

        return newLightLevel + 1;
    }

    private void OnDrawGizmos()
    {
        Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPoint.InverseTransformDirection(directionOfSourceLight), out RaycastHit castHit, lightLength, lightLayer);
        Debug.DrawLine(lightSpawnPoint.position, castHit.point);
    }
}
