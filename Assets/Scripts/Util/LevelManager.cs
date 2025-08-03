using System;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [Header("Level Specific Settings")]
    [SerializeField] int levelNumber;

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

    public static event Action OnLevelSolved;

    void Awake() {
        if (cameraManager) SetCameraSettings();
        if (levelTransitioner) SetTransitionSettings();
        if (circleWIpeTransition) SetFadeSettings();

        GameManager.Instance.LevelNumber = levelNumber;
    }

    void Start() {
        if (startWithFade) circleWIpeTransition.OpenBlackScreen();
    }

    private void SetCameraSettings() {
        cameraManager.MaxOrthoSize = maxOrthoSize;
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

    public void TriggerLevelSolved() {
        DialogManager.Instance.HaltDialog();
        DialogInfo[] dialogInfos = DialogManager.Instance.Dialog.endingLevelSequnces[levelNumber].DialogInfos;
        StartCoroutine(DialogManager.Instance.ShowdialogSequence(dialogInfos));

        OnLevelSolved?.Invoke();
    }
}
