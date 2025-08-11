using System.Collections.Generic;
using UnityEngine;

public class SplitBallBro : MonoBehaviour, ILightReciever, ILightReflector
{
    [SerializeField] private bool isSource;

    [Header("Line Settings")]
    [SerializeField] private Transform lineSpawnPoint;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private float lineLength = 75f;
    [SerializeField] private float lineWidth = 0.25f;
    [SerializeField] private LayerMask lineHitLayers;

    [Header("Color Settings")]
    [SerializeField] protected float lightGlowAmount = 4;
    [SerializeField] protected Color level1Color = Color.white;
    [SerializeField] protected Color level2Color = Color.yellow;
    [SerializeField] protected Color level3Color = new(1, 0.5f, 0);
    [SerializeField] protected Color level4Color = Color.red;
    [SerializeField] protected Color level5Color = new(1, 0, 1);
    [SerializeField] protected Color level6Color = Color.blue;
    [SerializeField] protected Color level7Color = Color.green;

    public bool IsLit { get; private set; }
    public bool IsSource => isSource;
    public List<BeamData> CurrentlyRecievingBeams => currentlyRecievingBeams;
    public List<BeamData> CurrentlyReflectedBeams => currentlyReflectedBeams;

    public List<BeamData> currentlyRecievingBeams;
    private int reflectedBeamLevel;
    private List<BeamData> currentlyReflectedBeams;
    private LineRenderer[] spawnedLines;

    #region Unity Methods
    private void Awake()
    {
        currentlyRecievingBeams = new();
        currentlyReflectedBeams = new();
        spawnedLines = new LineRenderer[2];

        if (isSource)
        {
            IsLit = true;
            currentlyReflectedBeams.Add(new(level1Color, 1, lineSpawnPoint.position, lineSpawnPoint.forward, this, this, null, null));
        }
    }

    private void Update()
    {
        if (!IsTrulyBeingHitBySomething())
        {
            for (int i = 0; i < currentlyRecievingBeams.Count; i++)
            {
                OnBeamStopRecieved(currentlyRecievingBeams[0]);
            }
        }

        if (IsLit)
        {
            ReflectBeams();
        }
    }
    #endregion

    #region Recieving Light
    public void OnBeamRecieved(BeamData data)
    {
        if (isSource) return;

        if (GetLightPower() == reflectedBeamLevel)
        {
            if (currentlyRecievingBeams.Contains(data)) return;
        }
        
        if (!CheckIfSameReciever(data)) currentlyRecievingBeams.Add(data);

        currentlyReflectedBeams.Clear();

        Quaternion rotation = Quaternion.AngleAxis(45, Vector3.up);
        Vector3 leftDirection = rotation * lineSpawnPoint.forward;
        Vector3 rightDirection = new(-leftDirection.x, leftDirection.y, leftDirection.z);

        if (isSource) reflectedBeamLevel = 1;
        else reflectedBeamLevel = GetLightPower();

        BeamData beamToShootLeft = new(GetLightColorFromPower(), reflectedBeamLevel, lineSpawnPoint.position, leftDirection, this, this, null, null);
        BeamData beamToShootRight = new(GetLightColorFromPower(), reflectedBeamLevel, lineSpawnPoint.position, rightDirection, this, this, null, null);

        currentlyReflectedBeams.Add(beamToShootLeft);
        currentlyReflectedBeams.Add(beamToShootRight);

        IsLit = true;
    }

    public void OnBeamStopRecieved(BeamData data)
    {
        if (isSource) return;
        if (currentlyRecievingBeams.Count == 0) return;

        currentlyRecievingBeams.Remove(data);
        print("Remove Split");

        if (currentlyRecievingBeams.Count == 0)
        {
            for (int i = 0; i < currentlyReflectedBeams.Count; i++)
            {
                currentlyReflectedBeams[i].TargetReciever?.OnBeamStopRecieved(currentlyReflectedBeams[i]);
            }
            DeactivateLight();
            return;
        }

        if (!CheckPathForSource(this))
        {
            for (int i = 0; i < currentlyReflectedBeams.Count; i++)
            {
                currentlyReflectedBeams[i].TargetReciever?.OnBeamStopRecieved(currentlyReflectedBeams[i]);
            }
            DeactivateLight();
            return;
        }


    }
    #endregion

    #region Reflecting Light
    public void ReflectBeams()
    {
        for (int i = 0; i < currentlyReflectedBeams.Count; i++)
        {
            // Raycast
            //bool hitSomething = Physics.Raycast(currentlyReflectedBeams[i].Origin, currentlyReflectedBeams[i].Direction, out RaycastHit castHit, lineLength, lineHitLayers);
            bool hitSomething = Physics.SphereCast(currentlyReflectedBeams[i].Origin, 0.05f, lineSpawnPoint.TransformDirection(currentlyReflectedBeams[i].Direction), out RaycastHit castHit, lineLength, lineHitLayers);

            // Find end position of line
            Vector3 lightBeamEndPos;
            if (hitSomething) lightBeamEndPos = lineSpawnPoint.InverseTransformPoint(castHit.point);
            else lightBeamEndPos = currentlyReflectedBeams[i].Direction * lineLength;
            lightBeamEndPos.y = 0;

            // Spawn linerenderer
            if (spawnedLines[i] == null)
            {
                spawnedLines[i] = Instantiate(linePrefab, lineSpawnPoint);
            }

            // Set points on line
            spawnedLines[i].positionCount = 2;
            spawnedLines[i].SetPosition(0, Vector3.zero);
            spawnedLines[i].SetPosition(1, lightBeamEndPos);

            spawnedLines[i].startColor = spawnedLines[i].endColor = currentlyReflectedBeams[i].Color;

            // Set the line width
            spawnedLines[i].startWidth = spawnedLines[i].endWidth = lineWidth;

            // Deactivate previous hit object if applicable
            if (currentlyReflectedBeams[i].TargetReciever != null) // If hitting a reciever last frame
            {
                // If a non receiver object interupts the connection to a receiver object
                if (castHit.collider != null && currentlyReflectedBeams[i].TargetRecieverTransform != castHit.collider.transform) // If the receiver hit last frame is a different object from the one hit this frame 
                {
                    currentlyReflectedBeams[i].TargetReciever.OnBeamStopRecieved(currentlyReflectedBeams[i]);
                    currentlyReflectedBeams[i] = new(currentlyReflectedBeams[i].Color, currentlyReflectedBeams[i].Power, currentlyReflectedBeams[i].Origin, currentlyReflectedBeams[i].Direction, this, this, null, null);
                }

                // If a reciever object moves out of the way of the beam
                if (castHit.collider == null && currentlyReflectedBeams[i].TargetReciever != null)
                {
                    currentlyReflectedBeams[i].TargetReciever.OnBeamStopRecieved(currentlyReflectedBeams[i]);
                    currentlyReflectedBeams[i] = new(currentlyReflectedBeams[i].Color, currentlyReflectedBeams[i].Power, currentlyReflectedBeams[i].Origin, currentlyReflectedBeams[i].Direction, this, this, null, null);
                }
            }

            // Activate currently hit object if applicable
            if (castHit.collider != null && castHit.collider.TryGetComponent(out ILightReciever hitReciever))
            {
                currentlyReflectedBeams[i] = new(currentlyReflectedBeams[i].Color, currentlyReflectedBeams[i].Power, currentlyReflectedBeams[i].Origin, currentlyReflectedBeams[i].Direction, this, this, hitReciever, castHit.collider.transform);
                hitReciever?.OnBeamRecieved(currentlyReflectedBeams[i]);
            }
        }

        //// Color any additional materials
        //if (lightUpMaterial != null) lightUpMaterial.SetColor("_BaseColor", GetColorForLightLevel() * lightGlowAmount);
    }
    private void DeactivateLight()
    {
        IsLit = false;

        for (int i = 0; i < spawnedLines.Length; i++)
        {
            Destroy(spawnedLines[i].gameObject);
        }
    }
    #endregion

    #region Utility Methods
    private bool CheckIfSameReciever(BeamData data)
    {
        if (reflectedBeamLevel == 0) return false;

        for (int i = 0; i < currentlyRecievingBeams.Count; i++)
        {
            BeamData currentBeamData = currentlyRecievingBeams[i];
            if (currentBeamData.SourceReciever == data.SourceReciever)
            {
                currentlyRecievingBeams[i] = data;
                return true;
            }
        }

        return false;
    }

    private Color GetLightColorFromPower() =>
    reflectedBeamLevel switch
    {
        1 => level1Color,
        2 => level2Color,
        3 => level3Color,
        4 => level4Color,
        5 => level5Color,
        6 => level6Color,
        7 => level7Color,
        _ => throw new System.NotImplementedException(),
    };

    private int GetLightPower()
    {
        if (currentlyRecievingBeams.Count == 0) return 0;

        currentlyRecievingBeams.Sort((a, b) => b.Power.CompareTo(a.Power));

        if (currentlyRecievingBeams.Count == 1)
        {
            return currentlyRecievingBeams[0].Power;
        }

        if (currentlyRecievingBeams[0].Power == currentlyRecievingBeams[1].Power)
        {
            return currentlyRecievingBeams[0].Power + 1;
        }
        else
        {
            return currentlyRecievingBeams[0].Power;
        }
    }

    public bool CheckPathForSource(ILightReciever root, List<ILightReciever> visitedRecievers = null)
    {
        if (root.IsSource) return true;

        visitedRecievers ??= new();

        if (visitedRecievers.Contains(root)) return false;

        visitedRecievers.Add(root);

        foreach (BeamData data in root.CurrentlyRecievingBeams)
        {
            if (CheckPathForSource(data.SourceReciever, visitedRecievers)) return true;
        }

        return false;
    }

    private bool IsTrulyBeingHitBySomething()
    {
        if (isSource) return true;

        foreach (var beamGoingIntoThis in currentlyRecievingBeams)
        {
            foreach (var beamBeingShotOutOfSource in beamGoingIntoThis.SourceReflector.CurrentlyReflectedBeams)
            {
                if (beamBeingShotOutOfSource.TargetRecieverTransform == transform)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}
