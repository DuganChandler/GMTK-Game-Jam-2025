using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class RotateableObject : MonoBehaviour, IInteractable<float> {

    private Rigidbody rb;
    private bool busy = false;

    public bool Busy => busy;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public IEnumerator Interact(Transform initiator, float directionSign) {
        float currentY = transform.eulerAngles.y;
        float targetY  = currentY + directionSign * 45f;
        if (!busy) {
            busy = true;
            yield return rb
                .DORotate(new Vector3(0, targetY, 0), 
                        0.2f, 
                        RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion(); 
        }

        busy = false;
    }
        
}
