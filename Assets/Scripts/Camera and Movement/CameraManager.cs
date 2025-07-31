using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CameraManager : MonoBehaviour {
    private Vector2 _delta;

    private bool _isMoving, _isRotating, _isBusy;
    private float _xRotation, rotationDirection;

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float roationSpeed = 0.5f;

    private void Awake() {
        _xRotation = transform.rotation.eulerAngles.x;
    }

    public void Onlook(InputAction.CallbackContext context) {
        _delta = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context) {
        _isMoving = context.started || context.performed;
    }

    public void OnRotateRight(InputAction.CallbackContext context) {
        if (_isBusy) return;

        _isRotating = context.performed;
        if (_isRotating) {
            rotationDirection = 1;
        }

        if (context.canceled) {
            _isBusy = true;
            SnapRotation();
        }

    }

    public void OnRotateLeft(InputAction.CallbackContext context) {
        if (_isBusy) return;

        _isRotating = context.performed;
        if (_isRotating) {
            rotationDirection = -1;
        }

        if (context.canceled) {
            _isBusy = true;
            SnapRotation();
        }

    }

    private void LateUpdate() {
        if (_isMoving) {
            Vector3 pos = transform.right * (_delta.x * -moveSpeed);
            pos += transform.up * (_delta.y * -moveSpeed);
            transform.position += pos * Time.deltaTime;
        }

        if (_isRotating) {
            transform.Rotate(new Vector3(_xRotation, (_delta.x + roationSpeed) * rotationDirection, 0.0f));
            transform.rotation = Quaternion.Euler(_xRotation, transform.rotation.eulerAngles.y, 0.0f);
        }
    }

    private void SnapRotation() {
        transform.DORotate(SnapToVector(), 0.5f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                _isBusy = false;
            });
    }

    private Vector3 SnapToVector() {
        float currentY = Mathf.Ceil(transform.rotation.eulerAngles.y);

        float endVal = currentY switch {
            >= 0 and <= 90 => 45.0f,
            >= 91 and <= 180 => 135.0f,
            >= 181 and <= 270 => 225.0f,
            _ => 315.0f
        };

        return new Vector3(_xRotation, endVal, 0.0f);
    }
}
