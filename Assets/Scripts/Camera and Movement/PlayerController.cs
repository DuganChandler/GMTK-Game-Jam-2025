using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour {
    [Header("Interactions")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("Camera")]
    [SerializeField] private CameraManager cameraManager;
    [Tooltip("0..3, camera yaw index; 0=+Z forward, 1=+X, 2=-Z, 3=-X")]
    public int yawIndex = 0;

    [Header("Movement")]
    public float stepDuration = 0.12f;

    private Vector3 moveInput;
    public Vector3 MoveInput => moveInput;

    private Vector3Int _facingDelta;

    private Rigidbody rb;
    private Animator anim;
    private GridBody _body;
    private bool _busy;
    private float timeIdle = 0;

    // Grabbing
    private GridBody _grabbed;
    private GrabHighlight _grabbedHighlight; 
    private bool _rotating = false; 

    private const string MOVEINPUTZ_PARAMETER = "MoveInputZ";
    private const string INTERACTING_PARAMETER = "IsInteracting";

    static readonly Vector3Int[] FORWARDS = {
        new(0,0, 1), 
        new(1,0,0), 
        new(0,0,-1), 
        new(-1,0,0)
    };

    static readonly Vector3Int[] RIGHTS = {
        new(1,0,0), 
        new(0,0,-1), 
        new(-1,0,0), 
        new(0,0,1)
    };

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

    void Awake() {
        _body = GetComponent<GridBody>();
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        cameraManager = Camera.main.GetComponent<CameraManager>();
        yawIndex = cameraManager.YawIndex;
        _facingDelta = FORWARDS[yawIndex];
    }

    public void TryStepForward() => TryMove(FORWARDS[yawIndex]);
    public void TryStepBack() => TryMove(-FORWARDS[yawIndex]);
    public void TryStepRight() => TryMove(RIGHTS[yawIndex]);
    public void TryStepLeft() => TryMove(-RIGHTS[yawIndex]);

    public void OnMove(InputAction.CallbackContext context) {
        if (context.started) {
            moveInput = context.ReadValue<Vector2>();

            Vector3Int direction = InputToDelta(moveInput);
            if (direction != Vector3Int.zero) {
                _facingDelta = direction;
            }

            if (direction == FORWARDS[yawIndex]) {
                TryStepForward();
            } else if (direction == -FORWARDS[yawIndex]) {
                TryStepBack();
            } else if (direction == RIGHTS[yawIndex]) {
                TryStepRight();
            } else if (direction == -RIGHTS[yawIndex]) {
                TryStepLeft();
            }
        }
    }

    void Update() {
        CheckIdleDialog(); 
        yawIndex = cameraManager.YawIndex;
        if (_grabbed && !StillAdjacentToGrabbed(_body.GridPos)) ReleaseGrab();

    }

    private Vector3Int GetFacingDelta() {
        if (_facingDelta == Vector3Int.zero) {
            return FORWARDS[yawIndex];
        }

        return _facingDelta;
    }

    private Vector3Int InputToDelta(Vector2 v) {
        if (Mathf.Abs(v.x) < 0.25f && Mathf.Abs(v.y) < 0.25f) {
            return Vector3Int.zero;
        }

        Vector3Int f = FORWARDS[yawIndex];
        Vector3Int r = RIGHTS[yawIndex];
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) {
            return (v.x >= 0f) ? r : -r;
        } else {
            return (v.y >= 0f) ? f : -f;
        }
    }

    private Vector3 InteractProbeWorld(Vector3Int face) {
        GridMap map = GridMap.I;
        Vector3Int frontBody = _body.GridPos + face;
        return map.GridToWorld(frontBody);
    }

    void TryMove(Vector3Int delta) {
        if (_busy) return;

        if (delta.x != 0 || delta.z != 0) {
            _facingDelta = new(delta.x, 0, delta.z);
        }

        GridMap map = GridMap.I;
        Vector3Int fromBody = _body.GridPos;
        Vector3Int toBody   = fromBody + delta;
        Vector3Int support = toBody + GridMap.Down;

        Vector3 flat = new(delta.x, 0f, delta.z);
        if (flat.sqrMagnitude > 0.001f && _grabbed == null)
            transform.rotation = Quaternion.LookRotation(flat, Vector3.up);

        if (_grabbed != null) {
            if (delta.y != 0) return;

            // PUSH
            if (_grabbed.GridPos == fromBody + delta) {
                Vector3Int objFrom = _grabbed.GridPos;
                Vector3Int objTo = objFrom + delta;
                if (_grabbed.Pushable && CanPlaceObjAtBody(objTo, out var objToSupportFlags)) {
                    StartCoroutine(AnimatePush(_grabbed, fromBody, objFrom, objTo, objToSupportFlags));
                }
                return; 
            }

            // PULL (grabbed is behind)
            if (_grabbed.GridPos == fromBody - delta) {
                Vector3Int objTo = fromBody; // we vacate this cell
                if (!map.IsStaticBlocked(toBody) && !map.HasOccupant(toBody) &&
                    map.TryGetGround(support, out var supFlags) && supFlags.HasFlag(GroundFlags.Walkable) &&
                    _grabbed.Pushable&& CanPlaceObjAtBody(objTo, out var objToSupportFlags, ignoreOccupantCell: fromBody)) {
                    StartCoroutine(AnimatePull(_grabbed, fromBody, toBody, objTo, objToSupportFlags));
                }
                return; 
            }
            return;
        }

        // No Grab
        if (delta.y != 0) {
            // Optional ladder logic:
            bool fromOK = map.TryGetGround(fromBody + GridMap.Down, out var f) && f.HasFlag(GroundFlags.Ladder);
            bool toOK = map.TryGetGround(support, out var t) && t.HasFlag(GroundFlags.Ladder);
            if (!(fromOK || toOK)) return;
            if (map.IsStaticBlocked(toBody) || map.HasOccupant(toBody)) return;

            StartCoroutine(AnimateMove(fromBody, toBody, null));
            return;
        }

        if (map.IsStaticBlocked(toBody) || map.HasOccupant(toBody)) return;
        if (!map.TryGetGround(support, out var supportFlags)) return;
        if (!supportFlags.HasFlag(GroundFlags.Walkable)) return;

        StartCoroutine(AnimateMove(fromBody, toBody, null));
    }

    bool CanPlaceObjAtBody( Vector3Int crateBody, out GroundFlags supportFlags, Vector3Int? ignoreOccupantCell = null) {
        GridMap map = GridMap.I;
        Vector3Int support = crateBody + GridMap.Down;

        supportFlags = GroundFlags.None;

        if (map.IsStaticBlocked(crateBody)) return false;

        if (map.HasOccupant(crateBody)) {
            if (!(ignoreOccupantCell.HasValue && ignoreOccupantCell.Value == crateBody))
                return false;
        }

        if (!map.TryGetGround(support, out supportFlags)) return false;

        if (supportFlags.HasFlag(GroundFlags.Walkable)) return true;
        if (supportFlags.HasFlag(GroundFlags.Pit) && supportFlags.HasFlag(GroundFlags.Fillable)) return true;

        return false;
    }


    IEnumerator AnimateMove(Vector3Int fromBody, Vector3Int toBody, System.Action afterLanding) {
        _busy = true;
        GridMap map = GridMap.I;

        map.Vacate(fromBody);
        map.Occupy(toBody, _body);

        Vector3 a = map.BodyToWorldFeet(fromBody);
        Vector3 b = map.BodyToWorldFeet(toBody);

        float t = 0f;
        while (t < stepDuration) {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / stepDuration);
            _body.transform.position = Vector3.Lerp(a, b, k);
            yield return null;
        }

        _body.SetGridPos(toBody);
        _busy = false;
        afterLanding?.Invoke();
    }

    IEnumerator AnimatePush(GridBody crate, Vector3Int playerFrom, Vector3Int crateFrom, Vector3Int crateTo, GroundFlags crateToSupportFlags) {
        _busy = true; 
        GridMap map = GridMap.I;

        map.Vacate(playerFrom);
        map.Occupy(crateFrom, _body);
        map.Vacate(crateFrom);
        map.Occupy(crateTo, crate);

        Vector3 p0 = map.BodyToWorldFeet(playerFrom);
        Vector3 p1 = map.BodyToWorldFeet(crateFrom);
        Vector3 c0 = map.BodyToWorldFeet(crateFrom);
        Vector3 c1 = map.BodyToWorldFeet(crateTo);

        float t = 0f;
        while (t < stepDuration) {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / stepDuration);
            _body.transform.position  = Vector3.Lerp(p0, p1, k);
            crate.transform.position  = Vector3.Lerp(c0, c1, k);
            yield return null;
        }

        _body.SetGridPos(crateFrom);
        crate.SetGridPos(crateTo);

        _busy = false;
    }

    IEnumerator AnimatePull(GridBody crate, Vector3Int playerFrom, Vector3Int playerTo, Vector3Int crateTo, GroundFlags crateToSupportFlags) {
        _busy = true; 
        GridMap map = GridMap.I;

        map.Vacate(playerFrom);
        map.Occupy(playerTo, _body);
        map.Vacate(crate.GridPos);
        map.Occupy(crateTo, crate);

        Vector3 p0 = map.BodyToWorldFeet(playerFrom);
        Vector3 p1 = map.BodyToWorldFeet(playerTo);
        Vector3 c0 = map.BodyToWorldFeet(crate.GridPos);
        Vector3 c1 = map.BodyToWorldFeet(crateTo);

        float t = 0f;
        while (t < stepDuration) {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / stepDuration);
            _body.transform.position  = Vector3.Lerp(p0, p1, k);
            crate.transform.position  = Vector3.Lerp(c0, c1, k);
            yield return null;
        }

        _body.SetGridPos(playerTo);
        crate.SetGridPos(crateTo);

        _busy = false;
    }

    private bool TryToggleGrab() {
        GridMap map = GridMap.I;

        Vector3Int f = GetFacingDelta();
        Vector3Int front = _body.GridPos + f;
        Vector3Int back  = _body.GridPos - f;

        GridBody target = map.GetOccupant(front);
        if (target == null || !target.Pushable) {
            target = map.GetOccupant(back);
            if (target == null || !target.Pushable) return false;
        }

        _grabbed = target;

        _grabbedHighlight = _grabbed.GetComponent<GrabHighlight>();
        if (_grabbedHighlight == null) _grabbedHighlight = _grabbed.gameObject.AddComponent<GrabHighlight>();
        _grabbedHighlight.SetHighlighted(true);

        return true;
    }

    void ReleaseGrab() {
        if (_grabbedHighlight) _grabbedHighlight.SetHighlighted(false);
        _grabbedHighlight = null;
        _grabbed = null;
    }

    bool StillAdjacentToGrabbed(Vector3Int fromBody) {
        if (_grabbed == null) return false;
        Vector3Int direction = _grabbed.GridPos - fromBody;
        return direction.y == 0 && ((Mathf.Abs(direction.x) == 1 && direction.z == 0) || (Mathf.Abs(direction.z) == 1 && direction.x == 0));
    }

    public void OnInteract(InputAction.CallbackContext ctx) {
        if (ctx.started){
            if (TryToggleGrab()) return;

            Collider[] hits = GetInteractColliders();
            if (hits.Length > 0) {
                int randomInt = UnityEngine.Random.Range(0, 100);
                // if (randomInt <= 25) CheckInteractedDialog(collider);

                if (hits[0].TryGetComponent<IInteractable<PlayerController>>(out var interactable)) {
                    IsInteracting = true;
                    anim.SetBool(INTERACTING_PARAMETER, true);
                    StartCoroutine(interactable.Interact(transform, this));
                }
            }
        } else if (ctx.canceled) {
            IsInteracting = false;
            anim.SetBool(INTERACTING_PARAMETER, false);
            if (_grabbed != null) {
                ReleaseGrab();
            }
        }
    }

    private Collider[] GetInteractColliders() {
            Vector3Int face = GetFacingDelta();
            Vector3 probe = InteractProbeWorld(face);
            float radius = GridMap.I.cellSize * 0.45f;
            return Physics.OverlapSphere(probe, radius, interactableLayer);
    }

    public IEnumerator RotateObj(float targetY) {
        _rotating = true;
        yield return _grabbed.transform
                        .DORotate(new Vector3(0, targetY, 0),
                            0.2f,
                            RotateMode.FastBeyond360)
                        .SetEase(Ease.OutQuad)
                        .WaitForCompletion();
        _rotating = false;
    }

    public void OnInteractRotateRight(InputAction.CallbackContext ctx) {
        if (ctx.started && _grabbed != null && !_busy && !_rotating) {
            float currentY = _grabbed.transform.eulerAngles.y;
            float targetY = currentY + 45f;
            StartCoroutine(RotateObj(targetY));
        }
    }

    public void OnInteractRotateLeft(InputAction.CallbackContext ctx) {
        if (ctx.started && _grabbed != null && !_busy && !_rotating) {
            float currentY = _grabbed.transform.eulerAngles.y;
            float targetY = currentY + -45f;
            StartCoroutine(RotateObj(targetY));
        }
    }

    private void CheckInteractedDialog(Collider[] colliders) {
        string tag = colliders[0].gameObject.tag;
        DialogManager tempDialogManager = DialogManager.Instance;
        DialogInfo[] dialogInfos = null;

        switch (tag) {
            case "BallBroRed":
                dialogInfos = tempDialogManager.GetRandomDialogInfos(tempDialogManager.Dialog.ballBroRed);
                break;
            case "BallBroGreen":
                dialogInfos = tempDialogManager.GetRandomDialogInfos(tempDialogManager.Dialog.ballBroGreen);
                break;
            case "BallBroYellow":
                dialogInfos = tempDialogManager.GetRandomDialogInfos(tempDialogManager.Dialog.ballBroYellow);
                break;
            case "BallBroPurple":
                dialogInfos = tempDialogManager.GetRandomDialogInfos(tempDialogManager.Dialog.ballBroPurple);
               break;
        }

        StartCoroutine(tempDialogManager.ShowdialogSequence(dialogInfos));
    }

    private void CheckIdleDialog() {
        if (rb.linearVelocity == Vector3.zero && GameManager.Instance.GameState == GameState.Playing) {
            timeIdle += Time.deltaTime;
        } else {
            timeIdle = 0;
        }

        if (timeIdle > 15 && DialogManager.Instance != null) {
            timeIdle = 0;
            DialogInfo[] dialogInfos = DialogManager.Instance.GetRandomDialogInfos(
                                            DialogManager.Instance.Dialog.idleSequences
                                        );
            if (dialogInfos != null || dialogInfos.Length > 0) {
                StartCoroutine(DialogManager.Instance.ShowdialogSequence(dialogInfos));
            }
        }
    }
}


static class FlagExt {
    public static bool HasFlag(this GroundFlags v, GroundFlags f) => (v & f) != 0;
}
