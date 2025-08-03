using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogManager : MonoBehaviour {
    [Header("Dialog Sequence Class")]
    [SerializeField] Dialog dialog;

    [Header("Dialog Boxes")]
    [SerializeField] DialogBox playerdialogBox;
    [SerializeField] DialogBox otherDialogBox;

    [Header("Dialog Speed")]
    [SerializeField] int letterPerSecond;

    private float duartion = 0;

    public Dialog Dialog => dialog;

    public bool PlayerIsShowing { get; private set; }
    public bool OtherIsShowing { get; private set; }
    public bool SequenceIsShowing {get; private set; }

    public static DialogManager Instance { get; private set; }
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }

    public IEnumerator ShowPlayerDialog(DialogInfo[] dialogInfo) {
        if (dialogInfo == null || dialogInfo.Length <= 0 || SequenceIsShowing) yield break;
        playerdialogBox.gameObject.SetActive(true);
        PlayerIsShowing = true;

        foreach (DialogInfo info in dialogInfo) {
            playerdialogBox.Portrait.sprite = info.portrait; 
            yield return TypeDialog(info.line, playerdialogBox.DialogText);
            yield return new WaitForSeconds(info.duartion);
        }

        playerdialogBox.gameObject.SetActive(false);
        PlayerIsShowing = false;
    }

    public IEnumerator ShowOtherDialog(DialogInfo[] dialogInfo) {
        if (dialogInfo == null || dialogInfo.Length <= 0 || SequenceIsShowing) yield break;
        otherDialogBox.gameObject.SetActive(true);
        OtherIsShowing= true;

        foreach (DialogInfo info in dialogInfo) {
            otherDialogBox.Portrait.sprite = info.portrait; 
            yield return TypeDialog(info.line, otherDialogBox.DialogText);
            yield return new WaitForSeconds(info.duartion);
        }

        otherDialogBox.gameObject.SetActive(false);
        OtherIsShowing = false;
    }

    public IEnumerator ShowdialogSequence(DialogInfo[] dialogInfo) {
        if (dialogInfo == null || dialogInfo.Length <= 0 || SequenceIsShowing) yield break;
        SequenceIsShowing = true;

        for (int i = 0; i < dialogInfo.Length; ++i) {
            DialogInfo info = dialogInfo[i];

            if (i >= 1 && dialogInfo[i - 1].isPlayer != info.isPlayer) {
                duartion = dialogInfo[i - 1].duartion / 2.0f;
            } else {
                duartion = info.duartion;
            }

            if (info.isPlayer) {
                if (!playerdialogBox.isActiveAndEnabled) playerdialogBox.gameObject.SetActive(true);
                playerdialogBox.Portrait.sprite = info.portrait;
                yield return TypeDialog(info.line, playerdialogBox.DialogText);
                yield return new WaitForSeconds(duartion);
                playerdialogBox.gameObject.SetActive(false);
            } else {
                if (!otherDialogBox.isActiveAndEnabled) otherDialogBox.gameObject.SetActive(true);
                otherDialogBox.Portrait.sprite = info.portrait;
                yield return TypeDialog(info.line, otherDialogBox.DialogText);
                yield return new WaitForSeconds(duartion);
                otherDialogBox.gameObject.SetActive(false);
            }
        }

        SequenceIsShowing = false;
    }

    public IEnumerator TypeDialog(string line, TextMeshProUGUI text) {
        text.text = "";
        foreach (var letter in line.ToCharArray()) {
            text.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }
    } 

    public void HaltDialog() {
        playerdialogBox.gameObject.SetActive(false);
        otherDialogBox.gameObject.SetActive(false);

        PlayerIsShowing = false;
        OtherIsShowing = false;
        SequenceIsShowing = false;

        StopCoroutine(ShowdialogSequence(null));
        StopCoroutine(ShowOtherDialog(null));
        StopCoroutine(ShowPlayerDialog(null));
        StopCoroutine(TypeDialog(null, null));
    }

    public DialogInfo[] GetRandomDialogInfos(DialogSequnce[] dialogSequnces) {
        if (dialogSequnces.Length <= 0) return new DialogInfo[0];
        int randomIndex = Random.Range(0, dialogSequnces.Length);
        return dialogSequnces[randomIndex].DialogInfos;
    } 
}
