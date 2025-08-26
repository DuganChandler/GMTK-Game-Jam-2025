using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool levelEnder = false;

    [SerializeField] private Animator animator;
    [SerializeField] private GameObject trigger;
    [SerializeField] private Room linkedRoom;
    [SerializeField] private Vector3Int connectedPos;

    public static event Action<Room> OnCurrentRoomChange;
    public static event Action<Vector3Int> OnPlayerEnterRoom;

    void OpenDoor()
    {
        animator.SetTrigger("Open");
        trigger.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!levelEnder)
        {
            // Teleport player to connected spawn point

            // Have camera manager transition to room associated with connected spawn point
            OnCurrentRoomChange?.Invoke(linkedRoom);
            OnPlayerEnterRoom?.Invoke(connectedPos);
        }
    }
}
