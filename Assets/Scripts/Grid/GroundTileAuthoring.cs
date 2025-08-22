using UnityEngine;

[ExecuteAlways]
public class GroundTileAuthoring : MonoBehaviour {
    public GroundFlags flags = GroundFlags.Walkable;
    public Vector3Int size = new(1,1,1);

    Vector3Int[] _cells;

    void OnValidate() { RebuildAndSnap(); }
    void Reset() { RebuildAndSnap(); }

    void RebuildAndSnap() {
        GridMap map = GridMap.I != null ? GridMap.I : FindFirstObjectByType<GridMap>();
        if (!map) return;

        Vector3Int baseCell = map.WorldToGrid(transform.position);

        _cells = BuildCells(baseCell, size);

        transform.position = map.GridToWorld(baseCell);
    }

    Vector3Int[] BuildCells(Vector3Int baseCell, Vector3Int size) {
        int count = size.x * size.y * size.z;

        var arr = new Vector3Int[count]; 

        int i=0;
        for (int y = 0; y < size.y; y++) {
            for (int z = 0; z < size.z; z++) {
                for (int x = 0; x < size.x; x++) {
                    arr[i++] = baseCell + new Vector3Int(x, y, z);
                }
            }
        }

        return arr;
    }

    void Start() {
        if (!Application.isPlaying) return;

        var map = GridMap.I;
        if (!map) { 
            Debug.LogError("No GridMap in scene."); 
            return; 
        }

        if (_cells == null || _cells.Length == 0) RebuildAndSnap();

        foreach (var c in _cells) {
            map.RegisterGround(c, flags);
        }
    }

    void OnDestroy() {
        if (!Application.isPlaying) return;

        GridMap map = GridMap.I;
        if (!map || _cells == null) return;

        foreach (var c in _cells){
            map.UnregisterGround(c);
        } 
    }

    void OnDrawGizmos() {
        GridMap map = GridMap.I != null ? GridMap.I : FindFirstObjectByType<GridMap>();
        if (!map || _cells == null) return;
        Gizmos.color = Color.yellow;
        foreach (var c in _cells)
            Gizmos.DrawWireCube(map.GridToWorld(c), Vector3.one * map.cellSize);
    }
}
