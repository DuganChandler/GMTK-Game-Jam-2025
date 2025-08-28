using System;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [Header("Level Specific Settings")]
    [SerializeField] int levelNumber;
    [SerializeField] Room startingRoom;

    [Header("Fade Settings")]
    [SerializeField] private CircleWIpeTransition circleWIpeTransition;
    [SerializeField] private bool startWithFade;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Transform player; 

    [Header("Camera Settings")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private float maxOrthoSize;
    [SerializeField] private float zoomDuration;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private Transform center;

    [Header("Transition Settings")]
    [SerializeField] private LevelTransitioner levelTransitioner;
    [SerializeField] private string nextLevelName;

    public static event Action OnRoomSolved;
    public static event Action OnLevelSolved;


    void Awake() {
        if (cameraManager) SetCameraSettings();
        if (levelTransitioner) SetTransitionSettings();
        if (circleWIpeTransition) SetFadeSettings();

        GameManager.Instance.LevelNumber = levelNumber;
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerEntersRoom += GoToRoom;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerEntersRoom -= GoToRoom;
    }

    void Start() {
        if (startWithFade) circleWIpeTransition.OpenBlackScreen();
        
        if (DialogManager.Instance == null) return;

        DialogManager.Instance.HaltDialog();

        DialogSequnce[] dialogSequnces = DialogManager.Instance.Dialog.startingLevelSequences;
        if (dialogSequnces.Length - 1 >= levelNumber && levelNumber != 1) {
            int randomIndex = UnityEngine.Random.Range(1, 6);
            DialogInfo[] dialogInfos = dialogSequnces[randomIndex].DialogInfos;
            StartCoroutine(DialogManager.Instance.ShowdialogSequence(dialogInfos));
        }
    }

    private void SetCameraSettings() {
        cameraManager.MaxOrthoSize = startingRoom.camOrthoSize;
        cameraManager.ZoomDuration = zoomDuration;
        cameraManager.MoveSpeed = moveSpeed;
        cameraManager.RotationSpeed = rotationSpeed;
        cameraManager.Center = center;
    }

    public void SetTransitionSettings() {
        levelTransitioner.CircleWIpeTransition = circleWIpeTransition;
        levelTransitioner.SceneName = nextLevelName;
    }

    public void SetFadeSettings() {
        circleWIpeTransition.Player = player;
        circleWIpeTransition.FadeDuration = fadeDuration;
    }

    public void TrigggerRoomSolved() {
        SoundManager.Instance.PlaySound("OpenDoor");

        DialogManager.Instance.HaltDialog();

        DialogSequnce[] dialogSequnces = DialogManager.Instance.Dialog.endingLevelSequnces;
        if (dialogSequnces.Length - 1 >= levelNumber) {
            DialogInfo[] dialogInfos = dialogSequnces[levelNumber].DialogInfos;
            StartCoroutine(DialogManager.Instance.ShowdialogSequence(dialogInfos));
        }

        OnRoomSolved?.Invoke();
    }

    private void GoToRoom(Room room)
    {
        cameraManager.MaxOrthoSize = room.camOrthoSize;
        cameraManager.ShiftCamera(room.center.position);
    }
}
