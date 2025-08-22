using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum GroundFlags { None=0, Walkable=1<<0, Pit=1<<1, Fillable=1<<2, Ladder=1<<3, Elevator=1<<4 }

[DefaultExecutionOrder(-10000)]
public class GridMap : MonoBehaviour {
    public static GridMap I { get; private set; }

    [Header("Grid Settings")]
    public Vector3 origin = Vector3.zero;
    public float cellSize = 1.0f;

    public static readonly Vector3Int Up = new(0, 1, 0);
    public static readonly Vector3Int Down = new(0,-1, 0);

    private readonly Dictionary<Vector3Int, GroundFlags> _ground = new();
    private readonly HashSet<Vector3Int> _staticBlockers = new();
    private readonly Dictionary<Vector3Int, GridBody> _occupants = new();
    private readonly Dictionary<Vector3Int, Vector3Int> _elevatorLinks = new();

    void Awake() {
        if (I != null && I != this) { 
            Destroy(gameObject); 
            return; 
        } 
        I = this;
    }

    // Register Tiles API 
    public void RegisterGround(Vector3Int cell, GroundFlags flags) => _ground[cell] = flags;
    public void UnregisterGround(Vector3Int cell) => _ground.Remove(cell);

    public void RegisterStaticBlocker(Vector3Int cell) => _staticBlockers.Add(cell);
    public void UnregisterStaticBlocker(Vector3Int cell) => _staticBlockers.Remove(cell);

    // Grid Coord Queries 
    public bool TryGetGround(Vector3Int cell, out GroundFlags flags) => _ground.TryGetValue(cell, out flags);
    public bool IsStaticBlocked(Vector3Int cell) => _staticBlockers.Contains(cell);
    public bool HasOccupant(Vector3Int cell) => _occupants.ContainsKey(cell);
    public GridBody GetOccupant(Vector3Int cell) => _occupants.TryGetValue(cell, out var b) ? b : null;

    public void Occupy(Vector3Int cell, GridBody b) => _occupants[cell] = b;
    public void Vacate(Vector3Int cell) { if (_occupants.ContainsKey(cell)) _occupants.Remove(cell); }

    // World <-> Grid
    public Vector3Int WorldToGrid(Vector3 worldPos) {
        Vector3 position = (worldPos - origin) / cellSize;
        return new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
    }

    public Vector3 GridToWorld(Vector3Int gridPos) {
        return origin + new Vector3((gridPos.x + 0.5f) * cellSize, (gridPos.y + 0.5f) * cellSize, (gridPos.z + 0.5f) * cellSize);
    }

    public Vector3 GridToWorldTop(Vector3Int g) {
        float s = cellSize;
        return origin + new Vector3(
            (g.x + 0.5f) * s,
            (g.y + 1f)   * s,   // <- top of the cell, not center
            (g.z + 0.5f) * s
        );
    }

    public Vector3Int WorldTopToGrid(Vector3 w) {
        float s = cellSize; var p = (w - origin) / s;
        return new Vector3Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y - 0.5f), Mathf.FloorToInt(p.z));
    }

    public int GetTopWalkableYAtXZ(int x, int z, int fallbackY = 0) {
        int best = int.MinValue;
        foreach (var kv in _ground) {
            var cell = kv.Key;
            if (cell.x == x && cell.z == z && (kv.Value & GroundFlags.Walkable) != 0)
                best = Mathf.Max(best, cell.y);
        }
        return best == int.MinValue ? fallbackY : best;
    }

    public Vector3 BodyToWorldFeet(Vector3Int bodyCell) => GridToWorldTop(bodyCell + Down);
}
