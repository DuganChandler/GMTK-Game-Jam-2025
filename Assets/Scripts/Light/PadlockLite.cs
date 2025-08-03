using UnityEngine;
using UnityEngine.Events;

public class PadlockLite : Padlock
{
    [SerializeField] private UnityEvent onActivate;

    public override bool Activate(int lightLevel)
    {
        if (lightLevel < lightLevelRequiredToActivate) return false;

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;

        onActivate?.Invoke();

        return true;
    }
}
