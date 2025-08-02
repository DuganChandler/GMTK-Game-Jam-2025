using UnityEngine;
using UnityEngine.Events;

public class LightReciever : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected int lightLevelRequiredToActivate;

    [SerializeField] private UnityEvent onActivation;

    public void Activate(int lightLevel)
    {
        if (lightLevel < lightLevelRequiredToActivate) return;

        onActivation?.Invoke();
    }
}
