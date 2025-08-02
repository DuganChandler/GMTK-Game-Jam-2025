using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour, IInteractable<PlayerController> {
    [Tooltip("Units per second along the chosen axis")]
    public float pushSpeed = 3f;
    private Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;                                // ← dynamic now
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete; 
    }

    public IEnumerator Interact(Transform playerT, PlayerController playerController) {
        if (!playerController) yield break;

        if (!playerController.TryGetComponent<Rigidbody>(out var playerRb)) yield break;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

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
                Vector3 delta = pushSpeed * Time.fixedDeltaTime * dir;

                rb.MovePosition(transform.position + delta);
                playerRb.MovePosition(playerT.position + delta);
            }

            yield return new WaitForFixedUpdate();
        }

        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }
}
