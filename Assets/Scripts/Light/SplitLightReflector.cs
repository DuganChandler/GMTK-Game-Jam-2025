using UnityEngine;

public class SplitLightReflector : LightReflector
{
    [SerializeField] private Transform lightSpawnPointTwo;
    private LineRenderer lightBeamTwo;
    private Transform currentlyHitObjectTwo;

    public override void ActivateStart(int levelGoingIn)
    {
        if (lightBeam != null) Destroy(lightBeam.gameObject);
        DeactivateCurrentlyHitReflector();
        DeactivateCurrentlyHitReflectorTwo();

        Active = true;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();
    }

    public override bool Activate(int levelGoingIn)
    {
        if (castOnStart || remainCastingAfterActive) return false;

        if (lightBeam != null) Destroy(lightBeam.gameObject);
        DeactivateCurrentlyHitReflector();
        DeactivateCurrentlyHitReflectorTwo();

        Active = true;

        lightsGoingIntoThis.Add(levelGoingIn);
        lightLevel = CalculateLightLevel();

        return true;
    }

    public override void Deactivate(int levelGoingOut)
    {
        DeactivateCurrentlyHitReflector();
        DeactivateCurrentlyHitReflectorTwo();

        lightsGoingIntoThis.Remove(levelGoingOut);
        lightLevel = CalculateLightLevel();

        if (lightLevel <= 0) Active = false;
    }

    protected override void CastLight()
    {
        if (rotatable != null && rotatable.Busy)
        {
            if (lightBeam != null)
            {
                DeactivateCurrentlyHitReflector();
                Destroy(lightBeam.gameObject);
            }
            if (lightBeamTwo != null)
            {
                DeactivateCurrentlyHitReflectorTwo();
                Destroy(lightBeamTwo.gameObject);
            }
            return;
        }

        bool hitSomething = Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPoint.forward, out RaycastHit castHit, lightLength, lightLayer);
        bool hitSomethingTwo = Physics.SphereCast(lightSpawnPoint.position, lightRadius, lightSpawnPointTwo.forward, out RaycastHit castHitTwo, lightLength, lightLayer);

        // Spawn light
        if (lightBeam == null)
        {
            lightBeam = Instantiate(linePrefab, lightSpawnPoint);
            lightBeamTwo = Instantiate(linePrefab, lightSpawnPointTwo);
            lightBeam.startWidth = lightBeam.endWidth = lightRadius;
            lightBeamTwo.startWidth = lightBeamTwo.endWidth = lightRadius;
        }

        // Color light
        lightBeam.startColor = lightBeam.endColor = GetColorForLightLevel();
        lightBeamTwo.startColor = lightBeamTwo.endColor = GetColorForLightLevel();

        // Color any additional materials
        lightUpMaterial.SetColor("_BaseColor", GetColorForLightLevel() * lightGlowAmount);

        // Find end point
        Vector3 lightBeamEndPos;
        if (hitSomething)
        {
            lightBeamEndPos = lightSpawnPoint.InverseTransformPoint(castHit.point);
            lightBeamEndPos.x = 0;
        }
        else lightBeamEndPos = new Vector3(0, 0, lightLength);

        // Find second end point
        Vector3 lightBeamEndPosTwo;
        if (hitSomethingTwo)
        {
            lightBeamEndPosTwo = lightSpawnPointTwo.InverseTransformPoint(castHitTwo.point);
            lightBeamEndPosTwo.x = 0;
        }
        else lightBeamEndPosTwo = new Vector3(0, 0, lightLength);


        // Set points on line
        lightBeam.positionCount = 2;
        lightBeam.SetPosition(0, Vector3.zero);
        lightBeam.SetPosition(1, lightBeamEndPos);

        // Set points on line 2
        lightBeamTwo.positionCount = 2;
        lightBeamTwo.SetPosition(0, Vector3.zero);
        lightBeamTwo.SetPosition(1, lightBeamEndPosTwo);

        // Check if light hit something
        if (!hitSomething)
        {
            DeactivateCurrentlyHitReflector();
        }
        else
        {
            if (castHit.collider.transform != currentlyHitObject && castHit.collider.transform != transform)
            {
                DeactivateCurrentlyHitReflector();
                currentlyHitObject = castHit.collider.transform;

                if (lightLevel > 0)
                {
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
            }
        }



        // Check if light hit something Two
        if (!hitSomethingTwo)
        {
            DeactivateCurrentlyHitReflectorTwo();
        }
        else
        {
            if (castHitTwo.collider.transform != currentlyHitObjectTwo && castHitTwo.collider.transform != transform)
            {
                DeactivateCurrentlyHitReflectorTwo();
                currentlyHitObjectTwo = castHitTwo.collider.transform;

                if (lightLevel > 0)
                {
                    if (currentlyHitObjectTwo.TryGetComponent<LightReflector>(out LightReflector hitReflectorTwo))
                    {
                        if (!hitReflectorTwo.Activate(lightLevel)) currentlyHitObject = null;
                        return;
                    }
                    else if (currentlyHitObjectTwo.TryGetComponent<LightAmplifier>(out LightAmplifier amplifierTwo))
                    {
                        if (!amplifierTwo.Activate(lightLevel, lightSpawnPointTwo.forward)) currentlyHitObject = null;
                    }
                    else if (currentlyHitObjectTwo.TryGetComponent<LightReciever>(out LightReciever recieverTwo))
                    {
                        if (!recieverTwo.Activate(lightLevel)) currentlyHitObject = null;
                    }
                }
            }
        }
    }

    protected override void LateUpdate()
    {
        if (!Active && !remainCastingAfterActive) if (lightBeam != null) Destroy(lightBeam.gameObject);
        if (!Active && !remainCastingAfterActive) if (lightBeamTwo != null) Destroy(lightBeamTwo.gameObject);
    }

    protected override void DeactivateCurrentlyHitReflector()
    {
        if (currentlyHitObject != null)
        {
            Transform temp = currentlyHitObject;
            currentlyHitObject = null;

            if (temp.transform.TryGetComponent<LightReflector>(out LightReflector currentlyHitReflector)) currentlyHitReflector.Deactivate(lightLevel);
            else if (temp.transform.TryGetComponent<LightAmplifier>(out LightAmplifier currentlyHitAmplifier)) currentlyHitAmplifier.Deactivate(lightLevel);
        }
    }

    private void DeactivateCurrentlyHitReflectorTwo()
    {
        if (currentlyHitObjectTwo != null)
        {
            Transform temp = currentlyHitObjectTwo;
            currentlyHitObjectTwo = null;

            if (temp.transform.TryGetComponent<LightReflector>(out LightReflector currentlyHitReflectorTwo)) currentlyHitReflectorTwo.Deactivate(lightLevel);
            else if (temp.transform.TryGetComponent<LightAmplifier>(out LightAmplifier currentlyHitAmplifierTwo)) currentlyHitAmplifierTwo.Deactivate(lightLevel);
        }
    }
}
