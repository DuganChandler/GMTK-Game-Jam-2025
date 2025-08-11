using UnityEngine;

public readonly struct BeamData
{
    readonly Color color;
    readonly int power;
    readonly Vector3 origin;
    readonly Vector3 direction;
    readonly ILightReciever sourceReciever;
    readonly ILightReciever targetReciever;
    readonly Transform targetRecieverTransform;

    public BeamData(Color color, int power, Vector3 origin, Vector3 direction, ILightReciever source, ILightReciever target, Transform targetTransform)
    {
        this.color = color;
        this.power = power;
        this.origin = origin;
        this.direction = direction;
        this.sourceReciever = source;
        this.targetReciever = target;
        this.targetRecieverTransform = targetTransform;
    }

    public Color Color => color;
    public int Power => power;
    public Vector3 Origin => origin;
    public Vector3 Direction => direction;
    public ILightReciever SourceReciever => sourceReciever;
    public ILightReciever TargetReciever => targetReciever;
    public Transform TargetRecieverTransform => targetRecieverTransform;
}
