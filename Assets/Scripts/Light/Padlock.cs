using UnityEngine;

public class Padlock : LightReciever
{
    [Header("Materials")]
    [SerializeField] private Material redMat;
    [SerializeField] private Material orangeMat;
    [SerializeField] private Material yellowMat;
    [SerializeField] private Material greenMat;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material purpleMat;
    [SerializeField] private Material whiteMat;

    private Renderer rend;

    private void Awake()
    {
        TryGetComponent(out rend);

        if (rend == null) return;

        rend.material = lightLevelRequiredToActivate switch
        {
            1 => whiteMat,
            2 => yellowMat,
            3 => orangeMat,
            4 => redMat,
            5 => purpleMat,
            _ => throw new System.NotImplementedException(),
        };
    }
}
