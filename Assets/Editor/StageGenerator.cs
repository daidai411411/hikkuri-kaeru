using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// ステージ設計シートから書き出した CSV（Assets/StageData/{シーン名}.csv）を読み、
/// 開いているステージシーンの中身を作り直すエディタ拡張。
/// 前提：シーンに Stage1 と同じ構成（Stage / Retry / Border の各 Tilemap）があること。
/// </summary>
public static class StageGenerator
{
    private const string StageDataFolder = "Assets/StageData";
    private const string UndoName = "ステージ生成";

    [MenuItem("Tools/ステージ生成")]
    private static void GenerateForActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        string csvPath = $"{StageDataFolder}/{scene.name}.csv";
        if (!File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog(UndoName, $"{csvPath} が見つかりません。\nシーン名と同じ名前の CSV を置いてください。", "OK");
            return;
        }

        StageTilemaps tilemaps = StageTilemaps.Find(scene);
        if (tilemaps == null)
        {
            EditorUtility.DisplayDialog(UndoName, "Stage Tilemap / Retry Tilemap / Border Tilemap が見つかりません。\nStage1 を複製したシーンで実行してください。", "OK");
            return;
        }

        Transform gimmicksRoot = FindByName(scene, "Gimmicks");
        if (gimmicksRoot == null)
        {
            EditorUtility.DisplayDialog(UndoName, "Gimmicks オブジェクトが見つかりません。\nStage1 を複製したシーンで実行してください。", "OK");
            return;
        }

        StageLayout layout = StageLayout.Load(csvPath);
        StageTilePainter.Paint(layout, tilemaps, UndoName);
        StageGimmickPlacer.Result gimmicks = StageGimmickPlacer.Place(layout, gimmicksRoot, tilemaps.Stage.transform, UndoName);
        StageMarkerPlacer.Place(layout, scene, tilemaps.Stage.transform, UndoName);
        WarnAboutUnknownSymbols(layout);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[{UndoName}] {csvPath} からタイルを塗りました（{layout.Cells.Count()} マス）。"
            + $"ギミック：新規 {gimmicks.Created}、そのまま {gimmicks.Kept}、削除 {gimmicks.Removed}");
    }

    private static void WarnAboutUnknownSymbols(StageLayout layout)
    {
        HashSet<char> known = new HashSet<char>(StageTilePainter.Symbols.Concat(StageGimmickPlacer.Symbols).Concat(StageMarkerPlacer.Symbols));
        string[] unknown = layout.Cells
            .Where(pair => !known.Contains(pair.Value))
            .Select(pair => $"'{pair.Value}' ({pair.Key.x},{pair.Key.y})")
            .ToArray();
        if (unknown.Length > 0)
        {
            Debug.LogWarning($"[{UndoName}] 知らない記号を飛ばしました：{string.Join("、", unknown)}");
        }
    }

    /// <summary>
    /// シーンの中から名前で1つ探す（非アクティブも含む）。見つからなければ null。
    /// </summary>
    public static Transform FindByName(Scene scene, string name)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .FirstOrDefault(t => t.name == name);
    }
}

/// <summary>
/// シーンに1つずつある目印（F カエル、G ゴール、? 看板）を、記号の位置へ動かす。
/// どれも新しく作らず、Stage1 から複製したものを動かす。
/// </summary>
public static class StageMarkerPlacer
{
    private const char Frog = 'F';
    private const char Goal = 'G';
    private const char Sign = '?';
    private const float LandingGap = 0.01f;

    public static IEnumerable<char> Symbols => new[] { Frog, Goal, Sign };

    /// <param name="layout">ステージの地図。</param>
    /// <param name="scene">対象のシーン。Player・Goal・HELPS（看板の親）を名前で探す。</param>
    /// <param name="stageSpace">セル座標の基準になる Transform（Stage Tilemap）。</param>
    /// <param name="undoName">Undo に表示する名前。</param>
    public static void Place(StageLayout layout, Scene scene, Transform stageSpace, string undoName)
    {
        PlaceFrog(layout, StageGenerator.FindByName(scene, "Player"), stageSpace, undoName);
        PlaceGoal(layout, StageGenerator.FindByName(scene, "Goal"), stageSpace, undoName);
        PlaceSigns(layout, StageGenerator.FindByName(scene, "HELPS"), stageSpace, undoName);
    }

    // 当たり判定の底をセルの下辺（床の表面）にそろえ、開始時に落ちたりめり込んだりしないようにする
    private static void PlaceFrog(StageLayout layout, Transform player, Transform stageSpace, string undoName)
    {
        Vector2Int[] cells = CellsOf(layout, Frog);
        if (!IsPlaceable(player, "Player", cells, Frog))
        {
            return;
        }

        BoxCollider2D body = player.GetComponent<BoxCollider2D>();
        float bottomFromPivot = (body.offset.y - body.size.y / 2f) * player.lossyScale.y;
        Vector2 local = new Vector2(cells[0].x + 0.5f, cells[0].y - bottomFromPivot + LandingGap);

        Undo.RecordObject(player, undoName);
        player.position = stageSpace.TransformPoint(local);
    }

    // ゴールは 1×1 のスプライトと当たり判定を縦に伸ばして使う（Stage1 と同じ作り）
    private static void PlaceGoal(StageLayout layout, Transform goal, Transform stageSpace, string undoName)
    {
        Vector2Int[] cells = CellsOf(layout, Goal);
        if (goal == null)
        {
            Debug.LogWarning("[ステージ生成] Goal が見つからないので、ゴールを置けませんでした");
            return;
        }
        if (cells.Length == 0)
        {
            Debug.LogWarning("[ステージ生成] CSV に G がないので、ゴールを動かしませんでした");
            return;
        }

        int column = cells[0].x;
        int bottom = cells.Where(c => c.x == column).Min(c => c.y);
        int top = bottom;
        while (layout.Get(column, top + 1) == Goal)
        {
            top++;
        }
        if (cells.Any(c => c.x != column || c.y < bottom || c.y > top))
        {
            Debug.LogWarning($"[ステージ生成] G が縦1列につながっていません。x={column} の y={bottom}〜{top} だけをゴールにしました");
        }

        int height = top - bottom + 1;
        Undo.RecordObject(goal, undoName);
        goal.position = stageSpace.TransformPoint(new Vector2(column + 0.5f, bottom + height / 2f));
        goal.localScale = new Vector3(goal.localScale.x, height, goal.localScale.z);
    }

    // 看板は左の ? から順に HELP1, HELP2… を割り当てる。余った看板は消さずに非表示にする（絵を後で使い回せるように）
    private static void PlaceSigns(StageLayout layout, Transform signsRoot, Transform stageSpace, string undoName)
    {
        Vector2Int[] cells = CellsOf(layout, Sign).OrderBy(c => c.x).ThenByDescending(c => c.y).ToArray();
        if (signsRoot == null)
        {
            if (cells.Length > 0)
            {
                Debug.LogWarning("[ステージ生成] HELPS が見つからないので、看板を置けませんでした");
            }
            return;
        }

        Transform[] signs = signsRoot.Cast<Transform>().OrderBy(t => t.name).ToArray();
        for (int i = 0; i < signs.Length; i++)
        {
            Undo.RecordObject(signs[i].gameObject, undoName);
            Undo.RecordObject(signs[i], undoName);
            bool used = i < cells.Length;
            signs[i].gameObject.SetActive(used);
            if (used)
            {
                signs[i].position = stageSpace.TransformPoint(new Vector2(cells[i].x + 0.5f, cells[i].y + 0.5f));
            }
        }

        if (cells.Length > signs.Length)
        {
            Debug.LogWarning($"[ステージ生成] ? が {cells.Length} 個ありますが、看板は {signs.Length} 枚しかありません。足りない分は HELPS の下に看板を足してください");
        }
    }

    private static bool IsPlaceable(Transform target, string targetName, Vector2Int[] cells, char symbol)
    {
        if (target == null)
        {
            Debug.LogWarning($"[ステージ生成] {targetName} が見つからないので、{symbol} を置けませんでした");
            return false;
        }
        if (cells.Length != 1)
        {
            Debug.LogWarning($"[ステージ生成] {symbol} は1つだけ置いてください（今は {cells.Length} 個）。{targetName} は動かしていません");
            return false;
        }
        return true;
    }

    private static Vector2Int[] CellsOf(StageLayout layout, char symbol)
    {
        return layout.Cells.Where(pair => pair.Value == symbol).Select(pair => pair.Key).ToArray();
    }
}

/// <summary>
/// CSV に書かれたステージの地図。セル座標は Unity のワールド座標と同じ（セル x,y はワールドの x〜x+1, y〜y+1）。
/// CSV の形式：1行目が x の見出し、1列目が y の見出し、それ以外のセルが記号1文字（空欄は何もない）。
/// </summary>
public class StageLayout
{
    public const char Empty = '\0';

    private readonly Dictionary<Vector2Int, char> cells = new Dictionary<Vector2Int, char>();

    public IEnumerable<KeyValuePair<Vector2Int, char>> Cells => cells;

    public char Get(Vector2Int cell) => cells.TryGetValue(cell, out char symbol) ? symbol : Empty;

    public char Get(int x, int y) => Get(new Vector2Int(x, y));

    /// <summary>
    /// CSV を読み込む。
    /// </summary>
    /// <param name="path">プロジェクトからの相対パス。</param>
    /// <exception cref="System.FormatException">見出しが整数でないとき。</exception>
    public static StageLayout Load(string path)
    {
        StageLayout layout = new StageLayout();
        string[] lines = File.ReadAllLines(path);
        int[] xs = lines[0].Split(',').Skip(1).Select(int.Parse).ToArray();

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] columns = line.Split(',');
            int y = int.Parse(columns[0]);
            for (int i = 1; i < columns.Length && i - 1 < xs.Length; i++)
            {
                string value = columns[i].Trim();
                if (value.Length > 0)
                {
                    layout.cells[new Vector2Int(xs[i - 1], y)] = value[0];
                }
            }
        }

        return layout;
    }
}

/// <summary>
/// ステージシーンにある3つのタイルマップ。
/// </summary>
public class StageTilemaps
{
    public Tilemap Stage { get; private set; }
    public Tilemap Retry { get; private set; }
    public Tilemap Border { get; private set; }

    public Tilemap[] All => new[] { Stage, Retry, Border };

    /// <summary>
    /// シーンから名前でタイルマップを探す。1つでも欠けていれば null。
    /// </summary>
    public static StageTilemaps Find(Scene scene)
    {
        Tilemap[] found = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Tilemap>(true))
            .ToArray();

        StageTilemaps tilemaps = new StageTilemaps
        {
            Stage = found.FirstOrDefault(t => t.name == "Stage Tilemap"),
            Retry = found.FirstOrDefault(t => t.name == "Retry Tilemap"),
            Border = found.FirstOrDefault(t => t.name == "Border Tilemap"),
        };
        return tilemaps.All.Any(t => t == null) ? null : tilemaps;
    }
}

/// <summary>
/// 地形の記号（# = ~ |）をタイルマップに塗る。塗る前に3つのタイルマップを全部消す。
/// </summary>
public static class StageTilePainter
{
    private const char Trunk = '#';
    private const char Leaf = '=';
    private const char Pond = '~';
    private const char Border = '|';

    public static IEnumerable<char> Symbols => new[] { Trunk, Leaf, Pond, Border };

    private const string TrunkPlainPath = "Assets/Pictures/Tiles/枝3.asset";
    private const string TrunkKnotPath = "Assets/Pictures/Tiles/枝1.asset";
    private const string TrunkVinePath = "Assets/Pictures/Tiles/枝2.asset";
    private const string LeafLeftPath = "Assets/Tilemap/grass_tile/main_15.asset";
    private const string LeafMiddlePath = "Assets/Tilemap/grass_tile/main_5.asset";
    private const string LeafRightPath = "Assets/Tilemap/grass_tile/main_11.asset";
    private const string PondPath = "Assets/Pictures/Tiles/池3.asset";
    private const string BorderPath = "Assets/Tilemap/crosshatch-1000_0.asset";

    // Stage1 の柱と同じ向き（時計回りに90度）
    private static readonly Matrix4x4 VerticalTrunk = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -90f));

    public static void Paint(StageLayout layout, StageTilemaps tilemaps, string undoName)
    {
        Undo.RegisterCompleteObjectUndo(tilemaps.All, undoName);
        foreach (Tilemap tilemap in tilemaps.All)
        {
            tilemap.ClearAllTiles();
        }

        TileBase pond = Load(PondPath);
        TileBase border = Load(BorderPath);

        foreach (KeyValuePair<Vector2Int, char> pair in layout.Cells)
        {
            Vector3Int position = (Vector3Int)pair.Key;
            switch (pair.Value)
            {
                case Trunk:
                    tilemaps.Stage.SetTile(position, TrunkTile(pair.Key));
                    if (IsVertical(layout, pair.Key, Trunk))
                    {
                        tilemaps.Stage.SetTransformMatrix(position, VerticalTrunk);
                    }
                    break;
                case Leaf:
                    tilemaps.Stage.SetTile(position, LeafTile(layout, pair.Key));
                    break;
                case Pond:
                    tilemaps.Retry.SetTile(position, pond);
                    break;
                case Border:
                    tilemaps.Border.SetTile(position, border);
                    break;
            }
        }
    }

    // 縦に続いていて横に続いていないセルだけを縦向きにする（床の端や段の角は横向きのまま）
    private static bool IsVertical(StageLayout layout, Vector2Int cell, char symbol)
    {
        bool horizontal = layout.Get(cell + Vector2Int.left) == symbol || layout.Get(cell + Vector2Int.right) == symbol;
        bool vertical = layout.Get(cell + Vector2Int.up) == symbol || layout.Get(cell + Vector2Int.down) == symbol;
        return vertical && !horizontal;
    }

    // Stage1 の比率（節・つるが1割ずつ）に合わせ、座標から決めて作り直しても同じ見た目にする
    private static TileBase TrunkTile(Vector2Int cell)
    {
        int hash = (cell.x * 73856093) ^ (cell.y * 19349663);
        int bucket = ((hash % 10) + 10) % 10;
        return bucket switch
        {
            0 => Load(TrunkKnotPath),
            1 => Load(TrunkVinePath),
            _ => Load(TrunkPlainPath),
        };
    }

    private static TileBase LeafTile(StageLayout layout, Vector2Int cell)
    {
        bool hasLeft = layout.Get(cell + Vector2Int.left) == Leaf;
        bool hasRight = layout.Get(cell + Vector2Int.right) == Leaf;
        if (hasLeft && !hasRight)
        {
            return Load(LeafRightPath);
        }
        if (!hasLeft && hasRight)
        {
            return Load(LeafLeftPath);
        }
        return Load(LeafMiddlePath);
    }

    private static TileBase Load(string path)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
        if (tile == null)
        {
            throw new FileNotFoundException($"タイルが見つかりません：{path}");
        }
        return tile;
    }
}

/// <summary>
/// ギミックの記号（^ v M w R - P V）からプレハブを置く。
/// 生成したものは Gimmicks/Generated の下に「記号_x_y」という名前で置く。
/// 作り直すときは同じ名前のものを作り直さずに残す（Unity で調整した値や、台座と門のつなぎ込みを消さないため）。
/// 位置・向き・大きさ・レイヤーだけは毎回 CSV に合わせ直す。
/// </summary>
public static class StageGimmickPlacer
{
    /// <summary>配置結果の件数。</summary>
    public struct Result
    {
        public int Created;
        public int Kept;
        public int Removed;
    }

    private struct Placement
    {
        public char Symbol;
        public Vector2Int Origin;
        public Vector2Int Size;
        public bool UpsideDown;
        public int WaveIndex;

        public string Name => $"{Symbol}_{Origin.x}_{Origin.y}";
    }

    private const string GeneratedName = "Generated";
    private const string PrefabFolder = "Assets/Prefabs/Gimmicks";
    private const float WaveStepSeconds = 0.2f;
    private const int NoWave = -1;

    private static readonly Dictionary<char, string> PrefabNames = new Dictionary<char, string>
    {
        { '^', "FixedThorn" },
        { 'v', "FixedThorn" },
        { 'M', "MovingThorn" },
        { 'w', "MovingThorn" },
        { 'R', "Rock" },
        { '-', "Branch" },
        { 'P', "PressurePlate" },
        { 'V', "VineGate" },
    };

    // PlayerController の接地判定が Ground レイヤーしか見ないため、乗れるギミックはシーン上で Ground にする
    private static readonly HashSet<char> Footholds = new HashSet<char> { 'R', '-', 'P', 'V' };

    private static readonly HashSet<char> Solids = new HashSet<char> { '#', '=' };

    public static IEnumerable<char> Symbols => PrefabNames.Keys;

    /// <param name="layout">ステージの地図。</param>
    /// <param name="gimmicksRoot">シーンの Gimmicks。この下に Generated を作る。</param>
    /// <param name="stageSpace">セル座標の基準になる Transform（Stage Tilemap）。</param>
    /// <param name="undoName">Undo に表示する名前。</param>
    public static Result Place(StageLayout layout, Transform gimmicksRoot, Transform stageSpace, string undoName)
    {
        Transform generated = GetOrCreateGenerated(gimmicksRoot, undoName);
        ConfirmRemoveUnmanagedObjects(gimmicksRoot, generated, undoName);

        Result result = new Result();
        Dictionary<string, GameObject> existing = new Dictionary<string, GameObject>();
        foreach (Transform child in generated.Cast<Transform>().ToList())
        {
            if (existing.ContainsKey(child.name))
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                result.Removed++;
                continue;
            }
            existing.Add(child.name, child.gameObject);
        }

        List<Placement> placements = BuildPlacements(layout);
        foreach (Placement placement in placements)
        {
            if (existing.TryGetValue(placement.Name, out GameObject kept))
            {
                existing.Remove(placement.Name);
                result.Kept++;
                Apply(kept, placement, stageSpace, undoName);
                continue;
            }

            GameObject created = Instantiate(placement.Symbol, generated, undoName);
            created.name = placement.Name;
            if (placement.WaveIndex != NoWave)
            {
                SetWaveOffset(created, placement.WaveIndex);
            }
            Apply(created, placement, stageSpace, undoName);
            result.Created++;
        }

        foreach (GameObject stale in existing.Values)
        {
            Undo.DestroyObjectImmediate(stale);
            result.Removed++;
        }

        return result;
    }

    private static List<Placement> BuildPlacements(StageLayout layout)
    {
        List<Placement> placements = new List<Placement>();
        HashSet<Vector2Int> grouped = new HashSet<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, char> pair in layout.Cells.OrderBy(p => p.Key.y).ThenBy(p => p.Key.x))
        {
            Vector2Int cell = pair.Key;
            char symbol = pair.Value;
            if (!PrefabNames.ContainsKey(symbol) || grouped.Contains(cell))
            {
                continue;
            }

            switch (symbol)
            {
                case '-':
                    List<Vector2Int> run = HorizontalRun(layout, cell);
                    grouped.UnionWith(run);
                    placements.Add(new Placement { Symbol = symbol, Origin = run[0], Size = new Vector2Int(run.Count, 1), WaveIndex = NoWave });
                    break;
                case 'V':
                    List<Vector2Int> region = ConnectedRegion(layout, cell);
                    grouped.UnionWith(region);
                    Vector2Int min = new Vector2Int(region.Min(c => c.x), region.Min(c => c.y));
                    Vector2Int max = new Vector2Int(region.Max(c => c.x), region.Max(c => c.y));
                    placements.Add(new Placement { Symbol = symbol, Origin = min, Size = max - min + Vector2Int.one, WaveIndex = NoWave });
                    break;
                case 'M':
                case 'w':
                    List<Vector2Int> wave = HorizontalRun(layout, cell);
                    placements.Add(new Placement { Symbol = symbol, Origin = cell, Size = Vector2Int.one, WaveIndex = wave.Count >= 2 ? wave.IndexOf(cell) : NoWave });
                    break;
                case 'P':
                    bool onCeiling = Solids.Contains(layout.Get(cell + Vector2Int.up)) && !Solids.Contains(layout.Get(cell + Vector2Int.down));
                    placements.Add(new Placement { Symbol = symbol, Origin = cell, Size = Vector2Int.one, UpsideDown = onCeiling, WaveIndex = NoWave });
                    break;
                default:
                    placements.Add(new Placement { Symbol = symbol, Origin = cell, Size = Vector2Int.one, WaveIndex = NoWave });
                    break;
            }
        }

        return placements;
    }

    private static void Apply(GameObject target, Placement placement, Transform stageSpace, string undoName)
    {
        Undo.RecordObject(target.transform, undoName);
        Vector2 origin = placement.Origin;
        Vector2 size = placement.Size;
        float angle = placement.UpsideDown ? 180f : 0f;
        Vector2 local;

        switch (placement.Symbol)
        {
            case 'v':
                local = origin + new Vector2(0.5f, 0.5f);
                angle = 180f;
                break;
            case 'M':
                // MovingThorn は根元（床の表面）が原点
                local = origin + new Vector2(0.5f, 0f);
                break;
            case 'w':
                local = origin + new Vector2(0.5f, 1f);
                angle = 180f;
                break;
            case '-':
                ResizeTiled(target, new Vector2(size.x, target.GetComponent<SpriteRenderer>().size.y), undoName);
                // 枝の上面をセルの上辺にそろえ、床（#）の上に立つのと同じ高さで乗れるようにする
                local = origin + new Vector2(size.x / 2f, 1f - target.GetComponent<SpriteRenderer>().size.y / 2f);
                break;
            case 'V':
                ResizeTiled(target, size, undoName);
                local = origin + size / 2f;
                break;
            default:
                local = origin + new Vector2(0.5f, 0.5f);
                break;
        }

        target.transform.position = stageSpace.TransformPoint(local);
        target.transform.rotation = stageSpace.rotation * Quaternion.Euler(0f, 0f, angle);

        if (Footholds.Contains(placement.Symbol))
        {
            SetLayerRecursively(target, LayerMask.NameToLayer("Ground"), undoName);
        }
    }

    // Tiled 描画のスプライトと当たり判定を、セルの大きさに合わせて伸ばす
    private static void ResizeTiled(GameObject target, Vector2 size, string undoName)
    {
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
        Undo.RecordObjects(new Object[] { spriteRenderer, boxCollider }, undoName);
        spriteRenderer.size = size;
        boxCollider.size = size;
    }

    // 横並びの左端から 0.2 秒ずつ遅れて引っ込むようにし、安全な隙間が右へ流れる波にする
    private static void SetWaveOffset(GameObject target, int waveIndex)
    {
        SerializedObject serialized = new SerializedObject(target.GetComponent<MovingThorn>());
        float cycle = new[] { "hiddenDuration", "warningDuration", "riseDuration", "outDuration", "sinkDuration" }
            .Sum(name => serialized.FindProperty(name).floatValue);
        serialized.FindProperty("startOffset").floatValue = Mathf.Repeat(-waveIndex * WaveStepSeconds, cycle);
        serialized.ApplyModifiedProperties();
    }

    private static List<Vector2Int> HorizontalRun(StageLayout layout, Vector2Int cell)
    {
        char symbol = layout.Get(cell);
        Vector2Int start = cell;
        while (layout.Get(start + Vector2Int.left) == symbol)
        {
            start += Vector2Int.left;
        }

        List<Vector2Int> run = new List<Vector2Int>();
        for (Vector2Int c = start; layout.Get(c) == symbol; c += Vector2Int.right)
        {
            run.Add(c);
        }
        return run;
    }

    private static List<Vector2Int> ConnectedRegion(StageLayout layout, Vector2Int cell)
    {
        char symbol = layout.Get(cell);
        List<Vector2Int> region = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int> { cell };
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(cell);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            region.Add(current);
            foreach (Vector2Int next in directions.Select(d => current + d))
            {
                if (layout.Get(next) == symbol && visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }
        return region;
    }

    private static GameObject Instantiate(char symbol, Transform parent, string undoName)
    {
        string path = $"{PrefabFolder}/{PrefabNames[symbol]}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            throw new FileNotFoundException($"プレハブが見つかりません：{path}");
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        Undo.RegisterCreatedObjectUndo(instance, undoName);
        return instance;
    }

    private static Transform GetOrCreateGenerated(Transform gimmicksRoot, string undoName)
    {
        Transform generated = gimmicksRoot.Find(GeneratedName);
        if (generated != null)
        {
            return generated;
        }

        GameObject created = new GameObject(GeneratedName);
        Undo.RegisterCreatedObjectUndo(created, undoName);
        created.transform.SetParent(gimmicksRoot, false);
        return created.transform;
    }

    // Stage1 を複製したシーンには Stage1 のギミックが残るため、生成のたびに消すかどうかを聞く
    private static void ConfirmRemoveUnmanagedObjects(Transform gimmicksRoot, Transform generated, string undoName)
    {
        GameObject[] unmanaged = gimmicksRoot.Cast<Transform>()
            .Where(child => child != generated)
            .Select(child => child.gameObject)
            .ToArray();
        if (unmanaged.Length == 0)
        {
            return;
        }

        string names = string.Join("、", unmanaged.Select(go => go.name));
        bool remove = EditorUtility.DisplayDialog(
            undoName,
            $"Gimmicks の下に、生成ツールの管理外のものがあります：\n{names}\n\nStage1 から複製したものなら削除してください（Undo で戻せます）。",
            "削除する",
            "残す");
        if (!remove)
        {
            Debug.LogWarning($"[{undoName}] Gimmicks の下の管理外のものを残しました：{names}");
            return;
        }

        foreach (GameObject target in unmanaged)
        {
            Undo.DestroyObjectImmediate(target);
        }
        Debug.Log($"[{undoName}] Gimmicks の下の管理外のものを削除しました：{names}");
    }

    private static void SetLayerRecursively(GameObject target, int layer, string undoName)
    {
        foreach (Transform t in target.GetComponentsInChildren<Transform>(true))
        {
            Undo.RecordObject(t.gameObject, undoName);
            t.gameObject.layer = layer;
        }
    }
}
