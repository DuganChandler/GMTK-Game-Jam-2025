using UnityEngine;

public enum GameState {
    Dialog,
    Pause,
    MainMenu,
    Playing,
    Busy
}

public class GameManager : MonoBehaviour {
    private static GameManager _Instance;
    public static GameManager Instance { 
        get { 
            if (!_Instance) {
                _Instance = new GameObject().AddComponent<GameManager>();
                _Instance.name = _Instance.GetType().ToString();
                DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        } 
    }

    public GameState GameState { get; set; }
}