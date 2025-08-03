using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PushableObject : MonoBehaviour, IInteractable<PlayerController>
{
    private readonly float pushSpeed = 5f;
    [Tooltip("Which layers count as an obstacle?")]
    public LayerMask wallLayerMask;

    private Rigidbody rb;
    private Collider col;
    private Vector3 halfExtents;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        halfExtents = col.bounds.extents;

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    public IEnumerator Interact(Transform playerT, PlayerController playerController) {
        if (playerController == null) yield break;
        if (!playerController.TryGetComponent<Rigidbody>(out var playerRb)) yield break;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // ──────────────── LOCK ────────────────
        var fj = gameObject.AddComponent<FixedJoint>();
        fj.connectedBody = playerRb;
        fj.breakForce = Mathf.Infinity;
        fj.breakTorque = Mathf.Infinity;
        fj.enableCollision = false;  // avoid self-collision
        // ───────────────────────────────────────

        while (playerController.IsInteracting)
        {
            Vector3 rawInput = playerController.MoveInput;
            Vector3 planar   = new Vector3(rawInput.x, 0f, rawInput.z);
            if (planar.sqrMagnitude > 0.01f)
            {
                Transform cam = Camera.main.transform;
                Vector3 f = cam.forward; f.y = 0; f.Normalize();
                Vector3 r = cam.right;   r.y = 0; r.Normalize();
                Vector3 dir = (planar.x*r + planar.z*f).normalized;

                float dist = pushSpeed * Time.fixedDeltaTime;

                if (!Physics.CapsuleCast(
                        playerRb.position + Vector3.up*0.5f, 
                        playerRb.position + Vector3.up*1.5f, 
                        0.5f, 
                        dir, 
                        out RaycastHit phit, 
                        dist + 0.01f, 
                        wallLayerMask))
                {
                    playerRb.MovePosition(playerRb.position + dir * dist);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        // ──────────────── UNLOCK ────────────────
        Destroy(fj);
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        // ─────────────────────────────────────────
    }

}

