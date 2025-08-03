using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour {
    [SerializeField] private Image _portrait; 
    [SerializeField] TextMeshProUGUI _dialogText;

    public Image Portrait => _portrait;
    public TextMeshProUGUI DialogText=> _dialogText;
}
