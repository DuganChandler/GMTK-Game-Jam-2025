using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour, IInteractable<PlayerController> {
    [Tooltip("Units per second along the chosen axis")]
    public float pushSpeed = 3f;

    void Awake() {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public IEnumerator Interact(Transform playerT, PlayerController playerController) {
        if (!playerController) yield break;

        playerT.SetParent(transform, worldPositionStays: true);

        Vector3 toPlayer = playerT.position - transform.position;
        float dotR = Vector3.Dot(toPlayer, transform.right);
        float dotF = Vector3.Dot(toPlayer, transform.forward);

        Vector3 slideAxis = Mathf.Abs(dotR) > Mathf.Abs(dotF)
            ? transform.right
            : transform.forward; 

        float sideSign = (Mathf.Abs(dotR) > Mathf.Abs(dotF) ? dotR : dotF) >= 0 ? 1f : -1f;

        while (playerController.IsInteracting) {
            float f = playerController.MoveInput.z;
            if (Mathf.Abs(f) > 0.1f) {
                Vector3 dir = -sideSign * f * slideAxis;
                transform.position += pushSpeed * Time.fixedDeltaTime * dir;
            }

            yield return new WaitForFixedUpdate();
        }

        playerT.SetParent(null, worldPositionStays: true);
    }
}
