using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour {
    [Header("Zoom Variables")]
    [SerializeField] private float zoomDuration = 1.0f;
    [SerializeField] private float maxOrthoSize = 10.0f;

    [Header("UI Elements")] 
    [SerializeField] private GameObject mainMenuElements;
    
    void Awake() {
        GameManager.Instance.GameState = GameState.MainMenu;
        mainMenuElements.SetActive(true);
    }

    public void OnStart() {
        StartGame();
    }

    public void OnQuit() {
        Application.Quit();
    }

    void StartGame() {
        // Do a fade later?
        mainMenuElements.SetActive(false);

        Camera cam = Camera.main;
        cam
          .DOOrthoSize(maxOrthoSize, zoomDuration)
          .SetEase(Ease.OutBounce)
          .OnComplete(() => GameManager.Instance.GameState = GameState.Playing);

        // Sequence seq = DOTween.Sequence();

        // seq
        //   .Append(cam.DOOrthoSize(maxOrthoSize, zoomDuration).SetEase(Ease.OutBounce))
        //   .Join(cam.transform.DOMove(new Vector3(0, 0, 0), zoomDuration).SetEase(Ease.OutQuad))
        //   .OnComplete(() => GameManager.Instance.GameState = GameState.Playing);

    }
}
