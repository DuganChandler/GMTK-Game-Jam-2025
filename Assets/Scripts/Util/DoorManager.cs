using UnityEngine;

public class DoorManager : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject trigger;

    void OnEnable() {
        LevelManager.OnRoomSolved += OpenDoor;
    }

    void OnDisable() {
        LevelManager.OnRoomSolved -= OpenDoor;
    }

    void OpenDoor() {
        animator.SetTrigger("Open");
        trigger.SetActive(true);
    }
}
