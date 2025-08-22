using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CameraManager : MonoBehaviour {
    private Vector2 _delta;

    private bool _isMoving, _isRotating, _isBusy;
    private float _xRotation, rotationDirection;

    public int YawIndex { get; private set; } = 0;

    private float _moveSpeed = 10.0f;
    public float MoveSpeed {
        set {
            _moveSpeed = value;
        }
    }

    private float _rotationSpeed = 0.5f;
    public float RotationSpeed {
        set {
            _rotationSpeed = value;
        }
    }

    private Transform _center;
    public Transform Center {
        set {
            _center = value;
        }
    }

    private float _maxOrthoSize;
    public float MaxOrthoSize {
        get {
            return _maxOrthoSize;
        } set {
            _maxOrthoSize = value;
        }
    }

    private float _zoomDuration;
    public float ZoomDuration {
        get {
            return _zoomDuration;
        } set {
            _zoomDuration = value;
        }
    }

    private void Awake() {
        _xRotation = transform.rotation.eulerAngles.x;
    }

    private void Start() {
        if (_center) {
            transform.position = new(_center.position.x, transform.position.y, _center.position.z);
        }
    }

    void OnEnable() {
        CircleWIpeTransition.OnTransitionComplete += SetCamera;
    }

    void OnDisable() {
        CircleWIpeTransition.OnTransitionComplete -= SetCamera;
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
            Vector3 pos = transform.right * (_delta.x * -_moveSpeed);
            pos += transform.up * (_delta.y * -_moveSpeed);
            transform.position += pos * Time.deltaTime;
        }

        if (_isRotating) {
            transform.Rotate(new Vector3(_xRotation, (_delta.x + _rotationSpeed) * rotationDirection, 0.0f));
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
            >= 0 and <= 90 => 25.0f,
            >= 91 and <= 180 => 115.0f,
            >= 181 and <= 270 => 205.0f,
            _ => 295.0f
        };

        switch (endVal) {
            case 25.0f:
                YawIndex = 0;
                break;
            case 115.0f:
                YawIndex = 1;
                break;
            case 205.0f:
                YawIndex = 2;
                break;
            case 295.0f:
                YawIndex = 3;
                break;
        }

        return new Vector3(_xRotation, endVal, 0.0f);
    }

    public void SetCamera(bool closed) {
        if (closed) return; 

        Camera cam = Camera.main;
        Sequence seq = DOTween.Sequence();

        seq
          .Append(cam.DOOrthoSize(_maxOrthoSize, _zoomDuration).SetEase(Ease.OutBounce))
          .Join(cam.transform.DOMove(new Vector3(0, 0, 0), _zoomDuration).SetEase(Ease.OutQuad))
          .OnComplete(() => GameManager.Instance.GameState = GameState.Playing);
    }
}
