using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 3D 迷宫生成器 —— DFS 算法。
/// 墙体/地板/天花板支持多种预制体随机选取，
/// 装饰物分为房间类和物体类两个可增删列表。
/// 挂到场景空 GameObject 上，外部调用 GenerateMaze(5, 5) 即可。
/// </summary>
public class MazeGenerator3D : MonoBehaviour
{
    [Header("迷宫规格")]
    public int width = 12;
    public int height = 12;
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.3f;
    [Tooltip("墙体 / 装饰物预制体的基础边长。设为你墙体预制体的实际边长。")]
    public float prefabUnitSize = 1f;
    [Tooltip("地板面片的原始边长（单位：世界单位）。设为 0 则按 3D 模型处理。Unity Plane=10，Quad=1。")]
    public float floorPlaneUnitSize = 10f;
    [Tooltip("天花板面片的原始边长。设为 0 则按 3D 模型处理。")]
    public float ceilingPlaneUnitSize = 10f;
    [Tooltip("天花板面片是否默认朝下。勾选则不额外旋转。")]
    public bool ceilingFacesDown = true;

    [Header("起点 / 终点")]
    [Tooltip("迷宫起点格子坐标")]
    public Vector2Int startCell = Vector2Int.zero;
    [Tooltip("迷宫终点格子坐标")]
    public Vector2Int endCell = new Vector2Int(11, 11);

    [Header("墙体预制体（随机选取）")]
    [Tooltip("可拖入多种墙体，生成时随机选择")]
    public GameObject[] wallPrefabs;
    [Header("边界墙体预制体（随机选取）")]
    [Tooltip("迷宫外边缘围墙的预制体，单独设置材质。空则沿用 Wall Prefabs")]
    public GameObject[] borderWallPrefabs;
    [Header("地板预制体（随机选取）")]
    [Tooltip("可拖入多种地板，生成时随机选择")]
    public GameObject[] floorPrefabs;
    [Header("天花板预制体（随机选取）")]
    [Tooltip("可拖入多种天花板，生成时随机选择")]
    public GameObject[] ceilingPrefabs;

    [Header("入口 / 出口")]
    public bool openEntry = true;
    public bool openExit = true;

    [Header("起点 / 终点房间")]
    [Tooltip("起点房间预制体（放置在迷宫外，通过入口连通）")]
    public GameObject startRoomPrefab;
    [Tooltip("终点房间预制体（放置在迷宫外，通过出口连通）")]
    public GameObject endRoomPrefab;
    [Tooltip("房间与迷宫边界的距离（单位：格子数），用于微调房间位置")]
    public float roomPlacementOffset = 0f;

    [Header("起点 / 终点可视化标记")]
    [Tooltip("起点处放置的标记预制体（如旗帜、光柱等）")]
    public GameObject startMarkerPrefab;
    [Tooltip("终点处放置的标记预制体")]
    public GameObject endMarkerPrefab;
    public float markerYOffset = 2f;

    [Header("安全锁控门")]
    [Tooltip("放置在通道上的门预制体")]
    public GameObject doorPrefab;
    [Range(0f, 1f)] public float doorChance = 0.3f;

    [Header("烟雾隐蔽通道")]
    [Tooltip("随机生成的烟雾粒子预制体")]
    public GameObject smokePrefab;
    [Range(0f, 1f)] public float smokeChance = 0.15f;

    [Header("房间类装饰（可自由增删）")]
    [Tooltip("如：无意义房间、储物间等，Inspector 中可增减条目")]
    public List<DecorationEntry> roomDecorations = new();

    [Header("物体类装饰（可自由增删）")]
    [Tooltip("如：储水罐、箱子等，Inspector 中可增减条目")]
    public List<DecorationEntry> objectDecorations = new();

    [Header("天花板灯光（可自由增删）")]
    [Tooltip("放置在天花板上的灯光预制体，自动翻转向下")]
    public List<DecorationEntry> ceilingLightDecorations = new();

    [Header("墙壁灯光（可自由增删）")]
    [Tooltip("放置在墙壁上的灯光预制体，随机选墙并自动朝内")]
    public List<DecorationEntry> wallLightDecorations = new();

    [Header("调试寻路")]
    [Tooltip("启用后自动生成小球从起点走到终点")]
    public bool enableDebugNavigation = false;
    [Tooltip("调试小球的预制体（Sphere）")]
    public GameObject debugBallPrefab;
    public float debugBallSpeed = 5f;
    public float debugBallYOffset = 1.5f;

    // ---- 内部 ----
    private MazeCell3D[,] cells;
    private readonly Vector2Int[] dirs = { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) };
    private readonly HashSet<(int x, int y, int dir)> placedWalls = new();
    private readonly List<(int x, int y, int dir)> passages = new();
    private Dictionary<GameObject, Vector3> _meshSizeCache = new();

    private void Start() => Generate();

    // ==================== 公开 API ====================

    /// <summary>
    /// 外部调用: 生成指定大小的迷宫。起点 (0,0)，终点 (w-1, h-1)。
    /// 例: mazeGenerator.GenerateMaze(5, 5);
    /// </summary>
    public void GenerateMaze(int w, int h)
    {
        width = w;
        height = h;
        startCell = Vector2Int.zero;
        endCell = new Vector2Int(w - 1, h - 1);
        Generate();
    }

    /// <summary>
    /// 外部调用: 生成指定起点终点的迷宫。
    /// 例: mazeGenerator.GenerateMaze(8, 8, new Vector2Int(0, 0), new Vector2Int(7, 3));
    /// </summary>
    public void GenerateMaze(int w, int h, Vector2Int start, Vector2Int end)
    {
        width = w;
        height = h;
        startCell = start;
        endCell = end;
        Generate();
    }

    [ContextMenu("生成迷宫")]
    public void Generate()
    {
        startCell.x = Mathf.Clamp(startCell.x, 0, width - 1);
        startCell.y = Mathf.Clamp(startCell.y, 0, height - 1);
        endCell.x = Mathf.Clamp(endCell.x, 0, width - 1);
        endCell.y = Mathf.Clamp(endCell.y, 0, height - 1);

        ClearChildren();
        InitCells();
        passages.Clear();

        DFS(startCell.x, startCell.y);

        if (openEntry) cells[startCell.x, startCell.y].SetOuterWall(startCell.x, startCell.y, width, height);
        if (openExit)  cells[endCell.x, endCell.y].SetOuterWall(endCell.x, endCell.y, width, height);

        BuildGeometry();
        PlaceDoors();
        PlaceSmoke();
        PlaceStartRoom();
        PlaceEndRoom();
        PlaceMarkers();
        PlaceDecorations(roomDecorations, "房间类");
        PlaceDecorations(objectDecorations, "物体类");
        PlaceCeilingLights();
        PlaceWallLights();

        if (enableDebugNavigation && debugBallPrefab != null)
            StartCoroutine(DebugNavigate());

        Debug.Log($"迷宫生成完毕: {width}x{height}, 起点{startCell}, 终点{endCell}");
    }

    // ==================== DFS ====================

    private void InitCells()
    {
        cells = new MazeCell3D[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y] = new MazeCell3D();
    }

    private void DFS(int x, int y)
    {
        cells[x, y].visited = true;
        var shuffled = Shuffle(dirs);

        foreach (var d in shuffled)
        {
            int nx = x + d.x, ny = y + d.y;
            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
            if (cells[nx, ny].visited) continue;

            RemoveWallBetween(x, y, d);
            passages.Add((x, y, DirToIndex(d)));
            DFS(nx, ny);
        }
    }

    private void RemoveWallBetween(int x, int y, Vector2Int d)
    {
        if (d.y ==  1) { cells[x, y].top    = false; cells[x, y + 1].bottom = false; }
        if (d.x ==  1) { cells[x, y].right  = false; cells[x + 1, y].left   = false; }
        if (d.y == -1) { cells[x, y].bottom = false; cells[x, y - 1].top    = false; }
        if (d.x == -1) { cells[x, y].left   = false; cells[x - 1, y].right  = false; }
    }

    // ==================== 构建几何体 ====================

    private void BuildGeometry()
    {
        float hw = wallHeight * 0.5f;
        placedWalls.Clear();

        // 外围围墙（正方体）
        BuildBoundaryWalls(hw);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!cells[x, y].visited) continue;

                var c = cells[x, y];
                float cx = (x + 0.5f) * cellSize;
                float cz = (y + 0.5f) * cellSize;

                CreateFloor(cx, cz);
                if (HasAny(ceilingPrefabs)) CreateCeiling(cx, hw * 2f, cz);

                // 内部右墙（跳过最右列，围墙已处理）
                if (c.right && x < width - 1)
                    PlaceInteriorWall(x, y, 1,
                        new Vector3((x + 1) * cellSize, hw, cz),
                        new Vector3(wallThickness, wallHeight, cellSize));

                // 内部顶墙（跳过最顶行，围墙已处理）
                if (c.top && y < height - 1)
                    PlaceInteriorWall(x, y, 0,
                        new Vector3(cx, hw, (y + 1) * cellSize),
                        new Vector3(cellSize, wallHeight, wallThickness));
            }
        }
    }

    // ---- 围墙（正方体）----

    private void BuildBoundaryWalls(float hw)
    {
        if (!HasAny(borderWallPrefabs) && !HasAny(wallPrefabs)) return;

        // 左墙  底墙  右墙  顶墙
        // dir:   3     2     1     0   (对应格子的 wall flag 方向检查)
        BuildBoundarySide(0,              height,  hw, true,  3);
        BuildBoundarySide(width * cellSize, height,  hw, true,  1);
        BuildBoundarySide(0,              width,   hw, false, 2);
        BuildBoundarySide(height * cellSize, width,  hw, false, 0);
    }

    private void BuildBoundarySide(float edgeCoord, int cellCount, float hw, bool alongZ, int dir)
    {
        var pool = HasAny(borderWallPrefabs) ? borderWallPrefabs : wallPrefabs;
        var prefab = PickRandom(pool);
        if (prefab == null) return;

        Vector3 meshSize = GetMeshSize(prefab);

        for (int i = 0; i < cellCount; i++)
        {
            int gx = alongZ ? 0 : i;
            int gy = alongZ ? i : 0;

            //if ((gx == startCell.x && gy == startCell.y && openEntry) ||
            //    (gx == endCell.x && gy == endCell.y && openExit))
            //    continue;

            //if (dir == 0 && !cells[gx, gy].top) continue;
            //if (dir == 1 && !cells[gx, gy].right) continue;
            //if (dir == 2 && !cells[gx, gy].bottom) continue;
            //if (dir == 3 && !cells[gx, gy].left) continue;

            //var key = (gx, gy, dir);
            //if (placedWalls.Contains(key)) continue;
            //placedWalls.Add(key);

            float cx = (gx + 0.5f) * cellSize;
            float cz = (gy + 0.5f) * cellSize;

            Vector3 pos;
            Vector3 targetSize;
            if (alongZ)
            {
                pos = new Vector3(edgeCoord, hw, cz);
                targetSize = new Vector3(wallThickness, wallHeight, cellSize);
            }
            else
            {
                pos = new Vector3(cx, hw, edgeCoord);
                targetSize = new Vector3(cellSize, wallHeight, wallThickness);
            }

            var wall = Instantiate(prefab, pos, Quaternion.identity, transform);
            wall.transform.localScale = SafeDivide(targetSize, meshSize);
        }
    }

    // ---- 内部墙（长方体）----

    private void PlaceInteriorWall(int x, int y, int dir, Vector3 pos, Vector3 targetSize)
    {
        var key = (x, y, dir);
        if (placedWalls.Contains(key)) return;
        placedWalls.Add(key);

        var prefab = PickRandom(wallPrefabs);
        if (prefab == null) return;

        Vector3 meshSize = GetMeshSize(prefab);
        var wall = Instantiate(prefab, pos, Quaternion.identity, transform);
        wall.transform.localScale = SafeDivide(targetSize, meshSize);
    }

    // ---- 地板 / 天花板 ----

    private void CreateFloor(float cx, float cz)
    {
        var prefab = PickRandom(floorPrefabs);
        if (prefab == null) return;

        var f = Instantiate(prefab, new Vector3(cx, 0f, cz), Quaternion.identity, transform);
        if (floorPlaneUnitSize > 0f)
            f.transform.localScale = new Vector3(cellSize / floorPlaneUnitSize, 1f, cellSize / floorPlaneUnitSize);
        else
        {
            Vector3 ms = GetMeshSize(prefab);
            f.transform.localScale = new Vector3(cellSize / ms.x, 0.1f / ms.y, cellSize / ms.z);
        }
    }

    private void CreateCeiling(float cx, float topY, float cz)
    {
        var prefab = PickRandom(ceilingPrefabs);
        if (prefab == null) return;

        Quaternion rot = Quaternion.identity;
        if (ceilingPlaneUnitSize > 0f && !ceilingFacesDown)
            rot = Quaternion.Euler(180, 0, 0);

        var obj = Instantiate(prefab, new Vector3(cx, topY, cz), rot, transform);
        if (ceilingPlaneUnitSize > 0f)
            obj.transform.localScale = new Vector3(cellSize / ceilingPlaneUnitSize, 1f, cellSize / ceilingPlaneUnitSize);
        else
        {
            Vector3 ms = GetMeshSize(prefab);
            obj.transform.localScale = new Vector3(cellSize / ms.x, 0.1f / ms.y, cellSize / ms.z);
        }
    }

    // ---- 网格尺寸 ----

    private Vector3 GetMeshSize(GameObject prefab)
    {
        if (_meshSizeCache.TryGetValue(prefab, out var s)) return s;
        var mfs = prefab.GetComponentsInChildren<MeshFilter>();
        if (mfs.Length == 0) { s = Vector3.one * prefabUnitSize; }
        else
        {
            Bounds b = new Bounds();
            bool first = true;
            foreach (var mf in mfs)
            {
                var mb = mf.sharedMesh.bounds;
                if (first) { b = mb; first = false; }
                else b.Encapsulate(mb);
            }
            s = b.size;
        }
        _meshSizeCache[prefab] = s;
        return s;
    }

    private static Vector3 SafeDivide(Vector3 a, Vector3 b)
    {
        return new Vector3(
            b.x > 0.0001f ? a.x / b.x : 1f,
            b.y > 0.0001f ? a.y / b.y : 1f,
            b.z > 0.0001f ? a.z / b.z : 1f
        );
    }

    // ==================== 门 & 烟雾 ====================

    private void PlaceDoors()
    {
        if (doorPrefab == null) return;

        foreach (var (x, y, dir) in passages)
        {
            if (Random.value > doorChance) continue;
            Vector3 pos = GetPassageWorldPos(x, y, dir);
            Instantiate(doorPrefab, pos, Quaternion.identity, transform);
        }
    }

    private void PlaceSmoke()
    {
        if (smokePrefab == null) return;

        foreach (var (x, y, dir) in passages)
        {
            if (Random.value > smokeChance) continue;
            Vector3 pos = GetPassageWorldPos(x, y, dir);
            Instantiate(smokePrefab, pos, Quaternion.identity, transform);
        }
    }

    private Vector3 GetPassageWorldPos(int x, int y, int dir)
    {
        float cx = (x + 0.5f) * cellSize;
        float cz = (y + 0.5f) * cellSize;
        float halfCell = cellSize * 0.5f;

        return dir switch
        {
            0 => new Vector3(cx, wallHeight * 0.5f, cz + halfCell),
            1 => new Vector3(cx + halfCell, wallHeight * 0.5f, cz),
            2 => new Vector3(cx, wallHeight * 0.5f, cz - halfCell),
            3 => new Vector3(cx - halfCell, wallHeight * 0.5f, cz),
            _ => Vector3.zero
        };
    }

    // ==================== 起点 / 终点房间 ====================

    private void PlaceStartRoom()
    {
        if (startRoomPrefab == null) return;

        Vector3 startWorld = GetCellWorldPos(startCell.x, startCell.y);
        Vector3 roomPos = startWorld + new Vector3(-cellSize - roomPlacementOffset, 0f, 0f);

        var go = Instantiate(startRoomPrefab, roomPos, Quaternion.identity, transform);
        go.name = "StartRoom";
        Debug.Log($"起点房间已放置在 {startCell} 左侧");
    }

    private void PlaceEndRoom()
    {
        if (endRoomPrefab == null) return;

        Vector3 endWorld = GetCellWorldPos(endCell.x, endCell.y);
        Vector3 roomPos = endWorld + new Vector3(cellSize + roomPlacementOffset, 0f, 0f);

        var go = Instantiate(endRoomPrefab, roomPos, Quaternion.identity, transform);
        go.name = "EndRoom";
        Debug.Log($"终点房间已放置在 {endCell} 右侧");
    }

    private void PlaceMarkers()
    {
        if (startMarkerPrefab != null)
        {
            Vector3 pos = GetCellWorldPos(startCell.x, startCell.y);
            pos.y = markerYOffset;
            var m = Instantiate(startMarkerPrefab, pos, Quaternion.identity, transform);
            m.name = "StartMarker";
        }

        if (endMarkerPrefab != null)
        {
            Vector3 pos = GetCellWorldPos(endCell.x, endCell.y);
            pos.y = markerYOffset;
            var m = Instantiate(endMarkerPrefab, pos, Quaternion.identity, transform);
            m.name = "EndMarker";
        }
    }

    // ==================== 装饰物生成 ====================

    private void PlaceDecorations(List<DecorationEntry> entries, string categoryName)
    {
        if (entries == null || entries.Count == 0) return;

        float halfCell = cellSize * 0.5f;
        float halfThick = wallThickness * 0.5f;
        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!cells[x, y].visited) continue;
                if ((x == startCell.x && y == startCell.y) || (x == endCell.x && y == endCell.y)) continue;

                var c = cells[x, y];
                float baseX = (x + 0.5f) * cellSize;
                float baseZ = (y + 0.5f) * cellSize;

                // 计算格子内可步行区域（避开墙壁）
                float xMin = -halfCell + (c.left   ? wallThickness : halfThick);
                float xMax =  halfCell - (c.right  ? wallThickness : halfThick);
                float zMin = -halfCell + (c.bottom ? wallThickness : halfThick);
                float zMax =  halfCell - (c.top    ? wallThickness : halfThick);

                if (xMax <= xMin || zMax <= zMin) continue;

                foreach (var entry in entries)
                {
                    if (entry.prefab == null) continue;
                    if (Random.value > entry.spawnChance) continue;

                    float f = Mathf.Clamp01(entry.randomOffset);
                    float offX = Random.Range(xMin * f, xMax * f);
                    float offZ = Random.Range(zMin * f, zMax * f);
                    Vector3 pos = new Vector3(baseX + offX, 0f, baseZ + offZ);

                    Quaternion rot = entry.randomYRotation
                        ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                        : Quaternion.identity;

                    var go = Instantiate(entry.prefab, pos, rot, transform);
                    if (entry.uniformScale > 0f)
                        go.transform.localScale = Vector3.one * (entry.uniformScale / prefabUnitSize);

                    count++;
                }
            }
        }

        if (count > 0)
            Debug.Log($"已生成{categoryName}装饰: {count} 个");
    }

    // ==================== 灯光生成 ====================

    private void PlaceCeilingLights()
    {
        if (ceilingLightDecorations == null || ceilingLightDecorations.Count == 0) return;

        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!cells[x, y].visited) continue;
                if ((x == startCell.x && y == startCell.y) || (x == endCell.x && y == endCell.y)) continue;

                foreach (var entry in ceilingLightDecorations)
                {
                    if (entry.prefab == null) continue;
                    if (Random.value > entry.spawnChance) continue;

                    Vector3 basePos = GetCellWorldPos(x, y);
                    Vector3 offset = new(
                        Random.Range(-1f, 1f) * cellSize * entry.randomOffset,
                        0f,
                        Random.Range(-1f, 1f) * cellSize * entry.randomOffset
                    );

                    Vector3 pos = new Vector3(basePos.x + offset.x, wallHeight * 0.98f, basePos.z + offset.z);
                    Quaternion rot = Quaternion.Euler(
                        180f,
                        entry.randomYRotation ? Random.Range(0f, 360f) : 0f,
                        0f
                    );

                    var go = Instantiate(entry.prefab, pos, rot, transform);
                    if (entry.uniformScale > 0f)
                        go.transform.localScale = Vector3.one * (entry.uniformScale / prefabUnitSize);

                    count++;
                }
            }
        }

        if (count > 0) Debug.Log($"已生成天花板灯光: {count} 个");
    }

    private void PlaceWallLights()
    {
        if (wallLightDecorations == null || wallLightDecorations.Count == 0) return;

        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!cells[x, y].visited) continue;
                if ((x == startCell.x && y == startCell.y) || (x == endCell.x && y == endCell.y)) continue;

                foreach (var entry in wallLightDecorations)
                {
                    if (entry.prefab == null) continue;
                    if (Random.value > entry.spawnChance) continue;

                    int wallDir = Random.Range(0, 4);
                    Vector3 wallPos = GetPassageWorldPos(x, y, wallDir);
                    float along = Random.Range(-cellSize * 0.4f, cellSize * 0.4f);

                    Vector3 pos = wallDir == 0 || wallDir == 2
                        ? new Vector3(wallPos.x + along, wallHeight * 0.5f, wallPos.z)
                        : new Vector3(wallPos.x, wallHeight * 0.5f, wallPos.z + along);

                    float yAngle = wallDir switch { 0 => 180f, 1 => 270f, 2 => 0f, 3 => 90f, _ => 0f };
                    Quaternion rot = Quaternion.Euler(0f, yAngle, 0f);

                    var go = Instantiate(entry.prefab, pos, rot, transform);
                    if (entry.uniformScale > 0f)
                        go.transform.localScale = Vector3.one * (entry.uniformScale / prefabUnitSize);

                    count++;
                }
            }
        }

        if (count > 0) Debug.Log($"已生成墙壁灯光: {count} 个");
    }

    // ==================== 寻路 & 移动接口 ====================

    /// <summary>
    /// 检查两个相邻格子之间是否可以通行（无墙阻隔）。
    /// 供玩家移动脚本调用。
    /// </summary>
    public bool CanWalk(int fromX, int fromY, int toX, int toY)
    {
        int dx = toX - fromX;
        int dy = toY - fromY;

        if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1) return false;
        if (fromX < 0 || fromX >= width || fromY < 0 || fromY >= height) return false;
        if (toX < 0 || toX >= width || toY < 0 || toY >= height) return false;

        if (dx ==  1) return !cells[fromX, fromY].right  && !cells[toX, toY].left;
        if (dx == -1) return !cells[fromX, fromY].left   && !cells[toX, toY].right;
        if (dy ==  1) return !cells[fromX, fromY].top    && !cells[toX, toY].bottom;
        if (dy == -1) return !cells[fromX, fromY].bottom && !cells[toX, toY].top;

        return false;
    }

    /// <summary>
    /// 获取某个格子四个方向的可通行状态。
    /// 返回 bool[4]: {top, right, bottom, left} 为 true 表示可通行（无墙）。
    /// </summary>
    public bool[] GetPassableDirections(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return new bool[4];

        var c = cells[x, y];
        return new bool[4]
        {
            !c.top && y + 1 < height,
            !c.right && x + 1 < width,
            !c.bottom && y - 1 >= 0,
            !c.left && x - 1 >= 0
        };
    }

    /// <summary>
    /// 获取从 startCell 到 endCell 的 BFS 最短路径。
    /// 返回格子坐标列表，不可达时返回 null。
    /// </summary>
    public List<Vector2Int> FindPath()
    {
        var queue = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == endCell)
            {
                var path = new List<Vector2Int>();
                var node = current;
                while (node != startCell)
                {
                    path.Add(node);
                    node = cameFrom[node];
                }
                path.Add(startCell);
                path.Reverse();
                return path;
            }

            foreach (var d in dirs)
            {
                int nx = current.x + d.x, ny = current.y + d.y;
                var next = new Vector2Int(nx, ny);

                if (!CanWalk(current.x, current.y, nx, ny)) continue;
                if (visited.Contains(next)) continue;

                visited.Add(next);
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        return null;
    }

    // ==================== 调试寻路 ====================

    /// <summary>
    /// 调试用: 生成3D小球从起点自动寻路到终点，验证迷宫可通行。
    /// </summary>
    [ContextMenu("调试寻路")]
    public IEnumerator DebugNavigate()
    {
        if (cells == null)
        {
            Debug.LogError("请先生成迷宫再调试寻路。");
            yield break;
        }

        var path = FindPath();
        if (path == null || path.Count == 0)
        {
            Debug.LogError($"迷宫不可达！起点 {startCell} 无法走到终点 {endCell}。");
            yield break;
        }

        Debug.Log($"调试寻路: 共 {path.Count} 步, 从 {path[0]} 到 {path[path.Count - 1]}");

        Vector3 startPos = GetCellWorldPos(path[0].x, path[0].y);
        startPos.y = debugBallYOffset;
        var ball = Instantiate(debugBallPrefab, startPos, Quaternion.identity);
        ball.name = "DebugPathBall";

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 target = GetCellWorldPos(path[i].x, path[i].y);
            target.y = debugBallYOffset;

            while (Vector3.Distance(ball.transform.position, target) > 0.05f)
            {
                ball.transform.position = Vector3.MoveTowards(
                    ball.transform.position, target, debugBallSpeed * Time.deltaTime);
                yield return null;
            }
        }

        Debug.Log("调试小球已到达终点！迷宫可通行。");

        if (Application.isPlaying)
            Destroy(ball, 0.5f);
        else
            DestroyImmediate(ball);
    }

    // ==================== 工具 ====================

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
        _meshSizeCache.Clear();
    }

    private List<T> Shuffle<T>(T[] array)
    {
        var list = new List<T>(array);
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
        return list;
    }

    private int DirToIndex(Vector2Int d)
    {
        if (d.y ==  1) return 0;
        if (d.x ==  1) return 1;
        if (d.y == -1) return 2;
        return 3;
    }

    private GameObject PickRandom(GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }

    private bool HasAny(GameObject[] arr)
    {
        return arr != null && arr.Length > 0;
    }

    /// <summary> 获取格子的世界坐标中心 </summary>
    public Vector3 GetCellWorldPos(int x, int y)
    {
        return new((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
    }

    /// <summary> 将世界坐标反查为迷宫格子坐标（自动钳制到有效范围） </summary>
    public Vector2Int GetCellFromWorldPos(Vector3 worldPos)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.x / cellSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.z / cellSize), 0, height - 1);
        return new Vector2Int(x, y);
    }

    /// <summary> 查询某个格子是否已被访问（走廊） </summary>
    public bool IsCellVisited(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height && cells[x, y] != null && cells[x, y].visited;
    }

    /// <summary> 迷宫宽度（只读） </summary>
    public int Width => width;

    /// <summary> 迷宫高度（只读） </summary>
    public int Height => height;

    /// <summary> 格子大小（只读） </summary>
    public float CellSize => cellSize;
}

// ==================== 装饰物配置 ====================

/// <summary>
/// 装饰物条目 —— 房间类/物体类/灯光通用。
/// Inspector 中可自由增删，每项指定预制体、生成概率、随机偏移。
/// </summary>
[System.Serializable]
public class DecorationEntry
{
    [Tooltip("装饰物预制体")]
    public GameObject prefab;

    [Range(0f, 1f), Tooltip("每个格子生成概率")]
    public float spawnChance = 0.1f;

    [Range(0f, 0.5f), Tooltip("在格子内的随机偏移幅度")]
    public float randomOffset = 0.3f;

    [Tooltip("统一缩放倍数（相对于 prefabUnitSize）。0 表示使用预制体原始大小")]
    public float uniformScale = 1f;

    [Tooltip("是否随机 Y 轴旋转（灯光类会自动修正朝向后再随机）")]
    public bool randomYRotation = true;
}
