using UnityEngine;

public class LightReciever : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] protected int lightLevelRequiredToActivate;
    [SerializeField] LevelManager levelManager;

    public bool Activate(int lightLevel) {
        if (lightLevel < lightLevelRequiredToActivate) return false;

        levelManager.TriggerLevelSolved();

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;

        return true;
    }
}
