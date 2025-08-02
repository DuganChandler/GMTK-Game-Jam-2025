using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitioner : MonoBehaviour {
    [SerializeField] private CircleWIpeTransition circleWIpeTransition;

    private string _sceneName;
    public string SceneName {
        set {
            _sceneName = value;
        }
    }

    void OnEnable() {
        CircleWIpeTransition.OnTransitionComplete += StartSceneLoad; 
    }

    void OnDisable() {
        CircleWIpeTransition.OnTransitionComplete -= StartSceneLoad; 
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            circleWIpeTransition.CloseBlackScreen();
        } 
    }

    void StartSceneLoad(bool closed) {
        if (!closed) return;
        SceneManager.LoadScene(_sceneName);
    }
}
