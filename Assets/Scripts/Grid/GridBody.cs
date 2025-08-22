using UnityEngine;

[RequireComponent(typeof(Collider))]
[ExecuteAlways]
public class GridBody : MonoBehaviour {
    [Header("Editor Snapping")]
    [SerializeField] private bool snapInEditor = true;

    private Vector3 _lastEditorPos;

    [SerializeField] private bool _pushable = false;
    [SerializeField] private bool _rotatable = false;

    public bool Pushable => _pushable;
    public bool Rotatable => _rotatable;
    public Vector3Int GridPos { get; private set; }
    
    void Start() {
        if (!Application.isPlaying) return;

        GridMap map = GridMap.I;
        Vector3Int support = map.WorldTopToGrid(transform.position);
        support.y = map.GetTopWalkableYAtXZ(support.x, support.z, support.y);
        GridPos = support + GridMap.Up;
        transform.position = map.GridToWorldTop(support);
        map.Occupy(GridPos, this);
    }

    public void SetGridPos(Vector3Int bodyCell) {
        GridPos = bodyCell;
        transform.position = GridMap.I.BodyToWorldFeet(bodyCell);
    }


    void OnDisable() {
        if (!Application.isPlaying || !GridMap.I) return;
        GridMap.I.Vacate(GridPos);
    }

    void OnValidate() {
       if (!Application.isPlaying) {
        SnapToSupportTopEditor();
       } 
    }

    void Reset() {
        SnapToSupportTopEditor();
    }

    void LateUpdate() {
        if (Application.isPlaying || !snapInEditor) return; 

        if (transform.position != _lastEditorPos) {
            SnapToSupportTopEditor();
            _lastEditorPos = transform.position;
        }

    }

    [ContextMenu("Snap To Support Top (Editor)")]
    public void SnapToSupportTopEditor() {
        GridMap map = GridMap.I != null ? GridMap.I : FindFirstObjectByType<GridMap>();
        if (!map) return;

        Vector3Int support = map.WorldTopToGrid(transform.position);
        transform.position = map.GridToWorldTop(support);
    }

    void OnDrawGizmos() {
        GridMap map = GridMap.I != null ? GridMap.I : FindFirstObjectByType<GridMap>();
        if (!map) return;

        // Infer support/body in edit mode from current transform
        Vector3Int sup = map.WorldTopToGrid(transform.position);
        sup.y = map.GetTopWalkableYAtXZ(sup.x, sup.z, sup.y);
        Vector3Int body = sup + GridMap.Up;

        Gizmos.color = _pushable? Color.magenta : Color.cyan; 
        Gizmos.DrawWireCube(map.GridToWorld(body), Vector3.one * map.cellSize);

        Gizmos.color = Color.green; 
        Gizmos.DrawWireCube(map.GridToWorld(sup), Vector3.one * map.cellSize);
    }
}
