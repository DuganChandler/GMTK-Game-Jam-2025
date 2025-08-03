using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private GameObject pauseMenu;

    private float timer;

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (timer > 0) return;
        if (GameManager.Instance.GameState != GameState.Playing && GameManager.Instance.GameState != GameState.Pause) return;

        pauseMenu.SetActive(!pauseMenu.activeSelf);
        if (GameManager.Instance.GameState == GameState.Pause) GameManager.Instance.GameState = GameState.Playing;
        else GameManager.Instance.GameState = GameState.Pause;

        timer = 0.1f;
    }

    public void OnMainMenu() {
        DialogManager.Instance.HaltDialog();
        SceneManager.LoadScene("Level1Scene");
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }
}
