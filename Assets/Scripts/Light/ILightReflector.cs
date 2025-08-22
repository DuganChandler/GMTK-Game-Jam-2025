using System.Collections.Generic;

public interface ILightReflector
{
    public List<BeamData> CurrentlyReflectedBeams { get; }

    public void ReflectBeams();
}
