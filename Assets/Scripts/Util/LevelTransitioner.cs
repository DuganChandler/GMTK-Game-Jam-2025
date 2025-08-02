using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitioner : MonoBehaviour {
    [SerializeField] private string sceneName;
    [SerializeField] private CircleWIpeTransition circleWIpeTransition;

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
        SceneManager.LoadScene(sceneName);
    }
}
