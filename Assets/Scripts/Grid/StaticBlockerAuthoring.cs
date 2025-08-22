using UnityEngine;

[ExecuteAlways]
public class StaticBlockerAuthoring : MonoBehaviour {
    public Vector3Int size = new(1,1,1);
    Vector3Int[] _cells;

    void OnValidate() { RebuildAndSnap(); }
    void Reset()      { RebuildAndSnap(); }

    void RebuildAndSnap() {
        var map = GridMap.I != null ? GridMap.I : FindFirstObjectByType<GridMap>();
        if (!map) return;
        Vector3Int baseCell = map.WorldToGrid(transform.position);
        _cells = BuildCells(baseCell, size);
        transform.position = map.GridToWorld(baseCell);
    }

    Vector3Int[] BuildCells(Vector3Int baseCell, Vector3Int sz) {
        int count = sz.x*sz.y*sz.z; var arr = new Vector3Int[count]; int i=0;
        for (int y=0;y<sz.y;y++) for (int z=0;z<sz.z;z++) for (int x=0;x<sz.x;x++)
            arr[i++] = baseCell + new Vector3Int(x,y,z);
        return arr;
    }

    void Start() {
        if (!Application.isPlaying) return;
        var map = GridMap.I;
        if (!map) { Debug.LogError("No GridMap in scene."); return; }
        if (_cells == null || _cells.Length == 0) RebuildAndSnap();
        foreach (var c in _cells) map.RegisterStaticBlocker(c);
    }

    void OnDestroy() {
        if (!Application.isPlaying) return;
        var map = GridMap.I;
        if (!map || _cells == null) return;
        foreach (var c in _cells) map.UnregisterStaticBlocker(c);
    }

    void OnDrawGizmos() {
        var map = GridMap.I != null ? GridMap.I : FindFirstObjectByType<GridMap>();
        if (!map || _cells == null) return;
        Gizmos.color = Color.red;
        foreach (var c in _cells)
            Gizmos.DrawWireCube(map.GridToWorld(c), Vector3.one * map.cellSize);
    }
}
