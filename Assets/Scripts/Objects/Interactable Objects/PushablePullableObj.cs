using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour, IInteractable<PlayerController> {
    [Tooltip("Units per second when pushing/pulling")]
    public float pushSpeed = 3f;
    private Rigidbody rb;

    [Tooltip("Y‐axis rotation to convert input into iso world‐space")]
    public float isoYAngle = 45f;

    private Quaternion isoRotation;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        isoRotation = Quaternion.Euler(0f, isoYAngle, 0f);
    }

    public IEnumerator Interact(Transform playerT, PlayerController playerController) {
        if (playerController == null) yield break;
        if (!playerController.TryGetComponent<Rigidbody>(out var playerRb)) yield break;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        int randomNum = Random.Range(0, 100);
        if (randomNum <= 25) {
            DialogInfo[] dialogInfos = DialogManager.Instance.GetRandomDialogInfos(
                                            DialogManager.Instance.Dialog.pushPullSequences
                                        );
            StartCoroutine(DialogManager.Instance.ShowdialogSequence(dialogInfos));
        }

        while (playerController.IsInteracting) {
            Vector3 rawInput = playerController.MoveInput;      // x/z from your PlayerController
            Vector3 planar = new(rawInput.x, 0f, rawInput.z);

            if (planar.sqrMagnitude > 0.01f) {
                // rotate that planar vector by our iso angle
                Vector3 isoDir = (isoRotation * planar).normalized;
                Vector3 delta   = pushSpeed * Time.fixedDeltaTime * isoDir;

                rb.MovePosition(rb.position     + delta);
                playerRb.MovePosition(playerRb.position + delta);
            }

            yield return new WaitForFixedUpdate();
        }

        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }
}

