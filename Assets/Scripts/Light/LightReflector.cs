using System.Collections.Generic;
using UnityEngine;

public class LightReflector : MonoBehaviour
{
    [Header("Pillar Settings")]
    [SerializeField] protected bool remainCastingAfterActive = false;
    [SerializeField] private bool castOnStart = false;
    [SerializeField] protected LayerMask lightLayer;
    [SerializeField] protected float lightRadius = 0.25f;
    [SerializeField] protected float lightLength = 15f;

    [Header("Prefabs")]
    [SerializeField] protected LineRenderer linePrefab;

    [Header("Colors")]
    [SerializeField] private int numberOfLightUpMaterial = -1;
    [SerializeField] protected float lightGlowAmount = 4;
    [SerializeField] protected Color normalColor;
    [SerializeField] protected Color level1Color;
    [SerializeField] protected Color level2Color;
    [SerializeField] protected Color level3Color;
    [SerializeField] protected Color level4Color;
    [SerializeField] protected Color level5Color;

    [Header("Components")]
    [SerializeField] protected Transform lightSpawnPoint;

    public bool Active { get; protected set; }

    protected Material lightUpMaterial;
    protected LineRenderer lightBeam;
    private Renderer rend;
    protected Transform currentlyHitObject;
    protected int lightLevel;
    protected List<int> lightsGoingIntoThis;

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

        if (castOnStart) ActivateStart(1);
    }

    public virtual void ActivateStart(int levelGoingIn)
    {
        if (lightBeam != null) Destroy(lightBeam.gameObject);
        DeactivateCurrentlyHitReflector();

        Active = true;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();
    }

    public bool Activate(int levelGoingIn)
    {
        Debug.Log("Activate called");
        if (castOnStart || remainCastingAfterActive) return false;

        if (lightBeam != null) Destroy(lightBeam.gameObject);
        DeactivateCurrentlyHitReflector();

        Active = true;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();

        return true;
    }

    public virtual void Deactivate(int levelGoingOut)
    {
        DeactivateCurrentlyHitReflector();

        lightsGoingIntoThis.Remove(levelGoingOut);
        lightLevel = CalculateLightLevel();

        if (lightLevel <= 0) Active = false;
    }

    protected virtual void CastLight()
    {
        bool hitSomething = Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPoint.forward, out RaycastHit castHit, lightLength, lightLayer);

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
            DeactivateCurrentlyHitReflector();
            return;
        }

        if (castHit.collider.transform == currentlyHitObject) return;


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

    protected virtual void LateUpdate()
    {
        if (!Active && !remainCastingAfterActive) 
            if (lightBeam != null)
            {
                Destroy(lightBeam.gameObject);
                lightUpMaterial.SetColor("_BaseColor", GetColorForLightLevel());
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

    protected virtual void DeactivateCurrentlyHitReflector()
    {
        Transform temp = currentlyHitObject;
        currentlyHitObject = null;
        if (temp != null)
        {
            if (temp.transform.TryGetComponent<LightReflector>(out LightReflector currentlyHitReflector)) currentlyHitReflector.Deactivate(lightLevel);
            else if (temp.transform.TryGetComponent<LightAmplifier>(out LightAmplifier currentlyHitAmplifier)) currentlyHitAmplifier.Deactivate(lightLevel);
        }
    }

    protected int CalculateLightLevel()
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
