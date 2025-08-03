using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float accelerationTime = 0.1f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 moveInput;
    private Rigidbody rb;
    private Animator anim;

    private const string MOVEINPUTZ_PARAMETER = "MoveInputZ";
    private const string INTERACTING_PARAMETER = "IsInteracting";

    public Vector3 MoveInput => moveInput;

    private bool _isMoving = false;
    public bool IsMoving {
        get {
            return _isMoving;
        } private set {
            _isMoving = value;
        }
    }

    private bool _isInteracting= false;
    public bool IsInteracting{
        get {
            return _isInteracting;
        } private set {
            _isInteracting= value;
        }
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate() {
        if (IsInteracting || GameManager.Instance.GameState != GameState.Playing) {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Rotate();
        Run();
    }

    void Run() {
        Quaternion rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        Vector3 rotatedMoveInput = rotation * moveInput;
        Vector3 targetVelocity = new(rotatedMoveInput.x * walkSpeed, rb.linearVelocity.y, rotatedMoveInput.z * walkSpeed);
        Vector3 linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, accelerationTime);
        rb.linearVelocity = linearVelocity;
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
        if (GameManager.Instance.GameState != GameState.Playing) {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
        moveInput = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveInput.x != 0 || moveInput.y != 0) {
            IsMoving = true;
        } else {
            IsMoving = false;
        }

        if (!IsInteracting) {
            anim.SetFloat(MOVEINPUTZ_PARAMETER, Mathf.Abs(moveInput.magnitude));
        } else 
        {
            anim.SetFloat(MOVEINPUTZ_PARAMETER, moveInput.z);
        }
    }

    public void OnInteractRotateRight(InputAction.CallbackContext ctx) {
        if (ctx.started) {
            var facingDirection = transform.forward;
            var InteractPos = transform.position + facingDirection;

            var collider = Physics.OverlapSphere(InteractPos, 0.3f, interactableLayer);
            if (collider.Length > 0) {
                if (collider[0].TryGetComponent<IInteractable<float>>(out var interactable)) {
                    StartCoroutine(interactable.Interact(transform, 1.0f));
                }
            }

        }
    }

    public void OnInteractRotateLeft(InputAction.CallbackContext ctx) {
        if (ctx.started) {
            var facingDirection = transform.forward;
            var InteractPos = transform.position + facingDirection;

            var collider = Physics.OverlapSphere(InteractPos, 0.3f, interactableLayer);
            if (collider.Length > 0) {
                if (collider[0].TryGetComponent<IInteractable<float>>(out var interactable)) {
                    StartCoroutine(interactable.Interact(transform, -1.0f));
                }
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx) {
        if (ctx.started){
            rb.linearVelocity = Vector3.zero;            

            var facingDirection = transform.forward;
            var InteractPos = transform.position + facingDirection;

            var collider = Physics.OverlapSphere(InteractPos, 0.3f, interactableLayer);
            if (collider.Length > 0) {
                if (collider[0].TryGetComponent<IInteractable<PlayerController>>(out var interactable)) {
                    IsInteracting = true;
                    anim.SetBool(INTERACTING_PARAMETER, true);
                    StartCoroutine(interactable.Interact(transform, this));
                }
            }
        } else if (ctx.canceled) {
            rb.linearVelocity = Vector3.zero;            
            IsInteracting = false;
            anim.SetBool(INTERACTING_PARAMETER, false);
        }
    }
}
