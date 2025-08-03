using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour {
    [Header("UI Elements")] 
    [SerializeField] private GameObject mainMenuElements;

    [SerializeField] private GameObject startButton;

    private CameraManager cameraManager;
    
    void Awake() {
        GameManager.Instance.GameState = GameState.MainMenu;
        cameraManager = Camera.main.GetComponent<CameraManager>();
        mainMenuElements.SetActive(true);
    }

    void Start() {
        SoundManager.Instance.PlayMusicNoFade("MainTheme"); 
        EventSystem.current.SetSelectedGameObject(startButton);
    }

    public void OnStart() {
        StartGame();
    }

    public void OnQuit() {
        Application.Quit();
    }

    void StartGame() {
        mainMenuElements.SetActive(false);

        Camera cam = Camera.main;
        cam
          .DOOrthoSize(cameraManager.MaxOrthoSize, cameraManager.ZoomDuration)
          .SetEase(Ease.OutBounce)
          .OnComplete(() => {
            GameManager.Instance.GameState = GameState.Playing;
            StartCoroutine(
                DialogManager.Instance.ShowdialogSequence(
                    DialogManager.Instance.Dialog.startingLevelSequences[0].DialogInfos
                )
            );
        });
    }
}
