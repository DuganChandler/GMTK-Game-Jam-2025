using System.Collections;
using UnityEngine;

public interface IInteractable<T> {
    IEnumerator Interact(Transform initiator, T data);
}