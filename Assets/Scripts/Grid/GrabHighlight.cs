using UnityEngine;

[DisallowMultipleComponent]
public class GrabHighlight : MonoBehaviour {
    [ColorUsage(true, true)] public Color emissionColor = new(1f, 0.7f, 0.2f, 1f); 
    [Range(0f, 2f)] public float intensity = 1.2f;

    Renderer[] _renderers;
    MaterialPropertyBlock _mpb;
    static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int _BaseColor = Shader.PropertyToID("_BaseColor"); 
    static readonly int _Color = Shader.PropertyToID("_Color"); 

    void Awake() {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();
    }

    public void SetHighlighted(bool on) {
        foreach (var r in _renderers) {
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            if (on) {
                _mpb.SetColor(_EmissionColor, emissionColor * Mathf.LinearToGammaSpace(intensity));
                if (r.sharedMaterial && r.sharedMaterial.HasProperty(_BaseColor)) {
                    var baseCol = r.sharedMaterial.GetColor(_BaseColor);
                    _mpb.SetColor(_BaseColor, baseCol * 1.05f);
                } else if (r.sharedMaterial && r.sharedMaterial.HasProperty(_Color)) {
                    var col = r.sharedMaterial.GetColor(_Color);
                    _mpb.SetColor(_Color, col * 1.05f);
                }
            } else {
                _mpb.SetColor(_EmissionColor, Color.black);
            }
            r.SetPropertyBlock(_mpb);
        }

        foreach (var r in _renderers) {
            if (!r || !r.sharedMaterial) continue;
            r.sharedMaterial.EnableKeyword("_EMISSION");
        }
    }
}
