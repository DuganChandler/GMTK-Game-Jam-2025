using UnityEngine;

public class LevelManager : MonoBehaviour {
    [SerializeField] private bool startWithFade;
    [SerializeField] private CircleWIpeTransition circleWIpeTransition;

    void Start() {
        if (startWithFade) {
            circleWIpeTransition.OpenBlackScreen();
        } 
    }
}
