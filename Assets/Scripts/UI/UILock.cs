using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILock : MonoBehaviour {
    [SerializeField] private GameObject glass;

    private Button button;
    void Start() {
        button = GetComponent<Button>(); 
    }

    void Update() {
        if (button.gameObject == EventSystem.current.currentSelectedGameObject) {
            glass.SetActive(true);
        } else {
            glass.SetActive(false);
        }
    }
}
