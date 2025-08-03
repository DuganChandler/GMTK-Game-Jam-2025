using UnityEngine;
// starting level, ending level, interacting with ball boy -> change depening on the type, moving objects, idle for too long

[System.Serializable]
public struct DialogInfo {
    public string line;
    public Sprite portrait;
    public float duartion;
    public bool isPlayer;
}

[System.Serializable]
public class DialogSequnce {
    [SerializeField] private DialogInfo[] _dialogInfos; 
    public DialogInfo[] DialogInfos => _dialogInfos;
}

public class Dialog : MonoBehaviour{
    [Header("Level Sequences")]
    public DialogSequnce[] startingLevelSequences;
    public DialogSequnce[] endingLevelSequnces;

    [Header("Ball Boy Sequences")]
    public DialogSequnce[] ballBoyPurple;
    public DialogSequnce[] ballBoyGreen;
    public DialogSequnce[] ballBoyYellow;

    [Header("Idle Sequences")]
    public DialogSequnce[] idleSequences;

    [Header("Interact Sequnces")]
    public DialogSequnce[] pushPullSequences;
}