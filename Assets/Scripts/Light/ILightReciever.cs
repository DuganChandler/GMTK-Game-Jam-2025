using System.Collections.Generic;
using UnityEngine;

public interface ILightReciever
{
    public List<BeamData> CurrentlyRecievingBeams { get; }
    public bool IsSource { get; }

    public void OnBeamRecieved(BeamData data);
    public void OnBeamStopRecieved(BeamData data);
}
