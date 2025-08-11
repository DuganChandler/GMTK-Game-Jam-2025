using UnityEngine;

[System.Serializable]
public struct SFXTrack {
    public string sfxName;
    public AudioClip sfxClip;
}

public class SFXLibrary : MonoBehaviour {
    public SFXTrack[] sFXTracks;
    
    public AudioClip GetSFXClipByName(string name) {
        foreach (SFXTrack sFXTrack in sFXTracks) {
            if (sFXTrack.sfxName == name) {
                return sFXTrack.sfxClip;
            }
        }

        Debug.LogError($"No SFX track with name: {name}");
        return null;
    }
}
