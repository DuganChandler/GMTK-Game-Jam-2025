using UnityEngine;

[System.Serializable]
public struct MusicTrack {
    public string trackName;
    public AudioClip audioClip;
}

public class MusicLibrary : MonoBehaviour {
    public MusicTrack[] musicTracks;
    
    public AudioClip GetMusicClipByName(string name) {
        foreach (MusicTrack musicTrack in musicTracks) {
            if (musicTrack.trackName == name) {
                return musicTrack.audioClip;
            }
        }

        Debug.LogError($"No music track with name: {name}");
        return null;
    }
}
