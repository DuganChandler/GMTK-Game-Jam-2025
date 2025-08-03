using UnityEngine;

public class DoorManager : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject trigger;

    void OnEnable() {
        LevelManager.OnLevelSolved += OpenDoor;
    }

    void OnDisable() {
        LevelManager.OnLevelSolved -= OpenDoor;
    }

    void OpenDoor() {
        animator.SetTrigger("Open");
        trigger.SetActive(true);
    }
}
