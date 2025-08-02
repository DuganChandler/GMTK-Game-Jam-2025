using UnityEngine;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour {
    [Header("UI Elements")] 
    [SerializeField] private GameObject mainMenuElements;

    private CameraManager cameraManager;
    
    void Awake() {
        GameManager.Instance.GameState = GameState.MainMenu;
        cameraManager = Camera.main.GetComponent<CameraManager>();
        mainMenuElements.SetActive(true);
    }

    void Start() {
        SoundManager.Instance.PlayMusicNoFade("MainTheme"); 
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
          .OnComplete(() => GameManager.Instance.GameState = GameState.Playing);
    }
}
