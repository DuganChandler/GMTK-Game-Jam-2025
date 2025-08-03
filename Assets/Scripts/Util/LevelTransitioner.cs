using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitioner : MonoBehaviour {

    private CircleWIpeTransition _circleWIpeTransition;
    public CircleWIpeTransition CircleWIpeTransition {
        set {
            _circleWIpeTransition = value;
        }
    }

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
            _circleWIpeTransition.CloseBlackScreen();
        } 
    }

    void StartSceneLoad(bool closed) {
        if (!closed) return;
        SceneManager.LoadScene(_sceneName);
    }
}
