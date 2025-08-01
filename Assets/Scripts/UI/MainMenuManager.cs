using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private float zoomDuration = 1.0f;
    [SerializeField] private float maxOrthoSize = 10.0f;
    
    void Awake() {
        GameManager.Instance.GameState = GameState.MainMenu;
    }

    public void OnStart() {
        StartGame();
    }

    public void OnQuit() {
        Application.Quit();
    }

    void StartGame() {
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
