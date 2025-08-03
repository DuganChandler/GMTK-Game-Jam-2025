using UnityEngine;

public class ShrinkerController : MonoBehaviour {
    public void PlayerPuzzleSolved() {
        SoundManager.Instance.PlaySound("PuzzleSolved");
    }
}
