using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private float walkSpeed = 10f;

    private Vector3 moveInput;
    private Rigidbody rb;

    private bool _isMoving = false;
    public bool IsMoving {
        get {
            return _isMoving;
        } private set {
            _isMoving = value;
        }
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        Rotate();
        Run();
    }

    void Run() {
        Quaternion rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        Vector3 rotatedMoveInput = rotation * moveInput;
        Vector3 velocity = new Vector3(rotatedMoveInput.x * walkSpeed, rb.linearVelocity.y, rotatedMoveInput.z * walkSpeed);
        rb.linearVelocity = velocity;
    }

    void Rotate() {
         if (moveInput.sqrMagnitude > 0.001f) {
            Quaternion cameraYaw = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            Vector3 rotatedInput = cameraYaw * moveInput;
            Quaternion targetRotation = Quaternion.LookRotation(rotatedInput, Vector3.up);
            transform.rotation = targetRotation; //Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        if (GameManager.Instance.GameState != GameState.Playing) return;
        moveInput = context.ReadValue<Vector2>();
        moveInput = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveInput.x != 0 || moveInput.y != 0) {
            IsMoving = true;
        } else {
            IsMoving = false;
        }
    }
}
